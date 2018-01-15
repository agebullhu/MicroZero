#ifndef ZMQ_API_BROADCASTING_STATION_H
#pragma once
#include <stdinc.h>
#include "ZeroStation.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* @brief 表示一个广播站点
		*/
		class BroadcastingStationBase :public ZeroStation
		{
		public:
			BroadcastingStationBase(string name, int type)
				: ZeroStation(name, type, ZMQ_ROUTER, ZMQ_PUB, -1)
			{
			}

			/**
			* @brief 执行一条命令
			*/
			sharp_char command(const char* caller, vector< sharp_char> lines) override
			{
				bool res = publish(caller, lines[0], lines[1]);
				return sharp_char(res ? "OK" : "Bad");
			}

			/**
			*@brief 广播内容
			*/
			bool publish(string caller, string title, string arg) const;
			/**
			*@brief 广播内容
			*/
			bool publish(string caller, string title, string plan, string arg) const;

			/**
			* @brief 处理请求
			*/
			void request(ZMQ_HANDLE socket)override;

			/**
			* 心跳的响应
			*/
			void heartbeat()override {}
			/**
			* @brief 处理反馈
			*/
			void response()override {}

			/**
			*@brief 发送消息
			*/
			bool pub_data(string publiher, string title, string arg);

			/**
			*@brief 发送消息
			*/
			bool pub_data(string publiher, string line);
		};

		/**
		 * @brief 表示一个广播站点
		 */
		class BroadcastingStation :public BroadcastingStationBase
		{
		public:
			BroadcastingStation(string name)
				:BroadcastingStationBase(name, STATION_TYPE_PUBLISH)
			{

			}
			virtual ~BroadcastingStation() {}
			/**
			* 运行一个广播线程
			*/
			static void run(string publish_name)
			{
				zmq_threadstart(start, new BroadcastingStation(publish_name));
			}
			/**
			*消息泵
			*/
			static void start(void* arg);
			/**
			*@brief 广播内容
			*/
			static bool publish(string station, string publiher, string title, string arg);
		};


		/**
		* @brief 表示一个广播站点
		*/
		class SystemMonitorStation :BroadcastingStationBase
		{
			/**
			* @brief 能否继续工作
			*/
			bool can_do() const override
			{
				return (_station_state == station_state::Run || _station_state == station_state::Pause) && get_net_state() < NET_STATE_DISTORY;
			}
			static SystemMonitorStation* example;
		public:
			SystemMonitorStation()
				:BroadcastingStationBase("SystemMonitor", STATION_TYPE_MONITOR)
			{

			}
			virtual ~SystemMonitorStation() {}

			///**
			//* @brief 暂停
			//*/
			//bool pause(bool waiting) override
			//{
			//	return false;
			//}

			///**
			//* @brief 继续
			//*/
			//bool resume(bool waiting)override
			//{
			//	return false;
			//}

			///**
			//* @brief 结束
			//*/
			//bool close(bool waiting)override
			//{
			//	return false;
			//}
			/**
			* 运行一个广播线程
			*/
			static void run()
			{
				if (example != nullptr)
					return;
				zmq_threadstart(start, nullptr);
			}
			/**
			*消息泵
			*/
			static void start(void*);

			/**
			*@brief 广播内容
			*/
			static bool monitor(string publiher, string state, string content)
			{
				if (example != nullptr)
					return example->publish(publiher, state, content);
				return false;
			}

		};
	}
}
#endif