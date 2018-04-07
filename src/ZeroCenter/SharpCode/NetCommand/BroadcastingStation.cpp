/**
 * ZMQ广播代理类
 *
 *
 */

#include "stdafx.h"
#include "BroadcastingStation.h"
 #include <utility>
#include "StationWarehouse.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		 * \brief 单例
		 */
		SystemMonitorStation* SystemMonitorStation::example = nullptr;

		/**
		*\brief 发布消息
		*/
		bool BroadcastingStationBase::publish(const sharp_char& title, const sharp_char& description, vector<sharp_char>& datas)
		{
			send_more(_inner_socket, *title);
			send_more(_inner_socket, *description);
			_zmq_state = send(_inner_socket, datas);
			return _zmq_state == ZmqSocketState::Succeed;
		}

		/**
		*\brief 发布消息
		*/
		bool BroadcastingStation::publish(string station, string publiher, string title, string sub, string arg)
		{
			auto pub = StationWarehouse::find(std::move(station));
			if (pub == nullptr || pub->get_station_type() != STATION_TYPE_PUBLISH)
				return false;
			return static_cast<BroadcastingStationBase*>(pub)->publish(std::move(publiher), std::move(title), std::move(sub), std::move(arg));
		}

		/**
		*\brief 发布消息
		*/
		bool BroadcastingStationBase::publish(const string& publiher, const string& title, const string& arg)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (!can_do() || publiher.length() == 0)
				return false;
			sharp_char description;
			description.alloc(6);
			char* buf = description.get_buffer();
			buf[0] = 2;
			buf[1] = zero_pub_publisher;
			buf[3] = zero_arg;
			buf[4] = zero_end;
			vector<sharp_char> datas;
			datas.push_back(title.c_str());
			datas.push_back(description);
			datas.push_back(publiher.c_str());
			datas.push_back(arg.c_str());
			return send_data(datas);
		}
		/**
		*\brief 发布消息
		*/
		bool BroadcastingStationBase::publish(const string& publiher, const string& title, const string& sub, const string& arg) 
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
			datas.push_back(title.c_str());
			datas.push_back(description);
			datas.push_back(publiher.c_str());
			datas.push_back(sub.c_str());
			datas.push_back(arg.c_str());
			return send_data(datas);
		}

		/**
		*\brief 发布消息
		*/
		bool BroadcastingStationBase::publish(const string& publiher, const string& title, const string& sub, const string& plan, const string& arg) const
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (!can_do() || publiher.length() == 0)
				return false;
			Message message;
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
			message.messages.push_back(title.c_str());
			datas.push_back(message.messages_description);
			datas.push_back(publiher.c_str());
			datas.push_back(sub.c_str());
			datas.push_back(arg.c_str());

			message.read_plan(plan.c_str());

			return plan_next(message, true);
		}
		/**
		* \brief 处理请求
		*/
		void BroadcastingStationBase::request(ZMQ_HANDLE socket)
		{
			vector<sharp_char> list;
			//0 路由到的地址 1 空帧 2 标题 3内容标签 4..n 内容
			_zmq_state = recv(socket, list);
			if (_zmq_state != ZmqSocketState::Succeed)
			{
				log_error3("接收消息失败%s(%d)%s", _station_name.c_str(), _inner_port, state_str(_zmq_state));
				return;
			}
			const auto caller = list[0];
			if(list.size() <= 3 || list[3][0] >= list[3].size() || list[3][0] - 4 >= list.size())
			{
				_zmq_state = send_addr(socket, *caller);
				_zmq_state = send_late(socket, "err");
				return;
			}
			auto title = list[2];
			sharp_char description = list[3];
			sharp_char plan,sub,arg, publisher,id;
			const auto buf = description.get_buffer();
			for(int idx = 1;idx <= buf[0];idx++)
			{
				const int idx2 = 1 + idx;
				if(idx2 >= list.size())
				{
					_zmq_state = send_addr(socket, *caller);
					_zmq_state = send_late(socket, "err");
					return;
				}
				switch(buf[idx])
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
			int cnt = 1;
			vector<sharp_char> data;
			buf[++cnt] = zero_pub_publisher;
			data.push_back(caller);
			if (!id.empty())
			{
				buf[++cnt] = zero_request_id;
				data.push_back(id);
			}
			if (!sub.empty())
			{
				buf[++cnt] = zero_pub_sub;
				data.push_back(sub);
			}
			if (!arg.empty())
			{
				buf[++cnt] = zero_arg;
				data.push_back(arg);
			}
			buf[0] = cnt;
			buf[++cnt] = zero_end;

			Message message;
			message.messages.push_back(title);
			message.messages.push_back(description);
			for (sharp_char& item : data)
			{
				message.messages.push_back(item);
			}
			if(!plan.empty())
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

		void BroadcastingStation::start(void* arg)
		{
			BroadcastingStation* station = static_cast<BroadcastingStation*>(arg);
			if (!StationWarehouse::join(station))
			{
				return;
			}
			if (station->_zmq_state == ZmqSocketState::Succeed)
				log_msg3("%s(%d | %d)正在启动", station->_station_name.c_str(), station->_out_port, station->_inner_port);
			else
				log_msg3("%s(%d | %d)正在重启", station->_station_name.c_str(), station->_out_port, station->_inner_port);
			if (!station->initialize())
			{
				log_msg3("%s(%d | %d)无法启动", station->_station_name.c_str(), station->_out_port, station->_inner_port);
				return;
			}
			log_msg3("%s(%d | %d)正在运行", station->_station_name.c_str(), station->_out_port, station->_inner_port);

			zmq_threadstart(plan_poll, arg);
			bool reStrart = station->poll();
			while (station->_in_plan_poll)
			{
				sleep(1);
			}
			//zmq_threadclose(t);
			StationWarehouse::left(station);
			station->destruct();
			if (reStrart)
			{
				BroadcastingStation* station2 = new BroadcastingStation(station->_station_name);
				station2->_zmq_state = ZmqSocketState::Again;
				zmq_threadstart(start, station2);
			}
			else
			{
				log_msg3("%s(%d | %d)已关闭", station->_station_name.c_str(), station->_out_port, station->_inner_port);
			}
			delete station;
		}


		void SystemMonitorStation::start(void*)
		{
			example = new SystemMonitorStation();
			if (!StationWarehouse::join(example))
			{
				delete example;
				return;
			}
			if (example->_zmq_state == ZmqSocketState::Succeed)
				log_msg3("%s(%d | %d)正在启动", example->_station_name.c_str(), example->_out_port, example->_inner_port);
			else
				log_msg3("%s(%d | %d)正在重启", example->_station_name.c_str(), example->_out_port, example->_inner_port);
			if (!example->initialize())
			{
				log_msg3("%s(%d | %d)无法启动", example->_station_name.c_str(), example->_out_port, example->_inner_port);
				return;
			}
			log_msg3("%s(%d | %d)正在运行", example->_station_name.c_str(), example->_out_port, example->_inner_port);

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
				zmq_threadstart(start, nullptr);
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
