#include "../stdafx.h"
#include "zmq_extend.h"
#include <arpa/inet.h>
using namespace std;
namespace agebull
{
	namespace zmq_net
	{
		namespace socket_ex
		{
			/**
			* \brief 配置ZMQ连接对象
			* \param socket
			* \param name
			* \return
			*/
			void set_sockopt(ZMQ_HANDLE& socket, const char* name)
			{
				if (name != nullptr)
					zmq_setsockopt(socket, ZMQ_IDENTITY, name, strlen(name));
				//assert(zmq_result == 0);
				//列消息只作用于已完成的链接
				if (json_config::IMMEDIATE >= 0)
					setsockopt(socket, ZMQ_IMMEDIATE, json_config::IMMEDIATE);
				//关闭设置停留时间,毫秒
				if (json_config::LINGER >= 0)
					setsockopt(socket, ZMQ_LINGER, json_config::LINGER);
				if (json_config::RCVTIMEO >= 0)
					setsockopt(socket, ZMQ_RCVTIMEO, json_config::RCVTIMEO);
				if (json_config::SNDHWM >= 0)
					setsockopt(socket, ZMQ_SNDHWM, json_config::SNDHWM);
				if (json_config::RCVHWM >= 0)
					setsockopt(socket, ZMQ_RCVHWM, json_config::RCVHWM);
				if (json_config::SNDBUF >= 0)
					setsockopt(socket, ZMQ_SNDBUF, json_config::SNDBUF);
				if (json_config::RCVBUF >= 0)
					setsockopt(socket, ZMQ_RCVBUF, json_config::RCVBUF);
				if (json_config::SNDTIMEO >= 0)
					setsockopt(socket, ZMQ_SNDTIMEO, json_config::SNDTIMEO);
				if (json_config::BACKLOG >= 0)
					setsockopt(socket, ZMQ_BACKLOG, json_config::BACKLOG);
				if (json_config::SNDTIMEO >= 0)
					setsockopt(socket, ZMQ_SNDTIMEO, json_config::SNDTIMEO);
				if (json_config::RCVTIMEO >= 0)
					setsockopt(socket, ZMQ_RCVTIMEO, json_config::RCVTIMEO);

				//setsockopt(socket, ZMQ_HEARTBEAT_IVL, 10000);
				//setsockopt(socket, ZMQ_HEARTBEAT_TIMEOUT, 200);
				//setsockopt(socket, ZMQ_HEARTBEAT_TTL, 200);

				//setsockopt(socket, ZMQ_TCP_KEEPALIVE, 1);
				//setsockopt(socket, ZMQ_TCP_KEEPALIVE_IDLE, 1024);
				//setsockopt(socket, ZMQ_TCP_KEEPALIVE_INTVL, 1024);

			}

			ZMQ_HANDLE create_req_socket(const char* addr, int type, const char* name)
			{
				log_msg3("create_req_socket: %s(%d) > %s", name, type, addr);
				ZMQ_HANDLE socket = zmq_socket(get_zmq_context(), type);
				if (socket == nullptr)
				{
					return nullptr;
				}
				set_sockopt(socket, name);
				if (zmq_connect(socket, addr) >= 0)
					return socket;
				zmq_close(socket);
				return nullptr;
			}

			ZMQ_HANDLE create_res_socket(const char* addr, int type, const char* name)
			{
				log_msg3("create_res_socket : %s(%d) > %s", name, type, addr);
				ZMQ_HANDLE socket = zmq_socket(get_zmq_context(), type);
				if (socket == nullptr)
				{
					return nullptr;
				}
				set_sockopt(socket, name);
				if (zmq_bind(socket, addr) >= 0)
					return socket;
				zmq_close(socket);
				return nullptr;
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

			void close_res_socket(ZMQ_HANDLE& socket, const char* addr)
			{
				zmq_unbind(socket, addr);
				while (zmq_close(socket) == -1)
					log_error(state_str(check_zmq_error()));
				socket = nullptr;
			}

			void close_req_socket(ZMQ_HANDLE& socket, const char* addr)
			{
				zmq_disconnect(socket, addr);
				while (zmq_close(socket) == -1)
					log_error(state_str(check_zmq_error()));
				socket = nullptr;
			}
		}
		namespace zmq_monitor
		{
			//网络监控
			void do_monitor(shared_char addr)
			{
				zmq_event_t event;
				printf("starting monitor...\n");
				void* inproc = zmq_socket(get_zmq_context(), ZMQ_PAIR);
				assert(inproc);
				agebull::zmq_net::socket_ex::setsockopt(inproc, ZMQ_RCVTIMEO, 1000);
				zmq_connect(inproc, *addr);
				while (get_net_state() < NET_STATE_DISTORY)
				{
					if (read_event_msg(inproc, &event) == 1)
						continue;
					switch (event.event)
					{
					case ZMQ_EVENT_CLOSED:
						log_debug1(DEBUG_BASE, 1, "ZMQ网络监控%d:连接已关闭", event.value);
						zmq_close(inproc);
						return;
					case ZMQ_EVENT_CLOSE_FAILED:
						log_error1("ZMQ网络监控%d:连接关闭失败", event.value);
						zmq_close(inproc);
						return;
					case ZMQ_EVENT_MONITOR_STOPPED:
						log_debug1(DEBUG_BASE, 1, "ZMQ网络监控%d:监控关闭", event.value);
						zmq_close(inproc);
						return;
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
			}
			//网络监控
			void zmq_monitor(shared_char addr)
			{
				boost::thread thread_xxx(boost::bind(&do_monitor, addr));
			}

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
		}
	}
}