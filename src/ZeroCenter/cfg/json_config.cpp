#include "json_config.h"
#include "../log/mylogger.h"
namespace agebull
{
	config_item config_item::empty;
	global_config json_config::global;
	int global_config::base_tcp_port = 7999;
	int global_config::plan_exec_timeout = 300;
	int global_config::plan_cache_size = 1024;
	int global_config::pool_timeout = 1000;
	bool global_config::monitor_socket = false;
	bool global_config::api_route_mode = false;
	bool global_config::trace_net = false;
	//bool global_config::use_ipc_protocol = false;
	char global_config::redis_addr[512] = "127.0.0.1:6379";
	int global_config::redis_defdb = 0x10;
	int global_config::worker_sound_ivl = 2000;
	char global_config::service_key[512] = "agebull";


	int global_config::IMMEDIATE = 1;
	int global_config::LINGER = -1;
	int global_config::SNDHWM = -1;
	int global_config::SNDBUF = -1;
	int global_config::RCVTIMEO = 500;
	int global_config::RCVHWM = -1;
	int global_config::RCVBUF = -1;
	int global_config::SNDTIMEO = 500;
	int global_config::BACKLOG = -1;
	int global_config::MAX_SOCKETS = -1;
	int global_config::IO_THREADS = -1;
	int global_config::MAX_MSGSZ = -1;
	int global_config::RECONNECT_IVL = 100;
	int global_config::CONNECT_TIMEOUT = 3000;
	int global_config::RECONNECT_IVL_MAX = 3000;
	int global_config::TCP_KEEPALIVE = 1;
	int global_config::TCP_KEEPALIVE_IDLE = 4096;
	int global_config::TCP_KEEPALIVE_INTVL = 4096;
	int global_config::HEARTBEAT_IVL = 60000;
	int global_config::HEARTBEAT_TIMEOUT = 1000;
	int global_config::HEARTBEAT_TTL = 60000;
	
	/**
	* \brief 系统根目录
	*/
	char global_config::root_path[512] = "";

	/**
	* \brief 全局配置初始化
	*/
	void json_config::read()
	{
		if (!load_file("zero_center.json", global))
			return;
		auto& zmq = global["ZMQ"];
		if (zmq.value_map.size() > 0)
		{
			global_config::IMMEDIATE = zmq.number("IMMEDIATE", global_config::IMMEDIATE);
			global_config::LINGER = zmq.number("LINGER", global_config::LINGER);
			global_config::RCVHWM = zmq.number("RCVHWM", global_config::RCVHWM);
			global_config::RCVBUF = zmq.number("RCVBUF", global_config::RCVBUF);
			global_config::RCVTIMEO = zmq.number("RCVTIMEO", global_config::RCVTIMEO);
			global_config::SNDHWM = zmq.number("SNDHWM", global_config::SNDHWM);
			global_config::SNDBUF = zmq.number("SNDBUF", global_config::SNDBUF);
			global_config::SNDTIMEO = zmq.number("SNDTIMEO", global_config::SNDTIMEO);
			global_config::BACKLOG = zmq.number("BACKLOG", global_config::BACKLOG);
			global_config::MAX_SOCKETS = zmq.number("MAX_SOCKETS", global_config::MAX_SOCKETS);
			global_config::IO_THREADS = zmq.number("IO_THREADS", global_config::IO_THREADS);
			global_config::MAX_MSGSZ = zmq.number("MAX_MSGSZ", global_config::MAX_MSGSZ);

			global_config::RECONNECT_IVL = zmq.number("RECONNECT_IVL", global_config::RECONNECT_IVL);
			global_config::CONNECT_TIMEOUT = zmq.number("CONNECT_TIMEOUT", global_config::CONNECT_TIMEOUT);
			global_config::RECONNECT_IVL_MAX = zmq.number("RECONNECT_IVL_MAX", global_config::RECONNECT_IVL_MAX);
			global_config::TCP_KEEPALIVE = zmq.number("TCP_KEEPALIVE", global_config::TCP_KEEPALIVE);
			global_config::TCP_KEEPALIVE_IDLE = zmq.number("TCP_KEEPALIVE_IDLE", global_config::TCP_KEEPALIVE_IDLE);
			global_config::TCP_KEEPALIVE_INTVL = zmq.number("TCP_KEEPALIVE_INTVL", global_config::TCP_KEEPALIVE_INTVL);
			global_config::HEARTBEAT_IVL = zmq.number("HEARTBEAT_IVL", global_config::HEARTBEAT_IVL);
			global_config::HEARTBEAT_TIMEOUT = zmq.number("HEARTBEAT_TIMEOUT", global_config::HEARTBEAT_TIMEOUT);
			global_config::HEARTBEAT_TTL = zmq.number("HEARTBEAT_TTL", global_config::HEARTBEAT_TTL);
		}
		auto& zero = global["zero"];
		if (zero.value_map.size() > 0)
		{
			global_config::plan_exec_timeout = zero.number("plan_exec_timeout", global_config::plan_exec_timeout);
			global_config::plan_cache_size = zero.number("plan_cache_size", global_config::plan_cache_size);
			global_config::base_tcp_port = zero.number("base_tcp_port", global_config::base_tcp_port);
			global_config::worker_sound_ivl = zero.number("worker_sound_ivl", global_config::worker_sound_ivl);
			global_config::monitor_socket = zero.boolean("monitor_socket", global_config::monitor_socket);
			global_config::trace_net = zero.boolean("trace_net", global_config::trace_net);
			global_config::api_route_mode = zero.boolean("api_route_mode", global_config::api_route_mode);
			global_config::pool_timeout = zero.number("pool_timeout", global_config::pool_timeout);
			
			var key = zero.str("service_key");
			if (key != nullptr)
				strcpy(global_config::service_key, key);
		}
		auto& redis = global["redis"];
		if (redis.value_map.size() > 0)
		{
			var addr = redis.str("addr");
			if (addr != nullptr)
				strcpy(global_config::redis_addr, addr);
			global_config::redis_defdb = redis.number("defdb", global_config::redis_defdb);
		}
		//use_ipc_protocol = get_global_bool("use_ipc_protocol", use_ipc_protocol);
	}
	/**
	* \brief 全局配置初始化
	*/
	void json_config::init()
	{
		read();
		log_msg1("config => base_tcp_port : %d", global_config::base_tcp_port);
		log_msg1("config => monitor_socket : %d", global_config::monitor_socket);
		log_msg1("config => api_route_mode : %d", global_config::api_route_mode);
		log_msg1("config => pool_timeout : %d", global_config::pool_timeout);
		log_msg1("config => worker_sound_ivl : %d", global_config::worker_sound_ivl);
		//log_msg1("config => use_ipc_protocol : %d", use_ipc_protocol);
		log_msg1("config => trace_net : %d", global_config::trace_net);
		log_msg1("config => plan_exec_timeout : %d", global_config::plan_exec_timeout);
		log_msg1("config => plan_cache_size : %d", global_config::plan_cache_size);

		log_msg1("redis => addr : %s", global_config::redis_addr);
		log_msg1("redis => defdb : %d", global_config::redis_defdb);

		log_msg1("ZMQ => MAX_SOCKETS : %d", global_config::MAX_SOCKETS);
		log_msg1("ZMQ => IO_THREADS : %d", global_config::IO_THREADS);
		log_msg1("ZMQ => MAX_MSGSZ : %d", global_config::MAX_MSGSZ);

		log_msg1("ZMQ => IMMEDIATE : %d", global_config::IMMEDIATE);
		log_msg1("ZMQ => LINGER : %d", global_config::LINGER);
		log_msg1("ZMQ => RCVHWM : %d", global_config::RCVHWM);
		log_msg1("ZMQ => RCVBUF : %d", global_config::RCVBUF);
		log_msg1("ZMQ => RCVTIMEO : %d", global_config::RCVTIMEO);
		log_msg1("ZMQ => SNDHWM : %d", global_config::SNDHWM);
		log_msg1("ZMQ => SNDBUF : %d", global_config::SNDBUF);
		log_msg1("ZMQ => SNDTIMEO : %d", global_config::SNDTIMEO);
		log_msg1("ZMQ => BACKLOG : %d", global_config::BACKLOG);

		log_msg1("ZMQ => TCP_KEEPALIVE : %d", global_config::TCP_KEEPALIVE);
		log_msg1("ZMQ => TCP_KEEPALIVE_IDLE : %d", global_config::TCP_KEEPALIVE_IDLE);
		log_msg1("ZMQ => TCP_KEEPALIVE_INTVL : %d", global_config::TCP_KEEPALIVE_INTVL);

		log_msg1("ZMQ => HEARTBEAT_IVL : %d", global_config::HEARTBEAT_IVL);
		log_msg1("ZMQ => HEARTBEAT_TIMEOUT : %d", global_config::HEARTBEAT_TIMEOUT);
		log_msg1("ZMQ => HEARTBEAT_TTL : %d", global_config::HEARTBEAT_TTL);
	}

	/**
	* \brief 读取配置内容
	*/
	bool json_config::load_file(const char* file_name, config_item& root)
	{
		acl::string path;
		path.format("%s/config/%s", global_config::root_path, file_name);
		std::cout << path.c_str() << endl;
		root.name = path.c_str();
		ACL_VSTREAM *fp = acl_vstream_fopen(path.c_str(), O_RDONLY, 0700, 8192);
		if (fp == nullptr)
			return false;
		acl::string cfg;
		int ret = 0;
		char buf[1024];
		while (ret != ACL_VSTREAM_EOF) {
			ret = acl_vstream_gets_nonl(fp, buf, sizeof(buf));
			cfg += buf;
		}
		acl_vstream_fclose(fp);
		read(cfg, root);
		return true;
	}
	/**
	* \brief 读取配置内容
	*/
	void json_config::read(acl::string& str, config_item& root)
	{
		root.value_map.clear();
		acl::json json;
		json.update(str);
		read(&json.get_root(), root);
	}

	/**
	* \brief 读取配置内容
	*/
	void json_config::read(acl::json_node* json, config_item& par)
	{
		acl::json_node* iter = json->first_child();
		while (iter)
		{
			if (iter->tag_name())
			{
				config_item  item;
				item.name = iter->tag_name();
				var obj = iter->get_obj();
				item.is_value = obj == nullptr;
				if (item.is_value)
				{
					item.value = iter->get_text();
				}
				else
				{
					read(obj, item);
				}
				par.value_map.insert(std::make_pair(item.name, item));
			}
			iter = json->next_child();
		}
	}
	/**
	* \brief 取配置
	* \param name 名称
	* \param def 缺省值
	* \return 布尔
	*/
	bool config_item::boolean(const char * name, bool def)
	{
		auto vl = value_map.find(name);
		return vl == value_map.end() ? def : strcasecmp(vl->second.value.c_str(), "true") == 0;
	}
	/**
	* \brief 取配置
	* \param name 名称
	* \param def 缺省值
	* \return 数字
	*/
	int config_item::number(const char * name, int def)
	{
		auto vl = value_map.find(name);
		return vl == value_map.end() ? def : atoi(vl->second.value.c_str());
	}
	/**
	* \brief 取配置
	* \param name 名称
	* \param def 不存在时的缺省值
	* \return 文本
	*/
	const char* config_item::str(const char * name, const char* def)
	{
		auto vl = value_map.find(name);
		return vl == value_map.end() ? def : vl->second.value.c_str();
	}
	/**
	* \brief 取配置
	* \param name 名称
	* \return 文本
	*/
	config_item& config_item::operator[](const char * name)
	{
		auto vl = value_map.find(name);
		return vl == value_map.end() ? empty : vl->second;
	}
	/**
	* \brief 取配置
	* \param name 名称
	* \return 子节点
	*/
	config_item& config_item::item(const char * name)
	{
		auto vl = value_map.find(name);
		return vl == value_map.end() ? empty : vl->second;
	}
}