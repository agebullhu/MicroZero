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
		* @brief 实例队列访问锁
		*/
		boost::mutex StationWarehouse::_mutex;
		/**
		* @brief 当前活动的发布类
		*/
		NetDispatcher* NetDispatcher::example = nullptr;


		/**
		*@brief 广播内容
		*/
		bool monitor(string publiher, string state, string content)
		{
			return SystemMonitorStation::monitor(publiher, state, content);
		}


		/**
		* @brief 暂停
		*/
		bool  NetStation::pause(bool waiting)
		{
			if (station_state::Run != _station_state)
				return false;
			_station_state = station_state::Pause;
			monitor(_station_name, "station_pause", "");
			return true;
		}

		/**
		* @brief 继续
		*/
		bool  NetStation::resume(bool waiting)
		{
			if (station_state::Pause != _station_state)
				return false;
			_station_state = station_state::Run;
			_state_semaphore.post();
			monitor(_station_name, "station_resume", "");
			return true;
		}

		/**
		* @brief 结束
		*/
		bool  NetStation::close(bool waiting)
		{
			if (!can_do())
				return false;
			_station_state = station_state::Closing;
			while (waiting && _station_state == station_state::Closing)
				thread_sleep(1000);
			monitor(_station_name, "station_closing", "");
			return true;
		}
		/**
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
			//assert(examples.size() == 0);
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
			if (station_type > STATION_TYPE_MONITOR)
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
			{
				boost::lock_guard<boost::mutex> guard(_mutex);
				if (examples.find(station->_station_name) != examples.end())
					return false;
				examples.insert(make_pair(station->_station_name, station));
			}
			station->_config = install(station->_station_type, station->_station_name.c_str());
			config cfg(station->_config);
			station->_inner_port = cfg.number("inner_port");
			station->_out_port = cfg.number("out_port");
			station->_heart_port = cfg.number("heart_port");

			SystemMonitorStation::monitor(station->_station_name, "station_join", station->_config.c_str());
			return true;
		}
		/**
		* @brief 退出服务
		*/
		bool StationWarehouse::left(NetStation* station)
		{
			{
				boost::lock_guard<boost::mutex> guard(_mutex);
				auto iter = examples.find(station->_station_name);
				if (iter == examples.end() || iter->second != station)
					return false;
				examples.erase(station->_station_name);
			}
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


		/**
		* 当远程调用进入时的处理
		*/
		string NetDispatcher::pause_station(string arg)
		{
			if (arg == "*")
			{
				for (map<string, NetStation*>::value_type station : StationWarehouse::examples)
				{
					station.second->pause(true);
				}
				return "Ok";
			}
			NetStation* station = StationWarehouse::find(arg);
			if (station == nullptr)
			{
				return"NoFind";
			}
			return station->pause(true) ? "Ok" : "Failed";
		}

		/**
		* @brief 继续站点
		*/
		string NetDispatcher::resume_station(string arg)
		{
			if (arg == "*")
			{
				for (map<string, NetStation*>::value_type station : StationWarehouse::examples)
				{
					station.second->resume(true);
				}
				return "Ok";
			}
			NetStation* station = StationWarehouse::find(arg);
			if (station == nullptr)
			{
				return ("NoFind");
			}
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
				return "IsRuning";
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
		string NetDispatcher::install_station(int type, string stattion)
		{
			auto cfg = StationWarehouse::install(type, stattion.c_str());
			return StationWarehouse::restore(cfg) ? "Ok" : "Failed";
		}
		/**
		* @brief 远程调用
		*/
		string  NetDispatcher::call_station(string stattion, string command, string argument)
		{
			NetStation* station = StationWarehouse::find(stattion);
			if (station == nullptr)
			{
				return "unknow stattion";
			}
			vector<string> lines;
			lines.push_back(command);
			lines.push_back(argument);
			auto result= station->command("_sys_", lines);
			return result;
		}

		/**
		* 当远程调用进入时的处理
		*/
		string NetDispatcher::close_station(string stattion)
		{
			if (stattion == "*")
			{
				for (map<string, NetStation*>::value_type station : StationWarehouse::examples)
				{
					station.second->close(true);
				}
				return "Ok";
			}
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
		string NetDispatcher::host_info(string stattion)
		{
			if (stattion == "*")
			{
				string result = "[";
				for (map<string, NetStation*>::value_type station : StationWarehouse::examples)
				{
					result += station.second->get_config();
					result += ",";
				}
				result += "]";
				return result;
			}
			NetStation* station = StationWarehouse::find(stattion);
			if (station == nullptr)
			{
				return"NoFind";
			}
			return station->get_config();
		}
		/**
		* @brief 执行命令
		*/
		string NetDispatcher::exec_command(const char* command, const  char* argument)
		{
			acl::string str = command;
			if (str.ncompare("call", 4) == 0)
			{
				auto list = str.split("_", true);
				if (list.size() != 3)
					return ("NoSupper");
				auto iter = list.begin();
				auto host = (++iter)->c_str();
				auto cmd = (++iter)->c_str();

				//for (int i = 0; i< 500; i++)
				//	boost::thread thread_loc(boost::bind(call_station, host, cmd, argument));
				return call_station(host, cmd, argument);
			}
			
			if (str.compare("pause", false) == 0)
			{
				return	pause_station(string(argument));
			}
			if (str.compare("resume", false) == 0)
			{
				return	resume_station(string(argument));
			}
			if (str.compare("close", false) == 0)
			{
				return	close_station(string(argument));
			}
			if (str.compare("start", false) == 0)
			{
				return	start_station(string(argument));
			}
			if (str.compare("host", false) == 0)
			{
				return	host_info(string(argument));
			}
			if (str.compare("install_api", false) == 0)
			{
				return install_station(STATION_TYPE_API, string(argument));
			}
			if (str.compare("install_pub", false) == 0)
			{
				return install_station(STATION_TYPE_PUBLISH, string(argument));
			}
			if (str.compare("install_vote", false) == 0)
			{
				return install_station(STATION_TYPE_VOTE, string(argument));
			}
			if (str.compare("exit", false) == 0)
			{
				boost::thread(boost::bind(distory_net_command));
				return "OK";
			}
			return ("NoSupper");
		}

		/**
		* 当远程调用进入时的处理
		*/
		void NetDispatcher::request(ZMQ_HANDLE socket)
		{
			vector<sharp_char> list;
			//0 路由到的地址 1 空帧 2 命令 3 参数
			_zmq_state = recv(_out_socket, list);
			string result = exec_command(*list[2], *list[3]);
			send_more(send_addr,*list[0]);
			send_late(socket, result.c_str());
		}
	}
}
