#pragma once
#ifndef AGEBULL_CONFIG_H
#include "../stdinc.h"
namespace agebull
{
	struct config_item
	{
	private:
		static config_item empty;
	public:
		std::string name;
		std::string value;
		bool is_value;
		/**
		* \brief 配置内容
		*/
		std::map<std::string, config_item> value_map;
		config_item() : is_value(false)
		{
		}

		/**
		* \brief 取配置
		* \param name 名称
		 * \param def 缺省值
		* \return 文本
		*/
		const char* str(const char * name, const char* def = nullptr);
		/**
		* \brief 取配置
		* \param name 名称
		 * \param def 缺省值
		* \return 数字
		*/
		int number(const char * name, int def = 0);
		/**
		* \brief 取配置
		* \param name 名称
		 * \param def 缺省值
		* \return 布尔
		*/
		bool boolean(const char * name, bool def = false);
		/**
		* \brief 取配置
		* \param name 名称
		* \return 子节点
		*/
		config_item& item(const char * name);
		/**
		* \brief 取配置
		* \param name 名称
		* \return 子节点
		*/
		config_item& operator[](const char * name);
	};
	struct global_config :public config_item
	{
		/**
		* \brief 系统根目录
		*/
		static char root_path[512];
		static int base_tcp_port;
		static int plan_exec_timeout;
		static int plan_cache_size;
		//static bool use_ipc_protocol;
		static char redis_addr[512];
		static int redis_defdb;
		static int worker_sound_ivl;
		static int IMMEDIATE;
		static int LINGER;
		static int RCVHWM;
		static int RCVBUF;
		static int RCVTIMEO;
		static int SNDHWM;
		static int SNDBUF;
		static int SNDTIMEO;
		static int BACKLOG;
		static int MAX_SOCKETS;
		static int IO_THREADS;
		static int MAX_MSGSZ;
		static int RECONNECT_IVL;
		static int CONNECT_TIMEOUT;
		static int RECONNECT_IVL_MAX;
		static int TCP_KEEPALIVE;
		static int TCP_KEEPALIVE_IDLE;
		static int TCP_KEEPALIVE_INTVL;
		static int HEARTBEAT_IVL;
		static int HEARTBEAT_TIMEOUT;
		static int HEARTBEAT_TTL;
		static char service_key[512];
	};
	class json_config
	{
	public:
		/**
		 * \brief 全局配置
		 */
		static global_config global;
		/**
		* \brief 全局配置初始化
		*/
		static void init();

		/**
		* \brief 全局配置初始化
		*/
		static void read();

		/**
		* \brief 读取配置内容
		*/
		static void read(acl::string& json, config_item& root);
		/**
		* \brief 读取配置内容
		*/
		static void read(acl::json_node* json, config_item& item);
		/**
		* \brief 读取配置内容
		*/
		static bool load_file(const char* file_name, config_item& root);
	};
	/**
	* \brief 大小写敏感的文本匹配，返回匹配的下标
	* \param dests 目标
	* \param src 比较源
	* \return 目标的第一个算0，或小于0表示未找到
	*/
	template<int N>
	int strmatch(const char *src, const char*  (&dests)[N])
	{
		if (src == nullptr || src[0] == 0)
			return -1;
		for (int i = 0; i < N; i++)
		{
			const char * dest = dests[i];
			int idx = 0;
			for (; dest[idx] != 0 && src[idx] != 0; idx++)
			{
				if (dest[idx] == src[idx])
					continue;
				idx = -1;
				break;
			}
			if (idx >= 0 && dest[idx] == 0 && src[idx] == 0)
				return i;
		}
		return -1;
	}


	/**
	* \brief 大小写不敏感的文本匹配，返回匹配的下标
	* \param dests 目标
	* \param src 比较源
	* \return 目标的第一个算0，或小于0表示未找到
	*/
	template<int N>
	int strmatchi(const char *src, const char*  (&dests)[N])
	{
		if (src == nullptr || src[0] == 0)
			return -1;
		for (int i = 0; i < N; i++)
		{
			const char * dest = dests[i];
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
				return i;
		}
		return -1;
	}
	inline int64 json_read_num(acl::json_node* json_node, int64 def = 0LL)
	{
		auto num = json_node->get_int64();
		return num == nullptr ? def : *num;
	}
	inline int json_read_int(acl::json_node* json_node, int def = 0)
	{
		auto num = json_node->get_int64();
		return num == nullptr ? def : static_cast<int>(*num);
	}
	inline int64 json_read_bool(acl::json_node* json_node, bool def = false)
	{
		auto num = json_node->get_bool();
		return num == nullptr ? def : *num;
	}

#define json_add_str(node,key,text) if(!text.empty())  node.add_text(key, text.c_str())

#define json_add_num(node,key,num) if(num)  node.add_number(key, num)

#define json_add_bool(node,key,num) if(num)  node.add_bool(key, true)

#define json_add_array_str(node,text) if(!text.empty()) node.add_array_text(text.c_str())

}
#endif