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

			/**
			* \brief 工作SOCKET类型
			*/
			int response_zmq_type_;

			/*
			*\brief 轮询节点
			*/
			zmq_pollitem_t* poll_items_;
			/*
			*\brief 节点数量
			*/
			int poll_count_;
			/**
			* \brief 实例队列访问锁
			*/
			boost::mutex send_mutex_;
			/**
			* \brief 实例队列访问锁
			*/
			boost::mutex mutex_;
			/**
			* \brief 站点名称
			*/
			string station_name_;
			/**
			* \brief 站点类型
			*/
			int station_type_;
			/**
			* \brief 任务计划轮询状态
			*/
			bool in_plan_poll_;
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
			ZMQ_HANDLE response_socket_tcp_;
		protected:
			/**
			* \brief 实例队列访问锁
			*/
			static boost::mutex results_mutex_;
			static map<int64, vector<sharp_char>> results;
			/**
			* \brief 当前ZMQ执行状态
			*/
			zmq_socket_state zmq_state_;
			/**
			* \brief 构造
			*/
			zero_station(string name, int type, int request_zmq_type, int response_zmq_type, int heart_zmq_type);

			/**
			* \brief 构造
			*/
			zero_station(shared_ptr<zero_config>& config, int type, int request_zmq_type, int response_zmq_type, int heart_zmq_type);

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
			* \brief 外部地址
			*/
			string get_out_address() const
			{
				return config_->get_out_address();
			}

			/**
			* \brief 工作地址
			*/
			string get_inner_address() const
			{
				return config_->get_inner_address();
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
			* \brief 析构
			*/
			virtual ~zero_station()
			{
				zero_station::close(true);
			}

			/**
			* \brief 能否继续工作
			*/
			virtual bool can_do() const
			{
				return (config_->station_state_ == station_state::Run || config_->station_state_ == station_state::Pause) && get_net_state() ==
					NET_STATE_RUNING;
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
		protected:

			/**
			*\brief 发送消息
			*/
			bool send_response(vector<sharp_char>& datas, size_t first_index = 0)
			{
				boost::lock_guard<boost::mutex> guard(send_mutex_);
				config_->worker_out++;
				zmq_state_ = send(response_socket_tcp_, datas, first_index);

				if (zmq_state_ == zmq_socket_state::Succeed)
					return true;
				config_->worker_err++;
				const char* err_msg = state_str(zmq_state_);
				log_error2("send_response error %d:%s", zmq_state_, err_msg);
				return false;
			}
			/**
			* \brief 发送
			*/
			bool send_request_result(vector<sharp_char>& ls)
			{
				boost::lock_guard<boost::mutex> guard(send_mutex_);
				config_->request_out++;
				void* socket = ls[0][0] == '-' ? request_socket_ipc_ : request_scoket_tcp_;
				zmq_state_ = send(socket, ls);

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
			bool send_request_status(ZMQ_HANDLE socket, const char* addr, char code = ZERO_STATUS_ERROR_ID, const char* global_id = nullptr, const char* reqId = nullptr, const char* msg = nullptr)
			{
				++config_->request_out;
				zmq_state_ = send_status(socket, addr, code, global_id, reqId, msg);
				if (zmq_state_ == zmq_socket_state::Succeed)
					return true;
				++config_->request_err;
				const char* err_msg = state_str(zmq_state_);
				log_error2("send_request_status error %d:%s", zmq_state_, err_msg);
				return false;
			}
			/**
			* \brief 发送帧
			*/
			bool send_request_status(const char* addr, char code = ZERO_STATUS_ERROR_ID, const char* global_id = nullptr, const char* reqId = nullptr, const char* msg = nullptr)
			{
				boost::lock_guard<boost::mutex> guard(send_mutex_);
				++config_->request_out;
				zmq_state_ = send_status(addr[0] == '-' ? request_socket_ipc_ : request_scoket_tcp_, addr, code, global_id, reqId, msg);
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
			* \brief 工作开始（发送到工作者）
			*/
			virtual void job_start(ZMQ_HANDLE socket, vector<sharp_char>& list) = 0;//, sharp_char& global_id
			/**
			* \brief 工作结束(发送到请求者)
			*/
			virtual void job_end(vector<sharp_char>& list)
			{
			}
		private:
			/**
			* \brief 计划轮询
			*/
			static void plan_poll(zero_station* station)
			{
				station->plan_poll_();
			}
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
			zmq_state_ = recv(socket, list);
			if (zmq_state_ != zmq_socket_state::Succeed)
			{
				config_->log(state_str(zmq_state_));
				return;
			}
			const size_t list_size = list.size();
			if (list_size < 3)
			{
				send_request_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID_ID);
				return;
			}
			char* description = *list[2];
			const size_t size = list[2].size();
			const auto frame_size = static_cast<size_t>(description[0]);
			if (frame_size >= size || (frame_size + 3) != list_size || (description[size - 1] != ZERO_FRAME_END && description[size - 1] != ZERO_FRAME_GLOBAL_ID))
			{
				send_request_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID_ID);
				return;
			}

			//const int64 id = station_warehouse::get_glogal_id();
			//sharp_char global_id(16);
			//sprintf(*global_id, "%llx", id);
			if (description[1] == ZERO_COMMAND_GLOBAL_ID)
			{
				sharp_char global_id(32);
				sprintf(*global_id, "%llx", station_warehouse::get_glogal_id());
				send_request_status(socket, *list[0], ZERO_STATUS_OK_ID, *global_id);
			}
			else if (description[1] == ZERO_STATE_CODE_PLAN)
			{
				//job_plan(socket, id, global_id, list);
				job_plan(socket, list);
			}
			else//ZERO_COMMAND_GLOBAL_ID
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
			zmq_state_ = recv(response_socket_tcp_, list);
			if (zmq_state_ == zmq_socket_state::TimedOut)
			{
				config_->worker_err++;
				return;
			}
			if (zmq_state_ != zmq_socket_state::Succeed)
			{
				config_->worker_err++;
				log_error3("接收结果失败%s(%d)%s", get_station_name(), config_->worker_port_, state_str(zmq_state_));
				return;
			}
			job_end(list);
		}

	}
}
#endif//!_ZERO_STATION_H
