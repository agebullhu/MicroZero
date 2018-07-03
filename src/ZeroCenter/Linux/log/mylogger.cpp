#include "mylogger.h"
#include "../stdafx.h"
#include <boost/date_time.hpp>
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
	void out_debug(boost::posix_time::ptime now, const char* msg)
	{
		acl::string strTime;
		strTime.format("[%s] %s\n", to_iso_extended_string(now).c_str(), msg);
		{
			//boost::lock_guard<boost::mutex> guard(server_cmd_mutex);
			cout << strTime.c_str();
		}
	}
	void log_acl_msg(const char* msg)
	{
		out_debug(boost::posix_time::microsec_clock::local_time(), msg);
		acl::log::msg1(msg);
	}
	void log_acl_warn(const char* fname, int line, const char* func, const char* msg)
	{
		out_debug(boost::posix_time::microsec_clock::local_time(), msg);
		acl::log::warn4(fname, line, func, msg);
	}
	void log_acl_error(const char* fname, int line, const char* func, const char* msg)
	{
		out_debug(boost::posix_time::microsec_clock::local_time(), msg);
		acl::log::error4(fname, line, func, msg);
	}
	void log_acl_fatal(const char* fname, int line, const char* func, const char* msg)
	{
		out_debug(boost::posix_time::microsec_clock::local_time(), msg);
		acl::log::fatal4(fname, line, func, msg);
	}
	void log_acl_debug(int section, int  level, const char* fname, int line, const char* func, const char* msg)
	{
		if (level < 2)
		{
			out_debug(boost::posix_time::microsec_clock::local_time(), msg);
		}
		acl::log::msg6(section, level, fname, line, func, msg);
	}
	void log_acl_trace(int section, int  level, const char* msg)
	{
		if (level < 2)
		{
			out_debug(boost::posix_time::microsec_clock::local_time(), msg);
		}
		acl::log::msg1(msg);
	}
}