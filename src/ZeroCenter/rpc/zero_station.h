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
			* \brief 配置
			*/
			shared_ptr<zero_config> config_;

		protected:
			/**
			* \brief 调用句柄
			*/
			ZMQ_HANDLE request_scoket_tcp_;
			/**
			* \brief 调用句柄
			*/
			ZMQ_HANDLE request_socket_ipc_;
		private:
			/**
			* \brief 调用句柄
			*/
			ZMQ_HANDLE request_socket_inproc_;

			/**
			* \brief 调用句柄
			*/
			ZMQ_HANDLE plan_socket_inproc_;

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
			//static map<int64, vector<shared_char>> results;
			/**
			* \brief 当前ZMQ执行状态
			*/
			zmq_socket_state zmq_state_;
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
				return config_->station_name_.c_str();
			}

			/**
			* \brief 能否继续工作
			*/
			bool can_do() const
			{
				return config_->is_run() && get_net_state() == NET_STATE_RUNING;
			}

		protected:
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
		public:
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
			bool send_response(ZMQ_HANDLE socket, const  vector<shared_char>& datas, const  size_t first_index = 0)
			{
				if (socket == nullptr)
					return false;
				zmq_state_ = socket_ex::send(socket, datas, first_index);
				if (zmq_state_ == zmq_socket_state::Succeed)
					return true;
				config_->worker_err++;
				const char* err_msg = socket_ex::state_str(zmq_state_);
				log_error2("send_response error %d:%s", zmq_state_, err_msg);
				return false;
			}
		protected:
			/**
			*\brief 发送消息
			*/
			bool send_response(const vector<shared_char>& datas, const  size_t first_index = 0)
			{
				if (!config_->hase_ready_works())
				{
					return false;
				}
				boost::lock_guard<boost::mutex> guard(send_mutex_);
				config_->worker_out++;
				ZMQ_HANDLE socket[2] = { worker_out_socket_tcp_ ,worker_out_socket_ipc_ };
				//#pragma omp parallel  for schedule(static,2)
				for (size_t i = 0; i < 2; i++)
				{
					send_response(socket[i], datas, first_index);
				}
				return zmq_state_ == zmq_socket_state::Succeed;
			}

			/**
			* \brief 发送
			*/
			bool send_request_result(ZMQ_HANDLE socket, vector<shared_char>& ls)
			{
				boost::lock_guard<boost::mutex> guard(send_mutex_);
				config_->request_out++;
				zmq_state_ = socket_ex::send(socket, ls);

				if (zmq_state_ == zmq_socket_state::Succeed)
					return true;
				++config_->worker_err;
				const char* err_msg = socket_ex::state_str(zmq_state_);
				log_error2("send_request_result error %d:%s", zmq_state_, err_msg);
				return false;
			}
			/**
			* \brief 发送帧
			*/
			bool send_request_status(ZMQ_HANDLE socket, const char* addr, uchar state, const char* global_id = nullptr, const char* req_id = nullptr, const char* reqer = nullptr, const char* msg = nullptr)
			{
				++config_->request_out;
				zmq_state_ = socket_ex::send_status(socket, addr, state, global_id, req_id, reqer, msg);
				if (zmq_state_ == zmq_socket_state::Succeed)
					return true;
				++config_->request_err;
				const char* err_msg = socket_ex::state_str(zmq_state_);
				log_error2("send_request_status error %d:%s", zmq_state_, err_msg);
				return false;
			}
			/**
			* \brief 发送帧
			*/
			bool send_request_status(ZMQ_HANDLE socket, const char* addr, uchar state, vector<shared_char>& ls, size_t glbid_idx, size_t reqid_idx, size_t reqer_idx, const char* msg = nullptr)
			{
				return send_request_status(socket, addr, state,
					glbid_idx == 0 ? nullptr : *ls[glbid_idx],
					reqid_idx == 0 ? nullptr : *ls[reqid_idx],
					reqer_idx == 0 ? nullptr : *ls[reqer_idx],
					msg);
			}
			/**
			* \brief 发送帧
			*/
			bool send_request_status(ZMQ_HANDLE socket, uchar state, vector<shared_char>& ls, size_t glbid_idx, size_t reqid_idx, size_t reqer_idx, const char* msg = nullptr)
			{
				return send_request_status(socket, *ls[0], state,
					glbid_idx == 0 ? nullptr : *ls[glbid_idx],
					reqid_idx == 0 ? nullptr : *ls[reqid_idx],
					reqer_idx == 0 ? nullptr : *ls[reqer_idx],
					msg);
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
			* \brief 网络监控
			*/
			static void monitor_poll(zero_station* station)
			{
				station->monitor();
			}
			/**
			* \brief 网络监控
			* \return
			*/
			void monitor() const;
			/**
			* \brief 工作开始（发送到工作者）
			*/
			virtual void job_start(ZMQ_HANDLE socket, vector<shared_char>& list, bool inner) = 0;//, shared_char& global_id
			/**
			* \brief 工作结束(发送到请求者)
			*/
			virtual void job_end(vector<shared_char>& list)
			{
			}
		private:
			/**
			* \brief 工作进入计划
			*/
			void job_plan(ZMQ_HANDLE socket, vector<shared_char>& list);
			/**
			* \brief 计划执行完成
			*/
			void plan_end(vector<shared_char>& list);

		};
	}
}
#endif//!_ZERO_STATION_H
