#include "../stdafx.h"
#include "net_command.h"
#pragma unmanaged
using namespace std;
namespace agebull
{
	typedef struct
	{
		ushort event;
		int value;
		char address[256];
	}zmq_event_t;

	int read_event_msg(void* s, zmq_event_t* event)
	{
		//int rc = zmq_socket_monitor(rep, "inproc://monitor.rep", ZMQ_EVENT_ALL);
		//    assert(rc == 0);
		int rc;
		zmq_msg_t msg1;  // binary part
		zmq_msg_init(&msg1);
		rc = zmq_msg_recv(&msg1, s, 0);
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
		size_t len = zmq_msg_size(&msg2);
		memcpy(event->address, zmq_msg_data(&msg2), len);
		event->address[len] = '\0';
		return 0;
	}
	//网络监控
#ifdef CLIENT_COMMAND
	DWORD WINAPI zmq_monitor(LPVOID arg)
	{
		char * address = static_cast<char*>(arg);
#else
	DWORD zmq_monitor(const char * address)
	{
#endif
		zmq_event_t event;
		printf("starting monitor...\n");
		void* inproc = zmq_socket(get_zmq_context(), ZMQ_PAIR);
		assert(inproc);
		int iRcvTimeout = 1000;
		zmq_setsockopt(inproc, ZMQ_RCVTIMEO, &iRcvTimeout, sizeof(iRcvTimeout));
		int rc = zmq_connect(inproc, address);
		assert(rc == 0);
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