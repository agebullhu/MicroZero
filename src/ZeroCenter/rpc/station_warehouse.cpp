#include "../stdafx.h"
#include "broadcasting_station.h"
#include "api_station.h"
//#include "vote_station.h"
#include<stdio.h>
#include<string.h>
namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 端口自动分配的Redis键名
		*/
#define port_redis_key "net:port:next"

		/**
		 * \brief 活动实例集合
		 */
		map<string, zero_station*> station_warehouse::examples_;
		/**
		* \brief 配置集合
		*/
		map<string, shared_ptr<zero_config>> station_warehouse::configs_;
		/**
		* \brief 实例队列访问锁
		*/
		boost::mutex station_warehouse::examples_mutex_;
		/**
		* \brief 实例队列访问锁
		*/
		boost::mutex station_warehouse::config_mutex_;

		/**
		* \brief 全局ID
		*/
		int64 station_warehouse::glogal_id_ = 0;
		/**
		* \brief 启动次数
		*/
		int64 station_warehouse::reboot_num_ = 0;


		/**
		* \brief 还原站点
		*/
		bool station_warehouse::restore(shared_ptr<zero_config>& config)
		{
			if (config->is_state(station_state::Uninstall))
				return false;
			switch (config->station_type_)
			{
			case STATION_TYPE_API:
				api_station::run(config);
				return true;
				//case STATION_TYPE_VOTE:
				//	vote_station::run(config);
				//	return true;
			case STATION_TYPE_PUBLISH:
				broadcasting_station::run(config);
				return true;
			default:
				return false;
			}
		}

		/**
		* \brief 初始化
		*/
		bool station_warehouse::initialize()
		{
			redis_live_scope redis(json_config::redis_defdb);
			acl::string val;
			if (redis->get(port_redis_key, val) && atol(val.c_str()) >= json_config::base_tcp_port)
			{
				int cursor = 0;
				do
				{
					size_t count = 10;
					vector<acl::string> keys;
					cursor = redis->scan(cursor, keys, "net:host:*", &count);
					for (const acl::string& key : keys)
					{
						if (redis->get(key, val) && !val.empty())
						{
							shared_ptr<zero_config> config = make_shared<zero_config>();
							config->read_json(val);
							insert_config(config);
						}
					}
				} while (cursor > 0);
			}
			else
			{
				acl::string port;
				port.format_append("%d", json_config::base_tcp_port);
				redis->set(port_redis_key, port);
				install("SystemManage", STATION_TYPE_DISPATCHER, "man");
				install("PlanDispatcher", STATION_TYPE_PLAN, "plan");
				install("RemoteLog", STATION_TYPE_PUBLISH, "log");
				install("HealthCenter", STATION_TYPE_PUBLISH, "hea");
			}
			return true;
		}

		/**
		* \brief 清除所有站点
		*/
		void station_warehouse::clear()
		{
			glogal_id_ = 0LL;
			{
				redis_live_scope redis(json_config::redis_defdb);
				redis->flushdb();
			}
			{
				boost::lock_guard<boost::mutex> guard(config_mutex_);
				configs_.clear();
			}
			initialize();
		}
		/**
		* \brief 遍历配置
		*/
		void station_warehouse::foreach_configs(std::function<void(shared_ptr<zero_config>&)> look)
		{
			boost::lock_guard<boost::mutex> guard(config_mutex_);
			for (auto & config : configs_)
			{
				look(config.second);
			}
		}

		/**
		* \brief 上传文档
		*/
		bool station_warehouse::upload_doc(const char* station_name, shared_char& doc)
		{
			zero_event(zero_net_event::event_station_doc, "doc", station_name, *doc);
			acl::string path;
			path.format("%sdoc/%s.json", json_config::root_path.c_str(), station_name);
			std::cout << path.c_str() << endl;
			int fid = open(path, O_RDWR);
			if (fid >= 0)
			{
				size_t size = write(fid, *doc, doc.size());
				ftruncate(fid, (__off_t)size);
				close(fid);
				return true;
			}
			return false;
			/*FILE *fp = fopen(path.c_str(),"w");
			if (fp == nullptr)
			{
				path.format("%sdoc", json_config::root_path.c_str());
				mkdir(path, 00700);
				path.format("%sdoc/%s.json", json_config::root_path.c_str(), station_name);
				fp = fopen(path.c_str(), "w");
			}
			if (fp == nullptr)
				return false;
			size_t size = fwrite(*doc, sizeof(char), doc.size(), fp);
			fflush(fp);
			fclose(fp);
			return size == doc.size();*/
		}

		/**
		* \brief 获取文档
		*/
		bool station_warehouse::get_doc(const char* station_name, string& doc)
		{
			acl::string path;
			path.format("%sdoc/%s.json", json_config::root_path.c_str(), station_name);
			std::cout << path.c_str() << endl;
			ACL_VSTREAM *fp = acl_vstream_fopen(path.c_str(), O_RDONLY, 0700, 8192);
			if (fp == nullptr)
			{
				return false;
			}
			int ret = 0;
			char buf[1024];
			while (ret != ACL_VSTREAM_EOF) {
				ret = acl_vstream_gets_nonl(fp, buf, sizeof(buf));
				if (ret > 0)
					doc += buf;
			}
			acl_vstream_fclose(fp);
			return true;
		}
		string host_json;
		/**
		* \brief 取机器信息
		*/
		char station_warehouse::host_info(const string& station_name, string& json)
		{
			boost::lock_guard<boost::mutex> guard(config_mutex_);
			if (station_name == "*")
			{
				if (host_json.length() > 0)
				{
					json = host_json;
					return ZERO_STATUS_OK_ID;
				}
				json = "[";
				bool first = true;
				for (auto& config : configs_)
				{
					if (first)
						first = false;
					else
						json += ",";
					json += config.second->to_full_json().c_str();
				}
				json += "]";
				host_json = json;
				return ZERO_STATUS_OK_ID;
			}
			const auto iter = configs_.find(station_name);
			if (iter == configs_.end())
			{
				return ZERO_STATUS_NOT_FIND_ID;
			}
			json = iter->second->to_full_json().c_str();
			return ZERO_STATUS_OK_ID;
		}

		/**
		* \brief 保存配置
		*/
		void station_warehouse::insert_config(shared_ptr<zero_config> config)
		{
			host_json.clear();
			config->check_type_name();
			{
				boost::lock_guard<boost::mutex> guard(config_mutex_);
				assert(configs_.find(config->station_name_) == configs_.end());
				configs_[config->station_name_] = config;
			}
		}

		/**
		* \brief 保存配置
		*/
		void station_warehouse::save_configs()
		{
			boost::lock_guard<boost::mutex> guard(config_mutex_);
			redis_live_scope redis(json_config::redis_defdb);
			for (auto& iter : configs_)
			{
				boost::format fmt("net:host:%1%");
				fmt % iter.first;
				redis->set(fmt.str().c_str(), iter.second->to_info_json());
			}
		}
		shared_ptr<zero_config> empty_config;
		/**
		* \brief 取得配置
		*/
		shared_ptr<zero_config>& station_warehouse::get_config(const string& station_name, bool find_redis)
		{
			boost::lock_guard<boost::mutex> guard(config_mutex_);
			const auto iter = configs_.find(station_name);
			if (iter != configs_.end())
			{
				return iter->second;
			}
			if (!find_redis)
				return empty_config;
			boost::format fmt("net:host:%1%");
			fmt % station_name;
			auto key = fmt.str();
			redis_live_scope redis(json_config::redis_defdb);
			acl::string json;
			if (redis->get(key.c_str(), json) && !json.empty())
			{
				shared_ptr<zero_config> config = make_shared<zero_config>();

				config->read_json(json);
				configs_.insert(make_pair(station_name, config));
				return configs_[station_name];
			}
			return empty_config;
		}
		/**
		* \brief 还原站点
		*/
		int station_warehouse::restore()
		{
			glogal_id_ = 0LL;
			int cnt = 0;
			boost::lock_guard<boost::mutex> guard(config_mutex_);
			for (auto& kv : configs_)
			{
				if (kv.second->station_type_ <= STATION_TYPE_DISPATCHER || kv.second->station_type_ > STATION_TYPE_SPECIAL)
					continue;
				if (restore(kv.second))
					cnt++;
			}
			return cnt;
		}
		/**
		* \brief 安装站点
		*/
		bool station_warehouse::install(const char* json_str)
		{
			if (json_str == nullptr || json_str[0] != '{')
				return false;
			zero_config new_cfg;
			new_cfg.read_json(json_str);
			if (new_cfg.station_name_.empty() || !new_cfg.is_custom_station())
				return false;
			shared_ptr<zero_config> config = get_config(new_cfg.station_name_);
			if (config != nullptr)
			{
				return false;
			}
			config = make_shared<zero_config>();

			config->station_name_ = new_cfg.station_name_;
			config->short_name_ = new_cfg.short_name_;
			config->station_type_ = new_cfg.station_type_;
			config->station_caption_ = new_cfg.station_caption_;
			config->alias_ = new_cfg.alias_;
			config->station_description_ = new_cfg.station_description_;

			return install(config);
		}

		const char* station_types_1[] = { "api", "pub", "vote" };
		enum class station_types_2
		{
			api, pub, vote
		};

		/**
		* 当远程调用进入时的处理
		*/
		char station_warehouse::install_station(const char* station_name, const char* type_name, const char* short_name)
		{
			shared_ptr<zero_config> config = get_config(station_name);
			if (config != nullptr)
			{
				if (!config->is_state(station_state::Uninstall))
					return false;
				config->set_state(station_state::None);
				acl::string json = save(config);
				zero_event(zero_net_event::event_station_update, "station", station_name, json.c_str());
				return true;
			}
			bool success;
			int idx = strmatchi(type_name, station_types_1);
			switch (static_cast<station_types_2>(idx))
			{
			case station_types_2::api:
				success = install(station_name, STATION_TYPE_API, short_name); break;
			case station_types_2::pub:
				success = install(station_name, STATION_TYPE_PUBLISH, short_name); break;
			case station_types_2::vote:
				success = install(station_name, STATION_TYPE_VOTE, short_name); break;
			default:
				return ZERO_STATUS_NOT_SUPPORT_ID;
			}
			return success ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID;
		}
		/**
		* \brief 安装站点
		*/
		bool station_warehouse::install(const char* station_name, int station_type, const char* short_name)
		{
			shared_ptr<zero_config> config = make_shared<zero_config>();
			config->station_name_ = station_name;
			config->short_name_ = short_name;
			config->station_type_ = station_type;

			return install(config);
		}
		/**
		* \brief 安装站点
		*/
		bool station_warehouse::install(shared_ptr<zero_config>& config)
		{
			var old = get_config(config->station_name_);
			if (old)
				return false;
			bool failed = false;
			foreach_configs([&config, &failed](shared_ptr<zero_config>& cfg)
			{
				if (!failed)
				{
					acl::string name = config->short_name_.c_str();
					if (name.compare(cfg->station_name_.c_str(), false) == 0 ||
						name.compare(cfg->short_name_.c_str(), false) == 0)
					{
						failed = true;
					}
					for (auto alia : cfg->alias_)
					{
						if (name.compare(alia.c_str(), false) == 0)
						{
							failed = true;
							break;
						}
					}
				}
			});
			if (failed)
				return false;
			redis_live_scope redis(json_config::redis_defdb);
			int64 port;
			redis->incr(port_redis_key, &port);
			config->request_port_ = static_cast<int>(port);
			redis->incr(port_redis_key, &port);
			config->worker_out_port_ = static_cast<int>(port);
			//API与VOTE有返回值接口
			if (config->station_type_ >= STATION_TYPE_API && config->station_type_ < STATION_TYPE_PLAN)
			{
				redis->incr(port_redis_key, &port);
				config->worker_in_port_ = static_cast<int>(port);
			}
			acl::string json = save(config);
			insert_config(config);
			config->log("install");
			zero_event(zero_net_event::event_station_install, "station", config->station_name_.c_str(), json.c_str());
			return true;
		}
		/**
		* \brief 站点更新
		*/
		bool station_warehouse::update(const char* str)
		{
			shared_ptr<zero_config> new_cfg = make_shared<zero_config>();
			new_cfg->read_json(str);
			var config = get_config(new_cfg->station_name_);
			if (!config)
				return false;
			if (new_cfg->station_name_ != config->station_name_)
				return false;
			config->station_caption_ = new_cfg->station_caption_;
			config->station_description_ = new_cfg->station_description_;
			if (config->station_type_ > STATION_TYPE_DISPATCHER && config->station_type_ < STATION_TYPE_SPECIAL)
			{
				config->alias_ = new_cfg->alias_;
			}
			acl::string json = save(config);
			zero_event(zero_net_event::event_station_update, "station", config->station_name_.c_str(), json.c_str());
			config->log("update");
			return true;
		}

		/**
		* \brief 保存站点
		*/
		acl::string station_warehouse::save(shared_ptr<zero_config>& config)
		{
			host_json.clear();
			var json = config->to_info_json();
			boost::format fmt("net:host:%1%");
			fmt % config->station_name_;
			redis_live_scope redis(json_config::redis_defdb);
			redis->set(fmt.str().c_str(), json);
			return json;
		}
		/**
		* \brief 站点卸载
		*/
		bool station_warehouse::uninstall(const string& station_name)
		{
			shared_ptr<zero_config> config = get_config(station_name);
			if (config->is_state(station_state::Uninstall) ||
				config->station_type_ <= STATION_TYPE_DISPATCHER ||
				config->station_type_ >= STATION_TYPE_SPECIAL)
				return false;
			config->set_state(station_state::Uninstall);
			save(config);
			config->log("uninstall");
			auto station = instance(station_name);
			if (station != nullptr)
			{
				station->close(true);
			}
			zero_event(zero_net_event::event_station_uninstall, "station", config->station_name_.c_str(), nullptr);
			return true;
		}

		/**
		* \brief 加入站点
		*/
		bool station_warehouse::join(zero_station* station)
		{
			station->config_->log("join");
			{
				boost::lock_guard<boost::mutex> guard(examples_mutex_);
				if (examples_.find(station->config_->station_name_) != examples_.end())
					return false;
				station->config_->runtime_state(station_state::Run);
				examples_.insert(make_pair(station->config_->station_name_, station));
			}
			acl::string json = station->config_->to_full_json();
			zero_event(zero_net_event::event_station_join, "station", station->config_->station_name_.c_str(), json.c_str());
			return true;
		}

		/**
		* \brief 退出站点
		*/
		bool station_warehouse::left(zero_station* station)
		{
			station->config_->log("left");
			{
				boost::lock_guard<boost::mutex> guard(examples_mutex_);
				const auto iter = examples_.find(station->config_->station_name_);
				if (iter == examples_.end() || iter->second != station)
					return false;
				station->config_->runtime_state(station_state::Closed);
				examples_.erase(iter);
			}
			zero_event(zero_net_event::event_station_left, "station", station->config_->station_name_.c_str(),"");
			return true;
		}

		/**
		* 当远程调用进入时的处理
		*/
		char station_warehouse::close_station(const string& station_name)
		{
			if (station_name == "*")
			{
				boost::lock_guard<boost::mutex> guard(examples_mutex_);
				for (auto& station : examples_)
				{
					station.second->close(true);
				}
				return ZERO_STATUS_OK_ID;
			}
			zero_station* station = instance(station_name);
			if (station == nullptr)
			{
				return ZERO_STATUS_NOT_FIND_ID;
			}
			return station->close(true) ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID;
		}

		/**
		* 当远程调用进入时的处理
		*/
		char station_warehouse::pause_station(const string& arg)
		{
			if (arg == "*")
			{
				boost::lock_guard<boost::mutex> guard(examples_mutex_);
				for (map<string, zero_station*>::value_type& station : examples_)
				{
					station.second->pause(true);
				}
				return ZERO_STATUS_OK_ID;
			}
			zero_station* station = instance(arg);
			if (station == nullptr)
			{
				return ZERO_STATUS_NOT_FIND_ID;
			}
			return station->pause(true) ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID;
		}

		/**
		* \brief 继续站点
		*/
		char station_warehouse::resume_station(const string& arg)
		{
			if (arg == "*")
			{
				boost::lock_guard<boost::mutex> guard(examples_mutex_);
				for (map<string, zero_station*>::value_type& station : examples_)
				{
					station.second->resume(true);
				}
				return ZERO_STATUS_OK_ID;
			}
			zero_station* station = instance(arg);
			if (station != nullptr)
				return station->resume(true) ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID;
			shared_ptr<zero_config> config = get_config(arg);
			if (config->is_state(station_state::Uninstall))
				return ZERO_STATUS_NOT_FIND_ID;
			return restore(config) ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID;
		}

		/**
		* 当远程调用进入时的处理
		*/
		char station_warehouse::start_station(string station_name)
		{
			zero_station* station = instance(station_name);
			if (station != nullptr)
			{
				return ZERO_STATUS_RUNING_ID;
			}
			shared_ptr<zero_config> config = get_config(station_name);
			if (config == nullptr)
				return ZERO_STATUS_NOT_FIND_ID;
			return restore(config) ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID;
		}
		/**
		* \brief 查找已运行站点
		*/
		zero_station* station_warehouse::instance(const string& name)
		{
			boost::lock_guard<boost::mutex> guard(examples_mutex_);
			const auto iter = examples_.find(name);
			if (iter == examples_.end())
				return nullptr;
			return iter->second;
		}


		/**
		* 心跳的响应
		*/
		bool station_warehouse::heartbeat(char cmd, vector<shared_char> list)
		{
			auto config = get_config(list[2], false);
			if (config == nullptr)
				return false;
			switch (cmd)
			{
			case ZERO_BYTE_COMMAND_HEART_JOIN:
				config->worker_join(*list[3], *list[4]);
				return true;
			case ZERO_BYTE_COMMAND_HEART_READY:
				config->worker_ready(*list[3]);
				return true;
			case ZERO_BYTE_COMMAND_HEART_PITPAT:
				config->worker_heartbeat(*list[3]);
				return true;
			case ZERO_BYTE_COMMAND_HEART_LEFT:
				config->worker_left(*list[3]);
				return true;
			default:
				return false;
			}
		}

		/**
		* \brief 设置关闭
		*/
		void station_warehouse::set_all_destroy()
		{
			foreach_configs([](shared_ptr<zero_config>& config)
			{
				boost::lock_guard<boost::mutex> guard(config->mutex_);
				config->runtime_state(station_state::Destroy);
			});
			save_configs();
		}
	}
}
