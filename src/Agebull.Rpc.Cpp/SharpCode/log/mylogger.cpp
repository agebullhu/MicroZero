#include "mylogger.h"
#include <stdinc.h>
using namespace std;
#ifdef NOCLR
#include <acl.h>
#include <boost/lexical_cast.hpp>
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

void log_acl_msg(const char* msg)
{
	std::cout << std::endl << msg;
	acl::log::msg1(msg);
}
void log_acl_msg(string msg)
{
	std::cout << std::endl << msg;
	acl::log::msg1(msg.c_str());
}
void log_acl_warn(const char* fname, int line, const char* func, string msg)
{
	//std::cout << std::endl << msg;
	acl::log::warn4(fname, line, func, msg.c_str());
}
void log_acl_error(const char* fname, int line, const char* func, string msg)
{
	std::cout << std::endl << msg;
	acl::log::error4(fname, line, func, msg.c_str());
}
void log_acl_fatal(const char* fname, int line, const char* func, string msg)
{
	acl::log::fatal4(fname, line, func, msg.c_str());
}
void log_acl_debug(int section, int  level, const char* fname, int line, const char* func, std::string msg)
{
	if (level < 2)
		std::cout << std::endl << msg;
	acl::log::msg6(section, level, fname, line, func, msg.c_str());
}
void log_acl_trace(int section, int  level, string msg)
{
	if (level < 2)
		std::cout << std::endl << msg;
	acl::log::msg1(msg.c_str());
}
#endif
#ifdef CLIENT_TEST
void log_acl_msg(const char* msg)
{
	std::cout << std::endl << msg;
}
void log_acl_msg(string msg)
{
	std::cout << std::endl << msg;
}
void log_acl_warn(const char* fname, int line, const char* func, string msg)
{
	std::cout << std::endl << msg;
}
void log_acl_error(const char* fname, int line, const char* func, string msg)
{
	std::cout << std::endl << msg;
}
void log_acl_fatal(const char* fname, int line, const char* func, string msg)
{
	std::cout << std::endl << msg;
}
void log_acl_debug(int section, int  level, const char* fname, int line, const char* func, string msg)
{
	if (level < 2)
		std::cout << std::endl << msg;
}
void log_acl_trace(int section, int  level, string msg)
{
	if (level < 2)
		std::cout << std::endl << msg;
}
#endif
#ifdef CLR
FILE* file = nullptr;
void BeginWriteFile()
{
	tm date;
	date_now(date);
	if (file == nullptr)
	{
		fopen_s(&file,"log.log", "a+");

		fprintf(file, "\n************************************************************\n系统启动:[%d-%d-%d %d:%d:%d]\n************************************************************"
			, date.tm_year + 1900
			, date.tm_mon + 1
			, date.tm_mday
			, date.tm_hour
			, date.tm_min
			, date.tm_sec);
	}

	fprintf(file, "\n[%d-%d-%d %d:%d:%d]"
		, date.tm_year + 1900
		, date.tm_mon + 1
		, date.tm_mday
		, date.tm_hour
		, date.tm_min
		, date.tm_sec);
}
void EndWriteFile()
{
	fflush(file);
}

void log_acl_msg(const char* msg)
{
	std::cout << std::endl << msg;
	BeginWriteFile();
	tm date;
	date_now(date);
	fprintf(file, msg);
	EndWriteFile();
}
void log_acl_msg(string msg)
{
	std::cout << std::endl << msg;
	BeginWriteFile();
	fprintf(file, msg.c_str());
	EndWriteFile();
}
void log_acl_warn(const char* fname, int line, const char* func, string msg)
{
	std::cout << std::endl << msg;
	BeginWriteFile();
	fprintf(file, "File:%s Line:%d Function:%s Warning:%s", fname, line, func, msg.c_str());
	EndWriteFile();
}
void log_acl_error(const char* fname, int line, const char* func, string msg)
{
	std::cout << std::endl << msg;
	BeginWriteFile();
	fprintf(file, "File:%s Line:%d Function:%s Fatal:%s", fname, line, func, msg.c_str());
	EndWriteFile();
}
void log_acl_fatal(const char* fname, int line, const char* func, string msg)
{
	std::cout << std::endl << msg;
	BeginWriteFile();
	fprintf(file, "File:%s Line:%d Function:%s Fatal:%s", fname, line, func, msg.c_str());
	EndWriteFile();
}
void log_acl_debug(int section, int  level, const char* fname, int line, const char* func, string msg)
{
	if (level <= 1)
		std::cout << std::endl << msg;
	BeginWriteFile();
	fprintf(file, "File:%s Line:%d Function:%s Section:%d Level:%d Debug:%s", fname, line, func, section, level, msg.c_str());
	EndWriteFile();
}
void log_acl_trace(int section, int  level, string msg)
{
	if (level <= 1)
		std::cout << std::endl << msg;
	BeginWriteFile();
	fprintf(file, "Section:%d Level:%d Trace:%s", section, level, msg.c_str());
	EndWriteFile();
}

#endif