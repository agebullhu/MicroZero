#ifndef ZMQ_API_BROADCASTING_STATION_H
#pragma once
#include <stdinc.h>
#include <utility>
#include "ZeroStation.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 表示一个广播站点
		*/
		class broadcasting_station_base :public zero_station
		{
		public:
			broadcasting_station_base(string name, int type)
				: zero_station(std::move(name), type, ZMQ_ROUTER, ZMQ_PUB, -1)
			{
			}

			/**
			* \brief 执行一条命令
			*/
			sharp_char command(const char* caller, vector< sharp_char> lines) override
			{
				bool res = publish(caller, lines[0], lines[1], lines[1]);
				return sharp_char(res ? ZERO_STATUS_OK : ZERO_STATUS_FAILED);
			}


			/**
			* \brief 处理请求
			*/
			void request(ZMQ_HANDLE socket, bool inner)override;

			/**
			* 心跳的响应
			*/
			void heartbeat()override {}
			/**
			* \brief 处理反馈
			*/
			void response()override {}

			/**
			*\brief 发送消息
			*/
			bool publish(const sharp_char& title, const sharp_char& description, vector<sharp_char>& datas);

			/**
			*\brief 发布消息
			*/
			bool publish(const string& publiher, const string& title, const string& arg);
			/**
			*\brief 广播内容
			*/
			bool publish(const string& caller, const string& title, const string& sub, const string& arg);
			/**
			*\brief 广播内容
			*/
			bool publish(const string& caller, const string& title, const string& plan, const string& sub, const string& arg) const;
		
		};

		/**
		 * \brief 表示一个广播站点
		 */
		class broadcasting_station :public broadcasting_station_base
		{
		public:
			broadcasting_station(string name)
				:broadcasting_station_base(name, STATION_TYPE_PUBLISH)
			{

			}
			virtual ~broadcasting_station() = default;
			/**
			* 运行一个广播线程
			*/
			static void run(string publish_name)
			{
				zmq_threadstart(launch, new broadcasting_station(publish_name));
			}
			/**
			*消息泵
			*/
			static void launch(void* arg);
			/**
			*\brief 广播内容
			*/
			static bool publish(string station, string publiher, string title, string sub, string arg);
		};


		/**
		* \brief 表示一个广播站点
		*/
		class system_monitor_station :broadcasting_station_base
		{
			/**
			* \brief 能否继续工作
			*/
			bool can_do() const override
			{
				return (_station_state == station_state::Run || _station_state == station_state::Pause) && get_net_state() < NET_STATE_DISTORY;
			}
			static system_monitor_station* example_;
		public:
			system_monitor_station()
				:broadcasting_station_base("SystemMonitor", STATION_TYPE_MONITOR)
			{

			}
			virtual ~system_monitor_station() = default;

			///**
			//* \brief 暂停
			//*/
			//bool pause(bool waiting) override
			//{
			//	return false;
			//}

			///**
			//* \brief 继续
			//*/
			//bool resume(bool waiting)override
			//{
			//	return false;
			//}

			///**
			//* \brief 结束
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
				if (example_ != nullptr)
					return;
				zmq_threadstart(launch, nullptr);
			}
			/**
			*消息泵
			*/
			static void launch(void*);

			/**
			*\brief 广播内容
			*/
			static bool monitor(string publiher, string state, string content)
			{
				if (example_ != nullptr)
					return example_->publish(publiher, state, content);
				return false;
			}
		};
	}
}
#endif