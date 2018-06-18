/**
 * 站点调度对象
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
		bool zero_event_async(string publiher, zero_net_event event_type, string content)
		{
			boost::thread thread_xxx(boost::bind(station_dispatcher::zero_event, std::move(publiher), event_type, std::move(content)));
			return true;
		}


		/**
		*\brief 广播内容
		*/
		bool zero_event_sync(string publiher, zero_net_event event_type, string content)
		{
			return station_dispatcher::zero_event(std::move(publiher), event_type, std::move(content));
		}
		/**
		*\brief 广播内容
		*/
		bool station_dispatcher::zero_event(const string& publiher, const zero_net_event event_name, const string& content)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (instance == nullptr || get_net_state() == NET_STATE_DISTORY)
				return false;
			shared_char desc;
			char* description = desc.alloc_desc(2, static_cast<char>(event_name));
			description[2] = ZERO_FRAME_PUBLISHER;
			vector<shared_char> datas;
			datas.emplace_back("zero_event");
			datas.emplace_back(desc);
			datas.emplace_back(publiher.c_str());
			if (content.length() == 0)
			{
				description[0] = 1;
			}
			else
			{
				description[3] = ZERO_FRAME_CONTENT;
				datas.emplace_back(content.c_str());
			}
			desc.check_desc_size();
			return instance->send_response(datas);
		}
		/**
		*\brief 发布消息
		*/
		bool station_dispatcher::publish(const string& title, const string& publiher, const string& arg)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (get_net_state() == NET_STATE_DISTORY)
				return false;
			shared_char desc;
			char* description = desc.alloc_desc(3);
			description[2] = ZERO_FRAME_PUBLISHER;
			description[3] = ZERO_FRAME_CONTENT;
			description[4] = ZERO_FRAME_GLOBAL_ID;
			vector<shared_char> datas;
			datas.emplace_back(title.c_str());
			datas.emplace_back(desc);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(arg.c_str());

			shared_char global_id;
			global_id.set_value(station_warehouse::get_glogal_id());
			datas.emplace_back(global_id);
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
		char station_dispatcher::exec_command(const char* command, vector<shared_char>& arguments, string& json)
		{
			const char* commands[]=
			{
				"call", "pause", "resume", "start", "close", "host", "install", "uninstall"
			};
			const int idx = strmatchi(command, commands);
			switch (idx)
			{
			case 0:
			{
				//if (arguments.size() < 2)
				//{
				//	return ZERO_STATUS_FRAME_INVALID_ID;
				//}

				//const auto host = *arguments[0];
				//arguments.erase(arguments.begin());

				//return call_station(host, arguments);
				return ZERO_STATUS_NOT_SUPPORT_ID;
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
				return host_info(arguments.empty() ? "*" : arguments[0], json);
			}
			case 6:
			{
				if (arguments.size() < 3)
					return ZERO_STATUS_MANAGE_INSTALL_ARG_ERROR_ID;
				return install_station(*arguments[0], *arguments[1], *arguments[2]);
			}
			case 7:
			{
				if (arguments.empty())
					return ZERO_STATUS_MANAGE_INSTALL_ARG_ERROR_ID;
				return uninstall(arguments[0]) ? ZERO_STATUS_OK_ID : ZERO_STATUS_ERROR_ID;
			}
			default:
				return ZERO_STATUS_NOT_SUPPORT_ID;
			}
		}
		/**
		* \brief 执行命令
		*/
		string station_dispatcher::exec_command(const char* command, const char* argument)
		{
			acl::string str = command;
			vector<shared_char> args{ shared_char(argument) };
			string json;
			return exec_command(command, args, json) == '\0' ? ZERO_STATUS_OK : ZERO_STATUS_ERROR;
		}

		/**
		* \brief 执行一条命令
		*/
		shared_char station_dispatcher::command(const char* caller, vector<shared_char> lines)
		{
			const string val = call_station(caller, lines[0], lines[1]);
			return shared_char(val);
		}

		/**
		* \brief 取机器信息
		*/
		char station_dispatcher::host_info(const string& stattion, string& json)
		{
			return station_warehouse::host_info(stattion, json);
		}
		/**
		* 心跳的响应
		*/
		bool station_dispatcher::heartbeat(char cmd, vector<shared_char> list)
		{
			auto config = station_warehouse::get_config(list[2], false);
			if (config == nullptr)
				return false;
			switch (cmd)
			{
			case ZERO_BYTE_COMMAND_HEART_JOIN:
				config->worker_join(*list[3], *list[4]);
				return true;
			case ZERO_BYTE_COMMAND_HEART_READY:
				config->worker_ready(*list[3]);
				return true;
			case ZERO_BYTE_COMMAND_HEART_PITPAT:
				config->worker_heartbeat(*list[3]);
				return true;
			case ZERO_BYTE_COMMAND_HEART_LEFT:
				config->worker_left(*list[3]);
				return true;
			default:
				return false;
			}
		}
		/**
		* 当远程调用进入时的处理
		*/
		char station_dispatcher::pause_station(const string& arg)
		{
			if (arg == "*")
			{
				boost::lock_guard<boost::mutex> guard(station_warehouse::examples_mutex_);
				for (map<string, zero_station*>::value_type& station : station_warehouse::examples_)
				{
					station.second->pause(true);
				}
				return ZERO_STATUS_OK_ID;
			}
			zero_station* station = station_warehouse::instance(arg);
			if (station == nullptr)
			{
				return ZERO_STATUS_NOT_FIND_ID;
			}
			return station->pause(true) ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID;
		}

		/**
		* \brief 继续站点
		*/
		char station_dispatcher::resume_station(const string& arg)
		{
			if (arg == "*")
			{
				boost::lock_guard<boost::mutex> guard(station_warehouse::examples_mutex_);
				for (map<string, zero_station*>::value_type& station : station_warehouse::examples_)
				{
					station.second->resume(true);
				}
				return ZERO_STATUS_OK_ID;
			}
			zero_station* station = station_warehouse::instance(arg);
			if (station == nullptr)
			{
				return (ZERO_STATUS_NOT_FIND_ID);
			}
			return station->resume(true) ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID;
		}

		/**
		* 当远程调用进入时的处理
		*/
		char station_dispatcher::start_station(string stattion)
		{
			zero_station* station = station_warehouse::instance(stattion);
			if (station != nullptr)
			{
				return ZERO_STATUS_RUNING_ID;
			}
			shared_ptr<zero_config> config = station_warehouse::get_config(stattion);
			if (config == nullptr)
				return ZERO_STATUS_NOT_FIND_ID;
			return station_warehouse::restore(config) ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID;
		}

		/**
		* 当远程调用进入时的处理
		*/
		char station_dispatcher::install_station(const char* type_name, const char* stattion, const char* short_name)
		{
			const char* types[]= { "api", "pub", "vote" };
			const int type = strmatchi(type_name, types);
			bool success;
			switch (type)
			{
			case 0:
				success = station_warehouse::install(stattion, STATION_TYPE_API, short_name); break;
			case 1:
				success = station_warehouse::install(stattion, STATION_TYPE_PUBLISH, short_name); break;
			case 2:
				success = station_warehouse::install(stattion, STATION_TYPE_VOTE, short_name); break;
			default:
				return ZERO_STATUS_NOT_SUPPORT_ID;
			}
			return success ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID;
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
				return ZERO_STATUS_NOT_FIND;
			}
			vector<shared_char> lines;
			lines.emplace_back(command);
			lines.emplace_back(argument);
			auto result = station->command("-system", lines);
			return result;
		}


		/**
		* \brief 远程调用
		*/
		string station_dispatcher::call_station(const char* stattion, vector<shared_char>& arguments)
		{
			zero_station* station = station_warehouse::instance(stattion);
			if (station == nullptr)
			{
				return ZERO_STATUS_NOT_FIND;
			}
			const shared_char empty;
			if (arguments.size() == 1)
			{
				arguments.emplace_back(empty);
				arguments.emplace_back(empty);
				arguments.emplace_back(empty);
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
		char station_dispatcher::close_station(const string& stattion)
		{
			if (stattion == "*")
			{
				boost::lock_guard<boost::mutex> guard(station_warehouse::examples_mutex_);
				for (auto& station : station_warehouse::examples_)
				{
					station.second->close(true);
				}
				return ZERO_STATUS_OK_ID;
			}
			zero_station* station = station_warehouse::instance(stattion);
			if (station == nullptr)
			{
				return (ZERO_STATUS_NOT_FIND_ID);
			}
			return station->close(true) ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID;
		}

		/**
		* \brief 工作开始（发送到工作者）
		*/
		void station_dispatcher::job_start(ZMQ_HANDLE socket, vector<shared_char>& list, bool inner)
		{
			char* const buf = list[1].get_buffer();
			switch (buf[1])
			{
			case ZERO_BYTE_COMMAND_PING:
				send_request_status(socket, *list[0], ZERO_STATUS_OK_ID);
				return;
			case ZERO_BYTE_COMMAND_HEART_JOIN:
			case ZERO_BYTE_COMMAND_HEART_READY:
			case ZERO_BYTE_COMMAND_HEART_PITPAT:
			case ZERO_BYTE_COMMAND_HEART_LEFT:
				const bool success = heartbeat(buf[1], list);
				send_request_status(socket, *list[0], success ? ZERO_STATUS_OK_ID : ZERO_STATUS_ERROR_ID);
				return;
			}
			const char* cmd = nullptr;
			size_t rqid_index = 0, glid_index = 0, reqer_index = 0;
			vector<shared_char> arg;
			const auto frame_size = list[1].size();
			for (size_t idx = 2; idx <= frame_size; idx++)
			{
				switch (buf[idx])
				{
				case ZERO_FRAME_COMMAND:
					cmd = *list[idx];
					break;
				case ZERO_FRAME_REQUEST_ID:
					rqid_index = idx;
					break;
				case ZERO_FRAME_REQUESTER:
					reqer_index = idx;
					break;
				case ZERO_FRAME_ARG:
					arg.emplace_back(list[idx]);
					break;
				case ZERO_FRAME_GLOBAL_ID:
					glid_index = idx;
					break;
				}
			}
			if (cmd == nullptr)
			{
				send_request_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID_ID,
					glid_index == 0 ? nullptr : *list[glid_index],
					rqid_index == 0 ? nullptr : *list[rqid_index],
					reqer_index == 0 ? nullptr : *list[reqer_index]);
				return;
			}
			string json;
			const char code = exec_command(cmd, arg, json);

			send_request_status(socket, *list[0], code,
				glid_index == 0 ? nullptr : *list[glid_index],
				rqid_index == 0 ? nullptr : *list[rqid_index],
				reqer_index == 0 ? nullptr : *list[reqer_index],
				code == ZERO_STATUS_OK_ID && json.length() > 0 ? json.c_str() : nullptr);
		}

		void station_dispatcher::launch(shared_ptr<station_dispatcher>& station)
		{
			zero_config& config = station->get_config();
			config.start();
			if (!station_warehouse::join(station.get()))
			{
				instance = nullptr;
				config.failed("join warehouse");
				set_command_thread_bad(config.station_name_.c_str());
				return;
			}
			if (!station->initialize())
			{
				instance = nullptr;
				config.failed("initialize");
				set_command_thread_bad(config.station_name_.c_str());
				return;
			}
			boost::thread(boost::bind(monitor_poll));
			station->poll();
			//等待monitor_poll结束
			station->task_semaphore_.wait();
			station_warehouse::left(station.get());
			instance = nullptr;
			if (get_net_state() == NET_STATE_RUNING)
			{
				station->destruct();
				config.restart();
				run(station->get_config_ptr());
			}
			else
			{
				wait_close();
				thread_sleep(json_config::SNDTIMEO < 0 ? 1000 : json_config::SNDTIMEO + 10);//让未发送数据完成发送
				station->destruct();
				config.closed();
			}
			set_command_thread_end(config.station_name_.c_str());
		}
		/**
		* \brief 监控轮询
		*/
		void station_dispatcher::monitor_poll()
		{
			zero_config& config = instance->get_config();
			config.log("monitor poll start");
			instance->task_semaphore_.post();
			while (get_net_state() < NET_STATE_CLOSING)
			{
				thread_sleep(json_config::worker_sound_ivl);
				vector<string> cfgs;//复制避免锁定时间过长
				vector<string> names;//复制避免锁定时间过长
				station_warehouse::foreach_configs([&cfgs, &names](shared_ptr<zero_config>& cfg)
				{
					cfg->check_works();
					names.emplace_back(cfg->station_name_);
					cfgs.emplace_back(cfg->to_json(true).c_str());
				});
				instance->zero_event("*", zero_net_event::event_worker_sound_off, "");

				for (size_t i = 0; i < names.size(); i++)
				{
					instance->zero_event(names[i], zero_net_event::event_station_state, cfgs[i]);
				}
			}
			instance->task_semaphore_.post();
		}
	}
}
