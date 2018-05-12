#ifndef ZMQ_API_BROADCASTING_STATION_H
#pragma once
#include "../stdinc.h"
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
				const bool res = publish(caller, lines[0], lines[1], lines[1]);
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
			bool publish(const string& publiher, const string& title, const string& sub, const string& arg);
			/**
			*\brief 广播内容
			*/
			bool publish(const string& publiher, const string& title, const string& sub, const string& plan, const string& arg) const;
		
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
			* \brief 运行一个广播线程
			*/
			static void run(string publish_name)
			{
				zmq_threadstart(launch, new broadcasting_station(publish_name));
			}
			/**
			*\brief 运行一个广播线程
			*/
			static void launch(void* arg);
			/**
			*\brief 广播内容
			*/
			static bool publish(const string& station, const string& publiher, const string& title, const string& sub, const string& arg);
		};


		/**
		* \brief 系统监视站点
		*/
		class system_monitor_station :broadcasting_station_base
		{
			/**
			* \brief 能否继续工作
			*/
			bool can_do() const override
			{
				return (station_state_ == station_state::Run || station_state_ == station_state::Pause) && get_net_state() < NET_STATE_DISTORY;
			}

			/**
			* \brief 单例
			*/
			static system_monitor_station* example_;
		public:
			system_monitor_station()
				:broadcasting_station_base("SystemMonitor", STATION_TYPE_MONITOR)
			{

			}
			virtual ~system_monitor_station() = default;

			/**
			* \brief 运行一个广播线程
			*/
			static void run()
			{
				if (example_ != nullptr)
					return;
				zmq_threadstart(launch, nullptr);
			}
			/**
			* \brief 消息泵
			*/
			static void launch(void*);

			/**
			*\brief 广播内容
			*/
			static bool monitor(const string& publiher, const string& state, const string& content)
			{
				if (example_ != nullptr)
					return example_->publish(publiher, state, content);
				return false;
			}
		};
	}
}
#endif