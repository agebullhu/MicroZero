#ifndef ZMQ_API_BROADCASTING_STATION_H
#pragma once
#include <stdinc.h>
#include "NetStation.h"
namespace agebull
{
	namespace zmq_net
	{
		struct PublishItem
		{
			string publiher;
			string title;
			string arg;
		};

		/**
		* @brief 表示一个广播站点
		*/
		class BroadcastingStationBase :public NetStation
		{
			/**
			* @brief 状态信号量
			*/
			boost::interprocess::interprocess_semaphore _pub_semaphore;
			queue<PublishItem> items;
		public:
			BroadcastingStationBase(string name,int type)
				:NetStation(name, type, ZMQ_REQ, -1, -1)
				, _pub_semaphore(1)
			{

			}
			/**
			*消息泵
			*/
			static void dopub(BroadcastingStationBase* netobj)
			{
				netobj->poll_pub();
			}
			/**
			* 运行一个广播线程
			*/
			bool poll_pub();

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
			bool publish(string publiher, string title, string arg);

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
				BroadcastingStation* station = new BroadcastingStation(publish_name);
				boost::thread(boost::bind(start, shared_ptr<BroadcastingStation>(station)));
			}
			/**
			*消息泵
			*/
			static void start(shared_ptr<BroadcastingStation> arg)
			{
				BroadcastingStation* station = arg.get();
				if (!StationWarehouse::join(station))
				{
					return;
				}
				if (station->_zmq_state == 0)
					log_msg3("%s(%s | %s)正在启动", station->_station_name, station->_out_address, station->_inner_address);
				else
					log_msg3("%s(%s | %s)正在重启", station->_station_name, station->_out_address, station->_inner_address);
				boost::thread(boost::bind(dopub, static_cast<BroadcastingStationBase*>(station)));
				bool reStrart = station->poll();
				StationWarehouse::left(station);
				if (reStrart)
				{
					run(station->_station_name);
				}
				else
				{
					log_msg3("%s(%s | %s)已关闭", station->_station_name, station->_out_address, station->_inner_address);
				}
			}
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
			static SystemMonitorStation* example;
		public:
			SystemMonitorStation()
				:BroadcastingStationBase("SystemMonitor", STATION_TYPE_MONITOR)
			{

			}
			virtual ~SystemMonitorStation() {}

			/**
			* 运行一个广播线程
			*/
			static void run()
			{
				if (example != nullptr)
					return;
				example = new SystemMonitorStation();
				boost::thread(boost::bind(start));
			}
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
				boost::thread(boost::bind(dopub, static_cast<BroadcastingStationBase*>(example)));
				bool reStrart = example->poll();
				StationWarehouse::left(example);
				if (reStrart)
				{
					//delete example;
					example = new SystemMonitorStation();
					example->_zmq_state = -1;
					boost::thread(boost::bind(start));
				}
				else
				{
					log_msg3("%s(%s | %s)已关闭", example->_station_name, example->_out_address, example->_inner_address);
					//delete example;
					//example = nullptr;
				}
			}
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