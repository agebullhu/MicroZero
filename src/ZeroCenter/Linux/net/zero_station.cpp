#include "../stdafx.h"
#include "zero_station.h"

#define port_redis_key "net:port:next"

namespace agebull
{
	namespace zmq_net
	{
		//map<int64, vector<shared_char>> zero_station::results;
		//boost::mutex zero_station::results_mutex_;

		zero_station::zero_station(const string name, int type, int request_zmq_type)
			: request_zmq_type_(request_zmq_type)
			, station_type_(type)
			, poll_items_(nullptr)
			, poll_count_(0)
			, task_semaphore_(0)
			, station_name_(name)
			, state_semaphore_(1)
			, config_(station_warehouse::get_config(name))
			, request_scoket_tcp_(nullptr)
			, request_socket_ipc_(nullptr)
			, request_socket_inproc_(nullptr)
			, plan_socket_inproc_(nullptr)
			, worker_in_socket_tcp_(nullptr)
			, worker_out_socket_tcp_(nullptr)
			, worker_out_socket_ipc_(nullptr)
			, zmq_state_(zmq_socket_state::Succeed)
		{
			assert(request_zmq_type_ != ZMQ_PUB);
		}

		zero_station::zero_station(shared_ptr<zero_config>& config, int type, int request_zmq_type)
			: request_zmq_type_(request_zmq_type)
			, station_type_(type)
			, poll_items_(nullptr)
			, poll_count_(0)
			, task_semaphore_(0)
			, station_name_(config->station_name_)
			, state_semaphore_(1)
			, config_(config)
			, request_scoket_tcp_(nullptr)
			, request_socket_ipc_(nullptr)
			, request_socket_inproc_(nullptr)
			, plan_socket_inproc_(nullptr)
			, worker_in_socket_tcp_(nullptr)
			, worker_out_socket_tcp_(nullptr)
			, worker_out_socket_ipc_(nullptr)
			, zmq_state_(zmq_socket_state::Succeed)
		{
			assert(request_zmq_type_ != ZMQ_PUB);
		}

		/**
		* \brief 初始化
		*/
		bool zero_station::initialize()
		{
			if (config_->is_state(station_state::Uninstall))
				return false;
			boost::lock_guard<boost::mutex> guard(mutex_);
			config_->runtime_state(station_state::Start);
			zmq_state_ = zmq_socket_state::Succeed;


			const char* station_name = get_station_name();
			poll_items_ = new zmq_pollitem_t[5];
			memset(poll_items_, 0, sizeof(zmq_pollitem_t) * 5);
			poll_count_ = 0;
			request_scoket_tcp_ = socket_ex::create_res_socket_tcp(station_name, request_zmq_type_, config_->request_port_);
			if (request_scoket_tcp_ == nullptr)
			{
				config_->runtime_state(station_state::Failed);
				config_->error("initialize request tpc", zmq_strerror(zmq_errno()));
				return false;
			}
			poll_items_[poll_count_++] = { request_scoket_tcp_, 0, ZMQ_POLLIN, 0 };
			if (json_config::use_ipc_protocol)
			{
				request_socket_ipc_ = socket_ex::create_res_socket_ipc(station_name, "req", request_zmq_type_);
				if (request_socket_ipc_ == nullptr)
				{
					config_->runtime_state(station_state::Failed);
					config_->error("initialize request ipc", zmq_strerror(zmq_errno()));
					return false;
				}
				poll_items_[poll_count_++] = { request_socket_ipc_, 0, ZMQ_POLLIN, 0 };
			}

			request_socket_inproc_ = socket_ex::create_res_socket_inproc(station_name, request_zmq_type_);
			if (request_socket_inproc_ == nullptr)
			{
				config_->runtime_state(station_state::Failed);
				config_->error("initialize request ipc", zmq_strerror(zmq_errno()));
				return false;
			}
			poll_items_[poll_count_++] = { request_socket_inproc_, 0, ZMQ_POLLIN, 0 };


			if (station_type_ > STATION_TYPE_DISPATCHER && station_type_ < STATION_TYPE_SPECIAL)
				plan_socket_inproc_ = socket_ex::create_req_socket_inproc("PlanDispatcher", station_name);

			if (station_type_ >= STATION_TYPE_API && station_type_ < STATION_TYPE_SPECIAL)
			{
				worker_out_socket_tcp_ = socket_ex::create_res_socket_tcp(station_name, ZMQ_PUSH, config_->worker_out_port_);
				if (worker_out_socket_tcp_ == nullptr)
				{
					config_->runtime_state(station_state::Failed);
					config_->error("initialize worker out", zmq_strerror(zmq_errno()));
					return false;
				}
				//char addr[256];
				//sprintf(addr, "inproc://%s_monitor.rep", config_->station_name_.c_str());
				//zmq_socket_monitor(worker_out_socket_tcp_, addr, ZMQ_EVENT_ALL);

				worker_in_socket_tcp_ = socket_ex::create_res_socket_tcp(station_name, ZMQ_DEALER, config_->worker_in_port_);
				if (worker_in_socket_tcp_ == nullptr)
				{
					config_->runtime_state(station_state::Failed);
					config_->error("initialize worker in", zmq_strerror(zmq_errno()));
					return false;
				}
				poll_items_[poll_count_++] = { worker_in_socket_tcp_, 0, ZMQ_POLLIN, 0 };
			}
			else
			{
				worker_out_socket_tcp_ = socket_ex::create_res_socket_tcp(station_name, ZMQ_PUB, config_->worker_out_port_);
				if (worker_out_socket_tcp_ == nullptr)
				{
					config_->runtime_state(station_state::Failed);
					config_->error("initialize worker out", zmq_strerror(zmq_errno()));
					return false;
				}
				if (json_config::use_ipc_protocol)
				{
					worker_out_socket_ipc_ = socket_ex::create_res_socket_ipc(station_name, "sub", ZMQ_PUB);
					if (worker_out_socket_ipc_ == nullptr)
					{
						config_->runtime_state(station_state::Failed);
						config_->error("initialize worker out", zmq_strerror(zmq_errno()));
						return false;
					}
				}
			}
			config_->start();
			return true;
		}

		/**
		* \brief 析构
		*/

		bool zero_station::destruct()
		{
			boost::lock_guard<boost::mutex> guard(mutex_);
			if (poll_items_ == nullptr)
				return true;
			if (request_scoket_tcp_ != nullptr)
			{
				socket_ex::close_res_socket(request_scoket_tcp_, config_->get_request_address().c_str());
			}
			if (request_socket_ipc_ != nullptr)
			{
				make_ipc_address(address, get_station_name(), "req");
				socket_ex::close_res_socket(request_socket_ipc_, address);
			}
			if (request_socket_inproc_ != nullptr)
			{
				make_inproc_address(address, get_station_name());
				socket_ex::close_res_socket(request_socket_inproc_, address);
			}
			if (plan_socket_inproc_ != nullptr)
			{
				make_inproc_address(address, "PlanDispatcher");
				socket_ex::close_req_socket(plan_socket_inproc_, address);
			}
			if (worker_in_socket_tcp_ != nullptr)
			{
				socket_ex::close_res_socket(worker_in_socket_tcp_, config_->get_work_in_address().c_str());
			}
			if (worker_out_socket_tcp_ != nullptr)
			{
				socket_ex::close_res_socket(worker_out_socket_tcp_, config_->get_work_out_address().c_str());
			}
			if (worker_out_socket_ipc_ != nullptr)
			{
				make_ipc_address(address, get_station_name(), "sub");
				socket_ex::close_res_socket(worker_out_socket_ipc_, address);
			}
			delete[]poll_items_;
			poll_items_ = nullptr;
			return true;
		}
		/**
		* \brief 消息泵
		* \return
		*/
		bool zero_station::poll()
		{
			config_->runing();
			//登记线程开始
			set_command_thread_run(get_station_name());
			while (true)
			{
				if (!can_do())
				{
					zmq_state_ = zmq_socket_state::Intr;
					break;
				}
				if (check_pause())
					continue;
				if (!can_do())
				{
					zmq_state_ = zmq_socket_state::Intr;
					break;
				}

				const int state = zmq_poll(poll_items_, poll_count_, 10000);
				if (state == 0 || !can_do())//超时或需要关闭
					continue;
				if (state < 0)
				{
					zmq_state_ = check_zmq_error();
					if (zmq_state_ < zmq_socket_state::Again)
						continue;
					break;
				}
				zmq_state_ = zmq_socket_state::Succeed;
				//#pragma omp parallel  for schedule(dynamic,3)
				for (int idx = 0; idx < poll_count_; idx++)
				{
					if (poll_items_[idx].revents & ZMQ_POLLIN)
					{
						if (poll_items_[idx].socket == request_scoket_tcp_)
						{
							config_->request_in++;
							request(poll_items_[idx].socket, false);
						}
						else if (poll_items_[idx].socket == request_socket_ipc_)
						{
							config_->request_in++;
							request(poll_items_[idx].socket, false);
						}
						else if (poll_items_[idx].socket == request_socket_inproc_)
						{
							config_->request_in++;
							request(poll_items_[idx].socket, true);
						}
						else if (poll_items_[idx].socket == worker_in_socket_tcp_)
						{
							config_->worker_in++;
							response();
						}
					}
					/*if (poll_items_[idx].revents & ZMQ_POLLOUT)
					{
						if (poll_items_[idx].socket == request_scoket_tcp_)
						{
							config_->request_out++;
						}
						else if (poll_items_[idx].socket == request_socket_ipc_)
						{
							config_->request_out++;
						}
						else if (poll_items_[idx].socket == worker_out_socket_tcp_)
						{
							config_->worker_out++;
						}
					}
					if (poll_items_[idx].revents & ZMQ_POLLERR)
					{
						const zmq_socket_state err_state = check_zmq_error();
						config_->error("ZMQ_POLLERR", state_str(err_state));
					}*/
				}
			}
			const zmq_socket_state state = zmq_state_;
			config_->closing();
			return state < zmq_socket_state::Term && state > zmq_socket_state::Empty;
		}

		/**
		* \brief 工作集合的响应
		*/
		void zero_station::response()
		{

			vector<shared_char> list;
			zmq_state_ = socket_ex::recv(worker_in_socket_tcp_, list);
			if (zmq_state_ == zmq_socket_state::TimedOut)
			{
				config_->worker_err++;
				return;
			}
			if (zmq_state_ != zmq_socket_state::Succeed)
			{
				config_->worker_err++;
				config_->error("read work result", state_str(zmq_state_));
				return;
			}
			if (list.size() < 2)
			{
				config_->worker_err++;
				config_->error("work result layout error", "size < 2");
				return;
			}
			if (list[0][0] == '*')//来自计划
			{
				plan_end(list);
			}
			else
			{
				job_end(list);
			}
		}

		/**
		* \brief 调用集合的响应
		*/
		void zero_station::request(ZMQ_HANDLE socket, bool inner)
		{
			vector<shared_char> list;
			zmq_state_ = socket_ex::recv(socket, list);
			if (zmq_state_ != zmq_socket_state::Succeed)
			{
				config_->log(state_str(zmq_state_));
				return;
			}
			const size_t list_size = inner ? list.size() - 1 : list.size();
			if (list_size < 2)
			{
				send_request_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID_ID);
				return;
			}
			shared_char description = list[inner ? 2 : 1];
			const size_t descr_size = description.desc_size();
			const size_t frame_size = description.frame_size();
			char state = description.state();
			if (state < ZERO_BYTE_COMMAND_NONE || frame_size >= descr_size || (frame_size + 2) != list_size)
			{
				send_request_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID_ID);
				return;
			}
			if (station_type_ > STATION_TYPE_DISPATCHER && station_type_ < STATION_TYPE_SPECIAL)
			{
				if (state == ZERO_BYTE_COMMAND_PLAN)
				{
					job_plan(socket, list);
					return;
				}
				if (state == ZERO_BYTE_COMMAND_GLOBAL_ID)
				{
					char global_id[32];
					sprintf(global_id, "%llx", station_warehouse::get_glogal_id());

					size_t reqid = 0, reqer = 0;
					for (size_t i = 2; i <= frame_size + 2; i++)
					{
						switch (description[i])
						{
						case ZERO_FRAME_REQUESTER:
							reqer = i;
							break;
						case ZERO_FRAME_REQUEST_ID:
							reqid = i;
							break;
						}
					}
					send_request_status(socket, *list[0], ZERO_STATUS_OK_ID,
						global_id,
						reqid == 0 ? nullptr : *list[reqid],
						reqer == 0 ? nullptr : *list[reqer]);
					return;
				}
			}
			job_start(socket, list, inner);
		}

		/**
		* \brief 工作进入计划
		*/
		void zero_station::job_plan(ZMQ_HANDLE socket, vector<shared_char>& list)
		{
			list.emplace_back(station_name_);
			list[1].append_frame(ZERO_FRAME_STATION_ID);
			if (socket_ex::send(plan_socket_inproc_, list) != zmq_socket_state::Succeed)
			{
				send_request_status(socket, *list[0], ZERO_STATUS_SEND_ERROR_ID);
				return;
			}

			vector<shared_char> result;
			if (socket_ex::recv(plan_socket_inproc_, result) == zmq_socket_state::Succeed)
			{
				result.insert(result.begin(), list[0]);
				send_request_result(socket, result);
			}
			else
			{
				send_request_status(socket, *list[0], ZERO_STATUS_RECV_ERROR_ID);
			}
		}
		/**
		* \brief 计划执行完成
		*/
		void zero_station::plan_end(vector<shared_char>& list)
		{
			shared_char description = list[1];
			list.emplace_back(station_name_);
			description.append_frame(ZERO_FRAME_STATION_ID);
			if (socket_ex::send(plan_socket_inproc_, list) != zmq_socket_state::Succeed)
			{
				config_->error("send to plan dispatcher failed", desc_str(false, *list[1], list.size()));
			}
		}
		/**
		* \brief 暂停
		*/

		bool zero_station::pause(bool waiting)
		{
			if (!config_->is_state(station_state::Run))
				return false;
			config_->log("pause");
			config_->runtime_state(station_state::Pause);
			state_semaphore_.post();
			zero_event_async(config_->station_name_, zero_net_event::event_station_pause, "");
			return true;
		}

		/**
		* \brief 继续
		*/
		bool zero_station::resume(bool waiting)
		{
			if (!config_->is_state(station_state::Pause))
				return false;
			config_->log("resume");
			config_->runtime_state(station_state::Run);
			state_semaphore_.post();
			zero_event_async(config_->station_name_, zero_net_event::event_station_resume, "");
			return true;
		}

		/**
		* \brief 结束
		*/

		bool zero_station::close(bool waiting)
		{
			switch (config_->get_state())
			{
			case station_state::Run:
			case station_state::Pause:
			case station_state::Uninstall:
				break;
			default:
				return false;
			}
			config_->log("close");
			config_->runtime_state(station_state::Closing);
			state_semaphore_.post();
			zero_event_async(config_->station_name_, zero_net_event::event_station_closing, "");
			while (waiting && config_->is_state(station_state::Closing))
				thread_sleep(200);
			return true;
		}



		/**
		* \brief 网络监控
		* \return
		*/
		void zero_station::monitor() const
		{
			char addr[256];
			sprintf(addr, "inproc://%s_monitor.rep", config_->station_name_.c_str());

			zmq_event_t event;
			printf("starting monitor...\n");
			void* inproc = zmq_socket(get_zmq_context(), ZMQ_PAIR);
			assert(inproc);
			setsockopt(inproc, ZMQ_RCVTIMEO, 1000);
			zmq_connect(inproc, addr);
			while (get_net_state() < NET_STATE_DISTORY)
			{
				if (read_event_msg(inproc, &event) == 1)
					continue;
				switch (event.event)
				{
				case ZMQ_EVENT_CLOSED:
					log_debug1(DEBUG_BASE, 1, "ZMQ网络监控%d:连接已关闭", event.value);
					zmq_close(inproc);
					return;
				case ZMQ_EVENT_CLOSE_FAILED:
					log_debug1(DEBUG_BASE, 1, "ZMQ网络监控%d:连接关闭失败", event.value);
					zmq_close(inproc);
					return;
				case ZMQ_EVENT_MONITOR_STOPPED:
					log_debug1(DEBUG_BASE, 1, "ZMQ网络监控%d:监控关闭", event.value);
					zmq_close(inproc);
					return;
				case ZMQ_EVENT_LISTENING:
					log_debug1(DEBUG_BASE, 1, "ZMQ网络监控%d:正在侦听数据", event.value);
					break;
				case ZMQ_EVENT_BIND_FAILED:
					log_debug2(DEBUG_BASE, 1, "ZMQ网络监控%d:绑定端口失败%s", event.value, event.address);
					break;
				case ZMQ_EVENT_ACCEPTED:
					log_debug2(DEBUG_BASE, 1, "ZMQ网络监控%d:接收到%s的数据", event.value, event.address);
					break;
				case ZMQ_EVENT_ACCEPT_FAILED:
					log_debug2(DEBUG_BASE, 1, "ZMQ网络监控%d:接收%s的数据出错", event.value, event.address);
					break;
				case ZMQ_EVENT_CONNECTED:
					log_debug2(DEBUG_BASE, 1, "ZMQ网络监控%d:与%s连接成功", event.value, event.address);
					break;
				case ZMQ_EVENT_CONNECT_DELAYED:
					log_debug2(DEBUG_BASE, 1, "ZMQ网络监控%d:与%s连接发生延迟", event.value, event.address);
					break;
				case ZMQ_EVENT_CONNECT_RETRIED:
					log_debug2(DEBUG_BASE, 1, "ZMQ网络监控%d:重新连接%s", event.value, event.address);
					break;
				case ZMQ_EVENT_DISCONNECTED:
					log_debug2(DEBUG_BASE, 1, "ZMQ网络监控%d:与%s连接关闭", event.value, event.address);
					break;
				default: break;
				}
			}
			zmq_close(inproc);
		}
	}
}