#include "../stdafx.h"
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
			zero_config& config = station->get_config();
			if (!station->initialize())
			{
				config.failed("initialize");
				set_command_thread_bad(config.station_name_.c_str());
				return;
			}
			if (!station_warehouse::join(station.get()))
			{
				config.failed("join warehouse");
				set_command_thread_bad(config.station_name_.c_str());
				return;
			}
			boost::thread(boost::bind(run_plan_poll, station.get()));
			station->task_semaphore_.wait();
			station->poll();
			station->task_semaphore_.wait();
			station_warehouse::left(station.get());
			station->destruct();
			if (!config.is_state(station_state::stop) && get_net_state() == net_state_runing)
			{
				config.restart();
				run(station->get_config_ptr());
			}
			else
			{
				config.closed();
			}
			set_command_thread_end(config.station_name_.c_str());
		}
		/**
		* \brief 工作开始（发送到工作者）
		*/
		inline void plan_dispatcher::job_start(zmq_handler socket, vector<shared_char>& list, bool inner)
		{
			if (!inner)
			{
				//外部管理接口
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
				case ZERO_FRAME_COMMAND:
					cmd = *list[idx];
					break;
				case ZERO_FRAME_REQUEST_ID:
					rqid_index = idx;
					break;
				case ZERO_FRAME_REQUESTER:
					rqer_index = idx;
					break;
				case ZERO_FRAME_ARG:
					arg.emplace_back(list[idx]);
					break;
				case ZERO_FRAME_GLOBAL_ID:
					glid_index = idx;
					break;
				}
			}
			if (cmd == nullptr)
			{
				send_request_status(socket, ZERO_STATUS_ARG_INVALID_ID, list, glid_index, rqid_index, rqer_index);
				return;
			}
			string json;
			const char code = exec_command(cmd, arg, json);
			send_request_status(socket, code, list, glid_index, rqid_index, rqer_index, json.c_str());

		}

		/**
		* \brief 计划列表
		*/
		void plan_dispatcher::plan_list(string& json)
		{
			redis_live_scope redis(global_config::redis_defdb);
			json = "[";
			bool first = true;
			int cursor = 0;
			do
			{
				vector<acl::string> keys;
				cursor = redis->scan(cursor, keys, "msg:*");
				for (acl::string& key : keys)
				{
					if (first)
						first = false;
					else
						json.append(",");
					shared_ptr<plan_message> message = plan_message::load_message(key.c_str());

					json.append(message->write_json());
				}

			} while (cursor > 0);
			json.append("]");
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
				plan_list(json);
				return ZERO_STATUS_OK_ID;
			}
			case plan_commands_2::message:
			{
				if (arguments.size() < 1)
				{
					return ZERO_STATUS_ARG_INVALID_ID;
				}
				shared_ptr<plan_message> plan = plan_message::load_message(*arguments[0]);
				if (!plan)
				{
					return ZERO_STATUS_ARG_INVALID_ID;
				}
				json = plan->write_json();
				return ZERO_STATUS_OK_ID;
			}
			case plan_commands_2::skip:
			{
				if (arguments.size() < 2)
				{
					return ZERO_STATUS_ARG_INVALID_ID;
				}
				shared_ptr<plan_message> plan = plan_message::load_message(*arguments[0]);
				if (!plan || plan->plan_state >= plan_message_state::close)
				{
					return ZERO_STATUS_ARG_INVALID_ID;
				}
				int skip = atoi(*arguments[1]);
				if (skip < 0)
					skip = -1;
				if (skip != plan->skip_set)
				{
					plan->set_skip(skip);
				}
				return ZERO_STATUS_OK_ID;
			}
			case plan_commands_2::pause:
			{
				if (arguments.size() < 1)
				{
					return ZERO_STATUS_ARG_INVALID_ID;
				}
				shared_ptr<plan_message> plan = plan_message::load_message(*arguments[0]);
				if (!plan || plan->plan_state >= plan_message_state::pause)
				{
					return ZERO_STATUS_ARG_INVALID_ID;
				}
				plan->pause();
				return ZERO_STATUS_OK_ID;
			}
			case plan_commands_2::reset:
			{
				if (arguments.size() < 1)
				{
					return ZERO_STATUS_ARG_INVALID_ID;
				}
				shared_ptr<plan_message> plan = plan_message::load_message(*arguments[0]);
				if (!plan || plan->plan_state < plan_message_state::pause || 
					plan->plan_state > plan_message_state::error)
				{
					return ZERO_STATUS_ARG_INVALID_ID;
				}
				plan->reset();
				return ZERO_STATUS_OK_ID;
			}
			case plan_commands_2::close:
			{
				if (arguments.size() < 1)
				{
					return ZERO_STATUS_ARG_INVALID_ID;
				}
				shared_ptr<plan_message> plan = plan_message::load_message(*arguments[0]);
				if (!plan)
				{
					return ZERO_STATUS_ARG_INVALID_ID;
				}
				plan->close();
				return ZERO_STATUS_OK_ID;
			}
			case plan_commands_2::remove:
			{
				if (arguments.size() < 1)
				{
					return ZERO_STATUS_ARG_INVALID_ID;
				}
				shared_ptr<plan_message> plan = plan_message::load_message(*arguments[0]);
				if (!plan || plan->plan_state < plan_message_state::close)
				{
					return ZERO_STATUS_ARG_INVALID_ID;
				}
				plan->remove();
				return ZERO_STATUS_OK_ID;
			}
			default:
				return ZERO_STATUS_NOT_SUPPORT_ID;
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
					thread_sleep(1000 - sec);
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
			shared_char caller = list[0];
			list.erase(list.begin());

			shared_char description(16);
			description.state(ZERO_BYTE_COMMAND_PROXY);
			shared_char plan_caller(128);

			shared_ptr<plan_message> message = make_shared<plan_message>();
			message->caller = list[0];
			message->frames.emplace_back(plan_caller);
			message->frames.emplace_back(description);

			size_t plan = 0, rqid = 0, glid = 0, reqer = 0, cmdid = 0;
			for (size_t idx = 2; idx <= static_cast<size_t>(list[1][0] + 2); idx++)
			{
				switch (list[1][idx])
				{
				case ZERO_FRAME_PLAN:
					plan = idx;
					continue;
				case ZERO_FRAME_STATION_ID:
					message->station = list[idx];
				case ZERO_FRAME_END:
					continue;
				case ZERO_FRAME_REQUEST_ID:
					message->request_id = list[idx];
					break;
				case ZERO_FRAME_REQUESTER:
					reqer = idx;
					break;
				case ZERO_FRAME_GLOBAL_ID:
					glid = idx;
					break;
				case ZERO_FRAME_COMMAND:
					message->command = list[idx];
					cmdid = idx;
					break;
				}
				description.append_frame(list[1][idx]);
				message->frames.emplace_back(list[idx]);
			}
			auto config = station_warehouse::get_config(message->station);
			if (plan == 0 || !config || message->station.empty() || cmdid == 0)
			{
				send_request_status(socket, *caller, ZERO_STATUS_ARG_INVALID_ID, list, glid, rqid, reqer);
				return false;
			}

			description.append_frame(ZERO_FRAME_PLAN);
			message->frames.emplace_back("");

			shared_char global_id;
			if (glid == 0)
			{
				global_id.set_int64x(message->plan_id = station_warehouse::get_glogal_id());
				description.append_frame(ZERO_FRAME_GLOBAL_ID);
				message->frames.emplace_back(global_id);
			}
			else
			{
				global_id = list[glid];
				message->plan_id = atoll(*list[glid]);
			}
			sprintf(plan_caller.get_buffer(), "*:msg:%s:%llx", *message->station, message->plan_id); //计划特殊的请求者(虚拟)

			message->station_type = config->station_type_;
			message->read_plan(*list[plan]);

			if (message->plan_repet == 0 || (message->skip_set > 0 && message->plan_repet > 0 && message->plan_repet <= message->skip_set))
			{
				send_request_status(socket, *caller, ZERO_STATUS_ARG_INVALID_ID, list, glid, rqid, reqer);
				return false;
			}
			if (message->plan_time <= 0)
			{
				message->plan_time =time(nullptr);
			}
			message->next();
			plan_message::add_local(message);
			send_request_status(socket, *caller, ZERO_STATUS_PLAN_ID, list, glid, rqid, reqer);
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
			auto ptr = sockets_.find(*message->station);
			shared_ptr<inner_socket> socket;
			if (ptr == sockets_.end())
			{
				socket = make_shared<inner_socket>(get_station_name(), *message->station);
				sockets_.insert(make_pair(*message->station, socket));
			}
			else
			{
				socket = ptr->second;
			}
			message->frames[1].state(ZERO_BYTE_COMMAND_PROXY);
			message->frames[message->frames.size() - 2] = message->write_info();
			var state = socket->send(message->frames);
			message->frames[message->frames.size() - 2] = "";//防止无义的保存

			vector<shared_char> result;
			if (state != zmq_socket_state::succeed)
			{
				auto config = station_warehouse::get_config(message->station, false);
				shared_char frame;
				frame.alloc_frame(6, config ? ZERO_STATUS_SEND_ERROR_ID : ZERO_STATUS_NOT_FIND_ID);
				result.emplace_back(frame);
				on_plan_result(message, config ? ZERO_STATUS_SEND_ERROR_ID : ZERO_STATUS_NOT_FIND_ID, result);
				return;
			}
			state = socket->recv(result);
			if (state != zmq_socket_state::succeed)
			{
				shared_char frame;
				frame.alloc_frame(6, ZERO_STATUS_RECV_ERROR_ID);
				result.emplace_back(frame);
				on_plan_result(message, ZERO_STATUS_RECV_ERROR_ID, result);
				return;
			}
			if (result[0][1] != ZERO_STATUS_RUNING_ID)
			{
				on_plan_result(message, result[0].state(), result);
				return;
			}
			message->exec_state = result[0].state();
			message->save_message(false, true, false, false, false, false);
		}

		/**
		* \brief 计划执行返回
		*/
		void plan_dispatcher::on_plan_result(vector<shared_char>& list)
		{
			list.erase(list.begin());
			shared_ptr<plan_message> message = plan_message::load_message(list[0].get_buffer() + 2);

			if (message == nullptr)
			{
				get_config().error("message is remove", list[0].get_buffer());
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
			if (state == ZERO_BYTE_COMMAND_WAITING)
			{
				message->exec_state = ZERO_STATUS_WAIT_ID;
			}
			else
			{
				message->exec_state = state;
				if (state == ZERO_STATUS_NOT_FIND_ID || state == ZERO_STATUS_NOT_SUPPORT_ID)//无法执行
				{
					message->pause();
				}
				else if (state < ZERO_STATUS_BUG_ID)//执行成功或逻辑失败
				{
					message->real_repet += 1;
					message->next();
				}
				else if (state < ZERO_STATUS_ERROR_ID)//有BUG无法执行
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
			message->save_message_result(*message->station, list);
		}

		/**
		*\brief 通知内容
		*/
		bool plan_dispatcher::zero_event(zero_net_event event_type, const plan_message* message)
		{
			shared_char description;
			if (message != nullptr)
				description.alloc_frame_2(static_cast<char>(event_type), ZERO_FRAME_SUBTITLE, ZERO_FRAME_CONTEXT);
			else
				description.alloc_frame(static_cast<char>(event_type), ZERO_FRAME_SUBTITLE);
			vector<shared_char> datas;
			datas.emplace_back(*message->description);
			datas.emplace_back(description);
			datas.emplace_back(shared_char().set_int64x(message->plan_id));
			if (message)
			{
				datas.emplace_back(event_type == zero_net_event::event_plan_add ? message->write_json() : message->write_state());
			}
			return send_response(datas);
		}
		/**
		*\brief 发布消息
		*/
		bool plan_dispatcher::result_event(shared_ptr<plan_message>& message, vector<shared_char>& result)
		{
			shared_char description;
			description.alloc_frame_3(static_cast<char>(zero_net_event::event_plan_result)
				, ZERO_FRAME_SUBTITLE, ZERO_FRAME_CONTEXT, ZERO_FRAME_STATUS);
			vector<shared_char> datas;
			datas.emplace_back(*message->description);
			datas.emplace_back(description);
			datas.emplace_back(shared_char().set_int64x(message->plan_id));
			datas.emplace_back(message->write_state());
			datas.emplace_back(result[0]);
			for (size_t idx = 2; idx < result.size() && idx < result[0].size(); idx++)
			{
				if (result[0][idx] == ZERO_FRAME_END)
					break;
				description.append_frame(result[0][idx]);
				datas.emplace_back(result[idx - 1]);
			}
			return send_response(datas);
		}
	}
}
