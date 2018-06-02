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
			void job_start(ZMQ_HANDLE socket, vector<sharp_char>& list) final;//, sharp_char& global_id
			/**
			*\brief 发布消息
			*/
			bool publish(const string& title, const string& publiher, const string& arg);
			/**
			* 心跳的响应
			*/
			static bool heartbeat(char cmd, vector<sharp_char> lines);
		public:
			/**
			* \brief 执行一条命令
			*/
			sharp_char command(const char* caller, vector<sharp_char> lines) final;
			/**
			* \brief 取机器信息
			*/
			static char host_info(const string& stattion, string& json);
			/**
			* \brief 站点安装
			*/
			static char install_station(const char* type_name, const char* stattion, const char* short_name);
			/**
			* \brief 站点卸载
			*/
			static bool uninstall(const string& stattion);
			/**
			*  \brief 启动站点
			*/
			static char start_station(string stattion);
			/**
			* \brief 暂停站点
			*/
			static char pause_station(const string& stattion);
			/**
			* \brief 继续站点
			*/
			static char resume_station(const string& stattion);
			/**
			* \brief 关闭站点
			*/
			static char close_station(const string& stattion);
			/**
			* \brief 远程调用
			*/
			static string call_station(const string& stattion, const string& command, const string& argument);
			/**
			* \brief 远程调用
			*/
			static string call_station(const char* stattion, vector<sharp_char>& arguments);
			/**
			* \brief 执行命令
			*/
			static char exec_command(const char* command, vector<sharp_char>& arguments, string& json);
			/**
			* \brief 执行命令
			*/
			static string exec_command(const char* command, const char* argument);
			/**
			*\brief 广播内容
			*/
			static bool monitor(const string& publiher, const string& event_name, const string& content)
			{
				if (instance != nullptr)
					return instance->publish(event_name, publiher, content);
				return false;
			}
			/**
			*\brief 广播内容
			*/
			static bool publish(const string& station, const string& publiher, const string& title, const string& sub, const string& arg);
		};
	}
}
#endif//!ZMQ_API_NET_DISPATCHER_H