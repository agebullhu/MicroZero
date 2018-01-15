#ifndef ZMQ_API_NET_DISPATCHER_H
#pragma once
#include <stdinc.h>
#include "ZeroStation.h"
#include "StationWarehouse.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		 * @brief 表示一个广播站点
		 */
		class NetDispatcher :ZeroStation
		{
			/**
			* @brief 单例
			*/
			static NetDispatcher* instance;
		public:
			/**
			* @brief 构造
			*/
			NetDispatcher()
				:ZeroStation("SystemManage", STATION_TYPE_DISPATCHER, ZMQ_ROUTER, -1, -1)
			{

			}
			/**
			* @brief 析构
			*/
			~NetDispatcher() override {}
			/**
			* @brief 开始执行
			*/
			static void start(void*);

			/**
			*  @brief 执行
			*/
			static void run()
			{
				if (instance != nullptr)
				{
					return;
				}
				instance = new NetDispatcher();
				zmq_threadstart(start, nullptr);
				//boost::thread thrds_s(boost::bind(start));
			}
		private:

			/**
			* @brief 暂停
			*/
			bool pause(bool waiting) override
			{
				return false;
			}

			/**
			* @brief 继续
			*/
			bool resume(bool waiting)override
			{
				return false;
			}

			/**
			* @brief 结束
			*/
			bool close(bool waiting)override
			{
				return false;
			}
			/**
			* @brief 处理请求
			*/
			void request(ZMQ_HANDLE socket)override;
		public:
			/**
			* @brief 执行一条命令
			*/
			sharp_char command(const char* caller, vector<sharp_char> lines) override;
			/**
			* @brief 站点安装
			*/
			static string install_station(string type_name, string stattion);
			/**
			* @brief 取机器信息
			*/
			static string host_info(string stattion);
			/**
			*  @brief 启动站点
			*/
			static string start_station(string stattion);
			/**
			* @brief 暂停站点
			*/
			static string pause_station(string stattion);
			/**
			* @brief 继续站点
			*/
			static string resume_station(string stattion);
			/**
			* @brief 关闭站点
			*/
			static string close_station(string stattion);
			/**
			* @brief 远程调用
			*/
			static string call_station(string stattion, string command, string argument);
			/**
			* @brief 远程调用
			*/
			static string call_station(const char* stattion, vector<sharp_char>& arguments);
			/**
			* @brief 执行命令
			*/
			static string exec_command(const char* command, vector<sharp_char> arguments);
			/**
			* @brief 执行命令
			*/
			static string exec_command(const char* command, const char* argument);

		};
	}
}
#endif