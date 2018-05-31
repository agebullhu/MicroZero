#include "config.h"

namespace agebull
{
	std::map<std::string, std::string> config::global_cfg_;
	/**
	* \brief 系统根目录
	*/
	acl::string config::root_path;

	/**
	* \brief 全局配置初始化
	*/
	void config::init()
	{
		acl::string path;
		path.format("%sconfig/zero_center.json", root_path.c_str());

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
		}
		else
		{
			global_cfg_.insert(make_pair("base_tcp_port", "7999"));
			global_cfg_.insert(make_pair("use_ipc_protocol", "true"));
			global_cfg_.insert(make_pair("redis_addr", "127.0.0.1:6379"));
			global_cfg_.insert(make_pair("redis_defdb", "15"));
		}
	}
	/**
	* \brief 读取配置内容
	*/
	void config::read(const char* str, std::map<std::string, std::string>& cfg)
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
	config::config(const char* json)
	{
		read(acl::string(json), value_map_);
	}
	/**
	* \brief 取全局配置
	* \param name 名称
	* \return 值
	*/
	std::string& config::get_global_string(const char * name)
	{
		
		return global_cfg_[name];
	}

	/**
	* \brief 取全局配置
	* \param name 名称
	* \return 值
	*/
	int config::get_global_int(const char * name)
	{
		
		auto vl = global_cfg_[name];
		return vl.empty() ? 0 : atoi(vl.c_str());
	}
	/**
	* \brief 取全局配置
	* \param name 名称
	* \return 值
	*/
	bool config::get_global_bool(const char * name)
	{
		
		auto vl = global_cfg_[name];
		return !vl.empty() && strcasecmp(vl.c_str(), "true") == 0;
	}
	/**
	* \brief 取配置
	* \param name 名称
	* \return 布尔
	*/
	bool config::boolean(const char * name)
	{
		
		auto vl = value_map_[name];
		return !vl.empty() && strcasecmp(vl.c_str(), "true") == 0;
	}
	/**
	* \brief 取配置
	* \param name 名称
	* \return 数字
	*/
	int config::number(const char * name)
	{
		
		auto vl = value_map_[name];
		return vl.empty() ? 0 : atoi(vl.c_str());
	}
	/**
	* \brief 取配置
	* \param name 名称
	* \return 文本
	*/
	std::string& config::operator[](const char * name)
	{
		return value_map_[name];
	}

}