#pragma once
#ifndef _ZMQ_EXTEND_H_
#include <stdinc.h>

namespace agebull
{
	namespace zmq_net
	{

#define STATION_TYPE_DISPATCHER 1
#define STATION_TYPE_MONITOR 2
#define STATION_TYPE_API 3
#define STATION_TYPE_VOTE 4
#define STATION_TYPE_PUBLISH 5



		/**
		*@brief 广播内容
		*/
		bool monitor(string publiher, string state, string content);
		/**
		* \brief  站点状态
		*/
		enum class station_state
		{
			/**
			* \brief 无，刚构造
			*/
			None,
			/**
			* \brief 正在启动
			*/
			Start,
			/**
			* \brief 正在运行
			*/
			Run,
			/**
			* \brief 已暂停
			*/
			Pause,
			/**
			* \brief 将要关闭
			*/
			Closing,
			/**
			* \brief 错误状态
			*/
			Failed,
			/**
			* \brief 已关闭
			*/
			Closed,
			/**
			* \brief 已销毁，析构已调用
			*/
			Destroy
		};

		/**
		* @brief ZMQ套接字状态
		*/
		enum class ZmqSocketState
		{
			/**
			* @brief 没问题
			*/
			Succeed,
			/**
			* @brief 后续还有消息
			*/
			More,

			/**
			* @brief 空帧
			*/
			Empty,

			/**
			* @brief 主机不可达
			*/
			HostUnReach,
			/**
			* @brief 网络关闭
			*/
			NetDown,

			/**
			* @brief 网络不可达
			*/
			NetUnReach,

			/**
			* @brief 网络重置
			*/
			NetReset,

			/**
			* @brief 未连接
			*/
			NotConn,
			/**
			* @brief 连接已在使用中？
			*/
			ConnRefUsed,
			/**
			* @brief 连接中断
			*/
			ConnAborted,

			/**
			* @brief 连接重置
			*/
			ConnReset,

			/**
			* @brief 超时
			*/
			TimedOut,

			/**
			* @brief 正在处理中？
			*/
			InProgress,

			/**
			* @brief 跨线程调用？
			*/
			Mthread,

			/**
			* @brief 指定的socket不可用
			*/
			NotSocket,

			/**
			* @brief 内存不足
			*/
			NoBufs,

			/**
			* @brief 消息大小不合适？
			*/
			MsgSize,

			/**
			* @brief 指定的socket相关联的context已关闭
			*/
			Term,

			/**
			* @brief 系统信号中断
			*/
			Intr,

			/**
			* @brief 不支持？
			*/
			NotSup,

			/**
			* @brief 不支持的协议
			*/
			ProtoNoSupport,

			/**
			* @brief 协议不兼容
			*/
			NoCompatProto,

			/**
			* @brief ？
			*/
			AfNoSupport,

			/**
			* @brief 地址问题？
			*/
			AddrNotAvAll,
			/**
			* @brief 地址已被使用
			*/
			AddrInUse,
			/**
			* @brief ？
			*/
			Fsm,

			/**
			* @brief 重启
			*/
			Again,
			/**
			* @brief 其它错误
			*/
			Unknow
		};

#define check_zmq_state()\
	if (_zmq_state > ZmqSocketState::Succeed)\
	{\
		if (_zmq_state < ZmqSocketState::Again)\
			continue;\
		break;\
	}


#define close_socket(socket,addr)\
		zmq_unbind(socket, addr.c_str());\
		zmq_close(socket);\
		socket = nullptr




		//生成ZMQ连接对象
		inline void setsockopt(ZMQ_HANDLE socket)
		{
			//zmq_result = zmq_socket_monitor(socket, "inproc://server_cmd.rep", ZMQ_EVENT_ALL);
			//assert(zmq_result == 0);
			int iZMQ_IMMEDIATE = 1;//列消息只作用于已完成的链接
			zmq_setsockopt(socket, ZMQ_IMMEDIATE, &iZMQ_IMMEDIATE, sizeof(int));
			int iLINGER = 500;//关闭设置停留时间,毫秒
			zmq_setsockopt(socket, ZMQ_LINGER, &iLINGER, sizeof(int));
			int iRcvTimeout = 500;
			zmq_setsockopt(socket, ZMQ_RCVTIMEO, &iRcvTimeout, sizeof(int));
			int iSndTimeout = 500;
			zmq_setsockopt(socket, ZMQ_SNDTIMEO, &iSndTimeout, sizeof(int));
		}

		//生成ZMQ连接对象
		inline ZMQ_HANDLE create_req_socket(const char* addr, int type, const char* name)
		{
			ZMQ_HANDLE socket = zmq_socket(get_zmq_context(), type);
			if (socket == nullptr)
			{
				return nullptr;
			}
			if (name != nullptr)
				zmq_setsockopt(socket, ZMQ_IDENTITY, name, strlen(name));


			int iZMQ_IMMEDIATE = 1;//列消息只作用于已完成的链接
			zmq_setsockopt(socket, ZMQ_IMMEDIATE, &iZMQ_IMMEDIATE, sizeof(int));
			int iLINGER = 5000;//关闭设置停留时间,毫秒
			zmq_setsockopt(socket, ZMQ_LINGER, &iLINGER, sizeof(int));
			int iRcvTimeout = 5000;
			zmq_setsockopt(socket, ZMQ_RCVTIMEO, &iRcvTimeout, sizeof(int));
			int iSndTimeout = 5000;
			zmq_setsockopt(socket, ZMQ_SNDTIMEO, &iSndTimeout, sizeof(int));

			int zmq_result = zmq_connect(socket, addr);
			if (zmq_result >= 0)
				return socket;
			zmq_close(socket);
			return nullptr;
		}
		/**
		* \brief 生成用于调用的套接字
		*/
		inline ZMQ_HANDLE create_inproc_socket(const char* name, const char* host)
		{
			char address[MAX_PATH];
			sprintf_s(address, "inproc://%s", host);
			return create_req_socket(address, ZMQ_REQ, name);
		}

		//生成ZMQ连接对象(tcp)
		inline ZMQ_HANDLE create_server_socket(const char* name, int type, int port)
		{
			char host[MAX_PATH];
			sprintf_s(host, "tcp://*:%d", port);
			ZMQ_HANDLE socket = zmq_socket(get_zmq_context(), type);
			if (socket == nullptr)
			{
				log_error2("构造对象(%s)发生错误:%s", host, zmq_strerror(zmq_errno()));
				return nullptr;
			}

#if WIN32
			SOCKET fd;
			size_t sz = sizeof(SOCKET);
#else
			int fd;
			size_t sz = sizeof(int);
#endif

			zmq_getsockopt(socket, ZMQ_FD, &fd, &sz);
			int nodelay = 1;
			int rc = setsockopt(fd, IPPROTO_TCP, TCP_NODELAY, reinterpret_cast<char*>(&nodelay), sizeof(int));
			assert(rc == 0);
			setsockopt(socket);

			int zmq_result = zmq_bind(socket, host);
			if (zmq_result >= 0)
				return socket;
			zmq_close(socket);
			log_error2("绑定地址(%s)发生错误:%s", host, zmq_strerror(zmq_errno()));
			return nullptr;
		}

		//生成ZMQ连接对象(inproc)
		inline ZMQ_HANDLE create_server_socket(const char* name, int type)
		{
			char host[MAX_PATH];
			sprintf_s(host, "inproc://%s", name);
			ZMQ_HANDLE socket = zmq_socket(get_zmq_context(), type);
			if (socket == nullptr)
			{
				log_error2("构造对象(%s)发生错误:%s", host, zmq_strerror(zmq_errno()));
				return nullptr;
			}

			setsockopt(socket);

			int zmq_result = zmq_bind(socket, host);
			if (zmq_result >= 0)
				return socket;
			zmq_close(socket);
			log_error2("绑定地址(%s)发生错误:%s", host, zmq_strerror(zmq_errno()));
			return nullptr;
		}

		/**
		* \brief 检查ZMQ错误状态
		* \return 状态
		*/
		inline ZmqSocketState check_zmq_error()
		{
			int err = zmq_errno();
			switch (err)
			{
			case 0:
				return ZmqSocketState::Empty;
			case ETERM:
				return ZmqSocketState::Intr;
			case ENOTSOCK:
				return ZmqSocketState::NotSocket;
			case EINTR:
				return ZmqSocketState::Intr;
			case EAGAIN:
			case ETIMEDOUT:
				return ZmqSocketState::TimedOut;
				//return ZmqSocketState::TimedOut;
			case ENOTSUP:
				return ZmqSocketState::NotSup;
			case EPROTONOSUPPORT:
				return ZmqSocketState::ProtoNoSupport;
			case ENOBUFS:
				return ZmqSocketState::NoBufs;
			case ENETDOWN:
				return ZmqSocketState::NetDown;
			case EADDRINUSE:
				return ZmqSocketState::AddrInUse;
			case EADDRNOTAVAIL:
				return ZmqSocketState::AddrNotAvAll;
			case ECONNREFUSED:
				return ZmqSocketState::ConnRefUsed;
			case EINPROGRESS:
				return ZmqSocketState::InProgress;
			case EMSGSIZE:
				return ZmqSocketState::MsgSize;
			case EAFNOSUPPORT:
				return ZmqSocketState::AfNoSupport;
			case ENETUNREACH:
				return ZmqSocketState::NetUnReach;
			case ECONNABORTED:
				return ZmqSocketState::ConnAborted;
			case ECONNRESET:
				return ZmqSocketState::ConnReset;
			case ENOTCONN:
				return ZmqSocketState::NotConn;
			case EHOSTUNREACH:
				return ZmqSocketState::HostUnReach;
			case ENETRESET:
				return ZmqSocketState::NetReset;
			case EFSM:
				return ZmqSocketState::Fsm;
			case ENOCOMPATPROTO:
				return ZmqSocketState::NoCompatProto;
			case EMTHREAD:
				return ZmqSocketState::Mthread;
			default:
				return ZmqSocketState::Unknow;
			}
		}

		/**
		* \brief 检查ZMQ错误状态
		* \return 状态
		*/
		inline const char* state_str(ZmqSocketState state)
		{
			switch (state)
			{
			case ZmqSocketState::Succeed: return "Succeed";
			case ZmqSocketState::More: return "More";
			case ZmqSocketState::Empty: return "Empty";
			case ZmqSocketState::HostUnReach: return "HostUnReach";
			case ZmqSocketState::NetDown: return "NetDown";
			case ZmqSocketState::NetUnReach: return "NetUnReach";
			case ZmqSocketState::NetReset: return "NetReset";
			case ZmqSocketState::NotConn: return "NotConn";
			case ZmqSocketState::ConnRefUsed: return "ConnRefUsed";
			case ZmqSocketState::ConnAborted: return "ConnAborted";
			case ZmqSocketState::ConnReset: return "ConnReset";
			case ZmqSocketState::TimedOut: return "TimedOut";
			case ZmqSocketState::InProgress: return "InProgress";
			case ZmqSocketState::Mthread: return "Mthread";
			case ZmqSocketState::NotSocket: return "NotSocket";
			case ZmqSocketState::NoBufs: return "NoBufs";
			case ZmqSocketState::MsgSize: return "MsgSize";
			case ZmqSocketState::Term: return "Term";
			case ZmqSocketState::Intr: return "Intr";
			case ZmqSocketState::NotSup: return "NotSup";
			case ZmqSocketState::ProtoNoSupport: return "ProtoNoSupport";
			case ZmqSocketState::NoCompatProto: return "NoCompatProto";
			case ZmqSocketState::AfNoSupport: return "AfNoSupport";
			case ZmqSocketState::AddrNotAvAll: return "AddrNotAvAll";
			case ZmqSocketState::AddrInUse: return "AddrInUse";
			case ZmqSocketState::Fsm: return "Fsm";
			case ZmqSocketState::Again: return "Again";
			case ZmqSocketState::Unknow: return "Unknow";
			default:return "*";
			}
		}
		/**
		* @brief 接收
		*/
		inline ZmqSocketState recv(ZMQ_HANDLE socket, sharp_char& data, int flag = 0)
		{
			//接收命令请求
			zmq_msg_t msg_call;
			int state = zmq_msg_init(&msg_call);
			if (state < 0)
			{
				return ZmqSocketState::NoBufs;
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
			return more == 0 ? ZmqSocketState::Succeed : ZmqSocketState::More;
		}

		/**
		* @brief 接收
		*/
		inline ZmqSocketState recv(ZMQ_HANDLE socket, vector<sharp_char>& ls, int flag = 0)
		{
			int more;
			do
			{
				zmq_msg_t msg;
				int re = zmq_msg_init(&msg);
				if (re < 0)
				{
					return ZmqSocketState::NoBufs;
				}
				re = zmq_msg_recv(&msg, socket, flag);
				if (re < 0)
				{
					return check_zmq_error();
				}
				if (re == 0)
					ls.push_back(sharp_char());
				else
					ls.push_back(sharp_char(msg));
				zmq_msg_close(&msg);
				size_t size = sizeof(int);
				zmq_getsockopt(socket, ZMQ_RCVMORE, &more, &size);
			} while (more != 0);
			return ZmqSocketState::Succeed;
		}

		/**
		* @brief 发送最后一帧
		*/
		inline ZmqSocketState send_late(ZMQ_HANDLE socket, const char* string)
		{
			int state = zmq_send(socket, string, strlen(string), ZMQ_DONTWAIT);
			if (state < 0)
			{
				return check_zmq_error();
			}
			return ZmqSocketState::Succeed;
		}

		/**
		* @brief 发送帧
		*/
		inline ZmqSocketState send_more(ZMQ_HANDLE socket, const char* string)
		{
			int state = zmq_send(socket, string, strlen(string), ZMQ_SNDMORE);
			if (state < 0)
			{
				return check_zmq_error();
			}
			return ZmqSocketState::Succeed;
		}
		/**
		* @brief 发送帧
		*/
		inline ZmqSocketState send_addr(ZMQ_HANDLE socket, const char* addr)
		{
			int state = zmq_send(socket, addr, strlen(addr), ZMQ_SNDMORE);
			if (state < 0)
			{
				return check_zmq_error();
			}
			state = zmq_send(socket, "", 0, ZMQ_SNDMORE);
			if (state < 0)
			{
				return check_zmq_error();
			}
			return ZmqSocketState::Succeed;
		}

		/**
		* @brief 发送
		*/
		inline ZmqSocketState send(ZMQ_HANDLE socket, vector<sharp_char>::iterator iter, vector<sharp_char>::iterator end)
		{
			size_t idx = 0;
			while (iter != end)
			{
				int state = zmq_send(socket, iter->operator*(), iter->size(), ZMQ_SNDMORE);
				if (state < 0)
				{
					return check_zmq_error();
				}
				++iter;
			}
			return send_late(socket, "");
		}
		/**
		* @brief 发送
		*/
		inline ZmqSocketState send(ZMQ_HANDLE socket, vector<sharp_char>& ls)
		{
			size_t idx = 0;
			for (; idx < ls.size() - 1; idx++)
			{
				int state = zmq_send(socket, *ls[idx], ls[idx].size(), ZMQ_SNDMORE);
				if (state < 0)
				{
					return check_zmq_error();
				}
			}
			return send_late(socket, *ls[idx]);
		}
		/**
		* @brief 发送
		*/
		inline ZmqSocketState send(ZMQ_HANDLE socket, vector<string>& ls)
		{
			size_t idx = 0;
			for (; idx < ls.size() - 1; idx++)
			{
				int state = zmq_send(socket, ls[idx].c_str(), ls[idx].length(), ZMQ_SNDMORE);
				if (state < 0)
				{
					return check_zmq_error();
				}
			}
			return send_late(socket, ls[idx].c_str());
		}

		/**
		 * @brief 连接的SOCKET简单封装
		 */
		template<int zmq_type = ZMQ_REQ, bool is_tcp = false>
		class RequestSocket
		{
			char _address[MAX_PATH];
			ZMQ_HANDLE _socket;
			ZmqSocketState _state;
#ifdef TIMER
			char _station[MAX_PATH];
#endif
		public:
			RequestSocket(const char* name, const char* station)
				:_state(ZmqSocketState::Succeed)
			{
				sprintf_s(_address, "%s://%s", is_tcp ? "tcp" : "inproc", station);
				_socket = create_req_socket(_address, zmq_type, name);
				if (is_tcp)
				{
#if WIN32
					SOCKET fd;
					size_t sz = sizeof(SOCKET);
#else
					int fd;
					size_t sz = sizeof(int);
#endif

					zmq_getsockopt(_socket, ZMQ_FD, &fd, &sz);
					int nodelay = 1;
					int rc = setsockopt(fd, IPPROTO_TCP, TCP_NODELAY, reinterpret_cast<char*>(&nodelay), sizeof(int));
					assert(rc == 0);
				}
#ifdef TIMER
				strcpy(_station, station);
#endif
			}
			~RequestSocket()
			{
				zmq_disconnect(_socket, _address);
				zmq_close(_socket);
			}
			ZmqSocketState get_state() const
			{
				return _state;
			}
			/**
			* @brief 接收
			*/
			ZmqSocketState recv(sharp_char& data, int flag = 0) const
			{
				return agebull::zmq_net::recv(_socket, data, flag);
			}

			/**
			* @brief 接收
			*/
			ZmqSocketState recv(vector<sharp_char>& ls, int flag = 0) const
			{
				return agebull::zmq_net::recv(_socket, ls, flag);
			}

			/**
			* @brief 发送
			*/
			ZmqSocketState send_late(const char* string) const
			{
				return agebull::zmq_net::send_late(_socket, string);
			}

			/**
			* @brief 发送帧
			*/
			ZmqSocketState send_more(const char* string) const
			{
				return agebull::zmq_net::send_more(_socket, string);
			}
			/**
			* @brief 发送
			*/
			ZmqSocketState send(vector<sharp_char>& ls) const
			{
				return agebull::zmq_net::send(_socket, ls);
			}
			/**
			* @brief 发送
			*/
			ZmqSocketState send(vector<string>& ls) const
			{
				return agebull::zmq_net::send(_socket, ls);
			}
			/**
			* @brief 进行一次请求
			* @return 如果返回为false,请检查state.
			*/
			bool request(vector<string>& arguments, vector<sharp_char>& result, int retry = 3)
			{
#ifdef TIMER
				boost::posix_time::ptime start = boost::posix_time::microsec_clock::universal_time();
#endif
				_state = send(arguments);

#ifdef TIMER
				log_debug2(DEBUG_TIMER, 3, "【%s】 send-%d-ns", _station, (boost::posix_time::microsec_clock::universal_time() - start).total_microseconds());
#endif

				if (_state != ZmqSocketState::Succeed)
					return false;

#ifdef TIMER
				start = boost::posix_time::microsec_clock::universal_time();
#endif
				int cnt = 0;
				do
				{
					_state = recv(result);
					if (_state == ZmqSocketState::Succeed)
					{
						break;
					}
				} while (_state == ZmqSocketState::TimedOut && ++cnt < retry);

#ifdef TIMER
				log_debug2(DEBUG_TIMER, 3, "【%s】 recv-%d-ns", _station, (boost::posix_time::microsec_clock::universal_time() - start).total_microseconds());
#endif
				return _state == ZmqSocketState::Succeed;
			}
		};


		/**
		* \brief 计划类型
		*/
		enum class plan_date_type
		{
			/**
			* \brief 无计划，立即发送
			*/
			None,
			/**
			* \brief 在指定的时间发送
			*/
			Time,
			/**
			* \brief 分钟间隔后发送
			*/
			Minute,
			/**
			* \brief 小时间隔后发送
			*/
			Hour,
			/**
			* \brief 日间隔后发送
			*/
			Day,
			/**
			* \brief 周间隔后发送
			*/
			Week,
			/**
			* \brief 月间隔后发送
			*/
			Month,
			/**
			* \brief 年间隔后发送
			*/
			Year
		};
		struct PlanMessage
		{
			/**
			* \brief 消息标识
			*/
			size_t plan_id;

			/**
			* \brief 计划类型
			*/
			plan_date_type plan_type;

			/**
			* \brief 类型值
			*/
			int plan_value;

			/**
			* \brief 重复次数,0不重复 >0重复次数,-1永久重复
			*/
			int plan_repet;

			/**
			* \brief 执行次数
			*/
			size_t real_repet;

			/**
			* \brief 发起者
			*/
			string caller;
			/**
			* \brief 消息内容
			*/
			vector<string> messages;
		};
	}
}

#endif
