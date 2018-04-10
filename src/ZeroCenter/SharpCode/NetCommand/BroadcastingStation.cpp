/**
 * ZMQ广播代理类
 *
 *
 */

#include "stdafx.h"
#include <NetCommand/broadcastingstation.h>
#include <NetCommand/StationWarehouse.h>

namespace agebull
{
	namespace zmq_net
	{
		/**
		 * \brief 单例
		 */
		system_monitor_station* system_monitor_station::example = nullptr;

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
			const auto pub = station_warehouse::find(station);
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
			buf[1] = zero_pub_publisher;
			buf[2] = zero_arg;
			buf[3] = zero_end;
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
			buf[1] = zero_pub_publisher;
			buf[2] = zero_pub_sub;
			buf[3] = zero_arg;
			buf[4] = zero_end;
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
			buf[1] = zero_pub_publisher;
			buf[2] = zero_pub_sub;
			buf[3] = zero_arg;
			buf[4] = zero_end;
			message.messages.emplace_back(title.c_str());
			datas.push_back(message.messages_description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(sub.c_str());
			datas.emplace_back(arg.c_str());

			message.read_plan(plan.c_str());

			return plan_next(message, true);
		}
		/**
		* \brief 处理请求
		*/
		void broadcasting_station_base::request(ZMQ_HANDLE socket)
		{
			vector<sharp_char> list;
			//0 路由到的地址 1 空帧 2 标题 3内容标签 4..n 内容
			_zmq_state = recv(socket, list);
			if (_zmq_state != zmq_socket_state::Succeed)
			{
				log_error3("接收消息失败%s(%d)%s", _station_name.c_str(), _inner_port, state_str(_zmq_state));
				return;
			}
			const auto caller = list[0];
			if (list.size() <= 3)
			{
				_zmq_state = send_addr(socket, *caller);
				_zmq_state = send_late(socket, "err");
				return;
			}
			const auto title = list[2];
			sharp_char description = list[3];
			sharp_char plan, sub, arg, publisher, id;
			char* const buf = description.get_buffer();
			const size_t size = list.size();
			for (size_t idx = 1; idx <= static_cast<size_t>(buf[0]) && idx < description.size(); idx++)
			{
				const size_t idx2 = 3 + idx;
				if (idx2 >= size)
				{
					_zmq_state = send_addr(socket, *caller);
					_zmq_state = send_late(socket, "err");
					return;
				}
				switch (buf[idx])
				{
				case zero_plan:
					plan = list[idx2];
					break;
				case zero_pub_publisher:
					publisher = list[idx2];
					break;
				case zero_pub_sub:
					sub = list[idx2];
					break;
				case zero_arg:
					arg = list[idx2];
					break;
				case zero_request_id:
					id = list[idx2];
					break;
				}
			}
			plan_message message;
			message.messages.push_back(title);
			message.messages.push_back(description);

			int cnt = 0;
			buf[++cnt] = zero_pub_publisher;
			message.messages.push_back(publisher);
			if (!id.empty())
			{
				buf[++cnt] = zero_request_id;
				message.messages.push_back(id);
			}
			if (!sub.empty())
			{
				buf[++cnt] = zero_pub_sub;
				message.messages.push_back(sub);
			}
			if (!arg.empty())
			{
				buf[++cnt] = zero_arg;
				message.messages.push_back(arg);
			}
			buf[0] = cnt;

			//std::cout << endl << caller.get_buffer() << "-" << title.get_buffer() << endl;
			//for (sharp_char& item : message.messages)
			//{
			//	std::cout << item.get_buffer() << endl;
			//}
			if (!plan.empty())
			{
				_zmq_state = send_addr(socket, *caller);
				_zmq_state = send_late(socket, "plan");
				message.read_plan(plan.get_buffer());
				message.request_caller = caller;
				message.messages_description = description;
				plan_next(message, true);
			}
			else
			{
				_zmq_state = send_addr(socket, *caller);
				_zmq_state = send_late(socket, "ok");
				send_data(message.messages);
			}
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
			example = new system_monitor_station();
			if (!station_warehouse::join(example))
			{
				delete example;
				return;
			}
			if (example->_zmq_state == zmq_socket_state::Succeed)
				log_msg4("%s(%d | %d) %d 正在启动", example->_station_name.c_str(), example->_out_port, example->_inner_port, example->_station_type);
			else
				log_msg4("%s(%d | %d) %d 正在重启", example->_station_name.c_str(), example->_out_port, example->_inner_port, example->_station_type);
			if (!example->initialize())
			{
				log_msg4("%s(%d | %d) %d 无法启动", example->_station_name.c_str(), example->_out_port, example->_inner_port, example->_station_type);
				return;
			}
			log_msg4("%s(%d | %d) %d 正在运行", example->_station_name.c_str(), example->_out_port, example->_inner_port, example->_station_type);
			zmq_threadstart(plan_poll, example);
			const bool reStrart = example->poll();
			while (example->_in_plan_poll)
			{
				sleep(1);
			}
			//发送关闭消息
			{
				acl::string msg;
				msg.format("station_end\r\n%s\r\n%d\r\n", example->_station_name.c_str(), example->_inner_port);
				send_late(example->_inner_socket, msg.c_str());
				thread_sleep(100);
			}
			example->destruct();
			if (reStrart)
			{
				zmq_threadstart(launch, nullptr);
			}
			else
			{
				log_msg3("%s(%d | %d)已关闭", example->_station_name.c_str(), example->_out_port, example->_inner_port);
			}
			delete example;
			example = nullptr;
		}
	}
}
