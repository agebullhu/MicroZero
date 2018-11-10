/**
 * ZMQ广播代理类
 *
 *
 */

#include "../stdafx.h"
#include "station_warehouse.h"
#include "broadcasting_station.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 处理请求
		*/
		/**
		* \brief 工作开始（发送到工作者）
		*/
		void broadcasting_station::job_start(ZMQ_HANDLE socket, vector<shared_char>& list, bool inner)
		{
			shared_char caller = list[0];
			if (inner)
				list.erase(list.begin());
			var description = list[1];
			if (description[1] == ZERO_BYTE_COMMAND_FIND_RESULT)
			{
				int64 min = atoll(*list[2]);
				int64 max = atoll(*list[3]);

				vector<vector<zmq_net::shared_char>> datas;
				_storage.load(min, max, datas);
				char msg[128];
				sprintf(msg, "%lu", datas.size());
				send_request_status(socket, *caller, ZERO_STATUS_OK_ID, list, 0, 0, 0, msg);
				for (auto data : datas)
				{
					send_response(worker_in_socket_tcp_, data);
				}
				return;
			}

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
			int64 global_id = 0;
			if (gid > 0)
				global_id = 0;
			if (get_config().is_fidelity)
			{
				_storage.save(*list[tid],
					sid > 0 ? *list[sid] : nullptr,
					cid > 0 ? *list[cid] : nullptr,
					rid > 0 ? *list[rid] : nullptr,
					pid > 0 ? *list[pid] : nullptr,
					global_id);
			}
			list[0] = list[tid];
			send_response(list, 0);
		}

		/**
		*\brief 发布消息
		*/
		bool broadcasting_station::publish(const shared_char& title, const shared_char& description, vector<shared_char>& datas)
		{
			datas.insert(datas.insert(datas.begin(), title), description);
			return send_response(datas);
		}

		char frames1[] = { ZERO_FRAME_PUBLISHER, ZERO_FRAME_CONTENT, ZERO_FRAME_GLOBAL_ID };
		/**
		*\brief 发布消息
		*/
		bool broadcasting_station::publish(const string& publiher, const string& title, const string& arg)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (!can_do() || publiher.length() == 0)
				return false;
			shared_char description;
			description.alloc_frame(frames1);
			vector<shared_char> datas;
			datas.emplace_back(title.c_str());
			datas.emplace_back(description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(arg.c_str());
			shared_char global_id;
			global_id.set_int64x(station_warehouse::get_glogal_id());
			datas.emplace_back(global_id);

			return send_response(datas);
		}
		char frames2[] = { ZERO_FRAME_PUBLISHER,ZERO_FRAME_SUBTITLE, ZERO_FRAME_CONTENT, ZERO_FRAME_GLOBAL_ID };
		/**
		*\brief 发布消息
		*/
		bool broadcasting_station::publish(const string& publiher, const string& title, const string& sub, const string& arg)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (!can_do() || publiher.empty())
				return false;
			shared_char description;
			description.alloc_frame(frames2);
			vector<shared_char> datas;
			datas.emplace_back(title.c_str());
			datas.emplace_back(description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(sub.c_str());
			datas.emplace_back(arg.c_str());

			shared_char global_id;
			global_id.set_int64x(station_warehouse::get_glogal_id());

			datas.emplace_back(global_id);

			return send_response(datas);
		}

		char frames3[] = { ZERO_FRAME_PUBLISHER,ZERO_FRAME_SUBTITLE, ZERO_FRAME_CONTENT,ZERO_FRAME_REQUEST_ID, ZERO_FRAME_GLOBAL_ID,ZERO_FRAME_LOCAL_ID };
		/**
		*\brief 发布消息
		*/
		bool broadcasting_station::publish(const string& publiher, const string& title, const string& sub, const string& arg, const string& rid, const int64 gid, const int64 lid)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (!can_do() || publiher.length() == 0)
				return false;
			shared_char description;
			description.alloc_frame(frames3);
			vector<shared_char> datas;
			datas.emplace_back(title.c_str());
			datas.emplace_back(description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(sub.c_str());
			datas.emplace_back(arg.c_str());
			datas.emplace_back(rid.c_str());

			shared_char global_id;
			global_id.set_int64x(gid);
			datas.emplace_back(global_id);

			shared_char local_id;
			local_id.set_int64x(lid);
			datas.emplace_back(local_id);

			return send_response(datas);
		}
		/**
		*\brief 运行一个广播线程
		*/
		void broadcasting_station::launch(shared_ptr<broadcasting_station> station)
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
			station->poll();
			station_warehouse::left(station.get());
			station->destruct();
			if (!config.is_state(station_state::Stop) && get_net_state() == NET_STATE_RUNING)
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
