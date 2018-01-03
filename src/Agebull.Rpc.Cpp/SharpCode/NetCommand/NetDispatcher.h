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
			NetDispatcher()
				:NetStation("SystemManage", STATION_TYPE_DISPATCHER, ZMQ_REP, -1, -1)
			{

			}
			virtual ~NetDispatcher() {}
		private:
			/**
			* @brief 当前活动的发布类
			*/
			static NetDispatcher* example;

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
		public:
			/**
			*消息泵
			*/
			static void start()
			{
				if (!StationWarehouse::join(example))
				{
					delete example;
					return;
				}
				if (example->_zmq_state == 0)
					log_msg3("%s(%s | %s)正在启动", example->_station_name, example->_out_address, example->_inner_address);
				else
					log_msg3("%s(%s | %s)正在重启", example->_station_name, example->_out_address, example->_inner_address);
				bool reStrart = example->poll();
				StationWarehouse::left(example);
				if (reStrart)
				{
					delete example;
					example = new NetDispatcher();
					example->_zmq_state = -1;
					boost::thread thrds_s(boost::bind(start));
				}
				else
				{
					log_msg3("%s(%s | %s)已关闭", example->_station_name, example->_out_address, example->_inner_address);
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
				boost::thread thrds_s(boost::bind(start));
			}

			/**
			* @brief 开始执行一条命令
			*/
			void command_start(const  char* caller, vector<string> lines)
			{
				exec_command(lines[0].c_str(), lines[1].c_str());
			}
			/**
			* @brief 结束执行一条命令
			*/
			void command_end(const  char* caller, vector<string> lines)
			{
				//send_result(lines[0]);
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
			virtual void request()override;

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
			* @brief 执行命令
			*/
			static string exec_command(const char* command, const  char* argument);

		};
	}
}
#endif