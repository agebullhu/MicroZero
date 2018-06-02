#include "../stdafx.h"
#include "net_command.h"
using namespace std;
namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 配置ZMQ连接对象
		* \param socket
		* \param name
		* \return
		*/
		void set_sockopt(const ZMQ_HANDLE& socket, const char* name)
		{
			int iRcvTimeout;
			if (name != nullptr)
				zmq_setsockopt(socket, ZMQ_IDENTITY, name, strlen(name));
			//char monitor[MAX_PATH];
			//sprintf(monitor,"inproc://_%s_monitor.rep", name);
			//int zmq_result = zmq_socket_monitor(socket, monitor, ZMQ_EVENT_ALL);
			//assert(zmq_result == 0);
			int iZMQ_IMMEDIATE = 1;//列消息只作用于已完成的链接
			zmq_setsockopt(socket, ZMQ_IMMEDIATE, &iZMQ_IMMEDIATE, sizeof(int));
			int iLINGER = 500;//关闭设置停留时间,毫秒
			zmq_setsockopt(socket, ZMQ_LINGER, &iLINGER, sizeof(int));
			iRcvTimeout = 3000;
			zmq_setsockopt(socket, ZMQ_RCVTIMEO, &iRcvTimeout, sizeof(int));
			int iHwm = 4096;
			zmq_setsockopt(socket, ZMQ_SNDHWM, &iHwm, sizeof(int));
			zmq_setsockopt(socket, ZMQ_RCVHWM, &iHwm, sizeof(int));
			int iBuf =  0xFFFFFF;
			zmq_setsockopt(socket, ZMQ_SNDBUF, &iBuf, sizeof(int));
			zmq_setsockopt(socket, ZMQ_RCVBUF, &iBuf, sizeof(int));
			int iSndTimeout = 3000;
			zmq_setsockopt(socket, ZMQ_SNDTIMEO, &iSndTimeout, sizeof(int));
			int iBackLog = 10000;
			zmq_setsockopt(socket, ZMQ_BACKLOG, &iBackLog, sizeof(int));
		}

		/**
		* \brief 生成用于TCP的套接字
		*/
		bool set_tcp_nodelay(ZMQ_HANDLE socket)
		{
			//boost::asio::detail::socket_type fd = 0;
			//size_t sz = sizeof(boost::asio::detail::socket_type);
			//int re = zmq_getsockopt(socket, ZMQ_USE_FD, &fd, &sz);
			//const char nodelay = 1;

			//boost::asio::detail::socket_ops::state_type type;
			//boost::system::error_code err;
			//re = boost::asio::detail::socket_ops::setsockopt(fd, type,IPPROTO_TCP, TCP_NODELAY, &nodelay, sizeof(char), err);
			//	
			//	return re == 0;
			return true;
		}
		typedef struct
		{
			ushort event;
			int value;
			char address[256];
		}zmq_event_t;

		int read_event_msg(void* s, zmq_event_t* event)
		{
			zmq_msg_t msg1;  // binary part
			zmq_msg_init(&msg1);
			int rc = zmq_msg_recv(&msg1, s, 0);
			if (rc == -1)
				return 1;
			const char* data = static_cast<char*>(zmq_msg_data(&msg1));
			memcpy(&(event->event), data, sizeof(event->event));
			memcpy(&(event->value), data + sizeof(event->event), sizeof(event->value));

			zmq_msg_t msg2;  //  address part
			zmq_msg_init(&msg2);
			//assert(zmq_msg_more(&msg1) != 0);
			rc = zmq_msg_recv(&msg2, s, 0);
			if (rc == -1)
				return 0;
			//assert(zmq_msg_more(&msg2) == 0);
			// copy binary data to event struct
			// copy address part
			const size_t len = zmq_msg_size(&msg2);
			memcpy(event->address, zmq_msg_data(&msg2), len);
			event->address[len] = '\0';
			return 0;
		}
		//网络监控
		DWORD zmq_monitor(const char * address)
		{
			zmq_event_t event;
			printf("starting monitor...\n");
			void* inproc = zmq_socket(get_zmq_context(), ZMQ_PAIR);
			assert(inproc);
			int iRcvTimeout = 1000;
			zmq_setsockopt(inproc, ZMQ_RCVTIMEO, &iRcvTimeout, sizeof(iRcvTimeout));
			zmq_connect(inproc, address);
			while (get_net_state() == NET_STATE_RUNING)
			{
				if (read_event_msg(inproc, &event) == 1)
					continue;
				switch (event.event)
				{
				case ZMQ_EVENT_CLOSED:
					log_debug1(DEBUG_BASE, 1, "ZMQ网络监控%d:连接已关闭", event.value);
					zmq_close(inproc);
					return 0;
				case ZMQ_EVENT_CLOSE_FAILED:
					log_error1("ZMQ网络监控%d:连接关闭失败", event.value);
					zmq_close(inproc);
					return 0;
				case ZMQ_EVENT_MONITOR_STOPPED:
					log_debug1(DEBUG_BASE, 1, "ZMQ网络监控%d:监控关闭", event.value);
					zmq_close(inproc);
					return 0;
				case ZMQ_EVENT_LISTENING:
					log_debug1(DEBUG_BASE, 1, "ZMQ网络监控%d:正在侦听数据", event.value);
					break;
				case ZMQ_EVENT_BIND_FAILED:
					log_debug2(DEBUG_BASE, 1, "ZMQ网络监控%d:绑定端口失败%s", event.value, event.address);
					break;
				case ZMQ_EVENT_ACCEPTED:
					log_debug2(DEBUG_BASE, 1, "ZMQ网络监控%d:接收到%s的数据", event.value, event.address);
					break;
				case ZMQ_EVENT_ACCEPT_FAILED:
					log_error2("ZMQ网络监控%d:接收%s的数据出错", event.value, event.address);
					break;
				case ZMQ_EVENT_CONNECTED:
					log_debug2(DEBUG_BASE, 1, "ZMQ网络监控%d:与%s连接成功", event.value, event.address);
					break;
				case ZMQ_EVENT_CONNECT_DELAYED:
					log_debug2(DEBUG_BASE, 1, "ZMQ网络监控%d:与%s连接发生延迟", event.value, event.address);
					break;
				case ZMQ_EVENT_CONNECT_RETRIED:
					log_debug2(DEBUG_BASE, 1, "ZMQ网络监控%d:重新连接%s", event.value, event.address);
					break;
				case ZMQ_EVENT_DISCONNECTED:
					log_debug2(DEBUG_BASE, 1, "ZMQ网络监控%d:与%s连接关闭", event.value, event.address);
					break;
				default: break;
				}
			}
			zmq_close(inproc);
			return 0;
		}
	}
}