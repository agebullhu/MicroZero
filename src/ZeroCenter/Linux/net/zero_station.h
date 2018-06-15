#pragma once
#ifndef _ZERO_STATION_H
#define _ZERO_STATION_H
#include "../stdinc.h"
#include "zmq_extend.h"
#include "zero_plan.h"
#include "station_warehouse.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 表示一个基于ZMQ的网络站点
		*/
		class zero_station
		{
			friend class station_warehouse;
			/**
			* \brief 外部SOCKET类型
			*/
			int request_zmq_type_;

			/*
			*\brief 轮询节点
			*/
			zmq_pollitem_t* poll_items_;
			/*
			*\brief 节点数量
			*/
			int poll_count_;
		protected:
			/**
			* \brief 子任务同步结束使用的信号量
			*/
			boost::interprocess::interprocess_semaphore task_semaphore_;
		private:
			/**
			* \brief 实例队列访问锁
			*/
			boost::mutex send_mutex_;
			/**
			* \brief 实例队列访问锁
			*/
			boost::mutex mutex_;
		protected:
			/**
			* \brief 站点名称
			*/
			string station_name_;
		private:
			/**
			* \brief 站点类型
			*/
			int station_type_;
			/**
			* \brief 状态信号量
			*/
			boost::interprocess::interprocess_semaphore state_semaphore_;
			/**
			* \brief 配置
			*/
			shared_ptr<zero_config> config_;

			/**
			* \brief 调用句柄
			*/
			ZMQ_HANDLE request_scoket_tcp_;
			/**
			* \brief 调用句柄
			*/
			ZMQ_HANDLE request_socket_ipc_;
			/**
			* \brief 工作句柄
			*/
			ZMQ_HANDLE worker_in_socket_tcp_;
			/**
			* \brief 工作句柄
			*/
			ZMQ_HANDLE worker_out_socket_tcp_;
			/**
			* \brief 工作句柄
			*/
			ZMQ_HANDLE worker_out_socket_ipc_;
		protected:
			/**
			* \brief 实例队列访问锁
			*/
			//static boost::mutex results_mutex_;
			/**
			* \brief 所有返回值
			*/
			//static map<int64, vector<sharp_char>> results;
			/**
			* \brief 当前ZMQ执行状态
			*/
			zmq_socket_state zmq_state_;
			/**
			* \brief 构造
			*/
			zero_station(string name, int type, int request_zmq_type);

			/**
			* \brief 构造
			*/
			zero_station(shared_ptr<zero_config>& config, int type, int request_zmq_type);

			/**
			* \brief 析构
			*/
			virtual ~zero_station()
			{
				zero_station::close(true);
			}

		public:
			/**
			* \brief 取得配置内容
			*/
			zero_config& get_config() const
			{
				return *config_;
			}

			/**
			* \brief 取得配置内容
			*/
			shared_ptr<zero_config>& get_config_ptr()
			{
				return config_;
			}
			/**
			* \brief 当前ZMQ执行状态
			*/
			zmq_socket_state get_zmq_state() const
			{
				return zmq_state_;
			}

			/**
			* \brief API服务名称
			*/
			int get_station_type() const
			{
				return config_->station_type_;
			}

			/**
			* \brief 当前站点状态
			*/
			station_state get_station_state() const
			{
				return config_->station_state_;
			}

			/**
			* \brief 当前站点状态
			*/
			void set_station_state(station_state state) const
			{
				config_->station_state_ = state;
			}

			/**
			* \brief API服务名称
			*/
			const char* get_station_name() const
			{
				return config_->station_name_.c_str();
			}

			/**
			* \brief 载入现在到期的内容
			*/
			void load_now(vector<plan_message>& messages) const;

			/**
			* \brief 删除一个计划
			*/
			bool remove_next(plan_message& message) const;

			/**
			* \brief 计划下一次执行时间
			*/
			bool plan_next(plan_message& message, bool first) const;

			/**
			* \brief 保存下一次执行时间
			*/
			bool save_next(plan_message& message) const;

			/**
			* \brief 保存消息
			*/
			bool save_message(plan_message& message) const;

			/**
			* \brief 读取消息
			*/
			bool load_message(uint id, plan_message& message) const;

			/**
			* \brief 删除消息
			*/
			bool remove_message(plan_message& message) const;

			/**
			* \brief 保存消息参与者
			*/
			bool save_message_worker(uint msgid, vector<const char*>& workers) const;

			/**
			* \brief 保存消息参与者返回值
			*/
			bool save_message_result(uint msgid, const string& worker, const string& response) const;

			/**
			* \brief 取一个参与者的消息返回值
			*/
			acl::string get_message_result(uint msgid, const char* worker) const;

			/**
			* \brief 取全部参与者消息返回值
			*/
			map<acl::string, acl::string> get_message_result(uint msgid) const;

			/**
			* \brief 执行一条命令
			*/
			virtual sharp_char command(const char* caller, vector<sharp_char> lines) = 0;
		public:
			/**
			* \brief 能否继续工作
			*/
			bool can_do() const
			{
				return (config_->station_state_ == station_state::Run ||
					config_->station_state_ == station_state::Pause) &&
					get_net_state() == NET_STATE_RUNING;
			}

			/**
			* \brief 检查是否暂停
			*/
			bool check_pause()
			{
				if (config_->station_state_ == station_state::Pause)
				{
					state_semaphore_.timed_wait(time_span(1000));
					return config_->station_state_ == station_state::Pause;
				}
				return false;
			}

			/**
			* \brief 初始化
			*/
			bool initialize();

			/**
			* \brief 网络轮询
			*/
			bool poll();

			/**
			* \brief 析构
			*/
			bool destruct();

			/**
			* \brief 暂停
			*/
			virtual bool pause(bool waiting);
			/**
			* \brief 继续
			*/
			virtual bool resume(bool waiting);
			/**
			* \brief 结束
			*/
			virtual bool close(bool waiting);
		private:
			/**
			*\brief 发送消息
			*/
			bool send_response(ZMQ_HANDLE socket, const  vector<sharp_char>& datas, const  size_t first_index = 0)
			{
				if (socket == nullptr)
					return false;
				zmq_state_ = socket_ex::send(socket, datas, first_index);
				if (zmq_state_ == zmq_socket_state::Succeed)
					return true;
				config_->worker_err++;
				const char* err_msg = state_str(zmq_state_);
				log_error2("send_response error %d:%s", zmq_state_, err_msg);
				return false;
			}
		protected:
			/**
			*\brief 发送消息
			*/
			bool send_response(const vector<sharp_char>& datas, const  size_t first_index = 0)
			{
				if (!config_->hase_ready_works())
				{
					return false;
				}
				boost::lock_guard<boost::mutex> guard(send_mutex_);
				config_->worker_out++;
				ZMQ_HANDLE socket[2] = { worker_out_socket_tcp_ ,worker_out_socket_ipc_ };
#pragma omp parallel  for schedule(static,2)
				for (size_t i = 0; i < 2; i++)
				{
					send_response(socket[i], datas, first_index);
				}
				return zmq_state_ == zmq_socket_state::Succeed;
			}
			/**
			* \brief 发送
			*/
			bool send_request_result(vector<sharp_char>& ls)
			{
				boost::lock_guard<boost::mutex> guard(send_mutex_);

				config_->request_out++;
				void* socket = ls[0][0] == '-' ? request_socket_ipc_ : request_scoket_tcp_;
				zmq_state_ = socket_ex::send(socket, ls);

				if (zmq_state_ == zmq_socket_state::Succeed)
					return true;
				++config_->worker_err;
				const char* err_msg = state_str(zmq_state_);
				log_error2("send_request_result error %d:%s", zmq_state_, err_msg);
				return false;
			}

			/**
			* \brief 发送帧
			*/
			bool send_request_status(ZMQ_HANDLE socket, const char* addr, char code = ZERO_STATUS_ERROR_ID, const char* global_id = nullptr, const char* req_id = nullptr, const char* reqer = nullptr, const char* msg = nullptr)
			{
				++config_->request_out;
				zmq_state_ = socket_ex::send_status(socket, addr, code, global_id, req_id, reqer, msg);
				if (zmq_state_ == zmq_socket_state::Succeed)
					return true;
				++config_->request_err;
				const char* err_msg = state_str(zmq_state_);
				log_error2("send_request_status error %d:%s", zmq_state_, err_msg);
				return false;
			}
		private:
			/**
			* \brief 工作集合的响应
			*/
			void response();

			/**
			* \brief 调用集合的响应
			*/
			void request(ZMQ_HANDLE socket, bool inner);

		protected:
			/**
			* \brief 计划轮询
			*/
			static void monitor_poll(zero_station* station)
			{
				station->monitor();
			}
		private:
			/**
			* \brief 网络监控
			* \return
			*/
			void monitor();
		protected:
			/**
			* \brief 工作开始（发送到工作者）
			*/
			virtual void job_start(ZMQ_HANDLE socket, vector<sharp_char>& list) = 0;//, sharp_char& global_id
			/**
			* \brief 工作结束(发送到请求者)
			*/
			virtual void job_end(vector<sharp_char>& list)
			{
			}
			/**
			* \brief 计划轮询
			*/
			static void plan_poll(zero_station* station)
			{
				station->plan_poll_();
			}
		private:
			/**
			* \brief 保存计划
			*/
			void save_plan(ZMQ_HANDLE socket, vector<sharp_char> list) const;

			/**
			* \brief 计划轮询
			*/
			void plan_poll_();

			/**
			* \brief 工作进入计划
			*/
			void job_plan(ZMQ_HANDLE socket, vector<sharp_char>& list);//, const int64 id, sharp_char& global_id
		};

		/**
		* \brief 调用集合的响应
		*/
		inline void zero_station::request(ZMQ_HANDLE socket, bool inner)
		{
			vector<sharp_char> list;
			zmq_state_ = socket_ex::recv(socket, list);
			if (zmq_state_ != zmq_socket_state::Succeed)
			{
				config_->log(state_str(zmq_state_));
				return;
			}
			const size_t list_size = list.size();
			if (list_size < 2 || list[1].size() < 2)
			{
				send_request_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID_ID);
				return;
			}
			char* description = list[1].get_buffer();
			const size_t size = list[1].size();
			const auto frame_size = static_cast<size_t>(description[0]);
			if (frame_size >= size || (frame_size + 2) != list_size || (description[size - 1] != ZERO_FRAME_END && description[size - 1] != ZERO_FRAME_GLOBAL_ID))
			{
				send_request_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID_ID);
				return;
			}
			//const int64 id = station_warehouse::get_glogal_id();
			//sharp_char global_id(16);
			//sprintf(*global_id, "%llx", id);
			if (description[1] == ZERO_BYTE_COMMAND_GLOBAL_ID)
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

				send_request_status(socket, *list[0], ZERO_STATUS_OK_ID, global_id, reqid == 0 ? nullptr : *list[reqid], reqer == 0 ? nullptr : *list[reqer]);
			}
			else if (description[1] == ZERO_STATE_CODE_PLAN)
			{
				//job_plan(socket, id, global_id, list);
				job_plan(socket, list);
			}
			else//ZERO_BYTE_COMMAND_GLOBAL_ID
			{
				//job_start(socket, global_id, list);
				job_start(socket, list);
			}
		}

		/**
		* \brief 工作集合的响应
		*/
		inline void zero_station::response()
		{
			vector<sharp_char> list;
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
			job_end(list);
		}

	}
}
#endif//!_ZERO_STATION_H
