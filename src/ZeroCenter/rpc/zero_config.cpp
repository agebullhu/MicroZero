#include "../stdinc.h"
#include "zero_config.h"
#include "../ext/shared_char.h"
#include "../cfg/json_config.h"

namespace agebull
{
	namespace zero_net
	{
		int station_config::check_worker(station_worker& worker)
		{
			if (worker.worker_state == 5)
				return -1;
			var ms = boost::posix_time::microsec_clock::local_time() - worker.worker_last;
			var tm = ms.total_milliseconds() / global_config::worker_sound_ivl;
			

			if (tm <= 1)
			{
				worker.worker_state = 1;
				return worker.worker_health = 5;
			}
			if (tm <= 2)
			{
				worker.worker_state = 2;
				return worker.worker_health = 4;
			}
			if (tm <= 4)
			{
				worker.worker_state = 2;
				return worker.worker_health = 3;
			}
			worker.worker_state = 3;
			if (tm <= 8)
			{
				return worker.worker_health = 2;
			}
			if (tm <= 16)
			{
				return worker.worker_health = 1;
			}
			if (tm <= 32)
			{
				return worker.worker_health = 0;
			}
			return worker.worker_health = -1;
		}


		void station_config::worker_join(const char* worker_name, const char* ip)
		{
			boost::lock_guard<boost::mutex> guard(mutex);
			bool hase = false;
			for (station_worker& iter : workers)
			{
				if (strcmp(iter.worker_name, worker_name) == 0)
				{
					if (iter.worker_state > 2)//曾经失联
						log("worker_join*reincarnation ", worker_name);
					iter.active();
					hase = true;
					break;
				}
			}
			if (!hase)
			{
				station_worker wk;
				memset(wk.worker_name, 0, sizeof(wk.worker_name));
				strcpy(wk.worker_name, worker_name);
				wk.worker_state = 1;
				wk.worker_health = 5;
				wk.active();
				workers.push_back(wk);
				++ready_works_;
				log("worker_join", worker_name);
			}
		}


		void station_config::worker_heartbeat(const char* worker_name)
		{
			boost::lock_guard<boost::mutex> guard(mutex);
			for (station_worker& iter : workers)
			{
				if (strstr(iter.worker_name, worker_name) != nullptr)
				{
					//if (iter.worker_state > 2)//曾经失联
					//	log("worker_ready*reincarnation ", iter.worker_name);
					iter.active();
				}
				else
				{
					check_worker(iter);
				}
			}
		}
		void station_config::worker_ready(const char* worker_name)
		{
			boost::lock_guard<boost::mutex> guard(mutex);
			bool hase = false;
			for (station_worker& iter : workers)
			{
				if (strcmp(iter.worker_name, worker_name) == 0)
				{
					if (iter.worker_state > 2)//曾经失联
						log("worker_ready*reincarnation ", worker_name);
					iter.active();
					hase = true;
					break;
				}
			}
			if (!hase)
			{
				station_worker wk;
				memset(&wk, 0, sizeof(wk));
				strcpy(wk.worker_name, worker_name);
				wk.active();
				workers.push_back(wk);
				log("worker_ready", worker_name);
			}
		}

		/**
		* \brief 站点名称
		*/
		station_worker* station_config::get_workers()
		{
			{
				boost::lock_guard<boost::mutex> guard(mutex);
				int size = static_cast<int>(workers.size());
				if (size == 0)
				{
					worker_idx_ = -1;
					return nullptr;
				}
				if (size == 1)
				{
					worker_idx_ = 0;
					if (workers[0].worker_state < 5)
						return &workers[0];
					workers.erase(workers.begin());
					return nullptr;
				}
				if (++worker_idx_ >= size)
				{
					worker_idx_ = 0;
				}
				if (workers[worker_idx_].worker_state < 5)
					return &workers[worker_idx_];
				workers.erase(workers.begin() + worker_idx_);
			}
			return get_workers();
		}
		void station_config::worker_left(const char* worker_name)
		{
			if (workers.size() == 0)
				return;

			boost::lock_guard<boost::mutex> guard(mutex);
			vector<station_worker> array = workers;
			for (int i = static_cast<int>(array.size()) - 1; i >= 0; --i)
			{
				if (strstr(array[i].worker_name, worker_name) != nullptr)
				{
					workers.erase(workers.begin() + i);
					log("worker_left", array[i].worker_name);
				}
			}
		}

		void station_config::check_works()
		{
			if (workers.size() == 0)
				return;
			boost::lock_guard<boost::mutex> guard(mutex);
			ready_works_ = 0;
			vector<station_worker> array = workers;
			for (int i = static_cast<int>(array.size()) - 1; i >= 0; --i)
			{
				check_worker(workers[i]);
				if (workers[i].worker_state <= 2)
				{
					++ready_works_;
				}
				else if (workers[i].worker_health < 1)
				{
					log("station_worker failed", array[i].worker_name);
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
			//, "station_alias"
			, "station_state"
			, "request_in"
			, "request_out"
			, "request_err"
			, "request_deny"
			, "worker_in"
			, "worker_out"
			, "worker_err"
			, "worker_deny"
			//, "short_name"
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
			//, station_alias
			, station_state
			, request_in
			, request_out
			, request_err
			, request_deny
			, worker_in
			, worker_out
			, worker_err
			, worker_deny
			//, short_name
		};
		void station_config::read_json(const char* val)
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
				//case config_fields::short_name:
				//	short_name = iter->get_string();
				//	break;
				case config_fields::description:
					station_description = iter->get_string();
					break;
				case config_fields::caption:
					station_caption = iter->get_string();
					break;
				//case config_fields::station_alias:
				//	alias.clear();
				//	{
				//		var ch = iter->first_child();
				//		var iter_arr = ch->first_child();
				//		while (iter_arr)
				//		{
				//			auto txt = iter_arr->get_text();
				//			if (txt != nullptr)
				//				alias.emplace_back(txt);
				//			iter_arr = ch->next_child();
				//		}
				//	}
				//	break;
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
				case config_fields::request_deny:
					request_deny = json_read_num(iter);
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
				case config_fields::worker_deny:
					worker_deny = json_read_num(iter);
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
		acl::string station_config::to_json(int type)
		{
			boost::lock_guard<boost::mutex> guard(mutex);
			acl::json json;
			acl::json_node& node = json.create_node();

			json_add_str(node, "name", station_name);
			//站点基础信息,不包含在状态中
			if (type != 2)
			{
				json_add_num(node, "station_type", station_type);
				json_add_str(node, "station_address", station_address);
				json_add_num(node, "request_port", request_port);
				json_add_num(node, "worker_in_port", worker_in_port);
				json_add_num(node, "worker_out_port", worker_out_port);
				json_add_bool(node, "is_base", is_base);
				json_add_str(node, "description", station_description);
				//node.add_text("short_name", short_name.empty() ? station_name.c_str() : short_name.c_str());
				//if (alias.size() > 0)
				//{
				//	acl::json_node& array = json.create_array();
				//	for (auto alia : alias)
				//	{
				//		json_add_array_str(array, alia);
				//	}
				//	node.add_child("station_alias", array);
				//}
			}
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
				for (auto& station_worker : workers)
				{
					acl::json_node& work = json.create_node();
					json_add_chr(work, "worker_name", station_worker.worker_name);
					json_add_num(work, "worker_health", station_worker.worker_health);
					json_add_num(work, "worker_state", station_worker.worker_state);
					json_add_num(work, "worker_last", to_time_t(station_worker.worker_last));
					json_add_str(work, "worker_ip", station_worker.worker_ip);
					array.add_child(work);
				}
				node.add_child("workers", array);
			}

			json_add_num(node, "station_state", static_cast<int>(station_state_));
			return node.to_string();
		}
	}
}
