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
		void broadcasting_station::request(ZMQ_HANDLE socket, bool inner)
		{
			vector<sharp_char> list;
			zmq_state_ = recv(socket, list);
			if (zmq_state_ != zmq_socket_state::Succeed)
			{
				config_->log(state_str(zmq_state_));
				return;
			}
			const sharp_char& caller = list[0];
			if (list.size() <= 3)
			{
				send_request_status(socket, *caller, ZERO_STATUS_FRAME_INVALID);
				return;
			}
			sharp_char& title = list[2];
			sharp_char& description = list[3];
			char* const buf = description.get_buffer();
			const size_t list_size = list.size();
			const auto frame_size = static_cast<size_t>(buf[0]);
			if (frame_size >= description.size() || (frame_size + 4) != list_size)
			{
				send_request_status(socket, *caller, ZERO_STATUS_FRAME_INVALID);
				return;
			}
			bool hase_plan = false;
			for (size_t idx = 1; idx <= frame_size; idx++)
			{
				if (buf[idx] == ZERO_FRAME_PLAN)
				{
					hase_plan = true;
					break;
				}
			}
			if (!hase_plan)
			{
				send_request_status(socket, *caller, ZERO_STATUS_OK);
				send_response(list, 2);
				//cout << *caller << endl;
				return;
			}
			plan_message message;
			message.messages.push_back(title);
			message.messages.push_back(description);
			sharp_char plan, sub, arg, publisher, id;
			for (size_t idx = 1; idx <= frame_size; idx++)
			{
				switch (buf[idx])
				{
				case ZERO_FRAME_PLAN:
					plan = list[3 + idx];
					break;
				case ZERO_FRAME_PUBLISHER:
					publisher = list[3 + idx];
					break;
				case ZERO_FRAME_SUBTITLE:
					sub = list[3 + idx];
					break;
				case ZERO_FRAME_ARG:
					arg = list[3 + idx];
					break;
				case ZERO_FRAME_REQUEST_ID:
					id = list[3 + idx];
					break;
				}
			}
			int cnt = 0;
			buf[++cnt] = ZERO_FRAME_PUBLISHER;
			message.messages.push_back(publisher);
			if (!id.empty())
			{
				buf[++cnt] = ZERO_FRAME_REQUEST_ID;
				message.messages.push_back(id);
			}
			if (!sub.empty())
			{
				buf[++cnt] = ZERO_FRAME_SUBTITLE;
				message.messages.push_back(sub);
			}
			if (!arg.empty())
			{
				buf[++cnt] = ZERO_FRAME_ARG;
				message.messages.push_back(arg);
			}
			buf[0] = static_cast<char>(cnt);

			message.read_plan(plan.get_buffer());
			message.request_caller = caller;
			message.messages_description = description;
			plan_next(message, true);


			send_request_status(socket, *caller, ZERO_STATUS_PLAN);

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
			buf[0] = 2;
			buf[1] = ZERO_FRAME_PUBLISHER;
			buf[2] = ZERO_FRAME_ARG;
			buf[3] = ZERO_FRAME_END;
			vector<sharp_char> datas;
			datas.emplace_back(title.c_str());
			datas.emplace_back(description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(arg.c_str());
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
			buf[0] = 3;
			buf[1] = ZERO_FRAME_PUBLISHER;
			buf[2] = ZERO_FRAME_SUBTITLE;
			buf[3] = ZERO_FRAME_ARG;
			buf[4] = ZERO_FRAME_END;
			vector<sharp_char> datas;
			datas.emplace_back(title.c_str());
			datas.push_back(description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(sub.c_str());
			datas.emplace_back(arg.c_str());

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
			plan_message message;
			message.request_caller = publiher;
			vector<sharp_char> datas;
			vector<string> argument;
			message.messages_description.alloc(6);
			char* buf = message.messages_description.get_buffer();
			buf[0] = 3;
			buf[1] = ZERO_FRAME_PUBLISHER;
			buf[2] = ZERO_FRAME_SUBTITLE;
			buf[3] = ZERO_FRAME_ARG;
			buf[4] = ZERO_FRAME_END;
			message.messages.emplace_back(title.c_str());
			datas.emplace_back(message.messages_description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(sub.c_str());
			datas.emplace_back(arg.c_str());

			message.read_plan(plan.c_str());

			return plan_next(message, true);
		}

		/**
		*\brief 运行一个广播线程
		*/
		void broadcasting_station::launch(shared_ptr<broadcasting_station> station)
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
