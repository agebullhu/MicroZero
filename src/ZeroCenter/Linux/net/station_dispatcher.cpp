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
		bool station_dispatcher::zero_event(const string publiher, const zero_net_event event_name, const string content)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (instance == nullptr || get_net_state() == NET_STATE_DISTORY)
				return false;
			shared_char description;

			description.alloc_frame_1(static_cast<char>(event_name), ZERO_FRAME_PUBLISHER);
			vector<shared_char> datas;
			datas.emplace_back("zero_event");
			datas.emplace_back(description);
			datas.emplace_back(publiher.c_str());
			if (content.length() != 0)
			{
				description.append_frame(ZERO_FRAME_CONTENT);
				datas.emplace_back(content.c_str());
			}
			return instance->send_response(datas);
		}
		char frames[] = {
			ZERO_FRAME_PUBLISHER,
			ZERO_FRAME_CONTENT,
			ZERO_FRAME_GLOBAL_ID
		};
		/**
		*\brief 发布消息
		*/
		bool station_dispatcher::publish(const string& title, const string& publiher, const string& arg)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (get_net_state() == NET_STATE_DISTORY)
				return false;
			shared_char description;
			description.alloc_frame(frames);
			vector<shared_char> datas;
			datas.emplace_back(title.c_str());
			datas.emplace_back(description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(arg.c_str());

			shared_char global_id;
			global_id.set_int64x(station_warehouse::get_glogal_id());
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

		const char* station_commands_1[] =
		{
			"pause", "resume", "start", "close", "host", "install", "uninstall", "update", "doc"
		};

		enum class station_commands_2
		{
			pause, resume, start, close, host, install, uninstall, update,doc
		};
		/**
		* \brief 执行命令
		*/
		char station_dispatcher::exec_command(const char* command, vector<shared_char>& arguments, string& json)
		{
			int idx = strmatchi(command, station_commands_1);
			switch (static_cast<station_commands_2>(idx))
			{
			case station_commands_2::doc:
			{
				switch (arguments.size())
				{
				case 2:
					return station_warehouse::upload_doc(*arguments[0], arguments[1]);
				case 1:
					return station_warehouse::get_doc(*arguments[0], json) ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID;
				}
				return ZERO_STATUS_ARG_INVALID_ID;
			}
			case station_commands_2::install:
			{
				switch (arguments.size())
				{
				case 2:
					return station_warehouse::install_station(*arguments[1], *arguments[0], *arguments[2]);
				case 1:
					return station_warehouse::install(*arguments[0]) ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID;
				}
				return ZERO_STATUS_ARG_INVALID_ID;
			}
			case station_commands_2::uninstall:
			{
				if (arguments.empty())
					return ZERO_STATUS_ARG_INVALID_ID;
				return station_warehouse::uninstall(arguments[0]) ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID;
			}
			case station_commands_2::update:
			{
				if (arguments.empty())
					return ZERO_STATUS_ARG_INVALID_ID;
				return station_warehouse::update(*arguments[0]) ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID;
			}
			case station_commands_2::pause:
			{
				return station_warehouse::pause_station(arguments.empty() ? "*" : arguments[0]);
			}
			case station_commands_2::resume:
			{
				return station_warehouse::resume_station(arguments.empty() ? "*" : arguments[0]);
			}
			case station_commands_2::start:
			{
				return station_warehouse::start_station(arguments.empty() ? "*" : arguments[0]);
			}
			case station_commands_2::close:
			{
				return station_warehouse::close_station(arguments.empty() ? "*" : arguments[0]);
			}
			case station_commands_2::host:
			{
				return station_warehouse::host_info(arguments.empty() ? "*" : arguments[0], json);
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
				const bool success = station_warehouse::heartbeat(buf[1], list);
				send_request_status(socket, *list[0], success ? ZERO_STATUS_OK_ID : ZERO_STATUS_FAILED_ID);
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
				send_request_status(socket, ZERO_STATUS_ARG_INVALID_ID, list, glid_index, rqid_index, reqer_index);
				return;
			}
			string json;
			const char code = exec_command(cmd, arg, json);

			send_request_status(socket, *list[0], code, list, glid_index, rqid_index, reqer_index,
				code == ZERO_STATUS_OK_ID && json.length() > 0 ? json.c_str() : nullptr);
		}

		void station_dispatcher::launch(shared_ptr<station_dispatcher>& station)
		{
			zero_config& config = station->get_config();
			if (!station->initialize())
			{
				instance = nullptr;
				config.failed("initialize");
				set_command_thread_bad(config.station_name_.c_str());
				return;
			}
			if (!station_warehouse::join(station.get()))
			{
				instance = nullptr;
				config.failed("join warehouse");
				set_command_thread_bad(config.station_name_.c_str());
				return;
			}
			boost::thread(boost::bind(monitor_poll));
			station->poll();
			//等待monitor_poll结束
			station->task_semaphore_.wait();
			station_warehouse::left(station.get());
			if (get_net_state() == NET_STATE_RUNING)
			{
				instance = nullptr;
				station->destruct();
				config.restart();
				run(station->get_config_ptr());
			}
			else
			{
				config.log("waiting closed");
				wait_close();
				thread_sleep(json_config::SNDTIMEO < 0 ? 1000 : json_config::SNDTIMEO + 10);//让未发送数据完成发送

				instance = nullptr;
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
					cfgs.emplace_back(cfg->to_status_json().c_str());
				});
				instance->zero_event("*", zero_net_event::event_worker_sound_off, "");

				for (size_t i = 0; i < names.size(); i++)
				{
					instance->zero_event(names[i], zero_net_event::event_station_state, cfgs[i]);
				}
			}
			instance->task_semaphore_.post();
			config.log("monitor poll end");
		}
	}
}
