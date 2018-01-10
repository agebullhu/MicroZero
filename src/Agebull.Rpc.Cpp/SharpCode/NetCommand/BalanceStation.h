#pragma once
#ifndef _ZMQ_NET_BALANCESTATION_H_
#include <stdinc.h>
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
		class BalanceStation : public ZeroStation
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
				: ZeroStation(name, NetType, ZMQ_ROUTER, ZMQ_ROUTER, ZMQ_ROUTER)
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
			* @brief 工作对象退出
			*/
			virtual void worker_left(const char* addr);

			/**
			* @brief 工作对象加入
			*/
			virtual void worker_join(const char* addr, const  char* value, bool ready = false);
		};


		/**
		* \brief
		*/
		template <typename TNetStation, class TWorker, int NetType>
		void BalanceStation<TNetStation, TWorker, NetType>::heartbeat()
		{
			vector<sharp_char> list;
			//0 路由到的地址 1 空帧 2 命令 3 服务器地址
			_zmq_state = recv(_heart_socket, list);
			switch (list[2][0])
			{
			case '@':
				worker_join(*list[3], *list[3]);
				break;
			case '-':
				worker_left(*list[3]);
				break;
			default: break;
			}
			_zmq_state = send_addr(_heart_socket, *list[0]);
			_zmq_state = send_late(_heart_socket, "OK");
		}

		/**
		* \brief
		* \param addr
		*/
		template <typename TNetStation, class TWorker, int NetType>
		void BalanceStation<TNetStation, TWorker, NetType>::worker_left(const char* addr)
		{
			auto vote = _workers.find(addr);
			if (vote != _workers.end())
			{
				_workers.erase(addr);
				monitor(_station_name, "worker_left", addr);
				vector<sharp_char> result;
				vector<string> argument;
				argument.push_back("@");
				argument.push_back(addr);
				RequestSocket<ZMQ_REQ, false> socket("_sys_", _station_name.c_str());
				socket.request(argument, result);
			}
		}

		/**
		* \brief
		* \param addr
		* \param value
		* \param ready
		*/
		template <typename TNetStation, class TWorker, int NetType>
		void BalanceStation<TNetStation, TWorker, NetType>::worker_join(const char* addr, const  char* value, bool ready)
		{
			TWorker item = create_item(addr, value);
			auto old = _workers.find(addr);
			if (old == _workers.end())
			{
				_workers.insert(make_pair(addr, item));
				//cout << addr << endl;
				monitor(_station_name, "worker_join", addr);
			}
			else
			{
				old->second = item;
				//monitor(_station_name, "worker_heart", addr);
			}
		}

	}
}
#endif
