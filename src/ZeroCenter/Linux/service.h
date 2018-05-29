#ifndef AGEBULL_RPCSERVICE_H
#define AGEBULL_RPCSERVICE_H
#pragma once
#include "stdafx.h"
#include "net/station_warehouse.h"

namespace agebull
{
	namespace zmq_net
	{
		class rpc_service
		{
			/**
			 * \brief 取本机IP并显示在控制台
			 */
			static bool get_local_ips();
		public:
			/**
			* \brief 初始化
			*/
			static bool initialize()
			{
				// 在程序初始化时打开日志
				acl::acl_cpp_init();
				string path;
				get_process_file_path(path);
				cout << "Current folder:" << path << endl;
				auto log = path;
				log.append("/zero.log");
				logger_open(log.c_str(), "zero_center", DEBUG_CONFIG);
				{
					redis_live_scope scope;
					if (!ping_redis())
					{
						std::cout << "Redis:failed" << endl;
						return false;
					}
					std::cout << "Redis:ready" << endl;
				}
				if (!get_local_ips())
				{
					std::cout << "Ip:empty" << endl;
					return false;
				}
				int major, minor, patch;
				zmq_version(&major, &minor, &patch); 
				printf("Current ØMQ version is %d.%d.%d\n", major, minor, patch);
				return station_warehouse::initialize();
			}

			/**
			* \brief
			*/
			static void start()
			{
				config_zero_center();
				start_zero_center();

				log_msg("zero center in service");
			}

			/**
			* \brief 中止
			*/
			static void stop()
			{
				close_net_command();
				distory_net_command();
				thread_sleep(50);
				acl::log::close();
			}

		};

		/**
		* \brief 系统信号处理
		*/
		void on_sig(int sig);
	}
}
#endif //!AGEBULL_RPCSERVICE_H