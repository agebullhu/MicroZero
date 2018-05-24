#ifndef ZMQ_API_STATION_H
#define ZMQ_API_STATION_H
#pragma once
#include "../stdinc.h"
#include "zero_station.h"
namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief API站点
		*/
		class api_station :public zero_station
		{
		public:
			/**
			* \brief 构造
			*/
			api_station(string name)
				: zero_station(name, STATION_TYPE_API, ZMQ_ROUTER, ZMQ_DEALER, ZMQ_ROUTER)
			{
			}

			/**
			* \brief 析构
			*/
			virtual ~api_station() = default;

			/**
			*\brief 运行
			*/
			static void run(const string& name)
			{
				boost::thread thrds_s1(boost::bind(launch, std::make_shared<api_station>(name)));
			}

			/**
			*\brief 运行
			*/
			static void run(shared_ptr<zero_config>& config)
			{
				if (config->station_state_ == station_state::Uninstall)
					return;
				if (config->station_state_ > station_state::ReStart && config->station_state_ < station_state::Closed)
				{
					config->station_state_ = station_state::ReStart;
					return;
				}
				auto station = new api_station(config->station_name_);
				station->config_ = config;
				boost::thread(boost::bind(launch, shared_ptr<api_station>(station)));
			}
			/**
			* \brief 执行
			*/
			static void launch(shared_ptr<api_station>& station);
		private:
			/**
			* \brief 执行一条命令
			*/
			sharp_char command(const char* caller, vector<sharp_char> lines) final;

			/**
			* \brief 工作开始（发送到工作者）
			*/
			void job_start(ZMQ_HANDLE socket, sharp_char& global_id, vector<sharp_char>& list) final;
			/**
			* \brief 工作结束(发送到请求者)
			*/
			void job_end(vector<sharp_char>& list) final;
		};

		/**
		* \brief 工作开始（发送到工作者）
		*/
		inline void api_station::job_start(ZMQ_HANDLE socket, sharp_char& global_id, vector<sharp_char>& list)
		{
			//路由到其中一个工作对象
			//const char* worker;
			//while(!_balance.get_host(worker))
			//{
			//	worker_left(worker);
			//}
			//if (worker == nullptr)
			//{
			//	zmq_state_ = send_status(socket, *caller, ZERO_STATUS_API_NOT_WORKER);
			//	return zmq_state_ == zmq_socket_state::Succeed;
			//}
			if (list[2][1] == ZERO_COMMAND_WAITING)
				return;
			if (!send_response(list))
			{
				send_request_status(socket, *list[0], ZERO_STATUS_NOT_WORKER_ID, *global_id);
			}
			else
			{
				send_request_status(socket, *list[0], ZERO_STATUS_WAITING_ID, *global_id);
			}
		}

		/**
		* \brief 工作结束(发送到请求者)
		*/
		inline void api_station::job_end(vector<sharp_char>& list)
		{
			send_request_result(list[1][0] == '-' ? request_socket_ipc_ : request_scoket_tcp_, list);
		}
	}
}
#endif//!ZMQ_API_STATION_H
