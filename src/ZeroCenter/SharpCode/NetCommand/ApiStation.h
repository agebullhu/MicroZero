#ifndef ZMQ_API_STATION_H
#pragma once
#include <stdinc.h>
#include <utility>
#include "ZeroStation.h"
#include "BalanceStation.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 负载均衡处理类
		*/
		class host_balance :private map<string, time_t>
		{
			/**
			* \brief 当前工作者下标
			*/
			size_t _index;
			boost::mutex _mutex;
			vector<string> list;
		public:
			host_balance()
				: _index(0)
			{

			}

			/**
			* \brief 加入集群
			*/
			void join(const char* host)
			{
				boost::lock_guard<boost::mutex> guard(_mutex);
				auto iter = find(host);
				if (iter == end())
				{
					insert(make_pair(host, time(nullptr) + 10LL));
					list.emplace_back(host);
				}
				else
				{
					iter->second = time(nullptr) + 10LL;
				}
			}

			/**
			* \brief 主机工作完成
			*/
			void succees(const char* host)
			{
				boost::lock_guard<boost::mutex> guard(_mutex);
				auto iter = find(host);
				if (iter == end())
				{
					insert(make_pair(host, time(nullptr) + 10LL));
					list.emplace_back(host);
				}
				else
				{
					iter->second = time(nullptr) + 10LL;
				}
			}

			/**
			* \brief 主机工作失败
			*/
			void bad(const char* host)
			{
				boost::lock_guard<boost::mutex> guard(_mutex);
				left_(host);
			}

			/**
			* \brief 退出集群
			*/
			void left(const char* host)
			{
				boost::lock_guard<boost::mutex> guard(_mutex);
				left_(host);
			}

			/**
			* \brief 取一个可用的主机
			*/
			const char* get_host()
			{
				boost::lock_guard<boost::mutex> guard(_mutex);
				return get_host_();
			}
		private:
			/**
			* \brief 退出集群
			*/
			void left_(const string& host)
			{
				erase(host);
				auto iter = list.begin();
				while (iter != list.end())
				{
					if (*iter == host)
					{
						list.erase(iter);
						break;
					}
					++iter;
				}
			}
			/**
			* \brief 取一个可用的主机
			*/
			const char* get_host_()
			{
				if (list.empty())
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
					if (++_index == size())
						_index = 0;
					auto iter = find(list[_index]);
					if (iter == end())
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
		* \brief API站点
		*/
		class api_station :public balance_station<api_station, string, STATION_TYPE_API>
		{
			/**
			* \brief 发布消息队列访问锁
			*/
			host_balance _balance;
		public:
			/**
			* \brief 构造
			*/
			api_station(string name)
				: balance_station<api_station, string, STATION_TYPE_API>(std::move(name))
			{
			}

			/**
			* \brief 析构
			*/
			virtual ~api_station() = default;

			/**
			*消息泵
			*/
			static void run(string name)
			{
				zmq_threadstart(launch, new api_station(std::move(name)));
			}

			/**
			* \brief 执行
			*/
			static void launch(void* arg)
			{
				api_station* station = static_cast<api_station*>(arg);
				if (!station_warehouse::join(station))
				{
					return;
				}
				if (!station->do_initialize())
					return;

				bool reStrart = station->poll();

				station_warehouse::left(station);
				station->destruct();
				if (reStrart)
				{
					api_station* station2 = new api_station(station->_station_name);
					station2->_zmq_state = ZmqSocketState::Again;
					zmq_threadstart(launch, station2);
				}
				else
				{
					log_msg3("Station:%s(%d | %d) is closed", station->_station_name.c_str(), station->_out_port, station->_inner_port);
				}
				delete station;
			}
		private:
			/**
			* \brief 工作集合的响应
			*/
			void response() override;
			/**
			* \brief 调用集合的响应
			*/
			void request(ZMQ_HANDLE socket) override;
			/**
			* \brief 执行一条命令
			*/
			sharp_char command(const char* caller, vector<sharp_char> lines) override;

			/**
			* \brief 工作集合的响应
			*/
			string create_item(const char* addr, const char* value) override
			{
				return value;
			}

			/**
			* \brief 工作开始（发送到工作者）
			*/
			bool job_start(vector<sharp_char>& list);
			/**
			* \brief 工作结束(发送到请求者)
			*/
			bool job_end(vector<sharp_char>& list);

			/**
			* \brief 工作对象退出
			*/
			void worker_left(const char* addr) override
			{
				_balance.left(addr);
				balance_station<api_station, string, STATION_TYPE_API>::worker_left(addr);
			}

			/**
			* \brief 工作对象加入
			*/
			virtual void worker_join(const char* addr, const char* value, bool ready = false) override;
		};
	}
}
#endif
