#ifndef ZMQ_API_NET_DISPATCHER_H
#pragma once
#include <stdinc.h>
#include "NetStation.h"
#include "StationWarehouse.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		 * @brief 表示一个广播站点
		 */
		class NetDispatcher :NetStation
		{
			/**
			* @brief 当前活动的发布类
			*/
			static NetDispatcher* example;
		public:
			NetDispatcher()
				:NetStation("SystemManage", STATION_TYPE_DISPATCHER, ZMQ_ROUTER, -1, -1)
			{

			}
			virtual ~NetDispatcher() {}
		//private:

		//	/**
		//	* @brief 暂停
		//	*/
		//	bool pause(bool waiting) override
		//	{
		//		return false;
		//	}

		//	/**
		//	* @brief 继续
		//	*/
		//	bool resume(bool waiting)override
		//	{
		//		return false;
		//	}

		//	/**
		//	* @brief 结束
		//	*/
		//	bool close(bool waiting)override
		//	{
		//		return false;
		//	}
		public:
			/**
			*消息泵
			*/
			static void start(void*)
			{
				if (!StationWarehouse::join(example))
				{
					delete example;
					return;
				}
				if (example->_zmq_state == ZmqSocketState::Succeed)
					log_msg3("%s(%d | %d)正在启动", example->_station_name.c_str(), example->_out_port, example->_inner_port)
				else
					log_msg3("%s(%d | %d)正在重启", example->_station_name.c_str(), example->_out_port, example->_inner_port)
				if (!example->initialize())
				{
					log_msg3("%s(%d | %d)无法启动", example->_station_name.c_str(), example->_out_port, example->_inner_port)
					return;
				}
				log_msg3("%s(%d | %d)正在运行", example->_station_name.c_str(), example->_out_port, example->_inner_port)
				bool reStrart = example->poll();
				StationWarehouse::left(example);
				example->destruct();
				if (reStrart)
				{
					delete example;
					example = new NetDispatcher();
					example->_zmq_state = ZmqSocketState::Again;
					zmq_threadstart(start, nullptr);
				}
				else
				{
					log_msg3("%s(%d | %d)已关闭", example->_station_name.c_str(), example->_out_port, example->_inner_port)
					delete example;
					example = nullptr;
				}
			}

			/**
			* 运行一个广播线程
			*/
			static void run()
			{
				if (example != nullptr)
				{
					return;
				}
				example = new NetDispatcher();
				zmq_threadstart(start, nullptr);
				//boost::thread thrds_s(boost::bind(start));
			}

			/**
			* @brief 执行一条命令
			*/
			sharp_char command(const char* caller, vector<string> lines) override
			{
				string val = call_station(caller, lines[0].c_str(), lines[1].c_str());
				return sharp_char(val);
			}
		private:

			/**
			* @brief 处理反馈
			*/
			virtual void response()override
			{

			}
			/**
			* @brief 处理请求
			*/
			virtual void request(ZMQ_HANDLE socket)override;

			/**
			* 心跳的响应
			*/
			virtual void heartbeat()override
			{

			}
		public:
			/**
			* @brief 站点安装
			*/
			static string install_station(int type, string stattion);
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
			* @brief 执行命令
			*/
			static string exec_command(const char* command, const  char* argument);

		};
	}
}
#endif