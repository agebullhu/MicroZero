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
			case STATION_TYPE_DISPATCHER:
				NetDispatcher::run("SystemManage");
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
				install(STATION_TYPE_PUBLISH, "SystemMonitor");
			}
		}
		/**
		* @brief 初始化服务
		*/
		acl::string StationWarehouse::install(int station_type,const char* station_name)
		{
			char* key;
			{
				boost::format fmt("net:host:%1%:%2%");
				fmt % station_type % station_name;
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
			{
				TransRedis::get_context()->incr(port_redis_key, &port);
				boost::format fmt1("tcp://*:%1%");
				fmt1 % port;
				node.add_number("heart_port", port);
				node.add_text("heart_addr", fmt1.str().c_str());
			}
			{
				TransRedis::get_context()->incr(port_redis_key, &port);
				boost::format fmt1("tcp://*:%1%");
				fmt1 % port;
				node.add_number("inner_port", port);
				node.add_text("inner_addr", fmt1.str().c_str());
			}
			val = node.to_string();
			TransRedis::get_context()->set(key, val);
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
			acl::string val = install(station->_station_type, station->_station_name.c_str());
			config cfg(val);
			station->_innerAddress = cfg["inner_addr"];
			station->_outAddress = cfg["out_addr"];
			station->_heartAddress = cfg["heart_addr"];
			examples.insert(make_pair(station->_station_name, station));
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
		*消息泵
		*/
		bool NetDispatcher::send_result(string caller, string state)
		{
			if (caller.length() == 0)
			{
				cout << state << endl;
				return false;
			}
			s_sendmore(example->_outSocket, caller.c_str());
			s_sendmore(example->_outSocket, "");
			s_send(example->_outSocket, state.c_str());//真实发送
			return true;
		}

		/**
		* 当远程调用进入时的处理
		*/
		void NetDispatcher::pause_station(string caller, string stattion)
		{
			NetStation* station = StationWarehouse::find(stattion);
			if (station == nullptr)
			{
				send_result(caller, "NoFind");
				return;
			}
			if (station->pause(true))
				send_result(caller, "Ok");
			else
				send_result(caller, "Failed");
		}

		/**
		* @brief 继续站点
		*/
		void NetDispatcher::resume_station(string caller, string stattion)
		{
			NetStation* station = StationWarehouse::find(stattion);
			if (station == nullptr)
			{
				send_result(caller, "NoFind");
				return;
			}
			if (station->resume(true))
				send_result(caller, "Ok");
			else
				send_result(caller, "Failed");
		}
		/**
		* 当远程调用进入时的处理
		*/
		void NetDispatcher::start_station(string caller, string stattion)
		{
			NetStation* station = StationWarehouse::find(stattion);
			if (station != nullptr)
			{
				send_result(caller, "IsRuning");
				return;
			}
			if (station->resume(true))
				send_result(caller, "Ok");
			else
				send_result(caller, "Failed");
		}

		/**
		* 当远程调用进入时的处理
		*/
		void NetDispatcher::shutdown(string caller, string stattion)
		{
			distory_net_command();
			send_result(caller, "Ok");
		}
		/**
		* 当远程调用进入时的处理
		*/
		void NetDispatcher::close_station(string caller, string stattion)
		{
			NetStation* station = StationWarehouse::find(stattion);
			if (station == nullptr)
			{
				send_result(caller, "NoFind");
				return;
			}
			if (station->close(true))
				send_result(caller, "Ok");
			else
				send_result(caller, "Failed");
		}

		/**
		* @brief 执行命令
		*/
		void NetDispatcher::exec_command(const char* client_addr, const  char* command, const  char* argument)
		{
			if (_stricmp(command, "pause")==0)
			{
				pause_station( string(client_addr), string(argument));
			}
			else if (_stricmp(command, "resume") == 0)
			{
				resume_station( string(client_addr), string(argument));
			}
			else if (_stricmp(command, "close") == 0)
			{
				close_station( string(client_addr), string(argument));
			}
			else if (_stricmp(command, "start") == 0)
			{
				start_station( string(client_addr), string(argument));
			}
			else if (_stricmp(command, "shutdown") == 0)
			{
				shutdown( string(client_addr), string(argument));
			}
			else
			{
				send_result(client_addr, "NoSupper");
			}
		}

		/**
		* 当远程调用进入时的处理
		*/
		void NetDispatcher::onCallerPollIn()
		{
			// 获取下一个client的请求，交给空闲的worker处理
			// client请求的消息格式是：[client地址][空帧][请求内容]
			char* client_addr = s_recv(_outSocket, 0);
			recv_empty(_outSocket);
			char* command = s_recv(_outSocket);
			recv_empty(_outSocket);
			char* argument = s_recv(_outSocket);

			boost::thread thrds_s3(boost::bind(exec_command, client_addr, command, argument));
			
			free(client_addr);
			free(command);
			free(argument);
		}

		bool NetDispatcher::poll()
		{
			_zmq_state = 0;
			_outSocket = create_socket(ZMQ_ROUTER, _outAddress.c_str());
			zmq_pollitem_t items[] = {
				{ _outSocket, 0, ZMQ_POLLIN, 0 }
			};
			log_msg3("%s(%s | %s)已启动", _station_name, _outAddress, _innerAddress);
			//登记线程开始
			set_command_thread_start();
			_station_state = station_state::Run;
			while (can_do())
			{
				poll_check_pause();
				poll_zmq_poll(1);
				if (items[0].revents & ZMQ_POLLIN)
				{
					onCallerPollIn();
				}
			}
			_station_state = station_state::Closing;
			zmq_unbind(_outSocket, _outAddress.c_str());
			zmq_close(_outSocket);
			_outSocket = nullptr;
			//登记线程关闭
			set_command_thread_end();
			_station_state = station_state::Closed;
			return _zmq_state < 0;
		}
	}
}
