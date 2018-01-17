/**
 * ZMQ广播代理类
 *
 *
 */

#include "stdafx.h"
#include "BroadcastingStation.h"
#include "StationWarehouse.h"

namespace agebull
{
	namespace zmq_net
	{
		SystemMonitorStation* SystemMonitorStation::example = nullptr;

		/**
		*@brief 发布消息
		*/
		bool BroadcastingStationBase::pub_data(string publiher, string title, string arg)
		{
			sharp_char data;
			data.alloc(4 + publiher.length() + title.length() + arg.length());
			sprintf_s(data.get_buffer(), data.size() + 1, "%s\r\n%s\r\n%s", title.c_str(), publiher.c_str(), arg.c_str());
			_zmq_state = send_late(_inner_socket, *data);
			return _zmq_state == ZmqSocketState::Succeed;
		}

		/**
		*@brief 发布消息
		*/
		bool BroadcastingStationBase::pub_data(string publiher, string line)
		{
			_zmq_state = send_late(_inner_socket, line.c_str());
			return _zmq_state == ZmqSocketState::Succeed;
		}

		/**
		*@brief 发布消息
		*/
		bool BroadcastingStationBase::publish(string publiher, string title, string arg) const
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (!can_do() || publiher.length() == 0)
				return false;
			vector<sharp_char> result;
			vector<string> argument;
			argument.push_back(title);
			argument.push_back(arg);
			RequestSocket<ZMQ_REQ, false> socket(publiher.c_str(), _station_name.c_str());
			return socket.request(argument, result);
		}

		/**
		*@brief 发布消息
		*/
		bool BroadcastingStationBase::publish(string publiher, string title, string plan, string arg) const
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (!can_do() || publiher.length() == 0)
				return false;
			vector<sharp_char> result;
			vector<string> argument;
			argument.push_back(title);
			argument.push_back(plan);
			argument.push_back(arg);
			RequestSocket<ZMQ_REQ, false> socket(publiher.c_str(), _station_name.c_str());
			return socket.request(argument, result);
		}

		/**
		* @brief 处理请求
		*/
		void BroadcastingStationBase::request(ZMQ_HANDLE socket)
		{
			vector<sharp_char> list;
			//0 路由到的地址 1 空帧 2 标题 3时间 4 内容
			_zmq_state = recv(socket, list);
			if (_zmq_state != ZmqSocketState::Succeed)
			{
				log_error3("接收消息失败%s(%d)%s", _station_name.c_str(), _inner_port, state_str(_zmq_state));
				return;
			}
			switch (list.size())
			{
			case 3:
			{
				pub_data(list[0], list[2]);
				_zmq_state = send_addr(socket, *list[0]);
				_zmq_state = send_late(socket, "ok");
				return;
			}
			case 4:
			{
				pub_data(list[0], list[2], list[3]);
				_zmq_state = send_addr(socket, *list[0]);
				_zmq_state = send_late(socket, "ok");
				return;
			}
			default:
				if (list[3].empty())
				{
					pub_data(list[0], list[2], list[3]);
					_zmq_state = send_addr(socket, *list[0]);
					_zmq_state = send_late(socket, "ok");
					return;
				}
				break;
			}
			Message message;
			acl::json json;
			json.update(*list[3]);
			acl::json_node* iter = json.first_node();
			while (iter)
			{
				int idx = strmatchi(5, iter->tag_name(), "plan_type", "plan_value", "plan_repet");
				switch (idx)
				{
				case 0:
					message.plan_type = static_cast<plan_date_type>(static_cast<int>(*iter->get_int64()));
					break;
				case 1:
					message.plan_value = static_cast<int>(*iter->get_int64());
					break;
				case 2:
					message.plan_repet = static_cast<int>(*iter->get_int64());
					break;
				default: break;
				}
				iter = json.next_node();
			}
			message.request_caller = list[0];
			message.messages.push_back(list[2]);
			message.messages.push_back(list[4]);
			plan_next(message, true);
			_zmq_state = send_addr(socket, *list[0]);
			_zmq_state = send_late(socket, "plan");
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

		/**
		*@brief 发布消息
		*/
		bool BroadcastingStation::publish(string station, string publiher, string title, string arg)
		{
			auto pub = StationWarehouse::find(station);
			if (pub == nullptr || pub->get_station_type() != STATION_TYPE_PUBLISH)
				return false;
			return static_cast<BroadcastingStationBase*>(pub)->publish(publiher, title, arg);
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
			bool reStrart = example->poll();
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
