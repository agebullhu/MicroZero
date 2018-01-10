#ifndef ZMQ_API_STATION_H
#pragma once
#include <stdinc.h>
#include "NetStation.h"
#include "BalanceStation.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* @brief 负载均衡处理类
		*/
		class HostBalance :private vector<string>
		{
			/**
			* @brief 当前工作者下标
			*/
			size_t _index;
		public:
			HostBalance()
				: _index(0)
			{
			}

			/**
			* @brief 加入集群
			*/
			void join(const char* host)
			{
				if (host == nullptr || strlen(host) == 0)
					return;
				push_back(host);
			}

			/**
			* @brief 主机工作完成
			*/
			void succees(const char* host)
			{
				//_hosts.push_back(host);
			}

			/**
			* @brief 主机工作失败
			*/
			void bad(const char* host)
			{
				left(host);
			}

			/**
			* @brief 退出集群
			*/
			void left(const char* host)
			{
				auto iter = begin();
				while (iter != end())
				{
					if (iter->compare(host) == 0)
					{
						erase(iter);
						break;
					}
					++iter;
				}
			}

			/**
			* @brief 取一个可用的主机
			*/
			const char* get_host()
			{
				if (size() == 0)
					return nullptr;
				if (size() == 1)
					return at(0).c_str();
				if (_index < size())
					return at(_index++).c_str();
				_index = 1;
				return at(0).c_str();
			}
		};

		/**
		* @brief API站点
		*/
		class ApiStation :public BalanceStation<ApiStation, string, STATION_TYPE_API>
		{
			HostBalance _balance;
			boost::posix_time::ptime  _start;
			/**
			* @brief 发布消息队列访问锁
			*/
			boost::mutex _mutex;
		public:
			/**
			* @brief 构造
			*/
			ApiStation(string name)
				: BalanceStation<ApiStation, string, STATION_TYPE_API>(name)
			{
			}

			/**
			* @brief 析构
			*/
			virtual ~ApiStation()
			{
			}

			/**
			*消息泵
			*/
			static void run(string name)
			{
				zmq_threadstart(start, new ApiStation(name));
			}

			/**
			* @brief 执行
			*/
			static void start(void* arg)
			{
				ApiStation* station = static_cast<ApiStation*>(arg);
				if (!StationWarehouse::join(station))
				{
					return;
				}
				if (station->_zmq_state == ZmqSocketState::Succeed)
					log_msg3("%s(%d | %d)正在启动", station->_station_name.c_str(), station->_out_port, station->_inner_port)
				else
					log_msg3("%s(%d | %d)正在重启", station->_station_name.c_str(), station->_out_port, station->_inner_port)
				if (!station->initialize())
				{
					log_msg3("%s(%d | %d)无法启动", station->_station_name.c_str(), station->_out_port, station->_inner_port)
					return;
				}
				log_msg3("%s(%d | %d)正在运行", station->_station_name.c_str(), station->_out_port, station->_inner_port)
				bool reStrart = station->poll();

				StationWarehouse::left(station);
				station->destruct();
				if (reStrart)
				{
					ApiStation* station2 = new ApiStation(station->_station_name);
					station2->_zmq_state = ZmqSocketState::Again;
					zmq_threadstart(start, station2);
				}
				else
				{
					log_msg3("%s(%d | %d)已关闭", station->_station_name.c_str(), station->_out_port, station->_inner_port)
				}
				delete station;
			}
		private:
			/**
			* @brief 工作集合的响应
			*/
			void response() override;
			/**
			* @brief 调用集合的响应
			*/
			void request(ZMQ_HANDLE socket) override;
			/**
			* @brief 执行一条命令
			*/
			sharp_char command(const char* caller, vector< string> lines) override;

			/**
			* @brief 工作集合的响应
			*/
			string create_item(const char* addr, const char* value) override
			{
				return value;
			}

			/**
			* @brief 工作开始（发送到工作者）
			*/
			bool job_start(const char* client_addr, const char* command, const char* request);
			/**
			* @brief 工作结束(发送到请求者)
			*/
			bool job_end(const char* client_addr, const char* response);

			/**
			* @brief 工作对象退出
			*/
			void worker_left(const char* addr) override
			{
				_balance.left(addr);
				BalanceStation<ApiStation, string, STATION_TYPE_API>::worker_left(addr);
			}

			/**
			* @brief 工作对象加入
			*/
			void worker_join(const char* addr, const char* value, bool ready = false) override
			{
				if (addr == nullptr || strlen(addr) == 0)
					return;
				_balance.join(addr);
				BalanceStation<ApiStation, string, STATION_TYPE_API>::worker_join(addr, value, ready);
			}
		};
	}
}
#endif
