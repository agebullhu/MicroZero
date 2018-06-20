#include "../stdinc.h"
#include "zero_config.h"
#include "../cfg/json_config.h"

namespace agebull
{
	namespace zmq_net
	{
		int worker::check()
		{
			const int64 tm = time(nullptr) - pre_time;

			if (state == -1)
				state = 1;
			if (tm <= 2)
			{
				return level = 5;
			}
			if (tm <= 6)
			{
				return level = 4;
			}
			if (tm <= 10)
			{
				return level = 3;
			}
			state = -1;
			if (tm <= 15)
			{
				return level = 2;
			}
			if (tm <= 30)
			{
				return level = 1;
			}
			if (tm <= 60)
			{
				return level = 0;
			}
			return level = -1;
		}

		void zero_config::worker_join(const char* real_name, const char* ip)
		{
			boost::lock_guard<boost::mutex> guard(mutex_);
			auto iter = workers.find(real_name);
			if (iter == workers.end())
			{
				worker wk;
				wk.real_name = real_name;
				wk.ip_address = ip;
				workers.insert(make_pair(real_name, wk));
			}
			else
			{
				iter->second.ip_address = ip;
				iter->second.level = -1;
				iter->second.pre_time = time(nullptr);
				if (iter->second.state == 1)
				{
					--ready_works_;
				}
				iter->second.state = 0;
			}
			log("worker_join", real_name);
		}

		void zero_config::worker_ready(const char* real_name)
		{
			boost::lock_guard<boost::mutex> guard(mutex_);
			auto iter = workers.find(real_name);
			if (iter == workers.end())
			{
				worker wk;
				wk.real_name = real_name;
				wk.state = 1;
				wk.level = 5;
				workers.insert(make_pair(real_name, wk));
				++ready_works_;
				log("worker_ready", real_name);
			}
			else
			{
				if (iter->second.state != 1) //曾经失联
				{
					++ready_works_;
					iter->second.state = 1;
					log("worker_ready", real_name);
				}
				iter->second.active();
			}
		}

		void zero_config::worker_left(const char* real_name)
		{
			boost::lock_guard<boost::mutex> guard(mutex_);
			auto iter = workers.find(real_name);
			if (iter == workers.end())
				return;
			if (iter->second.state == 1)
				--ready_works_;
			workers.erase(real_name);
			log("worker_left", real_name);
		}

		void zero_config::check_works()
		{
			boost::lock_guard<boost::mutex> guard(mutex_);
			int ready = 0;
			vector<string> lefts;
			for (auto& work : workers)
			{
				work.second.check();
				if (work.second.level < 0)
				{
					lefts.emplace_back(work.first);
				}
				else if (work.second.state == 1)
				{
					++ready;
				}
			}
			for (auto& worker : lefts)
			{
				workers.erase(worker);
				log("worker_left", worker.c_str());
			}
			ready_works_ = ready;
		}
		const char* fields[] =
		{
			"station_name"
			, "station_type"
			, "request_port"
			, "worker_out_port"
			, "worker_in_port"
			, "_description"
			, "_caption"
			, "station_alias"
			, "station_state"
			, "request_in"
			, "request_out"
			, "request_err"
			, "worker_in"
			, "worker_out"
			, "worker_err"
			, "short_name"
		}; 
		enum class config_fields
		{
			station_name
			, station_type
			, request_port
			, worker_out_port
			, worker_in_port
			, description
			, caption
			, station_alias
			, station_state
			, request_in
			, request_out
			, request_err
			, worker_in
			, worker_out
			, worker_err
			, short_name
		};
		void zero_config::read_json(const char* val)
		{
			boost::lock_guard<boost::mutex> guard(mutex_);
			acl::json json;
			json.update(val);
			acl::json_node* iter = json.first_node();
			while (iter)
			{
				const char* tag = iter->tag_name();
				if (tag == nullptr || tag[0] == 0)
				{
					iter = json.next_node();
					continue;
				}
				
				const int idx = strmatchi(tag, fields);
				switch ((config_fields)idx)
				{
				case config_fields::station_name:
					station_name_ = iter->get_string();
					break;
				case config_fields::station_type:
					station_type_ = static_cast<int>(*iter->get_int64());
					break;
				case config_fields::request_port:
					request_port_ = static_cast<int>(*iter->get_int64());
					break;
				case config_fields::worker_out_port:
					worker_out_port_ = static_cast<int>(*iter->get_int64());
					break;
				case config_fields::worker_in_port:
					worker_in_port_ = static_cast<int>(*iter->get_int64());
					break;
				case config_fields::description:
					station_description_ = iter->get_string();
					break;
				case config_fields::caption:
					station_caption_ = iter->get_string();
					break;
				case config_fields::station_alias:
					alias_.clear();
					{
						auto ajson = iter->get_obj();
						if (!ajson->is_array())
							break;
						auto it = ajson->first_child();
						while (it)
						{
							alias_.emplace_back(it->get_string());
							it = ajson->next_child();
						}
					}
					break;
				case config_fields::station_state:
					station_state_ = static_cast<station_state>(*iter->get_int64());
					break;
				case config_fields::request_in:
					request_in = *iter->get_int64();
					break;
				case config_fields::request_out:
					request_out = *iter->get_int64();
					break;
				case config_fields::worker_err:
					worker_err = *iter->get_int64();
					break;
				case config_fields::worker_in:
					worker_in = *iter->get_int64();
					break;
				case config_fields::worker_out:
					worker_out = *iter->get_int64();
					break;
				case config_fields::short_name:
					short_name = *iter->get_string();
					break;
				default: break;
				}
				iter = json.next_node();
			}
			check_type_name();
		}

		/**
		* \brief 写入JSON
		* \param type 记录类型 0 全量 1 心跳时的动态信息 2 配置保存时无动态信息
		*/
		acl::string zero_config::to_json(int type)
		{
			boost::lock_guard<boost::mutex> guard(mutex_);
			acl::json json;
			acl::json_node& node = json.create_node();
			node.add_text("station_name", station_name_.c_str());
			node.add_number("station_state", static_cast<int>(station_state_));
			if (type != 1)
			{
				if (!short_name.empty())
					node.add_text("short_name", short_name.c_str());
				if (!station_caption_.empty())
					node.add_text("_caption", station_caption_.c_str());
				if (!station_description_.empty())
					node.add_text("_description", station_description_.c_str());
				if (alias_.size() > 0)
				{
					acl::json_node& array = json.create_array();
					array.set_tag("station_alias");
					array.add_array_text(short_name.c_str());
					for (auto alia : alias_)
					{
						array.add_array_text(alia.c_str());
					}
				}
				if (station_type_ > 0)
					node.add_number("station_type", station_type_);
				if (request_port_ > 0)
					node.add_number("request_port", request_port_);
				if (worker_in_port_ > 0)
					node.add_number("worker_in_port", worker_in_port_);
				if (worker_out_port_ > 0)
					node.add_number("worker_out_port", worker_out_port_);
			}
			if (type < 2)
			{
				node.add_number("request_in", request_in);
				node.add_number("request_out", request_out);
				node.add_number("request_err", request_err);
				node.add_number("worker_in", worker_in);
				node.add_number("worker_out", worker_out);
				node.add_number("worker_err", worker_err);
				acl::json_node& array = json.create_array();
				for (auto& worker : workers)
				{
					acl::json_node& work = json.create_node();
					work.add_number("level", worker.second.level);
					work.add_number("state", worker.second.state);
					work.add_number("pre_time", worker.second.pre_time);
					work.add_text("real_name", worker.second.real_name.c_str());
					work.add_text("ip_address", worker.second.ip_address.c_str());
					array.add_child(work);
				}
				node.add_child("workers", array);
			}
			return node.to_string();
		}
	}
}
