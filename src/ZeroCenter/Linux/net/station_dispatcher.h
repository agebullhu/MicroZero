#ifndef ZMQ_API_NET_DISPATCHER_H
#define ZMQ_API_NET_DISPATCHER_H
#pragma once
#include "../stdinc.h"
#include "zero_station.h"

namespace agebull
{
	namespace zmq_net
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
				:zero_station("SystemManage", STATION_TYPE_DISPATCHER, ZMQ_ROUTER)
			{

			}
			/**
			* \brief 构造
			*/
			station_dispatcher(shared_ptr<zero_config>& config)
				:zero_station(config, STATION_TYPE_DISPATCHER, ZMQ_ROUTER)
			{

			}
			/**
			* \brief 析构
			*/
			~station_dispatcher() final = default;

			/**
			* \brief 运行一个广播线程
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
			static void monitor_poll();

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
			void job_start(ZMQ_HANDLE socket, vector<shared_char>& list, bool inner) final;
			/**
			*\brief 发布消息
			*/
			bool publish(const string& title, const string& publiher, const string& arg);
		public:
			/**
			* \brief 执行一条命令
			*/
			shared_char command(const char* caller, vector<shared_char> lines) final;

			/**
			* \brief 远程调用
			*/
			string call_station(const string& stattion, const string& command, const string& argument);
			/**
			* \brief 远程调用
			*/
			string call_station(const char* stattion, vector<shared_char>& arguments);
			/**
			* \brief 执行命令
			*/
			char exec_command(const char* command, vector<shared_char>& arguments, string& json);
			/**
			* \brief 执行命令
			*/
			string exec_command(const char* command, const char* argument);
			/**
			*\brief 广播内容
			*/
			static bool zero_event(const string publiher, const zero_net_event event_name, const string content);
			/**
			*\brief 广播内容
			*/
			static bool publish(const string& station, const string& publiher, const string& title, const string& sub, const string& arg);
		};
	}
}
#endif//!ZMQ_API_NET_DISPATCHER_H