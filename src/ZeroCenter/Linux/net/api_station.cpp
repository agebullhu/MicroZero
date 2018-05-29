#include "../stdafx.h"
#include "api_station.h"
#include "ipc_request_socket.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 开始执行一条命令
		*/
		sharp_char api_station::command(const char* caller, vector<sharp_char> lines)
		{
			vector<sharp_char> response;
			ipc_request_socket<ZMQ_REQ> socket(caller, get_station_name());
			if (socket.request(lines, response))
				return response.empty() ? ZERO_STATUS_ERROR : response[0];
			switch (socket.get_state())
			{
			case zmq_socket_state::TimedOut:
				return ZERO_STATUS_TIMEOUT;
			default:
				return ZERO_STATUS_NET_ERROR;
			}
		}

		/**
		* \brief 执行
		*/
		void api_station::launch(shared_ptr<api_station>& station)
		{
			zero_config& config = station->get_config();
			config.log_start();
			if (!station_warehouse::join(station.get()))
			{
				config.log_failed();
				return;
			}
			if (!station->initialize())
			{
				config.log_failed();
				return;
			}
			station->poll();
			station_warehouse::left(station.get());
			station->destruct();
			if (config.station_state_ != station_state::Uninstall && get_net_state() == NET_STATE_RUNING)
			{
				config.station_state_ = station_state::ReStart;
				run(station->get_config_ptr());
			}
			else
			{
				config.log_closed();
			}
			thread_sleep(1000);
		}

		/**
		* \brief 工作开始（发送到工作者）
		*/
		inline void api_station::job_start(ZMQ_HANDLE socket, vector<sharp_char>& list)//, sharp_char& global_id
		{
			//路由到其中一个工作对象
			//const char* worker;
			//while(!_balance.get_host(worker))
			//{
			//	worker_left(worker);
			//}
			//if (worker == nullptr)
			//{
			//	zmq_state_ = send_status(socket, *caller, ZERO_STATUS_API_NOT_WORKER);
			//	return zmq_state_ == zmq_socket_state::Succeed;
			//}
			//if (list[2][1] == ZERO_COMMAND_WAITING)
			//	return;
			switch (list[2][1])
			{
			case ZERO_COMMAND_FIND_RESULT:
			{
				boost::lock_guard<boost::mutex> guard(results_mutex_);
				auto iter = results.find(atoll(*list[3]));
				if (iter == results.end())
					send_request_status(socket, *list[0], ZERO_STATUS_NOT_WORKER_ID, *list[3]);
				else
				{
					iter->second[0] = list[0];
					send_request_result(iter->second);
				}
			}break;
			case ZERO_COMMAND_CLOSE_RESULT:
			{
				boost::lock_guard<boost::mutex> guard(results_mutex_);
				const auto iter = results.find(atoll(*list[3]));
				if (iter != results.end())
					results.erase(iter);
				send_request_status(socket, *list[0], ZERO_STATUS_OK_ID, *list[3]);
			}break;
			default:
				if (!send_response(list))
				{
					send_request_status(socket, *list[0], ZERO_STATUS_NOT_WORKER_ID);//, *global_id
				}
				break;
			}
		}
		/**
		* \brief 工作结束(发送到请求者)
		*/
		void api_station::job_end(vector<sharp_char>& list)
		{
			{
				boost::lock_guard<boost::mutex> guard(results_mutex_);
				results.insert(make_pair(atoll(*list[list.size() - 1]), list));
				while (results.size() > 9999)
				{
					results.erase(results.begin());
				}
			}
			send_request_result(list);
		}

	}
}
