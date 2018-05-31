#pragma once
#ifndef AGEBULL_CONFIG_H
#include "../stdinc.h"
namespace agebull
{
	class config
	{
		/**
		 * \brief 全局配置
		 */
		static std::map<std::string, std::string> global_cfg_;

	public:
		/**
		* \brief 全局配置初始化
		*/
		static void init();

		/**
		* \brief 系统根目录
		*/
		static acl::string root_path;

		/**
		 * \brief 取全局配置
		 * \param name 名称
		 * \return 值
		 */
		static int get_global_int(const char * name);
		/**
		* \brief 取全局配置
		* \param name 名称
		* \return 值
		*/
		static bool get_global_bool(const char * name);
		/**
		* \brief 取全局配置
		* \param name 名称
		* \return 文本
		*/
		static std::string& get_global_string(const char * name);
	private:
		/**
		* \brief 配置内容
		*/
		std::map<std::string, std::string> value_map_;
		/**
		* \brief 读取配置内容
		*/
		static void read(const char* json, std::map<std::string, std::string>& cfg);
	public:
		/**
		* \brief 构造
		* \param json JSON内容
		*/
		explicit config(const char* json);

		/**
		* \brief 取配置
		* \param name 名称
		* \return 数字
		*/
		int number(const char * name);
		/**
		* \brief 取配置
		* \param name 名称
		* \return 布尔
		*/
		bool boolean(const char * name);
		/**
		* \brief 取配置
		* \param name 名称
		* \return 文本
		*/
		std::string& operator[](const char * name);
	};

	/**
	* \brief 大小写敏感的文本匹配，返回匹配的下标（目标的第一个算1，或小等于0表示未找到）
	* \param cnt 参数总数
	* \remark 第一个（0下标）为比较源，其它的为目标
	* \return 目标的第一个算0，或小于0表示未找到
	*/
	inline int strmatch(int cnt, ...)
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
				idx = -1;
				break;
			}
			if (idx >= 0 && dest[idx] == 0 && src[idx] == 0)
				return i - 1;
		}
		va_end(ap);
		return -1;
	}


	/**
	 * \brief 大小写不敏感的文本匹配，返回匹配的下标（目标的第一个算1，或小等于0表示未找到）
	 * \param cnt 参数总数
	 * \param ...
	 * \remark 第一个（0下标）为比较源，其它的为目标
	 * \return 目标的第一个算0，或小于0表示未找到
	 */
	inline int strmatchi(int cnt, ...)
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
				idx = -1;
				break;
			}
			if (idx >= 0 && dest[idx] == 0 && src[idx] == 0)
				return i - 1;
		}
		va_end(ap);
		return -1;
	}
}
#endif