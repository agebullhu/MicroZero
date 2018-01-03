#ifndef AGEBULL_RPC_H
#pragma once
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
		auto log = path;
		log.append(".log");
		logger_open(log.c_str(), "mq_server", DEBUG_CONFIG);
	}

	/**
	* \brief
	*/
	static void Start()
	{
		init_net_command();
		start_net_command();
	}

	/**
	* \brief 中止
	*/
	static void Stop()
	{
		distory_net_command();
		acl::log::close();
	}
};
#endif AGEBULL_RPC_H