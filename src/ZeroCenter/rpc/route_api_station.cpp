#include "../stdafx.h"
#include "route_api_station.h"
#include "inner_socket.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 执行
		*/
		void route_api_station::launch(shared_ptr<route_api_station>& station)
		{
			zero_config& config = station->get_config();
			if (!station->initialize())
			{
				config.failed("initialize");
				set_command_thread_bad(config.station_name.c_str());
				return;
			}
			if (!station_warehouse::join(station.get()))
			{
				config.failed("join warehouse");
				set_command_thread_bad(config.station_name.c_str());
				return;
			}
			station->task_semaphore_.post();
			station->poll();
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
			set_command_thread_end(config.station_name.c_str());
		}

		/**
		* \brief 工作开始（发送到工作者）
		*/
		inline void route_api_station::job_start(zmq_handler socket, vector<shared_char>& list, bool inner)
		{
			shared_char caller = list[0];
			if (inner)
				list.erase(list.begin());
			shared_char& description = list[1];
			size_t reqid = 0, reqer = 0, worker = 0, glid_index = 0;
			for (size_t idx = 2; idx <= description.desc_size() && idx < list.size(); idx++)
			{
				switch (description[idx])
				{
				case zero_def::frame::requester:
					reqer = idx;
					break;
				case zero_def::frame::responser:
					worker = idx;
					break; 
				case zero_def::frame::request_id:
					reqid = idx;
					break;
				case zero_def::frame::global_id:
					glid_index = idx;
					break;
				}
			}
			if (glid_index == 0)
			{
				send_request_status(socket, *caller, zero_def::status::frame_invalid, list, 0, reqid, reqer);
				return;
			}
			switch (description.command())
			{
			case zero_def::command::find_result:
			{
				//boost::lock_guard<boost::mutex> guard(results_mutex_);
				//auto iter = results.find(atoll(*list[glid_index]));
				//if (iter != results.end())
				//{
				//	iter->second[0] = list[0];
				//	send_request_result(iter->second);
				//}
				//else
				send_request_status(socket, *caller, zero_def::status::not_worker, list, glid_index, reqid, reqer);
			}break;
			case zero_def::command::close_request:
			{
				/*boost::lock_guard<boost::mutex> guard(results_mutex_);
				const auto iter = results.find(atoll(*list[glid_index]));
				if (iter != results.end())
					results.erase(iter);*/
				send_request_status(socket, *caller, zero_def::status::ok, list, glid_index, reqid, reqer);
			}break;
			default:
				socket_ex::send_addr(socket, *list[worker]);
				if (send_response(list) != zmq_socket_state::succeed)
				{
					send_request_status(socket, *caller, zero_def::status::not_worker, list, glid_index, reqid, reqer);
				}
				else if (description.command() == zero_def::command::proxy)//必须返回信息到代理
				{
					send_request_status(socket, *caller, zero_def::status::runing, list, glid_index, reqid, reqer);
				}
				break;
			}
		}
		/**
		* \brief 工作结束(发送到请求者)
		*/
		void route_api_station::job_end(vector<shared_char>& list)
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
