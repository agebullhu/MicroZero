/**
 * ZMQ队列类
 */

#include "../stdafx.h"
#include "station_warehouse.h"
#include "queue_station.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 内部命令
		*/
		bool queue_station::extend_command(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner)
		{
			if (description.command() == ZERO_BYTE_COMMAND_RESTART)
			{
				send_request_status(socket, list, description, inner, ZERO_STATUS_OK_ID);
				int64 min = atoll(*list[2]);
				int64 max = atoll(*list[3]);

				_storage.load(min, max, [this](vector<shared_char>& data)
				{
					send_response(data);
				});
				return true;
			}
			return false;
		}

		/**
		* \brief 处理请求
		*/
		/**
		* \brief 工作开始（发送到工作者）
		*/
		void queue_station::job_start(zmq_handler socket, vector<shared_char>& list, bool inner)
		{
			shared_char caller = list[0];
			if (inner)
				list.erase(list.begin());
			var description = list[1];
			size_t pid = 0, gid = 0, cid = 0, tid = 0, sid = 0, rid = 0;
			for (size_t idx = 2; idx <= description.frame_size() + 2; idx++)
			{
				switch (description[idx])
				{
				case ZERO_FRAME_REQUEST_ID:
					pid = idx;
					break;
				case ZERO_FRAME_REQUESTER:
					rid = idx;
					break;
				case ZERO_FRAME_GLOBAL_ID:
					gid = idx;
					break;
				case ZERO_FRAME_PUB_TITLE:
					tid = idx;
					break;
				case ZERO_FRAME_SUBTITLE:
					sid = idx; 
					break;
				case ZERO_FRAME_CONTENT:
					cid = idx;
					break;
				}
			}
			if (tid == 0)
			{
				send_request_status(socket, *caller, ZERO_STATUS_FRAME_INVALID_ID, list, gid, rid, pid);
				return;
			}
			send_request_status(socket, *caller, ZERO_STATUS_OK_ID, list, gid, rid, pid);

			int64 id = _storage.save(*list[tid],
				sid > 0 ? *list[sid] : nullptr,
				cid > 0 ? *list[cid] : nullptr,
				rid > 0 ? *list[rid] : nullptr,
				pid > 0 ? *list[pid] : nullptr,
				gid > 0 ? atoll(*list[pid]) : 0);
			description.append_frame(ZERO_FRAME_LOCAL_ID);

			shared_char local_id;
			local_id.set_int64x(id);
			list.emplace_back(local_id);

			list[0] = list[tid];
			send_response(list, 0);
		}

		/**
		*\brief 运行一个通知线程
		*/
		void queue_station::launch(shared_ptr<queue_station> station)
		{
			zero_config& config = station->get_config();
			if (!station->initialize())
			{
				config.failed("initialize");
				set_command_thread_bad(config.station_name_.c_str());
				return;
			}
			if (!station_warehouse::join(station.get()))
			{
				config.failed("join warehouse");
				set_command_thread_bad(config.station_name_.c_str());
				return;
			}
			station->_storage.prepare_storage(station->config_);
			station->poll();
			station_warehouse::left(station.get());
			station->destruct();
			if (!config.is_state(station_state::stop) && get_net_state() == net_state_runing)
			{
				config.restart();
				run(station->get_config_ptr());
			}
			else
			{
				config.closed();
			}
			set_command_thread_end(config.station_name_.c_str());
		}
	}
}
