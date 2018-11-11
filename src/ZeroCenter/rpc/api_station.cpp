#include "../stdafx.h"
#include "api_station.h"
#include "inner_socket.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 执行
		*/
		void api_station::launch(shared_ptr<api_station>& station)
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
			station->task_semaphore_.post();
			station->poll();
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
		inline void api_station::job_start(zmq_handler socket, vector<shared_char>& list, bool inner)
		{
			shared_char caller = list[0];
			if (inner)
				list.erase(list.begin());
			var description = list[1];
			if (!send_response(list))
			{
				send_request_status(socket, list, description, inner, ZERO_STATUS_NOT_WORKER_ID);
			}
			else if (description[1] == ZERO_BYTE_COMMAND_PROXY)//必须返回信息到代理
			{
				send_request_status(socket, list, description, inner, ZERO_STATUS_RUNING_ID);
			}
		}
		/**
		* \brief 内部命令
		*/
		bool api_station::extend_command(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner)
		{
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
				send_request_status(socket, list, description, inner, ZERO_STATUS_NOT_WORKER_ID);
				return true;
			};
			case ZERO_BYTE_COMMAND_CLOSE_REQUEST:
			{
				/*boost::lock_guard<boost::mutex> guard(results_mutex_);
				const auto iter = results.find(atoll(*list[glid_index]));
				if (iter != results.end())
					results.erase(iter);*/
				send_request_status(socket, list, description, inner, ZERO_STATUS_OK_ID);
				return true;
			}
			}
			return false;
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
			send_request_result(list[0][0] == '-' ? request_socket_inproc_ : request_scoket_tcp_, list);
		}

	}
}
