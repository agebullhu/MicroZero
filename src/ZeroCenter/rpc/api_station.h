#ifndef ZMQ_API_STATION_H
#define ZMQ_API_STATION_H
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
		class api_station :public zero_station
		{
		public:
			/**
			* \brief 构造
			*/
			api_station(string name)
				: zero_station(name, station_type_api, ZMQ_ROUTER, ZMQ_PUSH)
			{
			}

			/**
			* \brief 构造
			*/
			api_station(shared_ptr<zero_config>& config)
				: zero_station(config, station_type_api, ZMQ_ROUTER, ZMQ_PUSH)
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
				if (config->is_state(station_state::stop))
					return;
				boost::thread(boost::bind(launch, std::make_shared<api_station>(config)));
			}
			/**
			* \brief 执行
			*/
			static void launch(shared_ptr<api_station>& station);
		private:
			/**
			* \brief 内部命令
			*/
			bool extend_command(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner) override;
			/**
			* \brief 工作开始（发送到工作者）
			*/
			void job_start(zmq_handler socket, vector<shared_char>& list, bool inner) final;
			/**
			* \brief 工作结束(发送到请求者)
			*/
			void job_end(vector<shared_char>& list) final;
		};

	}
}
#endif//!ZMQ_API_STATION_H
