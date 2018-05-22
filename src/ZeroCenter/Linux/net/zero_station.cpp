#include "../stdafx.h"
#include "zero_station.h"

#define port_redis_key "net:port:next"

namespace agebull
{
	namespace zmq_net
	{
		zero_station::zero_station(const string name, int type, int request_zmq_type, int response_zmq_type, int heart_zmq_type)
			: station_name_(name)
			, station_type_(type)
			, in_plan_poll_(false)
			, config_(station_warehouse::get_config(name))
			, state_semaphore_(1)
			, request_zmq_type_(request_zmq_type)
			, response_zmq_type_(response_zmq_type)
			, poll_items_(nullptr)
			, poll_count_(0)
			, request_scoket_tcp_(nullptr)
			, request_socket_ipc_(nullptr)
			, response_socket_tcp_(nullptr)
			, zmq_state_(zmq_socket_state::Succeed)
		{

		}

		/**
		* \brief 初始化
		*/

		bool zero_station::initialize()
		{
			config_->station_state_ = station_state::Start;
			zmq_state_ = zmq_socket_state::Succeed;
			poll_count_ = 0;

			if (request_zmq_type_ >= 0 && request_zmq_type_ != ZMQ_PUB)
			{
				poll_count_ += 2;
			}
			if (response_zmq_type_ >= 0 && response_zmq_type_ != ZMQ_PUB)
			{
				poll_count_++;
			}
			poll_items_ = new zmq_pollitem_t[poll_count_];
			memset(poll_items_, 0, sizeof(zmq_pollitem_t) * poll_count_);
			int cnt = 0;

			if (request_zmq_type_ >= 0)
			{
				request_scoket_tcp_ = create_res_socket_tcp(get_station_name(), request_zmq_type_, config_->request_port_);
				if (request_scoket_tcp_ == nullptr)
				{
					config_->station_state_ = station_state::Failed;
					log_error2("%s initialize error(out) %s", get_station_name(), zmq_strerror(zmq_errno()));
					set_command_thread_bad(get_station_name());
					return false;
				}
				request_socket_ipc_ = create_res_socket_ipc(get_station_name(), request_zmq_type_);
				if (request_socket_ipc_ == nullptr)
				{
					config_->station_state_ = station_state::Failed;
					log_error2("%s initialize error(ipc) %s", get_station_name(), zmq_strerror(zmq_errno()));
					set_command_thread_bad(get_station_name());
					return false;
				}
				if (request_zmq_type_ != ZMQ_PUB)
				{
					poll_items_[cnt++] = { request_scoket_tcp_, 0, ZMQ_POLLIN, 0 };
					poll_items_[cnt++] = { request_socket_ipc_, 0, ZMQ_POLLIN, 0 };
				}
			}
			if (response_zmq_type_ >= 0)
			{
				response_socket_tcp_ = create_res_socket_tcp(get_station_name(), response_zmq_type_, config_->worker_port_);
				if (response_socket_tcp_ == nullptr)
				{
					config_->station_state_ = station_state::Failed;
					log_error2("%s initialize error(inner) %s", get_station_name(), zmq_strerror(zmq_errno()));
					set_command_thread_bad(get_station_name());
					return false;
				}
				if (response_zmq_type_ != ZMQ_PUB)
					poll_items_[cnt] = { response_socket_tcp_, 0, ZMQ_POLLIN, 0 };
			}
			//if (heart_zmq_type_ >= 0)
			//{
			//	heart_socket_tcp_ = create_res_socket_tcp(get_station_name(), heart_zmq_type_, config_->heart_port_);
			//	if (heart_socket_tcp_ == nullptr)
			//	{
			//		config_->station_state_ = station_state::Failed;
			//		log_error2("%s initialize error(heart) %s", get_station_name(), zmq_strerror(zmq_errno()));
			//		set_command_thread_bad(get_station_name());
			//		return false;
			//	}
			//	if (heart_zmq_type_ != ZMQ_PUB)
			//		poll_items_[cnt] = { heart_socket_tcp_, 0, ZMQ_POLLIN, 0 };
			//}
			config_->station_state_ = station_state::Run;
			return true;
		}

		/**
		* \brief 析构
		*/

		bool zero_station::destruct()
		{
			if (poll_items_ == nullptr)
				return true;
			delete[]poll_items_;
			poll_items_ = nullptr;
			config_->station_state_ = station_state::Closing;
			if (request_scoket_tcp_ != nullptr)
			{
				close_res_socket(request_scoket_tcp_, get_out_address().c_str());
			}
			if (response_socket_tcp_ != nullptr)
			{
				close_res_socket(response_socket_tcp_, get_inner_address().c_str());
			}
			if (request_socket_ipc_ != nullptr)
			{
				char address[MAX_PATH];
				if (config::get_global_bool("use_ipc_protocol"))
					sprintf(address, "ipc://%s.ipc", get_station_name());
				else
					sprintf(address, "inproc://%s", get_station_name());
				close_res_socket(request_socket_ipc_, address);
			}
			//登记线程关闭
			set_command_thread_end(get_station_name());
			config_->station_state_ = station_state::Closed;
			return true;
		}
		/**
		* \brief
		* \return
		*/

		bool zero_station::poll()
		{
			config_->log_runing();
			boost::thread(boost::bind(plan_poll, this));
			config_->log("net pool start");
			set_command_thread_start(get_station_name());
			//登记线程开始
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
				if (state == 0)//超时
					continue;
				if (config_->station_state_ == station_state::Pause)
					continue;
				if (state < 0)
				{
					zmq_state_ = check_zmq_error();
					check_zmq_state()
				}
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
						else if (poll_items_[idx].socket == response_socket_tcp_)
						{
							config_->worker_in++;
							response();
						}
						check_zmq_state()
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
						cout << "error";
						//ERROR
					}
				}
			}
			const zmq_socket_state state = zmq_state_;
			config_->log_closing();
			while (in_plan_poll_)
			{
				sleep(10);
			}
			config_->log("net pool end");
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
				boost::this_thread::sleep(boost::posix_time::milliseconds(200));
			return true;
		}

		/**
		* \brief 计划轮询
		*/
		void zero_station::plan_poll_()
		{
			in_plan_poll_ = true;
			config_->log("plan poll start");
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
					command(msg.request_caller.c_str(), msg.messages);
					if (zmq_state_ == zmq_socket_state::Succeed)
					{
						plan_next(msg, false);
					}
				}
			}
			config_->log("plan poll end");
			in_plan_poll_ = false;
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
				messages.push_back(message);
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

			sprintf(id, "%d", message.plan_id);
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
			sprintf(id, "%d", message.plan_id);
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
			if (message.plan_id == 0)
			{
				sprintf(key, "zero:identity:%s", get_station_name());
				message.plan_id = static_cast<uint32_t>(trans_redis::get_context().incr_redis(key)) + 1;
			}
			sprintf(key, "zero:message:%s:%8x", get_station_name(), message.plan_id);
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
			sprintf(id, "%d", message.plan_id);
			//1 删除消息
			sprintf(key, "zero:message:%s:%8x", get_station_name(), message.plan_id);
			redis->del(key);
			//2 删除计划
			if (message.plan_type > plan_date_type::none)
			{
				sprintf(key, "zero:plan:%s", get_station_name());
				redis->zrem(key, id);
			}
			//3 删除参与者
			sprintf(key, "zero:worker:%s:%8x", get_station_name(), message.plan_id);
			acl::string val(128);
			while (redis->spop(key, val))
			{
				sprintf(key, "zero:request:%s:%s", get_station_name(), val.c_str());
				redis->srem(key, id);
			}
			redis->del(key);
			//4 删除返回值
			sprintf(key, "zero:result:%s:%8x", get_station_name(), message.plan_id);
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