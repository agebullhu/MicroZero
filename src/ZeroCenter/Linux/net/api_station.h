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
				if(config->station_state_ > station_state::ReStart && config->station_state_ < station_state::Closed)
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
			* \brief 工作集合的响应
			*/
			void response() override;
			/**
			* \brief 调用集合的响应
			*/
			void request(ZMQ_HANDLE socket, bool inner) override;
			/**
			* \brief 执行一条命令
			*/
			sharp_char command(const char* caller, vector<sharp_char> lines) override;


			/**
			* \brief 工作进入计划
			*/
			bool job_plan(ZMQ_HANDLE socket, vector<sharp_char>& list);
			/**
			* \brief 工作开始（发送到工作者）
			*/
			bool job_start(ZMQ_HANDLE socket, vector<sharp_char>& list);
			/**
			* \brief 工作结束(发送到请求者)
			*/
			bool job_end(vector<sharp_char>& list);

		};
	}
}
#endif//!ZMQ_API_STATION_H
