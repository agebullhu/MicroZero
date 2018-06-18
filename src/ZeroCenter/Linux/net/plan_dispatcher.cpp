#include "../stdafx.h"
#include "zero_plan.h"
#include "plan_dispatcher.h"
#include "ipc_request_socket.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 单例
		*/
		plan_dispatcher* plan_dispatcher::instance = nullptr;

		/**
		* \brief 执行计划
		*/
		int plan_dispatcher::exec_plan(plan_message& msg)
		{
			if (msg.last_state == ZERO_STATUS_RUNING_ID)
			{
				var span = boost::posix_time::second_clock::local_time() - boost::posix_time::from_time_t(msg.exec_time);
				//超时未到且还未执行完成,不重复下发
				if (span.seconds() < json_config::plan_exec_timeout) //
				{
					get_config().error("plan delay to short", msg.plan_id);
					return 0;
				}
			}

			if (msg.station.empty() || msg.messages.size() < 5)
			{
				msg.last_state = ZERO_STATUS_FRAME_PLANERROR_ID;
				get_config().error("error plan data", msg.plan_id);
				return 2; //错误数据不执行
			}
			auto ptr = sockets_.find(*msg.station);
			shared_ptr<inproc_request_socket> socket;
			if (ptr == sockets_.end())
			{
				socket = make_shared<inproc_request_socket>(get_station_name(), *msg.station);
				sockets_.insert(make_pair(*msg.station, socket));
			}
			else
			{
				socket = ptr->second;
			}
			vector<shared_char> result;
			msg.messages[0].set_desc_type(ZERO_BYTE_COMMAND_PROXY);
			shared_char time = msg.messages[msg.messages.size() - 2];
			if (socket->send(msg.messages) == zmq_socket_state::Succeed)
			{
				if (socket->recv(result) == zmq_socket_state::Succeed)
				{
					msg.last_state = result[0][1];
					if (msg.last_state == ZERO_STATUS_RUNING_ID)
					{
						msg.save_message_result(*msg.station, result);
					}
				}
				else
				{
					msg.last_state = ZERO_STATUS_RECV_ERROR_ID;
				}
			}
			else
			{
				msg.last_state = ZERO_STATUS_SEND_ERROR_ID;
			}
			return 2;
		}

		/**
		* \brief 计划轮询
		*/
		void plan_dispatcher::plan_poll()
		{
			get_config().log("plan poll start");
			while (can_do())
			{
				thread_sleep(1000);
				if (!can_do())
					break;
				vector<plan_message> messages;
				plan_message::load_now([this](plan_message& msg)
				{
					if (!can_do())
						return 1;
					return exec_plan(msg);
				});
			}

			sockets_.clear();
			get_config().log("plan poll end");
			task_semaphore_.post();
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
				return;
			}
			list.erase(list.begin());
			def_msg_key(key, &message);
			message.load_message(key);
			message.last_state = list[0][1];
			message.real_repet += 1;
			message.save();
			message.save_message_result(*message.station, list);

			publish(message, list);
			return true;
		}

		/**
		*\brief 发布消息
		*/
		bool plan_dispatcher::publish(plan_message& message, vector<shared_char>& result)
		{
			vector<shared_char> datas;
			acl::string title;
			title.format("%s:exec:%lld", *message.station, message.plan_id);

			datas.emplace_back(title);
			shared_char desc;
			datas.emplace_back(desc);
			datas.emplace_back(message.caller);
			datas.emplace_back(message.write_json());
			if (message.last_state >= ZERO_STATUS_SEND_ERROR_ID)
			{
				const char* result_desc = *result[0];
				char* description = desc.alloc_desc(result_desc[0] + 2, static_cast<char>(message.last_state));
				description[2] = ZERO_FRAME_PUBLISHER;
				description[3] = ZERO_FRAME_CONTENT;
				for (size_t idx = 2; idx < result[0].size(); idx++)
				{
					if (result_desc[idx] == ZERO_FRAME_END)
						break;
					description[idx + 2] = result_desc[idx];
					datas.emplace_back(result[idx - 1]);
				}
			}
			else
			{
				char* description = desc.alloc_desc(2, static_cast<char>(message.last_state));
				description[2] = ZERO_FRAME_PUBLISHER;
				description[3] = ZERO_FRAME_CONTENT;
			}
			desc.check_desc_size();
			return send_response(datas);
		}

		/**
		* \brief 开始执行一条命令
		*/
		shared_char plan_dispatcher::command(const char* caller, vector<shared_char> lines)
		{
			return ZERO_STATUS_NET_ERROR;
		}

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
		* \brief 计划进入
		*/
		bool plan_dispatcher::on_plan_start(ZMQ_HANDLE socket, vector<shared_char>& list)
		{
			shared_char caller = list[0];
			list.erase(list.begin());
			char* const description = list[1].get_buffer();
			const size_t desc_size = description[0] + 2;

			plan_message message;
			message.caller = list[0];
			shared_char plan_caller(128);
			message.messages.emplace_back(plan_caller);
			message.messages.emplace_back(list[1]);
			size_t plan = 0, rqid = 0, glid = 0, reqer = 0, cmdid = 0;
			size_t pidx = 1;
			for (size_t idx = 2; idx <= desc_size; idx++)
			{
				switch (description[idx])
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
				description[++pidx] = description[idx];
				message.messages.emplace_back(list[idx]);
			}
			if (plan == 0 || message.station.empty() || cmdid == 0)
			{
				send_request_status(socket, *caller, ZERO_STATUS_FRAME_INVALID_ID, glid == 0 ? nullptr : *list[glid],
				                    rqid == 0 ? nullptr : *list[rqid], reqer == 0 ? nullptr : *list[reqer]);
				return false;
			}
			description[++pidx] = ZERO_FRAME_PLAN_TIME;
			message.messages.emplace_back("1234567987654321"); //未来用时间
			shared_char global_id;
			if (glid == 0)
			{
				global_id.set_value(message.plan_id = station_warehouse::get_glogal_id());
				description[++pidx] = ZERO_FRAME_GLOBAL_ID;
				message.messages.emplace_back(global_id);
			}
			else
			{
				global_id = list[glid];
				message.plan_id = atoll(*list[glid]);
			}
			sprintf(plan_caller.get_buffer(), "*:%lld", message.plan_id); //计划特殊的请求者(虚拟)


			message.read_plan(*list[plan]);

			description[0] = static_cast<char>(pidx - 1);
			description[1] = ZERO_BYTE_COMMAND_PROXY;
			list[1].check_desc_size();
			memset(description + pidx + 1, 0, list[1].size() - pidx - 1);

			message.save();
			send_request_status(socket, *caller, ZERO_STATUS_PLAN_ID, *global_id, rqid == 0 ? nullptr : *list[rqid],
			                    reqer == 0 ? nullptr : *list[reqer]);
			return true;
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
		* \brief 工作结束(发送到请求者)
		*/
		void plan_dispatcher::job_end(vector<shared_char>& list)
		{
		}
	}
}
