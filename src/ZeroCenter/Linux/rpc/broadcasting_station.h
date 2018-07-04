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
				: zero_station(std::move(name), STATION_TYPE_PUBLISH, ZMQ_ROUTER)
			{
			}

			/**
			 * \brief 构造
			 * \param config 
			 */
			broadcasting_station(shared_ptr<zero_config>& config)
				: zero_station(config, STATION_TYPE_PUBLISH, ZMQ_ROUTER)
			{
			}

			/**
			* \brief 工作开始（发送到工作者）
			*/
			void job_start(ZMQ_HANDLE socket, vector<shared_char>& list,bool inner) final;
			/**
			*\brief 发送消息
			*/
			bool publish(const shared_char& title, const shared_char& description, vector<shared_char>& datas);

			/**
			*\brief 发布消息
			*/
			bool publish(const string& publiher, const string& title, const string& arg);
			/**
			*\brief 广播内容
			*/
			bool publish(const string& publiher, const string& title, const string& sub, const string& arg);
			
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
				if (config->is_state(station_state::Uninstall))
					return;
				boost::thread(boost::bind(launch, std::make_shared<broadcasting_station>(config)));
			}
			/**
			*\brief 运行一个广播线程
			*/
			static void launch(shared_ptr<broadcasting_station> station);
		};
	}
}
#endif//!ZMQ_API_BROADCASTING_STATION_H