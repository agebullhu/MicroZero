#pragma once
#ifndef ZERO_STATION_H
#define ZERO_STATION_H
#include "../stdinc.h"
#include "zmq_extend.h"
#include "zero_plan.h"
#include "station_warehouse.h"


namespace agebull
{
	namespace zero_net
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
			int req_zmq_type_;
			/**
			* \brief 外部SOCKET类型
			*/
			int res_zmq_type_;

			/**
			* \brief 站点类型
			*/
			int station_type_;

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
		protected:
			/**
			* \brief 异步子任务同步等待的信号量
			*/
			boost::interprocess::interprocess_semaphore task_semaphore_;
			/**
			* \brief 站点名称
			*/
			string station_name_;
			/**
			* \brief 站点配置
			*/
			shared_ptr<zero_config> config_;
			/**
			* \brief 远程调用句柄
			*/
			zmq_handler request_scoket_tcp_;
			/**
			* \brief 进程内调用（plan）调用句柄
			*/
			zmq_handler request_socket_inproc_;
			/**
			* \brief 调用句柄
			*/
			//ZMQ_HANDLE request_socket_ipc_;

			/**
			* \brief 写入计划的调用句柄
			*/
			zmq_handler plan_socket_inproc_;

			/**
			* \brief 调用句柄
			*/
			zmq_handler proxy_socket_inproc_;

			/**
			* \brief 工作句柄
			*/
			zmq_handler worker_in_socket_tcp_;
			/**
			* \brief 工作站点调用句柄
			*/
			zmq_handler worker_out_socket_tcp_;
			/**
			* \brief 工作句柄
			*/
			//ZMQ_HANDLE worker_out_socket_ipc_;
			/**
			* \brief 实例队列访问锁
			*/
			//static boost::mutex results_mutex_;
			/**
			* \brief 所有返回值
			*/
			//static map<int64, vector<shared_char>> results;
			/**
			* \brief 当前ZMQ执行状态
			*/
			zmq_socket_state zmq_state_;
		protected:
			/**
			* \brief 构造
			*/
			zero_station(string name, int type, int request_zmq_type, int res_zmq_type);

			/**
			* \brief 构造
			*/
			zero_station(shared_ptr<zero_config>& config, int type, int request_zmq_type, int res_zmq_type);

			/**
			* \brief 析构
			*/
			virtual ~zero_station()
			{
			}

			/**
			* \brief 初始化
			*/
			bool initialize();

			/**
			* \brief 网络轮询
			*/
			virtual bool poll();

			/**
			* \brief 析构
			*/
			virtual bool destruct();

			/**
			* \brief 扩展初始化
			*/
			virtual void initialize_ext() {}
		public:
			/**
			* \brief 暂停
			*/
			bool pause();
			/**
			* \brief 继续
			*/
			bool resume();
			/**
			* \brief 关闭
			*/
			virtual bool close(bool waiting);
		protected:
			/**
			*\brief 发送消息到工作站点
			*/
			inline bool send_response(zmq_handler socket, const  vector<shared_char>& datas, size_t first_index = 0);
			/**
			*\brief 发送消息到工作站点
			*/
			inline bool send_response(const vector<shared_char>& datas, size_t first_index = 0);
			/**
			* \brief 发送请求结果到调用者
			*/
			inline void send_request_status(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner, uchar state);
			/**
			* \brief 发送请求结果到调用者
			*/
			inline bool send_request_result(zmq_handler socket, vector<shared_char>& ls);
			/**
			* \brief 发送请求结果到调用者
			*/
			inline bool send_request_status(zmq_handler socket, const char* addr, uchar state, const char* global_id = nullptr, const char* req_id = nullptr, const char* reqer = nullptr, const char* msg = nullptr);
			/**
			* \brief 发送请求结果到调用者
			*/
			inline bool send_request_status(zmq_handler socket, const char* addr, uchar state, vector<shared_char>& ls, size_t glbid_idx, size_t reqid_idx, size_t reqer_idx, const char* msg = nullptr);
			/**
			* \brief 发送请求结果到调用者
			*/
			inline bool send_request_status(zmq_handler socket, uchar state, vector<shared_char>& ls, size_t glbid_idx, size_t reqid_idx, size_t reqer_idx, const char* msg = nullptr);
		private:
			/**
			* \brief 工作集合的响应
			*/
			void on_response();

			/**
			* \brief 调用集合的响应
			*/
			void on_request(zmq_pollitem_t& socket, bool inner);

			/**
			* \brief 调用集合的响应
			*/
			void on_request(zmq_handler socket, bool inner);

			/**
			* \brief 简单命令
			*/
			bool simple_command(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner);
		protected:
			/**
			* \brief 内部扩展命令
			*/
			virtual bool simple_command_ex(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner)
			{
				return false;
			}
		private:
			/**
			* \brief 请求取得全局标识
			*/
			void global_id_req(zmq_handler socket, vector<shared_char>& list, shared_char& description);
		protected:
			/**
			* \brief 工作开始（发送到工作者）
			*/
			virtual void job_start(zmq_handler socket, vector<shared_char>& list, bool inner) = 0;//, shared_char& global_id
			
			/**
			* \brief 工作结束(发送到请求者)
			*/
			virtual void job_end(vector<shared_char>& list) {}
		private:
			/**
			* \brief 工作进入计划
			*/
			void job_plan(zmq_handler socket, vector<shared_char>& list);
			/**
			* \brief 计划执行完成
			*/
			void plan_end(vector<shared_char>& list);

			/**
			* \brief 反向代理执行完成
			*/
			void proxy_end(vector<shared_char>& list) const;
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
				return config_->station_type;
			}

			/**
			* \brief 当前站点状态
			*/
			station_state get_station_state() const
			{
				return config_->get_state();
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
				return config_->station_name.c_str();
			}

			/**
			* \brief 能否继续工作
			*/
			bool can_do() const
			{
				return config_->is_run() && get_net_state() == zero_def::net_state::runing;
			}

		};

		/**
		* \brief 内部命令
		*/
		inline void zero_station::send_request_status(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner, uchar state)
		{
			const auto caller = list[0];
			if (inner)
				list.erase(list.begin());
			size_t reqid = 0, reqer = 0, glid_index = 0;
			for (size_t i = 2; i <= description.desc_size() && i < list.size(); i++)
			{
				switch (description[i])
				{
				case zero_def::frame::requester:
					reqer = i;
					break;
				case zero_def::frame::request_id:
					reqid = i;
					break;
				case zero_def::frame::global_id:
					glid_index = i;
					break;
				default: ;
				}
			}
			send_request_status(socket, *caller, state, list, glid_index, reqid, reqer);
		}

		/**
		*\brief 发送消息到工作站点
		*/
		inline bool zero_station::send_response(zmq_handler socket, const  vector<shared_char>& datas, const  size_t first_index)
		{
			if (socket == nullptr)
				return false;
			zmq_state_ = socket_ex::send(socket, datas, first_index);
			if (zmq_state_ == zmq_socket_state::succeed)
				return true;
			config_->worker_err++;
			const char* err_msg = socket_ex::state_str(zmq_state_);
			log_error2("send_response error %d:%s", zmq_state_, err_msg);
			return false;
		}
		/**
		*\brief 发送消息到工作站点
		*/
		inline bool zero_station::send_response(const vector<shared_char>& datas, const  size_t first_index)
		{
			if (!config_->hase_ready_works())
			{
				return false;
			}
			boost::lock_guard<boost::mutex> guard(send_mutex_);
			config_->worker_out++;
			send_response(worker_out_socket_tcp_, datas, first_index);
			return zmq_state_ == zmq_socket_state::succeed;
		}

		/**
		* \brief 发送请求结果到调用者
		*/
		inline bool zero_station::send_request_result(zmq_handler socket, vector<shared_char>& ls)
		{
			boost::lock_guard<boost::mutex> guard(send_mutex_);
			config_->request_out++;
			zmq_state_ = socket_ex::send(socket, ls);

			if (zmq_state_ == zmq_socket_state::succeed)
				return true;
			++config_->worker_err;
			const char* err_msg = socket_ex::state_str(zmq_state_);
			log_error2("send_request_result error %d:%s", zmq_state_, err_msg);
			return false;
		}
		/**
		* \brief 发送请求结果到调用者
		*/
		inline bool zero_station::send_request_status(zmq_handler socket, const char* addr, uchar state, const char* global_id, const char* req_id, const char* reqer, const char* msg)
		{
			++config_->request_out;
			zmq_state_ = socket_ex::send_status(socket, addr, state, global_id, req_id, reqer, msg);
			if (zmq_state_ == zmq_socket_state::succeed)
				return true;
			++config_->request_err;
			const char* err_msg = socket_ex::state_str(zmq_state_);
			log_error2("send_request_status error %d:%s", zmq_state_, err_msg);
			return false;
		}
		/**
		* \brief 发送请求结果到调用者
		*/
		inline bool zero_station::send_request_status(zmq_handler socket, const char* addr, uchar state, vector<shared_char>& ls, size_t glbid_idx, size_t reqid_idx, size_t reqer_idx, const char* msg)
		{
			return send_request_status(socket, addr, state,
				glbid_idx == 0 ? nullptr : *ls[glbid_idx],
				reqid_idx == 0 ? nullptr : *ls[reqid_idx],
				reqer_idx == 0 ? nullptr : *ls[reqer_idx],
				msg);
		}
		/**
		* \brief 发送请求结果到调用者
		*/
		inline bool zero_station::send_request_status(zmq_handler socket, uchar state, vector<shared_char>& ls, size_t glbid_idx, size_t reqid_idx, size_t reqer_idx, const char* msg)
		{
			return send_request_status(socket, *ls[0], state,
				glbid_idx == 0 ? nullptr : *ls[glbid_idx],
				reqid_idx == 0 ? nullptr : *ls[reqid_idx],
				reqer_idx == 0 ? nullptr : *ls[reqer_idx],
				msg);
		}
	}

}
#endif//!_ZERO_STATION_H
