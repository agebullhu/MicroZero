/**
* ZMQ广播代理类
*
*
*/

#include "../stdafx.h"
#include "BroadcastingStation.h"
#include "StationWarehouse.h"
#include "system_monitor_station.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 单例
		*/
		system_monitor_station* system_monitor_station::instance = nullptr;


		/**
		*\brief 运行一个广播线程
		*/
		void system_monitor_station::launch(shared_ptr<system_monitor_station>& station)
		{
			station->config_->log_start();
			if (!station_warehouse::join(station.get()))
			{
				station->config_->log_failed();
				return;
			}
			if (!station->initialize())
			{
				station->config_->log_failed();
				return;
			}
			boost::thread(boost::bind(monitor_poll));
			station->poll();
			instance = nullptr;
			station_warehouse::left(station.get());
			station->destruct();
			if (station->config_->station_state_ != station_state::Uninstall && get_net_state() == NET_STATE_RUNING)
			{
				station->config_->station_state_ = station_state::ReStart;
				run(station->config_);
			}
			else
			{
				station->config_->log_closed();
			}
			sleep(1);
		}

		/**
		* \brief 监控轮询
		*/
		void system_monitor_station::monitor_poll()
		{
			instance->config_->log("monitor poll start");
			while (instance != nullptr && instance->can_do())
			{
				sleep(1);
				for (auto & config : station_warehouse::configs_)
				{
					monitor(config.first, "station_state",config.second->to_json().c_str());
				}
			}
			//instance_->config_->log("monitor poll end");
		}
	}
}