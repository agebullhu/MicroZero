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
		void broadcasting_station::job_start(ZMQ_HANDLE socket, sharp_char& global_id, vector<sharp_char>& list)
		{
			if (list.size() < 5)
			{
				send_request_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID_ID);
				return;
			}
#if _DEBUG_
			const sharp_char description = list[2];
			assert(description[2] == ZERO_FRAME_PUB_TITLE);
			assert(description[3] == ZERO_FRAME_REQUEST_ID);
#endif // _DEBUG_
			send_request_status(socket, *list[0], ZERO_STATUS_OK_ID, *global_id, *list[4]);
			list[2] = list[3];
			list[3] = description;
			char* buf = *description;
			for (size_t i = 2; i < description.size(); i++)
				buf[i] = buf[i + 1];
			send_response(list, 2);
		}

		/**
		*\brief 发布消息
		*/
		bool broadcasting_station::publish(const sharp_char& title, const sharp_char& description, vector<sharp_char>& datas)
		{
			send_more(response_socket_tcp_, *title);
			send_more(response_socket_tcp_, *description);
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
			sharp_char g(16);
			sprintf(*g, "%llx", id);
			datas.emplace_back(g);
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
			sharp_char g(16);
			sprintf(*g, "%llx", id);
			datas.emplace_back(g);

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
			sharp_char g(16);
			sprintf(*g, "%llx", id);

			plan_message message;
			message.request_caller = publiher.c_str();
			message.request_id = g;
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
			message.messages.emplace_back(g);
			message.messages.emplace_back(plan.c_str());
			message.messages.emplace_back(sub.c_str());
			message.messages.emplace_back(arg.c_str());
			message.messages.emplace_back(publiher.c_str());
			message.messages.emplace_back(g);


			message.read_plan(plan.c_str());

			return plan_next(message, true);
		}

		/**
		*\brief 运行一个广播线程
		*/
		void broadcasting_station::launch(shared_ptr<broadcasting_station> station)
		{
			station->config_->log_start();
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
			station->poll();
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
			thread_sleep(1000);
		}
	}
}
