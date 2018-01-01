#ifndef _AGEBULL_NET_COMMAND_H
#define _AGEBULL_NET_COMMAND_H
#pragma once
#include "stdinc.h"
#ifdef NOCLR
#include "redis/redis.h"
#endif
#ifdef WIN32
#include <objbase.h>  
#else
#include <uuid/uuid.h>  
typedef uuid_t GUID;
#endif

#include "command_def.h"
#include <tson/tson_serializer.h>
#include <Tson/tson_deserializer.h>
#ifdef EsTrade
#define COMMANDPROXY
#endif
#ifdef GBS_TRADE
#define COMMANDPROXY
#endif

using namespace std;
//初始化网络命令环境
int init_net_command();
//启动网络命令环境
int start_net_command();
//销毁网络命令环境
void distory_net_command();
//关闭网络命令环境
void close_net_command();
//登记线程开始
void set_command_thread_start();
//登记线程关闭
void set_command_thread_end();

//网络监控
DWORD zmq_monitor(const char * address);


//ZMQ上下文对象
ZMQ_HANDLE get_zmq_context();
//运行状态
NET_STATE get_net_state();
//写入CRC校验码
void write_crc(PNetCommand cmd);
//校验CRC校验码
bool check_crc(PNetCommand cmd);

//point是服务器标识(本地交易:lt, 易盛交易 : et, 易盛行情 : eq, 本地行情 : lq, 本地主账号 : la, 本地其它 : lo)
#define command_key_fmt "b:cmd:lt:%s"

//反序列化宏
#define deserialize_cmd_arg(cmd,type,name) \
	type name;\
	Agebull::Tson::Deserializer reader(get_cmd_buffer(cmd));\
	Deserialize(reader, &name);


//序列化宏
#define serialize_to_cmd(type,args,cmd_num) \
	PNetCommand net_cmd = reinterpret_cast<PNetCommand>(new char[sizeof(type) * 2]);\
	memset(net_cmd,0,NETCOMMAND_HEAD_LEN);\
	net_cmd->cmd_id = cmd_num;\
	Agebull::Tson::Serializer wirter(get_cmd_buffer(net_cmd), sizeof(type) * 2 - NETCOMMAND_HEAD_LEN);\
	Serialize(wirter, &args);



//生成ZMQ连接对象
inline static ZMQ_HANDLE create_socket(int type, const char* address)
{
	ZMQ_HANDLE socket = zmq_socket(get_zmq_context(), type);
	if (socket == nullptr)
	{
		auto err = zmq_strerror(errno);
		log_error2("构造SOCKET对象(%s)发生错误:%s", address, err);
		return nullptr;
	}
	//zmq_result = zmq_socket_monitor(socket, "inproc://server_cmd.rep", ZMQ_EVENT_ALL);
	//assert(zmq_result == 0);
	int iZMQ_IMMEDIATE = 1;//列消息只作用于已完成的链接
	zmq_setsockopt(socket, ZMQ_LINGER, &iZMQ_IMMEDIATE, sizeof(int));
	int iLINGER = 50;//关闭设置停留时间,毫秒
	zmq_setsockopt(socket, ZMQ_LINGER, &iLINGER, sizeof(int));
	int iRcvTimeout = 500;
	zmq_setsockopt(socket, ZMQ_RCVTIMEO, &iRcvTimeout, sizeof(int));
	int iSndTimeout = 500;
	zmq_setsockopt(socket, ZMQ_SNDTIMEO, &iSndTimeout, sizeof(int));
	int zmq_result = zmq_bind(socket, address);
	if (zmq_result < 0)
	{
		auto err = zmq_strerror(errno);
		zmq_close(socket);
		log_error2("绑定端口(%s)发生错误:%s", address, err);
		return nullptr;
	}
	return socket;
}
//检查ZMQ错误
inline static bool check_zmq_error(int& state, const char* address, const char* action)
{
	switch (errno)
	{
	case ETERM:
		log_error2("%s(%s)错误[与指定的socket相关联的context被终结了],自动关闭", action, address);
		state = 1;
		return false;
	case ENOTSOCK:
		log_error2("%s(%s)错误[指定的socket不可用],自动重启", action, address);
		state = 2;
		return false;
	case EINTR:
		log_error2("%s(%s)错误[在接接收到消息之前，这个操作被系统信号中断了],自动重启", action, address);
		state = 2;
		return false;
	case EAGAIN://使用非阻塞方式接收消息的时候没有接收到任何消息。
	default:
		state = 0;
		return true;
	}
}
#endif
