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
			boost::thread(boost::bind(run_proxy_poll, station.get()));
			//station->task_semaphore_.wait();
			station->poll();
			station->task_semaphore_.wait();
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
		* \brief 扩展初始化
		*/
		void proxy_dispatcher::initialize_ext()
		{
			for (auto& proxy : json_config::global["proxy"].value_map)
			{
				proxy_item item;
				item.name = proxy.first.c_str();
				item.res_addr = proxy.second.str("res");
				item.req_addr = proxy.second.str("req");
				proxys_.insert(make_pair(item.name, item));
			}
		}

		/**
		* \brief 消息泵
		* \return
		*/
		void proxy_dispatcher::proxy_poll()
		{
			if (proxys_.size() == 0)
				return;
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

			while (can_do())
			{
				const int state = zmq_poll(poll_items, index, 500);
				for (int idx = 0; idx < index; idx++)
				{
					proxy_item* item = proxys2[idx];
					if (item->req_socket == nullptr)
					{
						item->req_socket = socket_ex::create_req_socket(item->name.c_str(), ZMQ_PULL, item->req_addr.c_str(), "proxy");
						item->res_socket = socket_ex::create_req_socket(item->name.c_str(), ZMQ_PUSH, item->res_addr.c_str(), "proxy");
						zmq_monitor::set_monitor(item->name.c_str(), &item->req_socket, "proxy");
						poll_items[idx] = { item->req_socket, 0,item->req_socket == nullptr ? static_cast<short>(ZMQ_POLLERR) : static_cast<short>(ZMQ_POLLIN), 0 };
					}
				}
				if (state < 0)
				{
					zmq_state_ = socket_ex::check_zmq_error();
					if (zmq_state_ < zmq_socket_state::again)
						continue;
					break;
				}
				zmq_state_ = zmq_socket_state::succeed;
				//#pragma omp parallel  for schedule(dynamic,3)
				for (int idx = 0; idx < index; idx++)
				{
					if (poll_items[idx].revents & ZMQ_POLLIN)
					{
						proxy_item* item = proxys2[idx];
						vector<shared_char> list;
						zmq_state_ = socket_ex::recv(item->req_socket, list);
						on_start(item->res_socket, item->name, list);
					}
					poll_items[idx].revents = 0;
				}
			}
			delete[] poll_items;
			for (auto& item : proxys_)
			{
				socket_ex::close_res_socket(item.second.req_socket, item.second.req_addr.c_str());
				socket_ex::close_res_socket(item.second.res_socket, item.second.res_addr.c_str());
			}
			proxys_.clear();
		}
		/**
		* \brief 工作开始（发送到工作者）
		*/
		inline void proxy_dispatcher::job_start(zmq_handler socket, vector<shared_char>& list, bool inner)
		{
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
				on_result(res_socket, *list[0], ZERO_STATUS_FRAME_INVALID_ID);
				return false;
			}
			vector<shared_char> frames;
			shared_char plan_caller(128);
			sprintf(plan_caller.get_buffer(), "#:proxy:%s", *name); //请求者(虚拟)
			frames.emplace_back(plan_caller);

			shared_char description(16);
			description.state(ZERO_BYTE_COMMAND_PROXY);
			description.append_frame(ZERO_FRAME_ORIGINAL_1);
			frames.emplace_back(name);
			description.append_frame(ZERO_FRAME_ORIGINAL_2);
			frames.emplace_back(list[0]);

			bool hase = false;
			acl::string station;
			for (size_t idx = 2; idx <= static_cast<size_t>(list[1][0] + 2); idx++)
			{
				switch (list[1][idx])
				{
				case ZERO_FRAME_STATION_ID:
					station = list[idx];
					auto config = station_warehouse::get_config(station.c_str(), false);
					if (!config)
					{
						on_result(res_socket, *list[0], ZERO_STATUS_NOT_FIND_ID);
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
				auto config = station_warehouse::get_config(station.c_str(), false);
				on_result(res_socket, *list[0], config ? ZERO_STATUS_NOT_WORKER_ID : ZERO_STATUS_NOT_FIND_ID);
				return false;
			}
			inner_socket socket("proxy", station.c_str());
			if (socket.send(frames) != zmq_socket_state::succeed)
			{
				on_result(res_socket, *list[0], ZERO_STATUS_SEND_ERROR_ID);
				return false;
			}
			return true;
		}

		/**
		* \brief 计划执行返回
		*/
		void proxy_dispatcher::job_end(vector<shared_char>& list)
		{
			zmq_handler socket = nullptr;
			int hase = 0;
			for (size_t idx = 2; idx <= static_cast<size_t>(list[1][0] + 2); idx++)
			{
				switch (list[1][idx])
				{
				case ZERO_FRAME_ORIGINAL_1:
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
				case ZERO_FRAME_ORIGINAL_2:
					list[0] = list[idx];
					list[idx] = "";
					hase++;
					break;
				}
			}
			if (hase != 2)
				return;
			send_response(socket, list);
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
			send_response(socket, frames);
		}

	}
}
