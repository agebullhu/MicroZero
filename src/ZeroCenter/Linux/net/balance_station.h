#pragma once
#ifndef _ZMQ_NET_BALANCESTATION_H_
#include "../stdinc.h"
#include "zero_station.h"
namespace agebull
{
	namespace zmq_net
	{

		/**
		* \brief 表示一个基于ZMQ的负载均衡站点
		* \tparam TNetStation
		* \tparam TWorker
		* \tparam NetType
		*/
		template <typename TNetStation, class TWorker, int NetType>
		class balance_station : public zero_station
		{
		protected:
			/**
			* \brief 参与者集合
			*/
			map<string, TWorker> workers_;
		public:
			/**
			* \brief 构造
			*/
			balance_station(string name)
				: zero_station(name, NetType, ZMQ_ROUTER, ZMQ_DEALER, ZMQ_ROUTER)
			{
			}

		protected:

			/**
			* \brief 生成工作对象
			*/
			virtual TWorker create_item(const char* addr, const char* value) = 0;

			/**
			* \brief 工作对象退出
			*/
			virtual bool worker_left(const char* addr);

			/**
			* \brief 工作对象加入
			*/
			virtual bool worker_join(const char* addr);

			/**
			* \brief 工作对象心跳
			*/
			virtual bool worker_heat(const char* addr);
		};


		/**
		* \brief 负载均衡处理类
		*/
		class host_balance :private map<string, time_t>
		{
			/**
			* \brief 当前工作者下标
			*/
			size_t index_;
			/**
			* \brief 互斥量
			*/
			boost::mutex _mutex;
			vector<const char*> list;
		public:
			host_balance()
				: index_(0)
			{

			}

			/**
			* \brief 加入集群
			*/
			void join(const char* host)
			{
				boost::lock_guard<boost::mutex> guard(_mutex);

				const iterator iter = find(host);
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
			* \brief 主机工作完成
			*/
			void succees(const char* host)
			{
				boost::lock_guard<boost::mutex> guard(_mutex);
				const iterator iter = find(host);
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
			* \brief 主机工作失败
			*/
			void bad(const char* host)
			{
				boost::lock_guard<boost::mutex> guard(_mutex);
				left_inner(host);
			}

			/**
			* \brief 退出集群
			*/
			void left(const char* host)
			{
				boost::lock_guard<boost::mutex> guard(_mutex);
				left_inner(host);
			}

			/**
			* \brief 取一个可用的主机
			*/
			bool get_host(const char*& worker)
			{
				boost::lock_guard<boost::mutex> guard(_mutex);
				return get_next_host(worker);
			}
		private:
			/**
			* \brief 退出集群
			*/
			void left_inner(const char* host)
			{
				erase(host);
				for (auto i = list.begin(); i<list.end(); ++i)
				{
					if (strcasecmp(*i, host) == 0)
					{
						list.erase(i);
						break;
					}
				}
			}
			/**
			* \brief 取一个可用的主机
			*/
			bool get_next_host(const char*& worker)
			{
				if (list.empty())
				{
					worker = nullptr;
					return true;
				}
				if (size() == 1)
				{
					const auto iter = begin();
					if (time(nullptr) < iter->second)
					{
						worker = iter->first.c_str();
						return true;
					}
					clear();
					list.clear();
					worker = nullptr;
					return true;
				}
				if (++index_ == size())
					index_ = 0;
				const auto iter = find(list[index_]);
				if (iter == end())
				{
					auto i = list.begin();
					i += index_;
					list.erase(i);
					return get_next_host(worker);
				}
				worker = list[index_];
				return time(nullptr) < iter->second;
			}
		};

		/* *
		* \brief
		* /
		template <typename TNetStation, class TWorker, int NetType>
		void balance_station<TNetStation, TWorker, NetType>::heartbeat()
		{
			vector<sharp_char> list;
			//0 路由到的地址 1 空帧 2 命令 3 服务器地址
			zmq_state_ = recv(heart_socket_tcp_, list);
			bool success = false;
			const char* status;
			switch (list[1][0])
			{
			case ZERO_HEART_JOIN:
				success = worker_join(*list[2]);
				status = ZERO_STATUS_OK;
				break;
			case ZERO_HEART_LEFT:
				success = worker_left(*list[2]);
				status = success ? ZERO_STATUS_OK : ZERO_STATUS_ERROR;
				break;
			case ZERO_HEART_PITPAT:
				success = worker_heat(*list[2]);
				status = success ? ZERO_STATUS_OK : ZERO_STATUS_ERROR;
				break;
			default:
				status = ZERO_STATUS_NO_SUPPORT; break;
			}
			zmq_state_ = send_addr(heart_socket_tcp_, *list[0]);
			zmq_state_ = send_late(heart_socket_tcp_, status);
		}*/

		/**
		* \brief
		* \param addr
		*/
		template <typename TNetStation, class TWorker, int NetType>
		bool balance_station<TNetStation, TWorker, NetType>::worker_left(const char* addr)
		{
			if (workers_.find(addr) == workers_.end())
				return false;
			workers_.erase(addr);
			log_trace2(DEBUG_BASE, 1, "station %s => %s left", config_->station_name_.c_str(), addr);
			monitor_async(config_->station_name_, "worker_left", addr);

			//vector<sharp_char> result;
			//vector<string> argument{"@", addr};
			//inproc_request_socket<ZMQ_REQ> socket("_sys_", station_name_.c_str());
			//socket.request(argument, result);
			return true;
		}

		/**
		* \brief
		* \param addr
		*/
		template <typename TNetStation, class TWorker, int NetType>
		bool balance_station<TNetStation, TWorker, NetType>::worker_join(const char* addr)
		{
			TWorker item = create_item(addr, addr);
			auto old = workers_.find(addr);
			if (old != workers_.end())
				return false;
			workers_.insert(make_pair(addr, item));
			log_trace2(DEBUG_BASE, 1, "station %s => %s join", config_->station_name_.c_str(), addr);
			monitor_async(config_->station_name_, "worker_join", addr);
			return true;
		}


		/**
		* \brief
		* \param addr
		*/
		template <typename TNetStation, class TWorker, int NetType>
		bool balance_station<TNetStation, TWorker, NetType>::worker_heat(const char* addr)
		{
			TWorker item = create_item(addr, addr);
			auto old = workers_.find(addr);
			if (old == workers_.end())
			{
				return false;
			}
			old->second = item;
			monitor_async(config_->station_name_, "worker_heat", addr);
			return true;
		}

		/**
		* \brief 工作集合的响应
		*/
		string create_item(const char* addr, const char* value) override
		{
			return value;
		}

		/**
		* \brief 工作对象退出
		*/
		bool worker_left(const char* addr) override
		{
			if (addr == nullptr || strlen(addr) == 0)
				return false;
			if (!balance_station<api_station, string, STATION_TYPE_API>::worker_left(addr))
				return false;
			_balance.left(addr);
			return true;
		}

		/**
		* \brief 工作对象加入
		*/
		bool worker_join(const char* addr) override
		{
			if (addr == nullptr || strlen(addr) == 0)
				return false;
			if (!balance_station<api_station, string, STATION_TYPE_API>::worker_join(addr))
				return false;
			_balance.join(addr);
			return true;
		}
		/**
		* \brief 工作对象加入
		*/
		bool worker_heat(const char* addr) override
		{
			if (!balance_station<api_station, string, STATION_TYPE_API>::worker_heat(addr))
				return false;
			_balance.succees(addr);
			return true;
		}
	}
}
#endif
