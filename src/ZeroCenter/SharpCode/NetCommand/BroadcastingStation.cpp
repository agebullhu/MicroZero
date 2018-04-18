/**
 * ZMQ广播代理类
 *
 *
 */

#include "stdafx.h"
#include "NetCommand/broadcastingstation.h"
#include <NetCommand/StationWarehouse.h>

namespace agebull
{
	namespace zmq_net
	{
		/**
		 * \brief 单例
		 */
		system_monitor_station* system_monitor_station::example_ = nullptr;
		/**
		* \brief 处理请求
		*/
		void broadcasting_station_base::request(ZMQ_HANDLE socket, bool inner)
		{
			vector<sharp_char> list;
			_zmq_state = recv(socket, list);
			if (_zmq_state != zmq_socket_state::Succeed)
			{
				log_error3("接收消息失败%s(%d)%s", _station_name.c_str(), _inner_port, state_str(_zmq_state));
				return;
			}
			const sharp_char& caller = list[0];
			if (list.size() <= 3)
			{
				_zmq_state = send_addr(socket, *caller);
				_zmq_state = send_late(socket, ZERO_STATUS_FRAME_INVALID);
				return;
			}
			sharp_char& title = list[2];
			sharp_char& description = list[3];
			char* const buf = description.get_buffer();
			const size_t list_size = list.size();
			const size_t frame_size = static_cast<size_t>(buf[0]);
			if (frame_size >= description.size() || (frame_size + 4) != list_size)
			{
				_zmq_state = send_addr(socket, *caller);
				_zmq_state = send_late(socket, ZERO_STATUS_FRAME_INVALID);
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
				_zmq_state = send_addr(socket, *caller);
				_zmq_state = send_late(socket, ZERO_STATUS_OK);
				send_data(list, 2);
				return;
			}
			_zmq_state = send_addr(socket, *caller);
			_zmq_state = send_late(socket, ZERO_STATUS_PLAN);

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
			buf[0] = cnt;

			message.read_plan(plan.get_buffer());
			message.request_caller = caller;
			message.messages_description = description;
			plan_next(message, true);


		}

		/**
		*\brief 发布消息
		*/
		bool broadcasting_station_base::publish(const sharp_char& title, const sharp_char& description, vector<sharp_char>& datas)
		{
			send_more(_inner_socket, *title);
			send_more(_inner_socket, *description);
			_zmq_state = send(_inner_socket, datas);
			return _zmq_state == zmq_socket_state::Succeed;
		}

		/**
		*\brief 发布消息
		*/
		bool broadcasting_station::publish(string station, string publiher, string title, string sub, string arg)
		{
			const auto pub = station_warehouse::instance(station);
			if (pub == nullptr || pub->get_station_type() != STATION_TYPE_PUBLISH)
				return false;
			return dynamic_cast<broadcasting_station_base*>(pub)->publish(publiher, title, sub, arg);
		}

		/**
		*\brief 发布消息
		*/
		bool broadcasting_station_base::publish(const string& publiher, const string& title, const string& arg)
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
			datas.push_back(description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(arg.c_str());
			return send_data(datas);
		}
		/**
		*\brief 发布消息
		*/
		bool broadcasting_station_base::publish(const string& publiher, const string& title, const string& sub, const string& arg)
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
			return send_data(datas);
		}

		/**
		*\brief 发布消息
		*/
		bool broadcasting_station_base::publish(const string& publiher, const string& title, const string& sub, const string& plan, const string& arg) const
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
			datas.push_back(message.messages_description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(sub.c_str());
			datas.emplace_back(arg.c_str());

			message.read_plan(plan.c_str());

			return plan_next(message, true);
		}
		
		void broadcasting_station::launch(void* arg)
		{
			broadcasting_station* station = static_cast<broadcasting_station*>(arg);
			if (!station_warehouse::join(station))
			{
				return;
			}
			if (station->_zmq_state == zmq_socket_state::Succeed)
				log_msg4("%s(%d | %d) %d 正在启动", station->_station_name.c_str(), station->_out_port, station->_inner_port, station->_station_type);
			else
				log_msg4("%s(%d | %d) %d 正在重启", station->_station_name.c_str(), station->_out_port, station->_inner_port, station->_station_type);
			if (!station->initialize())
			{
				log_msg4("%s(%d | %d) %d 无法启动", station->_station_name.c_str(), station->_out_port, station->_inner_port, station->_station_type);
				return;
			}
			log_msg4("%s(%d | %d) %d 正在运行", station->_station_name.c_str(), station->_out_port, station->_inner_port, station->_station_type);

			zmq_threadstart(plan_poll, arg);
			const bool reStrart = station->poll();
			while (station->_in_plan_poll)
			{
				sleep(1);
			}
			//zmq_threadclose(t);
			station_warehouse::left(station);
			station->destruct();
			if (reStrart)
			{
				broadcasting_station* station2 = new broadcasting_station(station->_station_name);
				station2->_zmq_state = zmq_socket_state::Again;
				zmq_threadstart(launch, station2);
			}
			else
			{
				log_msg3("%s(%d | %d)已关闭", station->_station_name.c_str(), station->_out_port, station->_inner_port);
			}
			delete station;
		}


		void system_monitor_station::launch(void*)
		{
			example_ = new system_monitor_station();
			if (!station_warehouse::join(example_))
			{
				delete example_;
				return;
			}
			if (example_->_zmq_state == zmq_socket_state::Succeed)
				log_msg4("%s(%d | %d) %d 正在启动", example_->_station_name.c_str(), example_->_out_port, example_->_inner_port, example_->_station_type);
			else
				log_msg4("%s(%d | %d) %d 正在重启", example_->_station_name.c_str(), example_->_out_port, example_->_inner_port, example_->_station_type);
			if (!example_->initialize())
			{
				log_msg4("%s(%d | %d) %d 无法启动", example_->_station_name.c_str(), example_->_out_port, example_->_inner_port, example_->_station_type);
				return;
			}
			log_msg4("%s(%d | %d) %d 正在运行", example_->_station_name.c_str(), example_->_out_port, example_->_inner_port, example_->_station_type);
			zmq_threadstart(plan_poll, example_);
			const bool reStrart = example_->poll();
			while (example_->_in_plan_poll)
			{
				sleep(1);
			}
			//发送关闭消息
			{
				acl::string msg;
				msg.format("station_end\r\n%s\r\n%d\r\n", example_->_station_name.c_str(), example_->_inner_port);
				send_late(example_->_inner_socket, msg.c_str());
				thread_sleep(100);
			}
			example_->destruct();
			if (reStrart)
			{
				zmq_threadstart(launch, nullptr);
			}
			else
			{
				log_msg3("%s(%d | %d)已关闭", example_->_station_name.c_str(), example_->_out_port, example_->_inner_port);
			}
			delete example_;
			example_ = nullptr;
		}
	}
}
