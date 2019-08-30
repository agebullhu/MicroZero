#include "../stdafx.h"


#ifdef _ZERO_PLAN

#include "zero_plan.h"
#include "plan_dispatcher.h"
#include "inner_socket.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 单例
		*/
		plan_dispatcher* plan_dispatcher::instance = nullptr;

		/**
		* \brief 执行
		*/
		void plan_dispatcher::launch(shared_ptr<plan_dispatcher>& station)
		{
			station_config& config = station->get_config();
			config.is_base = true;
			if (!station->initialize())
			{
				config.failed("initialize");
				set_station_thread_bad(config.station_name.c_str());
				return;
			}
			if (!station_warehouse::join(station.get()))
			{
				config.failed("join warehouse");
				set_station_thread_bad(config.station_name.c_str());
				return;
			}
			boost::thread(boost::bind(run_plan_poll, station.get()));
			station->task_semaphore_.wait();
			//station->storage_.prepare(station->config_);
			station->poll();
			station->task_semaphore_.wait();
			station_warehouse::left(station.get());
			station->destruct();
			if (!config.is_state(station_state::stop) && get_net_state() == zero_def::net_state::runing)
			{
				config.restart();
				run(station->get_config_ptr());
			}
			else
			{
				config.closed();
			}
			set_station_thread_end(config.station_name.c_str());
		}
		/**
		* \brief 工作开始 : 处理请求数据
		*/
		inline void plan_dispatcher::job_start(zmq_handler socket, vector<shared_char>& list, bool inner, bool old)
		{
			if (!inner)
			{
				//外部管理接口
				trace(1, list, nullptr);
				on_plan_manage(socket, list);
			}
			else
			{
				//内部返回与计划下发
				if (list[1][0] == '*') //返回值
				{
					on_plan_result(list);
				}
				else
				{
					on_plan_start(socket, list);
				}
			}
		}

		/**
		* \brief 计划管理
		*/
		void plan_dispatcher::on_plan_manage(zmq_handler socket, vector<shared_char>& list)
		{
			const char* buf = *list[1];

			const char* cmd = nullptr;
			size_t rqid_index = 0, glid_index = 0, rqer_index = 0;
			vector<shared_char> arg;
			const auto desc_size = list[1].desc_size();
			for (size_t idx = 2; idx <= desc_size; idx++)
			{
				switch (buf[idx])
				{
				case zero_def::frame::command:
					cmd = *list[idx];
					break;
				case zero_def::frame::request_id:
					rqid_index = idx;
					break;
				case zero_def::frame::requester:
					rqer_index = idx;
					break;
				case zero_def::frame::arg:
					arg.emplace_back(list[idx]);
					break;
				case zero_def::frame::global_id:
					glid_index = idx;
					break;
				}
			}
			if (cmd == nullptr)
			{
				send_request_status_by_trace(socket, *list[0], zero_def::status::arg_invalid, list, glid_index, rqid_index, rqer_index, nullptr);
				return;
			}
			string json;
			const char code = exec_command(cmd, arg, json);
			send_request_status_by_trace(socket, *list[0], code, list, glid_index, rqid_index, rqer_index, json.c_str());
		}

		const char* plan_commands_1[] =
		{
			"list","message", "skip", "pause", "close", "remove", "reset"
		};

		enum class plan_commands_2
		{
			list, message, skip, pause, close, remove, reset
		};

		/**
		* \brief 执行命令
		*/
		char plan_dispatcher::exec_command(const char* command, vector<shared_char>& arguments, string& json)
		{
			int idx = strmatchi(command, plan_commands_1);
			switch (static_cast<plan_commands_2>(idx))
			{
			case plan_commands_2::list:
			{
				plan_message::plan_list(json);
				return zero_def::status::ok;
			}
			case plan_commands_2::message:
			{
				if (arguments.size() < 1)
				{
					return zero_def::status::arg_invalid;
				}
				shared_ptr<plan_message> plan = plan_message::load_message(*arguments[0]);
				if (!plan)
				{
					return zero_def::status::arg_invalid;
				}
				json = plan->write_json();
				return zero_def::status::ok;
			}
			case plan_commands_2::skip:
			{
				if (arguments.size() < 2)
				{
					return zero_def::status::arg_invalid;
				}
				shared_ptr<plan_message> plan = plan_message::load_message(*arguments[0]);
				if (!plan || plan->plan_state >= plan_message_state::close)
				{
					return zero_def::status::arg_invalid;
				}
				int skip = atoi(*arguments[1]);
				if (skip < 0)
					skip = -1;
				if (skip != plan->skip_set)
				{
					plan->set_skip(skip);
				}
				return zero_def::status::ok;
			}
			case plan_commands_2::pause:
			{
				if (arguments.size() < 1)
				{
					return zero_def::status::arg_invalid;
				}
				shared_ptr<plan_message> plan = plan_message::load_message(*arguments[0]);
				if (!plan || plan->plan_state >= plan_message_state::pause)
				{
					return zero_def::status::arg_invalid;
				}
				plan->pause();
				return zero_def::status::ok;
			}
			case plan_commands_2::reset:
			{
				if (arguments.size() < 1)
				{
					return zero_def::status::arg_invalid;
				}
				shared_ptr<plan_message> plan = plan_message::load_message(*arguments[0]);
				if (!plan || plan->plan_state < plan_message_state::pause ||
					plan->plan_state > plan_message_state::error)
				{
					return zero_def::status::arg_invalid;
				}
				plan->reset();
				return zero_def::status::ok;
			}
			case plan_commands_2::close:
			{
				if (arguments.size() < 1)
				{
					return zero_def::status::arg_invalid;
				}
				shared_ptr<plan_message> plan = plan_message::load_message(*arguments[0]);
				if (!plan)
				{
					return zero_def::status::arg_invalid;
				}
				plan->close();
				return zero_def::status::ok;
			}
			case plan_commands_2::remove:
			{
				if (arguments.size() < 1)
				{
					return zero_def::status::arg_invalid;
				}
				shared_ptr<plan_message> plan = plan_message::load_message(*arguments[0]);
				if (!plan || plan->plan_state < plan_message_state::close)
				{
					return zero_def::status::arg_invalid;
				}
				plan->remove();
				return zero_def::status::ok;
			}
			default:
				return zero_def::status::not_support;
			}
		}
		/**
		* \brief 计划轮询
		*/
		void plan_dispatcher::plan_poll()
		{
			get_config().log("plan poll start");
			task_semaphore_.post();
			boost::posix_time::ptime pre = boost::posix_time::second_clock::universal_time();
			while (can_do())
			{
				int sec = static_cast<int>((boost::posix_time::second_clock::universal_time() - pre).total_milliseconds());
				if (sec < 1000)//执行时间超过1秒则不等待,继续执行
				{
					THREAD_SLEEP(1000 - sec);
				}
				if (!can_do())
					break;
				pre = boost::posix_time::second_clock::universal_time();
				plan_message::exec_now([this](shared_ptr<plan_message>& msg)
					{
						return exec_plan(msg);
					});
			}
			sockets_.clear();
			get_config().log("plan poll end");
			task_semaphore_.post();
		}

		/**
		* \brief 计划进入
		*/
		bool plan_dispatcher::on_plan_start(zmq_handler socket, vector<shared_char>& list)
		{
			const shared_char caller = list[0];
			list.erase(list.begin());

			shared_char description(16);
			description.state(zero_def::command::proxy);
			shared_char frame_head(128);

			shared_ptr<plan_message> message = make_shared<plan_message>();
			message->caller = list[0];
			message->frames.emplace_back(frame_head);
			message->frames.emplace_back(description);

			size_t plan = 0, glid = 0, reqer = 0, pid = 0;

			const size_t rqid = 0, size = list[1].frame_size();
			for (size_t idx = 2; idx <= size && idx < list.size(); idx++)
			{
				switch (list[1][idx])
				{
				case zero_def::frame::plan:
					plan = idx;
					continue;
				case zero_def::frame::pub_title:
					frame_head = list[idx];
					pid = idx;
					continue;
				case zero_def::frame::station_id:
					message->station = list[idx];
					break;
				case zero_def::frame::request_id:
					message->request_id = list[idx];
					break;
				case zero_def::frame::requester:
					reqer = idx;
					break;
				case zero_def::frame::global_id:
					glid = idx;
					break;
				case zero_def::frame::command:
					message->command = list[idx];
					break;
				}
				description.append_frame(list[1][idx]);
				message->frames.emplace_back(list[idx]);
			}
			if (plan == 0 || message->station.empty() || message->command.empty())
			{
				send_request_status_by_trace(socket, *caller, zero_def::status::arg_invalid, list, glid, rqid, reqer, nullptr);
				return false;
			}
			const auto config = station_warehouse::get_config(message->station.c_str());
			if (!config)
			{
				send_request_status_by_trace(socket, *caller, zero_def::status::arg_invalid, list, glid, rqid, reqer, nullptr);
				return false;
			}
			message->read_plan(*list[plan]);
			if (!message->validate())
			{
				send_request_status_by_trace(socket, *caller, zero_def::status::arg_invalid, list, glid, rqid, reqer, nullptr);
				return false;
			}

			send_request_status_by_trace(socket, *caller, zero_def::status::jion_plan, list, glid, rqid, reqer, nullptr);

			message->station_type = config->station_type;

			description.append_frame(zero_def::frame::plan);
			message->frames.emplace_back("");

			shared_char global_id;
			if (glid == 0)
			{
				global_id.set_int64(station_warehouse::get_glogal_id());
				description.append_frame(zero_def::frame::global_id);
				message->frames.emplace_back(global_id);
			}
			else
			{
				global_id = list[glid];
			}
			message->plan_id = atoll(*global_id);
			message->frames[1].sync(description);

			if (pid == 0)
			{
				sprintf(frame_head.c_str(), "*:msg:%s:%llx", *message->station, message->plan_id); //计划特殊的请求者(虚拟)
			}


			//message->save_message(true,false,true,false,false,false);
			message->next();
			//storage_.save_plan(*message);
			return true;
		}

		/**
		* \brief 执行计划
		*/
		void plan_dispatcher::exec_plan(shared_ptr<plan_message>& message)
		{
			if (!can_do())
				return;
			message->plan_state = plan_message_state::execute;
			message->exec_time = time(nullptr);
			//const auto ptr = sockets_.find(*message->station);
			//shared_ptr<inner_socket> socket;
			//if (ptr == sockets_.end())
			//{
			//	socket = make_shared<inner_socket>(get_station_name(), *message->station);
			//	sockets_.insert(make_pair(*message->station, socket));
			//}
			//else
			//{
			//	socket = ptr->second;
			//}
			//char key[256];
			//sprintf(key, "%lld-%ld", message->plan_id, message->exec_time);
			inner_socket socket(get_station_name(), *message->station);
			message->frames[1].state(zero_def::command::proxy);
			message->frames[message->frames.size() - 2] = message->write_info();
			var state = socket.send(message->frames);
			message->frames[message->frames.size() - 2].free();//防止无义的保存

			vector<shared_char> result;
			if (state != zmq_socket_state::succeed)
			{
				const auto config = station_warehouse::get_config(message->station.c_str(), false);
				shared_char frame;
				frame.alloc_desc(6, config ? zero_def::status::send_error : zero_def::status::not_find);
				result.emplace_back(frame);
				on_plan_result(message, config ? zero_def::status::send_error : zero_def::status::not_find, result);
				return;
			}
			state = socket.recv(result);
			if (state != zmq_socket_state::succeed)
			{
				shared_char frame;
				frame.alloc_desc(6, zero_def::status::recv_error);
				result.emplace_back(frame);
				on_plan_result(message, zero_def::status::recv_error, result);
				return;
			}
			if (result[0][1] != zero_def::status::runing)
			{
				on_plan_result(message, result[0].state(), result);
				return;
			}
			message->exec_state = result[0].state();

			//storage_.save_log(*message);
			message->save_message(false, true, false, false, false, false);
		}

		/**
		* \brief 计划执行返回
		*/
		void plan_dispatcher::on_plan_result(vector<shared_char>& list)
		{
			list.erase(list.begin());
			shared_ptr<plan_message> message = plan_message::load_message(list[0].c_str() + 2);

			if (message == nullptr)
			{
				get_config().error("message is remove", list[0].c_str());
				return;
			}
			on_plan_result(message, list[1][1], list);
		}

		/**
		* \brief 计划执行返回
		*/
		void plan_dispatcher::on_plan_result(shared_ptr<plan_message>& message, char st, vector<shared_char>& list)
		{
			uchar state = *reinterpret_cast<uchar*>(&st);
			redis_live_scope scope(global_config::redis_defdb);
			if (state == zero_def::command::waiting)
			{
				message->exec_state = zero_def::status::wait;
			}
			else
			{
				message->exec_state = state;
				if (state == zero_def::status::not_find || state == zero_def::status::not_support)//无法执行
				{
					message->pause();
				}
				else if (state < zero_def::status::bug)//执行成功或逻辑失败
				{
					message->real_repet += 1;
					message->next();
				}
				else if (state < zero_def::status::error)//有BUG无法执行
				{
					message->real_repet += 1;
					message->pause();
				}
				else if (message->skip_set == 0)
				{
					message->skip_set = -2;
					message->skip_num = 1;
					message->plan_state = plan_message_state::retry;
				}
				else if (message->skip_set == -2)
				{
					if (message->skip_num >= 3)
					{
						message->pause();
					}
				}
				else
				{
					message->pause();
				}
			}
			result_event(message, list);
			//storage_.save_log(*message, list);
			message->save_message_result(*message->station, list);
		}

		/**
		*\brief 通知内容
		*/
		bool plan_dispatcher::zero_event(zero_net_event event_type, const plan_message* message)
		{
			shared_char description;
			if (message != nullptr)
				description.alloc_frame_desc(static_cast<char>(event_type), zero_def::frame::sub_title, zero_def::frame::context);
			else
				description.alloc_frame_desc(static_cast<char>(event_type), zero_def::frame::sub_title);
			vector<shared_char> datas;
			datas.emplace_back(*message->description);
			datas.emplace_back(description);
			datas.emplace_back(shared_char().set_int64(message->plan_id));
			if (message)
			{
				datas.emplace_back(event_type == zero_net_event::event_plan_add ? message->write_json() : message->write_state());
			}
			return send_response(datas, true) == zmq_socket_state::succeed;
		}
		/**
		*\brief 发布消息
		*/
		bool plan_dispatcher::result_event(shared_ptr<plan_message>& message, vector<shared_char>& result)
		{
			shared_char description;
			description.alloc_frame_desc(static_cast<char>(zero_net_event::event_plan_result)
				, zero_def::frame::sub_title, zero_def::frame::context, zero_def::frame::status);
			vector<shared_char> datas;
			datas.emplace_back(*message->description);
			datas.emplace_back(description);
			datas.emplace_back(shared_char().set_int64(message->plan_id));
			datas.emplace_back(message->write_state());
			datas.emplace_back(result[0]);
			for (size_t idx = 2; idx < result.size() && idx < result[0].size(); idx++)
			{
				switch (result[0][idx])
				{
				case zero_def::frame::general_end:
				case zero_def::frame::extend_end:
					break;
				}
				description.append_frame(result[0][idx]);
				datas.emplace_back(result[idx - 1]);
			}
			datas[1].sync(description);
			return send_response(datas, true) == zmq_socket_state::succeed;
		}
	}
}

#endif // PLAN