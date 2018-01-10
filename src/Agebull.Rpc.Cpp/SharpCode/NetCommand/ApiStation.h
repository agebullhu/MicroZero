#ifndef ZMQ_API_STATION_H
#pragma once
#include <stdinc.h>
#include "ZeroStation.h"
#include "BalanceStation.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* @brief 负载均衡处理类
		*/
		class HostBalance :private map<string, time_t>
		{
			/**
			* @brief 当前工作者下标
			*/
			size_t _index;
			boost::mutex _mutex;
			vector<string> list;
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
				boost::lock_guard<boost::mutex> guard(_mutex);
				auto iter = find(host);
				if (iter == end())
				{
					insert(make_pair(host, time(nullptr) + 10LL));
					list.push_back(host);
				}
				else
				{
					iter->second = time(nullptr) + 10LL;
				}
			}

			/**
			* @brief 主机工作完成
			*/
			void succees(const char* host)
			{
				boost::lock_guard<boost::mutex> guard(_mutex);
				auto iter = find(host);
				if (iter == end())
				{
					insert(make_pair(host, time(nullptr) + 10LL));
					list.push_back(host);
				}
				else
				{
					iter->second = time(nullptr) + 10LL;
				}
			}

			/**
			* @brief 主机工作失败
			*/
			void bad(const char* host)
			{
				boost::lock_guard<boost::mutex> guard(_mutex);
				left_(host);
			}

			/**
			* @brief 退出集群
			*/
			void left(const char* host)
			{
				boost::lock_guard<boost::mutex> guard(_mutex);
				left_(host);
			}

			/**
			* @brief 取一个可用的主机
			*/
			const char* get_host()
			{
				boost::lock_guard<boost::mutex> guard(_mutex);
				return get_host_();
			}
		private:
			/**
			* @brief 退出集群
			*/
			void left_(string host)
			{
				erase(host);
				auto iter = list.begin();
				while (iter != list.end())
				{
					if (iter->compare(host) == 0)
					{
						list.erase(iter);
						break;
					}
					++iter;
				}
			}
			/**
			* @brief 取一个可用的主机
			*/
			const char* get_host_()
			{
				if (size() == 0)
					return nullptr;
				if (size() == 1)
				{
					auto iter = begin();
					if (time(nullptr) < iter->second)
						return iter->first.c_str();
					clear();
					list.clear();
					return nullptr;
				}
				{
					if (_index == size())
						_index = 0;
					auto iter = find(list[_index]);
					if(iter == end())
					{
						left_(list[_index]);
						return get_host_();
					}
					if (time(nullptr) < iter->second)
						return iter->first.c_str();
					erase(iter);
				}
				return get_host_();
			}
		};

		/**
		* @brief API站点
		*/
		class ApiStation :public BalanceStation<ApiStation, string, STATION_TYPE_API>
		{
			HostBalance _balance;
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
			bool job_start(vector<sharp_char>& list);
			/**
			* @brief 工作结束(发送到请求者)
			*/
			bool job_end(vector<sharp_char>& list);

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
				if (ready)
				{
					_balance.join(addr);
					BalanceStation<ApiStation, string, STATION_TYPE_API>::worker_join(addr, value, ready);
				}
				else
				{
					_balance.succees(addr);
				}
			}
		};
	}
}
#endif
