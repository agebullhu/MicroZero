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
		protected:
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
			* \brief 写入跟踪的调用句柄
			*/
			zmq_handler trace_socket_inproc_;

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
				cout << "zero_station destory" << endl;
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
			* \brief 析构
			*/
			virtual void destruct_ext() {}

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
			inline zmq_socket_state send_response(zmq_handler socket, vector<shared_char>& datas, bool do_trace);
			/**
			*\brief 发送消息到工作站点
			*/
			inline zmq_socket_state send_response(vector<shared_char>& datas, bool do_trace);
			/**
			* \brief 发送请求结果到调用者
			*/
			inline void send_request_status_by_trace(zmq_handler socket, vector<shared_char>& list, shared_char& description, uchar state);
			/**
			* \brief 发送请求结果到调用者
			*/
			inline bool send_request_result(zmq_handler socket, vector<shared_char>& list, bool trace);
			/**
			* \brief 发送请求结果到调用者
			*/
			inline bool send_request_status(zmq_handler socket, const char* addr, uchar state, bool do_trace);
			/**
			* \brief 发送请求结果到调用者
			*/
			inline bool send_request_status(zmq_handler socket, const char* addr, uchar state, vector<shared_char>& ls, size_t glbid_idx, size_t reqid_idx, size_t reqer_idx, const char* msg = nullptr);
			/**
			* \brief 发送请求结果到调用者
			*/
			inline bool send_request_status_by_trace(zmq_handler socket, const char* addr, uchar state, vector<shared_char>& ls, size_t glbid_idx, size_t reqid_idx, size_t reqer_idx, const char* msg = nullptr);
		private:

			/**
			* \brief 调用集合的响应
			*/
			void on_response(zmq_pollitem_t& socket);
			/**
			* \brief 调用集合的响应
			*/
			void on_request(zmq_pollitem_t& socket, bool inner);
		protected:

			/**
			* \brief 简单命令
			*/
			virtual bool simple_command(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner);
		private:
			/**
			* \brief 请求取得全局标识
			*/
			void global_id_req(zmq_handler socket, vector<shared_char>& list, shared_char& description);
		protected:
			/**
			* \brief 工作开始 : 处理请求数据
			*/
			virtual void job_start(zmq_handler socket, vector<shared_char>& list, bool inner, bool old) = 0;

			/**
			* \brief 工作结束(发送到请求者)
			*/
			virtual void job_end(vector<shared_char>& list) {}

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
			void proxy_end(vector<shared_char>& list);
			/**
			* \brief 收到PING
			*/
			virtual void ping(zmq_handler socket, vector<shared_char>& list) {}

			/**
			* 心跳的响应
			* */
			virtual bool heartbeat(zmq_handler socket, uchar cmd, vector<shared_char> list) { return false; }

			/**
			* \brief 网络数据跟踪
			* \param io [in] 1 请求数据(from client) 2 下发数据(to worker) 3 返回数据(worker to client)
			* \param list 帧数据
			* \param err_msg 发生的错误描述
			*/
			virtual void trace(uchar io, vector<shared_char> list, const char* err_msg);

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
		  	virtual bool can_do() const
			{
				return config_->is_run() && get_net_state() == zero_def::net_state::runing;
			}
		};

		/**
		*\brief 发送消息到工作站点
		*/
		inline zmq_socket_state zero_station::send_response(vector<shared_char>& datas, bool do_trace)
		{
			return send_response(worker_out_socket_tcp_, datas, do_trace);
		}

		/**
		*\brief 发送消息到工作站点
		*/
		inline zmq_socket_state zero_station::send_response(zmq_handler socket, vector<shared_char>& datas, bool do_trace)
		{
			if (socket == nullptr)
				return zmq_socket_state::unknow;
			config_->worker_out++;

			zmq_socket_state state;
			{
				boost::lock_guard<boost::mutex> guard(send_mutex_);
				state = socket_ex::send(socket, datas, 0);
			}
			if (state == zmq_socket_state::succeed)
			{
				if (do_trace)
					trace(2, datas, nullptr);
				return state;
			}
			config_->worker_err++;
			const char* err_msg = socket_ex::state_str(state);
			config_->error("send_response", err_msg);
			if (do_trace)
				trace(2, datas, err_msg);
			return state;
		}

		/**
		* \brief 发送请求结果到调用者
		*/
		inline bool zero_station::send_request_status(zmq_handler socket, const char* addr, uchar state, bool do_trace)
		{
			vector<shared_char> ls;
			ls.push_back(addr);
			shared_char descirpt;
			descirpt.alloc_desc(8, state);
			ls.push_back(descirpt);
			descirpt.tag(zero_def::frame::result_end);
			return send_request_result(socket, ls, do_trace);
		}

		/**
		* \brief 内部命令
		*/
		inline void zero_station::send_request_status_by_trace(zmq_handler socket, vector<shared_char>& list, shared_char& description, uchar state)
		{
			vector<shared_char> ls;
			ls.push_back(list[0]);
			shared_char descirpt;
			descirpt.alloc_desc(8, state);
			ls.push_back(descirpt);
			for (size_t i = 2; i <= description.desc_size() && i <= description.alloc_size() && i < list.size(); i++)
			{
				switch (description[i])
				{
				case zero_def::frame::requester:
					descirpt.append_frame(zero_def::frame::requester);
					ls.push_back(list[i]);
					break;
				case zero_def::frame::request_id:
					descirpt.append_frame(zero_def::frame::request_id);
					ls.push_back(list[i]);
					break;
				case zero_def::frame::global_id:
					descirpt.append_frame(zero_def::frame::global_id);
					ls.push_back(list[i]);
					break;
				}
			}
			descirpt.tag(zero_def::frame::result_end);
			send_request_result(socket, ls, true);
		}
		/**
		* \brief 发送请求结果到调用者
		*/
		inline bool zero_station::send_request_status(zmq_handler socket, const char* addr, uchar state, vector<shared_char>& list,
			size_t glbid_idx, size_t reqid_idx, size_t reqer_idx, const char* msg)
		{
			vector<shared_char> ls;
			ls.push_back(addr);
			shared_char descirpt;
			descirpt.alloc_desc(8, state);
			ls.push_back(descirpt);
			if (reqid_idx > 0)
			{
				descirpt.append_frame(zero_def::frame::requester);
				ls.push_back(vector_str(list, reqid_idx));
			}
			if (reqer_idx > 0)
			{
				descirpt.append_frame(zero_def::frame::request_id);
				ls.push_back(vector_str(list, reqer_idx));
			}
			if (glbid_idx > 0)
			{
				descirpt.append_frame(zero_def::frame::global_id);
				ls.push_back(vector_str(list, glbid_idx));
			}
			if (msg != nullptr)
			{
				descirpt.append_frame(zero_def::frame::status);
				ls.push_back(msg);
			}
			descirpt.tag(zero_def::frame::result_end);
			return send_request_result(socket, ls, false);
		}
		/**
		* \brief 发送请求结果到调用者
		*/
		inline bool zero_station::send_request_status_by_trace(zmq_handler socket, const char* addr, uchar state,
			vector<shared_char>& list, size_t glbid_idx, size_t reqid_idx, size_t reqer_idx, const char* msg)
		{
			vector<shared_char> ls;
			ls.push_back(addr);
			shared_char descirpt;
			descirpt.alloc_desc(8, state);
			ls.push_back(descirpt);
			if (reqid_idx > 0)
			{
				descirpt.append_frame(zero_def::frame::requester);
				ls.push_back(vector_str(list, reqid_idx));
			}
			if (reqer_idx > 0)
			{
				descirpt.append_frame(zero_def::frame::request_id);
				ls.push_back(vector_str(list, reqer_idx));
			}
			if (glbid_idx > 0)
			{
				descirpt.append_frame(zero_def::frame::global_id);
				ls.push_back(vector_str(list, glbid_idx));
			}
			if (msg != nullptr)
			{
				descirpt.append_frame(zero_def::frame::status);
				ls.push_back(msg);
			}
			descirpt.tag(zero_def::frame::result_end);
			return send_request_result(socket, ls, true);
		}


		/**
		* \brief 发送请求结果到调用者
		*/
		inline bool zero_station::send_request_result(zmq_handler socket, vector<shared_char>& ls, bool do_trace)
		{
			zmq_socket_state state;
			{
				boost::lock_guard<boost::mutex> guard(send_mutex_);
				state = socket_ex::send(socket, ls);
			}
			config_->request_out++;
			if (state == zmq_socket_state::succeed)
			{
				if (do_trace)
					trace(3, ls, nullptr);
				return true;
			}
			++config_->request_err;
			const char* err_msg = socket_ex::state_str(state);
			config_->error("send_request_result", err_msg, *ls[0]);
			if (do_trace)
				trace(3, ls, err_msg);
			return false;
		}
	}

}
#endif//!_ZERO_STATION_H
