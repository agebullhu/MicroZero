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
			"name"
			, "station_type"
			, "request_port"
			, "worker_out_port"
			, "worker_in_port"
			, "description"
			, "caption"
			, "station_alias"
			, "station_state"
			, "request_in"
			, "request_out"
			, "request_err"
			, "worker_in"
			, "worker_out"
			, "worker_err"
			, "short_name"
			,"is_base"
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
			, is_base
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
				if (tag == nullptr || tag[0] == 0 || iter->is_null())
				{
					iter = json.next_node();
					continue;
				}

				const int idx = strmatchi(tag, fields);
				switch (static_cast<config_fields>(idx))
				{
				case config_fields::station_name:
					station_name_ = iter->get_string();
					break;
				case config_fields::station_type:
					station_type_ = json_read_int(iter);
					break;
				case config_fields::short_name:
					short_name_ = iter->get_string();
					break;
				case config_fields::description:
					station_description_ = iter->get_string();
					break;
				case config_fields::caption:
					station_caption_ = iter->get_string();
					break;
				case config_fields::is_base:
					is_base = json_read_bool(iter);
					break;
				case config_fields::station_alias:
					alias_.clear();
					{
						var ch = iter->first_child();
						var iter_arr = ch->first_child();
						while (iter_arr)
						{
							auto txt = iter_arr->get_text();
							if (txt != nullptr)
								alias_.emplace_back(txt);
							iter_arr = ch->next_child();
						}
					}
					break;
				case config_fields::request_port:
					request_port_ = json_read_int(iter);
					break;
				case config_fields::worker_out_port:
					worker_out_port_ = json_read_int(iter);
					break;
				case config_fields::worker_in_port:
					worker_in_port_ = json_read_int(iter);
					break;
				case config_fields::station_state:
					station_state_ = static_cast<station_state>(json_read_num(iter));
					break;
				case config_fields::request_in:
					request_in = json_read_num(iter);
					break;
				case config_fields::request_out:
					request_out = json_read_num(iter);
					break;
				case config_fields::request_err:
					request_err = json_read_num(iter);
					break;
				case config_fields::worker_err:
					worker_err = json_read_num(iter);
					break;
				case config_fields::worker_in:
					worker_in = json_read_num(iter);
					break;
				case config_fields::worker_out:
					worker_out = json_read_num(iter);
					break;
				default: break;
				}
				iter = json.next_node();
			}
			check_type_name();
		}

		/**
		* \brief 写入JSON
		* \param type 记录类型 0 全量 1 动态信息 2 基本信息
		*/
		acl::string zero_config::to_json(int type)
		{
			boost::lock_guard<boost::mutex> guard(mutex_);
			acl::json json;
			acl::json_node& node = json.create_node();
			json_add_str(node, "name", station_name_);
			json_add_num(node, "station_state", static_cast<int>(station_state_));
			//站点基础信息,不包含在状态中
			if (type != 2)
			{
				json_add_bool(node, "is_base", is_base);
				json_add_str(node, "short_name", short_name_);
				json_add_str(node, "description", station_description_);
				json_add_num(node, "station_type", station_type_);
				json_add_num(node, "request_port", request_port_);
				json_add_num(node, "worker_in_port", worker_in_port_);
				json_add_num(node, "worker_out_port", worker_out_port_);
				if (alias_.size() > 0)
				{
					acl::json_node& array = json.create_array();
					for (auto alia : alias_)
					{
						json_add_array_str(array, alia);
					}
					node.add_child("station_alias", array);
				}
			}
			//站点计数,不包含在基础信息中
			if (type != 1)
			{
				json_add_num(node, "request_in", request_in);
				json_add_num(node, "request_out", request_out);
				json_add_num(node, "request_err", request_err);
				json_add_num(node, "worker_in", worker_in);
				json_add_num(node, "worker_out", worker_out);
				json_add_num(node, "worker_err", worker_err);
			}
			//加入的工作站点信息,仅包含在状态
			if (type >= 2 && workers.size() > 0)
			{
				acl::json_node& array = json.create_array();
				for (auto& worker : workers)
				{
					acl::json_node& work = json.create_node();
					json_add_num(work, "level", worker.second.level);
					json_add_num(work, "state", worker.second.state);
					json_add_num(work, "pre_time", worker.second.pre_time);
					json_add_str(work, "real_name", worker.second.real_name);
					json_add_str(work, "ip_address", worker.second.ip_address);
					array.add_child(work);
				}
				node.add_child("workers", array);
			}

			return node.to_string();
		}
	}
}
