#ifndef ZMQ_API_NOTIFY_STATION_H
#define ZMQ_API_NOTIFY_STATION_H
#pragma once
#include "../stdinc.h"
#include <utility>
#include "zero_station.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 表示一个通知站点
		*/
		class notify_station :public zero_station
		{
			//static char frames1[], frames2[], frames3[];
		public:
			/**
			 * \brief 构造
			 * \param name
			 */
			notify_station(string name)
				: zero_station(std::move(name), zero_def::station_type::notify, ZMQ_ROUTER, ZMQ_PUB)
			{
			}

			/**
			 * \brief 构造
			 * \param config
			 */
			notify_station(shared_ptr<station_config>& config)
				: zero_station(config, zero_def::station_type::notify, ZMQ_ROUTER, ZMQ_PUB)
			{
			}

			/**
			* \brief 工作开始 : 处理请求数据
			*/
			void job_start(zmq_handler socket, vector<shared_char>& list, bool inner, bool old) final;
			/* *
			*\brief 发送消息
			* /
			bool publish(const shared_char& title, const shared_char& description, vector<shared_char>& datas);

			/* *
			*\brief 发布消息
			* /
			bool publish(const string& publiher, const string& title, const string& arg);
			/* *
			*\brief 通知内容
			* /
			bool publish(const string& publiher, const string& title, const string& sub, const string& arg);
			/* *
			*\brief 发布消息
			* /
			bool publish(const string& publiher, const string& title, const string& sub, const string& arg, const string& rid, const int64 gid, const int64 lid);
			*/

			/**
			 * \brief 析构
			 */
			virtual ~notify_station() = default;
			/**
			* \brief 运行一个通知线程
			*/
			static void run(string name)
			{
				boost::thread(launch, make_shared<notify_station>(name));
			}
			/**
			*\brief 运行
			*/
			static void run(shared_ptr<station_config>& config)
			{
				if (config->is_state(station_state::stop))
					return;
				boost::thread(boost::bind(launch, std::make_shared<notify_station>(config)));
			}
			/**
			*\brief 运行一个通知线程
			*/
			static void launch(shared_ptr<notify_station> station);
		};
	}
}
#endif//!ZMQ_API_NOTIFY_STATION_H