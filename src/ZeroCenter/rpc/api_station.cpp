#include "../stdafx.h"
#include "api_station.h"
#include "inner_socket.h"
#include "zero_frames.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 执行
		*/
		void api_station::launch(shared_ptr<api_station>& station)
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
			//station->task_semaphore_.post();
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
		inline void api_station::job_start(zmq_handler socket, vector<shared_char>& list, bool inner, bool old)
		{
			trace(1, list, nullptr);
			const shared_char caller = list[0];
			if (inner)
				list.erase(list.begin());
			var description = list[1];
			if (config_->get_state() == station_state::pause)
			{
				send_request_status_by_trace(socket, list, description, zero_def::status::pause,true);
				return;
			}
			zmq_socket_state state;
			if (global_config::api_route_mode)
			{
				do
				{
					auto * wk = config_->get_workers();
					if (wk == nullptr)
					{
						send_request_status_by_trace(socket, list, description, zero_def::status::not_worker, true);
						return;
					}
					list.insert(list.begin(), wk->worker_name);
					state = send_response(list, true);
					list.erase(list.begin());
					if (state == zmq_socket_state::host_un_reach)
					{
						wk->worker_state = 5;
						THREAD_SLEEP(5);
					}
				} while (state == zmq_socket_state::host_un_reach);
			}
			else
			{
				state = send_response(list, true);
			}
			if (state != zmq_socket_state::succeed)
			{
				zero_frames frames(list, list[1]);
				frames.check_in_frames();
				send_request_status_by_trace(socket, *caller, zero_def::status::not_worker, list, frames.glid_index, frames.rqid_index, frames.rqer_index);
			}
			else if (!old || description.command() == zero_def::command::proxy)//必须返回信息到代理
			{
				zero_frames frames(list, list[1]);
				frames.check_in_frames();
				send_request_status_by_trace(socket, *caller, zero_def::status::runing, list, frames.glid_index, frames.rqid_index, frames.rqer_index);
			}
		}

		/**
		* \brief 收到PING
		*/
		void api_station::ping(zmq_handler socket, vector<shared_char>& list)
		{
			config_->worker_heartbeat(*list[0]);
		}
		/**
		* 心跳的响应
		*/
		bool api_station::heartbeat(zmq_handler socket, uchar cmd, vector<shared_char> list)
		{
			switch (cmd)
			{
			case zero_def::command::heart_join:
				config_->worker_join(*list[0], *list[4]);
				break;
			case zero_def::command::heart_ready:
				config_->worker_ready(*list[0]);
				worker_event(zero_net_event::event_client_join,*list[2], *list[0]);
				break;
			case zero_def::command::heart_left:
				config_->worker_left(*list[0]);
				worker_event(zero_net_event::event_client_left,*list[2], *list[0]);
				break;
			default:
				config_->worker_ready(*list[0]);
				break;
			}
			send_request_status(socket, *list[0], zero_def::status::ok, false, false);
			return true;
		}


		/**
		* \brief 内部命令
		* /
		bool api_station::simple_command_ex(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner)
		{
			switch (description[1])
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
				send_request_status(socket, list, description, inner, zero_def::status::not_worker);
				return true;
			};
			case zero_def::command::close_request:
			{
				//boost::lock_guard<boost::mutex> guard(results_mutex_);
				//const auto iter = results.find(atoll(*list[glid_index]));
				//if (iter != results.end())
				//	results.erase(iter);
				send_request_status(socket, list, description, inner, zero_def::status::ok);
				return true;
			}
			}
			return false;
		}*/

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
				const uchar tag = list[1].tag();
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
