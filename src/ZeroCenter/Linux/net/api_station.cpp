#include "../stdafx.h"
#include "api_station.h"
#include "ipc_request_socket.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 开始执行一条命令
		*/
		sharp_char api_station::command(const char* caller, vector<sharp_char> lines)
		{
			vector<sharp_char> response;
			ipc_request_socket<ZMQ_REQ> socket(caller, get_station_name());
			if(socket.request(lines, response))
				return response.empty() ? ZERO_STATUS_ERROR : response[0];
			switch(socket.get_state())
			{
			case zmq_socket_state::TimedOut:
				return ZERO_STATUS_TIMEOUT;
			default:
				return ZERO_STATUS_NET_ERROR;
			}
		}

		/**
		* \brief 执行
		*/
		void api_station::launch(shared_ptr<api_station>& station)
		{
			station->config_->log_start();
			{
				boost::lock_guard<boost::mutex> guard(station->mutex_);
				if (!station_warehouse::join(station.get()))
				{
					station->config_->log_failed();
					return;
				}
			}
			if (!station->initialize())
			{
				station->config_->log_failed();
				return;
			}
			station->poll();
			{
				boost::lock_guard<boost::mutex> guard(station->mutex_);
				station_warehouse::left(station.get());
			}
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
			thread_sleep(1000);
		}


	}
}
