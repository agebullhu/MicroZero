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
		protected:
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
			* \brief 配置
			*/
			shared_ptr<zero_config> config_;
			/**
			* \brief 状态信号量
			*/
			boost::interprocess::interprocess_semaphore state_semaphore_;

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
			* \brief 调用句柄
			*/
			void* request_scoket_tcp_;
			/**
			* \brief 调用句柄
			*/
			void* request_socket_ipc_;
			/**
			* \brief 工作句柄
			*/
			void* response_socket_tcp_;
			/**
			* \brief 当前ZMQ执行状态
			*/
			zmq_socket_state zmq_state_;
		public:
			/**
			* \brief 取得配置内容
			*/
			virtual const zero_config& get_config() const
			{
				return *config_;
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
			* \brief 构造
			*/
			zero_station(string name, int type, int request_zmq_type, int response_zmq_type, int heart_zmq_type);

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

			/**
			* \brief 计划轮询
			*/
			static void plan_poll(zero_station* station)
			{
				station->plan_poll_();
			}

		protected:

			/**
			*\brief 发送消息
			*/
			bool send_response(vector<sharp_char>& datas, int first_index = 0)
			{
				config_->worker_out++;
				zmq_state_ = send(response_socket_tcp_, datas, first_index);
				if(zmq_state_ == zmq_socket_state::Succeed)
					return true;
				config_->worker_err++;
				return false;
			}
			/**
			* \brief 发送帧
			*/
			bool send_request_status(ZMQ_HANDLE socket, const char* addr, const char* status)
			{
				++config_->request_out;
				zmq_state_ = send_status(socket, addr, status);
				if (zmq_state_ == zmq_socket_state::Succeed)
					return true;
				++config_->request_err;
				return false;
			}
			/**
			* \brief 发送
			*/
			bool send_request_result(ZMQ_HANDLE socket, vector<sharp_char>& ls, int first_index = 0)
			{
				 config_->request_out++;
				 zmq_state_ = send(socket, ls, first_index);
				 if (zmq_state_ == zmq_socket_state::Succeed)
					 return true;
				 ++config_->request_err;
				 return false;
			}
			/**
			* \brief 保存计划
			*/
			void save_plan(ZMQ_HANDLE socket, vector<sharp_char> list) const
			{
				plan_message message;
				message.request_caller = list[0];
				for (size_t idx = 3; idx < list.size(); idx++)
				{
					message.messages.push_back(list[idx]);
				}
				message.read_plan(list[0].get_buffer());
				plan_next(message, true);
			}

			/**
			* \brief 计划轮询
			*/
			void plan_poll_();

			/**
			* \brief 工作集合的响应
			*/
			virtual void response()
			{
			}

			/**
			* \brief 调用集合的响应
			*/
			virtual void request(ZMQ_HANDLE socket, bool inner) = 0;
		};
	}
}
#endif//!_ZERO_STATION_H
