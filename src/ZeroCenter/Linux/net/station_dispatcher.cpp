/**
 * ZMQ广播代理类
 *
 *
 */

#include "../stdafx.h"
#include "station_dispatcher.h"
#include <utility>
#include "broadcasting_station.h"

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
			boost::thread thread_xxx(boost::bind(station_dispatcher::monitor, std::move(publiher), std::move(state), std::move(content)));
			return true;
		}


		/**
		*\brief 广播内容
		*/
		bool monitor_sync(string publiher, string state, string content)
		{
			return station_dispatcher::monitor(std::move(publiher), std::move(state), std::move(content));
		}

		/**
		*\brief 发布消息
		*/
		bool station_dispatcher::publish(const string& title, const string& publiher, const string& arg)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (get_net_state() == NET_STATE_DISTORY || publiher.length() == 0)
				return false;
			sharp_char description;
			description.alloc(6);
			char* buf = description.get_buffer();
			buf[0] = 2;
			buf[1] = ZERO_FRAME_PUBLISHER;
			buf[2] = ZERO_FRAME_ARG;
			buf[3] = ZERO_FRAME_END;
			vector<sharp_char> datas;
			datas.emplace_back(title.c_str());
			datas.emplace_back(description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(arg.c_str());
			return send_response(datas);
		}

		/**
		*\brief 发布消息
		*/
		bool station_dispatcher::publish(const string& station, const string& publiher,
			const string& title, const string& sub, const string& arg)
		{
			const auto pub = station_warehouse::instance(station);
			if (pub == nullptr || pub->get_station_type() != STATION_TYPE_PUBLISH)
				return false;
			return dynamic_cast<broadcasting_station*>(pub)->publish(publiher, title, sub, arg);
		}

		/**
		* \brief 执行命令
		*/
		string station_dispatcher::exec_command(const char* command, vector<sharp_char> arguments)
		{
			const int idx = strmatchi(12, command, "call", "pause", "resume", "start", "close", "host", "install", "uninstall", "exit", "ping", "heartbeat");
			if (idx < 0)
				return (ZERO_STATUS_NO_SUPPORT);
			switch (idx)
			{
			case 0:
			{
				if (arguments.size() < 2)
				{
					return ZERO_STATUS_FRAME_INVALID;
				}

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
				return uninstall(arguments[0]) ? ZERO_STATUS_OK : ZERO_STATUS_ERROR;
			}
			case 8:
			{
				boost::thread(boost::bind(distory_net_command));
				return ZERO_STATUS_OK;
			}
			case 9:
			{
				return ZERO_STATUS_OK;
			}
			case 10:
			{
				if (arguments.size() < 4)
					return ZERO_STATUS_FRAME_INVALID;
				;
				return heartbeat(arguments) ? ZERO_STATUS_OK : ZERO_STATUS_ERROR;
			}
			default:
				return ZERO_STATUS_NO_SUPPORT;
			}
		}
		/**
		* 心跳的响应
		*/
		bool station_dispatcher::heartbeat(vector<sharp_char> list)
		{
			auto config = station_warehouse::get_config(list[1], false);
			if (config == nullptr)
				return false;
			switch (list[0][0])
			{
			case ZERO_HEART_LEFT:
				config->worker_left(*list[2]);
				return true;
			case ZERO_HEART_JOIN:
			case ZERO_HEART_PITPAT:
				config->worker_heartbeat(*list[2], *list[3]);
				return true;
			default:
				return false;
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
			shared_ptr<zero_config> config = station_warehouse::get_config(stattion);
			if (config == nullptr)
				return ZERO_STATUS_NET_ERROR;
			return station_warehouse::restore(config) ? ZERO_STATUS_OK : ZERO_STATUS_FAILED;
		}

		/**
		* \brief 执行一条命令
		*/
		sharp_char station_dispatcher::command(const char* caller, vector<sharp_char> lines)
		{
			const string val = call_station(caller, lines[0], lines[1]);
			return sharp_char(val);
		}

		/**
		* 当远程调用进入时的处理
		*/
		string station_dispatcher::install_station(const string& type_name, const string& stattion)
		{
			const int type = strmatchi(4, type_name.c_str(), "api", "pub", "vote");
			bool success;
			switch (type)
			{
			case 0:
				success = station_warehouse::install(stattion.c_str(), STATION_TYPE_API); break;
			case 1:
				success = station_warehouse::install(stattion.c_str(), STATION_TYPE_PUBLISH); break;
			case 2:
				success = station_warehouse::install(stattion.c_str(), STATION_TYPE_VOTE); break;
			default:
				return ZERO_STATUS_NO_SUPPORT;
			}
			return success ? ZERO_STATUS_OK : ZERO_STATUS_FAILED;
		}

		/**
		* \brief 站点卸载
		*/
		bool station_dispatcher::uninstall(const string& stattion)
		{
			return station_warehouse::uninstall(stattion);
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
			auto result = station->command("-system", lines);
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
			auto result = station->command("-system", arguments);
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
					result += station.second->get_config().to_json().c_str();
				}
				result += "]";
				return result;
			}
			zero_station* station = station_warehouse::instance(stattion);
			if (station == nullptr)
			{
				return ZERO_STATUS_NO_FIND;
			}
			return station->get_config().to_json().c_str();
		}

		/**
		* 当远程调用进入时的处理
		*/
		void station_dispatcher::request(ZMQ_HANDLE socket, bool inner)
		{
			vector<sharp_char> list;
			//0 路由到的地址 1 空帧 2 命令 3 参数
			zmq_state_ = recv(request_scoket_tcp_, list);
			const sharp_char caller = list[0];
			const sharp_char cmd = list[2];

			list.erase(list.begin());
			list.erase(list.begin());
			list.erase(list.begin());

			string result = exec_command(*cmd, list);
			send_request_status(socket, *caller, result.c_str());
		}

		void station_dispatcher::launch(shared_ptr<station_dispatcher>& station)
		{
			station->config_->log_start();
			if (!station_warehouse::join(station.get()))
			{
				instance = nullptr;
				station->config_->log_failed();
				return;
			}
			if (!station->initialize())
			{
				instance = nullptr;
				station->config_->log_failed();
				return;
			}
			boost::thread(boost::bind(monitor_poll));
			station->poll();
			station_warehouse::left(station.get());
			if (station->config_->station_state_ != station_state::Uninstall && get_net_state() == NET_STATE_RUNING)
			{
				instance = nullptr;
				station->destruct();
				station->config_->station_state_ = station_state::ReStart;
				run(station->config_);
			}
			else
			{
				while (get_net_state() <= NET_STATE_CLOSED)
					thread_sleep(1000);
				instance = nullptr;
				station->destruct();
				station->config_->log_closed();
			}
			thread_sleep(1000);
		}

		/**
		* \brief 监控轮询
		*/
		void station_dispatcher::monitor_poll()
		{
			instance->config_->log("monitor poll start");
			while (instance != nullptr &&  get_net_state() <= NET_STATE_CLOSED)
			{
				thread_sleep(1000);
				for (auto & config : station_warehouse::configs_)
				{
					for (auto & work : config.second->workers)
					{
						if (work.second.check() < 0)
							config.second->workers.erase(work.first);
					}
					monitor(config.first, "station_state", config.second->to_json().c_str());
				}
				monitor("SystemManage", "worker_sound_off", "*");
			}
			for (auto & config : station_warehouse::configs_)
			{
				config.second->station_state_ = station_state::Destroy;
			}
			station_warehouse::save_configs();
		}
	}
}
