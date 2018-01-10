#ifndef _AGEBULL_NET_COMMAND_H
#define _AGEBULL_NET_COMMAND_H
#pragma once
#include "stdinc.h"
#include "command_def.h"

using namespace std;
//初始化网络命令环境
int init_net_command();
//启动网络命令环境
int start_net_command();
//销毁网络命令环境
void distory_net_command();
//关闭网络命令环境
void close_net_command(bool wait=true);
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
//运行状态
void set_net_state(NET_STATE state);
//写入CRC校验码
void write_crc(PNetCommand cmd);
//校验CRC校验码
bool check_crc(PNetCommand cmd);
#endif
