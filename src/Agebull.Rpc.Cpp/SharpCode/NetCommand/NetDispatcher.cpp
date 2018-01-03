/**
 * ZMQ广播代理类
 *
 *
 */

#include "stdafx.h"
#include "NetDispatcher.h"
#include "ApiStation.h"
#include "VoteStation.h"
#include "BroadcastingStation.h"

namespace agebull
{
	namespace zmq_net
	{

		map<string, NetStation*> StationWarehouse::examples;

		/**
#define STATION_TYPE_API 1
#define STATION_TYPE_VOTE 2
#define STATION_TYPE_PUBLISH 3
		* @brief 还原服务
		*/
		bool StationWarehouse::restore(acl::string& value)
		{
			config cfg(value);
			int type = cfg.number("station_type");
			switch (type)
			{
			case STATION_TYPE_API:
				ApiStation::run(cfg["station_name"]);
				return true;
			case STATION_TYPE_VOTE:
				VoteStation::run(cfg["station_name"]);
				return true;
			case STATION_TYPE_PUBLISH:
				BroadcastingStation::run(cfg["station_name"]);
				return true;
			default:
				return false;
			}
		}
#define port_redis_key "net:port:next"
		/**
		* @brief 还原服务
		*/
		int StationWarehouse::restore()
		{
			assert(examples.size() == 0);
			char* pattern = "net:host:*";
			int cnt = 0;
			RedisLiveScope redis_live_scope;
			{
				RedisDbScope db_scope(REDIS_DB_NET_STATION);
				TransRedis& redis = TransRedis::get_context();
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
		* @brief 清除所有服务
		*/
		void StationWarehouse::clear()
		{
			assert(examples.size() == 0);
			RedisLiveScope redis_live_scope;
			{
				RedisDbScope db_scope(REDIS_DB_NET_STATION);
				TransRedis& redis = TransRedis::get_context();
				redis->flushdb();
				redis->incrby(port_redis_key, 20180L);
				install(STATION_TYPE_DISPATCHER, "SystemManage");
				install(STATION_TYPE_MONITOR, "SystemMonitor");
			}
		}
		/**
		* @brief 初始化服务
		*/
		acl::string StationWarehouse::install(int station_type, const char* station_name)
		{
			char* key;
			{
				boost::format fmt("net:host:%1%");
				fmt % station_name;
				key = _strdup(fmt.str().c_str());
			}

			RedisLiveScope redis_live_scope;
			RedisDbScope db_scope(REDIS_DB_NET_STATION);
			acl::string val;
			if (TransRedis::get_context()->get(key, val) && !val.empty())
			{
				return val;
			}
			assert(TransRedis::get_context()->exists(port_redis_key));
			acl::json json;
			acl::json_node& node = json.create_node();
			node.add_text("station_name", station_name);
			node.add_number("station_type", station_type);
			long long port;
			{
				TransRedis::get_context()->incr(port_redis_key, &port);
				boost::format fmt1("tcp://*:%1%");
				fmt1 % port;
				node.add_number("out_port", port);
				node.add_text("out_addr", fmt1.str().c_str());
			}
			if (station_type >= STATION_TYPE_MONITOR)
			{
				TransRedis::get_context()->incr(port_redis_key, &port);
				boost::format fmt1("tcp://*:%1%");
				fmt1 % port;
				node.add_number("inner_port", port);
				node.add_text("inner_addr", fmt1.str().c_str());
			}
			if(station_type > STATION_TYPE_MONITOR)
			{
				TransRedis::get_context()->incr(port_redis_key, &port);
				boost::format fmt1("tcp://*:%1%");
				fmt1 % port;
				node.add_number("heart_port", port);
				node.add_text("heart_addr", fmt1.str().c_str());
			}
			val = node.to_string();
			TransRedis::get_context()->set(key, val);
			SystemMonitorStation::monitor(station_name, "station_install", val.c_str());
			delete[] key;
			return val;
		}
		/**
		* @brief 加入服务
		*/
		bool StationWarehouse::join(NetStation* station)
		{
			if (examples.find(station->_station_name) != examples.end())
				return false;

			station->_config = install(station->_station_type, station->_station_name.c_str());
			config cfg(station->_config);
			station->_inner_address = cfg["inner_addr"];
			station->_out_address = cfg["out_addr"];
			station->_heart_address = cfg["heart_addr"];
			examples.insert(make_pair(station->_station_name, station));

			SystemMonitorStation::monitor(station->_station_name, "station_join", station->_config.c_str());
			return true;
		}
		/**
		* @brief 退出服务
		*/
		bool StationWarehouse::left(NetStation* station)
		{
			auto iter = examples.find(station->_station_name);
			if (iter == examples.end() || iter->second != station)
				return false;
			examples.erase(station->_station_name);
			SystemMonitorStation::monitor(station->_station_name, "station_left", "");
			return true;
		}
		/**
		* @brief 加入服务
		*/
		NetStation* StationWarehouse::find(string name)
		{
			auto iter = examples.find(name);
			if (iter == examples.end())
				return nullptr;
			return iter->second;
		}

		NetDispatcher* NetDispatcher::example = nullptr;



		/**
		* 当远程调用进入时的处理
		*/
		string NetDispatcher::pause_station(string arg)
		{
			NetStation* station = StationWarehouse::find(arg);
			if (station == nullptr)
			{
				return"NoFind";
			}
			SystemMonitorStation::monitor(arg, "station_pause", station->get_config());
			return station->pause(true) ? "Ok" : "Failed";
		}

		/**
		* @brief 继续站点
		*/
		string NetDispatcher::resume_station(string arg)
		{
			NetStation* station = StationWarehouse::find(arg);
			if (station == nullptr)
			{
				return ("NoFind");
			}
			SystemMonitorStation::monitor(arg, "station_resume", station->get_config());
			return station->resume(true) ? "Ok" : "Failed";
		}
		/**
		* 当远程调用进入时的处理
		*/
		string NetDispatcher::start_station(string stattion)
		{
			NetStation* station = StationWarehouse::find(stattion);
			if (station != nullptr)
			{
				return ("IsRuning");
			}
			char* key;
			{
				boost::format fmt("net:host:%1%");
				fmt % stattion;
				key = _strdup(fmt.str().c_str());
			}
			RedisLiveScope redis_live_scope;
			RedisDbScope db_scope(REDIS_DB_NET_STATION);
			acl::string val;
			if (TransRedis::get_context()->get(key, val) && !val.empty())
			{
				return StationWarehouse::restore(val) ? "Ok" : "Failed";
			}
			return "Failed";
		}
		/**
		* 当远程调用进入时的处理
		*/
		string NetDispatcher::install_station(int type,string stattion)
		{
			auto cfg =StationWarehouse::install(type, stattion.c_str());
			return StationWarehouse::restore(cfg) ? "Ok" : "Failed";
		}

		/**
		* 当远程调用进入时的处理
		*/
		string NetDispatcher::close_station(string stattion)
		{
			NetStation* station = StationWarehouse::find(stattion);
			if (station == nullptr)
			{
				return ("NoFind");
			}
			return station->close(true) ? "Ok" : "Failed";
		}

		/**
		* @brief 取机器信息
		*/
		string NetDispatcher::host_info(string arg)
		{
			NetStation* station = StationWarehouse::find(arg);
			if (station == nullptr)
			{
				return"NoFind";
			}
			SystemMonitorStation::monitor(arg, "station_address", station->get_config());
			return (station->get_out_address());
		}
		/**
		* @brief 执行命令
		*/
		string NetDispatcher::exec_command(const char* command, const  char* argument)
		{
			if (_stricmp(command, "pause") == 0)
			{
				return	pause_station(string(argument));
			}
			if (_stricmp(command, "resume") == 0)
			{
				return	resume_station(string(argument));
			}
			if (_stricmp(command, "close") == 0)
			{
				return	close_station(string(argument));
			}
			if (_stricmp(command, "start") == 0)
			{
				return	start_station(string(argument));
			}
			if (_stricmp(command, "host") == 0)
			{
				return	host_info(string(argument));
			}
			if (_stricmp(command, "install_api") == 0)
			{
				return install_station(STATION_TYPE_API,string(argument));
			}
			if (_stricmp(command, "exit") == 0)
			{
				SystemMonitorStation::monitor("*", "exit", "");
				boost::thread(boost::bind(distory_net_command));
				return "OK";
			}
			return ("NoSupper");
		}

		/**
		* 当远程调用进入时的处理
		*/
		void NetDispatcher::request()
		{
			char* command = s_recv(_out_socket);
			recv_empty(_out_socket);
			char* argument = s_recv(_out_socket);

			string result = exec_command(command, argument);
			s_send(_out_socket, result.c_str());

			free(command);
			free(argument);
		}
	}
}
