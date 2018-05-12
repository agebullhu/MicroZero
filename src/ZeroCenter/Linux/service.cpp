// stdafx.cpp : 只包括标准包含文件的源文件
// StreamTest.pch 将作为预编译头
// stdafx.obj 将包含预编译类型信息

#include "stdafx.h"
#include "service.h"
namespace agebull
{
	/**
	* \brief 取本机IP并显示在控制台
	*/
	bool rpc_service::get_local_ips()
	{
		char hname[128];
		gethostname(hname, sizeof(hname));
		cout << "Host:" << hname << endl << "IPs:";
		struct addrinfo hint {};
		memset(&hint, 0, sizeof(hint));
		hint.ai_family = AF_INET;
		hint.ai_socktype = SOCK_STREAM;

		addrinfo* info = nullptr;
		char ipstr[16];
		bool first = true;
		if (getaddrinfo(hname, nullptr, &hint, &info) == 0 && info != nullptr)
		{
			addrinfo* now = info;
			do
			{
				inet_ntop(AF_INET, &(reinterpret_cast<struct sockaddr_in *>(now->ai_addr)->sin_addr), ipstr, 16);
				if (first)
					first = false;
				else
					cout << ",";
				cout << ipstr;
				now = now->ai_next;
			} while (now != nullptr);
			freeaddrinfo(info);
		}
		cout << endl;
		return !first;
	}

	/**
	* \brief 系统信号处理
	*/
	void on_sig(int sig) {
		cout << "SIG:" << sig << endl;
		switch (sig)
		{
		case SIGABRT://由调用abort函数产生，进程非正常退出
		case SIGINT://由Interrupt Key产生，通常是CTRL + C或者DELETE。发送给所有ForeGround Group的进程
		case SIGKILL://无法处理和忽略。中止某个进程
		case SIGTERM://请求中止进程，kill命令缺省发送
			boost::thread(boost::bind(agebull::rpc_service::stop));
			break;
			/*
			case SIGALRM://用alarm函数设置的timer超时或setitimer函数设置的interval timer超时
			case SIGBUS://某种特定的硬件异常，通常由内存访问引起
			case SIGCANCEL://由Solaris Thread Library内部使用，通常不会使用
			case SIGCHLD://进程Terminate或Stop的时候，SIGCHLD会发送给它的父进程。缺省情况下该Signal会被忽略
			case SIGCONT://当被stop的进程恢复运行的时候，自动发送
			case SIGEMT://和实现相关的硬件异常
			case SIGFPE://数学相关的异常，如被0除，浮点溢出，等等
			case SIGFREEZE://Solaris专用，Hiberate或者Suspended时候发送
			case SIGHUP://发送给具有Terminal的Controlling Process，当terminal被disconnect时候发送
			case SIGILL://非法指令异常
			case SIGINFO://BSD signal。由Status Key产生，通常是CTRL + T。发送给所有Foreground Group的进程
			case SIGIO://异步IO事件
			case SIGIOT://实现相关的硬件异常，一般对应SIGABRT
			case SIGLWP://由Solaris Thread Libray内部使用
			case SIGPIPE://在reader中止之后写Pipe的时候发送
			case SIGPOLL://当某个事件发送给Pollable Device的时候发送
			case SIGPROF://Setitimer指定的Profiling Interval Timer所产生
			case SIGPWR://和系统相关。和UPS相关。
			case SIGQUIT://输入Quit Key的时候（CTRL + \）发送给所有Foreground Group的进程
			case SIGSEGV://非法内存访问
			case SIGSTKFLT://Linux专用，数学协处理器的栈异常
			case SIGSTOP://中止进程。无法处理和忽略。
			case SIGSYS://非法系统调用
			case SIGTHAW://Solaris专用，从Suspend恢复时候发送
			case SIGTRAP://实现相关的硬件异常。一般是调试异常
			case SIGTSTP://Suspend Key，一般是Ctrl + Z。发送给所有Foreground Group的进程
			case SIGTTIN://当Background Group的进程尝试读取Terminal的时候发送
			case SIGTTOU://当Background Group的进程尝试写Terminal的时候发送
			case SIGURG://当out - of - band data接收的时候可能发送
			case SIGUSR1://用户自定义signal 1
			case SIGUSR2://用户自定义signal 2
			case SIGVTALRM://setitimer函数设置的Virtual Interval Timer超时的时候
			case SIGWAITING://Solaris Thread Library内部实现专用
			case SIGWINCH://当Terminal的窗口大小改变的时候，发送给Foreground Group的所有进程
			case SIGXCPU://当CPU时间限制超时的时候
			case SIGXFSZ://进程超过文件大小限制
			case SIGXRES://Solaris专用，进程超过资源限制的时候发送
			default:
			return;*/

		}
	}
}
