/**
 * 站点调度对象
 */

#include "../stdafx.h"
#include "station_dispatcher.h"
#include <utility>
#include "notify_station.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 当前活动的发布类
		*/
		station_dispatcher* station_dispatcher::instance = nullptr;

		/**
		*\brief 事件通知
		*/
		bool zero_event(zero_net_event event_type, const char* title, const char* sub, const char* content)
		{
			return station_dispatcher::publish_event(event_type, title, sub, content);
		}

		/**
		*\brief 系统事件通知
		*/
		bool system_event(zero_net_event event_type, const char* sub, const char* content)
		{
			return station_dispatcher::publish_event(event_type, "system", sub, content);
		}

		/**
		*\brief 通知内容
		*/
		bool station_dispatcher::publish_event(const zero_net_event event_name, const char* title, const char* sub, const char* content)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (instance == nullptr || get_net_state() == zero_def::net_state::distory)
				return false;
			shared_char description;

			description.alloc_frame_desc(static_cast<char>(event_name), zero_def::frame::sub_title);
			vector<shared_char> datas;
			datas.emplace_back(title);
			datas.emplace_back(description);
			datas.emplace_back(sub);
			if (content != nullptr)
			{
				description.append_frame(zero_def::frame::content);
				datas.emplace_back(content);
			}
			return instance->send_response(datas, false) == zmq_socket_state::succeed;
		}
		char frames[] = {
			zero_def::frame::publisher,
			zero_def::frame::content,
			zero_def::frame::global_id
		};
		/**
		*\brief 发布消息
		*/
		bool station_dispatcher::publish(const string& title, const string& publiher, const string& arg)
		{
			//boost::lock_guard<boost::mutex> guard(_mutex);
			if (get_net_state() == zero_def::net_state::distory)
				return false;
			shared_char description;
			description.alloc_desc(frames);
			vector<shared_char> datas;
			datas.emplace_back(title.c_str());
			datas.emplace_back(description);
			datas.emplace_back(publiher.c_str());
			datas.emplace_back(arg.c_str());

			shared_char global_id;
			global_id.set_int64(station_warehouse::get_glogal_id());
			datas.emplace_back(global_id);
			return send_response(datas, false) == zmq_socket_state::succeed;
		}

		/**
		* \brief 内部命令
		*/
		bool station_dispatcher::heartbeat(zmq_handler socket, uchar cmd, vector<shared_char> list)
		{
			bool success = list.size() > 2 && station_warehouse::heartbeat(cmd, list);
			send_request_status(socket, *list[0], success ? zero_def::status::ok : zero_def::status::failed, false);
			return true;
		}
		/**
		* \brief 收到PING
		*/
		void station_dispatcher::ping(zmq_handler socket, vector<shared_char>& list)
		{
			station_warehouse::heartbeat(zero_def::command::heart_pitpat, list);
		}
		/**
		* \brief 执行命令
		*/
		void station_dispatcher::restart()
		{
			config_->runtime_state(station_state::closing);
		}

		const char* station_commands_1[] =
		{
			"pause", "resume", "start", "close", "host", "install", "stop","recover", "update","remove", "doc"
		};

		enum class station_commands_2
		{
			pause, resume, start, close, host, install, stop, recover, update, remove, doc
		};

		/**
		* \brief 执行命令
		*/
		char station_dispatcher::exec_command(const char* command, vector<shared_char>& arguments, string& json) const
		{
			int idx = strmatchi(command, station_commands_1);
			switch (static_cast<station_commands_2>(idx))
			{
			case station_commands_2::doc:
			{
				switch (arguments.size())
				{
				case 2:
					return station_warehouse::upload_doc(*arguments[0], arguments[1]) ? zero_def::status::ok : zero_def::status::failed;
				case 1:
					return station_warehouse::get_doc(*arguments[0], json) ? zero_def::status::ok : zero_def::status::failed;
				}
				return zero_def::status::arg_invalid;
			}
			case station_commands_2::install:
			{
				switch (arguments.size())
				{
				case 4:
					return station_warehouse::install(*arguments[1], *arguments[0], *arguments[2], *arguments[3])
						? zero_def::status::ok
						: zero_def::status::failed;
				case 1:
					return station_warehouse::install(*arguments[0]) ? zero_def::status::ok : zero_def::status::failed;
				}
				return zero_def::status::arg_invalid;
			}
			case station_commands_2::stop:
			{
				if (arguments.empty())
					return zero_def::status::arg_invalid;
				return station_warehouse::stop(arguments[0]) ? zero_def::status::ok : zero_def::status::failed;
			}
			case station_commands_2::recover:
			{
				if (arguments.empty())
					return zero_def::status::arg_invalid;
				return station_warehouse::recover(*arguments[0]) ? zero_def::status::ok : zero_def::status::failed;
			}
			case station_commands_2::remove:
			{
				return station_warehouse::remove(*arguments[0]);
			}
			case station_commands_2::update:
			{
				if (arguments.empty())
					return zero_def::status::arg_invalid;
				return station_warehouse::update(*arguments[0]) ? zero_def::status::ok : zero_def::status::failed;
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
				return zero_def::status::not_support;
			}
		}

		/**
		* \brief 工作开始 : 处理请求数据
		*/
		void station_dispatcher::job_start(zmq_handler socket, vector<shared_char>& list, bool inner, bool old)
		{
			const char* cmd = nullptr;
			size_t rqid_index = 0, glid_index = 0, reqer_index = 0;
			vector<shared_char> arg;
			shared_char& desc = list[1];
			for (size_t idx = 2; idx <= desc.desc_size() && idx < list.size(); idx++)
			{
				switch (desc[idx])
				{
				case zero_def::frame::general_end:
				case zero_def::frame::extend_end:
					break;
				case zero_def::frame::command:
					cmd = *list[idx];
					break;
				case zero_def::frame::request_id:
					rqid_index = idx;
					break;
				case zero_def::frame::requester:
					reqer_index = idx;
					break;
				case zero_def::frame::arg:
					arg.emplace_back(list[idx]);
					break;
				case zero_def::frame::global_id:
					glid_index = idx;
					break;
				}
			}
			if (cmd == nullptr)
			{
				send_request_status(socket, *list[0], zero_def::status::arg_invalid, list, glid_index, rqid_index, reqer_index, nullptr);
				return;
			}
			string json;
			const char code = exec_command(cmd, arg, json);

			send_request_status(socket, *list[0], code, list, glid_index, rqid_index, reqer_index,
				code == zero_def::status::ok && json.length() > 0 ? json.c_str() : nullptr);
		}

		void station_dispatcher::launch(shared_ptr<station_dispatcher>& station)
		{
			zero_config& config = station->get_config();
			config.is_base = true;
			if (!station->initialize())
			{
				instance = nullptr;
				config.failed("initialize");
				set_command_thread_bad(config.station_name.c_str());
				return;
			}
			if (!station_warehouse::join(station.get()))
			{
				instance = nullptr;
				config.failed("join warehouse");
				set_command_thread_bad(config.station_name.c_str());
				return;
			}
			boost::thread(boost::bind(worker_monitor));
			station->task_semaphore_.wait();
			station->poll();
			//等待monitor_poll结束
			station->task_semaphore_.wait();
			if (get_net_state() == zero_def::net_state::runing)
			{
				instance = nullptr;
				station_warehouse::left(station.get());
				station->destruct();
				config.restart();
				run(station->get_config_ptr());
			}
			else
			{
				config.log("waiting closed");
				wait_close();
				//THREAD_SLEEP(global_config::SNDTIMEO < 0 ? 1000 : global_config::SNDTIMEO + 10);//让未发送数据完成发送
				instance = nullptr;
				station_warehouse::left(station.get());
				station->destruct();
				config.closed();
			}
			set_command_thread_end(config.station_name.c_str());
		}

		/**
		* \brief 监控轮询
		*/
		void station_dispatcher::worker_monitor()
		{
			station_dispatcher* dispatcher = instance;
			zero_config& config = dispatcher->get_config();
			config.log("worker_monitor start");
			dispatcher->task_semaphore_.post();
			while (instance != nullptr && instance->config_->is_run() && get_net_state() <= zero_def::net_state::runing)
			{
				THREAD_SLEEP(global_config::worker_sound_ivl);
				vector<string> cfgs;//复制避免锁定时间过长
				vector<string> names;//复制避免锁定时间过长
				station_warehouse::foreach_configs([&cfgs, &names](shared_ptr<zero_config>& cfg)
				{
					cfg->check_works();
					names.emplace_back(cfg->station_name);
					cfgs.emplace_back(cfg->to_status_json().c_str());
				});
				if (get_net_state() == zero_def::net_state::runing)
					dispatcher->publish_event(zero_net_event::event_worker_sound_off, "worker", nullptr, nullptr);
				for (size_t i = 0; i < names.size(); i++)
				{
					if (get_net_state() != zero_def::net_state::runing)
						break;
					dispatcher->publish_event(zero_net_event::event_station_state, "station", names[i].c_str(), cfgs[i].c_str());
				}
			}

			dispatcher->task_semaphore_.post();
			config.log("worker_monitor end");
		}
	}
}
