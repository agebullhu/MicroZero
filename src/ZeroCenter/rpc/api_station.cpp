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
		inline void api_station::job_start(zmq_handler socket, vector<shared_char>& list, bool inner)
		{
			shared_char caller = list[0];
			if (inner)
				list.erase(list.begin());
			var description = list[1];
			zmq_socket_state state;
			if (global_config::api_route_mode)
			{
				do
				{
					worker * wk = config_->get_worker();
					if (wk == nullptr)
					{
						send_request_status(socket, list, description, inner, zero_def::status::not_worker);
						return;
					}
					list.insert(list.begin(), wk->identity);
					state = send_response(list);
					list.erase(list.begin());
					if (state == zmq_socket_state::host_un_reach)
					{
						wk->state = 5;
						THREAD_SLEEP(5);
					}
				} while (state == zmq_socket_state::host_un_reach);
			}
			else
			{
				state = send_response(list);
			}
			if (state != zmq_socket_state::succeed)
			{
				send_request_status(socket, list, description, inner, zero_def::status::not_worker);
			}
			else if (description[1] == zero_def::command::proxy)//必须返回信息到代理
			{
				send_request_status(socket, list, description, inner, zero_def::status::runing);
			}
		}

		/**
		* 心跳的响应
		
		bool api_station::heartbeat(uchar cmd, vector<shared_char> list)
		{
			switch (cmd)
			{
			case zero_def::command::heart_join:
				config_->worker_join(*list[3], *list[4]);
				return true;
			case zero_def::command::heart_ready:
				zero_event(zero_net_event::event_client_join, "station", *list[2], *list[3]);
				config_->worker_ready(*list[3]);
				return true;
			case zero_def::command::heart_pitpat:
				config_->worker_heartbeat(*list[3]);
				return true;
			case zero_def::command::heart_left:
				zero_event(zero_net_event::event_client_left, "station", *list[2], *list[3]);
				config_->worker_left(*list[3]);
				return true;
			default:
				return false;
			}
		}*/

		/**
		* \brief 内部命令
		*/
		bool api_station::simple_command_ex(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner)
		{
			auto command = description.command();
			switch (command)
			{
			case zero_def::command::heart_join:
			case zero_def::command::heart_ready:
			case zero_def::command::heart_pitpat:
			case zero_def::command::heart_left:
				return true;// heartbeat(command, list);
			}
			return false;
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
			if (list[0][0] == zero_def::name::head::inproc)
			{
				send_request_result(request_socket_inproc_, list);
			}
			else
			{
				send_request_result(request_scoket_tcp_, list);
			}
		}
	}
}
