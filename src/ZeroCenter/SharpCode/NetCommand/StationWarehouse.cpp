#include "stdafx.h"
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


		/**
		 * \brief 活动实例集合
		 */
		map<string, zero_station*> station_warehouse::examples_;

		/**
		* \brief 实例队列访问锁
		*/
		boost::mutex station_warehouse::mutex_;

		/**
		* \brief 还原服务
		*/
		bool station_warehouse::restore(acl::string& value)
		{
			config cfg(value);
			int type = cfg.number("station_type");
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
		* \brief 还原服务
		*/
		int station_warehouse::restore()
		{
			//assert(examples.size() == 0);
			char pattern[] = "net:host:*";
			int cnt = 0;
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_STATION);
			{
				trans_redis& redis = trans_redis::get_context();
				int cursor = 0;
				do
				{
					size_t count = 10;
					vector<acl::string> keys;
					cursor = redis->scan(cursor, keys, pattern, &count);
					for (acl::string key : keys)
					{
						acl::string val;
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
		* \brief 清除所有服务
		*/
		void station_warehouse::clear()
		{
			assert(examples_.empty());
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_STATION);
			{
				trans_redis& redis = trans_redis::get_context();
				redis->flushdb();
				redis->incrby(port_redis_key, config::get_int("baseHost"));
			}
			install(STATION_TYPE_DISPATCHER, "SystemManage");
			install(STATION_TYPE_MONITOR, "SystemMonitor");
		}

		/**
		* \brief 初始化服务
		*/
		acl::string station_warehouse::install(int station_type, const char* station_name)
		{
			char* key;
			{
				boost::format fmt("net:host:%1%");
				fmt % station_name;
				key = _strdup(fmt.str().c_str());
			}

			redis_live_scope redis_live_scope(REDIS_DB_ZERO_STATION);
			acl::string val;
			if (trans_redis::get_context()->get(key, val) && !val.empty())
			{
				return val;
			}
			assert(trans_redis::get_context()->exists(port_redis_key));
			acl::json json;
			acl::json_node& node = json.create_node();
			node.add_text("station_name", station_name);
			node.add_number("station_type", station_type);
			long long port;
			{
				trans_redis::get_context()->incr(port_redis_key, &port);
				boost::format fmt1("tcp://*:%1%");
				fmt1 % port;
				node.add_number("out_port", port);
				node.add_text("out_addr", fmt1.str().c_str());
			}
			if (station_type >= STATION_TYPE_MONITOR)
			{
				trans_redis::get_context()->incr(port_redis_key, &port);
				boost::format fmt1("tcp://*:%1%");
				fmt1 % port;
				node.add_number("inner_port", port);
				node.add_text("inner_addr", fmt1.str().c_str());
			}
			if (station_type > STATION_TYPE_MONITOR)
			{
				trans_redis::get_context()->incr(port_redis_key, &port);
				boost::format fmt1("tcp://*:%1%");
				fmt1 % port;
				node.add_number("heart_port", port);
				node.add_text("heart_addr", fmt1.str().c_str());
			}
			val = node.to_string();
			trans_redis::get_context()->set(key, val);
			monitor(station_name, "station_install", val.c_str());
			delete[] key;
			return val;
		}

		/**
		* \brief 加入服务
		*/
		bool station_warehouse::join(zero_station* station)
		{
			{
				boost::lock_guard<boost::mutex> guard(mutex_);
				if (examples_.find(station->_station_name) != examples_.end())
					return false;
				examples_.insert(make_pair(station->_station_name, station));
			}
			station->_config = install(station->_station_type, station->_station_name.c_str());
			config cfg(station->_config);
			station->_inner_port = cfg.number("inner_port");
			station->_out_port = cfg.number("out_port");
			station->_heart_port = cfg.number("heart_port");

			monitor(station->_station_name, "station_join", station->_config.c_str());
			return true;
		}

		/**
		* \brief 退出服务
		*/
		bool station_warehouse::left(zero_station* station)
		{
			{
				boost::lock_guard<boost::mutex> guard(mutex_);
				auto iter = examples_.find(station->_station_name);
				if (iter == examples_.end() || iter->second != station)
					return false;
				examples_.erase(station->_station_name);
			}
			monitor(station->_station_name, "station_left", "");

			return true;
		}

		/**
		* \brief 加入服务
		*/
		zero_station* station_warehouse::find(const string& name)
		{
			auto iter = examples_.find(name);
			if (iter == examples_.end())
				return nullptr;
			return iter->second;
		}

	}
}
