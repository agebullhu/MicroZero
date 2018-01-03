#ifndef ZMQ_API_STATION_H
#pragma once
#include <stdinc.h>
#include "NetStation.h"
namespace agebull
{
	namespace zmq_net
	{

		/**
		* @brief 负载均衡处理类
		*/
		class HostBalance :public vector<string>
		{
			/**
			* @brief 当前工作者下标
			*/
			size_t _index;
		public:
			HostBalance()
				:vector<string>(16)
				, _index(0)
			{

			}
			/**
			* @brief 加入集群
			*/
			void join(const char* host)
			{
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
				size_t s = size();
				if (s == 0)
					return nullptr;
				if (s == 1)
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
		public:
			/**
			* @brief 构造
			*/
			ApiStation(string name);
			/**
			* @brief 析构
			*/
			virtual ~ApiStation() {}
			/**
			*消息泵
			*/
			static void run(string name)
			{
				ApiStation* netobj = new ApiStation(name);
				boost::thread thrds_s1(boost::bind(start, shared_ptr<ApiStation>(netobj)));
			}
			/**
			* @brief 执行
			*/
			static void start(shared_ptr<ApiStation> arg)
			{
				ApiStation* station = arg.get();
				if (!StationWarehouse::join(station))
				{
					return;
				}
				if (station->_zmq_state == 0)
					log_msg3("%s(%s | %s)正在启动", station->_station_name, station->_out_address, station->_inner_address);
				else
					log_msg3("%s(%s | %s)正在重启", station->_station_name, station->_out_address, station->_inner_address);
				bool reStrart = station->poll();
				StationWarehouse::left(station);
				if (reStrart)
				{
					run(station->_station_name);
				}
				else
				{
					log_msg3("%s(%s | %s)已关闭", station->_station_name, station->_out_address, station->_inner_address);
				}
			}
		private:
			/**
			* @brief 工作集合的响应
			*/
			void response() override;
			/**
			* @brief 调用集合的响应
			*/
			void request() override;
			/**
			* @brief 开始执行一条命令
			*/
			void command_start(const char* caller, vector< string> lines) override;
			/**
			* @brief 结束执行一条命令
			*/
			void command_end(const char* caller, vector< string> lines)override;
			/**
			* @brief 工作集合的响应
			*/
			string create_item(const char* addr, const char * value)override
			{
				return value;
			}
			/**
			* @brief 工作开始（发送到工作者）
			*/
			bool job_start(const char* work, const char* client_addr, const  char* request);
			/**
			* @brief 工作结束(发送到请求者)
			*/
			bool job_end(const char* client_addr, const  char* response);

			/**
			* @brief 工作对象退出
			*/
			void worker_left(char* addr)override
			{
				_balance.left(addr);
				BalanceStation<ApiStation, string, STATION_TYPE_API>::worker_left(addr);
			}

			/**
			* @brief 工作对象加入
			*/
			void worker_join(char* addr, char* value, bool ready = false)override
			{
				_balance.join(addr);
				BalanceStation<ApiStation, string, STATION_TYPE_API>::worker_join(addr, value, ready);
			}
		};
	}
}
#endif