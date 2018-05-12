#pragma once
#ifndef _ZMQ_NET_BALANCESTATION_H_
#include "../stdinc.h"
#include "ZeroStation.h"
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
			* 心跳的响应
			*/
			void heartbeat() override;
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
		* \brief
		*/
		template <typename TNetStation, class TWorker, int NetType>
		void balance_station<TNetStation, TWorker, NetType>::heartbeat()
		{
			vector<sharp_char> list;
			//0 路由到的地址 1 空帧 2 命令 3 服务器地址
			zmq_state_ = recv(heart_socket_, list);
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
			zmq_state_ = send_addr(heart_socket_, *list[0]);
			zmq_state_ = send_late(heart_socket_, status);
		}

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
			log_trace2(DEBUG_BASE, 1, "station %s => %s left", station_name_, addr);
			monitor_async(station_name_, "worker_left", addr);

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
			log_trace2(DEBUG_BASE, 1, "station %s => %s join", station_name_, addr);
			monitor_async(station_name_, "worker_join", addr);
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
			monitor_async(station_name_, "worker_heat", addr);
			return true;
		}
	}
}
#endif
