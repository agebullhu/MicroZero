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
//客户端异步消息订阅
void notify_sub(const char *address);
//客户端命令请求
void request_cmd(const char *address);
//初始化网络命令环境
int init_net_command();
//销毁网络命令环境
void distory_net_command();
//关闭网络命令环境
void close_net_command();
//登记线程开始
void set_command_thread_start();
//登记线程关闭
void set_command_thread_end();
#ifdef NOCLR
//启动网络命令环境
int start_net_command();
#else
//启动网络命令环境
int start_net_command(const char* pub_addr, const char* cmd_addr, const char* quote_addr);
#endif
#ifdef NOCLR
//point是服务器标识(本地交易:lt, 易盛交易 : et, 易盛行情 : eq, 本地行情 : lq, 本地主账号 : la, 本地其它 : lo)
#ifdef EsTrade
#define command_key_fmt "b:cmd:et:%s"
#else
#define command_key_fmt "b:cmd:lt:%s"
#endif

//网络监控
DWORD zmq_monitor(const char * address);

#ifdef SERVER
//服务端路由（未实现）
//void server_route();
//服务端命令处理
void server_request();
//服务端异步回发
void server_answer();
//服务端消息广播
void server_notify();
//服务端消息泵
void server_message_pump();
//服务端消息回发
void command_answer(NetCommandArgPtr& cmd);
//服务端消息发送
void command_answer(PNetCommand cmd);

//服务端广播发送
void notify_message(PNetCommand cmd);
//服务端广播发送
void notify_message(NetCommandArgPtr& cmd);

//加入待执行命令队列
void push_cmd(NetCommandArgPtr& call);
//加入待执行命令队列
void push_cmd(PNetCommand arg);
/**
* @brief 发布命令状态
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
inline void publish_command_state(NetCommandArgPtr& cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	memcpy(result, cmd.m_command, sizeof(NetCommand));
	result->cmd_state = state;
	result->data_len = 0;
	NetCommandArgPtr ptr(result);
	command_answer(ptr);
}/**
* @brief 发布命令状态
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
inline void publish_command_state(NetCommand* cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = state;
	result->data_len = 0;
	NetCommandArgPtr ptr(result);
	command_answer(ptr);
}

//推送数据变更到用户
template<class T>
inline void send_data_to_user(T& data, const char* token)
{
	if (token == nullptr)
		return;
	PNetCommand cmd_arg = SerializeToCommand(&data);
	size_t len = strlen(token);
	if (len > GUID_LEN)
		len = GUID_LEN - 1;
	for (size_t idx = 0; idx < len; idx++)
	{
		cmd_arg->user_token[idx] = token[idx];
	}
	cmd_arg->cmd_id = NET_COMMAND_DATA_CHANGED;
	cmd_arg->cmd_state = NET_COMMAND_STATE_DATA;
	command_answer(cmd_arg);
}

//推送数据变更到用户
template<class T>
inline void send_data_to_user(T& data, const char* token, const char* cmdkey)
{
	if (token == nullptr)
		return;
	PNetCommand cmd_arg = SerializeToCommand(&data);

	size_t len = strlen(token);
	if (len > GUID_LEN)
		len = GUID_LEN - 1;
	for (size_t idx = 0; idx < len; idx++)
	{
		cmd_arg->user_token[idx] = token[idx];
	}
	len = strlen(cmdkey);
	if (len > GUID_LEN)
		len = GUID_LEN - 1;
	for (size_t idx = 0; idx < len; idx++)
	{
		cmd_arg->cmd_identity[idx] = cmdkey[idx];
	}
	cmd_arg->cmd_id = NET_COMMAND_DATA_CHANGED;
	cmd_arg->cmd_state = NET_COMMAND_STATE_DATA;
	command_answer(cmd_arg);
}
//发送用户请求相关数据
template<class T>
inline void send_cmd_data(T& data, PNetCommand call_arg)
{
	PNetCommand cmd_arg = SerializeToCommand(&data);
	memcpy(cmd_arg, call_arg, NETCOMMAND_HEAD_LEN);
	cmd_arg->cmd_id = NET_COMMAND_DATA_PUSH;
	cmd_arg->cmd_state = NET_COMMAND_STATE_DATA;
	command_answer(cmd_arg);
}
//发送通知数据
template<class T>
inline void notify_data(T& data, const char* token)
{
	if (token == nullptr)
		return;
	PNetCommand cmd_arg = SerializeToCommand(&data);
	size_t len = strlen(token);
	if (len > GUID_LEN)
		len = GUID_LEN - 1;
	for (size_t idx = 0; idx < len; idx++)
	{
		cmd_arg->user_token[idx] = token[idx];
	}
	cmd_arg->cmd_id = NET_COMMAND_DATA_CHANGED;
	cmd_arg->cmd_state = NET_COMMAND_STATE_DATA;
	notify_message(cmd_arg);
}
#endif
#endif
#ifdef CLIENT_COMMAND

//生成GUID
inline GUID create_guid()
{
	GUID guid;
	CoCreateGuid(&guid);
	return guid;
}
//GUID打印到文本
inline void print_guid(GUID& guid, char* buffer)
{
	sprintf_s(buffer, 33, "%08X%04X%04X%02X%02X%02X%02X%02X%02X%02X%02X",
		guid.Data1, guid.Data2, guid.Data3,
		guid.Data4[0], guid.Data4[1], guid.Data4[2], guid.Data4[3],
		guid.Data4[4], guid.Data4[5], guid.Data4[6], guid.Data4[7]);
}
//初始化客户端
void init_client();
//清理客户端
void distory_client();
//客户端用户标识
char* get_user_token();
//设置命令头
void set_command_head(PNetCommand arg, NET_COMMAND cmd);
//客户端命令处理
DWORD WINAPI client_cmd(LPVOID);
//客户端异步消息订阅
DWORD WINAPI client_sub(LPVOID);
//客户端异步消息订阅
DWORD WINAPI quote_sub(LPVOID arg);
//客户端消息泵
DWORD WINAPI client_message_pump(LPVOID);
//网络监控
DWORD WINAPI zmq_monitor(LPVOID arg);
//行情独立处理
DWORD WINAPI quote_pump(LPVOID arg);

//行情订阅
DWORD quote_notify_sub(const char *address);
//命令调用
void request_net_cmmmand(NetCommandArgPtr& arg);
//命令调用
inline void request_net_cmmmand(PNetCommand arg, void*)
{
	NetCommandArgPtr ptr(arg);
	request_net_cmmmand(ptr);
}
//命令调用
inline void request_net_cmmmand(PNetCommand arg)
{
	NetCommandArgPtr ptr(arg);
	request_net_cmmmand(ptr);
}
#endif
#ifdef INNER_SERVER
//初始化客户端
void init_client();
//清理客户端
void distory_client();
//代理订阅
void proxy_sub();
//代理命令
void proxy_cmd();
//代理消息
void proxy_message_pump();
//设置命令头
void set_command_head(PNetCommand arg, NET_COMMAND cmd);

//命令调用
void request_net_cmmmand(NetCommandArgPtr& arg);
//命令调用
inline void request_net_cmmmand(PNetCommand arg, void*)
{
	NetCommandArgPtr ptr(arg);
	request_net_cmmmand(ptr);
}
//命令调用
inline void request_net_cmmmand(PNetCommand arg)
{
	NetCommandArgPtr ptr(arg);
	request_net_cmmmand(ptr);
}
#endif
#ifdef INNER_SETTLEMENT
//行情订阅
int quote_sub();
//行情订阅
DWORD quote_notify_sub(const char *address);
//行情订阅
void quote_pump();
//发送数据变更到交易服务
template<class T>
inline void send_change_to_trade(T data)
{
	PNetCommand cmd_arg = SerializeToCommand(&data);
	set_command_head(cmd_arg, NET_COMMAND_DATA_CHANGED);
	request_net_cmmmand(cmd_arg);
}
#endif
//通知消息泵
void notify_message_pump();
//ZMQ上下文对象
ZMQ_HANDLE get_zmq_context();
//运行状态
NET_STATE get_net_state();
//写入CRC校验码
void write_crc(PNetCommand cmd);
//校验CRC校验码
bool check_crc(PNetCommand cmd);

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


#endif