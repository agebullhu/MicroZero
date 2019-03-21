
#ifdef PROXYSTATION

#include "../stdafx.h"
#include "proxy_dispatcher.h"
#include "inner_socket.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 单例
		*/
		proxy_dispatcher* proxy_dispatcher::instance = nullptr;

		/**
		* \brief 执行
		*/
		void proxy_dispatcher::launch(shared_ptr<proxy_dispatcher>& station)
		{
			zero_config& config = station->get_config();
			set_station_thread_bad(config.station_name.c_str());
			return;
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
			boost::thread(boost::bind(run_proxy_poll, station.get()));
			//station->task_semaphore_.wait();
			station->poll();
			station->task_semaphore_.wait();
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
		* \brief 消息泵
		* \return
		*/
		void proxy_dispatcher::proxy_poll()
		{
			json_config::read();
			for (auto& proxy : json_config::global["proxy"].value_map)
			{
				proxy_item item;
				item.name = proxy.first.c_str();
				item.res_addr = proxy.second.str("res");
				item.req_addr = proxy.second.str("req");
				proxys_.insert(make_pair(item.name, item));
			}
			if (proxys_.size() == 0)
			{
				config_->log("proxy_poll", "empty");
				return;
			}
			auto* poll_items = new zmq_pollitem_t[proxys_.size()];
			vector<proxy_item*> proxys2;
			int index = 0;
			for (auto& item : proxys_)
			{
				item.second.req_socket = socket_ex::create_req_socket(item.second.name.c_str(), ZMQ_PULL, item.second.req_addr.c_str(), "proxy");
				item.second.res_socket = socket_ex::create_req_socket(item.second.name.c_str(), ZMQ_PUSH, item.second.res_addr.c_str(), "proxy");
				zmq_monitor::set_monitor(item.second.name.c_str(), &item.second.req_socket, "proxy");
				proxys2.emplace_back(&item.second);
				poll_items[index++] = { item.second.req_socket, 0,item.second.req_socket == nullptr ? static_cast<short>(ZMQ_POLLERR) : static_cast<short>(ZMQ_POLLIN), 0 };
			}
			config_->log("proxy_poll", "run");
			zmq_socket_state state;
			//boost::posix_time::ptime start = boost::posix_time::microsec_clock::universal_time();
			while (can_do())
			{
				const int re = zmq_poll(poll_items, index, 500);
				if (re <= 0)
				{
					if (re < 0)
					{
						state = socket_ex::check_zmq_error();
						config_->log("proxy_poll", socket_ex::state_str(state));
					}
					continue;
				}
				for (int idx = 0; idx < index; idx++)
				{
					if (poll_items[idx].revents & ZMQ_POLLIN)
					{
						proxy_item* item = proxys2[idx];
						vector<shared_char> list;
						state = socket_ex::recv(item->req_socket, list);
						if (state == zmq_socket_state::succeed)
							on_start(item->res_socket, item->name, list);
						else
							config_->log(item->name.c_str(), socket_ex::state_str(state));
					}
					poll_items[idx].revents = 0;
					poll_items[idx].fd = 0;
				}
			}
			delete[] poll_items;
			for (auto& item : proxys_)
			{
				socket_ex::close_res_socket(item.second.req_socket, item.second.req_addr.c_str());
				socket_ex::close_res_socket(item.second.res_socket, item.second.res_addr.c_str());
			}
			proxys_.clear();
			config_->log("proxy_poll", "end");
		}

		/**
		* \brief 工作开始 : 处理请求数据
		*/
		inline void proxy_dispatcher::job_start(zmq_handler socket, vector<shared_char>& list, bool inner, bool old)
		{
			if (inner)
			{
				list.erase(list.begin());
				job_end(list);
				return;
			}
			on_start(worker_out_socket_tcp_, "-proxy", list);
		}


		/**
		* \brief 计划进入
		*/
		bool proxy_dispatcher::on_start(zmq_handler res_socket, shared_char name, vector<shared_char>& list)
		{
			if (list.size() == 0)
				return false;
			if (list.size() == 1)
			{
				on_result(res_socket, *list[0], zero_def::status::frame_invalid);
				return false;
			}
			vector<shared_char> frames;
			shared_char plan_caller(128);
			sprintf(plan_caller.c_str(), "#:proxy:%s", *name); //请求者(虚拟)
			frames.emplace_back(plan_caller);

			shared_char description(16);
			frames.emplace_back(description);
			description.state(zero_def::command::none);
			description.append_frame(zero_def::frame::original_1);
			frames.emplace_back(name);
			description.append_frame(zero_def::frame::original_2);
			frames.emplace_back(list[0]);

			bool hase = false;
			acl::string station;
			for (size_t idx = 2; idx <= static_cast<size_t>(list[1][0] + 2) && idx < list.size(); idx++)
			{
				switch (list[1][idx])
				{
				case zero_def::frame::station_id:
					station = list[idx];
					auto config = station_warehouse::get_config(station.c_str(), false);
					if (!config)
					{
						on_result(res_socket, *list[0], zero_def::status::not_find);
						return false;
					}
					hase = true;
					continue;
				}
				description.append_frame(list[1][idx]);
				frames.emplace_back(list[idx]);
			}
			if (!hase)
			{
				on_result(res_socket, *list[0], zero_def::status::not_find);
				return false;
			}
			inner_socket socket("proxy", station.c_str());
			if (socket.send(frames) != zmq_socket_state::succeed)
			{
				on_result(res_socket, *list[0], zero_def::status::send_error);
				return false;
			}
			return true;
		}

		/**
		* \brief 计划执行返回
		*/
		void proxy_dispatcher::job_end(vector<shared_char>& list)
		{
			if (list.size() < 1)
				return;//BUG
			zmq_handler socket = nullptr;
			int hase = 0;
			for (size_t idx = 2; idx <= static_cast<size_t>(list[1][0] + 2); idx++)
			{
				switch (list[1][idx])
				{
				case zero_def::frame::original_1:
					if (strcmp(*list[idx], "-proxy") == 0)
						socket = worker_out_socket_tcp_;
					else
					{
						var iter = proxys_.find(*list[idx]);
						if (iter == proxys_.end())
							return;
						socket = iter->second.res_socket;
					}
					list[idx] = "";
					hase++;
					break;
				case zero_def::frame::original_2:
					list[0] = list[idx];
					list[idx] = "";
					hase++;
					break;
				}
			}
			if (hase != 2)
				return;
			send_response(socket, list,true);
		}

		/**
		* \brief 计划执行返回
		*/
		void proxy_dispatcher::on_result(zmq_handler socket, const char* caller, uchar state)
		{
			vector<shared_char> frames;
			frames.emplace_back(caller);
			shared_char description(3);
			description.state(state);
			frames.emplace_back(description);
			send_response(socket, frames, true);
		}

	}
}

#endif // PROXYSTATION
