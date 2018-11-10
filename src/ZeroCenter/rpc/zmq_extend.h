#pragma once
#ifndef _ZMQ_EXTEND_H_
#define _ZMQ_EXTEND_H_
#include "zero_net.h"
#include "../ext/shared_char.h"
namespace agebull
{
	namespace zmq_net
	{

		/**
		* \brief ZMQ套接字状态
		*/
		enum class zmq_socket_state
		{
			/**
			* \brief 没问题
			*/
			Succeed,
			/**
			* \brief 后续还有消息
			*/
			More,

			/**
			* \brief 空帧
			*/
			Empty,

			/**
			* \brief 主机不可达
			*/
			HostUnReach,
			/**
			* \brief 网络关闭
			*/
			NetDown,

			/**
			* \brief 网络不可达
			*/
			NetUnReach,

			/**
			* \brief 网络重置
			*/
			NetReset,

			/**
			* \brief 未连接
			*/
			NotConn,
			/**
			* \brief 连接已在使用中？
			*/
			ConnRefUsed,
			/**
			* \brief 连接中断
			*/
			ConnAborted,

			/**
			* \brief 连接重置
			*/
			ConnReset,

			/**
			* \brief 超时
			*/
			TimedOut,

			/**
			* \brief 正在处理中？
			*/
			InProgress,

			/**
			* \brief 跨线程调用？
			*/
			Mthread,

			/**
			* \brief 指定的socket不可用
			*/
			NotSocket,

			/**
			* \brief 内存不足
			*/
			NoBufs,

			/**
			* \brief 消息大小不合适？
			*/
			MsgSize,

			/**
			* \brief 指定的socket相关联的context已关闭
			*/
			Term,

			/**
			* \brief 系统信号中断
			*/
			Intr,

			/**
			* \brief 不支持？
			*/
			NotSup,

			/**
			* \brief 不支持的协议
			*/
			ProtoNoSupport,

			/**
			* \brief 协议不兼容
			*/
			NoCompatProto,

			/**
			* \brief ？
			*/
			AfNoSupport,

			/**
			* \brief 地址问题？
			*/
			AddrNotAvAll,
			/**
			* \brief 地址已被使用
			*/
			AddrInUse,
			/**
			* \brief ？
			*/
			Fsm,

			/**
			* \brief 重启
			*/
			Again,
			/**
			* \brief 其它错误
			*/
			Unknow
		};


/*#define make_ipc_address(addr,type,name)\
			char addr[MAX_PATH];\
			sprintf(addr, "ipc://%sipc/%s_%s.ipc", json_config::root_path.c_str(),type, name)
*/

#define make_inproc_address(addr,name)\
			char addr[MAX_PATH];\
			sprintf(addr, "inproc://%s.inp", name)

#define make_zmq_identity(addr,type,name)\
			char identity[MAX_PATH];\
			sprintf(identity, "%s_%s", type, name)

		namespace socket_ex
		{

			/**
			* \brief 检查ZMQ错误状态
			* \return 状态
			*/
			inline const char* state_str(zmq_socket_state state)
			{
				switch (state)
				{
				case zmq_socket_state::Succeed: return "Succeed";
				case zmq_socket_state::More: return "More";
				case zmq_socket_state::Empty: return "Empty";
				case zmq_socket_state::HostUnReach: return "HostUnReach";
				case zmq_socket_state::NetDown: return "NetDown";
				case zmq_socket_state::NetUnReach: return "NetUnReach";
				case zmq_socket_state::NetReset: return "NetReset";
				case zmq_socket_state::NotConn: return "NotConn";
				case zmq_socket_state::ConnRefUsed: return "ConnRefUsed";
				case zmq_socket_state::ConnAborted: return "ConnAborted";
				case zmq_socket_state::ConnReset: return "ConnReset";
				case zmq_socket_state::TimedOut: return "TimedOut";
				case zmq_socket_state::InProgress: return "InProgress";
				case zmq_socket_state::Mthread: return "Mthread";
				case zmq_socket_state::NotSocket: return "NotSocket";
				case zmq_socket_state::NoBufs: return "NoBufs";
				case zmq_socket_state::MsgSize: return "MsgSize";
				case zmq_socket_state::Term: return "Term";
				case zmq_socket_state::Intr: return "Intr";
				case zmq_socket_state::NotSup: return "NotSup";
				case zmq_socket_state::ProtoNoSupport: return "ProtoNoSupport";
				case zmq_socket_state::NoCompatProto: return "NoCompatProto";
				case zmq_socket_state::AfNoSupport: return "AfNoSupport";
				case zmq_socket_state::AddrNotAvAll: return "AddrNotAvAll";
				case zmq_socket_state::AddrInUse: return "AddrInUse";
				case zmq_socket_state::Fsm: return "Fsm";
				case zmq_socket_state::Again: return "Again";
				case zmq_socket_state::Unknow: return "Unknow";
				default:return "*";
				}
			}

			/**
			* \brief 检查ZMQ错误状态
			* \return 状态
			*/
			inline zmq_socket_state check_zmq_error()
			{
				const int err = zmq_errno();
				zmq_socket_state state;
				switch (err)
				{
				case 0:
					state = zmq_socket_state::Empty; break;
				case ETERM:
					state = zmq_socket_state::Intr; break;
				case ENOTSOCK:
					state = zmq_socket_state::NotSocket; break;
				case EINTR:
					state = zmq_socket_state::Intr; break;
				case EAGAIN:
				case ETIMEDOUT:
					state = zmq_socket_state::TimedOut; break;
					//state = ZmqSocketState::TimedOut;break;
				case ENOTSUP:
					state = zmq_socket_state::NotSup; break;
				case EPROTONOSUPPORT:
					state = zmq_socket_state::ProtoNoSupport; break;
				case ENOBUFS:
					state = zmq_socket_state::NoBufs; break;
				case ENETDOWN:
					state = zmq_socket_state::NetDown; break;
				case EADDRINUSE:
					state = zmq_socket_state::AddrInUse; break;
				case EADDRNOTAVAIL:
					state = zmq_socket_state::AddrNotAvAll; break;
				case ECONNREFUSED:
					state = zmq_socket_state::ConnRefUsed; break;
				case EINPROGRESS:
					state = zmq_socket_state::InProgress; break;
				case EMSGSIZE:
					state = zmq_socket_state::MsgSize; break;
				case EAFNOSUPPORT:
					state = zmq_socket_state::AfNoSupport; break;
				case ENETUNREACH:
					state = zmq_socket_state::NetUnReach; break;
				case ECONNABORTED:
					state = zmq_socket_state::ConnAborted; break;
				case ECONNRESET:
					state = zmq_socket_state::ConnReset; break;
				case ENOTCONN:
					state = zmq_socket_state::NotConn; break;
				case EHOSTUNREACH:
					state = zmq_socket_state::HostUnReach; break;
				case ENETRESET:
					state = zmq_socket_state::NetReset; break;
				case EFSM:
					state = zmq_socket_state::Fsm; break;
				case ENOCOMPATPROTO:
					state = zmq_socket_state::NoCompatProto; break;
				case EMTHREAD:
					state = zmq_socket_state::Mthread; break;
				default:
					state = zmq_socket_state::Unknow; break;
				}
#if _DEBUG_
				if (state != zmq_socket_state::Succeed)
					log_debug(0, 0, state_str(state));
#endif // _DEBUG_
				return state;
			}

			/**
			* \brief 生成用于本机调用的套接字
			*/
			inline void setsockopt(ZMQ_HANDLE& socket, int type, int value)
			{
				zmq_setsockopt(socket, type, &value, sizeof(int));
			}

			/**
			* \brief 关闭套接字
			* \param socket
			* \param addr
			* \return
			*/
			void close_res_socket(ZMQ_HANDLE& socket, const char* addr);

			/**
			* \brief 关闭套接字
			* \param socket
			* \param addr
			* \return
			*/
			void close_req_socket(ZMQ_HANDLE& socket, const char* addr);

			/**
			* \brief 配置ZMQ连接对象
			* \param socket
			* \param name
			* \return
			*/
			void set_sockopt(ZMQ_HANDLE& socket, const char* name = nullptr);

			/**
			* \brief 生成ZMQ连接对象
			* \param station
			* \param addr
			* \param type
			* \param name
			* \return
			*/
			ZMQ_HANDLE create_req_socket(const char* station, int type, const char* addr, const char* name);

			/**
			* \brief 生成ZMQ连接对象
			* \param station
			* \param addr
			* \param type
			* \param name
			* \return
			*/
			ZMQ_HANDLE create_res_socket(const char* station, const char* addr, int type, const char* name);

			/**
			* \brief 生成用于TCP的套接字
			*/
			bool set_tcp_nodelay(ZMQ_HANDLE socket);

			/**
			* \brief 生成用于TCP的套接字
			*/
			inline ZMQ_HANDLE create_res_socket_tcp(const char* name, int type, int port)
			{
				char host[MAX_PATH];
				sprintf(host, "tcp://*:%d", port);
				ZMQ_HANDLE socket = create_res_socket(name, host, type, name);
				if (socket == nullptr)
				{
					return nullptr;
				}
				if (!set_tcp_nodelay(socket))
					log_error2("socket(%s) option TCP_NODELAY bad:%s", host, zmq_strerror(zmq_errno()));
				return socket;
			}
			/**
			* \brief 生成用于TCP的套接字
			*/
			inline ZMQ_HANDLE create_req_socket_tcp(const char* station, const char* name, int type, int port)
			{
				char addr[MAX_PATH];
				sprintf(addr, "tcp://*:%d", port);
				ZMQ_HANDLE socket = create_req_socket(station, type, addr, name);
				if (socket == nullptr)
				{
					return nullptr;
				}
				if (!set_tcp_nodelay(socket))
					log_error2("socket(%s) option TCP_NODELAY bad:%s", addr, zmq_strerror(zmq_errno()));
				return socket;
			}
			/**
			* \brief 生成用于本机调用的套接字
			*/
			//inline ZMQ_HANDLE create_req_socket_ipc(const char* station, const char* name, int type)
			//{
			//	make_ipc_address(address, station, name);
			//	make_zmq_identity(identity, station, name);
			//	return create_req_socket(address, type, identity);
			//}
			/**
			* \brief 生成用于本机调用的套接字
			*/
			//inline ZMQ_HANDLE create_res_socket_ipc(const char* station, const char* name, int type)
			//{
			//	make_ipc_address(address, station, name);
			//	make_zmq_identity(identity, station, name);
			//	return create_res_socket(address, type, identity);
			//}
			/**
			* \brief 生成用于本机调用的套接字
			*/
			inline ZMQ_HANDLE create_req_socket_inproc(const char* station, const char* name)
			{
				make_inproc_address(address, station);
				char identity[MAX_PATH];
				sprintf(identity, "->%s_%s", station, name);
				return create_req_socket(station, ZMQ_DEALER, address, identity);
			}

			/**
			* \brief 生成用于本机调用的套接字
			*/
			inline ZMQ_HANDLE create_res_socket_inproc(const char* station, int type)
			{
				make_inproc_address(address, station);
				char identity[MAX_PATH];
				sprintf(identity, "%s_inp", station);
				return create_res_socket(station, address, type, identity);
			}
			/**
			* \brief 接收
			*/
			inline zmq_socket_state recv(ZMQ_HANDLE socket, shared_char& data, int flag = 0)
			{
				//接收命令请求
				zmq_msg_t msg_call;
				int state = zmq_msg_init(&msg_call);
				if (state < 0)
				{
					return zmq_socket_state::NoBufs;
				}
				state = zmq_msg_recv(&msg_call, socket, flag);
				if (state < 0)
				{
					return check_zmq_error();
				}
				data = msg_call;
				zmq_msg_close(&msg_call);
				int more;
				size_t size = sizeof(int);
				zmq_getsockopt(socket, ZMQ_RCVMORE, &more, &size);
				return more == 0 ? zmq_socket_state::Succeed : zmq_socket_state::More;
			}

			/**
			* \brief 接收
			*/
			inline zmq_socket_state recv(ZMQ_HANDLE socket, vector<shared_char>& ls, int flag = 0)
			{
				size_t size = sizeof(int);
				int more;
				do
				{
					zmq_msg_t msg;
					int re = zmq_msg_init(&msg);
					if (re < 0)
					{
						return zmq_socket_state::NoBufs;
					}
					re = zmq_msg_recv(&msg, socket, flag);
					if (re < 0)
					{
						return check_zmq_error();
					}
					if (re == 0)
						ls.emplace_back();
					else
						ls.emplace_back(msg);
					zmq_msg_close(&msg);
					zmq_getsockopt(socket, ZMQ_RCVMORE, &more, &size);
				} while (more != 0);
				return zmq_socket_state::Succeed;
			}

			/**
			* \brief 发送最后一帧
			*/
			inline zmq_socket_state send_late(ZMQ_HANDLE socket, const char* string)
			{
				const int state = zmq_send(socket, string, strlen(string), ZMQ_DONTWAIT);
				if (state < 0)
				{
					return check_zmq_error();
				}
				return zmq_socket_state::Succeed;
			}

			/**
			* \brief 发送帧
			*/
			inline zmq_socket_state send_more(ZMQ_HANDLE socket, const char* string)
			{
				const int state = zmq_send(socket, string, strlen(string), ZMQ_SNDMORE);
				if (state < 0)
				{
					return check_zmq_error();
				}
				return zmq_socket_state::Succeed;
			}
			/**
			* \brief 发送帧
			*/
			inline zmq_socket_state send_addr(ZMQ_HANDLE socket, const char* addr)
			{
				int state = zmq_send(socket, addr, strlen(addr), ZMQ_SNDMORE);
				if (state < 0)
				{
					return check_zmq_error();
				}
				/*state = zmq_send(socket, "", 0, ZMQ_SNDMORE);
				if (state < 0)
				{
					return check_zmq_error();
				}*/
				return zmq_socket_state::Succeed;
			}
			/**
			* \brief 发送
			*/
			inline int send_shared_char(ZMQ_HANDLE socket, const shared_char& iter, int flag)
			{
				if (iter.empty())
					return zmq_send(socket, "", 0, flag);
				return zmq_send(socket, *iter, iter.size(), flag);
			}

			/**
			* \brief 发送
			*/
			inline zmq_socket_state send(ZMQ_HANDLE socket, vector<shared_char>::iterator& iter, const vector<shared_char>::iterator& end)
			{
				while (iter != end)
				{
					const int state = send_shared_char(socket, *iter.base(), ZMQ_SNDMORE);
					if (state < 0)
					{
						return check_zmq_error();
					}
					++iter;
				}
				return send_late(socket, "");
			}
			/**
			* \brief 发送
			*/
			inline zmq_socket_state send(ZMQ_HANDLE socket, const vector<shared_char>& ls, const size_t first_index = 0)
			{
				if (first_index >= ls.size())
					return send_late(socket, "");
				size_t idx = first_index;
				for (; idx < ls.size() - 1; idx++)
				{
					const int state = send_shared_char(socket, ls[idx], ZMQ_SNDMORE);
					if (state < 0)
					{
						return check_zmq_error();
					}
				}
				const int state = send_shared_char(socket, ls[idx], ZMQ_DONTWAIT);
				if (state < 0)
				{
					return check_zmq_error();
				}
				return zmq_socket_state::Succeed;
			}
			/**
			* \brief 发送
			*/
			inline zmq_socket_state send(ZMQ_HANDLE socket, vector<string>& ls, size_t first_index = 0)
			{
				if (first_index >= ls.size())
					return send_late(socket, "");
				size_t idx = first_index;
				int state;
				for (; idx < ls.size() - 1; idx++)
				{
					state = send_shared_char(socket, ls[idx], ZMQ_SNDMORE);
					if (state < 0)
					{
						return check_zmq_error();
					}
				}
				state = send_shared_char(socket, ls[idx], ZMQ_DONTWAIT);
				if (state < 0)
				{
					return check_zmq_error();
				}
				return zmq_socket_state::Succeed;
			}
			/**
			* \brief 发送帧
			*/
			inline zmq_socket_state send_status(ZMQ_HANDLE socket, const char* addr, uchar state, const char* global_id, const char* req_id, const char* reqer, const char* msg)
			{
				uchar descirpt[8];
				descirpt[1] = state;

				int idx = 2;
				if (reqer != nullptr)
				{
					descirpt[idx++] = ZERO_FRAME_REQUESTER;
				}
				if (msg != nullptr)
				{
					descirpt[idx++] = ZERO_FRAME_MESSAGE;
				}
				if (req_id != nullptr)
				{
					descirpt[idx++] = ZERO_FRAME_REQUEST_ID;
				}
				if (global_id != nullptr)
				{
					descirpt[idx++] = ZERO_FRAME_GLOBAL_ID;
				}
				descirpt[0] = static_cast<char>(idx - 2);
				descirpt[idx] = ZERO_FRAME_END;

				int reqer_flags = ZMQ_SNDMORE;
				int reqId_flags = ZMQ_SNDMORE;
				int descirpt_flags = ZMQ_SNDMORE;
				int msg_flags = ZMQ_SNDMORE;
				if (global_id == nullptr)
				{
					if (req_id != nullptr)
					{
						reqId_flags = ZMQ_DONTWAIT;
					}
					else if (msg != nullptr)
					{
						msg_flags = ZMQ_DONTWAIT;
					}
					else if (reqer != nullptr)
					{
						reqer_flags = ZMQ_DONTWAIT;
					}
					else
					{
						descirpt_flags = ZMQ_DONTWAIT;
					}
				}

				int re = zmq_send(socket, addr, strlen(addr), ZMQ_SNDMORE);
				if (re < 0)
				{
					return check_zmq_error();
				}
				//state = zmq_send(socket, "", 0, ZMQ_SNDMORE);
				//if (state < 0)
				//{
				//	return check_zmq_error();
				//}
				re = zmq_send(socket, descirpt, idx + 1, descirpt_flags);
				if (re < 0)
				{
					return check_zmq_error();
				}
				if (reqer != nullptr)
				{
					re = zmq_send(socket, reqer, strlen(reqer), reqer_flags);
					if (re < 0)
					{
						return check_zmq_error();
					}
				}
				if (msg != nullptr)
				{
					re = zmq_send(socket, msg, strlen(msg), msg_flags);
					if (re < 0)
					{
						return check_zmq_error();
					}
				}
				if (req_id != nullptr)
				{
					re = zmq_send(socket, req_id, strlen(req_id), reqId_flags);
					if (re < 0)
					{
						return check_zmq_error();
					}
				}
				if (global_id != nullptr)
				{
					re = zmq_send(socket, global_id, strlen(global_id), ZMQ_DONTWAIT);
					if (re < 0)
					{
						return check_zmq_error();
					}
				}

				return zmq_socket_state::Succeed;
			}
		};

		namespace zmq_monitor
		{
			/**
			* \brief 网络监控
			* \param {shared_char} station 站点名称
			* \param {shared_char} addr 地址
			*/
			void do_monitor(shared_char station, shared_char addr);

			/**
			* \brief 网络监控
			* \param {shared_char} station 站点名称
			* \param {ZMQ_HANDLE} socket socket
			* \param {shared_char} addr 地址
			*/
			void set_monitor(const char* station, ZMQ_HANDLE socket, const char* type);
		}

	}
}

#endif//!_ZMQ_EXTEND_H_
