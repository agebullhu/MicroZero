#ifndef ZMQ_API_BROADCASTING_STATION_H
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
		class BroadcastingStation :NetStation
		{

		public:
			BroadcastingStation(string name)
				:NetStation(name, STATION_TYPE_PUBLISH, ZMQ_REQ, ZMQ_PUB, -1)
			{

			}
			virtual ~BroadcastingStation() {}
		public:
			/**
			*消息泵
			*/
			static void start(shared_ptr<BroadcastingStation> netobj)
			{
				station_run(netobj);
			}

			/**
			* 运行一个广播线程
			*/
			static void run(string publish_name)
			{
				BroadcastingStation* route = new BroadcastingStation(publish_name);
				boost::thread thrds_s1(boost::bind(start, shared_ptr<BroadcastingStation>(route)));
			}

			/**
			* @brief 开始执行一条命令
			*/
			void command_start(const char* caller, vector< string> lines) override
			{
				publish(caller, lines[0], lines[1]);
			}
			/**
			* @brief 结束执行一条命令
			*/
			void command_end(const char* caller, vector<string> lines) override
			{
				publish(caller, lines[0], lines[1]);
			}
			/**
			*@brief 广播内容
			*/
			static bool publish(string station, string publiher, string title, string arg);
			/**
			*@brief 广播内容
			*/
			static bool publish_monitor(string publiher, string title, string arg);
			/**
			*@brief 广播内容
			*/
			bool publish(string publiher, string title, string arg) const;

			/**
			* @brief 处理反馈
			*/
			void response()override;
			/**
			* @brief 处理请求
			*/
			void request()override;

			/**
			* 心跳的响应
			*/
			void heartbeat()override {}
		};
	}
}
#endif