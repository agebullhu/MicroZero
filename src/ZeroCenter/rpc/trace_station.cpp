/**
 * ZMQ通知代理类
 */

#include "../stdafx.h"
#include "station_warehouse.h"
#include "trace_station.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 处理请求
		*/
		/**
		* \brief 工作开始 : 处理请求数据
		*/
		void trace_station::job_start(zmq_handler socket, vector<shared_char>& list, bool inner, bool old)
		{
			if (config_->get_state() == station_state::pause)
			{
				return;
			}
			if (inner)
				list.erase(list.begin());
			var description = list[1];
			size_t request_id = 0, global_id = 0, station = 0, call_id = 0, station_type = 0, status=0;
			for (size_t idx = 2; idx <= description.desc_size() && idx < list.size(); idx++)
			{
				switch (description[idx])
				{
				case zero_def::frame::request_id:
					request_id = idx;
					break; 
				case zero_def::frame::station_id:
					station = idx;
					break;
				case zero_def::frame::global_id:
					global_id = idx;
					break;
				case zero_def::frame::call_id:
					call_id = idx;
					break;
				case zero_def::frame::station_type:
					station_type = idx;
					break;
				case zero_def::frame::status:
					status = idx;
					break;
				}
			}
			storage_->save(description.tag()
				, vector_str(list, request_id)
				, vector_int(list, call_id)
				, vector_int(list, global_id)
				, vector_str(list, station)
				, vector_str(list, station_type)
				, vector_str(list, status)
				, list);

			//list[0] = list[request_id];
			//send_response(list, 0);
		}

		/**
		*\brief 运行一个通知线程
		*/
		void trace_station::launch(shared_ptr<trace_station> station)
		{
			zero_config& config = station->get_config();
			config.is_base = false;
			if (!station->initialize())
			{
				config.failed("initialize");
				set_command_thread_bad(config.station_name.c_str());
				return;
			}
			if (!station_warehouse::join(station.get()))
			{
				config.failed("join warehouse");
				set_command_thread_bad(config.station_name.c_str());
				return;
			}
			station->poll();
			station_warehouse::left(station.get());
			station->destruct();
			if (!config.is_state(station_state::stop) && get_net_state() == zero_def::net_state::runing)
			{
				config.restart();
				run(station->get_config_ptr());
			}
			else
			{
				config.closed();
			}
			set_command_thread_end(config.station_name.c_str());
		}
	}
}
