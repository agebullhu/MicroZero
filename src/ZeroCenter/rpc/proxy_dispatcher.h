#pragma once
#ifndef _PROXY_DISPATCHER_H_
#define _PROXY_DISPATCHER_H_
#include "../stdinc.h"
#include "zero_station.h"
#include "inner_socket.h"

#ifdef PROXYSTATION

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 代理节点
		*/
		struct proxy_item
		{
			/**
			* \brief 名称
			*/
			std::string name;
			/**
			* \brief 返回地址
			*/
			std::string res_addr;
			/**
			* \brief 请求地址
			*/
			std::string req_addr;
			/**
			* \brief 请求句柄
			*/
			zmq_handler req_socket;
			/**
			* \brief 返回句柄
			*/
			zmq_handler res_socket;

			proxy_item(): req_socket(nullptr), res_socket(nullptr)
			{
			}
		};
		/**
		* \brief 表示计划任务调度服务
		*/
		class proxy_dispatcher :public zero_station
		{
			map<std::string, proxy_item> proxys_;
		public:
			/**
			* \brief 单例
			*/
			static proxy_dispatcher* instance;

			/**
			* \brief 构造
			*/
			proxy_dispatcher()
				: zero_station(zero_def::name::proxy_dispatcher, zero_def::station_type::proxy, ZMQ_ROUTER, ZMQ_PUB)
			{
			}

			/**
			* \brief 构造
			*/
			proxy_dispatcher(shared_ptr<station_config>& config)
				: zero_station(config, zero_def::station_type::plan, ZMQ_ROUTER, ZMQ_PUB)
			{
			}

			/**
			* \brief 析构
			*/
			~proxy_dispatcher() override
			{
				cout << "queue_station destory" << endl;
			}
			/**
			* \brief 运行一个通知线程
			*/
			static void run()
			{
				instance = new proxy_dispatcher();
				boost::thread(boost::bind(launch, shared_ptr<proxy_dispatcher>(instance)));
			}
		private:
			/**
			*\brief 运行
			*/
			static void run(shared_ptr<station_config>& config)
			{
				instance = new proxy_dispatcher(config);
				boost::thread(boost::bind(launch, shared_ptr<proxy_dispatcher>(instance)));
			}
			/**
			* \brief 消息泵
			*/
			static void launch(shared_ptr<proxy_dispatcher>& station);


			/**
			* \brief 计划轮询
			*/
			static void run_proxy_poll(proxy_dispatcher* station)
			{
				station->proxy_poll();
			}

			/**
			* \brief 计划轮询
			*/
			void proxy_poll();

			/**
			* \brief 进入
			*/
			bool on_start(zmq_handler socket, shared_char name, vector<shared_char>& list);

			/**
			* \brief 工作开始 : 处理请求数据
			*/
			void job_start(zmq_handler socket, vector<shared_char>& list, bool inner, bool old) final;

			/**
			* \brief 工作结束(发送到请求者)
			*/
			void job_end(vector<shared_char>& list) final;

			/**
			* \brief 返回
			*/
			void on_result(zmq_handler socket, const char* caller, uchar state);

		};
	}
}

#endif // PROXYSTATION
#endif //!_PROXY_DISPATCHER_H_