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
				:zero_station(DEF_SYSTEM_MANAGE, station_type_dispatcher, ZMQ_ROUTER, ZMQ_PUB)
			{

			}
			/**
			* \brief 构造
			*/
			station_dispatcher(shared_ptr<zero_config>& config)
				:zero_station(config, station_type_dispatcher, ZMQ_ROUTER, ZMQ_PUB)
			{

			}
			/**
			* \brief 析构
			*/
			~station_dispatcher() final = default;

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
			* \brief 监控轮询
			*/
			static void worker_monitor();

			/**
			* \brief 暂停
			*/
			bool pause(bool waiting) final
			{
				return false;
			}

			/**
			* \brief 继续
			*/
			bool resume(bool waiting)final
			{
				return false;
			}

			/**
			* \brief 结束
			*/
			bool close(bool waiting)final
			{
				return false;
			}
			/**
			* \brief 工作开始（发送到工作者）
			*/
			void job_start(zmq_handler socket, vector<shared_char>& list, bool inner) final;
			/**
			*\brief 发布消息
			*/
			bool publish(const string& title, const string& publiher, const string& arg);
		public:
			/**
			* \brief 执行命令
			*/
			char exec_command(const char* command, vector<shared_char>& arguments, string& json) const;
			
			/**
			*\brief 通知内容
			*/
			static bool publish_event(zero_net_event event_name, const char* title, const char* sub, const char* content);
		private:
			/**
			* \brief 内部命令
			*/
			bool extend_command(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner) override;
		};
	}
}
#endif//!ZMQ_API_NET_DISPATCHER_H