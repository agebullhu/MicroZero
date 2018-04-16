#pragma once
#ifndef _ZMQ_EXTEND_H_
#include <stdinc.h>
#include <command.h>

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
		*\brief 广播内容
		*/
		bool monitor_async(string publiher, string state, string content);
		/**
		*\brief 广播内容
		*/
		bool monitor_sync(string publiher, string state, string content);
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

#define check_zmq_state()\
	if (_zmq_state > zmq_socket_state::Succeed)\
	{\
		if (_zmq_state < zmq_socket_state::Again)\
			continue;\
		break;\
	}


		inline void close_socket(ZMQ_HANDLE& socket, const char*  addr)
		{
			zmq_unbind(socket, addr);
			zmq_close(socket);
			socket = nullptr;
		}




		/**
		* \brief 配置ZMQ连接对象
		* \param socket
		* \param name
		* \return
		*/
		inline void setsockopt(ZMQ_HANDLE socket, const char* name = nullptr)
		{
			if (name != nullptr)
				zmq_setsockopt(socket, ZMQ_IDENTITY, name, strlen(name));
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

		/**
		* \brief 生成ZMQ连接对象
		* \param addr
		* \param type
		* \param name
		* \return
		*/
		inline ZMQ_HANDLE create_req_socket(const char* addr, int type, const char* name)
		{
			ZMQ_HANDLE socket = zmq_socket(get_zmq_context(), type);
			if (socket == nullptr)
			{
				return nullptr;
			}
			setsockopt(socket, name);
			if (zmq_connect(socket, addr) >= 0)
				return socket;
			zmq_close(socket);
			return nullptr;
		}

		/**
		* \brief 生成ZMQ连接对象
		* \param addr
		* \param type
		* \param name
		* \return
		*/
		inline ZMQ_HANDLE create_res_socket(const char* addr, int type, const char* name)
		{
			ZMQ_HANDLE socket = zmq_socket(get_zmq_context(), type);
			if (socket == nullptr)
			{
				return nullptr;
			}
			setsockopt(socket, name);
			if (zmq_bind(socket, addr) >= 0)
				return socket;
			zmq_close(socket);
			return nullptr;
		}

		/**
		* \brief 生成用于TCP的套接字
		*/
		inline bool set_tcp_nodelay(ZMQ_HANDLE socket)
		{
#ifdef _WINDOWS
			SOCKET fd;
			size_t sz = sizeof(SOCKET);
#else
			int fd;
			size_t sz = sizeof(int);
#endif
			zmq_getsockopt(socket, ZMQ_FD, &fd, &sz);
			int nodelay = 1;
			return setsockopt(fd, IPPROTO_TCP, TCP_NODELAY, reinterpret_cast<char*>(&nodelay), sizeof(int)) >= 0;
		}

		/**
		* \brief 生成用于TCP的套接字
		*/
		inline ZMQ_HANDLE create_res_socket_tcp(const char* name, int type, int port)
		{
			char host[MAX_PATH];
			sprintf_s(host, "tcp://*:%d", port);
			ZMQ_HANDLE socket = create_res_socket(host, type, name);
			if (socket == nullptr)
			{
				return nullptr;
			}
			if (!set_tcp_nodelay(socket))
				log_error2("地址(%s)配置TCP_NODELAY失败:%s", host, zmq_strerror(zmq_errno()));
			return socket;
		}
		/**
		* \brief 生成用于TCP的套接字
		*/
		inline ZMQ_HANDLE create_req_socket_tcp(const char* name, int type, int port)
		{
			char host[MAX_PATH];
			sprintf_s(host, "tcp://*:%d", port);
			ZMQ_HANDLE socket = create_req_socket(host, type, name);
			if (socket == nullptr)
			{
				return nullptr;
			}
			if (!set_tcp_nodelay(socket))
				log_error2("地址(%s)配置TCP_NODELAY失败:%s", host, zmq_strerror(zmq_errno()));
			return socket;
		}
		/**
		* \brief 生成用于INPROC调用的套接字
		*/
		inline ZMQ_HANDLE create_req_socket_inproc(const char* name, const char* host)
		{
			char address[MAX_PATH];
			sprintf_s(address, "inproc://%s", host);
			return create_req_socket(address, ZMQ_REQ, name);
		}
		//生成ZMQ连接对象(inproc)
		inline ZMQ_HANDLE create_res_socket_inproc(const char* name, int type)
		{
			char host[MAX_PATH];
			sprintf_s(host, "inproc://%s", name);
			return create_res_socket(host, type, name);
		}


		/**
		* \brief 生成的套接字
		*/
		inline ZMQ_HANDLE create_res_socket(const char* name, int type, int port)
		{
			if (config::get_bool("use_ipc_protocol"))
			{
				char host[MAX_PATH];
				sprintf_s(host, "ipc:///%s/%d", name, port);
				return create_res_socket(host, type, name);
			}
			return create_res_socket_tcp(name, type, port);
		}

		/**
		* \brief 检查ZMQ错误状态
		* \return 状态
		*/
		inline zmq_socket_state check_zmq_error()
		{
			const int err = zmq_errno();
			switch (err)
			{
			case 0:
				return zmq_socket_state::Empty;
			case ETERM:
				return zmq_socket_state::Intr;
			case ENOTSOCK:
				return zmq_socket_state::NotSocket;
			case EINTR:
				return zmq_socket_state::Intr;
			case EAGAIN:
			case ETIMEDOUT:
				return zmq_socket_state::TimedOut;
				//return ZmqSocketState::TimedOut;
			case ENOTSUP:
				return zmq_socket_state::NotSup;
			case EPROTONOSUPPORT:
				return zmq_socket_state::ProtoNoSupport;
			case ENOBUFS:
				return zmq_socket_state::NoBufs;
			case ENETDOWN:
				return zmq_socket_state::NetDown;
			case EADDRINUSE:
				return zmq_socket_state::AddrInUse;
			case EADDRNOTAVAIL:
				return zmq_socket_state::AddrNotAvAll;
			case ECONNREFUSED:
				return zmq_socket_state::ConnRefUsed;
			case EINPROGRESS:
				return zmq_socket_state::InProgress;
			case EMSGSIZE:
				return zmq_socket_state::MsgSize;
			case EAFNOSUPPORT:
				return zmq_socket_state::AfNoSupport;
			case ENETUNREACH:
				return zmq_socket_state::NetUnReach;
			case ECONNABORTED:
				return zmq_socket_state::ConnAborted;
			case ECONNRESET:
				return zmq_socket_state::ConnReset;
			case ENOTCONN:
				return zmq_socket_state::NotConn;
			case EHOSTUNREACH:
				return zmq_socket_state::HostUnReach;
			case ENETRESET:
				return zmq_socket_state::NetReset;
			case EFSM:
				return zmq_socket_state::Fsm;
			case ENOCOMPATPROTO:
				return zmq_socket_state::NoCompatProto;
			case EMTHREAD:
				return zmq_socket_state::Mthread;
			default:
				return zmq_socket_state::Unknow;
			}
		}

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
		* \brief 接收
		*/
		inline zmq_socket_state recv(ZMQ_HANDLE socket, sharp_char& data, int flag = 0)
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
		inline zmq_socket_state recv(ZMQ_HANDLE socket, vector<sharp_char>& ls, int flag = 0)
		{
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
				size_t size = sizeof(int);
				zmq_getsockopt(socket, ZMQ_RCVMORE, &more, &size);
			} while (more != 0);
			return zmq_socket_state::Succeed;
		}

		/**
		* \brief 发送最后一帧
		*/
		inline zmq_socket_state send_late(ZMQ_HANDLE socket, const char* string)
		{
			int state = zmq_send(socket, string, strlen(string), ZMQ_DONTWAIT);
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
			int state = zmq_send(socket, string, strlen(string), ZMQ_SNDMORE);
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
			state = zmq_send(socket, "", 0, ZMQ_SNDMORE);
			if (state < 0)
			{
				return check_zmq_error();
			}
			return zmq_socket_state::Succeed;
		}

		/**
		* \brief 发送
		*/
		inline zmq_socket_state send(ZMQ_HANDLE socket, vector<sharp_char>::iterator iter, const vector<sharp_char>::iterator& end)
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
		* \brief 发送
		*/
		inline zmq_socket_state send(ZMQ_HANDLE socket, vector<sharp_char>& ls, int first_index = 0)
		{
			size_t idx = first_index;
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
		* \brief 发送
		*/
		inline zmq_socket_state send(ZMQ_HANDLE socket, vector<string>& ls)
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
		 * \brief 连接的SOCKET简单封装
		 */
		template<int zmq_type = ZMQ_REQ>
		class inproc_request_socket
		{
			char _address[MAX_PATH];
			ZMQ_HANDLE _socket;
			zmq_socket_state _state;
#ifdef TIMER
			char _station[MAX_PATH];
#endif
		public:
			/**
			 * \brief
			 * \param name
			 * \param station
			 */
			inproc_request_socket(const char* name, const char* station);

			~inproc_request_socket()
			{
				zmq_disconnect(_socket, _address);
				zmq_close(_socket);
			}
			zmq_socket_state get_state() const
			{
				return _state;
			}
			/**
			* \brief 接收
			*/
			zmq_socket_state recv(sharp_char& data, int flag = 0) const
			{
				return agebull::zmq_net::recv(_socket, data, flag);
			}

			/**
			* \brief 接收
			*/
			zmq_socket_state recv(vector<sharp_char>& ls, int flag = 0) const
			{
				return agebull::zmq_net::recv(_socket, ls, flag);
			}

			/**
			* \brief 发送
			*/
			zmq_socket_state send_late(const char* string) const
			{
				return agebull::zmq_net::send_late(_socket, string);
			}

			/**
			* \brief 发送帧
			*/
			zmq_socket_state send_more(const char* string) const
			{
				return agebull::zmq_net::send_more(_socket, string);
			}
			/**
			* \brief 发送
			*/
			zmq_socket_state send(vector<sharp_char>& ls) const
			{
				return agebull::zmq_net::send(_socket, ls);
			}
			/**
			* \brief 发送
			*/
			zmq_socket_state send(vector<string>& ls) const
			{
				return agebull::zmq_net::send(_socket, ls);
			}
			/**
			* \brief 进行一次请求
			* @return 如果返回为false,请检查state.
			*/
			bool request(vector<sharp_char>& arguments, vector<sharp_char>& result, int retry = 3)
			{
#ifdef TIMER
				boost::posix_time::ptime start = boost::posix_time::microsec_clock::universal_time();
#endif
				_state = send(arguments);

#ifdef TIMER
				log_debug2(DEBUG_TIMER, 3, "【%s】 send-%d-ns", _station, (boost::posix_time::microsec_clock::universal_time() - start).total_microseconds());
#endif

				if (_state != zmq_socket_state::Succeed)
					return false;

#ifdef TIMER
				start = boost::posix_time::microsec_clock::universal_time();
#endif
				int cnt = 0;
				do
				{
					_state = recv(result);
					if (_state == zmq_socket_state::Succeed)
					{
						break;
					}
				} while (_state == zmq_socket_state::TimedOut && ++cnt < retry);

#ifdef TIMER
				log_debug2(DEBUG_TIMER, 3, "【%s】 recv-%d-ns", _station, (boost::posix_time::microsec_clock::universal_time() - start).total_microseconds());
#endif
				return _state == zmq_socket_state::Succeed;
			}
			/**
			* \brief 进行一次请求
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

				if (_state != zmq_socket_state::Succeed)
					return false;

#ifdef TIMER
				start = boost::posix_time::microsec_clock::universal_time();
#endif
				int cnt = 0;
				do
				{
					_state = recv(result);
					if (_state == zmq_socket_state::Succeed)
					{
						break;
					}
				} while (_state == zmq_socket_state::TimedOut && ++cnt < retry);

#ifdef TIMER
				log_debug2(DEBUG_TIMER, 3, "【%s】 recv-%d-ns", _station, (boost::posix_time::microsec_clock::universal_time() - start).total_microseconds());
#endif
				return _state == zmq_socket_state::Succeed;
			}
		};

		template <int zmq_type>
		inproc_request_socket<zmq_type>::inproc_request_socket(const char* name, const char* station)
			: _state(zmq_socket_state::Succeed)
		{
			sprintf_s(_address, "inproc://%s", station);
#ifdef TIMER
			strcpy(_station, station);
#endif
		}


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
		/**
		* \brief 消息
		*/
		struct plan_message
		{
			/**
			* \brief 消息标识
			*/
			uint32_t plan_id;

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
			int real_repet;

			/**
			* \brief 发起者提供的标识
			*/
			string request_id;

			/**
			* \brief 发起者
			*/
			string request_caller;
			/**
			* \brief 消息描述
			*/
			sharp_char messages_description;

			/**
			* \brief 消息内容
			*/
			vector<sharp_char> messages;

			/**
			* \brief 从JSON中反序列化
			*/
			void read_plan(const char* plan)
			{
				acl::json json;
				json.update(plan);
				acl::json_node* iter = json.first_node();
				while (iter)
				{
					int idx = strmatchi(5, iter->tag_name(), "plan_type", "plan_value", "plan_repet");
					switch (idx)
					{
					case 0:
						plan_type = static_cast<plan_date_type>(static_cast<int>(*iter->get_int64()));
						break;
					case 1:
						plan_value = static_cast<int>(*iter->get_int64());
						break;
					case 2:
						plan_repet = static_cast<int>(*iter->get_int64());
						break;
					default: break;
					}
					iter = json.next_node();
				}
			}
			void read_json(acl::string& val)
			{
				acl::json json;
				json.update(val);
				acl::json_node* iter = json.first_node();
				while (iter)
				{
					int idx = strmatchi(5, iter->tag_name(), "plan_id", "plan_type", "plan_value", "plan_repet", "real_repet", "request_caller", "request_id", "messages_description", "messages");
					switch (idx)
					{
					case 0:
						plan_id = static_cast<int>(*iter->get_int64());
						break;
					case 1:
						plan_type = static_cast<plan_date_type>(static_cast<int>(*iter->get_int64()));
						break;
					case 2:
						plan_value = static_cast<int>(*iter->get_int64());
						break;
					case 3:
						plan_repet = static_cast<int>(*iter->get_int64());
						break;
					case 4:
						real_repet = static_cast<int>(*iter->get_int64());
						break;
					case 5:
						request_caller = iter->get_string();
						break;
					case 6:
						request_id = iter->get_string();
						break;
					case 7:
						messages_description = iter->get_string();
						break;
					case 8:
					{
						acl::json arr = iter->get_json();
						acl::json_node* iter_arr = arr.first_node();
						while (iter_arr)
						{
							messages.emplace_back(iter_arr->get_string());
							iter_arr = arr.next_node();
						}
					}
					break;
					default: break;
					}
					iter = json.next_node();
				}
			}
			acl::string write_json()
			{
				acl::json json;
				acl::json_node& node = json.create_node();
				node.add_number("plan_id", plan_id);
				if (plan_type > plan_date_type::None)
				{
					node.add_number("plan_type", static_cast<int>(plan_type));
					node.add_number("plan_value", plan_value);
					node.add_number("plan_repet", plan_repet);
					node.add_number("real_repet", real_repet);
				}
				node.add_text("request_id", request_id.c_str());
				node.add_text("request_caller", request_caller.c_str());
				node.add_text("messages_description", messages_description.get_buffer());
				acl::json_node& array = node.add_array(true);
				array.set_tag("messages");
				for (const auto& line : messages)
				{
					array.add_array_text(*line);
				}
				return node.to_string();
			}
		};
	}
}

#endif
