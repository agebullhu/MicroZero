#include "stdinc.h"
#include "net_command.h"
#include "BroadcastingStation.h"
#include "NetDispatcher.h"
using namespace std;

ZMQ_HANDLE net_context;
volatile NET_STATE net_state = NET_STATE_NONE;
//当前启动了多少命令线程
volatile int command_thread_count;
//ZMQ上下文对象
ZMQ_HANDLE get_zmq_context()
{
	return net_context;
}

//登记线程开始
void set_command_thread_start()
{
	command_thread_count++;
	log_msg1("网络处理线程数量%d->启动", command_thread_count);
}
//登记线程关闭
void set_command_thread_end()
{
	command_thread_count--;
	log_msg1("网络处理线程数量%d->关闭", command_thread_count);
}
//运行状态
NET_STATE get_net_state()
{
	return net_state;
}
//运行状态
void set_net_state(NET_STATE state)
{
	net_state = state;
}
//#ifdef COMMANDPROXY
//CommandProxy* proxy = new CommandProxy();
//#endif
//初始化网络命令环境
int init_net_command()
{
	log_msg("初始化网络命令环境...");
	net_state = NET_STATE_NONE;
	net_context = zmq_ctx_new();

	assert(net_context != nullptr);

	zmq_ctx_set(net_context, ZMQ_MAX_SOCKETS, 8192);
	zmq_ctx_set(net_context, ZMQ_IO_THREADS, 16);
	zmq_ctx_set(net_context, ZMQ_MAX_MSGSZ, 8192);


	//boost::thread smp(boost::bind(&server_message_pump));

	thread_sleep(50);
	log_msg("完成网络命令环境初始化");
	return net_state;
}
//启动网络命令环境
int start_net_command()
{
	net_state = NET_STATE_RUNING;

	log_msg("正在启动监听点");
	agebull::zmq_net::SystemMonitorStation::run();
	while (command_thread_count < 1)
	{
		cout << ".";
		thread_sleep(10);
	}
	cout << endl;
	agebull::zmq_net::SystemMonitorStation::monitor("*", "system_start", "*************We come ZeroNet,luck erery day!*************");
	log_msg("正在启动管理点");
	agebull::zmq_net::NetDispatcher::run();
	while (command_thread_count < 2)
	{
		cout << ".";
		thread_sleep(50);
	}
	cout << endl;

	log_msg("正在启动业务站点");
	int cnt = agebull::zmq_net::StationWarehouse::restore() + 2;

	while (command_thread_count < cnt)
	{
		cout << ".";
		thread_sleep(50);
	}
	cout << endl;
	log_msg("完成网络命令环境启动");
	return net_state;
}

//关闭网络命令环境
void close_net_command(bool wait)
{
	log_msg("正在关闭网络命令环境...");
	if (net_state != NET_STATE_RUNING)
		return;
	agebull::zmq_net::SystemMonitorStation::monitor("*", "system_stop", "*************ZeroNet is closed, see you late!*************");
	thread_sleep(10);

	net_state = NET_STATE_CLOSING;
	while (wait && command_thread_count > 1)
		thread_sleep(10);
	net_state = NET_STATE_CLOSED;
	log_msg("网络命令环境已关闭");
}
//销毁网络命令环境
void distory_net_command()
{
	if (net_state < NET_STATE_CLOSING)
		close_net_command();
	net_state = NET_STATE_DISTORY;
	zmq_ctx_shutdown(net_context);
	log_msg("网络命令环境已销毁");
}