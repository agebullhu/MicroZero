#include "../stdafx.h"
#include "broadcasting_station.h"
#include "api_station.h"
//#include "vote_station.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 端口自动分配的Redis键名
		*/
#define port_redis_key "net:port:next"
#define port_config_key "base_tcp_port"

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
		int64_t station_warehouse::glogal_id_ = 0LL;

		/**
		* \brief 还原站点
		*/
		bool station_warehouse::restore(shared_ptr<zero_config>& config)
		{
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
			redis_live_scope redis(REDIS_DB_ZERO_STATION);
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
						if (redis->get(key, val))
						{
							shared_ptr<zero_config> config(new zero_config());
							config->read_json(val);
							insert_config(config, false);
						}
					}
				} while (cursor > 0);
			}
			else
			{
				redis->set(port_redis_key, json_config::get_global_string(port_config_key).c_str());
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
				redis_live_scope redis(REDIS_DB_ZERO_STATION);
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
		char station_warehouse::host_info(const string& stattion, string& json)
		{
			boost::lock_guard<boost::mutex> guard(config_mutex_);
			if (stattion == "*")
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
					json += config.second->to_json(2).c_str();
				}
				json += "]";
				host_json = json;
				return ZERO_STATUS_OK_ID;
			}
			const auto iter = configs_.find(stattion);
			if (iter == configs_.end())
			{
				return ZERO_STATUS_NOT_FIND_ID;
			}
			json = iter->second->to_json(2).c_str();
			return ZERO_STATUS_OK_ID;
		}

		/**
		* \brief 保存配置
		*/
		zero_config& station_warehouse::insert_config(shared_ptr<zero_config>& config, bool save)
		{
			boost::lock_guard<boost::mutex> guard(config_mutex_);
			auto iter = configs_.find(config->station_name_);
			if (iter == configs_.end())
			{
				configs_.insert(make_pair(config->station_name_, config));
				if (!save)
					return *config;
				boost::format fmt("net:host:%1%");
				fmt % config->station_name_;
				redis_live_scope redis(REDIS_DB_ZERO_STATION);
				redis->set(fmt.str().c_str(), config->to_json(2));
				return *config;
			}
			config.swap(iter->second);
			return *iter->second;
		}

		/**
		* \brief 保存配置
		*/
		void station_warehouse::save_configs()
		{
			boost::lock_guard<boost::mutex> guard(config_mutex_);
			redis_live_scope redis(REDIS_DB_ZERO_STATION);
			for (auto& iter : configs_)
			{
				boost::format fmt("net:host:%1%");
				fmt % iter.first;
				redis->set(fmt.str().c_str(), iter.second->to_json(2));
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
			redis_live_scope redis(REDIS_DB_ZERO_STATION);
			acl::string json;
			if (redis->get(key.c_str(), json) && !json.empty())
			{
				shared_ptr<zero_config> config(new zero_config());
				config.reset();
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
		* \brief 站点卸载
		*/
		bool station_warehouse::uninstall(const string& station_name)
		{
			shared_ptr<zero_config> config = get_config(station_name);
			if (config->station_type_ <= STATION_TYPE_DISPATCHER || config->station_type_ >= STATION_TYPE_SPECIAL)
				return false;
			auto station = instance(station_name);
			if (station != nullptr)
			{
				station->close(true);
				station->config_->station_state_ = station_state::Uninstall;
			}
			{
				boost::lock_guard<boost::mutex> guard(config_mutex_);
				configs_.erase(station_name);
			}
			{
				redis_live_scope redis(REDIS_DB_ZERO_STATION);
				char key[256];
				sprintf(key, "net:host:%s", station_name.c_str());
				redis->del(key);
			}
			zero_event_async(station_name, zero_net_event::event_station_uninstall, "");
			return true;
		}
		/**
		* \brief 初始化站点
		*/
		bool station_warehouse::install(const char* station_name, int station_type, const char* short_name)
		{
			shared_ptr<zero_config> config = get_config(station_name);
			if (config != nullptr)
				return false;
			config.reset(new zero_config);
			boost::format fmt("net:host:%1%");
			fmt % station_name;
			auto key = fmt.str();
			redis_live_scope redis(REDIS_DB_ZERO_STATION);
			acl::string json;
			if (redis->get(key.c_str(), json) && !json.empty())
			{
				config->read_json(json);
				insert_config(config, false);
				return false;
			}

			config->station_name_ = station_name;
			config->short_name = short_name;
			config->station_type_ = station_type;
			int64 port;
			redis->incr(port_redis_key, &port);
			config->request_port_ = static_cast<int>(port);
			redis->incr(port_redis_key, &port);
			config->worker_out_port_ = static_cast<int>(port);
			if (station_type >= STATION_TYPE_API)
			{
				redis->incr(port_redis_key, &port);
				config->worker_in_port_ = static_cast<int>(port);
			}
			json = config->to_json(2);
			redis->set(key.c_str(), json);
			insert_config(config, false);
			zero_event_async(station_name, zero_net_event::event_station_install, json.c_str());
			config->check_type_name();
			return true;
		}

		/**
		* \brief 加入站点
		*/
		bool station_warehouse::join(zero_station* station)
		{
			{
				boost::lock_guard<boost::mutex> guard(examples_mutex_);
				if (examples_.find(station->config_->station_name_) != examples_.end())
					return false;
				station->config_->station_state_ = station_state::Run;
				examples_.insert(make_pair(station->config_->station_name_, station));
			}
			zero_event_async(station->config_->station_name_, zero_net_event::event_station_join, string(station->config_->to_json(0)));
			return true;
		}

		/**
		* \brief 退出站点
		*/
		bool station_warehouse::left(zero_station* station)
		{
			{
				boost::lock_guard<boost::mutex> guard(examples_mutex_);
				const auto iter = examples_.find(station->config_->station_name_);
				if (iter == examples_.end() || iter->second != station)
					return false;
				station->config_->station_state_ = station_state::Closed;
				examples_.erase(iter);
			}
			zero_event_async(station->config_->station_name_, zero_net_event::event_station_left, "");

			return true;
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
		* \brief 设置关闭
		*/
		void station_warehouse::set_close_info()
		{
			foreach_configs([](shared_ptr<zero_config>& config)
			{
				boost::lock_guard<boost::mutex> guard(config->mutex_);
				config->station_state_ = station_state::Closed;
			});
			save_configs();
		}
	}
}
