#pragma once
#ifndef RPC_MANAGER_H
namespace agebull
{
	namespace Rpc
	{
		/*
		* @briref 主机握手
		*/
		char* mathine_handle(const char* name,const char* key);
		/*
		* @briref 主机注册
		*/
		void mathine_regist();
		/*
		* @briref 主机配置同步
		*/
		void mathine_sync_config();
		/*
		* @briref 主机心跳
		*/
		void mathine_heart();
		/*
		* @briref 主机呼叫
		*/
		void mathine_call();
		/*
		* @briref 主机关机
		*/
		void mathine_shutdown();
		/*
		* @briref 主机下线
		*/
		void mathine_up_down();
		/*
		* @briref 主机下线
		*/
		void mathine_discard();
	}
}
#endif