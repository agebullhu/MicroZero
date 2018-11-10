#include "json_config.h"
#include "../log/mylogger.h"
namespace agebull
{
	std::map<std::string, std::string> json_config::global_cfg_;
	int json_config::base_tcp_port = 7999;
	int json_config::plan_exec_timeout = 300;
	int json_config::plan_cache_size = 1024;
	//bool json_config::use_ipc_protocol = false;
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
			plan_cache_size = get_global_int("plan_cache_size", plan_cache_size);
			base_tcp_port = get_global_int("base_tcp_port", base_tcp_port);
			//use_ipc_protocol = get_global_bool("use_ipc_protocol", use_ipc_protocol);
			var addr = get_global_string("redis_addr");
			if (addr.length() > 0)
				strcpy(redis_addr, addr.c_str());
			redis_defdb = get_global_int("redis_defdb", redis_defdb);
			worker_sound_ivl = get_global_int("worker_sound_ivl", worker_sound_ivl);
		}
		log_msg1("config => base_tcp_port : %d", base_tcp_port);
		log_msg1("config => worker_sound_ivl : %d", worker_sound_ivl);
		//log_msg1("config => use_ipc_protocol : %d", use_ipc_protocol);
		log_msg1("config => redis_addr : %s", redis_addr);
		log_msg1("config => redis_defdb : %d", redis_defdb);
		log_msg1("config => plan_exec_timeout : %d", plan_exec_timeout);
		log_msg1("config => plan_cache_size : %d", plan_cache_size);

		log_msg1("config => ZMQ_IMMEDIATE : %d", IMMEDIATE);
		log_msg1("config => ZMQ_LINGER : %d", LINGER);
		log_msg1("config => ZMQ_RCVHWM : %d", RCVHWM);
		log_msg1("config => ZMQ_RCVBUF : %d", RCVBUF);
		log_msg1("config => ZMQ_RCVTIMEO : %d", RCVTIMEO);
		log_msg1("config => ZMQ_SNDHWM : %d", SNDHWM);
		log_msg1("config => ZMQ_SNDBUF : %d", SNDBUF);
		log_msg1("config => ZMQ_SNDTIMEO : %d", SNDTIMEO);
		log_msg1("config => ZMQ_BACKLOG : %d", BACKLOG);
		log_msg1("config => ZMQ_MAX_SOCKETS : %d", MAX_SOCKETS);
		log_msg1("config => ZMQ_IO_THREADS : %d", IO_THREADS);
		log_msg1("config => ZMQ_MAX_MSGSZ : %d", MAX_MSGSZ);

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
	* \param def 缺省值
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
	* \param def 缺省值
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
	* \param def 缺省值
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

}