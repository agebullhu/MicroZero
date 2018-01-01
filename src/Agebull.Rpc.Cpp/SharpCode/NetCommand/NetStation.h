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
			* @brief API服务名称
			*/
			int _station_type;

			/**
			* @brief 外部地址
			*/
			string _outAddress;

			/**
			* @brief 工作地址
			*/
			string _innerAddress;

			/**
			* @brief 心跳地址
			*/
			string _heartAddress;
			/**
			* @brief 调用句柄
			*/
			void* _outSocket;

			/**
			* @brief 工作句柄
			*/
			void* _innerSocket;
			/**
			* @brief 心跳句柄
			*/
			void* _heartSocket;

			/**
			* @brief 当前站点状态
			*/
			station_state _station_state;

			/**
			* @brief 当前ZMQ执行状态
			*/
			int _zmq_state;
		public:
			/**
			* @brief 构造
			*/
			NetStation(string name, int type);

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
			 * @brief 接收空帧
			 */
			static void recv_empty(ZMQ_HANDLE socket);

			/**
			* @brief 能否继续工作
			*/
			bool can_do() const
			{
				return (_station_state == station_state::Run || _station_state == station_state::Pause) &&
					get_net_state() == NET_STATE_RUNING;
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
		};

		/**
		 * \brief 表示一个基于ZMQ的路由站点
		 * \tparam TNetStation 
		 * \tparam TWorker 
		 * \tparam NetType 
		 */
		template <typename TNetStation, class TWorker, int NetType>
		class RouteStation : public NetStation
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
			RouteStation(string name)
				: NetStation(name, NetType)
			{
			}

		protected:
			/**
			* @brief 执行
			*/
			bool poll();

			/**
			* @brief 工作集合的响应
			*/
			virtual void onWorkerPollIn() = 0;
			/**
			* @brief 调用集合的响应
			*/
			virtual void onCallerPollIn() = 0;

			/**
			* 心跳的响应
			*/
			virtual void heartbeat();

			/**
			* @brief 生成工作对象
			*/
			virtual TWorker create_item(const char* addr, const char* value) = 0;

			/**
			* @brief 工作对象退出
			*/
			virtual void left(char* addr);

			/**
			* @brief 工作对象加入
			*/
			virtual void join(char* addr, char* value, bool ready = false);
		};

#define poll_check_pause()\
	if (_station_state == station_state::Pause)\
	{\
		do\
		{\
			boost::posix_time::ptime now(boost::posix_time::microsec_clock::universal_time());\
			_state_semaphore.timed_wait(now + boost::posix_time::seconds(1));\
		} while (_station_state == station_state::Pause && get_net_state() == NET_STATE_RUNING);\
		if (!can_do())\
			break;\
	}

#define poll_zmq_poll(cnt)\
		_zmq_state = zmq_poll(items, cnt, 1000);\
		if (_zmq_state == 0)\
			continue;\
		if (_zmq_state == -1)\
			break;

		/**
		 * \brief 
		 * \param name 
		 * \param type 
		 */
		inline NetStation::NetStation(string name, int type)
			: _state_semaphore(1)
			, _station_name(name)
			, _station_type(type)
			, _outSocket(nullptr)
			, _innerSocket(nullptr)
			, _heartSocket(nullptr)
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
		template <typename TNetStation, class TWorker, int NetType>
		bool RouteStation<TNetStation, TWorker, NetType>::poll()
		{
			_zmq_state = 0;
			_outSocket = create_socket(ZMQ_ROUTER, _outAddress.c_str());
			_innerSocket = create_socket(ZMQ_ROUTER, _innerAddress.c_str());
			_heartSocket = create_socket(ZMQ_ROUTER, _heartAddress.c_str());
			zmq_pollitem_t items[] = {
				{_innerSocket, 0, ZMQ_POLLIN, 0},
				{_outSocket, 0, ZMQ_POLLIN, 0},
				{_heartSocket, 0, ZMQ_POLLIN, 0}
			};
			log_msg3("%s(%s | %s)已启动", _station_name, _outAddress, _innerAddress);
			//登记线程开始
			set_command_thread_start();
			_station_state = station_state::Run;
			while (can_do())
			{
				poll_check_pause();
				poll_zmq_poll(3);
				// 处理_innerSocket中inner的队列
				if (items[0].revents & ZMQ_POLLIN)
				{
					onWorkerPollIn();
				}

				if (items[1].revents & ZMQ_POLLIN)
				{
					onCallerPollIn();
				}

				if (items[2].revents & ZMQ_POLLIN)
				{
					heartbeat();
				}
			}
			_station_state = station_state::Closing;
			zmq_unbind(_outSocket, _outAddress.c_str());
			zmq_close(_outSocket);
			_outSocket = nullptr;
			zmq_unbind(_innerSocket, _innerAddress.c_str());
			zmq_close(_innerSocket);
			_innerSocket = nullptr;
			zmq_unbind(_heartSocket, _heartAddress.c_str());
			zmq_close(_heartSocket);
			_heartSocket = nullptr;
			//登记线程关闭
			set_command_thread_end();
			_station_state = station_state::Closed;
			return _zmq_state < 0;
		}

		/**
		 * \brief
		 */
		template <typename TNetStation, class TWorker, int NetType>
		void RouteStation<TNetStation, TWorker, NetType>::heartbeat()
		{
			// 将inner的地址入队
			char* inner_addr = s_recv(_heartSocket);
			recv_empty(_heartSocket);
			char* client_addr = s_recv(_heartSocket);
			recv_empty(_heartSocket);
			char* reply = s_recv(_heartSocket);
			// 如果是一个应答消息，则转发给client
			if (strcmp(client_addr, "PAPA") == 0)
			{
				join(inner_addr, reply);
			}
			else if (strcmp(client_addr, "MAMA") == 0)
			{
				join(inner_addr, reply);
			}
			else if (strcmp(client_addr, "LAOWANG") == 0)
			{
				left(inner_addr);
			}
			_zmq_state = s_sendmore(_heartSocket, inner_addr);
			if (_zmq_state < 0)
				cout << inner_addr << endl;
			_zmq_state = s_sendmore(_heartSocket, "");
			_zmq_state = s_send(_heartSocket, "OK");//真实发送
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
		void RouteStation<TNetStation, TWorker, NetType>::left(char* addr)
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
		void RouteStation<TNetStation, TWorker, NetType>::join(char* addr, char* value, bool ready)
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
				log_msg3("%s(%s | %s)正在启动", station->_station_name, station->_outAddress, station->_innerAddress);\
			else\
				log_msg3("%s(%s | %s)正在重启", station->_station_name, station->_outAddress, station->_innerAddress);\
			bool reStrart = station->poll();\
			StationWarehouse::left(station.get());\
			if (reStrart)\
			{\
				run(station->_station_name);\
			}\
			else\
			{\
				log_msg3("%s(%s | %s)已关闭", station->_station_name, station->_outAddress, station->_innerAddress);\
			}


	}
}



#endif
