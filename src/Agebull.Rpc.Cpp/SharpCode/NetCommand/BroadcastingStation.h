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
				:NetStation(name, STATION_TYPE_PUBLISH)
			{

			}
			virtual ~BroadcastingStation() {}
		private:
			/**
			*消息泵
			*/
			bool poll();
			/**
			* @brief 当前活动的发布类
			*/
			static map<string, shared_ptr<BroadcastingStation>> examples;

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
			*消息泵
			*/
			static bool publish(string name, string type, string arg);
			/**
			* @brief 设置关闭所有
			*/
			static void close_all(bool waiting)
			{
				map<string, shared_ptr<BroadcastingStation>>::iterator end = examples.end();
				for_each(examples.begin(), examples.end(), [](pair<string, shared_ptr<BroadcastingStation>> iter)
				{
					iter.second->close(false);
				});
				if (waiting)
				{
					while (examples.size() > 0)
						thread_sleep(250);
				}
			}
		};
	}
}
