#ifndef ZMQ_API_NET_DISPATCHER_H
#pragma once
#include <stdinc.h>
#include "ZeroStation.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		 * \brief 表示站点调度服务
		 */
		class station_dispatcher :public zero_station
		{
			/**
			* \brief 单例
			*/
			static station_dispatcher* instance;
		public:
			/**
			* \brief 构造
			*/
			station_dispatcher()
				:zero_station("SystemManage", STATION_TYPE_DISPATCHER, ZMQ_ROUTER, -1, -1)
			{

			}
			/**
			* \brief 析构
			*/
			~station_dispatcher() override = default;
			/**
			* \brief 开始执行
			*/
			static void start(void*);

			/**
			*  \brief 执行
			*/
			static void run()
			{
				if (instance != nullptr)
				{
					return;
				}
				instance = new station_dispatcher();
				zmq_threadstart(start, nullptr);
				//boost::thread thrds_s(boost::bind(start));
			}
		private:

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
			void request(ZMQ_HANDLE socket)override;
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
			static string call_station(string stattion, string command, string argument);
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

		};
	}
}
#endif