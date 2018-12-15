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
			* \param is_client
			* \return
			*/
			void set_sockopt(zmq_handler& socket, bool is_client, const char* identity)
			{
				//assert(zmq_result == 0);
				//列消息只作用于已完成的链接
				if (global_config::IMMEDIATE >= 0)
					setsockopt(socket, ZMQ_IMMEDIATE, global_config::IMMEDIATE);
				//关闭设置停留时间,毫秒
				if (global_config::LINGER >= 0)
					setsockopt(socket, ZMQ_LINGER, global_config::LINGER);

				if (global_config::MAX_MSGSZ > 0)
					setsockopt(socket, ZMQ_MAXMSGSIZE, global_config::MAX_MSGSZ);

				if (global_config::SNDBUF >= 0)
					setsockopt(socket, ZMQ_SNDBUF, global_config::SNDBUF);
				if (global_config::SNDTIMEO >= 0)
					setsockopt(socket, ZMQ_SNDTIMEO, global_config::SNDTIMEO);
				if (global_config::SNDHWM >= 0)
					setsockopt(socket, ZMQ_SNDHWM, global_config::SNDHWM);

				if (global_config::RCVHWM >= 0)
					setsockopt(socket, ZMQ_RCVHWM, global_config::RCVHWM);
				if (global_config::RCVBUF >= 0)
					setsockopt(socket, ZMQ_RCVBUF, global_config::RCVBUF);
				if (global_config::RCVTIMEO >= 0)
					setsockopt(socket, ZMQ_RCVTIMEO, global_config::RCVTIMEO);

				if (global_config::HEARTBEAT_IVL > 0)
				{
					setsockopt(socket, ZMQ_HEARTBEAT_IVL, 1);
					setsockopt(socket, ZMQ_HEARTBEAT_TIMEOUT, global_config::HEARTBEAT_TIMEOUT);
					setsockopt(socket, ZMQ_HEARTBEAT_TTL, global_config::HEARTBEAT_TTL);
				}

				if (global_config::TCP_KEEPALIVE > 0)
				{
					setsockopt(socket, ZMQ_TCP_KEEPALIVE, 1);
					setsockopt(socket, ZMQ_TCP_KEEPALIVE_IDLE, global_config::TCP_KEEPALIVE_IDLE);
					setsockopt(socket, ZMQ_TCP_KEEPALIVE_INTVL, global_config::TCP_KEEPALIVE_INTVL);
				}
				if (is_client)
				{
					if (identity != nullptr)
						zmq_setsockopt(socket, ZMQ_IDENTITY, identity, strlen(identity));

					if (global_config::CONNECT_TIMEOUT > 0)
						setsockopt(socket, ZMQ_CONNECT_TIMEOUT, global_config::CONNECT_TIMEOUT);
					if (global_config::RECONNECT_IVL > 0)
						setsockopt(socket, ZMQ_RECONNECT_IVL, global_config::RECONNECT_IVL);
					if (global_config::RECONNECT_IVL_MAX > 0)
						setsockopt(socket, ZMQ_RECONNECT_IVL_MAX, global_config::RECONNECT_IVL_MAX);
				}
				else
				{
					if (global_config::BACKLOG >= 0)
						setsockopt(socket, ZMQ_BACKLOG, global_config::BACKLOG);
				}

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
				set_sockopt(socket, true, identity);

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
				set_sockopt(socket, false, nullptr);
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
			bool print_client_addr(int fd, acl::string& str)
			{
				socklen_t          peer_addr_size;
				struct sockaddr_in peer_addr;
				peer_addr_size = sizeof(struct sockaddr);
				if (getpeername(fd, reinterpret_cast<struct sockaddr*>(&peer_addr), &peer_addr_size) != 0)
					return false;
				char esme_ip[200];
				str.format("%s:%d(%d)",
					inet_ntop(peer_addr.sin_family, &peer_addr.sin_addr, esme_ip, sizeof(esme_ip)),
					ntohs(peer_addr.sin_port),
					fd);
				return true;
			}

			/**
			* \brief 网络监控
			* \param {shared_char} station 站点名称
			* \param {shared_char} addr 地址
			*/
			void do_monitor(shared_char station, shared_char addr, zmq_handler*);

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
				boost::thread thread_xxx(boost::bind(&do_monitor, station, addr, socket));
			}

			/**
			* \brief 网络监控
			* \param station 站点
			* \param addr 地址
			* \param socket 连接
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
				while (get_net_state() < zero_def::net_state::distory)
				{
					if (read_event_msg(inproc, &event) == 1)
						continue;
					switch (event.event)
					{
					case ZMQ_EVENT_CLOSED:
						print_client_addr(event.value, str);
						log_msg2("[%s] : monitor > closed > %s", station.c_str(), str.c_str());
						zero_event(zero_net_event::event_monitor_net_close, "monitor", *station, str.c_str());
						//zmq_close(inproc);
						//*socket = nullptr;
						break;
					case ZMQ_EVENT_CLOSE_FAILED:
						log_msg1("[%s] : monitor > close failed", station.c_str());
						zmq_close(inproc);
						return;
					case ZMQ_EVENT_MONITOR_STOPPED:
						log_msg1("[%s] : monitor > monitor stepped", station.c_str());
						zmq_close(inproc);
						return;
					case ZMQ_EVENT_LISTENING:
						log_msg1("[%s] : monitor > listening", station.c_str());
						break;
					case ZMQ_EVENT_BIND_FAILED:
						log_msg2("[%s] : monitor > bind failed > %s", station.c_str(), event.address);
						break;
					case ZMQ_EVENT_ACCEPTED:
						print_client_addr(event.value, str);
						log_msg2("[%s] : monitor > join > %s", station.c_str(), str.c_str());
						zero_event(zero_net_event::event_monitor_net_connected, "monitor", *station, str.c_str());
						break;
					case ZMQ_EVENT_DISCONNECTED:
						print_client_addr(event.value, str);
						log_msg2("[%s] : monitor > left > %s", station.c_str(), str.c_str());
						zero_event(zero_net_event::event_monitor_net_close, "monitor", *station, str.c_str());
						break;
					case ZMQ_EVENT_ACCEPT_FAILED:
						print_client_addr(event.value, str);
						log_msg2("[%s] : monitor > join failed > %s", station.c_str(), str.c_str());
						zero_event(zero_net_event::event_monitor_net_failed, "monitor", *station, str.c_str());
						break;
					case ZMQ_EVENT_CONNECTED:
						log_msg2("[%s] : monitor > connected > %s", station.c_str(), event.address);
						break;
					case ZMQ_EVENT_CONNECT_DELAYED:
						log_msg2("[%s] : monitor > connect delayed > %s", station.c_str(), event.address);
						zero_event(zero_net_event::event_monitor_net_failed, "monitor", *station, event.address);
						break;
					case ZMQ_EVENT_CONNECT_RETRIED:
						log_msg2("[%s] : monitor > retried > %s", station.c_str(), event.address);
						zero_event(zero_net_event::event_monitor_net_try, "monitor", *station, event.address);
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
		namespace zero_def
		{
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
					case zero_def::command::none: //!\1 无特殊说明
						str.append("none");
						break;
					case zero_def::command::plan: //!\2  取全局标识
						str.append("plan");
						break;
					case zero_def::command::global_id: //!>  
						str.append("global_id");
						break;
					case zero_def::command::waiting: //!# 等待结果
						str.append("waiting");
						break;
					case zero_def::command::find_result: //!% 关闭结果
						str.append("find result");
						break;
					case zero_def::command::close_request: //!- Ping
						str.append("close request");
						break;
					case zero_def::command::ping: //!* 心跳加入
						str.append("ping");
						break;
					case zero_def::command::heart_join: //!J  心跳已就绪
						str.append("heart join");
						break;
					case zero_def::command::heart_ready: //!R  心跳进行
						str.append("heart ready");
						break;
					case zero_def::command::heart_pitpat: //!P  心跳退出
						str.append("heart pitpat");
						break;
					case zero_def::command::heart_left: //!L  
						str.append("heart left");
						break;
					}
				}
				else
				{
					str.append(R"(,"state":")");
					switch (state)
					{
					case zero_def::status::ok: //!(0x1)
						str.append(zero_def::status_text::OK);
						break;
					case zero_def::status::jion_plan: //!(0x2)
						str.append(zero_def::status_text::PLAN);
						break;
					case zero_def::status::runing: //!(0x3)
						str.append(zero_def::status_text::RUNING);
						break;
					case zero_def::status::bye: //!(0x4)
						str.append(zero_def::status_text::BYE);
						break;
					case zero_def::status::wecome: //!(0x5)
						str.append(zero_def::status_text::WECOME);
						break;
					case zero_def::status::vote_sended: //!(0x20)
						str.append(zero_def::status_text::VOTE_SENDED);
						break;
					case zero_def::status::vote_bye: //!(0x21)
						str.append(zero_def::status_text::VOTE_BYE);
						break;
					case zero_def::status::wait: //!(0x22)
						str.append(zero_def::status_text::WAITING);
						break;
					case zero_def::status::vote_waiting: //!(0x22)
						str.append(zero_def::status_text::WAITING);
						break;
					case zero_def::status::vote_start: //!(0x23)
						str.append(zero_def::status_text::VOTE_START);
						break;
					case zero_def::status::vote_end: //!(0x24)
						str.append(zero_def::status_text::VOTE_END);
						break;
					case zero_def::status::vote_closed: //!(0x25)
						str.append(zero_def::status_text::VOTE_CLOSED);
						break;
					case zero_def::status::error: //!(0x81)
						str.append(zero_def::status_text::ERROR);
						break;
					case zero_def::status::failed: //!(0x82)
						str.append(zero_def::status_text::FAILED);
						break;
					case zero_def::status::not_find: //!(0x83)
						str.append(zero_def::status_text::NOT_FIND);
						break;
					case zero_def::status::not_support: //!(0x84)
						str.append(zero_def::status_text::NOT_SUPPORT);
						break;
					case zero_def::status::frame_invalid: //!(0x85)
						str.append(zero_def::status_text::FRAME_INVALID);
						break;
					case zero_def::status::arg_invalid: //!(0x85)
						str.append(zero_def::status_text::ARG_INVALID);
						break;
					case zero_def::status::timeout: //!(0x86)
						str.append(zero_def::status_text::TIMEOUT);
						break;
					case zero_def::status::net_error: //!(0x87)
						str.append(zero_def::status_text::NET_ERROR);
						break;
					case zero_def::status::not_worker: //!(0x88)
						str.append(zero_def::status_text::NOT_WORKER);
						break;
					case zero_def::status::plan_error: //!(0x8B)
						str.append(zero_def::status_text::PLAN_ERROR);
						break;
					}
				}
				str.append(R"(","frames":[)");

				str.append(R"("Caller","FrameDescr")");
				for (size_t idx = 2; idx < len; idx++)
				{
					switch (desc[idx])
					{
					case zero_def::frame::end:
						str.append(",\"End\"");
						break;
						//全局标识
					case zero_def::frame::global_id:
						str.append(",\"GLOBAL_ID\"");
						break;
						//站点
					case zero_def::frame::station_id:
						str.append(",\"STATION_ID\"");
						break;
						//执行计划
					case zero_def::frame::plan:
						str.append(",\"PLAN\"");
						break;
						//参数
					case zero_def::frame::arg:
						str.append(",\"ARG\"");
						break;
						//参数
					case zero_def::frame::command:
						str.append(",\"COMMAND\"");
						break;
						//请求ID
					case zero_def::frame::request_id:
						str.append(",\"REQUEST_ID\"");
						break;
						//请求者/生产者
					case zero_def::frame::requester:
						str.append(",\"REQUESTER\"");
						break;
						//回复者/浪费者
					case zero_def::frame::responser:
						str.append(",\"RESPONSER\"");
						break;
						//通知主题
					case zero_def::frame::pub_title:
						str.append(",\"PUB_TITLE\"");
						break;
					case zero_def::frame::status:
						str.append(",\"STATUS\"");
						break;
						//网络上下文信息
					case zero_def::frame::context:
						str.append(",\"CONTEXT\"");
						break;
					case zero_def::frame::content_text:
						str.append(",\"CONTENT\"");
						break;
					case zero_def::frame::content_json:
						str.append(",\"JSON\"");
						break;
					case zero_def::frame::content_bin:
						str.append(",\"BIN\"");
						break;
					case zero_def::frame::content_xml:
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
}