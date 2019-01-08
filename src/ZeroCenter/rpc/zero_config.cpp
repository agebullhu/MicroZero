#include "../stdinc.h"
#include "zero_config.h"
#include "../ext/shared_char.h"
#include "../cfg/json_config.h"

namespace agebull
{
	namespace zero_net
	{
		int zero_config::check_worker(worker& worker)
		{
			if (worker.state == 5)
				return -1;
			const int64 tm = time(nullptr) - worker.pre_time;

			if (tm <= 2)
			{
				worker.state = 1;
				return worker.level = 5;
			}
			if (tm <= 4)
			{
				worker.state = 2;
				return worker.level = 4;
			}
			if (tm <= 8)
			{
				return worker.level = 3;
			}
			worker.state = 3;
			if (tm <= 16)
			{
				return worker.level = 2;
			}
			if (tm <= 32)
			{
				return worker.level = 1;
			}
			if (tm <= 60)
			{
				return worker.level = 0;
			}
			return worker.level = -1;
		}

		void zero_config::worker_join(const char* identity, const char* ip)
		{
			{
				boost::lock_guard<boost::mutex> guard(mutex);
				bool hase=false;
				for (worker& iter : workers)
				{
					if (strcmp(iter.identity, identity) == 0)
					{
						if (iter.state > 2)//曾经失联
							log("worker_join*reincarnation ", identity);
						iter.active();
						iter.state = 1;
						hase = true;
						break;
					}
				}
				if(!hase)
				{
					worker wk;
					memset(wk.identity, 0, sizeof(wk.identity));
					strcpy(wk.identity, identity);
					wk.state = 1;
					wk.level = 5;
					wk.active();
					workers.push_back(wk);
					++ready_works_;
					log("worker_join", identity);
				}
			}
			check_works();
		}

		void zero_config::worker_ready(const char* identity)
		{
			{
				boost::lock_guard<boost::mutex> guard(mutex);
				bool hase = false;
				for (worker& iter : workers)
				{
					if (strcmp(iter.identity, identity) == 0)
					{
						if (iter.state > 2)//曾经失联
							log("worker_ready*reincarnation ", identity);
						iter.active();
						hase = true;
						break;
					}
				}
				if (!hase)
				{
					worker wk;
					memset(&wk, 0, sizeof(wk));
					strcpy(wk.identity, identity);
					wk.active();
					workers.push_back(wk);
					log("worker_ready", identity);
				}
			}
			check_works();
		}

		/**
		* \brief 站点名称
		*/
		worker* zero_config::get_worker()
		{
			{
				boost::lock_guard<boost::mutex> guard(mutex);
				int size = static_cast<int>(workers.size());
				if (size == 0)
				{
					worker_idx = -1;
					return nullptr;
				}
				if (size == 1)
				{
					worker_idx = 0;
					if (workers[0].state < 5)
						return &workers[0];
					workers.erase(workers.begin());
					return nullptr;
				}
				if (++worker_idx >= size)
				{
					worker_idx = 0;
				}
				if (workers[worker_idx].state < 5)
					return &workers[worker_idx];
				workers.erase(workers.begin() + worker_idx);
			}
			return get_worker();
		}
		void zero_config::worker_left(const char* identity)
		{
			if (workers.size() == 0)
				return;

			boost::lock_guard<boost::mutex> guard(mutex);
			vector<worker> array = workers;
			for (int i = static_cast<int>(array.size())-1; i >= 0; --i)
			{
				if (strcmp(array[i].identity, identity) == 0)
				{
					workers.erase(workers.begin() + i);
					log("worker_left", identity);
				}
			}
		}

		void zero_config::check_works()
		{
			if (workers.size() == 0)
				return;
			boost::lock_guard<boost::mutex> guard(mutex);
			ready_works_ = 0;
			vector<worker> array = workers;
			for (int i = static_cast<int>(array.size()) - 1; i >= 0; --i)
			{
				check_worker(workers[i]);
				if (workers[i].state <= 2)
				{
					++ready_works_;
				}
				else if (workers[i].level < 1)
				{
					log("worker failed", array[i].identity);
					workers.erase(workers.begin() + i);
				}
			}
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
			boost::lock_guard<boost::mutex> guard(mutex);
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
					station_name = iter->get_string();
					break;
				case config_fields::station_type:
					station_type = json_read_int(iter);
					break;
				case config_fields::short_name:
					short_name = iter->get_string();
					break;
				case config_fields::description:
					station_description = iter->get_string();
					break;
				case config_fields::caption:
					station_caption = iter->get_string();
					break;
				case config_fields::is_base:
					is_base = false;// json_read_bool(iter);
					break;
				case config_fields::station_alias:
					alias.clear();
					{
						var ch = iter->first_child();
						var iter_arr = ch->first_child();
						while (iter_arr)
						{
							auto txt = iter_arr->get_text();
							if (txt != nullptr)
								alias.emplace_back(txt);
							iter_arr = ch->next_child();
						}
					}
					break;
				case config_fields::request_port:
					request_port = json_read_int(iter);
					break;
				case config_fields::worker_out_port:
					worker_out_port = json_read_int(iter);
					break;
				case config_fields::worker_in_port:
					worker_in_port = json_read_int(iter);
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
			boost::lock_guard<boost::mutex> guard(mutex);
			acl::json json;
			acl::json_node& node = json.create_node();
			//站点基础信息,不包含在状态中
			if (type != 2)
			{
				json_add_bool(node, "is_base", is_base);
				json_add_num(node, "station_type", station_type);
				json_add_num(node, "request_port", request_port);
				json_add_num(node, "worker_in_port", worker_in_port);
				json_add_num(node, "worker_out_port", worker_out_port);
			}
			json_add_str(node, "name", station_name);
			//站点基础信息,不包含在状态中
			if (type != 2)
			{
				json_add_str(node, "short_name", short_name);
				json_add_str(node, "description", station_description);
				if (alias.size() > 0)
				{
					acl::json_node& array = json.create_array();
					for (auto alia : alias)
					{
						json_add_array_str(array, alia);
					}
					node.add_child("station_alias", array);
				}
			}
			json_add_num(node, "station_state", static_cast<int>(station_state_));
			//站点计数,不包含在基础信息中
			if (type != 1)
			{
				json_add_num(node, "request_in", request_in);
				json_add_num(node, "request_out", request_out);
				json_add_num(node, "request_err", request_err);
				json_add_num(node, "request_deny", request_deny);

				json_add_num(node, "worker_in", worker_in);
				json_add_num(node, "worker_out", worker_out);
				json_add_num(node, "worker_err", worker_err);
				json_add_num(node, "worker_deny", worker_deny);
			}
			//加入的工作站点信息,仅包含状态
			if (type >= 2 && workers.size() > 0)
			{
				acl::json_node& array = json.create_array();
				for (auto& worker : workers)
				{
					acl::json_node& work = json.create_node();
					json_add_chr(work, "real_name", worker.identity);
					json_add_num(work, "level", worker.level);
					json_add_num(work, "state", worker.state);
					json_add_num(work, "pre_time", worker.pre_time);
					json_add_str(work, "ip_address", worker.ip_address);
					array.add_child(work);
				}
				node.add_child("workers", array);
			}

			return node.to_string();
		}
	}
}
