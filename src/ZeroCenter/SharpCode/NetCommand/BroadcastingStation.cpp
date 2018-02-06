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
		system_monitor_station* system_monitor_station::example = nullptr;

		/**
		*\brief 发布消息
		*/
		bool broadcasting_station_base::pub_data(const string& publiher, const string& title, const string& arg)
		{
			sharp_char data;
			data.alloc(4 + publiher.length() + title.length() + arg.length());
			sprintf_s(data.get_buffer(), data.size() + 1, "%s\r\n%s\r\n%s", title.c_str(), publiher.c_str(), arg.c_str());
			_zmq_state = send_late(_inner_socket, *data);
			return _zmq_state == ZmqSocketState::Succeed;
		}

		/**
		*\brief 发布消息
		*/
		bool broadcasting_station_base::pub_data(const string& publiher, const string& line)
		{
			_zmq_state = send_late(_inner_socket, line.c_str());
			return _zmq_state == ZmqSocketState::Succeed;
		}

		/**
		*\brief 发布消息
		*/
		bool broadcasting_station_base::publish(const string& publiher, const string& title, const string& arg) const
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
		*\brief 发布消息
		*/
		bool broadcasting_station_base::publish(const string& publiher, const string& title, const string& plan, const string& arg) const
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
		* \brief 处理请求
		*/
		void broadcasting_station_base::request(ZMQ_HANDLE socket)
		{
			vector<sharp_char> list;
			//0 路由到的地址 1 空帧 2 标题 3内容
			_zmq_state = recv(socket, list);
			if (_zmq_state != ZmqSocketState::Succeed)
			{
				log_error3("接收消息失败%s(%d)%s", _station_name.c_str(), _inner_port, state_str(_zmq_state));
				return;
			}
			if (list[2][0] == '@')//计划类型
			{
				save_plan(socket, list);
				return;
			}
			switch (list.size())
			{
			case 3:
			{
				pub_data(list[0], list[2]);
				_zmq_state = send_addr(socket, *list[0]);
				_zmq_state = send_late(socket, "+pub");
				return;
			}
			case 4:
				pub_data(list[0], list[2], list[3]);
				_zmq_state = send_addr(socket, *list[0]);
				_zmq_state = send_late(socket, "+pub");
				return;
			default:
				_zmq_state = send_addr(socket, *list[0]);
				_zmq_state = send_late(socket, "-err");
				break;
			}
		}

		void broadcasting_station::launch(void* arg)
		{
			broadcasting_station* station = static_cast<broadcasting_station*>(arg);
			if (!station_warehouse::join(station))
			{
				return;
			}
			if (!station->do_initialize())
				return;

			zmq_threadstart(plan_poll, arg);
			bool reStrart = station->poll();
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
				station2->_zmq_state = ZmqSocketState::Again;
				zmq_threadstart(launch, station2);
			}
			else
			{
				log_msg3("Station:%s(%d | %d) closed", station->_station_name.c_str(), station->_out_port, station->_inner_port);
			}
			delete station;
		}

		/**
		*\brief 发布消息
		*/
		bool broadcasting_station::publish(string station, string publiher, string title, string arg)
		{
			auto pub = station_warehouse::find(std::move(station));
			if (pub == nullptr || pub->get_station_type() != STATION_TYPE_PUBLISH)
				return false;
			return static_cast<broadcasting_station_base*>(pub)->publish(std::move(publiher), std::move(title), std::move(arg));
		}

		void system_monitor_station::launch(void*)
		{
			example = new system_monitor_station();
			if (!station_warehouse::join(example))
			{
				delete example;
				return;
			}
			if (!example->do_initialize())
				return;

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
				log_msg3("Station:%s(%d | %d) closed", example->_station_name.c_str(), example->_out_port, example->_inner_port);
			}
			delete example;
			example = nullptr;
		}
	}
}
