#ifndef ZMQ_API_BROADCASTING_STATION_H
#define ZMQ_API_BROADCASTING_STATION_H
#pragma once
#include "../stdinc.h"
#include <utility>
#include "zero_station.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 表示一个广播站点
		*/
		class broadcasting_station :public zero_station
		{
		public:
			/**
			 * \brief 构造
			 * \param name 
			 */
			broadcasting_station(string name)
				: zero_station(std::move(name), STATION_TYPE_PUBLISH, ZMQ_ROUTER, ZMQ_PUB, -1)
			{
			}

			/**
			* \brief 执行一条命令
			*/
			sharp_char command(const char* caller, vector< sharp_char> lines) final
			{
				const bool res = publish(caller, lines[0], lines[1], lines[1]);
				return sharp_char(res ? ZERO_STATUS_OK : ZERO_STATUS_FAILED);
			}


			/**
			* \brief 工作开始（发送到工作者）
			*/
			void job_start(ZMQ_HANDLE socket, sharp_char& global_id, vector<sharp_char>& list) final;
			/**
			*\brief 发送消息
			*/
			bool publish(const sharp_char& title, const sharp_char& description, vector<sharp_char>& datas);

			/**
			*\brief 发布消息
			*/
			bool publish(const string& publiher, const string& title, const string& arg);
			/**
			*\brief 广播内容
			*/
			bool publish(const string& publiher, const string& title, const string& sub, const string& arg);
			/**
			*\brief 广播内容
			*/
			bool publish(const string& publiher, const string& title, const string& sub, const string& plan, const string& arg) const;


			/**
			 * \brief 析构
			 */
			virtual ~broadcasting_station() = default;
			/**
			* \brief 运行一个广播线程
			*/
			static void run(string name)
			{
				boost::thread(launch, make_shared<broadcasting_station>(name));
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
				auto station = new broadcasting_station(config->station_name_);
				station->config_ = config;
				boost::thread(boost::bind(launch, shared_ptr<broadcasting_station>(station)));
			}
			/**
			*\brief 运行一个广播线程
			*/
			static void launch(shared_ptr<broadcasting_station> station);
		};
	}
}
#endif//!ZMQ_API_BROADCASTING_STATION_H