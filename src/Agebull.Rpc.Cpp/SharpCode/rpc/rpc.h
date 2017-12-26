#pragma once
#ifndef AGEBULL_RPC_H
#include "stdafx.h"

class CRpcService
{
public:
	/**
	* \brief 初始化
	*/
	static void Initialize()
	{
		// 在程序初始化时打开日志
		acl::acl_cpp_init();
		string path;
		GetProcessFilePath(path);
		string log = path;
		log.append(".log");
		logger_open(log.c_str(), "mq_server", DEBUG_CONFIG);
		log_acl_msg("Initialize");
	}

	/**
	* \brief 中止
	*/
	static void Stop()
	{
		log_acl_msg("Stop");
		//结束网络库
		distory_net_command();
		acl::log::close();
		thread_sleep(1000);
	}

	/**
	* \brief
	*/
	static void Start()
	{
		log_acl_msg("Start");
		// 这里添加自己的初始化代码...
		init_net_command();
		start_net_command();
	}
};
#endif AGEBULL_RPC_H