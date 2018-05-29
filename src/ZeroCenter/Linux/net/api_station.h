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
			* \brief 构造
			*/
			api_station(shared_ptr<zero_config>& config)
				: zero_station(config, STATION_TYPE_API, ZMQ_ROUTER, ZMQ_DEALER, ZMQ_ROUTER)
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
				boost::lock_guard<boost::mutex> guard(config->mutex_);
				if (config->station_state_ == station_state::Uninstall)
					return;
				if (config->station_state_ > station_state::ReStart && config->station_state_ < station_state::Closed)
				{
					config->station_state_ = station_state::ReStart;
					return;
				}
				boost::thread(boost::bind(launch, std::make_shared<api_station>(config)));
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
			void job_start(ZMQ_HANDLE socket, vector<sharp_char>& list) final;//, sharp_char& global_id
			/**
			* \brief 工作结束(发送到请求者)
			*/
			void job_end(vector<sharp_char>& list) final;
		};

	}
}
#endif//!ZMQ_API_STATION_H
