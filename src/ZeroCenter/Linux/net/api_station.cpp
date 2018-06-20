#include "../stdafx.h"
#include "api_station.h"
#include "inner_socket.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 开始执行一条命令
		*/
		shared_char api_station::command(const char* caller, vector<shared_char> lines)
		{
			//vector<shared_char> response;
			//ipc_request_socket<ZMQ_REQ> socket(caller, get_station_name());
			//if (socket.request(lines, response))
			//	return response.empty() ? ZERO_STATUS_ERROR : response[0];
			//switch (socket.get_state())
			//{
			//case zmq_socket_state::TimedOut:
			//	return ZERO_STATUS_TIMEOUT;
			//default:
			//	return ZERO_STATUS_NET_ERROR;
			//}
				return ZERO_STATUS_NET_ERROR;
		}

		/**
		* \brief 执行
		*/
		void api_station::launch(shared_ptr<api_station>& station)
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
			//boost::thread(boost::bind(monitor_poll, station.get()));
			station->task_semaphore_.post();
			station->poll();
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
		inline void api_station::job_start(ZMQ_HANDLE socket, vector<shared_char>& list, bool inner)
		{
			int frame_skip = inner ? 1 : 0;
			var description = *list[inner ? 2 : 1];
			size_t reqid = 0, reqer = 0, glid_index = 0;
			for (size_t i = 2; i <= static_cast<size_t>(description[0] + 2); i++)
			{
				switch (description[i])
				{
				case ZERO_FRAME_REQUESTER:
					reqer = i + frame_skip;
					break;
				case ZERO_FRAME_REQUEST_ID:
					reqid = i + frame_skip;
					break;
				case ZERO_FRAME_GLOBAL_ID:
					glid_index = i + frame_skip;
					break;
				}
			}
			if (glid_index == 0)
			{
				send_request_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID_ID,
					nullptr,
					reqid == 0 ? nullptr : *list[reqid],
					reqer == 0 ? nullptr : *list[reqer]);
			}
			switch (description[1])
			{
			case ZERO_BYTE_COMMAND_FIND_RESULT:
			{
				//boost::lock_guard<boost::mutex> guard(results_mutex_);
				//auto iter = results.find(atoll(*list[glid_index]));
				//if (iter != results.end())
				//{
				//	iter->second[0] = list[0];
				//	send_request_result(iter->second);
				//}
				//else
				send_request_status(socket, *list[0], ZERO_STATUS_NOT_WORKER_ID,
					*list[glid_index],
					reqid == 0 ? nullptr : *list[reqid],
					reqer == 0 ? nullptr : *list[reqer]);
			}break;
			case ZERO_BYTE_COMMAND_CLOSE_REQUEST:
			{
				/*boost::lock_guard<boost::mutex> guard(results_mutex_);
				const auto iter = results.find(atoll(*list[glid_index]));
				if (iter != results.end())
					results.erase(iter);*/
				send_request_status(socket, *list[0], ZERO_STATUS_OK_ID,
					*list[glid_index],
					reqid == 0 ? nullptr : *list[reqid],
					reqer == 0 ? nullptr : *list[reqer]);
			}break;
			default:
				if (!send_response(list))
				{
					send_request_status(socket, *list[0], ZERO_STATUS_NOT_WORKER_ID,
						*list[glid_index],
						reqid == 0 ? nullptr : *list[reqid],
						reqer == 0 ? nullptr : *list[reqer]);
				}
				else if(description[1] == ZERO_BYTE_COMMAND_PROXY)//必须返回信息到代理
				{
					send_request_status(socket, *list[0], ZERO_STATUS_RUNING_ID,
						*list[glid_index],
						reqid == 0 ? nullptr : *list[reqid],
						reqer == 0 ? nullptr : *list[reqer]);
				}
				break;
			}
		}
		/**
		* \brief 工作结束(发送到请求者)
		*/
		void api_station::job_end(vector<shared_char>& list)
		{
			/*{
				boost::lock_guard<boost::mutex> guard(results_mutex_);
				results.insert(make_pair(atoll(*list[list.size() - 1]), list));
				while (results.size() > 9999)
				{
					results.erase(results.begin());
				}
			}*/
			send_request_result(list[0][0] == '-' ? request_socket_ipc_ : request_scoket_tcp_,list);
		}

	}
}
