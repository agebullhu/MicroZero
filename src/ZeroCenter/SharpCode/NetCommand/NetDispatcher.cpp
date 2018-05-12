/**
 * ZMQ广播代理类
 *
 *
 */

#include "stdafx.h"
#include "NetDispatcher.h"
#include <utility>
#include "ApiStation.h"
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
			const int idx = strmatchi(11, command, "call", "pause", "resume", "start", "close", "host", "install", "uninstall", "exit", "ping");
			if (idx < 0)
				return (ZERO_STATUS_NO_SUPPORT);
			switch (idx)
			{
			case 0:
			{
				if (arguments.size() < 2)
					return ZERO_STATUS_FRAME_INVALID;

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
					return ZERO_STATUS_MANAGE_INSTALL_ARG_ERROR;
				return install_station(arguments[0], arguments[1]);
			}
			case 7:
			{
				if (arguments.empty())
					return ZERO_STATUS_MANAGE_INSTALL_ARG_ERROR;
				return uninstall_station(arguments[0]) ? ZERO_STATUS_OK: ZERO_STATUS_ERROR;
			}
			case 8:
			{
				boost::thread give_me_a_name(boost::bind(distory_net_command));
				sleep(1);
				return ZERO_STATUS_OK;
			}
			case 9:
			{
				return ZERO_STATUS_OK;
			}
			default:
				return ZERO_STATUS_NO_SUPPORT;
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
				return ZERO_STATUS_OK;
			}
			zero_station* station = station_warehouse::instance(arg);
			if (station == nullptr)
			{
				return ZERO_STATUS_NO_FIND;
			}
			return station->pause(true) ? ZERO_STATUS_OK : ZERO_STATUS_FAILED;
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
				return ZERO_STATUS_OK;
			}
			zero_station* station = station_warehouse::instance(arg);
			if (station == nullptr)
			{
				return (ZERO_STATUS_NO_FIND);
			}
			return station->resume(true) ? ZERO_STATUS_OK : ZERO_STATUS_FAILED;
		}

		/**
		* 当远程调用进入时的处理
		*/
		string station_dispatcher::start_station(string stattion)
		{
			zero_station* station = station_warehouse::instance(stattion);
			if (station != nullptr)
			{
				return ZERO_STATUS_RUNING;
			}
			boost::format fmt("net:host:%1%");
			fmt % stattion;
			string key = fmt.str();
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_STATION);
			acl::string val;
			if (trans_redis::get_context()->get(key.c_str(), val) && !val.empty())
			{
				return station_warehouse::restore(val) ? ZERO_STATUS_OK : ZERO_STATUS_FAILED;
			}
			return ZERO_STATUS_FAILED;
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
			const int type = strmatchi(4, type_name.c_str(), "api", "pub", "vote");
			acl::string config;
			switch (type)
			{
			case 0:
				station_warehouse::install(stattion.c_str(), STATION_TYPE_API, config); break;
			case 1:
				station_warehouse::install(stattion.c_str(), STATION_TYPE_PUBLISH, config); break;
			case 2:
				station_warehouse::install(stattion.c_str(), STATION_TYPE_VOTE, config); break;
			default:
				return ZERO_STATUS_NO_SUPPORT;
			}
			return station_warehouse::restore(config) ? ZERO_STATUS_OK : ZERO_STATUS_FAILED;
		}


		/**
		* \brief 站点卸载
		*/
		bool station_dispatcher::uninstall_station(const string& stattion)
		{
			return station_warehouse::uninstall_station(stattion);
		}
		/**
		* \brief 远程调用
		*/
		string station_dispatcher::call_station(const string& stattion, const string& command, const string& argument)
		{
			zero_station* station = station_warehouse::instance(stattion);
			if (station == nullptr)
			{
				return ZERO_STATUS_NO_FIND;
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
				return ZERO_STATUS_NO_FIND;
			}
			const sharp_char empty;
			if (arguments.size() == 1)
			{
				arguments.push_back(empty);
				arguments.push_back(empty);
				arguments.push_back(empty);
			}
			else if (arguments.size() == 2)
			{
				auto last = arguments.begin();
				++last;
				arguments.insert(last, 2, empty);
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
				return ZERO_STATUS_OK;
			}
			zero_station* station = station_warehouse::instance(stattion);
			if (station == nullptr)
			{
				return (ZERO_STATUS_NO_FIND);
			}
			return station->close(true) ? ZERO_STATUS_OK : ZERO_STATUS_FAILED;
		}

		/**
		* \brief 取机器信息
		*/
		string station_dispatcher::host_info(const string& stattion)
		{
			if (stattion == "*")
			{
				string result = "[";
				bool first = true;
				for (const map<string, zero_station*>::value_type& station : station_warehouse::examples_)
				{
					if (first)
						first = false;
					else
						result += ",";
					result += station.second->get_config();
				}
				result += "]";
				return result;
			}
			zero_station* station = station_warehouse::instance(stattion);
			if (station == nullptr)
			{
				return ZERO_STATUS_NO_FIND;
			}
			return station->get_config();
		}

		/**
		* 当远程调用进入时的处理
		*/
		void station_dispatcher::request(ZMQ_HANDLE socket, bool inner)
		{
			vector<sharp_char> list;
			//0 路由到的地址 1 空帧 2 命令 3 参数
			zmq_state_ = recv(request_scoket_, list);
			const sharp_char caller = list[0];
			const sharp_char cmd = list[2];

			list.erase(list.begin());
			list.erase(list.begin());
			list.erase(list.begin());

			string result = exec_command(*cmd, list);
			send_addr(socket, *caller);
			send_late(socket, result.c_str());
		}

		void station_dispatcher::launch(void*)
		{
			if (!station_warehouse::join(instance))
			{
				delete instance;
				return;
			}
			if (!instance->do_initialize())
				return;

			const bool reStrart = instance->poll();
			station_warehouse::left(instance);
			instance->destruct();
			if (reStrart)
			{
				delete instance;
				instance = new station_dispatcher();
				instance->zmq_state_ = zmq_socket_state::Again;
				zmq_threadstart(launch, nullptr);
			}
			else
			{
				log_msg3("Station:%s(%d | %d) closed", instance->station_name_.c_str(), instance->request_port_, instance->response_port_);
				delete instance;
				instance = nullptr;
			}
		}
	}
}
