#ifndef AGEBULL_RPCSERVICE_H
#define AGEBULL_RPCSERVICE_H
#pragma once
#include "stdafx.h"
namespace agebull
{
	namespace zmq_net
	{
		class rpc_service
		{
		public:
			/**
			* \brief 初始化
			*/
			static bool initialize();

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
		/**
		* \brief 取本机IP
		*/
		void get_local_ips(acl::string& host, vector<acl::string>& ips);
		/**
		* \brief 记录堆栈信息
		*/
		void sig_crash(int sig);
		/**
		* \brief sig对应的文本
		*/
		const char* sig_text(int sig);
	}
}
#endif //!AGEBULL_RPCSERVICE_H