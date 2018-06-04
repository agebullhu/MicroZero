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
		void broadcasting_station::job_start(ZMQ_HANDLE socket, vector<sharp_char>& list)//, sharp_char& global_id
		{
			if (list.size() < 5)
			{
				send_request_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID_ID);
				return;
			}
			const sharp_char description = list[2];
#if _DEBUG_
			assert(description[2] == ZERO_FRAME_PUB_TITLE);
			assert(description[3] == ZERO_FRAME_REQUEST_ID);
#endif // _DEBUG_
			send_request_status(socket, *list[0], ZERO_STATUS_OK_ID, nullptr, *list[4]);
			list[2] = list[3];
			list[3] = description;
			char* buf = description.get_buffer();
			for (size_t i = 2; i < description.size(); i++)
				buf[i] = buf[i + 1];
			send_response(list, 2);
		}

		/**
		*\brief 发布消息
		*/
		bool broadcasting_station::publish(const sharp_char& title, const sharp_char& description, vector<sharp_char>& datas)
		{
			const auto first = datas.begin();
			datas.insert(first, title);
			datas.insert(first, description);
			return send_response(datas);
		}

		/**
		*\brief 发布消息
		*/
		bool broadcasting_station::publish(const string& publiher, const string& title, const string& arg)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (!can_do() || publiher.length() == 0)
				return false;
			sharp_char description;
			description.alloc(6);
			char* buf = description.get_buffer();
			buf[0] = 3;
			buf[2] = ZERO_FRAME_PUBLISHER;
			buf[3] = ZERO_FRAME_ARG;
			buf[4] = ZERO_FRAME_GLOBAL_ID;
			vector<sharp_char> datas;
			datas.emplace_back(title.c_str());
			datas.emplace_back(description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(arg.c_str());
			const int64 id = station_warehouse::get_glogal_id();
			sharp_char global_id(32);
			sprintf(global_id.get_buffer(), "%llx", id);
			datas.emplace_back(global_id);
			return send_response(datas);
		}
		/**
		*\brief 发布消息
		*/
		bool broadcasting_station::publish(const string& publiher, const string& title, const string& sub, const string& arg)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (!can_do() || publiher.empty())
				return false;
			sharp_char description;
			description.alloc(6);
			char* buf = description.get_buffer();
			buf[0] = 4;
			buf[2] = ZERO_FRAME_PUBLISHER;
			buf[3] = ZERO_FRAME_SUBTITLE;
			buf[4] = ZERO_FRAME_ARG;
			buf[5] = ZERO_FRAME_GLOBAL_ID;
			vector<sharp_char> datas;
			datas.emplace_back(title.c_str());
			datas.emplace_back(description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(sub.c_str());
			datas.emplace_back(arg.c_str());
			const int64 id = station_warehouse::get_glogal_id();
			sharp_char global_id(32);
			sprintf(global_id.get_buffer(), "%llx", id);
			datas.emplace_back(global_id);

			return send_response(datas);
		}

		/**
		*\brief 发布消息
		*/
		bool broadcasting_station::publish(const string& publiher, const string& title, const string& sub, const string& plan, const string& arg) const
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (!can_do() || publiher.length() == 0)
				return false;
			const int64 id = station_warehouse::get_glogal_id();
			sharp_char global_id(32);
			sprintf(global_id.get_buffer(), "%llx", id);

			plan_message message;
			message.request_caller = publiher.c_str();
			message.request_id = global_id;
			message.plan_id = id;
			vector<sharp_char> datas;
			message.messages_description.alloc(8);
			char* buf = message.messages_description.get_buffer();
			buf[0] = 5;
			buf[2] = ZERO_FRAME_PUB_TITLE;
			buf[3] = ZERO_FRAME_REQUEST_ID;
			buf[4] = ZERO_FRAME_PLAN;
			buf[5] = ZERO_FRAME_SUBTITLE;
			buf[6] = ZERO_FRAME_ARG;
			buf[7] = ZERO_FRAME_PUBLISHER;
			buf[8] = ZERO_FRAME_GLOBAL_ID;
			message.messages.emplace_back(message.messages_description);
			message.messages.emplace_back(title.c_str());
			message.messages.emplace_back(global_id);
			message.messages.emplace_back(plan.c_str());
			message.messages.emplace_back(sub.c_str());
			message.messages.emplace_back(arg.c_str());
			message.messages.emplace_back(publiher.c_str());
			message.messages.emplace_back(global_id);


			message.read_plan(plan.c_str());

			return plan_next(message, true);
		}

		/**
		*\brief 运行一个广播线程
		*/
		void broadcasting_station::launch(shared_ptr<broadcasting_station> station)
		{
			zero_config& config = station->get_config();
			config.log_start();
			if (!station_warehouse::join(station.get()))
			{
				config.log_failed("join warehouse");
				set_command_thread_bad(config.station_name_.c_str());
				return;
			}
			if (!station->initialize())
			{
				config.log_failed("initialize");
				set_command_thread_bad(config.station_name_.c_str());
				return;
			}
			boost::thread(boost::bind(plan_poll, station.get()));
			station->poll();
			station_warehouse::left(station.get());
			station->destruct();
			if (config.station_state_ != station_state::Uninstall && get_net_state() == NET_STATE_RUNING)
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
