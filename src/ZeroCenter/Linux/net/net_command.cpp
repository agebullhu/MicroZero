#include "../stdafx.h"
#include "net_command.h"
#include "BroadcastingStation.h"
#include "NetDispatcher.h"
using namespace std;
namespace agebull
{
	ZMQ_HANDLE net_context;
	volatile NET_STATE net_state = NET_STATE_NONE;
	//当前启动了多少命令线程
	volatile int zero_thread_count = 0;
	//当前多少线程未正常启动
	volatile int zero_thread_bad = 0;
	//ZMQ上下文对象
	ZMQ_HANDLE get_zmq_context()
	{
		return net_context;
	}

	//线程计数清零
	void reset_command_thread()
	{
		zero_thread_bad = 0;
		zero_thread_count = 0;
	}
	//登记线程开始
	void set_command_thread_start(const char* name)
	{
		zero_thread_count++;
		log_msg2("zero thread join 【%s】(%d)", name, zero_thread_count);
	}
	//登记线程失败
	void set_command_thread_bad(const char* name)
	{
		zero_thread_bad++;
		log_msg2("zero thread bad 【%s】(%d)", name, zero_thread_bad);
	}
	//登记线程关闭
	void set_command_thread_end(const char* name)
	{
		zero_thread_count--;
		log_msg2("zero thread left 【%s】(%d)", name, zero_thread_count);
	}
	//运行状态
	NET_STATE get_net_state()
	{
		return net_state;
	}
	//运行状态
	//void set_net_state(NET_STATE state)
	//{
	//	net_state = state;
	//	log_msg1("zero state changed(%d)", net_state);
	//}
	//#ifdef COMMANDPROXY
	//CommandProxy* proxy = new CommandProxy();
	//#endif
	//初始化网络命令环境
	int config_zero_center()
	{
		log_msg("zero center initiate...");
		net_state = NET_STATE_NONE;
		net_context = zmq_ctx_new();

		assert(net_context != nullptr);

		//zmq_ctx_set(net_context, ZMQ_MAX_SOCKETS, 32767);
		//zmq_ctx_set(net_context, ZMQ_IO_THREADS, 1024);
		//zmq_ctx_set(net_context, ZMQ_MAX_MSGSZ, 32767);


		//boost::thread smp(boost::bind(&server_message_pump));

		//thread_sleep(50);
		log_msg("zero center initiated");
		return net_state;
	}
	//启动网络命令环境
	int start_zero_center()
	{
		log_msg("start system stations ...");
		net_state = NET_STATE_RUNING;

		zmq_net::station_dispatcher::run();
		while (zero_thread_count < 1)
		{
			cout << ".";
			thread_sleep(10);
		}
		cout << endl;

		zmq_net::system_monitor_station::run();
		while (zero_thread_count < 2)
		{
			cout << ".";
			thread_sleep(10);
		}
		cout << endl<<"waiting connect";

		for (int i=0;i<30;i++)
		{
			cout << ".";
			thread_sleep(100);
		}
		cout << endl;
		zmq_net::monitor_sync("*", "system_start", "*************Wecome ZeroNet,luck every day!*************");

		log_msg("start business stations...");
		int cnt = zmq_net::station_warehouse::restore() + 2;
		while (zero_thread_count < cnt)
		{
			cout << ".";
			thread_sleep(50);
		}
		cout << endl;
		log_msg("all station in service");
		return net_state;
	}

	//关闭网络命令环境
	void close_net_command(bool wait)
	{
		if (net_state != NET_STATE_RUNING)
			return;
		log_msg("zero center closing...");
		zmq_net::monitor_async("*", "system_stop", "*************ZeroNet is closed, see you late!*************");
		thread_sleep(10);

		net_state = NET_STATE_CLOSING;
		while (wait && zero_thread_count > 1)
			thread_sleep(10);
		net_state = NET_STATE_CLOSED;
		log_msg("zero center closed");
	}
	//销毁网络命令环境
	void distory_net_command()
	{
		if (net_state < NET_STATE_CLOSING)
			close_net_command();
		net_state = NET_STATE_DISTORY;
		zmq_ctx_shutdown(net_context);
		//zmq_ctx_term(net_context);
		net_context = nullptr;
		log_msg("zero center destoried");
	}
}