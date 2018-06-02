// stdafx.cpp : 只包括标准包含文件的源文件
// StreamTest.pch 将作为预编译头
// stdafx.obj 将包含预编译类型信息

#include "stdafx.h"
#include "service.h"
#include <netdb.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <signal.h>
#include <time.h>
#include <execinfo.h>
#include <string>
#include "net/station_warehouse.h"


namespace agebull
{
	namespace zmq_net
	{
		const int MAX_STACK_FRAMES = 128;

		/**
		* \brief 初始化
		*/
		bool rpc_service::initialize()
		{
			//系统信号发生回调绑定
			for (int sig = SIGHUP; sig < SIGSYS; sig++)
				signal(sig, agebull::zmq_net::on_sig);
			//ØMQ版本号
			int major, minor, patch;
			zmq_version(&major, &minor, &patch);
			// 初始化配置
			char buf[512];
			acl::string curpath = getcwd(buf, 512);
			var list = curpath.split("/");
			list.pop_back();
			config::root_path = "/";
			for (var& word : list)
			{
				config::root_path.append(word);
				config::root_path.append("/");
			}
			config::init();
			// 初始化日志
			var log = log_init();
			log_msg3("ØMQ version:%d.%d.%d", major, minor, patch);
			log_msg3("folder exec:%s\n    root:%s\n    log:%s", curpath.c_str(), config::root_path.c_str(), log.c_str());
			//本机IP信息
			acl::string host;
			vector<acl::string> ips;
			get_local_ips(host, ips);
			acl::string ip_info;
			ip_info.format_append("host:%s ips:", host.c_str());
			bool first = true;
			for (var& ip : ips)
			{
				if (first)
					first = false;
				else
					ip_info.append(",");
				ip_info.append(ip);
			}
			log_msg(ip_info);
			//REDIS环境检查
			trans_redis::redis_ip = config::get_global_string("redis_addr");
			trans_redis::redis_db = config::get_global_int("redis_defdb");
			if (!ping_redis())
			{
				log_error2("redis failed!\n   addr:%s default db:%d", trans_redis::redis_ip.c_str(), trans_redis::redis_db);
				return false;
			}
			log_msg2("redis addr:%s default db:%d", trans_redis::redis_ip.c_str(), trans_redis::redis_db);
			//站点仓库管理初始化
			return station_warehouse::initialize();
		}

		/**
		* \brief sig对应的文本
		*/
		const char* sig_text(int sig)
		{
			switch (sig)
			{
			case SIGABRT: return "由调用abort函数产生，进程非正常退出";
			case SIGKILL: return "无法处理和忽略。中止某个进程";
			case SIGTERM: return "请求中止进程，kill命令缺省发送";
			case SIGINT: return "由Interrupt Key产生，通常是CTRL + C或者DELETE。发送给所有ForeGround Group的进程";

			case SIGALRM: return "用alarm函数设置的timer超时或setitimer函数设置的interval timer超时";
			case SIGBUS: return "某种特定的硬件异常，通常由内存访问引起";
				//case SIGCANCEL: return "由Solaris Thread Library内部使用，通常不会使用";
			case SIGCHLD: return "进程Terminate或Stop的时候，SIGCHLD会发送给它的父进程。缺省情况下该Signal会被忽略";
			case SIGCONT: return "当被stop的进程恢复运行的时候，自动发送";
				//case SIGEMT: return "和实现相关的硬件异常
			case SIGFPE: return "数学相关的异常，如被0除，浮点溢出，等等";
				//case SIGFREEZE: return "Solaris专用，Hiberate或者Suspended时候发送
			case SIGHUP: return "发送给具有Terminal的Controlling Process，当terminal被disconnect时候发送";
			case SIGILL: return "非法指令异常";
				//case SIGINFO: return "BSD signal。由Status Key产生，通常是CTRL + T。发送给所有Foreground Group的进程
			case SIGIO: return "异步IO事件";
				//case SIGIOT: return "实现相关的硬件异常，一般对应SIGABRT";
					//case SIGLWP: return "由Solaris Thread Libray内部使用
			case SIGPIPE: return "在reader中止之后写Pipe的时候发送";
				//case SIGPOLL: return "当某个事件发送给Pollable Device的时候发送";
			case SIGPROF: return "Setitimer指定的Profiling Interval Timer所产生";
			case SIGPWR: return "和系统相关。和UPS相关。";
			case SIGQUIT: return "输入Quit Key的时候（CTRL + \\）发送给所有Foreground Group的进程";
			case SIGSEGV: return "非法内存访问";
			case SIGSTKFLT: return "Linux专用，数学协处理器的栈异常";
			case SIGSTOP: return "中止进程。无法处理和忽略。";
			case SIGSYS: return "非法系统调用";
				//case SIGTHAW: return "Solaris专用，从Suspend恢复时候发送";
			case SIGTRAP: return "实现相关的硬件异常。一般是调试异常";
			case SIGTSTP: return "Suspend Key，一般是Ctrl + Z。发送给所有Foreground Group的进程";
			case SIGTTIN: return "当Background Group的进程尝试读取Terminal的时候发送";
			case SIGTTOU: return "当Background Group的进程尝试写Terminal的时候发送";
			case SIGURG: return "当out - of - band data接收的时候可能发送";
			case SIGUSR1: return "用户自定义signal 1";
			case SIGUSR2: return "用户自定义signal 2";
			case SIGVTALRM: return "setitimer函数设置的Virtual Interval Timer超时的时候";
				//case SIGWAITING: return "Solaris Thread Library内部实现专用";
			case SIGWINCH: return "当Terminal的窗口大小改变的时候，发送给Foreground Group的所有进程";
			case SIGXCPU: return "当CPU时间限制超时的时候";
			case SIGXFSZ: return "进程超过文件大小限制";
				//case SIGXRES: return "Solaris专用，进程超过资源限制的时候发送
			default:
				return "未知中止原因";

			}

		}
		/**
		* \brief 系统信号处理
		*/
		void on_sig(int sig) {
			sig_crash(sig);
			switch (sig)
			{
			case SIGABRT://由调用abort函数产生，进程非正常退出
			case SIGKILL://无法处理和忽略。中止某个进程
			case SIGTERM://请求中止进程，kill命令缺省发送
			case SIGINT://由Interrupt Key产生，通常是CTRL + C或者DELETE。发送给所有ForeGround Group的进程
				boost::thread(boost::bind(rpc_service::stop));
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

		/**
		* \brief 记录堆栈信息
		*/
		void sig_crash(int sig)
		{
			try
			{
				time_t t = time(nullptr);
				tm* now = localtime(&t);
				log_error8(
					"#########################################################\n[%04d-%02d-%02d %02d:%02d:%02d][crash signal number:%d]\n[%s]",
					now->tm_year + 1900,
					now->tm_mon + 1,
					now->tm_mday,
					now->tm_hour,
					now->tm_min,
					now->tm_sec,
					sig, sig_text(sig));
#ifdef __linux
				signal(sig, SIG_DFL);
				void* array[MAX_STACK_FRAMES];
				char** strings = nullptr;
				var size = backtrace(array, MAX_STACK_FRAMES);
				strings = (char**)backtrace_symbols(array, size);
				for (int i = 0; i < size; ++i)
				{
					log_error2("%d %s", i, strings[i]);
				}
				free(strings);
#endif // __linux
			}
			catch (...)
			{
				log_error("exception");
			}
		}

		/**
		* \brief 取本机IP
		*/
		void get_local_ips(acl::string& host, vector<acl::string>& ips)
		{
			char hname[128];
			memset(hname, 0, sizeof(hname));
			gethostname(hname, sizeof(hname));
			host = hname;
			struct addrinfo hint {};
			memset(&hint, 0, sizeof(hint));
			hint.ai_family = AF_INET;
			hint.ai_socktype = SOCK_STREAM;

			addrinfo* info = nullptr;
			char ipstr[16];
			if (getaddrinfo(hname, nullptr, &hint, &info) == 0 && info != nullptr)
			{
				addrinfo* now = info;
				do
				{
					inet_ntop(AF_INET, &(reinterpret_cast<struct sockaddr_in *>(now->ai_addr)->sin_addr), ipstr, 16);
					ips.emplace_back(ipstr);
					now = now->ai_next;
				} while (now != nullptr);
				freeaddrinfo(info);
			}
		}

		/**
		* \brief 获取指定进程所对应的可执行（EXE）文件全路径
		* \param sFilePath - 进程句柄hProcess所对应的可执行文件路径
		* /
		void get_process_file_path(string& sFilePath)
		{
		#if WIN32

		char tsFileDosPath[MAX_PATH + 1];
		ZeroMemory(tsFileDosPath, sizeof(char)*(MAX_PATH + 1));

		HANDLE hProcess = GetCurrentProcess();
		DWORD re = GetProcessImageFileNameA(hProcess, tsFileDosPath, MAX_PATH + 1);
		CloseHandle(hProcess);
		if (0 == re)
		{
		return;
		}

		// 获取Logic Drive String长度
		UINT uiLen = GetLogicalDriveStrings(0, nullptr);
		if (0 == uiLen)
		{
		return;
		}

		char* pLogicDriveString = new char[uiLen + 1];
		ZeroMemory(pLogicDriveString, uiLen + 1);
		uiLen = GetLogicalDriveStringsA(uiLen, pLogicDriveString);
		if (0 == uiLen)
		{
		delete[]pLogicDriveString;
		return;
		}

		char szDrive[3] = " :";
		char* pDosDriveName = new char[MAX_PATH];
		char* pLogicIndex = pLogicDriveString;

		do
		{
		szDrive[0] = *pLogicIndex;
		uiLen = QueryDosDeviceA(szDrive, pDosDriveName, MAX_PATH);
		if (0 == uiLen)
		{
		if (ERROR_INSUFFICIENT_BUFFER != GetLastError())
		{
		break;
		}

		delete[]pDosDriveName;
		pDosDriveName = new char[uiLen + 1];
		uiLen = QueryDosDeviceA(szDrive, pDosDriveName, uiLen + 1);
		if (0 == uiLen)
		{
		break;
		}
		}

		uiLen = strlen(pDosDriveName);
		if (0 == _strnicmp(tsFileDosPath, pDosDriveName, uiLen))
		{
		sFilePath.append(szDrive);
		sFilePath.append(tsFileDosPath + uiLen);
		break;
		}

		while (*pLogicIndex++);
		} while (*pLogicIndex);

		delete[]pLogicDriveString;
		delete[]pDosDriveName;
		#else
		char buf[512];
		getcwd(buf, 512);
		sFilePath = buf;
		#endif
		}*/

	}
}