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
				:NetStation(name, STATION_TYPE_DISPATCHER)
			{

			}
			virtual ~NetDispatcher() {}
		private:
			/**
			*消息泵
			*/
			bool poll();
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
					log_msg3("%s(%s | %s)正在启动", example->_station_name, example->_outAddress, example->_innerAddress);
				else
					log_msg3("%s(%s | %s)正在重启", example->_station_name, example->_outAddress, example->_innerAddress);
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
					log_msg3("%s(%s | %s)已关闭", example->_station_name, example->_outAddress, example->_innerAddress);
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
			* @brief 工作集合的响应
			*/
			void onCallerPollIn();

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
