#ifndef ZMQ_ROUTE_API_STATION_H
#define ZMQ_ROUTE_API_STATION_H
#pragma once
#include "../stdinc.h"
#include "zero_station.h"
namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief API站点
		*/
		class route_api_station :public zero_station
		{
		public:
			/**
			* \brief 构造
			*/
			route_api_station(string name)
				: zero_station(name, zero_def::station_type::route_api, ZMQ_ROUTER, ZMQ_ROUTER)
			{
			}

			/**
			* \brief 构造
			*/
			route_api_station(shared_ptr<zero_config>& config)
				: zero_station(config, zero_def::station_type::route_api, ZMQ_ROUTER, ZMQ_ROUTER)
			{
			}
			/**
			* \brief 析构
			*/
			virtual ~route_api_station() = default;

			/**
			*\brief 运行
			*/
			static void run(const string& name)
			{
				boost::thread thrds_s1(boost::bind(launch, std::make_shared<route_api_station>(name)));
			}

			/**
			*\brief 运行
			*/
			static void run(shared_ptr<zero_config>& config)
			{
				if (config->is_state(station_state::stop))
					return;
				boost::thread(boost::bind(launch, std::make_shared<route_api_station>(config)));
			}
			/**
			* \brief 执行
			*/
			static void launch(shared_ptr<route_api_station>& station);
		private:
			/**
			* \brief 工作开始 : 处理请求数据
			*/
			void job_start(zmq_handler socket, vector<shared_char>& list, bool inner, bool old) final;
			/**
			* \brief 工作结束(发送到请求者)
			*/
			void job_end(vector<shared_char>& list) final;
		};

	}
}
#endif//!ZMQ_ROUTE_API_STATION_H
