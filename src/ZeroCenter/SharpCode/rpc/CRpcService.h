#ifndef AGEBULL_RPCSERVICE_H
#pragma once
#include "stdafx.h"
namespace agebull
{
	class rpc_service
	{
	public:
		/**
		* \brief 初始化
		*/
		static void initialize()
		{
			// 在程序初始化时打开日志
			acl::acl_cpp_init();
			string path;
			GetProcessFilePath(path);
			auto log = path;
			log.append(".log");
			logger_open(log.c_str(), "mq_server", DEBUG_CONFIG);
			if (!ping_redis())
			{
				std::cout << "Redis:failed";
			}
			else
			{
				std::cout << "Redis:ready";
			}
		}

		/**
		* \brief
		*/
		static void start()
		{
			init_net_command();
			start_net_command();

			log_msg("zero center in service");
		}

		/**
		* \brief 中止
		*/
		static void stop()
		{
			close_net_command();
			distory_net_command();
			acl::log::close();
		}
	};
}
#endif AGEBULL_RPCSERVICE_H