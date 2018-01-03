#pragma once
#ifndef C_ZMQ_NET_OBJECT
#include <stdinc.h>
#include <zeromq/zhelpers.h>

#define STATION_TYPE_DISPATCHER 1
#define STATION_TYPE_API 2
#define STATION_TYPE_VOTE 3
#define STATION_TYPE_PUBLISH 4

namespace agebull
{
	namespace zmq_net
	{
		class NetStation;
		/**
		* @brief 网络站点实例管理（站点仓库，是不是很脑洞的名字）
		*/
		class StationWarehouse
		{
		protected:
			/**
			* @brief 实例集合
			*/
			static map<string, NetStation*> examples;
		public:
			/**
			* @brief 清除所有服务
			*/
			static void clear();
			/**
			* @brief 还原服务
			*/
			static int restore();
			/**
			* @brief 初始化服务
			*/
			static acl::string install(int station_type, const char* station_name);
			/**
			* @brief 还原服务
			*/
			static bool restore(acl::string& value);
			/**
			* @brief 加入服务
			*/
			static bool join(NetStation* station);
			/**
			* @brief 加入服务
			*/
			static bool left(NetStation* station);
			/**
			* @brief 加入服务
			*/
			static NetStation* find(string name);
		};
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
			 * \brief 已关闭
			 */
			Closed,
			/**
			 * \brief 已销毁，析构已调用
			 */
			Destroy
		};

		/**
		* @brief 表示一个基于ZMQ的网络站点
		*/
		class NetStation
		{
			friend StationWarehouse;
		protected:
			/**
			* @brief 状态信号量
			*/
			boost::interprocess::interprocess_semaphore _state_semaphore;
		protected:
			/**
			* @brief API服务名称
			*/
			string _station_name;

			/**
			* @brief 外部地址
			*/
			string _out_address;

			/**
			* @brief 工作地址
			*/
			string _inner_address;

			/**
			* @brief 心跳地址
			*/
			string _heart_address;
			/**
			* @brief 调用句柄
			*/
			void* _out_socket;

			/**
			* @brief 工作句柄
			*/
			void* _inner_socket;
			/**
			* @brief 心跳句柄
			*/
			void* _heart_socket;

			/**
			* @brief 外部SOCKET类型
			*/
			int _out_zmq_type;

			/**
			* @brief 工作SOCKET类型
			*/
			int _inner_zmq_type;

			/**
			* @brief 心跳SOCKET类型
			*/
			int _heart_zmq_type;

			/**
			* @brief 当前ZMQ执行状态
			*/
			int _zmq_state;

		public:
			/**
			* @brief API服务名称
			*/
			int _station_type;
			/**
			* @brief 当前站点状态
			*/
			station_state _station_state;

			/**
			* @brief 构造
			*/
			NetStation(string name, int type, int out_zmq_type, int inner_zmq_type, int heart_zmq_type);

			/**
			* @brief 析构
			*/
			virtual ~NetStation()
			{
				close(true);
				_station_state = station_state::Destroy;
			}

		protected:
			/**
			* @brief 网络循环
			*/
			bool poll();
			/**
			* @brief 处理反馈
			*/
			virtual void response() = 0;
			/**
			* @brief 处理请求
			*/
			virtual void request() = 0;

			/**
			* 心跳的响应
			*/
			virtual void heartbeat() =0;
		protected:
			/**
			 * @brief 接收空帧
			 */
			static void recv_empty(ZMQ_HANDLE socket);


		protected:
			/**
			* @brief 能否继续工作
			*/
			bool can_do() const
			{
				return (_station_state == station_state::Run || _station_state == station_state::Pause) &&
					get_net_state() == NET_STATE_RUNING;
			}
			/**
			* @brief 检查是否暂停
			*/
			bool check_pause()
			{
				if (_station_state == station_state::Pause)
				{
					boost::posix_time::ptime now(boost::posix_time::microsec_clock::universal_time());
					_state_semaphore.timed_wait(now + boost::posix_time::seconds(1));
					return _station_state == station_state::Pause;
				}
				return false;
			}
		public:

			/**
			* @brief 暂停
			*/
			bool pause(bool waiting)
			{
				if (station_state::Run != _station_state)
					return false;
				_station_state = station_state::Pause;
				return true;
			}

			/**
			* @brief 继续
			*/
			bool resume(bool waiting)
			{
				if (station_state::Pause != _station_state)
					return false;
				_station_state = station_state::Run;
				_state_semaphore.post();
				return true;
			}

			/**
			* @brief 结束
			*/
			bool close(bool waiting)
			{
				if (!can_do())
					return false;
				_station_state = station_state::Closing;
				while (waiting && _station_state == station_state::Closing)
					thread_sleep(1000);
				return true;
			}
			/**
			* @brief 开始执行一条命令
			*/
			virtual void command_start(const char* caller, vector< string> lines) = 0;
			/**
			* @brief 结束执行一条命令
			*/
			virtual void command_end(const char* caller, vector< string> lines) = 0;
		};

		/**
		 * \brief 表示一个基于ZMQ的负载均衡站点
		 * \tparam TNetStation
		 * \tparam TWorker
		 * \tparam NetType
		 */
		template <typename TNetStation, class TWorker, int NetType>
		class BalanceStation : public NetStation
		{
		protected:
			/**
			* @brief 参与者集合
			*/
			map<string, TWorker> _workers;
		public:
			/**
			* @brief 构造
			*/
			BalanceStation(string name)
				: NetStation(name, NetType, ZMQ_ROUTER, ZMQ_ROUTER, ZMQ_REQ)
			{
			}

		protected:

			/**
			* @brief 生成工作对象
			*/
			virtual TWorker create_item(const char* addr, const char* value) = 0;

			/**
			* 心跳的响应
			*/
			virtual void heartbeat() override;
			/**
			* @brief 工作对象退出
			*/
			virtual void worker_left(char* addr) ;

			/**
			* @brief 工作对象加入
			*/
			virtual void worker_join(char* addr, char* value, bool ready = false);
		};




#define close_socket(socket,addr)\
		zmq_unbind(socket, addr.c_str());\
		zmq_close(socket);\
		socket = nullptr
		/**
		 * \brief
		 * \param name
		 * \param type
		 */
		inline NetStation::NetStation(string name, int type, int out_zmq_type, int inner_zmq_type, int heart_zmq_type)
			: _state_semaphore(1)
			, _station_name(name)
			, _station_type(type)
			, _out_socket(nullptr)
			, _inner_socket(nullptr)
			, _heart_socket(nullptr)
			, _out_zmq_type(out_zmq_type)
			, _inner_zmq_type(inner_zmq_type)
			, _heart_zmq_type(heart_zmq_type)
			, _station_state(station_state::None)
			, _zmq_state(0)
		{
		}

		/**
		 * \brief
		 * \param socket
		 */
		inline void NetStation::recv_empty(ZMQ_HANDLE socket)
		{
			char* empty = s_recv(socket, 0);
			if (empty != nullptr)
			{
				assert(empty[0] == 0);
				free(empty);
			}
		}

		/**
		 * \brief
		 * \return
		 */
		inline bool NetStation::poll()
		{
			_zmq_state = 0;
			int cnt = 0;
			zmq_pollitem_t items[3];
			if (_out_zmq_type >= 0)
			{
				_out_socket = create_socket(_out_zmq_type, _out_address.c_str());
				items[cnt++] = { _out_socket, 0, ZMQ_POLLIN, 0 };
			}
			if (_inner_zmq_type >= 0)
			{
				_inner_socket = create_socket(_inner_zmq_type, _inner_address.c_str());
				items[cnt++] = { _inner_socket, 0, ZMQ_POLLIN, 0 };
			}
			if (_heart_zmq_type >= 0)
			{
				_heart_socket = create_socket(_heart_zmq_type, _heart_address.c_str());
				items[cnt++] = { _heart_socket, 0, ZMQ_POLLIN, 0 };
			}

			log_msg3("%s(%s | %s)已启动", _station_name, _out_address, _inner_address);
			//登记线程开始
			set_command_thread_start();
			_station_state = station_state::Run;
			while (can_do())
			{
				if (check_pause())
					continue;

				_zmq_state = zmq_poll(items, cnt, 1000);
				if (_zmq_state == 0)
					continue;
				if (_zmq_state == -1)
					break;
				// 处理_inner_socket中inner的队列
				if (items[0].revents & ZMQ_POLLIN)
				{
					response();
				}

				if (cnt > 1 && items[1].revents & ZMQ_POLLIN)
				{
					request();
				}

				if (cnt > 2 && items[2].revents & ZMQ_POLLIN)
				{
					heartbeat();
				}
			}
			_station_state = station_state::Closing;
			if (_out_zmq_type >= 0)
			{
				close_socket(_out_socket, _out_address);
			}
			if (_inner_zmq_type >= 0)
			{
				close_socket(_inner_socket, _inner_address);
			}
			if (_heart_zmq_type >= 0)
			{
				close_socket(_heart_socket, _heart_address);
			}
			//登记线程关闭
			set_command_thread_end();
			_station_state = station_state::Closed;
			return _zmq_state < 0;
		}

		/**
		 * \brief
		 */
		template <typename TNetStation, class TWorker, int NetType>
		void BalanceStation<TNetStation, TWorker, NetType>::heartbeat()
		{
			// 将inner的地址入队
			char* inner_addr = s_recv(_heart_socket);
			recv_empty(_heart_socket);
			char* client_addr = s_recv(_heart_socket);
			recv_empty(_heart_socket);
			char* reply = s_recv(_heart_socket);
			// 如果是一个应答消息，则转发给client
			if (strcmp(client_addr, "PAPA") == 0)
			{
				worker_join(inner_addr, reply);
			}
			else if (strcmp(client_addr, "MAMA") == 0)
			{
				worker_join(inner_addr, reply);
			}
			else if (strcmp(client_addr, "LAOWANG") == 0)
			{
				worker_left(inner_addr);
			}
			_zmq_state = s_sendmore(_heart_socket, inner_addr);
			if (_zmq_state < 0)
				cout << inner_addr << endl;
			_zmq_state = s_sendmore(_heart_socket, "");
			_zmq_state = s_send(_heart_socket, "OK");//真实发送
			if (_zmq_state <= 0)
				cout << inner_addr << endl;

			free(inner_addr);
			free(client_addr);
			free(reply);
		}

		/**
		 * \brief
		 * \param addr
		 */
		template <typename TNetStation, class TWorker, int NetType>
		void BalanceStation<TNetStation, TWorker, NetType>::worker_left(char* addr)
		{
			auto vote = _workers.find(addr);
			if (vote != _workers.end())
			{
				_workers.erase(addr);
				cout << addr << "已退出" << endl;
			}
		}

		/**
		 * \brief
		 * \param addr
		 * \param value
		 * \param ready
		 */
		template <typename TNetStation, class TWorker, int NetType>
		void BalanceStation<TNetStation, TWorker, NetType>::worker_join(char* addr, char* value, bool ready)
		{
			TWorker item = create_item(addr, value);
			auto old = _workers.find(addr);
			if (old == _workers.end())
			{
				_workers.insert(make_pair(addr, item));

				cout << addr << "(" << addr << ")已加入(通过" << (ready ? "启动)" : "心跳)") << endl;
			}
			else
			{
				old->second = item;
				cout << addr << "(" << addr << ")还活着(通过心跳)" << endl;
			}
		}


		//开始线程的宏
#define station_run(station)\
			if(!StationWarehouse::join(station.get()))\
				return;\
			if (station->_zmq_state == 0)\
				log_msg3("%s(%s | %s)正在启动", station->_station_name, station->_out_address, station->_inner_address);\
			else\
				log_msg3("%s(%s | %s)正在重启", station->_station_name, station->_out_address, station->_inner_address);\
			bool reStrart = station->poll();\
			StationWarehouse::left(station.get());\
			if (reStrart)\
			{\
				run(station->_station_name);\
			}\
			else\
			{\
				log_msg3("%s(%s | %s)已关闭", station->_station_name, station->_out_address, station->_inner_address);\
			}


	}
}



#endif
