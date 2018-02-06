#ifndef _AGEBULL_NET_COMMAND_H
#define _AGEBULL_NET_COMMAND_H
#pragma once
#include "stdinc.h"
namespace agebull
{
	typedef void* ZMQ_HANDLE;

	typedef char* TSON_BUFFER;

	using namespace std;

	//网络状态
	typedef int NET_STATE;
	const NET_STATE NET_STATE_NONE = 0;
	const NET_STATE NET_STATE_RUNING = 1;
	const NET_STATE NET_STATE_CLOSING = 2;
	const NET_STATE NET_STATE_CLOSED = 3;
	const NET_STATE NET_STATE_DISTORY = 4;

	//ZMQ上下文对象
	ZMQ_HANDLE get_zmq_context();
	//运行状态
	NET_STATE get_net_state();
	//运行状态
	//void set_net_state(NET_STATE state);

	//初始化网络命令环境
	int init_net_command();
	//启动网络命令环境
	int start_net_command();
	//销毁网络命令环境
	void distory_net_command();
	//关闭网络命令环境
	void close_net_command(bool wait = true);

	//线程计数清零
	void reset_command_thread();
	//登记线程失败
	void set_command_thread_bad(const char* name);
	//登记线程开始
	void set_command_thread_start(const char* name);
	//登记线程关闭
	void set_command_thread_end(const char* name);

	//网络监控
	DWORD zmq_monitor(const char * address);
	//生成CRC校验码
	ushort get_crc(const char *msg, size_t len);
}
#endif
