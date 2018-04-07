#pragma once
#ifndef C_ZMQ_NET_OBJECT
#include <stdinc.h>
 #include <utility>
#include "sharp_char.h"
#include "zmq_extend.h"
#include "StationWarehouse.h"
namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 表示一个基于ZMQ的网络站点
		*/
		class ZeroStation
		{
			friend class StationWarehouse;
		protected:
			bool _in_plan_poll;

			acl::string _config;
			/**
			* \brief 状态信号量
			*/
			boost::interprocess::interprocess_semaphore _state_semaphore;
			/**
			* \brief API服务名称
			*/
			string _station_name;

			/**
			* \brief 站点类型
			*/
			int _station_type;

			/**
			* \brief 外部地址
			*/
			int _out_port;

			/**
			* \brief 工作地址
			*/
			int _inner_port;

			/**
			* \brief 心跳地址
			*/
			int _heart_port;
			/**
			* \brief 外部SOCKET类型
			*/
			int _out_zmq_type;

			/**
			* \brief 工作SOCKET类型
			*/
			int _inner_zmq_type;

			/**
			* \brief 心跳SOCKET类型
			*/
			int _heart_zmq_type;
			/*
			*\brief 轮询节点
			*/
			zmq_pollitem_t* _poll_items;
			/*
			*\brief 节点数量
			*/
			int _poll_count;
			/**
			* \brief 调用句柄
			*/
			void* _out_socket;
			/**
			* \brief 调用句柄
			*/
			void* _out_socket_inproc;
			/**
			* \brief 工作句柄
			*/
			void* _inner_socket;
			/**
			* \brief 心跳句柄
			*/
			void* _heart_socket;
			/**
			* \brief 当前ZMQ执行状态
			*/
			ZmqSocketState _zmq_state;
			/**
			* \brief 当前站点状态
			*/
			station_state _station_state;
		public:
			const char* get_config() const
			{
				return _config.c_str();
			}
			/**
			* \brief 当前ZMQ执行状态
			*/
			ZmqSocketState get_zmq_state() const
			{
				return _zmq_state;
			}
			/**
			* \brief API服务名称
			*/
			int get_station_type() const
			{
				return _station_type;
			}
			/**
			* \brief 当前站点状态
			*/
			station_state get_station_state() const
			{
				return _station_state;
			}
			/**
			* \brief API服务名称
			*/
			const string& get_station_name() const
			{
				return _station_name;
			}

			/**
			* \brief 外部地址
			*/
			string get_out_address()const
			{
				string addr("tcp://*:");
				addr += _out_port;
				return addr;
			}

			/**
			* \brief 工作地址
			*/
			string get_inner_address()const
			{
				string addr("tcp://*:");
				addr += _inner_port;
				return addr;
			}

			/**
			* \brief 心跳地址
			*/
			string get_heart_address()const
			{
				string addr("tcp://*:");
				addr += _heart_port;
				return addr;
			}

			/**
			* \brief 构造
			*/
			ZeroStation(string name, int type, int out_zmq_type, int inner_zmq_type, int heart_zmq_type)
				: _in_plan_poll(false)
				, _state_semaphore(1)
				, _station_name(std::move(name))
				, _station_type(type)
				, _out_port(0)
				, _inner_port(0)
				, _heart_port(0)
				, _out_zmq_type(out_zmq_type)
				, _inner_zmq_type(inner_zmq_type)
				, _heart_zmq_type(heart_zmq_type)
				, _poll_items(nullptr)
				, _poll_count(0)
				, _out_socket(nullptr)
				, _out_socket_inproc(nullptr)
				, _inner_socket(nullptr)
				, _heart_socket(nullptr)
				, _zmq_state(ZmqSocketState::Succeed)
				, _station_state(station_state::None)
			{
			}
			/**
			* \brief 载入现在到期的内容
			*/
			size_t load_now(vector<Message>& messages) const;

			/**
			* \brief 删除一个计划
			*/
			bool remove_next(Message& message) const;

			/**
			* \brief 计划下一次执行时间
			*/
			bool plan_next(Message& message, bool first) const;

			/**
			* \brief 保存下一次执行时间
			*/
			bool save_next(Message& message) const;

			/**
			* \brief 保存消息
			*/
			bool save_message(Message& message) const;

			/**
			* \brief 读取消息
			*/
			bool load_message(uint id, Message& message) const;

			/**
			* \brief 删除消息
			*/
			bool remove_message(Message& message) const;

			/**
			* \brief 保存消息参与者
			*/
			bool save_message_worker(uint msgid, vector<const char*>& workers) const;

			/**
			* \brief 保存消息参与者返回值
			*/
			bool save_message_result(uint msgid, const string& worker, const string& response) const;

			/**
			* \brief 取一个参与者的消息返回值
			*/
			acl::string get_message_result(uint msgid, const char* worker) const;

			/**
			* \brief 取全部参与者消息返回值
			*/
			map<acl::string, acl::string> get_message_result(uint msgid) const;

			/**
			* \brief 执行一条命令
			*/
			virtual sharp_char command(const char* caller, vector<sharp_char> lines) = 0;
		public:

			/**
			*\brief 发送消息
			*/
			bool send_data(vector<sharp_char>& datas)
			{
				_zmq_state = send(_inner_socket, datas);
				return _zmq_state == ZmqSocketState::Succeed;
			}
			/**
			* \brief 析构
			*/
			virtual ~ZeroStation()
			{
				ZeroStation::close(true);
				_station_state = station_state::Destroy;
			}
			/**
			* \brief 能否继续工作
			*/
			virtual bool can_do() const
			{
				return (_station_state == station_state::Run || _station_state == station_state::Pause) && get_net_state() == NET_STATE_RUNING;
			}
			/**
			* \brief 检查是否暂停
			*/
			bool check_pause()
			{
				if (_station_state == station_state::Pause)
				{
					_state_semaphore.timed_wait(time_span(1000));
					return _station_state == station_state::Pause;
				}
				return false;
			}
			/**
			* \brief 初始化
			*/
			bool initialize();

			/**
			* \brief 网络轮询
			*/
			bool poll();

			/**
			* \brief 析构
			*/
			bool destruct();

			/**
			* \brief 暂停
			*/
			virtual bool pause(bool waiting);
			/**
			* \brief 继续
			*/
			virtual bool resume(bool waiting);
			/**
			* \brief 结束
			*/
			virtual bool close(bool waiting);
			/**
			* \brief 计划轮询
			*/
			static void plan_poll(void* arg)
			{
				ZeroStation* station = static_cast<ZeroStation*>(arg);
				station->plan_poll_();
			}
		protected:
			/**
			* \brief 计划轮询
			*/
			void plan_poll_();

			/**
			* \brief 工作集合的响应
			*/
			virtual void response() {}
			/**
			* \brief 调用集合的响应
			*/
			virtual void request(ZMQ_HANDLE socket) = 0;
			/**
			* 心跳的响应
			*/
			virtual void heartbeat(){}
		};


	}
}
#endif
