/**
 * ZMQ通知代理类
 */

#include "../stdafx.h"
#include "station_warehouse.h"
#include "notify_station.h"

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
		void notify_station::job_start(zmq_handler socket, vector<shared_char>& list, bool inner, bool old)
		{
			trace(1, list, nullptr);
			if (config_->get_state() == station_state::pause)
			{
				send_request_status(socket, *list[0], zero_def::status::pause, true);
				return;
			}
			shared_char caller = list[0];
			if (inner)
				list.erase(list.begin());
			var description = list[1];
			size_t pid = 0, gid = 0, tid = 0, rid = 0;
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
				}
			}
			if (tid == 0)
			{
				send_request_status_by_trace(socket, *caller, zero_def::status::frame_invalid, list, gid, rid, pid);
				return;
			}
			send_request_status_by_trace(socket, *caller, zero_def::status::ok, list, gid, rid, pid);
			list[0] = list[tid];
			send_response(list, true);
		}

		/* *
		*\brief 发布消息
		* /
		bool notify_station::publish(const shared_char& title, const shared_char& description, vector<shared_char>& datas)
		{
			datas.insert(datas.insert(datas.begin(), title), description);
			return send_response(datas) == zmq_socket_state::succeed;
		}

		char notify_station::frames1[] = { zero_def::frame::publisher, zero_def::frame::content, zero_def::frame::global_id };
		/**
		*\brief 发布消息
		* /
		bool notify_station::publish(const string& publiher, const string& title, const string& arg)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (!can_do() || publiher.length() == 0)
				return false;
			shared_char description;
			description.alloc_desc(frames1);
			vector<shared_char> datas;
			datas.emplace_back(title.c_str());
			datas.emplace_back(description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(arg.c_str());
			shared_char global_id;
			global_id.set_int64(station_warehouse::get_glogal_id());
			datas.emplace_back(global_id);

			return send_response(datas) == zmq_socket_state::succeed;
		}
		char notify_station::frames2[] = { zero_def::frame::publisher,zero_def::frame::sub_title, zero_def::frame::content, zero_def::frame::global_id , zero_def::frame::global_id };
		/**
		*\brief 发布消息
		* /
		bool notify_station::publish(const string& publiher, const string& title, const string& sub, const string& arg)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (!can_do() || publiher.empty())
				return false;
			shared_char description;
			description.alloc_desc(frames2);
			vector<shared_char> datas;
			datas.emplace_back(title.c_str());
			datas.emplace_back(description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(sub.c_str());
			datas.emplace_back(arg.c_str());

			shared_char global_id;
			global_id.set_int64(station_warehouse::get_glogal_id());

			datas.emplace_back(global_id);

			return send_response(datas) == zmq_socket_state::succeed;
		}

		char notify_station::frames3[] = { zero_def::frame::publisher,zero_def::frame::sub_title, zero_def::frame::content,zero_def::frame::request_id, zero_def::frame::global_id,zero_def::frame::call_id };
		/**
		*\brief 发布消息
		* /
		bool notify_station::publish(const string& publiher, const string& title, const string& sub, const string& arg, const string& rid, const int64 gid, const int64 lid)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (!can_do() || publiher.length() == 0)
				return false;
			shared_char description;
			description.alloc_desc(frames3);
			vector<shared_char> datas;
			datas.emplace_back(title.c_str());
			datas.emplace_back(description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(sub.c_str());
			datas.emplace_back(arg.c_str());
			datas.emplace_back(rid.c_str());

			shared_char global_id;
			global_id.set_int64(gid);
			datas.emplace_back(global_id);

			shared_char local_id;
			local_id.set_int64(lid);
			datas.emplace_back(local_id);

			return send_response(datas) == zmq_socket_state::succeed;
		}*/
		/**
		*\brief 运行一个通知线程
		*/
		void notify_station::launch(shared_ptr<notify_station> station)
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
