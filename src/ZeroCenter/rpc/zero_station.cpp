#include "../stdafx.h"
#include "zero_station.h"
#include <netinet/in.h>
#include <arpa/inet.h>

#define port_redis_key "net:port:next"

namespace agebull
{
	namespace zero_net
	{

		//map<int64, vector<shared_char>> zero_station::results;
		//boost::mutex zero_station::results_mutex_;

		zero_station::zero_station(const string name, int type, int req_zmq_type, int res_zmq_type)
			: req_zmq_type_(req_zmq_type)
			, res_zmq_type_(res_zmq_type)
			, station_type_(type)
			, poll_items_(nullptr)
			, poll_count_(0)
			, task_semaphore_(0)
			, station_name_(name)
			, config_(station_warehouse::get_config(name))
			, request_scoket_tcp_(nullptr)
			//, request_socket_ipc_(nullptr)
			, request_socket_inproc_(nullptr)
			, plan_socket_inproc_(nullptr)
			, worker_in_socket_tcp_(nullptr)
			, worker_out_socket_tcp_(nullptr)
			//, worker_out_socket_ipc_(nullptr)
			, zmq_state_(zmq_socket_state::succeed)
		{
			assert(req_zmq_type_ != ZMQ_PUB);
		}

		zero_station::zero_station(shared_ptr<zero_config>& config, int type, int req_zmq_type, int res_zmq_type)
			: req_zmq_type_(req_zmq_type)
			, res_zmq_type_(res_zmq_type)
			, station_type_(type)
			, poll_items_(nullptr)
			, poll_count_(0)
			, task_semaphore_(0)
			, station_name_(config->station_name_)
			, config_(config)
			, request_scoket_tcp_(nullptr)
			//, request_socket_ipc_(nullptr)
			, request_socket_inproc_(nullptr)
			, plan_socket_inproc_(nullptr)
			, worker_in_socket_tcp_(nullptr)
			, worker_out_socket_tcp_(nullptr)
			//, worker_out_socket_ipc_(nullptr)
			, zmq_state_(zmq_socket_state::succeed)
		{
			assert(req_zmq_type_ != ZMQ_PUB);
		}

		/**
		* \brief 初始化
		*/
		bool zero_station::initialize()
		{
			if (config_->is_state(station_state::stop))
				return false;
			boost::lock_guard<boost::mutex> guard(mutex_);
			config_->runtime_state(station_state::start);
			zmq_state_ = zmq_socket_state::succeed;


			const char* station_name = get_station_name();
			poll_items_ = new zmq_pollitem_t[5];
			memset(poll_items_, 0, sizeof(zmq_pollitem_t) * 5);
			poll_count_ = 0;
			request_scoket_tcp_ = socket_ex::create_res_socket_tcp(station_name, req_zmq_type_, config_->request_port_);
			if (request_scoket_tcp_ == nullptr)
			{
				config_->runtime_state(station_state::failed);
				config_->error("initialize request tpc", zmq_strerror(zmq_errno()));
				return false;
			}
			poll_items_[poll_count_++] = { request_scoket_tcp_, 0, ZMQ_POLLIN, 0 };
			//if (json_config::use_ipc_protocol)
			//{
			//	request_socket_ipc_ = socket_ex::create_res_socket_ipc(station_name, "req", req_zmq_type_);
			//	if (request_socket_ipc_ == nullptr)
			//	{
			//		config_->runtime_state(station_state::Failed);
			//		config_->error("initialize request ipc", zmq_strerror(zmq_errno()));
			//		return false;
			//	}
			//	poll_items_[poll_count_++] = { request_socket_ipc_, 0, ZMQ_POLLIN, 0 };
			//}

			request_socket_inproc_ = socket_ex::create_res_socket_inproc(station_name, req_zmq_type_);
			if (request_socket_inproc_ == nullptr)
			{
				config_->runtime_state(station_state::failed);
				config_->error("initialize request ipc", zmq_strerror(zmq_errno()));
				return false;
			}
			poll_items_[poll_count_++] = { request_socket_inproc_, 0, ZMQ_POLLIN, 0 };


			if (IS_GENERAL_STATION(station_type_))
				plan_socket_inproc_ = socket_ex::create_req_socket_inproc("PlanDispatcher", station_name);

			if (IS_PUB_STATION(station_type_))
			{
				worker_out_socket_tcp_ = socket_ex::create_res_socket_tcp(station_name, ZMQ_PUB, config_->worker_out_port_);
				if (worker_out_socket_tcp_ == nullptr)
				{
					config_->runtime_state(station_state::failed);
					config_->error("initialize worker out", zmq_strerror(zmq_errno()));
					return false;
				}
				if (station_type_ == station_type_queue)
				{
					worker_in_socket_tcp_ = socket_ex::create_res_socket_tcp(station_name, ZMQ_PUB, config_->worker_in_port_);
					if (worker_in_socket_tcp_ == nullptr)
					{
						config_->runtime_state(station_state::failed);
						config_->error("initialize worker in", zmq_strerror(zmq_errno()));
						return false;
					}
				}
				//if (json_config::use_ipc_protocol)
				//{
				//	worker_out_socket_ipc_ = socket_ex::create_res_socket_ipc(station_name, "sub", ZMQ_PUB);
				//	if (worker_out_socket_ipc_ == nullptr)
				//	{
				//		config_->runtime_state(station_state::Failed);
				//		config_->error("initialize worker out", zmq_strerror(zmq_errno()));
				//		return false;
				//	}
				//}
			}
			else
			{
				int type = station_type_ == station_type_route_api ? ZMQ_ROUTER : ZMQ_PUSH;
				worker_out_socket_tcp_ = socket_ex::create_res_socket_tcp(station_name, type, config_->worker_out_port_);
				if (worker_out_socket_tcp_ == nullptr)
				{
					config_->runtime_state(station_state::failed);
					config_->error("initialize worker out", zmq_strerror(zmq_errno()));
					return false;
				}
				worker_in_socket_tcp_ = socket_ex::create_res_socket_tcp(station_name, ZMQ_DEALER, config_->worker_in_port_);
				if (worker_in_socket_tcp_ == nullptr)
				{
					config_->runtime_state(station_state::failed);
					config_->error("initialize worker in", zmq_strerror(zmq_errno()));
					return false;
				}
				poll_items_[poll_count_++] = { worker_in_socket_tcp_, 0, ZMQ_POLLIN, 0 };
			}
			initialize_ext();
			switch (station_type_)
			{
			case station_type_dispatcher:
				zmq_monitor::set_monitor(station_name, worker_out_socket_tcp_, "dis");
				break;
			case station_type_queue:
				zmq_monitor::set_monitor(station_name, worker_out_socket_tcp_, "que");
				break;
			case station_type_notify:
				zmq_monitor::set_monitor(station_name, worker_out_socket_tcp_, "pub");
				break;
			case station_type_api:
			case station_type_vote:
			case station_type_route_api:
				zmq_monitor::set_monitor(station_name, worker_in_socket_tcp_, "api");
				break;
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
			//if (request_socket_ipc_ != nullptr)
			//{
			//	make_ipc_address(address, get_station_name(), "req");
			//	socket_ex::close_res_socket(request_socket_ipc_, address);
			//}
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
			//if (worker_out_socket_ipc_ != nullptr)
			//{
			//	make_ipc_address(address, get_station_name(), "sub");
			//	socket_ex::close_res_socket(worker_out_socket_ipc_, address);
			//}
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
					zmq_state_ = zmq_socket_state::intr;
					break;
				}
				const int state = zmq_poll(poll_items_, poll_count_, 10000);
				if (state == 0)//超时或需要关闭
					continue;
				if (state < 0)
				{
					zmq_state_ = socket_ex::check_zmq_error();
					if (zmq_state_ < zmq_socket_state::again)
						continue;
					break;
				}
				zmq_state_ = zmq_socket_state::succeed;
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
						//else if (poll_items_[idx].socket == request_socket_ipc_)
						//{
						//	config_->request_in++;
						//	request(poll_items_[idx].socket, false);
						//}
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
			return state < zmq_socket_state::term && state > zmq_socket_state::empty;
		}

		/**
		* \brief 工作集合的响应
		*/
		void zero_station::response()
		{
			vector<shared_char> list;
			zmq_state_ = socket_ex::recv(worker_in_socket_tcp_, list);
			if (zmq_state_ == zmq_socket_state::timed_out)
			{
				config_->worker_err++;
				return;
			}
			if (zmq_state_ != zmq_socket_state::succeed)
			{
				config_->worker_err++;
				config_->error("read work result", socket_ex::state_str(zmq_state_));
				return;
			}
			if (list.size() < 2)
			{
				config_->worker_err++;
				config_->error("work result layout error", "size < 2");
				return;
			}
			if (list[0][0] == '*' && station_type_ != station_type_plan)
			{
				plan_end(list);
			}
			else {
				job_end(list);
			}
		}

		/**
		* \brief 调用集合的响应
		*/
		void zero_station::request(zmq_handler socket, bool inner)
		{
			vector<shared_char> list;
			zmq_state_ = socket_ex::recv(socket, list);
			if (zmq_state_ != zmq_socket_state::succeed)
			{
				config_->log(socket_ex::state_str(zmq_state_));
				return;
			}
			if (config_->station_state_ == station_state::pause)
			{
				send_request_status(socket, *list[0], ZERO_STATUS_PAUSE_ID);
				return;
			}
			const size_t list_size = inner ? list.size() - 1 : list.size();
			if (list_size < 2)
			{
				send_request_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID_ID);
				return;
			}
			shared_char& description = list[inner ? 2 : 1];
			if (description.command() < ZERO_BYTE_COMMAND_NONE)
			{
				send_request_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID_ID);
				return;
			}
			if (!inner)
			{
				if(description[description.frame_size()+1] != ZERO_FRAME_SERVICE_KEY)
				{
					send_request_status(socket, *list[0], ZERO_STATUS_DENY_ERROR_ID);
					return;
				}
				if(strcmp(*list[list.size() - 1], json_config::service_key) != 0)
				{
					send_request_status(socket, *list[0], ZERO_STATUS_DENY_ERROR_ID);
					return;
				}
			}
			const size_t descr_size = description.size();
			const size_t frame_size = description.frame_size();
			if ((frame_size + 1) > descr_size || (frame_size + 2) != list_size)
			{
				send_request_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID_ID);
				return;
			}
			if (!IS_SYS_STATION(station_type_) && inner_command(socket, list, description, inner))
			{
				return;
			}
			job_start(socket, list, inner);
		}

		/**
		* \brief 内部命令
		*/
		bool zero_station::inner_command(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner)
		{
			
			const uchar state = description.command();
			if (state == ZERO_BYTE_COMMAND_PLAN)
			{
				job_plan(socket, list);
				return true;
			}
			if (state == ZERO_BYTE_COMMAND_GLOBAL_ID)
			{
				char global_id[32];
				sprintf(global_id, "%llx", station_warehouse::get_glogal_id());

				size_t reqid = 0, reqer = 0;
				for (size_t i = 2; i <= description.frame_size() + 2; i++)
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
				return true;
			}
			return extend_command(socket, list, description, inner);
		}

		/**
		* \brief 内部命令
		*/
		void zero_station::send_request_status(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner, uchar state)
		{
			shared_char caller = list[0];
			if (inner)
				list.erase(list.begin());
			size_t reqid = 0, reqer = 0, glid_index = 0;
			for (size_t i = 2; i <= static_cast<size_t>(description[0] + 2); i++)
			{
				switch (description[i])
				{
				case ZERO_FRAME_REQUESTER:
					reqer = i;
					break;
				case ZERO_FRAME_REQUEST_ID:
					reqid = i;
					break;
				case ZERO_FRAME_GLOBAL_ID:
					glid_index = i;
					break;
				}
			}
			send_request_status(socket, *caller, state, list, glid_index, reqid, reqer);
		}
		/**
		* \brief 工作进入计划
		*/
		void zero_station::job_plan(zmq_handler socket, vector<shared_char>& list)
		{
			list.emplace_back(station_name_);
			list[1].append_frame(ZERO_FRAME_STATION_ID);
			if (socket_ex::send(plan_socket_inproc_, list) != zmq_socket_state::succeed)
			{
				send_request_status(socket, *list[0], ZERO_STATUS_SEND_ERROR_ID);
				return;
			}

			vector<shared_char> result;
			if (socket_ex::recv(plan_socket_inproc_, result) == zmq_socket_state::succeed)
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
			if (socket_ex::send(plan_socket_inproc_, list) != zmq_socket_state::succeed)
			{
				config_->error("send to plan dispatcher failed", desc_str(false, list[1].get_buffer(), list.size()));
			}
		}
		/**
		* \brief 暂停
		*/

		bool zero_station::pause(bool waiting)
		{
			if (!config_->is_state(station_state::run))
				return false;
			config_->log("pause");
			config_->runtime_state(station_state::pause);
			while (waiting && !config_->is_state(station_state::pause))
				thread_sleep(50);
			zero_event(zero_net_event::event_station_pause, "station", config_->station_name_.c_str(), nullptr);
			return true;
		}

		/**
		* \brief 继续
		*/
		bool zero_station::resume(bool waiting)
		{
			if (!config_->is_state(station_state::pause))
				return false;
			config_->log("resume");
			config_->runtime_state(station_state::run);
			while (waiting && !config_->is_state(station_state::run))
				thread_sleep(50);
			zero_event(zero_net_event::event_station_resume, "station", config_->station_name_.c_str(), nullptr);
			return true;
		}

		/**
		* \brief 结束
		*/

		bool zero_station::close(bool waiting)
		{
			switch (config_->get_state())
			{
			case station_state::run:
			case station_state::pause:
			case station_state::stop:
				break;
			default:
				return false;
			}
			config_->log("close");
			config_->runtime_state(station_state::closing);
			zero_event(zero_net_event::event_station_closing, "station", config_->station_name_.c_str(), nullptr);
			while (waiting && config_->is_state(station_state::closing))
				thread_sleep(50);
			return true;
		}
	}
}