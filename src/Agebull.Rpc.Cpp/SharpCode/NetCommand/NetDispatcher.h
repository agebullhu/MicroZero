#ifndef ZMQ_API_NET_DISPATCHER_H
#pragma once
#include <stdinc.h>
#include "NetStation.h"
namespace agebull
{
	namespace zmq_net
	{
		/**
		 * @brief 表示一个广播站点
		 */
		class NetDispatcher :NetStation
		{
			////返回消息队列
			//std::queue<NetCommandArgPtr> _queue;
			////返回消息队列锁
			//boost::interprocess::interprocess_semaphore _semaphore;

		public:
			NetDispatcher(string name)
				:NetStation(name, STATION_TYPE_DISPATCHER, ZMQ_REQ, -1, -1)
			{

			}
			virtual ~NetDispatcher() {}
		private:
			/**
			* @brief 当前活动的发布类
			*/
			static NetDispatcher* example;

		public:
			/**
			*消息泵
			*/
			static void start()
			{
				StationWarehouse::join(example);
				if (example->_zmq_state == 0)
					log_msg3("%s(%s | %s)正在启动", example->_station_name, example->_out_address, example->_inner_address);
				else
					log_msg3("%s(%s | %s)正在重启", example->_station_name, example->_out_address, example->_inner_address);
				bool reStrart = example->poll();
				StationWarehouse::left(example);
				if (reStrart)
				{
					example = new NetDispatcher(example->_station_name);
					example->_zmq_state = -1;
					boost::thread thrds_s(boost::bind(start));
				}
				else
				{
					log_msg3("%s(%s | %s)已关闭", example->_station_name, example->_out_address, example->_inner_address);
				}
			}

			/**
			* 运行一个广播线程
			*/
			static void run(const char* name)
			{
				if (example != nullptr)
				{
					return;
				}
				example = new NetDispatcher(name);
				boost::thread thrds_s(boost::bind(start));
			}

			/**
			*消息泵
			*/
			static bool send_result(string caller, string state);

			/**
			* @brief 开始执行一条命令
			*/
			void command_start(const char* caller, vector< string> lines)
			{
				exec_command(caller, lines[0].c_str(), lines[1].c_str());
			}
			/**
			* @brief 结束执行一条命令
			*/
			void command_end(const char* caller, vector< string> lines)
			{
				send_result(caller, lines[0]);
			}
		private:

			/**
			* @brief 处理反馈
			*/
			virtual void response()
			{

			}
			/**
			* @brief 处理请求
			*/
			virtual void request()override;

			/**
			* 心跳的响应
			*/
			virtual void heartbeat()
			{

			}
		public:
			/**
			* @brief 暂停站点
			*/
			static void pause_station(string caller, string stattion);
			/**
			* @brief 继续站点
			*/
			static void resume_station(string caller, string stattion);
			/**
			*  @brief 启动站点
			*/
			static void start_station(string caller, string stattion);
			/**
			* @brief 关闭站点
			*/
			static void close_station(string caller, string stattion);
			/**
			* @brief 执行命令
			*/
			static void exec_command(const char* client_addr, const  char* command, const  char* argument);
			/**
			* @brief 关闭所有站点
			*/
			static void shutdown(string caller, string stattion);
		};
	}
}
#endif