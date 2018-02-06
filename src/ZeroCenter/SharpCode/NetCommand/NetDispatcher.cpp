/**
 * ZMQ广播代理类
 *
 *
 */

#include "stdafx.h"
#include "NetDispatcher.h"
 #include <utility>
#include "ApiStation.h"
#include "VoteStation.h"
#include "BroadcastingStation.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 当前活动的发布类
		*/
		net_dispatcher* net_dispatcher::instance = nullptr;


		/**
		*\brief 广播内容
		*/
		bool monitor(string publiher, string state, string content)
		{
			boost::thread thread_xxx(boost::bind(system_monitor_station::monitor, std::move(publiher), std::move(state), std::move(content)));
			return true;
		}



		/**
		* 当远程调用进入时的处理
		*/
		string net_dispatcher::pause_station(const string& arg)
		{
			if (arg == "*")
			{
				for (map<string, zero_station*>::value_type station : station_warehouse::examples_)
				{
					station.second->pause(true);
				}
				return "Ok";
			}
			zero_station* station = station_warehouse::find(arg);
			if (station == nullptr)
			{
				return "NoFind";
			}
			return station->pause(true) ? "Ok" : "Failed";
		}

		/**
		* \brief 继续站点
		*/
		string net_dispatcher::resume_station(const string& arg)
		{
			if (arg == "*")
			{
				for (map<string, zero_station*>::value_type station : station_warehouse::examples_)
				{
					station.second->resume(true);
				}
				return "Ok";
			}
			zero_station* station = station_warehouse::find(arg);
			if (station == nullptr)
			{
				return ("NoFind");
			}
			return station->resume(true) ? "Ok" : "Failed";
		}

		/**
		* 当远程调用进入时的处理
		*/
		string net_dispatcher::start_station(string stattion)
		{
			zero_station* station = station_warehouse::find(stattion);
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
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_STATION);
			acl::string val;
			if (trans_redis::get_context()->get(key, val) && !val.empty())
			{
				return station_warehouse::restore(val) ? "Ok" : "Failed";
			}
			return "Failed";
		}

		/**
		* \brief 执行一条命令
		*/
		sharp_char net_dispatcher::command(const char* caller, vector<sharp_char> lines)
		{
			string val = call_station(caller, lines[0], lines[1]);
			return sharp_char(val);
		}
		
		/**
		* 当远程调用进入时的处理
		*/
		string net_dispatcher::install_station(const string& type_name, const string& stattion)
		{
			int type = strmatchi(4, type_name.c_str(), "api", "pub", "vote");
			acl::string config;
			switch(type)
			{
			case 0:
				config = station_warehouse::install(STATION_TYPE_API, stattion.c_str()); break;
			case 1:
				config = station_warehouse::install(STATION_TYPE_PUBLISH, stattion.c_str()); break;
			case 2:
				config = station_warehouse::install(STATION_TYPE_VOTE, stattion.c_str()); break;
			default:
				return "TypeError";
			}
			return station_warehouse::restore(config) ? "Ok" : "Failed";
		}

		/**
		* \brief 远程调用
		*/
		string net_dispatcher::call_station(string stattion, string command, string argument)
		{
			zero_station* station = station_warehouse::find(std::move(stattion));
			if (station == nullptr)
			{
				return "unknow stattion";
			}
			vector<sharp_char> lines;
			lines.emplace_back(command);
			lines.emplace_back(argument);
			auto result = station->command("_sys_", lines);
			return result;
		}

		/**
		* \brief 远程调用
		*/
		string net_dispatcher::call_station(const char* stattion, vector<sharp_char>& arguments)
		{
			zero_station* station = station_warehouse::find(stattion);
			if (station == nullptr)
			{
				return "unknow stattion";
			}
			if (arguments.size() == 1)
			{
				sharp_char empty;
				arguments.push_back(empty);
				arguments.push_back(empty);
				arguments.push_back(empty);
			}
			else if(arguments.size() == 2)
			{
				auto last = arguments.begin();
				++last;
				sharp_char empty;
				arguments.insert(last,2, empty);
			}
			auto result = station->command("_sys_", arguments);
			return result;
		}

		/**
		* 当远程调用进入时的处理
		*/
		string net_dispatcher::close_station(const string& stattion)
		{
			if (stattion == "*")
			{
				for (map<string, zero_station*>::value_type station : station_warehouse::examples_)
				{
					station.second->close(true);
				}
				return "Ok";
			}
			zero_station* station = station_warehouse::find(stattion);
			if (station == nullptr)
			{
				return ("NoFind");
			}
			return station->close(true) ? "Ok" : "Failed";
		}

		/**
		* \brief 取机器信息
		*/
		string net_dispatcher::host_info(const string& stattion)
		{
			if (stattion == "*")
			{
				string result = "[";
				for (map<string, zero_station*>::value_type station : station_warehouse::examples_)
				{
					result += station.second->get_config();
					result += ",";
				}
				result += "]";
				return result;
			}
			zero_station* station = station_warehouse::find(stattion);
			if (station == nullptr)
			{
				return "NoFind";
			}
			return station->get_config();
		}

		/**
		* \brief 执行命令
		*/
		string net_dispatcher::exec_command(const char* command, vector<sharp_char> arguments)
		{
			int idx = strmatchi(9, command, "call", "pause", "resume", "start", "close", "host", "install", "exit");
			if (idx < 0)
				return ("NoSupper");
			switch (idx)
			{
			case 0:
			{
				if (arguments.size() < 2)
					return ("ArgumentError! must like :call [name] [command] [argument]");

				auto host = *arguments[0];
				arguments.erase(arguments.begin());

				return call_station(host, arguments);
			}
			case 1:
			{
				return pause_station(arguments.empty()? "*" : arguments[0]);
			}
			case 2:
			{
				return resume_station(arguments.empty() ? "*" : arguments[0]);
			}
			case 3:
			{
				return start_station(arguments.empty() ? "*" : arguments[0]);
			}
			case 4:
			{
				return close_station(arguments.empty() ? "*" : arguments[0]);
			}
			case 5:
			{
				return host_info(arguments.empty() ? "*" : arguments[0]);
			}
			case 6:
			{
				if (arguments.size() < 2)
					return ("ArgumentError! must like :install [type] [name]");
				return install_station(arguments[0], arguments[1]);
			}
			case 7:
			{
				boost::thread(boost::bind(distory_net_command));
				sleep(1);
				return "OK";
			}
			default:
				return ("NoSupper");
			}
		}

		/**
		* \brief 执行命令
		*/
		string net_dispatcher::exec_command(const char* command, const char* argument)
		{
			acl::string str = command;
			return exec_command(command,{ sharp_char(argument) });
		}

		/**
		* 当远程调用进入时的处理
		*/
		void net_dispatcher::request(ZMQ_HANDLE socket)
		{
			vector<sharp_char> list;
			//0 路由到的地址 1 空帧 2 命令 3 参数
			_zmq_state = recv(_out_socket, list);
			sharp_char caller = list[0];
			sharp_char cmd = list[2];

			list.erase(list.begin());
			list.erase(list.begin());
			list.erase(list.begin());

			string result = exec_command(*cmd, list);
			send_addr(socket, *caller);
			send_late(socket, result.c_str());
		}

		void net_dispatcher::start(void*)
		{
			if (!station_warehouse::join(instance))
			{
				delete instance;
				return;
			}
			if (!instance->do_initialize())
				return;

			bool reStrart = instance->poll();
			station_warehouse::left(instance);
			instance->destruct();
			if (reStrart)
			{
				delete instance;
				instance = new net_dispatcher();
				instance->_zmq_state = ZmqSocketState::Again;
				zmq_threadstart(start, nullptr);
			}
			else
			{
				log_msg3("Station:%s(%d | %d) closed", instance->_station_name.c_str(), instance->_out_port, instance->_inner_port);
				delete instance;
				instance = nullptr;
			}
		}
	}
}
