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
			station_config& config = station->get_config();
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
			set_station_thread_end(config.station_name.c_str());
		}

		/**
		* \brief 工作开始 : 处理请求数据
		*/
		inline void route_api_station::job_start(zmq_handler socket, vector<shared_char>& list, bool inner, bool old)
		{
			trace(1, list, nullptr);
			if (config_->get_state() == station_state::pause)
			{
				send_request_status(socket, *list[0], zero_def::status::pause, true, true);
				return;
			}
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
			if (worker == 0)
			{
				send_request_status_by_trace(socket, *caller, zero_def::status::frame_invalid, list, glid_index, reqid, reqer);
				return;
			}
			shared_char router = list[worker];
			list.erase(list.begin() + worker);
			list.insert(list.begin(), router);
			if (send_response(list, false) != zmq_socket_state::succeed)
			{
				send_request_status_by_trace(socket, *caller, zero_def::status::not_worker, list, glid_index, reqid, reqer);
			}
			else if (!old || description.command() == zero_def::command::proxy)//必须返回信息到代理
			{
				send_request_status_by_trace(socket, *caller, zero_def::status::runing, list, glid_index, reqid, reqer);
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
			const auto caller = list[0];
			list.erase(list.begin());
			if (list[0][0] == zero_def::name::head::plan && station_type_ != zero_def::station_type::plan)
			{
				plan_end(list);
			}
			else if (list[0][0] == zero_def::name::head::proxy && station_type_ != zero_def::station_type::proxy)
			{
				proxy_end(list);
			}
			else
			{
				list.push_back(caller);
				uchar tag = list[1].tag();
				list[1].append_frame(zero_def::frame::responser);
				list[1].tag(tag);
				if (list[0][0] == zero_def::name::head::inproc)
				{
					send_request_result(request_socket_inproc_, list[1].state(), list, true, true);
				}
				else
				{
					send_request_result(request_scoket_tcp_, list[1].state(), list, true, true);
				}
			}
		}

	}
}
