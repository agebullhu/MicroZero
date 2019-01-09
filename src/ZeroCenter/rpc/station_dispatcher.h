#ifndef ZMQ_API_NET_DISPATCHER_H
#define ZMQ_API_NET_DISPATCHER_H
#pragma once
#include "../stdinc.h"
#include "zero_station.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		 * \brief 表示站点调度服务
		 */
		class station_dispatcher :public zero_station
		{
		public:
			/**
			* \brief 单例
			*/
			static station_dispatcher* instance;
			/**
			* \brief 构造
			*/
			station_dispatcher()
				:zero_station(zero_def::name::system_manage, zero_def::station_type::dispatcher, ZMQ_ROUTER, ZMQ_PUB)
			{

			}
			/**
			* \brief 构造
			*/
			station_dispatcher(shared_ptr<zero_config>& config)
				:zero_station(config, zero_def::station_type::dispatcher, ZMQ_ROUTER, ZMQ_PUB)
			{

			}
			/**
			* \brief 析构
			*/
			~station_dispatcher() override
			{
				cout << "station_dispatcher destory" << endl;
			}

			/**
			* \brief 运行一个通知线程
			*/
			static void run()
			{
				instance = new station_dispatcher();
				boost::thread(boost::bind(launch, shared_ptr<station_dispatcher>(instance)));
			}
			/**
			*\brief 运行
			*/
			static void run(shared_ptr<zero_config>& config)
			{
				instance = new station_dispatcher(config);
				boost::thread(boost::bind(launch, shared_ptr<station_dispatcher>(instance)));
			}
			/**
			* \brief 消息泵
			*/
			static void launch(shared_ptr<station_dispatcher>& station);
		private:
			/**
			* \brief 能否继续工作
			*/
			bool can_do() const final
			{
				return config_->is_run() && get_net_state() <= zero_def::net_state::closing;
			}
			/**
			* \brief 监控轮询
			*/
			static void worker_monitor();

			/**
			* \brief 工作开始 : 处理请求数据
			*/
			void job_start(zmq_handler socket, vector<shared_char>& list, bool inner, bool old) final;
		public:
			/**
			*\brief 通知内容
			*/
			static bool publish_event(zero_net_event event_name, const char* title, const char* sub, const char* content);
		private:
			/**
			*\brief 发布消息
			*/
			bool publish(const string& title, const string& publiher, const string& arg);
		public:
			/**
			* \brief 执行命令
			*/
			void restart();

		public:
			/**
			* \brief 执行命令
			*/
			char exec_command(const char* command, vector<shared_char>& arguments, string& json) const;

		private:
			/**
			* \brief 收到PING
			*/
			void ping(zmq_handler socket, vector<shared_char>& list) override;

			/**
			* 心跳的响应
			* */
			bool heartbeat(zmq_handler socket, uchar cmd, vector<shared_char> list) override;
		};
	}
}
#endif//!ZMQ_API_NET_DISPATCHER_H