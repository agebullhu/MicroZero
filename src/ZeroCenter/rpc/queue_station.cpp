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
		bool queue_station::simple_command_ex(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner)
		{
			if (description.command() == zero_def::command::restart)
			{
				send_request_status(socket, list, description, inner, zero_def::status::ok);
				int64 min = atoll(*list[2]);
				int64 max = atoll(*list[3]);

				storage_.load(min, max, [this](vector<shared_char>& data)
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
			const shared_char caller = list[0];
			if (inner)
				list.erase(list.begin());
			shared_char& description = list[1];
			size_t pid = 0, gid = 0, cid = 0, tid = 0, sid = 0, rid = 0;
			for (size_t idx = 2; idx <= description.desc_size() && idx < list.size(); idx++)
			{
				switch (description[idx])
				{
				case zero_def::frame::request_id:
					pid = idx;
					break;
				case zero_def::frame::requester:
					rid = idx;
					break;
				case zero_def::frame::global_id:
					gid = idx;
					break;
				case zero_def::frame::pub_title:
					tid = idx;
					break;
				case zero_def::frame::sub_title:
					sid = idx; 
					break;
				case zero_def::frame::content:
					cid = idx;
					break;
				}
			}
			if (tid == 0)
			{
				send_request_status(socket, *caller, zero_def::status::frame_invalid, list, gid, rid, pid);
				return;
			}
			send_request_status(socket, *caller, zero_def::status::ok, list, gid, rid, pid);

			const int64 id = storage_.save(*list[tid],
				sid > 0 ? *list[sid] : nullptr,
				cid > 0 ? *list[cid] : nullptr,
				rid > 0 ? *list[rid] : nullptr,
				pid > 0 ? *list[pid] : nullptr,
				gid > 0 ? atoll(*list[pid]) : 0);

			description.append_frame(zero_def::frame::local_id);
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
				set_command_thread_bad(config.station_name.c_str());
				return;
			}
			if (!station_warehouse::join(station.get()))
			{
				config.failed("join warehouse");
				set_command_thread_bad(config.station_name.c_str());
				return;
			}
			station->storage_.prepare_storage(station->config_);
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
