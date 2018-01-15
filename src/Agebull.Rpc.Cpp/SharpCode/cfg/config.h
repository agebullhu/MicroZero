#pragma once
#ifndef AGEBULL_CONFIG_H
#include <stdinc.h>
class config
{
	static std::map<std::string, std::string> m_machine_cfg;
	static void init();
public:
	static int get_int(const char * name);
	static std::string& get_config(const char * name);
private:
	std::map<std::string, std::string> m_cfg;
	static void read(acl::string& str, std::map<std::string, std::string>& cfg);
public:
	config(const char* json);
	int number(const char * name);
	bool boolean(const char * name);
	std::string& operator[](const char * name);
};

/* 功  能：获取指定进程所对应的可执行（EXE）文件全路径
* 参  数：hProcess - 进程句柄。必须具有PROCESS_QUERY_INFORMATION 或者
PROCESS_QUERY_LIMITED_INFORMATION 权限
*         sFilePath - 进程句柄hProcess所对应的可执行文件路径
* 返回值：
*/
void GetProcessFilePath(OUT string& sFilePath);


/**
*@brief 文本比较，返回匹配的下标或-1
*/
inline int strcmpi_array(int cnt, ...)
{
	va_list ap;
	va_start(ap, cnt);
	const char * src = va_arg(ap, const char *); //读取可变参数，的二个参数为可变参数的类型
	for (int i = 1; i < cnt; i++)
	{
		const char * dest = va_arg(ap, const char *); //读取可变参数，的二个参数为可变参数的类型
		int idx = 0;
		for (; dest[idx] != 0 && src[idx] != 0; idx++)
		{
			if (dest[idx] == src[idx])
				continue;
			if (dest[idx] >= 'a' && dest[idx] <= 'z' && dest[idx] - 32 == src[idx])
			{
				continue;
			}
			if (dest[idx] >= 'A' && dest[idx] <= 'Z' && dest[idx] + 32 == src[idx])
			{
				continue;
			}
			break;
		}
		if (dest[idx] == 0 && src[idx] == 0)
			return i - 1;
	}
	va_end(ap);
	return -1;
}
#endif