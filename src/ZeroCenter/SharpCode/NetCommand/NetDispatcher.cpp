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
		station_dispatcher* station_dispatcher::instance = nullptr;


		/**
		*\brief 广播内容
		*/
		bool monitor_async(string publiher, string state, string content)
		{
			boost::thread thread_xxx(boost::bind(system_monitor_station::monitor, std::move(publiher), std::move(state), std::move(content)));
			return true;
		}


		/**
		*\brief 广播内容
		*/
		bool monitor_sync(string publiher, string state, string content)
		{
			return system_monitor_station::monitor(std::move(publiher), std::move(state), std::move(content));
		}

		/**
		* \brief 执行命令
		*/
		string station_dispatcher::exec_command(const char* command, vector<sharp_char> arguments)
		{
			int idx = strmatchi(9, command, "call", "pause", "resume", "start", "close", "host", "install", "exit", "ping");
			if (idx < 0)
				return (zero_command_no_support);
			switch (idx)
			{
			case 0:
			{
				if (arguments.size() < 2)
					return zero_command_arg_error;

				const auto host = *arguments[0];
				arguments.erase(arguments.begin());

				return call_station(host, arguments);
			}
			case 1:
			{
				return pause_station(arguments.empty() ? "*" : arguments[0]);
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
					return zero_command_install_arg_error;
				return install_station(arguments[0], arguments[1]);
			}
			case 7:
			{
				boost::thread(boost::bind(distory_net_command));
				sleep(1);
				return zero_command_ok;
			}
			case 8:
			{
				return zero_command_ok;
			}
			default:
				return zero_command_no_support;
			}
		}

		/**
		* \brief 执行命令
		*/
		string station_dispatcher::exec_command(const char* command, const char* argument)
		{
			acl::string str = command;
			return exec_command(command, { sharp_char(argument) });
		}

		/**
		* 当远程调用进入时的处理
		*/
		string station_dispatcher::pause_station(const string& arg)
		{
			if (arg == "*")
			{
				auto examples = station_warehouse::examples_;
				for (map<string, zero_station*>::value_type& station : examples)
				{
					station.second->pause(true);
				}
				return zero_command_ok;
			}
			zero_station* station = station_warehouse::instance(arg);
			if (station == nullptr)
			{
				return zero_command_no_find;
			}
			return station->pause(true) ? zero_command_ok : zero_command_failed;
		}

		/**
		* \brief 继续站点
		*/
		string station_dispatcher::resume_station(const string& arg)
		{
			if (arg == "*")
			{
				auto examples = station_warehouse::examples_;
				for (map<string, zero_station*>::value_type& station : examples)
				{
					station.second->resume(true);
				}
				return zero_command_ok;
			}
			zero_station* station = station_warehouse::instance(arg);
			if (station == nullptr)
			{
				return (zero_command_no_find);
			}
			return station->resume(true) ? zero_command_ok : zero_command_failed;
		}

		/**
		* 当远程调用进入时的处理
		*/
		string station_dispatcher::start_station(string stattion)
		{
			zero_station* station = station_warehouse::instance(stattion);
			if (station != nullptr)
			{
				return zero_command_runing;
			}
			boost::format fmt("net:host:%1%");
			fmt % stattion;
			string key = fmt.str();
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_STATION);
			acl::string val;
			if (trans_redis::get_context()->get(key.c_str(), val) && !val.empty())
			{
				return station_warehouse::restore(val) ? zero_command_ok : zero_command_failed;
			}
			return zero_command_failed;
		}

		/**
		* \brief 执行一条命令
		*/
		sharp_char station_dispatcher::command(const char* caller, vector<sharp_char> lines)
		{
			string val = call_station(caller, lines[0], lines[1]);
			return sharp_char(val);
		}
		
		/**
		* 当远程调用进入时的处理
		*/
		string station_dispatcher::install_station(const string& type_name, const string& stattion)
		{
			int type = strmatchi(4, type_name.c_str(), "api", "pub", "vote");
			acl::string config;
			switch(type)
			{
			case 0:
				config = station_warehouse::install( stattion.c_str(), STATION_TYPE_API,config); break;
			case 1:
				config = station_warehouse::install(stattion.c_str(), STATION_TYPE_PUBLISH, config); break;
			case 2:
				config = station_warehouse::install(stattion.c_str(), STATION_TYPE_VOTE, config); break;
			default:
				return zero_command_no_support;
			}
			return station_warehouse::restore(config) ? zero_command_ok : zero_command_failed;
		}

		/**
		* \brief 远程调用
		*/
		string station_dispatcher::call_station(string stattion, string command, string argument)
		{
			zero_station* station = station_warehouse::instance(stattion);
			if (station == nullptr)
			{
				return zero_command_no_find;
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
		string station_dispatcher::call_station(const char* stattion, vector<sharp_char>& arguments)
		{
			zero_station* station = station_warehouse::instance(stattion);
			if (station == nullptr)
			{
				return zero_command_no_find;
			}
			const sharp_char empty;
			if (arguments.size() == 1)
			{
				arguments.push_back(empty);
				arguments.push_back(empty);
				arguments.push_back(empty);
			}
			else if(arguments.size() == 2)
			{
				auto last = arguments.begin();
				++last;
				arguments.insert(last,2, empty);
			}
			auto result = station->command("_sys_", arguments);
			return result;
		}

		/**
		* 当远程调用进入时的处理
		*/
		string station_dispatcher::close_station(const string& stattion)
		{
			if (stattion == "*")
			{
				map<string, zero_station*> examples = station_warehouse::examples_;
				for (map<string, zero_station*>::value_type station : examples)
				{
					station.second->close(true);
				}
				return zero_command_ok;
			}
			zero_station* station = station_warehouse::instance(stattion);
			if (station == nullptr)
			{
				return (zero_command_no_find);
			}
			return station->close(true) ? zero_command_ok : zero_command_failed;
		}

		/**
		* \brief 取机器信息
		*/
		string station_dispatcher::host_info(const string& stattion)
		{
			if (stattion == "*")
			{
				string result = "[";
				for (const map<string, zero_station*>::value_type& station : station_warehouse::examples_)
				{
					result += station.second->get_config();
					result += ",";
				}
				result += "]";
				return result;
			}
			zero_station* station = station_warehouse::instance(stattion);
			if (station == nullptr)
			{
				return zero_command_no_find;
			}
			return station->get_config();
		}

		/**
		* 当远程调用进入时的处理
		*/
		void station_dispatcher::request(ZMQ_HANDLE socket)
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

		void station_dispatcher::start(void*)
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
				instance = new station_dispatcher();
				instance->_zmq_state = zmq_socket_state::Again;
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
