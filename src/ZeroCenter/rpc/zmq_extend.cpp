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
			* \param identity
			* \return
			*/
			void set_sockopt(zmq_handler& socket, const char* identity)
			{
				if (identity != nullptr)
					zmq_setsockopt(socket, ZMQ_IDENTITY, identity, strlen(identity));
				//assert(zmq_result == 0);
				//列消息只作用于已完成的链接
				if (global_config::IMMEDIATE >= 0)
					setsockopt(socket, ZMQ_IMMEDIATE, global_config::IMMEDIATE);
				//关闭设置停留时间,毫秒
				if (global_config::LINGER >= 0)
					setsockopt(socket, ZMQ_LINGER, global_config::LINGER);
				if (global_config::RCVTIMEO >= 0)
					setsockopt(socket, ZMQ_RCVTIMEO, global_config::RCVTIMEO);
				if (global_config::SNDHWM >= 0)
					setsockopt(socket, ZMQ_SNDHWM, global_config::SNDHWM);
				if (global_config::RCVHWM >= 0)
					setsockopt(socket, ZMQ_RCVHWM, global_config::RCVHWM);
				if (global_config::SNDBUF >= 0)
					setsockopt(socket, ZMQ_SNDBUF, global_config::SNDBUF);
				if (global_config::RCVBUF >= 0)
					setsockopt(socket, ZMQ_RCVBUF, global_config::RCVBUF);
				if (global_config::SNDTIMEO >= 0)
					setsockopt(socket, ZMQ_SNDTIMEO, global_config::SNDTIMEO);
				if (global_config::BACKLOG >= 0)
					setsockopt(socket, ZMQ_BACKLOG, global_config::BACKLOG);
				if (global_config::SNDTIMEO >= 0)
					setsockopt(socket, ZMQ_SNDTIMEO, global_config::SNDTIMEO);
				if (global_config::RCVTIMEO >= 0)
					setsockopt(socket, ZMQ_RCVTIMEO, global_config::RCVTIMEO);
				if (global_config::MAX_MSGSZ > 0)
					setsockopt(socket, ZMQ_MAXMSGSIZE, global_config::MAX_MSGSZ);

				//setsockopt(socket, ZMQ_HEARTBEAT_IVL, 10000);
				//setsockopt(socket, ZMQ_HEARTBEAT_TIMEOUT, 200);
				//setsockopt(socket, ZMQ_HEARTBEAT_TTL, 200);

				//setsockopt(socket, ZMQ_TCP_KEEPALIVE, 1);
				//setsockopt(socket, ZMQ_TCP_KEEPALIVE_IDLE, 1024);
				//setsockopt(socket, ZMQ_TCP_KEEPALIVE_INTVL, 1024);

			}

			/**
			* \brief 检查类型的可读名称
			*/
			const char* zmq_type_name(int type)
			{
				switch (type)
				{
				case ZMQ_PAIR:
					return "ZMQ_PAIR";
				case ZMQ_PUB:
					return "ZMQ_PUB";
				case ZMQ_SUB:
					return "ZMQ_SUB";
				case ZMQ_REQ:
					return "ZMQ_REQ";
				case ZMQ_REP:
					return "ZMQ_REP";
				case ZMQ_DEALER:
					return "ZMQ_DEALER";
				case ZMQ_ROUTER:
					return "ZMQ_ROUTER";
				case ZMQ_PULL:
					return "ZMQ_PULL";
				case ZMQ_PUSH:
					return "ZMQ_PUSH";
				case ZMQ_XPUB:
					return "ZMQ_XPUB";
				case ZMQ_XSUB:
					return "ZMQ_XSUB";
				case ZMQ_STREAM:
					return "ZMQ_STREAM";
				default:
					return "ERROR";
				}
			}
			zmq_handler create_req_socket(const char* station, int type, const char* addr, const char* identity)
			{
				log_msg4("[%s] : create_req_socket(%s) > %s > %s", station, zmq_type_name(type), identity, addr);
				zmq_handler socket = zmq_socket(get_zmq_context(), type);
				if (socket == nullptr)
				{
					return nullptr;
				}
				set_sockopt(socket, identity);
				setsockopt(socket, ZMQ_RECONNECT_IVL, 100);
				setsockopt(socket, ZMQ_CONNECT_TIMEOUT, 3000);
				setsockopt(socket, ZMQ_RECONNECT_IVL_MAX, 3000);
				//setsockopt(socket, ZMQ_TCP_KEEPALIVE, 1);
				//setsockopt(socket, ZMQ_TCP_KEEPALIVE_IDLE, 1024);
				//setsockopt(socket, ZMQ_TCP_KEEPALIVE_INTVL, 1024);
				
				int state = zmq_connect(socket, addr);
				if (state == 0)
					return socket;
				zmq_close(socket);
				return nullptr;
			}

			zmq_handler create_res_socket(const char* station, const char* addr, int type)
			{
				log_msg3("[%s] : create_res_socket(%s) > %s", station, zmq_type_name(type), addr);
				zmq_handler socket = zmq_socket(get_zmq_context(), type);
				if (socket == nullptr)
				{
					return nullptr;
				}
				set_sockopt(socket, nullptr);
				if (zmq_bind(socket, addr) == 0)
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
			void set_monitor(const char* station, zmq_handler* socket, const char* type)
			{
				shared_char addr(256);
				sprintf(addr.get_buffer(), "inproc://%s_%s_monitor.inp", station, type);
				zmq_socket_monitor(*socket, *addr, ZMQ_EVENT_ALL);
				boost::thread thread_xxx(boost::bind(&zmq_monitor::do_monitor, station, addr, socket));
			}

			/**
			* \brief 网络监控
			* \param {shared_char} station 站点名称
			* \param {shared_char} addr 地址
			*/
			void do_monitor(shared_char station, shared_char addr, zmq_handler* socket)
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
						print_client_addr(event.value, str);
						log_msg2("[%s] : monitor > 连接关闭 > %s", station.c_str(), str.c_str());
						//zmq_close(inproc);
						//*socket = nullptr;
						break;
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
		/**
		  * \brief 说明帧解析
		  */
		acl::string desc_str(bool in, char* desc, size_t len)
		{
			if (desc == nullptr || len == 0)
				return "[EMPTY]";
			acl::string str;
			str.format_append("{\"size\":%d", desc[0]);
			uchar state = *reinterpret_cast<uchar*>(desc + 1);
			if (in)
			{
				str.append(R"(,"command":")");
				switch (state)
				{
				case ZERO_BYTE_COMMAND_NONE: //!\1 无特殊说明
					str.append("none");
					break;
				case ZERO_BYTE_COMMAND_PLAN: //!\2  取全局标识
					str.append("plan");
					break;
				case ZERO_BYTE_COMMAND_GLOBAL_ID: //!>  
					str.append("global_id");
					break;
				case ZERO_BYTE_COMMAND_WAITING: //!# 等待结果
					str.append("waiting");
					break;
				case ZERO_BYTE_COMMAND_FIND_RESULT: //!% 关闭结果
					str.append("find result");
					break;
				case ZERO_BYTE_COMMAND_CLOSE_REQUEST: //!- Ping
					str.append("close request");
					break;
				case ZERO_BYTE_COMMAND_PING: //!* 心跳加入
					str.append("ping");
					break;
				case ZERO_BYTE_COMMAND_HEART_JOIN: //!J  心跳已就绪
					str.append("heart join");
					break;
				case ZERO_BYTE_COMMAND_HEART_READY: //!R  心跳进行
					str.append("heart ready");
					break;
				case ZERO_BYTE_COMMAND_HEART_PITPAT: //!P  心跳退出
					str.append("heart pitpat");
					break;
				case ZERO_BYTE_COMMAND_HEART_LEFT: //!L  
					str.append("heart left");
					break;
				}
			}
			else
			{
				str.append(R"(,"state":")");
				switch (state)
				{
				case ZERO_STATUS_OK_ID: //!(0x1)
					str.append(ZERO_STATUS_OK);
					break;
				case ZERO_STATUS_PLAN_ID: //!(0x2)
					str.append(ZERO_STATUS_PLAN);
					break;
				case ZERO_STATUS_RUNING_ID: //!(0x3)
					str.append(ZERO_STATUS_RUNING);
					break;
				case ZERO_STATUS_BYE_ID: //!(0x4)
					str.append(ZERO_STATUS_BYE);
					break;
				case ZERO_STATUS_WECOME_ID: //!(0x5)
					str.append(ZERO_STATUS_WECOME);
					break;
				case ZERO_STATUS_VOTE_SENDED_ID: //!(0x20)
					str.append(ZERO_STATUS_VOTE_SENDED);
					break;
				case ZERO_STATUS_VOTE_BYE_ID: //!(0x21)
					str.append(ZERO_STATUS_VOTE_BYE);
					break;
				case ZERO_STATUS_WAIT_ID: //!(0x22)
					str.append(ZERO_STATUS_WAITING);
					break;
				case ZERO_STATUS_VOTE_WAITING_ID: //!(0x22)
					str.append(ZERO_STATUS_WAITING);
					break;
				case ZERO_STATUS_VOTE_START_ID: //!(0x23)
					str.append(ZERO_STATUS_VOTE_START);
					break;
				case ZERO_STATUS_VOTE_END_ID: //!(0x24)
					str.append(ZERO_STATUS_VOTE_END);
					break;
				case ZERO_STATUS_VOTE_CLOSED_ID: //!(0x25)
					str.append(ZERO_STATUS_VOTE_CLOSED);
					break;
				case ZERO_STATUS_ERROR_ID: //!(0x81)
					str.append(ZERO_STATUS_ERROR);
					break;
				case ZERO_STATUS_FAILED_ID: //!(0x82)
					str.append(ZERO_STATUS_FAILED);
					break;
				case ZERO_STATUS_NOT_FIND_ID: //!(0x83)
					str.append(ZERO_STATUS_NOT_FIND);
					break;
				case ZERO_STATUS_NOT_SUPPORT_ID: //!(0x84)
					str.append(ZERO_STATUS_NOT_SUPPORT);
					break;
				case ZERO_STATUS_FRAME_INVALID_ID: //!(0x85)
					str.append(ZERO_STATUS_FRAME_INVALID);
					break;
				case ZERO_STATUS_ARG_INVALID_ID: //!(0x85)
					str.append(ZERO_STATUS_ARG_INVALID);
					break;
				case ZERO_STATUS_TIMEOUT_ID: //!(0x86)
					str.append(ZERO_STATUS_TIMEOUT);
					break;
				case ZERO_STATUS_NET_ERROR_ID: //!(0x87)
					str.append(ZERO_STATUS_NET_ERROR);
					break;
				case ZERO_STATUS_NOT_WORKER_ID: //!(0x88)
					str.append(ZERO_STATUS_NOT_WORKER);
					break;
				case ZERO_STATUS_PLAN_ERROR_ID: //!(0x8B)
					str.append(ZERO_STATUS_PLAN_ERROR);
					break;
				}
			}
			str.append(R"(","frames":[)");

			str.append(R"("Caller","FrameDescr")");
			for (size_t idx = 2; idx < len; idx++)
			{
				switch (desc[idx])
				{
				case ZERO_FRAME_END:
					str.append(",\"End\"");
					break;
					//全局标识
				case ZERO_FRAME_GLOBAL_ID:
					str.append(",\"GLOBAL_ID\"");
					break;
					//站点
				case ZERO_FRAME_STATION_ID:
					str.append(",\"STATION_ID\"");
					break;
					//执行计划
				case ZERO_FRAME_PLAN:
					str.append(",\"PLAN\"");
					break;
					//参数
				case ZERO_FRAME_ARG:
					str.append(",\"ARG\"");
					break;
					//参数
				case ZERO_FRAME_COMMAND:
					str.append(",\"COMMAND\"");
					break;
					//请求ID
				case ZERO_FRAME_REQUEST_ID:
					str.append(",\"REQUEST_ID\"");
					break;
					//请求者/生产者
				case ZERO_FRAME_REQUESTER:
					str.append(",\"REQUESTER\"");
					break;
					//回复者/浪费者
				case ZERO_FRAME_RESPONSER:
					str.append(",\"RESPONSER\"");
					break;
					//通知主题
				case ZERO_FRAME_PUB_TITLE:
					str.append(",\"PUB_TITLE\"");
					break;
				case ZERO_FRAME_STATUS:
					str.append(",\"STATUS\"");
					break;
					//网络上下文信息
				case ZERO_FRAME_CONTEXT:
					str.append(",\"CONTEXT\"");
					break;
				case ZERO_FRAME_CONTENT_TEXT:
					str.append(",\"CONTENT\"");
					break;
				case ZERO_FRAME_CONTENT_JSON:
					str.append(",\"JSON\"");
					break;
				case ZERO_FRAME_CONTENT_BIN:
					str.append(",\"BIN\"");
					break;
				case ZERO_FRAME_CONTENT_XML:
					str.append(",\"XML\"");
					break;
				default:
					str.append(",\"Arg\"");
					break;
				}
			}
			str.append("]}");
			return str;
		}
	}
}