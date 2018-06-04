#include "../stdafx.h"
#include "zero_station.h"

#define port_redis_key "net:port:next"

namespace agebull
{
	namespace zmq_net
	{
		map<int64, vector<sharp_char>> zero_station::results;
		boost::mutex zero_station::results_mutex_;

		zero_station::zero_station(const string name, int type, int request_zmq_type)
			: request_zmq_type_(request_zmq_type)
			, poll_items_(nullptr)
			, poll_count_(0)
			, task_semaphore_(0)
			, station_name_(name)
			, station_type_(type)
			, state_semaphore_(1)
			, config_(station_warehouse::get_config(name))
			, request_scoket_tcp_(nullptr)
			, request_socket_ipc_(nullptr)
			, worker_in_socket_tcp_(nullptr)
			, worker_out_socket_tcp_(nullptr)
			, worker_out_socket_ipc_(nullptr)
			, zmq_state_(zmq_socket_state::Succeed)
		{
			assert(request_zmq_type_ != ZMQ_PUB);
		}
		zero_station::zero_station(shared_ptr<zero_config>& config, int type, int request_zmq_type)
			: request_zmq_type_(request_zmq_type)
			, poll_items_(nullptr)
			, poll_count_(0)
			, task_semaphore_(0xFF)
			, station_name_(config->station_name_)
			, station_type_(type)
			, state_semaphore_(1)
			, config_(config)
			, request_scoket_tcp_(nullptr)
			, request_socket_ipc_(nullptr)
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
			boost::lock_guard<boost::mutex> guard(mutex_);
			config_->station_state_ = station_state::Start;
			zmq_state_ = zmq_socket_state::Succeed;
			poll_count_ = 0;

			if (station_type_ >= STATION_TYPE_API)
			{
				poll_count_ = 3;
			}
			else
			{
				poll_count_ = 2;
			}
			const char* station_name = get_station_name();
			poll_items_ = new zmq_pollitem_t[poll_count_];
			memset(poll_items_, 0, sizeof(zmq_pollitem_t) * poll_count_);
			request_scoket_tcp_ = socket_ex::create_res_socket_tcp(station_name, request_zmq_type_, config_->request_port_);
			if (request_scoket_tcp_ == nullptr)
			{
				config_->station_state_ = station_state::Failed;
				config_->error("initialize request tpc", zmq_strerror(zmq_errno()));
				return false;
			}
			int cnt = 0;
			poll_items_[cnt++] = { request_scoket_tcp_, 0, ZMQ_POLLIN, 0 };
			if(json_config::use_ipc_protocol)
			{
				request_socket_ipc_ = socket_ex::create_res_socket_ipc(station_name, "req", request_zmq_type_);
				if (request_socket_ipc_ == nullptr)
				{
					config_->station_state_ = station_state::Failed;
					config_->error("initialize request ipc", zmq_strerror(zmq_errno()));
					return false;
				}
				poll_items_[cnt++] = { request_socket_ipc_, 0, ZMQ_POLLIN, 0 };
			}
			

			if (station_type_ >= STATION_TYPE_API)
			{
				worker_out_socket_tcp_ = socket_ex::create_res_socket_tcp(station_name, ZMQ_PUSH, config_->worker_out_port_);
				if (worker_out_socket_tcp_ == nullptr)
				{
					config_->station_state_ = station_state::Failed;
					config_->error("initialize worker out", zmq_strerror(zmq_errno()));
					return false;
				}

				worker_in_socket_tcp_ = socket_ex::create_res_socket_tcp(station_name, ZMQ_DEALER, config_->worker_in_port_);
				if (worker_out_socket_tcp_ == nullptr)
				{
					config_->station_state_ = station_state::Failed;
					config_->error("initialize worker in", zmq_strerror(zmq_errno()));
					return false;
				}
				poll_items_[cnt++] = { worker_in_socket_tcp_, 0, ZMQ_POLLIN, 0 };
			}
			else
			{
				worker_out_socket_tcp_ = socket_ex::create_res_socket_tcp(station_name, ZMQ_PUB, config_->worker_out_port_);
				if (worker_out_socket_tcp_ == nullptr)
				{
					config_->station_state_ = station_state::Failed;
					config_->error("initialize worker out", zmq_strerror(zmq_errno()));
					return false;
				}
				if (json_config::use_ipc_protocol)
				{
					worker_out_socket_ipc_ = socket_ex::create_res_socket_ipc(station_name, "sub", ZMQ_PUB);
					if (worker_out_socket_ipc_ == nullptr)
					{
						config_->station_state_ = station_state::Failed;
						config_->error("initialize worker out", zmq_strerror(zmq_errno()));
						return false;
					}
				}
			}
			config_->station_state_ = station_state::Run;
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
		* \brief
		* \return
		*/

		bool zero_station::poll()
		{
			config_->runing();
			task_semaphore_.wait();
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

				const int state = zmq_poll(poll_items_, poll_count_, 1000);
				if (state == 0|| !can_do())//超时或需要关闭
					continue;
				if (state < 0)
				{
					zmq_state_ = check_zmq_error();
					if (zmq_state_ < zmq_socket_state::Again)
						continue;
					break;
				}
				zmq_state_ = zmq_socket_state::Succeed;
#pragma omp parallel  for schedule(dynamic,3)
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
							request(poll_items_[idx].socket, true);
						}
						else if (poll_items_[idx].socket == worker_in_socket_tcp_)
						{
							config_->worker_in++;
							response();
						}
					}
					//if (poll_items_[idx].revents & ZMQ_POLLOUT)
					//{
					//	if (poll_items_[idx].socket == request_scoket_tcp_)
					//	{
					//		config_->request_out++;
					//	}
					//	else if (poll_items_[idx].socket == request_socket_ipc_)
					//	{
					//		config_->request_out++;
					//	}
					//	else if (poll_items_[idx].socket == response_socket_tcp_)
					//	{
					//		config_->response_out++;
					//	}
					//}
					if (poll_items_[idx].revents & ZMQ_POLLERR)
					{
						const zmq_socket_state err_state = check_zmq_error();
						config_->error("ZMQ_POLLERR", state_str(err_state));
					}
				}
			}
			const zmq_socket_state state = zmq_state_;
			config_->closing();
			task_semaphore_.wait();
			return state < zmq_socket_state::Term && state > zmq_socket_state::Empty;
		}


		/**
		* \brief 暂停
		*/

		bool zero_station::pause(bool waiting)
		{
			if (station_state::Run != config_->station_state_)
				return false;
			config_->station_state_ = station_state::Pause;
			state_semaphore_.post();
			monitor_async(config_->station_name_, "station_pause", "");
			return true;
		}

		/**
		* \brief 继续
		*/
		bool zero_station::resume(bool waiting)
		{
			if (station_state::Pause != config_->station_state_)
				return false;
			config_->station_state_ = station_state::Run;
			state_semaphore_.post();
			monitor_async(config_->station_name_, "station_resume", "");
			return true;
		}

		/**
		* \brief 结束
		*/

		bool zero_station::close(bool waiting)
		{
			if (station_state::Pause != config_->station_state_ && station_state::Run != config_->station_state_)
				return false;
			config_->station_state_ = station_state::Closing;
			state_semaphore_.post();
			monitor_async(config_->station_name_, "station_closing", "");
			while (waiting && config_->station_state_ == station_state::Closing)
				thread_sleep(200);
			return true;
		}

		/**
		* \brief 工作进入计划
		*/
		void zero_station::job_plan(ZMQ_HANDLE socket, vector<sharp_char>& list)//const int64 id, sharp_char& global_id, 
		{
			sharp_char& description = list[2];
			char* const buf = description.get_buffer();
			const auto frame_size = description.size();
			buf[1] = ZERO_STATE_CODE_PLAN;
			size_t plan = 0, rqid = 0, glid = 0;
			for (size_t idx = 2; idx <= frame_size; idx++)
			{
				switch (buf[idx])
				{
				case ZERO_FRAME_PLAN:
					plan = idx + 1;
					break;
				case ZERO_FRAME_REQUEST_ID:
					rqid = idx + 1;
					break;
				case ZERO_FRAME_GLOBAL_ID:
					glid = idx + 1;
					break;
				}
			}
			const sharp_char global_id = glid == 0 ? nullptr : list[glid];
			if (plan == 0)
			{
				send_request_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID_ID, *global_id, rqid == 0 ? nullptr : *list[rqid], "plan frame no find");
				return;
			}
			plan_message message;
			message.read_plan(list[plan].get_buffer());
			message.plan_id = atoll(*global_id);
			if (rqid != 0)
				message.request_id = list[rqid];
			message.request_caller = list[0];
			message.messages_description = description;
			message.messages = list;
			plan_next(message, true);
			send_request_status(socket, *list[0], ZERO_STATUS_PLAN_ID, *global_id, rqid == 0 ? nullptr : *list[rqid]);
		}

		void zero_station::save_plan(ZMQ_HANDLE socket, vector<sharp_char> list) const
		{
			plan_message message;
			message.request_caller = list[0];
			for (size_t idx = 3; idx < list.size(); idx++)
			{
				message.messages.emplace_back(list[idx]);
			}
			message.read_plan(list[0].get_buffer());
			plan_next(message, true);
		} /**
		* \brief 计划轮询
		*/
		void zero_station::plan_poll_()
		{
			config_->log("plan poll start");
			task_semaphore_.post();
			while (can_do())
			{
				bool doit = true;
				for (int i = 0; i < 60; i++)
				{
					thread_sleep(1000);
					if (!can_do())
					{
						doit = false;
						break;
					}
				}
				if (!doit)
					break;
				vector<plan_message> messages;
				load_now(messages);
				for (plan_message msg : messages)
				{
					command(*msg.request_caller, msg.messages);
					if (zmq_state_ == zmq_socket_state::Succeed)
					{
						plan_next(msg, false);
					}
				}
			}
			config_->log("plan poll end");
			task_semaphore_.post();
		}

		/**
		* \brief 载入现在到期的内容
		*/
		void zero_station::load_now(vector<plan_message>& messages) const
		{
			char zkey[100];
			sprintf(zkey, "zero:plan:%s", get_station_name());
			vector<acl::string> keys;
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);
			trans_redis::get_context()->zrangebyscore(zkey, 0, static_cast<double>(time(nullptr)), &keys);
			for (const acl::string& key : keys)
			{
				plan_message message;
				load_message(static_cast<uint>(acl_atoll(key.c_str())), message);
				messages.emplace_back(message);
			}
		}

		/**
		* \brief 删除一个计划
		*/

		bool zero_station::remove_next(plan_message& message) const
		{
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);
			char zkey[MAX_PATH];
			sprintf(zkey, "zero:plan:%s", get_station_name());
			char id[MAX_PATH];

			sprintf(id, "%lld", message.plan_id);
			return trans_redis::get_context()->zrem(zkey, id) >= 0;
		}


		/**
		* \brief 计划下一次执行时间
		* \param message
		* \param first
		* \return
		*/
		bool zero_station::plan_next(plan_message& message, const bool first) const
		{
			if (!first && message.plan_repet >= 0 && message.real_repet >= message.plan_repet)
			{
				remove_message(message);
				remove_next(message);
				return false;
			}
			if (!first)
				message.real_repet += 1;
			save_message(message);
			save_next(message);
			return true;
		}


		/**
		* \brief 保存下一次执行时间
		*/
		bool zero_station::save_next(plan_message& message) const
		{
			time_t t = time(nullptr);
			switch (message.plan_type)
			{
			case plan_date_type::time:
				t = message.plan_value;
				break;
			case plan_date_type::minute:
				t += message.plan_value * 60;
				break;
			case plan_date_type::hour:
				t += message.plan_value * 3600;
				break;
			case plan_date_type::day:
				t += message.plan_value * 24 * 3600;
				break;
			default: return false;
			}
			char zkey[MAX_PATH];
			sprintf(zkey, "msg:time:%s", get_station_name());

			char id[MAX_PATH];
			sprintf(id, "%lld", message.plan_id);
			map<acl::string, double> value;
			value.insert(make_pair(id, static_cast<double>(t)));

			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);
			{
				return trans_redis::get_context()->zadd(zkey, value) >= 0;
			}
		}

		/**
		* \brief 保存消息
		*/

		bool zero_station::save_message(plan_message& message) const
		{
			char key[MAX_PATH];
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);
			sprintf(key, "zero:message:%s:%llx", get_station_name(), message.plan_id);
			return trans_redis::get_context()->set(key, message.write_json().c_str());
		}

		/**
		* \brief 读取消息
		*/

		bool zero_station::load_message(uint id, plan_message& message) const
		{
			char key[MAX_PATH];
			sprintf(key, "zero:message:%s:%8x", get_station_name(), id);
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);
			acl::string val(128);
			if (!trans_redis::get_context()->get(key, val) || val.empty())
			{
				return false;
			}
			message.read_json(val);

			return true;
		}


		/**
		* \brief 删除一个消息
		*/

		bool zero_station::remove_message(plan_message& message) const
		{
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);
			trans_redis& redis = trans_redis::get_context();
			char key[MAX_PATH];
			char id[MAX_PATH];
			sprintf(id, "%llx", message.plan_id);
			//1 删除消息
			sprintf(key, "zero:message:%s:%llx", get_station_name(), message.plan_id);
			redis->del(key);
			//2 删除计划
			if (message.plan_type > plan_date_type::none)
			{
				sprintf(key, "zero:plan:%s", get_station_name());
				redis->zrem(key, id);
			}
			//3 删除参与者
			sprintf(key, "zero:worker:%s:%llx", get_station_name(), message.plan_id);
			acl::string val(128);
			while (redis->spop(key, val))
			{
				sprintf(key, "zero:request:%s:%s", get_station_name(), val.c_str());
				redis->srem(key, id);
			}
			redis->del(key);
			//4 删除返回值
			sprintf(key, "zero:result:%s:%llx", get_station_name(), message.plan_id);
			redis->del(key);
			return true;
		}

		/**
		* \brief 保存消息参与者
		*/

		bool zero_station::save_message_worker(uint msgid, vector<const char*>& workers) const
		{
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);
			char key[MAX_PATH];
			sprintf(key, "zero:worker:%s:%8x", get_station_name(), msgid);
			trans_redis::get_context()->sadd(key, workers);
			char id[MAX_PATH];

			sprintf(id, "%d", msgid);
			for (auto work : workers)
			{
				sprintf(key, "zero:request:%s:%s", get_station_name(), work);
				trans_redis::get_context()->sadd(key, id);
			}
			return true;
		}

		/**
		* \brief 保存消息参与者返回值
		*/

		bool zero_station::save_message_result(uint msgid, const string& worker, const string& response) const
		{
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);

			char key[MAX_PATH];
			sprintf(key, "zero:worker:%s:%8x", get_station_name(), msgid);
			trans_redis::get_context()->srem(key, worker.c_str());

			char id[MAX_PATH];
			sprintf(id, "%d", msgid);
			sprintf(key, "zero:request:%s:%s", get_station_name(), worker.c_str());
			trans_redis::get_context()->srem(key, id);

			sprintf(key, "zero:result:%s:%8x", get_station_name(), msgid);
			trans_redis::get_context()->hset(key, worker.c_str(), response.c_str());
			return true;
		}

		/**
		* \brief 取一个参与者的消息返回值
		*/

		acl::string zero_station::get_message_result(uint msgid, const char* worker) const
		{
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);

			char key[MAX_PATH];

			sprintf(key, "zero:result:%s:%8x", get_station_name(), msgid);
			acl::string val(128);
			trans_redis::get_context()->hget(key, worker, val);
			return val;
		}

		/**
		* \brief 取全部参与者消息返回值
		*/

		map<acl::string, acl::string> zero_station::get_message_result(uint msgid) const
		{
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);

			char key[MAX_PATH];
			map<acl::string, acl::string> result;
			sprintf(key, "zero:result:%s:%8x", get_station_name(), msgid);
			trans_redis::get_context()->hgetall(key, result);
			return result;
		}
	}
}