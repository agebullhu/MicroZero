#ifndef AGEBULL_RPCSERVICE_H
#define AGEBULL_RPCSERVICE_H
#pragma once
#include "stdafx.h"
namespace agebull
{
	namespace zmq_net
	{
		//初始化网络命令环境
		int config_zero_center();
		//启动网络命令环境
		int start_zero_center();
		//销毁网络命令环境
		void close_net_command();
		//生成CRC校验码
		ushort get_crc(const char *msg, size_t len);

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

		class rpc_service
		{
			/**
			* \brief 主线程等待信号量
			*/
			static boost::interprocess::interprocess_semaphore wait_semaphore;
		public:
			static boost::posix_time::ptime start_time;
			/**
			* \brief 初始化
			*/
			static bool initialize();

			/**
			* \brief
			*/
			static void start();

			/**
			*\brief 等待结束
			*/
			static void wait_zero();

			/**
			*\brief 等待结束
			*/
			static void close_zero();

			/**
			* \brief 中止
			*/
			static void stop();
		};
	}
}
#endif //!AGEBULL_RPCSERVICE_H