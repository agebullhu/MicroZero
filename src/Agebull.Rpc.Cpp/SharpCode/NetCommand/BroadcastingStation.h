#ifndef ZMQ_API_BROADCASTING_STATION_H
#pragma once
#include <stdinc.h>
#include "ZeroStation.h"
#include "StationWarehouse.h"

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
			sharp_char command(const char* caller, vector< string> lines) override
			{
				bool res = publish(caller,lines[0], lines[1]);
				return sharp_char(res ? "OK" : "Bad");
			}

			/**
			*@brief 广播内容
			*/
			bool publish(string caller, string title, string arg) const;

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
			static void start(void* arg)
			{
				BroadcastingStation* station = static_cast<BroadcastingStation*>(arg);
				if (!StationWarehouse::join(station))
				{
					return;
				}
				if (station->_zmq_state == ZmqSocketState::Succeed)
					log_msg3("%s(%d | %d)正在启动", station->_station_name.c_str(), station->_out_port, station->_inner_port)
				else
					log_msg3("%s(%d | %d)正在重启", station->_station_name.c_str(), station->_out_port, station->_inner_port)
				if (!station->initialize())
				{
					log_msg3("%s(%d | %d)无法启动", station->_station_name.c_str(), station->_out_port, station->_inner_port)
					return;
				}
				log_msg3("%s(%d | %d)正在运行", station->_station_name.c_str(), station->_out_port, station->_inner_port)

				bool reStrart = station->poll();


				StationWarehouse::left(station);
				station->destruct();
				if (reStrart)
				{
					BroadcastingStation* station2 = new BroadcastingStation(station->_station_name);
					station2->_zmq_state = ZmqSocketState::Again;
					zmq_threadstart(start, station2);
				}
				else
				{
					log_msg3("%s(%d | %d)已关闭", station->_station_name.c_str(), station->_out_port, station->_inner_port);
				}
				delete station;
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
			static void start(void*)
			{
				example = new SystemMonitorStation();
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
				
				example->_state_semaphore.wait();//等发布循环结束
				{
					string str("station_end " + example->_station_name + " ");
					str += example->_inner_port;
					send_late(example->_inner_socket, str.c_str());
				}
				example->destruct();
				if (reStrart)
				{
					zmq_threadstart(start, nullptr);
				}
				else
				{
					log_msg3("%s(%d | %d)已关闭", example->_station_name.c_str(), example->_out_port, example->_inner_port)
				}
				delete example;
				example = nullptr;
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