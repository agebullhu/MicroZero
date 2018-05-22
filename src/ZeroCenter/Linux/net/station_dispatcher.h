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
				:zero_station("SystemManage", STATION_TYPE_DISPATCHER, ZMQ_ROUTER, ZMQ_PUB, -1)
			{

			}
			/**
			* \brief 析构
			*/
			~station_dispatcher() override = default;

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
				instance = new station_dispatcher();
				instance->config_ = config;
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
			bool pause(bool waiting) override
			{
				return false;
			}

			/**
			* \brief 继续
			*/
			bool resume(bool waiting)override
			{
				return false;
			}

			/**
			* \brief 结束
			*/
			bool close(bool waiting)override
			{
				return false;
			}
			/**
			* \brief 处理请求
			*/
			void request(ZMQ_HANDLE socket,bool inner)override;
			/**
			*\brief 发布消息
			*/
			bool publish(const string& title, const string& publiher, const string& arg);
			/**
			* 心跳的响应
			*/
			static bool heartbeat(vector<sharp_char> lines);
		public:
			/**
			* \brief 执行一条命令
			*/
			sharp_char command(const char* caller, vector<sharp_char> lines) override;
			/**
			* \brief 站点安装
			*/
			static string install_station(const string& type_name, const string& stattion);
			/**
			* \brief 站点卸载
			*/
			static bool uninstall(const string& stattion);
			/**
			* \brief 取机器信息
			*/
			static string host_info(const string& stattion);
			/**
			*  \brief 启动站点
			*/
			static string start_station(string stattion);
			/**
			* \brief 暂停站点
			*/
			static string pause_station(const string& stattion);
			/**
			* \brief 继续站点
			*/
			static string resume_station(const string& stattion);
			/**
			* \brief 关闭站点
			*/
			static string close_station(const string& stattion);
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
			static string exec_command(const char* command, vector<sharp_char> arguments);
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