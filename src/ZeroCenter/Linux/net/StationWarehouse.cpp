#include "../stdafx.h"
#include "BroadcastingStation.h"
#include "ApiStation.h"
#include "VoteStation.h"

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
		* \brief 实例队列访问锁
		*/
		boost::mutex station_warehouse::mutex_;

		/**
		* \brief 还原站点
		*/
		bool station_warehouse::restore(acl::string& value)
		{
			config cfg(value);
			const int type = cfg.number("station_type");
			switch (type)
			{
			case STATION_TYPE_API:
				api_station::run(cfg["station_name"]);
				return true;
			case STATION_TYPE_VOTE:
				vote_station::run(cfg["station_name"]);
				return true;
			case STATION_TYPE_PUBLISH:
				broadcasting_station::run(cfg["station_name"]);
				return true;
			default:
				return false;
			}
		}
		
		/**
		* \brief 还原站点
		*/
		int station_warehouse::restore()
		{
			//assert(examples.size() == 0);
			int cnt = 0;
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_STATION);
			{
				trans_redis& redis = trans_redis::get_context();
				int cursor = 0;
				do
				{
					size_t count = 10;
					vector<acl::string> keys;
					cursor = redis->scan(cursor, keys, "net:host:*", &count);
					for (const acl::string& key : keys)
					{
						acl::string val(128);
						if (redis->get(key, val))
						{
							if (restore(val))
								cnt++;
						}
					}
				} while (cursor > 0);
			}
			return cnt;
		}

		/**
		* \brief 清除所有站点
		*/
		void station_warehouse::clear()
		{
			//assert(examples_.empty());
			{
				redis_live_scope redis_live_scope(REDIS_DB_ZERO_STATION);
				trans_redis& redis = trans_redis::get_context();
				redis->flushdb();
				redis.set(port_redis_key, config::get_config(port_config_key).c_str());
			}
			acl::string config;
			install("SystemManage", STATION_TYPE_DISPATCHER, config);
			install("SystemMonitor", STATION_TYPE_MONITOR, config);
		}

		/**
		* \brief 站点卸载
		*/
		bool station_warehouse::unstall_station(const string& name)
		{
			auto station = instance(name);
			if (station == nullptr)
				return false;
			station->close(true);
			boost::format fmt("net:host:%1%");
			fmt % name;
			auto key = fmt.str();
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_STATION);
			trans_redis& redis = trans_redis::get_context();

			return redis->del(key.c_str()) > 0;
		}
		/**
		* \brief 初始化站点
		*/
		bool station_warehouse::install(const char* station_name, int station_type, acl::string& config)
		{
			boost::format fmt("net:host:%1%");
			fmt % station_name;
			auto key = fmt.str();
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_STATION);
			trans_redis& redis = trans_redis::get_context();
			
			if (redis->get(key.c_str(), config) && !config.empty())
			{
				return false;
			}
			acl::json json;
			acl::json_node& node = json.create_node();
			node.add_text("station_name", station_name);
			node.add_number("station_type", station_type);
			long long port;
			{
				redis->incr(port_redis_key, &port);
				boost::format fmt1("tcp://*:%1%");
				fmt1 % port;
				node.add_number("out_port", port);
				node.add_text("out_addr", fmt1.str().c_str());
			}
			if (station_type >= STATION_TYPE_MONITOR)
			{
				redis->incr(port_redis_key, &port);
				boost::format fmt1("tcp://*:%1%");
				fmt1 % port;
				node.add_number("inner_port", port);
				node.add_text("inner_addr", fmt1.str().c_str());
			}
			if (station_type > STATION_TYPE_MONITOR)
			{
				redis->incr(port_redis_key, &port);
				boost::format fmt1("tcp://*:%1%");
				fmt1 % port;
				node.add_number("heart_port", port);
				node.add_text("heart_addr", fmt1.str().c_str());
			}
			config = node.to_string();
			redis->set(key.c_str(), config);
			monitor_async(station_name, "station_install", config.c_str());
			return true;
		}

		/**
		* \brief 加入站点
		*/
		bool station_warehouse::join(zero_station* station)
		{
			{
				boost::lock_guard<boost::mutex> guard(mutex_);
				if (examples_.find(station->station_name_) != examples_.end())
					return false;
				examples_.insert(make_pair(station->station_name_, station));
			}
			install(station->station_name_.c_str(), station->station_type_, station->config_);
			config cfg(station->config_);
			station->response_port_ = cfg.number("inner_port");
			station->request_port_ = cfg.number("out_port");
			station->heart_port_ = cfg.number("heart_port");

			monitor_async(station->station_name_, "station_join", station->config_.c_str());
			return true;
		}

		/**
		* \brief 退出站点
		*/
		bool station_warehouse::left(zero_station* station)
		{
			{
				boost::lock_guard<boost::mutex> guard(mutex_);
				const auto iter = examples_.find(station->station_name_);
				if (iter == examples_.end() || iter->second != station)
					return false;
				examples_.erase(station->station_name_);
			}
			monitor_async(station->station_name_, "station_left", station->station_name_);

			return true;
		}

		/**
		* \brief 查找已运行站点
		*/
		zero_station* station_warehouse::instance(const string& name)
		{
			const auto iter = examples_.find(name);
			if (iter == examples_.end())
				return nullptr;
			return iter->second;
		}

	}
}
