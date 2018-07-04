#pragma once
#ifndef _SYSTEM_MONITOR_STATION_H_
#pragma once
#include "../stdinc.h"
#include <utility>
#include "zero_station.h"
#include "broadcasting_station.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 系统监视站点
		*/
		class system_monitor_station :public broadcasting_station_base
		{
			/**
			* \brief 能否继续工作
			*/
			bool can_do() const override
			{
				return (config_->station_state_ == station_state::Run || config_->station_state_ == station_state::Pause) && get_net_state() < NET_STATE_DISTORY;
			}

			/**
			* \brief 监控轮询
			*/
			static void monitor_poll();
		public:
			/**
			* \brief 单例
			*/
			static system_monitor_station* instance;
			/**
			* \brief 构造
			*/
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
				instance = new system_monitor_station();
				boost::thread(boost::bind(launch, shared_ptr<system_monitor_station>(instance)));
			}
			/**
			*\brief 运行
			*/
			static void run(shared_ptr<zero_config>& config)
			{
				instance = new system_monitor_station();
				instance->config_ = config;
				boost::thread(boost::bind(launch, shared_ptr<system_monitor_station>(instance)));
			}
			/**
			* \brief 消息泵
			*/
			static void launch(shared_ptr<system_monitor_station>& station);

			/**
			*\brief 广播内容
			*/
			static bool monitor(const string& publiher, const string& state, const string& content)
			{
				if (instance != nullptr)
					return instance->publish(publiher, state, content);
				return false;
			}
		};
	}
}
#endif //!_SYSTEM_MONITOR_STATION_H_