#include <boost/lexical_cast.hpp>
#include "mylogger.h"
#include "../stdafx.h"
using namespace std;

//void myprintf(string msg,const char *fm, va_list ap)
//{
//	const char* fmt = fm;
//	char *s;
//	char flag;
//	while (*fmt) {
//		flag = *fmt++;
//		if (flag != '%') {
//			msg += boost::lexical_cast<string>(flag);
//			continue;
//		}
//		flag = *fmt++;//记得后移一位
//		switch (flag)
//		{
//		case 's':
//			s = va_arg(ap, char*);
//			msg.append(s);
//			break;
//		case 'd': /* int */
//			msg += boost::lexical_cast<string>(va_arg(ap, int));
//			break;
//		case 'f': /* double*/
//			msg += boost::lexical_cast<string>(va_arg(ap, double));
//			break;
//		case 'c': /* char*/
//		default:
//			msg += boost::lexical_cast<string>(va_arg(ap, char));
//			break;
//		}
//	}
//}

namespace agebull
{

	///C端命令调用队列锁
	boost::mutex server_cmd_mutex;

	/**
	* \bref 初始化日志
	*/
	acl::string log_init()
	{
		auto log = json_config::root_path;
		log.append("logs/zero_center.log");
		acl::acl_cpp_init();
		logger_open(log, "zero_center", DEBUG_CONFIG);
		return log;
	}

	/**
	* 输出到DEBUG窗口
	*/
	void out_debug(tm now,const char* msg)
	{
		//		msg = "\r\n" + msg;
		//#ifdef WIN32
		//		OutputDebugStringA(msg);
		//#endif
		char time_c[256];
		time2string(now, time_c);
		{
			boost::lock_guard<boost::mutex> guard(server_cmd_mutex);
			cout << endl << "[" << time_c << "] " << msg;
		}
	}
	void log_acl_msg(const char* msg)
	{
		tm now;
		date_now(now);
		out_debug(now, msg);
		acl::log::msg1(msg);
	}
	void log_acl_warn(const char* fname, int line, const char* func, const char* msg)
	{
		tm now;
		date_now(now);
		out_debug(now, msg);
		acl::log::warn4(fname, line, func, msg);
	}
	void log_acl_error(const char* fname, int line, const char* func, const char* msg)
	{
		tm now;
		date_now(now);
		out_debug(now, msg);
		acl::log::error4(fname, line, func, msg);
	}
	void log_acl_fatal(const char* fname, int line, const char* func, const char* msg)
	{
		tm now;
		date_now(now);
		out_debug(now, msg);
		acl::log::fatal4(fname, line, func, msg);
	}
	void log_acl_debug(int section, int  level, const char* fname, int line, const char* func, const char* msg)
	{
		if (level < 2)
		{
			tm now;
			date_now(now);
			out_debug(now, msg);
		}
		acl::log::msg6(section, level, fname, line, func, msg);
	}
	void log_acl_trace(int section, int  level, const char* msg)
	{
		if (level < 2)
		{
			tm now;
			date_now(now);
			out_debug(now, msg);
		}
		acl::log::msg1(msg);
	}
}