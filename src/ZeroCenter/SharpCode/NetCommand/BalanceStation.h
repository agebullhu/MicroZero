#pragma once
#ifndef _ZMQ_NET_BALANCESTATION_H_
#include <stdinc.h>
#include "ZeroStation.h"
namespace agebull
{
	namespace zmq_net
	{

		/**
		* \brief ��ʾһ������ZMQ�ĸ��ؾ���վ��
		* \tparam TNetStation
		* \tparam TWorker
		* \tparam NetType
		*/
		template <typename TNetStation, class TWorker, int NetType>
		class balance_station : public zero_station
		{
		protected:
			/**
			* \brief �����߼���
			*/
			map<string, TWorker> _workers;
		public:
			/**
			* \brief ����
			*/
			balance_station(string name)
				: zero_station(name, NetType, ZMQ_ROUTER, ZMQ_ROUTER, ZMQ_ROUTER)
			{
			}

		protected:

			/**
			* \brief ���ɹ�������
			*/
			virtual TWorker create_item(const char* addr, const char* value) = 0;

			/**
			* ��������Ӧ
			*/
			void heartbeat() override;
			/**
			* \brief ���������˳�
			*/
			virtual void worker_left(const char* addr);

			/**
			* \brief �����������
			*/
			virtual void worker_join(const char* addr, const  char* value, bool ready = false);
		};


		/**
		* \brief
		*/
		template <typename TNetStation, class TWorker, int NetType>
		void balance_station<TNetStation, TWorker, NetType>::heartbeat()
		{
			vector<sharp_char> list;
			//0 ·�ɵ��ĵ�ַ 1 ��֡ 2 ���� 3 ��������ַ
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
		void balance_station<TNetStation, TWorker, NetType>::worker_left(const char* addr)
		{
			auto vote = _workers.find(addr);
			if (vote != _workers.end())
			{
				_workers.erase(addr);
				monitor_async(_station_name, "worker_left", addr);

				vector<sharp_char> result;
				vector<string> argument{"@",addr };
				inproc_request_socket<ZMQ_REQ> socket("_sys_", _station_name.c_str());
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
		void balance_station<TNetStation, TWorker, NetType>::worker_join(const char* addr, const  char* value, bool ready)
		{
			TWorker item = create_item(addr, value);
			auto old = _workers.find(addr);
			if (old == _workers.end())
			{
				_workers.insert(make_pair(addr, item));
				log_trace2(DEBUG_BASE, 1, "station %s => %s join", _station_name, addr);
				monitor_async(_station_name, "worker_join", addr);
				monitor_async(_station_name, "worker_heat", value);
			}
			else
			{
				old->second = item;
				log_trace2(DEBUG_BASE, 1, "station %s => %s heartbeat", _station_name, addr);
				//monitor(_station_name, "worker_heart", addr);
			}
		}

	}
}
#endif