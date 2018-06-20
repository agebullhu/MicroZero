#include "../stdafx.h"
#include "zero_plan.h"
#include "plan_dispatcher.h"
#include "inner_socket.h"

namespace agebull
{
	namespace zmq_net
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
			config.start();
			if (!station_warehouse::join(station.get()))
			{
				config.failed("join warehouse");
				set_command_thread_bad(config.station_name_.c_str());
				return;
			}
			if (!station->initialize())
			{
				config.failed("initialize");
				set_command_thread_bad(config.station_name_.c_str());
				return;
			}

			boost::thread(boost::bind(run_plan_poll, station.get()));
			station->poll();
			station->task_semaphore_.wait();
			station_warehouse::left(station.get());
			station->destruct();
			if (config.station_state_ != station_state::Uninstall && get_net_state() == NET_STATE_RUNING)
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
		inline void plan_dispatcher::job_start(ZMQ_HANDLE socket, vector<shared_char>& list, bool inner)
		{
			if (list[0][0] == '*') //返回值
			{
				on_plan_result(list);
			}
			else
			{
				on_plan_start(socket, list);
			}
		}

		/**
		* \brief 计划轮询
		*/
		void plan_dispatcher::plan_poll()
		{
			get_config().log("plan poll start");
			boost::posix_time::ptime pre = boost::posix_time::second_clock::universal_time();
			while (can_do())
			{
				int sec = (int)(boost::posix_time::second_clock::universal_time() - pre).total_milliseconds();
				if (sec < 1000)//执行时间超过1秒则不等待,继续执行
				{
					thread_sleep(1000 - sec);
				}
				if (!can_do())
					break;
				pre = boost::posix_time::second_clock::universal_time();
				plan_message::exec_now([this](plan_message& msg)
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
		bool plan_dispatcher::on_plan_start(ZMQ_HANDLE socket, vector<shared_char>& list)
		{
			shared_char caller = list[0];
			list.erase(list.begin());

			shared_char description(16);
			description.status(ZERO_BYTE_COMMAND_PROXY);
			shared_char plan_caller(128);

			plan_message message;
			message.caller = list[0];
			message.messages.emplace_back(plan_caller);
			message.messages.emplace_back(description);

			size_t plan = 0, rqid = 0, glid = 0, reqer = 0, cmdid = 0;
			for (size_t idx = 2; idx <= static_cast<size_t>(list[1][0] + 2); idx++)
			{
				switch (list[1][idx])
				{
				case ZERO_FRAME_PLAN:
					plan = idx;
					continue;
				case ZERO_FRAME_STATION_ID:
					message.station = list[idx];
				case ZERO_FRAME_END:
					continue;
				case ZERO_FRAME_REQUEST_ID:
					message.request_id = list[idx];
					break;
				case ZERO_FRAME_REQUESTER:
					reqer = idx;
					break;
				case ZERO_FRAME_GLOBAL_ID:
					glid = idx;
					break;
				case ZERO_FRAME_COMMAND:
					cmdid = idx;
					break;
				}
				description.append_frame(list[1][idx]);
				message.messages.emplace_back(list[idx]);
			}
			if (plan == 0 || message.station.empty() || cmdid == 0)
			{
				send_request_status(socket, *caller, ZERO_STATUS_FRAME_INVALID_ID, glid == 0 ? nullptr : *list[glid],
					rqid == 0 ? nullptr : *list[rqid], reqer == 0 ? nullptr : *list[reqer]);
				return false;
			}
			description.append_frame(ZERO_FRAME_PLAN);
			message.messages.emplace_back("");

			shared_char global_id;
			if (glid == 0)
			{
				global_id.set_int64x(message.plan_id = station_warehouse::get_glogal_id());
				description.append_frame(ZERO_FRAME_GLOBAL_ID);
				message.messages.emplace_back(global_id);
			}
			else
			{
				global_id = list[glid];
				message.plan_id = atoll(*list[glid]);
			}
			sprintf(plan_caller.get_buffer(), "*%lld", message.plan_id); //计划特殊的请求者(虚拟)


			message.read_plan(*list[plan]);

			message.save();
			send_request_status(socket, *caller, ZERO_STATUS_PLAN_ID, *global_id, rqid == 0 ? nullptr : *list[rqid],
				reqer == 0 ? nullptr : *list[reqer]);
			return true;
		}

		/**
		* \brief 执行计划
		*/
		int plan_dispatcher::exec_plan(plan_message& message)
		{
			if (!can_do())
				return 0;
			if (message.last_state == ZERO_STATUS_RUNING_ID)
			{
				var span = boost::posix_time::second_clock::universal_time() - boost::posix_time::from_time_t(message.exec_time);
				//超时未到且还未执行完成,不重复下发
				if (span.seconds() < json_config::plan_exec_timeout)
				{
					get_config().error("plan delay to short", message.plan_id);
					return 0;
				}
			}

			if (message.last_state == ZERO_STATUS_FRAME_PLANERROR_ID)
			{
				get_config().error("error plan state remove from plan queue", message.plan_id);
				message.remove_next();
				return 0; //错误数据不执行
			}
			if (message.station.empty() || message.messages.size() < 5)
			{
				message.last_state = ZERO_STATUS_FRAME_PLANERROR_ID;
				get_config().error("error plan messages remove from plan queue", message.plan_id);
				message.remove_next();
				return 2; //错误数据不执行
			}
			auto ptr = sockets_.find(*message.station);
			shared_ptr<inner_socket> socket;
			if (ptr == sockets_.end())
			{
				socket = make_shared<inner_socket>(get_station_name(), *message.station);
				sockets_.insert(make_pair(*message.station, socket));
			}
			else
			{
				socket = ptr->second;
			}
			message.messages[1].status(ZERO_BYTE_COMMAND_PROXY);
			message.messages[message.messages.size() - 2] = message.write_info();
			if (socket->send(message.messages) != zmq_socket_state::Succeed)
			{
				message.last_state = ZERO_STATUS_SEND_ERROR_ID;
				return 2;
			}
			vector<shared_char> result;
			if (socket->recv(result) != zmq_socket_state::Succeed)
			{
				message.last_state = ZERO_STATUS_RECV_ERROR_ID;
				return 2;
			}
			message.last_state = result[0][1];
			if (message.last_state == ZERO_STATUS_RUNING_ID)
			{
				message.exec_time = time_t(nullptr);
			}
			else if (message.last_state == ZERO_STATUS_NOT_WORKER_ID)
			{
				publish(message, result);
			}
			else if (message.last_state == ZERO_STATUS_OK_ID)//仅广播
			{
				on_plan_result(result);
			}
			else
			{
				publish(message, result);
				get_config().error("error plan can`t exec remove from plan queue", message.plan_id);
				message.remove_next();
			}
			return 2;
		}

		/**
		* \brief 计划执行返回
		*/
		bool plan_dispatcher::on_plan_result(vector<shared_char>& list)
		{
			plan_message message;
			try
			{
				message.plan_id = atoll(list[0].get_buffer() + 1);
			}
			catch (const std::exception& ex)
			{
				get_config().error("result bad", ex.what());
				return false;
			}
			list.erase(list.begin());
			def_msg_key(key, &message);
			if (!message.load_message(key))
			{
				get_config().error("message is remove", key);
				return false;
			}
			message.save_message_result(*message.station, list);
			message.last_state = list[0][1];
			if (message.last_state < ZERO_BYTE_COMMAND_WAITING)
			{
				message.save_message();
			}
			else if (message.last_state < ZERO_STATUS_ERROR_FLAG)
			{
				message.real_repet += 1;
				message.plan_next();
			}
			else
			{
				message.save_message();
				message.join_queue(message.plan_time);
			}
			publish(message, list);
			return true;
		}

		/**
		*\brief 发布消息
		*/
		bool plan_dispatcher::publish(plan_message& message, vector<shared_char>& result)
		{
			vector<shared_char> datas;
			shared_char description;
			description.alloc_frame_2(static_cast<char>(message.last_state), ZERO_FRAME_SUBTITLE, ZERO_FRAME_CONTENT_JSON);

			datas.emplace_back(*message.description);
			datas.emplace_back(description);
			datas.emplace_back(shared_char().set_int64x(message.plan_id));
			datas.emplace_back(message.write_json());
			for (size_t idx = 2; idx < result[0].size(); idx++)
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
