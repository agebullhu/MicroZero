#pragma once
#ifndef _ZMQ_EXTEND_H_
#define _ZMQ_EXTEND_H_
#include "zero_net.h"
#include "../ext/shared_char.h"
namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief ZMQ套接字状态
		*/
		enum class zmq_socket_state
		{
			/**
			* \brief 没问题
			*/
			succeed,
			/**
			* \brief 后续还有消息
			*/
			more,

			/**
			* \brief 空帧
			*/
			empty,

			/**
			* \brief 主机不可达
			*/
			host_un_reach,
			/**
			* \brief 网络关闭
			*/
			net_down,

			/**
			* \brief 网络不可达
			*/
			net_un_reach,

			/**
			* \brief 网络重置
			*/
			net_reset,

			/**
			* \brief 未连接
			*/
			not_conn,
			/**
			* \brief 连接已在使用中？
			*/
			conn_ref_used,
			/**
			* \brief 连接中断
			*/
			conn_aborted,

			/**
			* \brief 连接重置
			*/
			conn_reset,

			/**
			* \brief 超时
			*/
			timed_out,

			/**
			* \brief 正在处理中？
			*/
			in_progress,

			/**
			* \brief 跨线程调用？
			*/
			m_thread,

			/**
			* \brief 指定的socket不可用
			*/
			not_socket,

			/**
			* \brief 内存不足
			*/
			no_bufs,

			/**
			* \brief 消息大小不合适？
			*/
			msg_size,

			/**
			* \brief 指定的socket相关联的context已关闭
			*/
			term,

			/**
			* \brief 系统信号中断
			*/
			intr,

			/**
			* \brief 不支持？
			*/
			not_sup,

			/**
			* \brief 不支持的协议
			*/
			proto_no_support,

			/**
			* \brief 协议不兼容
			*/
			no_compat_proto,

			/**
			* \brief ？
			*/
			af_no_support,

			/**
			* \brief 地址问题？
			*/
			addr_not_av_all,
			/**
			* \brief 地址已被使用
			*/
			addr_in_use,
			/**
			* \brief ？
			*/
			fsm,

			/**
			* \brief 重启
			*/
			again,
			/**
			* \brief 其它错误
			*/
			unknow
		};


/*#define make_ipc_address(addr,type,name)\
			char addr[MAX_PATH];\
			sprintf(addr, "ipc://%sipc/%s_%s.ipc", global_config::root_path,type, name)
*/

#define make_inproc_address(addr,name)\
			char addr[MAX_PATH];\
			sprintf(addr, "inproc://%s.inp", name)


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
				case zmq_socket_state::succeed: return "Succeed";
				case zmq_socket_state::more: return "More";
				case zmq_socket_state::empty: return "Empty";
				case zmq_socket_state::host_un_reach: return "HostUnReach";
				case zmq_socket_state::net_down: return "NetDown";
				case zmq_socket_state::net_un_reach: return "NetUnReach";
				case zmq_socket_state::net_reset: return "NetReset";
				case zmq_socket_state::not_conn: return "NotConn";
				case zmq_socket_state::conn_ref_used: return "ConnRefUsed";
				case zmq_socket_state::conn_aborted: return "ConnAborted";
				case zmq_socket_state::conn_reset: return "ConnReset";
				case zmq_socket_state::timed_out: return "TimedOut";
				case zmq_socket_state::in_progress: return "InProgress";
				case zmq_socket_state::m_thread: return "Mthread";
				case zmq_socket_state::not_socket: return "NotSocket";
				case zmq_socket_state::no_bufs: return "NoBufs";
				case zmq_socket_state::msg_size: return "MsgSize";
				case zmq_socket_state::term: return "Term";
				case zmq_socket_state::intr: return "Intr";
				case zmq_socket_state::not_sup: return "NotSup";
				case zmq_socket_state::proto_no_support: return "ProtoNoSupport";
				case zmq_socket_state::no_compat_proto: return "NoCompatProto";
				case zmq_socket_state::af_no_support: return "AfNoSupport";
				case zmq_socket_state::addr_not_av_all: return "AddrNotAvAll";
				case zmq_socket_state::addr_in_use: return "AddrInUse";
				case zmq_socket_state::fsm: return "Fsm";
				case zmq_socket_state::again: return "Again";
				case zmq_socket_state::unknow: return "Unknow";
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
					state = zmq_socket_state::empty; break;
				case ETERM:
					state = zmq_socket_state::intr; break;
				case ENOTSOCK:
					state = zmq_socket_state::not_socket; break;
				case EINTR:
					state = zmq_socket_state::intr; break;
				case EAGAIN:
				case ETIMEDOUT:
					state = zmq_socket_state::timed_out; break;
					//state = ZmqSocketState::TimedOut;break;
				case ENOTSUP:
					state = zmq_socket_state::not_sup; break;
				case EPROTONOSUPPORT:
					state = zmq_socket_state::proto_no_support; break;
				case ENOBUFS:
					state = zmq_socket_state::no_bufs; break;
				case ENETDOWN:
					state = zmq_socket_state::net_down; break;
				case EADDRINUSE:
					state = zmq_socket_state::addr_in_use; break;
				case EADDRNOTAVAIL:
					state = zmq_socket_state::addr_not_av_all; break;
				case ECONNREFUSED:
					state = zmq_socket_state::conn_ref_used; break;
				case EINPROGRESS:
					state = zmq_socket_state::in_progress; break;
				case EMSGSIZE:
					state = zmq_socket_state::msg_size; break;
				case EAFNOSUPPORT:
					state = zmq_socket_state::af_no_support; break;
				case ENETUNREACH:
					state = zmq_socket_state::net_un_reach; break;
				case ECONNABORTED:
					state = zmq_socket_state::conn_aborted; break;
				case ECONNRESET:
					state = zmq_socket_state::conn_reset; break;
				case ENOTCONN:
					state = zmq_socket_state::not_conn; break;
				case EHOSTUNREACH:
					state = zmq_socket_state::host_un_reach; break;
				case ENETRESET:
					state = zmq_socket_state::net_reset; break;
				case EFSM:
					state = zmq_socket_state::fsm; break;
				case ENOCOMPATPROTO:
					state = zmq_socket_state::no_compat_proto; break;
				case EMTHREAD:
					state = zmq_socket_state::m_thread; break;
				default:
					state = zmq_socket_state::unknow; break;
				}
#if _DEBUG_
				if (state != zmq_socket_state::succeed)
					log_debug(0, 0, state_str(state));
#endif // _DEBUG_
				return state;
			}

			/**
			* \brief 生成用于本机调用的套接字
			*/
			inline void setsockopt(zmq_handler& socket, int type, int value)
			{
				zmq_setsockopt(socket, type, &value, sizeof(int));
			}

			/**
			* \brief 关闭套接字
			* \param socket
			* \param addr
			* \return
			*/
			void close_res_socket(zmq_handler& socket, const char* addr);

			/**
			* \brief 关闭套接字
			* \param socket
			* \param addr
			* \return
			*/
			void close_req_socket(zmq_handler& socket, const char* addr);

			/**
			* \brief 配置ZMQ连接对象
			* \param socket
			* \param name
			* \param is_client
			* \return
			*/
			void set_sockopt(zmq_handler& socket,bool is_client, const char* name = nullptr);

			/**
			* \brief 生成ZMQ连接对象
			* \param station
			* \param addr
			* \param type
			* \param identity
			* \return
			*/
			zmq_handler create_req_socket(const char* station, int type, const char* addr, const char* identity=nullptr);

			/**
			* \brief 生成ZMQ连接对象
			* \param station
			* \param addr
			* \param type
			* \return
			*/
			zmq_handler create_res_socket(const char* station, const char* addr, int type);

			/**
			* \brief 生成用于TCP的套接字
			*/
			bool set_tcp_nodelay(zmq_handler socket);

			/**
			* \brief 生成用于TCP的套接字
			*/
			inline zmq_handler create_res_socket_tcp(const char* name, int type, int port)
			{
				char host[MAX_PATH];
				sprintf(host, "tcp://*:%d", port);
				return create_res_socket(name, host, type);
			}
			/**
			* \brief 生成用于TCP的套接字
			*/
			inline zmq_handler create_req_socket_tcp(const char* station, const char* identity, int type, int port)
			{
				char addr[MAX_PATH];
				sprintf(addr, "tcp://*:%d", port);
				return create_req_socket(station, type, addr, identity);
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
			inline zmq_handler create_req_socket_inproc(const char* station, const char* name)
			{
				make_inproc_address(address, station);
				char identity[MAX_PATH];

				sprintf(identity, "%c%c%s_%s", zero_def::name::head::inproc, zero_def::name::head::client, station, name);
				return create_req_socket(name, ZMQ_DEALER, address, identity);
			}

			/**
			* \brief 生成用于本机调用的套接字
			*/
			inline zmq_handler create_res_socket_inproc(const char* station, int type)
			{
				make_inproc_address(address, station);
				return create_res_socket(station, address, type);
			}
			/**
			* \brief 接收
			*/
			inline zmq_socket_state recv(zmq_handler socket, shared_char& data, int flag = 0)
			{
				//接收命令请求
				zmq_msg_t msg_call;
				int state = zmq_msg_init(&msg_call);
				if (state < 0)
				{
					return zmq_socket_state::no_bufs;
				}
				state = zmq_msg_recv(&msg_call, socket, flag);
				if (state < 0)
				{
					zmq_msg_close(&msg_call);
					return check_zmq_error();
				}
				data = msg_call;
				zmq_msg_close(&msg_call);
				int more;
				size_t size = sizeof(int);
				zmq_getsockopt(socket, ZMQ_RCVMORE, &more, &size);
				return more == 0 ? zmq_socket_state::succeed : zmq_socket_state::more;
			}

			/**
			* \brief 接收
			*/
			inline zmq_socket_state recv(zmq_handler socket, vector<shared_char>& ls, int flag = 0)
			{
				size_t size = sizeof(int);
				int more;
				do
				{
					zmq_msg_t msg;
					int re = zmq_msg_init(&msg);
					if (re < 0)
					{
						return zmq_socket_state::no_bufs;
					}
					re = zmq_msg_recv(&msg, socket, flag);
					if (re < 0)
					{
						zmq_msg_close(&msg);
						return check_zmq_error();
					}
					if (re == 0)
						ls.emplace_back();
					else
						ls.emplace_back(msg);
					zmq_msg_close(&msg);
					zmq_getsockopt(socket, ZMQ_RCVMORE, &more, &size);
				} while (more != 0);
				return zmq_socket_state::succeed;
			}

			/**
			* \brief 发送最后一帧
			*/
			inline zmq_socket_state send_late(zmq_handler socket, const char* string)
			{
				const int state = zmq_send(socket, string, strlen(string), ZMQ_DONTWAIT);
				if (state < 0)
				{
					return check_zmq_error();
				}
				return zmq_socket_state::succeed;
			}

			/**
			* \brief 发送帧
			*/
			inline zmq_socket_state send_more(zmq_handler socket, const char* string)
			{
				const int state = zmq_send(socket, string, strlen(string), ZMQ_SNDMORE);
				if (state < 0)
				{
					return check_zmq_error();
				}
				return zmq_socket_state::succeed;
			}
			/**
			* \brief 发送帧
			*/
			inline zmq_socket_state send_addr(zmq_handler socket, const char* addr)
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
				return zmq_socket_state::succeed;
			}
			/**
			* \brief 发送
			*/
			inline int send_shared_char(zmq_handler socket, const shared_char& iter, int flag)
			{
				if (iter.empty())
					return zmq_send(socket, "", 0, flag);
				return zmq_send(socket, iter.get_buffer(), iter.size(), flag);
			}

			/**
			* \brief 发送
			*/
			inline zmq_socket_state send(zmq_handler socket, vector<shared_char>::iterator& iter, const vector<shared_char>::iterator& end)
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
			inline zmq_socket_state send(zmq_handler socket, const vector<shared_char>& ls, const size_t first_index = 0)
			{
				size_t last = ls.size() - 1;
				if (first_index > last)
					return send_late(socket, "");
				size_t idx = first_index;
				for (; idx < last; idx++)
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
				return zmq_socket_state::succeed;
			}
			/**
			* \brief 发送
			*/
			inline zmq_socket_state send(zmq_handler socket, vector<string>& ls, size_t first_index = 0)
			{
				if (first_index >= ls.size())
					return send_late(socket, "");
				size_t idx = first_index;
				size_t last = ls.size() - 1;
				int state;
				for (; idx < last; idx++)
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
				return zmq_socket_state::succeed;
			}
		};

		namespace zmq_monitor
		{
			/**
			* \brief 网络监控
			* \para {int} _fd Socket句柄
			*/
			bool print_client_addr(int fd, acl::string& str);
			/**
			* \brief 网络监控
			* \param {shared_char} station 站点名称
			* \param {ZMQ_HANDLE} socket socket
			* \param {shared_char} addr 地址
			*/
			void set_monitor(const char* station, zmq_handler* socket, const char* type);
		}

	}
}

#endif//!_ZMQ_EXTEND_H_
