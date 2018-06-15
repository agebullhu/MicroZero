#include "json_config.h"

namespace agebull
{
	std::map<std::string, std::string> json_config::global_cfg_;
	int json_config::base_tcp_port;
	bool json_config::use_ipc_protocol;
	char json_config::redis_addr[512];
	int json_config::redis_defdb;
	int json_config::worker_sound_ivl;

	int json_config::IMMEDIATE = 1;
	int json_config::LINGER = -1;
	int json_config::SNDHWM = -1;
	int json_config::SNDBUF = -1;
	int json_config::RCVTIMEO = -1;
	int json_config::RCVHWM = -1;
	int json_config::RCVBUF = -1;
	int json_config::SNDTIMEO = -1;
	int json_config::BACKLOG = -1;
	int json_config::MAX_SOCKETS = -1;
	int json_config::IO_THREADS = -1;
	int json_config::MAX_MSGSZ = -1;

	/**
	* \brief 系统根目录
	*/
	acl::string json_config::root_path;

	/**
	* \brief 全局配置初始化
	*/
	void json_config::init()
	{
		acl::string path;
		path.format("%sconfig/zero_center.json", root_path.c_str());
		std::cout << path.c_str() << endl;
		ACL_VSTREAM *fp = acl_vstream_fopen(path.c_str(), O_RDONLY, 0700, 8192);
		if (fp != nullptr)
		{
			acl::string cfg;
			int ret = 0;
			char buf[1024];
			while (ret != ACL_VSTREAM_EOF) {
				ret = acl_vstream_gets_nonl(fp, buf, sizeof(buf));
				cfg += buf;
			}
			acl_vstream_fclose(fp);
			read(cfg.c_str(), global_cfg_);

			IMMEDIATE = get_global_int("ZMQ_IMMEDIATE",-1);
			LINGER = get_global_int("ZMQ_LINGER",-1);
			RCVHWM = get_global_int("ZMQ_RCVHWM",-1);
			RCVBUF = get_global_int("ZMQ_RCVBUF",-1);
			RCVTIMEO = get_global_int("ZMQ_RCVTIMEO",-1);
			SNDHWM = get_global_int("ZMQ_SNDHWM",-1);
			SNDBUF = get_global_int("ZMQ_SNDBUF",-1);
			SNDTIMEO = get_global_int("ZMQ_SNDTIMEO",-1);
			BACKLOG = get_global_int("ZMQ_BACKLOG",-1);
			MAX_SOCKETS = get_global_int("ZMQ_MAX_SOCKETS",-1);
			IO_THREADS = get_global_int("ZMQ_IO_THREADS",-1);
			MAX_MSGSZ = get_global_int("ZMQ_MAX_MSGSZ",-1);
		}
		else
		{
			global_cfg_.insert(make_pair("base_tcp_port", "7999"));
			global_cfg_.insert(make_pair("use_ipc_protocol", "false"));
			global_cfg_.insert(make_pair("redis_addr", "127.0.0.1:6379"));
			global_cfg_.insert(make_pair("redis_defdb", "15"));
			global_cfg_.insert(make_pair("worker_sound_ivl", "1000"));

		}
		base_tcp_port = get_global_int("base_tcp_port");
		use_ipc_protocol = get_global_bool("use_ipc_protocol");
		strcpy(redis_addr, get_global_string("redis_addr").c_str());
		redis_defdb = get_global_int("redis_defdb");
		worker_sound_ivl = get_global_int("worker_sound_ivl");
	}
	/**
	* \brief 读取配置内容
	*/
	void json_config::read(const char* str, std::map<std::string, std::string>& cfg)
	{
		cfg.clear();
		acl::json json;
		json.update(str);
		acl::json_node* iter = json.first_node();
		while (iter)
		{
			if (iter->tag_name())
			{
				cfg.insert(std::make_pair(iter->tag_name(), iter->get_text()));
			}
			iter = json.next_node();
		}
	}
	/**
	* \brief 构造
	* \param json JSON内容
	*/
	json_config::json_config(const char* json)
	{
		read(acl::string(json), value_map_);
	}
	/**
	* \brief 取全局配置
	* \param name 名称
	* \return 值
	*/
	std::string& json_config::get_global_string(const char * name)
	{
		return global_cfg_[name];
	}

	/**
	* \brief 取全局配置
	* \param name 名称
	* \param def 缺省值
	* \return 值
	*/
	int json_config::get_global_int(const char * name,int def)
	{
		auto vl = global_cfg_[name];
		return vl.empty() ? def : atoi(vl.c_str());
	}
	/**
	* \brief 取全局配置
	* \param name 名称
	* \return 值
	*/
	bool json_config::get_global_bool(const char * name, bool def)
	{
		auto vl = global_cfg_[name];
		return vl.empty() ? def : strcasecmp(vl.c_str(), "true") == 0;
	}
	/**
	* \brief 取配置
	* \param name 名称
	* \return 布尔
	*/
	bool json_config::boolean(const char * name, bool def)
	{
		auto vl = value_map_[name];
		return vl.empty() ? def : strcasecmp(vl.c_str(), "true") == 0;
	}
	/**
	* \brief 取配置
	* \param name 名称
	* \return 数字
	*/
	int json_config::number(const char * name, int def)
	{
		auto vl = value_map_[name];
		return vl.empty() ? def : atoi(vl.c_str());
	}
	/**
	* \brief 取配置
	* \param name 名称
	* \return 文本
	*/
	std::string& json_config::operator[](const char * name)
	{
		return value_map_[name];
	}

	/**
	* \brief 大小写敏感的文本匹配，返回匹配的下标（目标的第一个算1，或小等于0表示未找到）
	* \param cnt 参数总数
	* \param ... 第一个（0下标）为比较源，其它的为目标
	* \return 目标的第一个算0，或小于0表示未找到
	*/
	int strmatch(int cnt, ...)
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
	* \param ... 第一个（0下标）为比较源，其它的为目标
	* \return 目标的第一个算0，或小于0表示未找到
	*/
	int strmatchi(int cnt, ...)
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