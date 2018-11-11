#include "../stdafx.h"
#include "zmq_extend.h"
#include <arpa/inet.h>
using namespace std;
namespace agebull
{
	namespace zero_net
	{
		namespace socket_ex
		{
			/**
			* \brief 配置ZMQ连接对象
			* \param socket
			* \param name
			* \return
			*/
			void set_sockopt(zmq_handler& socket, const char* name)
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
				if (json_config::MAX_MSGSZ > 0)
					setsockopt(socket, ZMQ_MAXMSGSIZE, json_config::MAX_MSGSZ);
				
				//setsockopt(socket, ZMQ_HEARTBEAT_IVL, 10000);
				//setsockopt(socket, ZMQ_HEARTBEAT_TIMEOUT, 200);
				//setsockopt(socket, ZMQ_HEARTBEAT_TTL, 200);

				//setsockopt(socket, ZMQ_TCP_KEEPALIVE, 1);
				//setsockopt(socket, ZMQ_TCP_KEEPALIVE_IDLE, 1024);
				//setsockopt(socket, ZMQ_TCP_KEEPALIVE_INTVL, 1024);

			}

			zmq_handler create_req_socket(const char* station, int type, const char* addr, const char* name)
			{
				log_msg4("[%s] : create_req_socket(%d) > %s > %s", station,type, name, addr);
				zmq_handler socket = zmq_socket(get_zmq_context(), type);
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

			zmq_handler create_res_socket(const char* station, const char* addr, int type, const char* name)
			{
				log_msg3("[%s] : create_res_socket(%d) > %s", station, type, addr);
				zmq_handler socket = zmq_socket(get_zmq_context(), type);
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
			bool set_tcp_nodelay(zmq_handler socket)
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

			void close_res_socket(zmq_handler& socket, const char* addr)
			{
				zmq_unbind(socket, addr);
				while (zmq_close(socket) == -1)
					log_error(state_str(check_zmq_error()));
				socket = nullptr;
			}

			void close_req_socket(zmq_handler& socket, const char* addr)
			{
				zmq_disconnect(socket, addr);
				while (zmq_close(socket) == -1)
					log_error(state_str(check_zmq_error()));
				socket = nullptr;
			}
		}

		namespace zmq_monitor
		{

			typedef struct
			{
				ushort event;
				int value;
				char address[256];
			}zmq_event_t;

			int read_event_msg(void* s, zmq_event_t* event);

			/**
			* \brief 网络监控
			* \para {int} _fd Socket句柄
			*/
			bool print_client_addr(int _fd, acl::string& str)
			{
				socklen_t          peer_addr_size;
				struct sockaddr_in peer_addr;
				peer_addr_size = sizeof(struct sockaddr);
				if (getpeername(_fd, reinterpret_cast<struct sockaddr*>(&peer_addr), &peer_addr_size) != 0)
					return false;
				char esme_ip[200];
				str.format("%s:%d(%d)",
					inet_ntop(peer_addr.sin_family, &peer_addr.sin_addr, esme_ip, sizeof(esme_ip)),
					ntohs(peer_addr.sin_port),
					_fd);
				return true;
			}

			/**
			* \brief 网络监控
			* \param {shared_char} station 站点名称
			* \param {ZMQ_HANDLE} socket socket
			* \param {shared_char} addr 地址
			*/
			void set_monitor(const char* station, zmq_handler socket, const char* type)
			{
				shared_char addr(256);
				sprintf(addr.get_buffer(), "inproc://%s_%s_monitor.rep", station, type);
				zmq_socket_monitor(socket, *addr, ZMQ_EVENT_ALL);
				boost::thread thread_xxx(boost::bind(&zmq_monitor::do_monitor, station, addr));
			}

			/**
			* \brief 网络监控
			* \param {shared_char} station 站点名称
			* \param {shared_char} addr 地址
			*/
			void do_monitor(shared_char station, shared_char addr)
			{
				acl::string str;
				zmq_event_t event;
				log_msg2("[%s] : monitor > starting > %s", station.c_str(), addr.c_str());
				void* inproc = zmq_socket(get_zmq_context(), ZMQ_PAIR);
				assert(inproc);
				agebull::zero_net::socket_ex::setsockopt(inproc, ZMQ_RCVTIMEO, 1000);
				zmq_connect(inproc, *addr);
				while (get_net_state() < net_state_distory)
				{
					if (read_event_msg(inproc, &event) == 1)
						continue;
					switch (event.event)
					{
					case ZMQ_EVENT_CLOSED:
						log_msg1("[%s] : monitor > 连接关闭", station.c_str());
						zmq_close(inproc);
						return;
					case ZMQ_EVENT_CLOSE_FAILED:
						log_msg1("[%s] : monitor > 关闭失败", station.c_str());
						zmq_close(inproc);
						return;
					case ZMQ_EVENT_MONITOR_STOPPED:
						log_msg1("[%s] : monitor > 监控关闭", station.c_str());
						zmq_close(inproc);
						return;
					case ZMQ_EVENT_LISTENING:
						log_msg1("[%s] : monitor > 正在侦听", station.c_str());
						break;
					case ZMQ_EVENT_BIND_FAILED:
						log_msg2("[%s] : monitor > 绑定失败 > %s", station.c_str(), event.address);
						break;
					case ZMQ_EVENT_ACCEPTED:
						print_client_addr(event.value, str);
						log_msg2("[%s] : monitor > 客户端连接 > %s", station.c_str(), str.c_str());
						break;
					case ZMQ_EVENT_DISCONNECTED:
						print_client_addr(event.value, str);
						log_msg2("[%s] : monitor > 客户端断开 > %s", station.c_str(), str.c_str());
						break;
					case ZMQ_EVENT_ACCEPT_FAILED:
						print_client_addr(event.value, str);
						log_msg2("[%s] : monitor > 客户端连接出错 > %s", station.c_str(), str.c_str());
						break;
					case ZMQ_EVENT_CONNECTED:
						log_msg2("[%s] : monitor > 连接成功 > %s", station.c_str(), event.address);
						break;
					case ZMQ_EVENT_CONNECT_DELAYED:
						log_msg2("[%s] : monitor > 延迟连接 > %s", station.c_str(), event.address);
						break;
					case ZMQ_EVENT_CONNECT_RETRIED:
						log_msg2("[%s] : monitor > 重新连接 > %s", station.c_str(), event.address);
						break;
					default: break;
					}
				}
				zmq_close(inproc);
			}
			int read_event_msg(void* s, zmq_event_t* event)
			{
				memset(event, 0, sizeof(zmq_event_t));
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