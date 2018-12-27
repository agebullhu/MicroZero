#include "../stdafx.h"
#include "api_station.h"
#include "route_api_station.h"
#include "notify_station.h"
#include "queue_station.h"

namespace agebull
{
	namespace zero_net
	{

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
		* \brief 初始化
		*/
		bool station_warehouse::initialize()
		{
			redis_live_scope redis(global_config::redis_defdb);
			acl::string val;
			if (redis->get(zero_def::redis_key::next_port, val) && atol(val.c_str()) >= global_config::base_tcp_port)
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
				port.format_append("%d", global_config::base_tcp_port);
				redis->set(zero_def::redis_key::next_port, port);
				install(zero_def::name::system_manage, zero_def::station_type::dispatcher, "man", "ZeroNet system mamage station.", true);
				install(zero_def::name::proxy_dispatcher, zero_def::station_type::proxy, "proxy", "ZeroNet reverse proxy station.", true);
				install(zero_def::name::plan_dispatcher, zero_def::station_type::plan, "plan", "ZeroNet plan & task mamage station.", true);
				install("RemoteLog", zero_def::station_type::notify, "log", "ZeroNet remote log station.", true);
				install("HealthCenter", zero_def::station_type::notify, "hea", "ZeroNet health center log station.", true);
			}
			return true;
		}

		/**
		* \brief 恢复站点运行
		*/
		int station_warehouse::restore()
		{
			glogal_id_ = 0LL;
			int cnt = 0;
			boost::lock_guard<boost::mutex> guard(config_mutex_);
			for (auto& kv : configs_)
			{
				if (zero_def::station_type::is_sys_station(kv.second->station_type))
					continue;
				if (restore(kv.second))
					cnt++;
			}
			return cnt;
		}

		/**
		* \brief 还原站点
		*/
		bool station_warehouse::restore(shared_ptr<zero_config>& config)
		{
			if (config->is_state(station_state::stop))
				return false;
			switch (config->station_type)
			{
			case zero_def::station_type::api:
				api_station::run(config);
				return true;
			case zero_def::station_type::route_api:
				route_api_station::run(config);
				return true;
				//case STATION_TYPE_VOTE:
				//	vote_station::run(config);
				//	return true;
			case zero_def::station_type::notify:
				notify_station::run(config);
				return true;
			case zero_def::station_type::queue:
				queue_station::run(config);
				return true;
			default:
				return false;
			}
		}

		/**
		* \brief 清除所有站点
		*/
		void station_warehouse::clear()
		{
			glogal_id_ = 0LL;
			{
				redis_live_scope redis(global_config::redis_defdb);
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
					return zero_def::status::ok;
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
				return zero_def::status::ok;
			}
			const auto iter = configs_.find(station_name);
			if (iter == configs_.end())
			{
				return zero_def::status::not_find;
			}
			json = iter->second->to_full_json().c_str();
			return zero_def::status::ok;
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
				assert(configs_.find(config->station_name) == configs_.end());
				configs_[config->station_name] = config;
			}
		}

		/**
		* \brief 保存配置
		*/
		void station_warehouse::save_configs()
		{
			boost::lock_guard<boost::mutex> guard(config_mutex_);
			redis_live_scope redis(global_config::redis_defdb);
			for (auto& iter : configs_)
			{
				boost::format fmt("net:host:%1%");
				fmt % iter.first;
				redis->set(fmt.str().c_str(), iter.second->to_full_json());
			}
		}
		shared_ptr<zero_config> empty_config;
		/**
		* \brief 取得配置
		*/
		shared_ptr<zero_config>& station_warehouse::get_config(const char* station_name, bool find_redis)
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
			redis_live_scope redis(global_config::redis_defdb);
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
		* \brief 保存站点
		*/
		acl::string station_warehouse::save(shared_ptr<zero_config>& config)
		{
			host_json.clear();
			var json = config->to_full_json();
			boost::format fmt("net:host:%1%");
			fmt % config->station_name;
			redis_live_scope redis(global_config::redis_defdb);
			redis->set(fmt.str().c_str(), json);
			return json;
		}
		/**
		* \brief 安装站点
		*/
		bool station_warehouse::install(const char* json_str)
		{
			if (json_str == nullptr || json_str[0] != '{')
				return false;
			shared_ptr<zero_config> config = make_shared<zero_config>();
			config->read_json(json_str);
			if (config->station_name.empty() || !config->is_general())
				return false;
			config->is_base = false;
			return install(config);
		}

		const char* station_types_1[] = { "api", "pub", "vote", "rapi", "queue" };
		enum class station_types_2
		{
			api, pub, vote, rapi, queue
		};

		/**
		* \brief 安装站点
		*/
		bool station_warehouse::install(const char* station_name, const char* type_name, const char* short_name, const char* desc)
		{
			int type;
			int idx = strmatchi(type_name, station_types_1);
			switch (static_cast<station_types_2>(idx))
			{
			case station_types_2::api:
				type = zero_def::station_type::api;
				break;
			case station_types_2::rapi:
				type = zero_def::station_type::route_api;
				break;
			case station_types_2::pub:
				type = zero_def::station_type::notify;
				break;
			case station_types_2::vote:
				type = zero_def::station_type::vote;
				break;
			case station_types_2::queue:
				type = zero_def::station_type::queue;
				break;
			default:
				return false;
			}
			return install(station_name, type, short_name, desc, false);
		}

		/**
		* \brief 安装站点
		*/
		bool station_warehouse::install(shared_ptr<zero_config>& config)
		{
			if (!config || config->station_name.empty())
				return false;
			var old = get_config(config->station_name.c_str());
			if (old)
				return false;
			bool failed = false;
			foreach_configs([&config, &failed](shared_ptr<zero_config>& cfg)
			{
				if (!failed)
				{
					acl::string name = config->short_name.c_str();
					if (name.compare(cfg->station_name.c_str(), false) == 0 ||
						name.compare(cfg->short_name.c_str(), false) == 0)
					{
						failed = true;
					}
					for (auto alia : cfg->alias)
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
			redis_live_scope redis(global_config::redis_defdb);
			int64 port;
			redis->incr(zero_def::redis_key::next_port, &port);
			config->request_port = static_cast<int>(port);
			redis->incr(zero_def::redis_key::next_port, &port);
			config->worker_out_port = static_cast<int>(port);
			switch (config->station_type)
			{
			case zero_def::station_type::api:
			case zero_def::station_type::vote:
			case zero_def::station_type::queue:
			case zero_def::station_type::route_api:
			case zero_def::station_type::proxy:
				redis->incr(zero_def::redis_key::next_port, &port);
				config->worker_in_port = static_cast<int>(port);
			}
			acl::string json = save(config);
			insert_config(config);
			config->log("install");
			zero_event(zero_net_event::event_station_install, "station", config->station_name.c_str(), json.c_str());
			return true;
		}
		/**
		* \brief 站点更新
		*/
		bool station_warehouse::update(const char* str)
		{
			shared_ptr<zero_config> new_cfg = make_shared<zero_config>();
			new_cfg->read_json(str);
			var config = get_config(new_cfg->station_name.c_str());
			if (!config || config->is_base)
				return false;
			if (new_cfg->station_name != config->station_name)
				return false;
			config->station_caption = new_cfg->station_caption;
			config->station_description = new_cfg->station_description;
			if (zero_def::station_type::is_general_station(config->station_type))
			{
				config->alias = new_cfg->alias;
			}
			acl::string json = save(config);
			zero_event(zero_net_event::event_station_update, "station", config->station_name.c_str(), json.c_str());
			config->log("update");
			return true;
		}

		/**
		* \brief 还原已关停站点
		*/
		bool station_warehouse::recover(const char* station_name)
		{
			shared_ptr<zero_config> config = get_config(station_name);
			if (!config || !config->is_state(station_state::stop) || config->is_base)
				return false;
			config->log("recover");
			config->set_state(station_state::none);
			acl::string json = save(config);
			zero_event(zero_net_event::event_station_install, "station", config->station_name.c_str(), json.c_str());
			return true;
		}
		/**
		* \brief 删除已关停站点
		*/
		bool station_warehouse::remove(const char* station_name)
		{
			shared_ptr<zero_config> config = get_config(station_name);
			if (!config || !config->is_state(station_state::stop) || config->is_base)
				return false;
			host_json.clear();
			boost::format fmt("net:host:%1%");
			fmt % config->station_name;
			redis_live_scope redis(global_config::redis_defdb);
			redis->del(fmt.str().c_str());
			config->log("remove");
			var json = config->to_full_json();
			zero_event(zero_net_event::event_station_remove, "station", config->station_name.c_str(), json.c_str());
			configs_.erase(station_name);
			return true;
		}
		/**
		* \brief 站点关停
		*/
		bool station_warehouse::stop(const string& station_name)
		{
			shared_ptr<zero_config> config = get_config(station_name.c_str());
			if (!config || config->is_state(station_state::stop) || config->is_base)
				return false;
			config->set_state(station_state::stop);
			save(config);
			auto station = instance(station_name);
			if (station != nullptr)
			{
				station->close(true);
			}
			zero_event(zero_net_event::event_station_stop, "station", config->station_name.c_str(), nullptr);
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
				if (examples_.find(station->config_->station_name) != examples_.end())
					return false;
				station->config_->runtime_state(station_state::run);
				examples_.insert(make_pair(station->config_->station_name, station));
			}
			acl::string json = station->config_->to_full_json();
			zero_event(zero_net_event::event_station_join, "station", station->config_->station_name.c_str(), json.c_str());
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
				const auto iter = examples_.find(station->config_->station_name);
				if (iter == examples_.end() || iter->second != station)
					return false;
				station->config_->runtime_state(station_state::closed);
				examples_.erase(iter);
			}
			zero_event(zero_net_event::event_station_left, "station", station->config_->station_name.c_str(), "");
			return true;
		}

		/**
		* 当远程调用进入时的处理
		*/
		char station_warehouse::close_station(const string& station_name)
		{
			//if (station_name == "*")
			//{
			//	boost::lock_guard<boost::mutex> guard(examples_mutex_);
			//	for (auto& station : examples_)
			//	{
			//		if (station.second->get_config().is_general())
			//			station.second->close(true);
			//	}
			//	return zero_def::status::OK_ID;
			//}
			zero_station* station = instance(station_name);
			if (station == nullptr || !station->get_config().is_general())
			{
				return zero_def::status::not_support;
			}
			return station->close(true) ? zero_def::status::ok : zero_def::status::failed;
		}

		/**
		* 当远程调用进入时的处理
		*/
		char station_warehouse::pause_station(const string& arg)
		{
			//if (arg == "*")
			//{
			//	boost::lock_guard<boost::mutex> guard(examples_mutex_);
			//	for (map<string, zero_station*>::value_type& station : examples_)
			//	{
			//		if (station.second->get_config().is_general())
			//			station.second->pause(true);
			//	}
			//	return zero_def::status::OK_ID;
			//}
			zero_station* station = instance(arg);
			if (station == nullptr || !station->get_config().is_general())
			{
				return zero_def::status::not_support;
			}
			return station->pause() ? zero_def::status::ok : zero_def::status::failed;
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
					station.second->resume();
				}
				return zero_def::status::ok;
			}
			zero_station* station = instance(arg);
			if (station != nullptr)
				return station->resume() ? zero_def::status::ok : zero_def::status::failed;
			shared_ptr<zero_config> config = get_config(arg.c_str());
			if (!config || config->is_state(station_state::stop))
				return zero_def::status::not_find;
			return restore(config) ? zero_def::status::ok : zero_def::status::failed;
		}

		/**
		* 当远程调用进入时的处理
		*/
		char station_warehouse::start_station(string station_name)
		{
			zero_station* station = instance(station_name);
			if (station != nullptr)
			{
				return zero_def::status::runing;
			}
			shared_ptr<zero_config> config = get_config(station_name.c_str());
			if (config == nullptr)
				return zero_def::status::not_find;
			return restore(config) ? zero_def::status::ok : zero_def::status::failed;
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
		bool station_warehouse::heartbeat(uchar cmd, vector<shared_char>& list)
		{
			auto config = get_config(*list[2], false);
			if (config == nullptr)
				return false;
			switch (cmd)
			{
			case zero_def::command::heart_join:
				config->worker_join(*list[3], *list[4]);
				return true;
			case zero_def::command::heart_ready:
				zero_event(zero_net_event::event_client_join, "station", *list[2], *list[3]);
				config->worker_ready(*list[3]);
				return true;
			case zero_def::command::heart_pitpat:
				config->worker_heartbeat(*list[3]);
				return true;
			case zero_def::command::heart_left:
				zero_event(zero_net_event::event_client_left, "station", *list[2], *list[3]);
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
				boost::lock_guard<boost::mutex> guard(config->mutex);
				config->runtime_state(station_state::destroy);
			});
			save_configs();
		}


		/**
		* \brief 上传文档
		*/
		bool station_warehouse::upload_doc(const char* station_name, shared_char& doc)
		{
			zero_event(zero_net_event::event_station_doc, "doc", station_name, *doc);
			acl::string path;
			path.format("%s/doc/%s.json", global_config::root_path, station_name);
			std::cout << path.c_str() << endl;
			int fid = open(path, O_RDWR | O_CREAT, 00777);
			int re;
			if (fid < 0)
			{
				path.format("%s/doc", global_config::root_path);
				re = mkdir(path, 00777);
				std::cout << path.c_str() << re << endl;
				path.format("%s/doc/%s.json", global_config::root_path, station_name);
				fid = open(path, O_RDWR | O_CREAT, 00777);
			}
			if (fid < 0)
			{
				return false;
			}
			size_t size = write(fid, *doc, doc.size());
			re = ftruncate(fid, static_cast<__off_t>(size));
			re = close(fid);
			return true;
			/*FILE *fp = fopen(path.c_str(),"w");
			if (fp == nullptr)
			{
				path.format("%sdoc", global_config::root_path);
				mkdir(path, 00700);
				path.format("%sdoc/%s.json", global_config::root_path, station_name);
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
			path.format("%s/doc/%s.json", global_config::root_path, station_name);
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
	}
}
