#pragma once
#ifndef _ZERO_CONFIG_H_
#define _ZERO_CONFIG_H_
#include "../stdinc.h"
#include <utility>

namespace agebull
{
	namespace zmq_net
	{
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
			* \brief 重新启动
			*/
			ReStart,
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
			* \brief 错误状态
			*/
			Failed,
			/**
			* \brief 将要关闭
			*/
			Closing,
			/**
			* \brief 已关闭
			*/
			Closed,
			/**
			* \brief 已销毁，析构已调用
			*/
			Destroy,
			/**
			* \brief 已卸载
			*/
			Uninstall,
			/**
			* \brief 未知
			*/
			Unknow
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
		* \brief 工作对象
		*/
		struct worker
		{
			/**
			* \brief 上报的IP地址
			*/
			string ips;


			/**
			* \brief 上次心跳的时间
			*/
			time_t pre_time;

			/**
			* \brief 健康等级
			*/
			int level;

			/**
			 * \brief 构造
			 */
			worker()
				: pre_time(0)
				, level(-1)
			{

			}
			/**
			* \brief 激活
			*/
			void active()
			{
				pre_time = time(nullptr);
				level = 10;
			}
			/**
			* \brief 检查
			*/
			int check()
			{
				const int64 tm = time(nullptr) - pre_time;
				if (tm <= 1)
					return level = 10;
				if (tm <= 3)
					return level = 9;
				if (tm <= 5)
					return level = 8;
				if (tm <= 10)
					return level = 7;
				if (tm <= 30)
					return level = 6;
				if (tm <= 60)
					return level = 5;
				if (tm <= 120)
					return level = 4;
				if (tm <= 180)
					return level = 3;
				if (tm <= 240)
					return level = 2;
				if (tm <= 360)
					return level = 1;
				return level = -1;
			}
		};
		class zero_station;
		/**
		* \brief ZMQ的网络站点配置
		*/
		class zero_config
		{
			friend class zero_station;
		public:
			/**
			* \brief 实例队列访问锁
			*/
			boost::mutex mutex_;
			/**
			* \brief 站点名称
			*/
			string station_name_;
			/**
			* \brief 站点名称
			*/
			string short_name;
			/**
			* \brief 站点标题
			*/
			string station_caption_;
			/**
			* \brief 站点说明
			*/
			string station_description_;
			/**
			* \brief 站点别名
			*/
			vector<string> alias_;

			/**
			* \brief 站点类型
			*/
			int station_type_;

			/**
			* \brief 外部地址
			*/
			int request_port_;

			/**
			* \brief 工作地址
			*/
			int worker_port_;

			/**
			* \brief 当前站点状态
			*/
			station_state station_state_;
			/**
			* \brief 总请求次数
			*/
			int64 request_in, request_out, request_err;
			/**
			* \brief 总返回次数
			*/
			int64 worker_in, worker_out, worker_err;

			map<string, worker> workers;

			/**
			* \brief 构造
			*/
			zero_config()
				: station_type_(0)
				, request_port_(0)
				, worker_port_(0)
				, station_state_(station_state::None)
				, request_in(0)
				, request_out(0)
				, request_err(0)
				, worker_in(0)
				, worker_out(0)
				, worker_err(0)
			{

			}

			/**
			* \brief 构造
			* \param name
			* \param type
			*/
			zero_config(const string& name, int type)
				: station_name_(std::move(name))
				, request_port_(0)
				, worker_port_(0)
				, station_state_(station_state::None)
				, request_in(0)
				, request_out(0)
				, request_err(0)
				, worker_in(0)
				, worker_out(0)
				, worker_err(0)
			{

			}

			/**
			* \brief 心跳
			*/
			void worker_join(const char* w, const char* ips)
			{
				auto iter = workers.find(w);
				if (iter == workers.end())
				{
					worker wk;
					wk.ips = ips;
					{
						boost::lock_guard<boost::mutex> guard(mutex_);
						workers.insert(make_pair(w, wk));
					}
				}
				else
				{
					iter->second.ips = ips;
					iter->second.active();
				}
			}

			/**
			* \brief 心跳
			*/
			void worker_heartbeat(const char* w)
			{
				auto iter = workers.find(w);
				if (iter == workers.end())
				{
					worker wk;
					{
						boost::lock_guard<boost::mutex> guard(mutex_);
						workers.insert(make_pair(w, wk));
					}
				}
				else
				{
					iter->second.active();
				}
			}
			/**
			* \brief 心跳
			*/
			void worker_left(const char* w)
			{
				boost::lock_guard<boost::mutex> guard(mutex_);
				workers.erase(w);
			}

			/**
			* \brief 检查工作对象
			*/
			void check_works()
			{
				boost::lock_guard<boost::mutex> guard(mutex_);
				for (auto & work : workers)
				{
					if (work.second.check() < 0)
					{
						workers.erase(work.first);
					}
				}
			}

			/**
			* \brief 站点名称
			*/
			int get_station_type() const
			{
				return station_type_;
			}

			/**
			* \brief 当前站点状态
			*/
			station_state get_station_state() const
			{
				return station_state_;
			}

			/**
			* \brief 站点名称
			*/
			const string& get_station_name() const
			{
				return station_name_;
			}

			/**
			* \brief 外部地址
			*/
			string get_out_address() const
			{
				string addr("tcp://*:");
				addr += std::to_string(request_port_);
				return addr;
			}

			/**
			* \brief 工作地址
			*/
			string get_inner_address() const
			{
				string addr("tcp://*:");
				addr += std::to_string(worker_port_);
				return addr;
			}
			/**
			* \brief 从JSON中读取
			*/
			void read_json(const char* val)
			{
				boost::lock_guard<boost::mutex> guard(mutex_);
				acl::json json;
				json.update(val);
				acl::json_node* iter = json.first_node();
				while (iter)
				{
					const char* tag = iter->tag_name();
					if (tag == nullptr || tag[0] == 0)
					{
						iter = json.next_node();
						continue;
					}
					const int idx = strmatchi(16, tag
						, "station_name"
						, "station_type"
						, "request_port"
						, "worker_port"
						, "heart_port"
						, "description"
						, "caption"
						, "station_alias"
						, "station_state"
						, "request_in"
						, "request_out"
						, "request_err"
						, "worker_in"
						, "worker_out"
						, "worker_err"
						, "short_name");
					switch (idx)
					{
					case 0:
						station_name_ = iter->get_string();
						break;
					case 1:
						station_type_ = static_cast<int>(*iter->get_int64());
						break;
					case 2:
						request_port_ = static_cast<int>(*iter->get_int64());
						break;
					case 3:
						worker_port_ = static_cast<int>(*iter->get_int64());
						break;
					case 5:
						station_description_ = iter->get_string();
						break;
					case 6:
						station_caption_ = iter->get_string();
						break;
					case 7:
						alias_.clear();
						{
							auto ajson = iter->get_obj();
							if (!ajson->is_array())
								break;
							auto it = ajson->first_child();
							while (it)
							{
								alias_.emplace_back(it->get_string());
								it = ajson->next_child();
							}
						}
						break;
					case 8:
						station_state_ = static_cast<station_state>(*iter->get_int64());
						break;
					case 9:
						request_in = *iter->get_int64();
						break;
					case 10:
						request_out = *iter->get_int64();
						break;
					case 11:
						worker_err = *iter->get_int64();
						break;
					case 12:
						worker_in = *iter->get_int64();
						break;
					case 13:
						worker_out = *iter->get_int64();
						break;
					case 14:
						worker_err = *iter->get_int64();
						break;
					case 15:
						short_name = *iter->get_string();
						break;
					default: break;
					}
					iter = json.next_node();
				}

			}
			/**
			* \brief 写入JSON
			*/
			acl::string to_json(bool simple = false)
			{
				boost::lock_guard<boost::mutex> guard(mutex_);
				acl::json json;
				acl::json_node& node = json.create_node();
				if (!simple)
				{
					if (!station_name_.empty())
						node.add_text("station_name", station_name_.c_str());
					if (!short_name.empty())
						node.add_text("short_name", short_name.c_str());
					if (!station_description_.empty())
						node.add_text("_description", station_description_.c_str());
					if (!station_caption_.empty())
						node.add_text("_caption", station_caption_.c_str());
					if (alias_.size() > 0)
					{
						acl::json_node& array = json.create_array();
						for (auto alia : alias_)
						{
							array.add_array_text(alia.c_str());
						}
						node.add_child("station_alias", array);
					}
					if (station_type_ > 0)
						node.add_number("station_type", station_type_);
					if (request_port_ > 0)
						node.add_number("request_port", request_port_);
					if (worker_port_ > 0)
						node.add_number("worker_port", worker_port_);
				}
				node.add_number("station_state", static_cast<int>(station_state_));
				node.add_number("request_in", request_in);
				node.add_number("request_out", request_out);
				node.add_number("request_err", request_err);
				node.add_number("worker_in", worker_in);
				node.add_number("worker_out", worker_out);
				node.add_number("worker_err", worker_err);
				if (workers.size() > 0)
				{
					acl::json_node& array = json.create_array();
					for (auto& worker : workers)
					{
						acl::string str;
						//str.format("%s (%s) [%d]", worker.first.c_str(), worker.second.ips.c_str(), worker.second.level);
						str.format("%s [%d]", worker.first.c_str(), worker.second.level);
						array.add_array_text(str);
					}
					node.add_child("workers", array);
				}
				return node.to_string();
			}
			/**
			* \brief 开机日志
			*/
			void log_start()
			{
				log(station_state_ == station_state::ReStart ? "restart" : "start");
				station_state_ = station_state::Start;
			}
			/**
			* \brief 开机失败日志
			*/
			void log_failed()
			{
				log("con`t launch");
				station_state_ = station_state::Failed;
			}
			/**
			* \brief 开机正常日志
			*/
			void log_runing()
			{
				log("runing");
				station_state_ = station_state::Run;
			}
			/**
			* \brief 关机日志
			*/
			void log_closing()
			{
				log("closed");
				station_state_ = station_state::Closing;
			}
			/**
			* \brief 关机日志
			*/
			void log_closed()
			{
				log("closed");
				station_state_ = station_state::Closed;
			}
			/**
			* \brief 日志
			*/
			void log(const char* state) const
			{
				const char* type;
				switch (station_type_)
				{
				case STATION_TYPE_API:
					type = "      API";
					break;
				case STATION_TYPE_VOTE:
					type = "      VOTE";
					break;
				case STATION_TYPE_PUBLISH:
					type = "   PUBLISH";
					break;
				case  STATION_TYPE_DISPATCHER:
					type = "DISPATCHER";
					break;
				default:
					type = "       ERR";
					break;
				}
				if (station_state_ == station_state::ReStart)
					log_msg5("%s:%s(%d | %d) %s", type, station_name_.c_str(), request_port_, worker_port_, state)
				else
					log_msg5("%s:%s(%d | %d) %s", type, station_name_.c_str(), request_port_, worker_port_, state)

			}

		};
	}
}
#endif //!_ZERO_CONFIG_H_