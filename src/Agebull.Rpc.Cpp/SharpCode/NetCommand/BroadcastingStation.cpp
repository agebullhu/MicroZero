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

		/**
		*@brief 发布消息
		*/
		bool BroadcastingStationBase::publish(string publiher, string title, string arg) const
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (!can_do() || publiher.length() == 0)
				return false;
			vector<sharp_char> response;
			vector<string> argument;
			argument.push_back(title);
			argument.push_back(arg);
			RequestSocket<ZMQ_REQ, false> socket(publiher.c_str(), _station_name.c_str());
			return socket.request(argument, response);
		}

		/**
		* @brief 处理请求
		*/
		void BroadcastingStationBase::request(ZMQ_HANDLE socket)
		{
			vector<sharp_char> list;
			//0 路由到的地址 1 空帧 2 标题 3 内容
			_zmq_state = recv(socket, list);
			if (_zmq_state != ZmqSocketState::Succeed)
			{
				log_error3("接收消息失败%s(%d)%s", _station_name.c_str(), _inner_port, state_str(_zmq_state));
				return;
			}
			acl::string str(list[2]);
			str.append(" ").append(list[0]).append(" ").append(list[3]);

			_zmq_state = send_late(_inner_socket, str.c_str());

		}
		SystemMonitorStation* SystemMonitorStation::example = nullptr;
	}
}
