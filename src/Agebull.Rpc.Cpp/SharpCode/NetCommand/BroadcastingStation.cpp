/**
 * ZMQ广播代理类
 *
 *
 */

#include "stdafx.h"
#include "BroadcastingStation.h"

namespace agebull
{
	namespace zmq_net
	{
#define send_result(code,type,msg,addr)\
	log_error3("%s(%s),正在%s",msg, addr,code);\
	response[0] = type;\
	result = zmq_send(_out_socket, response, 1, ZMQ_DONTWAIT);\
	if(result <= 0)\
	{\
		if (!check_zmq_error(_zmq_state, request_address, code))\
			break;\
		continue;\
	}\
	log_debug2(DEBUG_REQUEST, 6, "%(%s)成功",code, request_address);

		/**
		*消息泵
		*/
		bool BroadcastingStation::publish(string station, string publiher, string title, string arg)
		{
			auto pub = StationWarehouse::find(station);
			if (pub == nullptr || pub->get_station_type() != STATION_TYPE_PUBLISH)
				return false;
			return static_cast<BroadcastingStationBase*>(pub)->publish(publiher, title, arg);
		}

		/**
		*消息泵
		*/
		bool BroadcastingStationBase::publish(string publiher, string title, string arg)
		{
			PublishItem item;
			item.publiher = publiher;
			item.title = title;
			item.arg = arg;
			items.push(item);
			_pub_semaphore.post();
			return true;
		}

		/**
		* \brief
		* \return
		*/
		bool BroadcastingStationBase::poll_pub()
		{
			_inner_socket = create_socket(ZMQ_PUB, _inner_address.c_str());
			//登记线程开始
			int state = 0;
			set_command_thread_start();
			{
				string str;
				str = str + "publish_start " + _station_name + " " + _inner_address;
				thread_sleep(1000);
				state = s_send(_inner_socket, str.c_str());
			}
			while (can_do())
			{
				boost::posix_time::ptime now(boost::posix_time::microsec_clock::universal_time());
				if (!_pub_semaphore.timed_wait(now + boost::posix_time::seconds(1)) || items.empty())
					continue;
				auto item = items.front();
				items.pop();
				item.title = item.title + " " + item.publiher + " " + item.arg;
				state = s_send(_inner_socket, item.title.c_str());
			}
			if (state == 0)
			{
				string str;
				str = str + "station_end " + _station_name + " " + _inner_address;
				state = s_send(_inner_socket, str.c_str());
			}
			close_socket(_inner_socket, _inner_address);
			//登记线程关闭
			set_command_thread_end();
			return true;
		}
		/**
		* @brief 处理反馈
		*/
		void BroadcastingStationBase::response()
		{

		}
		/**
		* @brief 处理请求
		*/
		void BroadcastingStationBase::request()
		{
			char* client_addr = s_recv(_out_socket, 0);
			recv_empty(_out_socket);
			char* title = s_recv(_out_socket);
			recv_empty(_out_socket);
			char* content = s_recv(_out_socket);

			publish(client_addr, title, content);

			free(client_addr);
			free(title);
			free(content);
		}
		SystemMonitorStation* SystemMonitorStation::example = nullptr;
	}
}