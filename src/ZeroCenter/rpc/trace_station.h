#ifndef ZMQ_API_TRACE_STATION_H
#define ZMQ_API_TRACE_STATION_H
#pragma once
#include "../stdinc.h"
#include <utility>
#include "zero_station.h"
#include "../sqlite/trace_storage.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 表示一个通知站点
		*/
		class trace_station :public zero_station
		{
			trace_storage* storage_;

			/**
			* \brief 扩展初始化
			*/
			void initialize_ext() final
			{
				storage_ = new trace_storage();
				storage_->prepare_storage();
			}
			/**
			* \brief 析构
			*/
			void destruct_ext() final
			{
				delete storage_;
			}

			/**
			* \brief 工作开始 : 处理请求数据
			*/
			void job_start(zmq_handler socket, vector<shared_char>& list, bool inner, bool old) final;
			/**
			*\brief 运行一个通知线程
			*/
			static void launch(shared_ptr<trace_station> station);
		public:
			/**
			 * \brief 构造
			 * \param name
			 */
			trace_station(string name)
				: zero_station(std::move(name), zero_def::station_type::trace, ZMQ_ROUTER, 0)
				, storage_(nullptr)
			{
			}

			/**
			 * \brief 构造
			 * \param config
			 */
			trace_station(shared_ptr<zero_config>& config)
				: zero_station(config, zero_def::station_type::trace, ZMQ_ROUTER, 0)
				, storage_(nullptr)
			{
			}
			/**
			 * \brief 析构
			 */
			virtual ~trace_station() = default;
			/**
			* \brief 运行一个通知线程
			*/
			static void run()
			{
				boost::thread(launch, make_shared<trace_station>(zero_net::zero_def::name::trace_dispatcher));
			}
			/**
			*\brief 运行
			*/
			static void run(shared_ptr<zero_config>& config)
			{
				if (config->is_state(station_state::stop))
					return;
				boost::thread(boost::bind(launch, std::make_shared<trace_station>(config)));
			}
		};
	}
}
#endif//!ZMQ_API_NOTIFY_STATION_H