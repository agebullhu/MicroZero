#ifndef AGEBULL_RPCSERVICE_H
#pragma once
#include "stdafx.h"
#include "winsock2.h"  
#include <direct.h>
#pragma comment(lib,"ws2_32.lib") // 静态库  
namespace agebull
{
	class rpc_service
	{
		/**
		 * \brief 取本机IP并显示在控制台
		 */
		static void get_local_ips();
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

			get_local_ips();
			char buffer[MAX_PATH + 1];
			char *p = _getcwd(buffer, MAX_PATH);
			cout << "Curent folder:" << p << endl;

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
			acl::log::close();
		}
	};

	inline void rpc_service::get_local_ips()
	{
		char hname[128];
		gethostname(hname, sizeof(hname));
		cout << "Host:" << hname << endl << "IPs:";
		struct addrinfo hint{};
		memset(&hint, 0, sizeof(hint));
		hint.ai_family = AF_INET;
		hint.ai_socktype = SOCK_STREAM;

		addrinfo* info = nullptr;
		char ipstr[16];
		if (getaddrinfo(hname, nullptr, &hint, &info) == 0 && info != nullptr)
		{
			addrinfo* now = info;
			bool first = true;
			do
			{
				inet_ntop(AF_INET, &(reinterpret_cast<struct sockaddr_in *>(now->ai_addr)->sin_addr), ipstr, 16);
				if (first)
					first = false;
				else
					cout << ",";
				cout << ipstr;
				now = now->ai_next;
			}
			while (now != nullptr);
			freeaddrinfo(info);
		}
		cout << endl;
	}
}
#endif AGEBULL_RPCSERVICE_H