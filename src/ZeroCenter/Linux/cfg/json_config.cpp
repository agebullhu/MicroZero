#include "json_config.h"

namespace agebull
{
	std::map<std::string, std::string> json_config::global_cfg_;
	int json_config::base_tcp_port = 7999;
	int json_config::plan_exec_timeout = 300;
	bool json_config::use_ipc_protocol = false;
	char json_config::redis_addr[512] = "127.0.0.1:6379";
	int json_config::redis_defdb = 0x10;
	int json_config::worker_sound_ivl = 2000;

	int json_config::IMMEDIATE = 1;
	int json_config::LINGER = -1;
	int json_config::SNDHWM = -1;
	int json_config::SNDBUF = -1;
	int json_config::RCVTIMEO = 500;
	int json_config::RCVHWM = -1;
	int json_config::RCVBUF = -1;
	int json_config::SNDTIMEO = 500;
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

			IMMEDIATE = get_global_int("ZMQ_IMMEDIATE", IMMEDIATE);
			LINGER = get_global_int("ZMQ_LINGER", LINGER);
			RCVHWM = get_global_int("ZMQ_RCVHWM", RCVHWM);
			RCVBUF = get_global_int("ZMQ_RCVBUF", RCVBUF);
			RCVTIMEO = get_global_int("ZMQ_RCVTIMEO", RCVTIMEO);
			SNDHWM = get_global_int("ZMQ_SNDHWM", SNDHWM);
			SNDBUF = get_global_int("ZMQ_SNDBUF", SNDBUF);
			SNDTIMEO = get_global_int("ZMQ_SNDTIMEO", SNDTIMEO);
			BACKLOG = get_global_int("ZMQ_BACKLOG", BACKLOG);
			MAX_SOCKETS = get_global_int("ZMQ_MAX_SOCKETS", MAX_SOCKETS);
			IO_THREADS = get_global_int("ZMQ_IO_THREADS", IO_THREADS);
			MAX_MSGSZ = get_global_int("ZMQ_MAX_MSGSZ", MAX_MSGSZ);

			plan_exec_timeout = get_global_int("plan_exec_timeout", plan_exec_timeout);
			base_tcp_port = get_global_int("base_tcp_port", base_tcp_port);
			use_ipc_protocol = get_global_bool("use_ipc_protocol", use_ipc_protocol);
			var addr = get_global_string("redis_addr");
			if (addr.length() > 0)
				strcpy(redis_addr, addr.c_str());
			redis_defdb = get_global_int("redis_defdb", redis_defdb);
			worker_sound_ivl = get_global_int("worker_sound_ivl", worker_sound_ivl);
		}
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
	int json_config::get_global_int(const char * name, int def)
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
	* \param dests 目标
	* \param src 比较源
	* \return 目标的第一个算0，或小于0表示未找到
	*/
	template<int N>
	int strmatch(const char *src, const char* (&dests)[N])
	{
		for (int i = 1; i < N; i++)
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
				return i - 1;
		}
		return -1;
	}


	/**
	* \brief 大小写不敏感的文本匹配，返回匹配的下标（目标的第一个算1，或小等于0表示未找到）
	* \param dests 目标
	* \param src 比较源
	* \return 目标的第一个算0，或小于0表示未找到
	*/
	template<int N>
	int strmatchi(const char *src, const char* (&dests)[N])
	{
		for (int i = 1; i < N; i++)
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
				return i - 1;
		}
		return -1;
	}
}