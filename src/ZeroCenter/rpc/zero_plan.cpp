#include "../stdafx.h"
#include "zero_plan.h"
#include "zero_station.h"
#include "inner_socket.h"
#include "plan_dispatcher.h"
using namespace boost::posix_time;

namespace agebull
{
	namespace zmq_net
	{
		//map<string, shared_ptr<plan_message>> local_chche;
		/**
		* \brief 加入本地缓存
		*/
		void plan_message::add_local(shared_ptr<plan_message>& msg)
		{
			//while (local_chche.size() > static_cast<size_t>(json_config::plan_cache_size))
			//{
			//	local_chche.erase(local_chche.begin());
			//}
			//def_msg_key(key, msg);
			//local_chche[key] = msg;
		}

		/**
		* \brief 保存下一次执行时间
		*/
		bool plan_message::next()
		{
			redis_live_scope redis(json_config::redis_defdb);
			if (plan_state == plan_message_state::none)
			{
				save_message(true, false, false, false, false, false);
			}
			else if (skip_set == -2)
			{
				skip_set = 0;
				skip_num = 0;
				save_message(false, false, false, false, true, false);
			}
			if (check_next())
				return true;
			error();
			return false;
		}

		/**
		* \brief 保存下一次执行时间
		*/
		bool plan_message::check_next()
		{
			if (plan_repet == 0 || plan_time <= 0 || (skip_set > 0 && plan_repet > 0 && plan_repet <= skip_set))
			{
				return false;
			}
			switch (plan_type)
			{
			case plan_date_type::time:
				return check_time();
			case plan_date_type::second:
				return check_delay(seconds(plan_value));
			case plan_date_type::minute:
				return check_delay(minutes(plan_value));
			case plan_date_type::hour:
				return check_delay(hours(plan_value));
			case plan_date_type::day:
				return check_delay(hours(plan_value * 24));
			case plan_date_type::week:
				return check_week();
			case plan_date_type::month:
				return check_month();
			default:
				return false;
			}
		}

		/**
		* \brief 检查时间
		*/
		bool plan_message::check_time()
		{
			if (real_repet > 0)
			{
				close();
			}
			else if (plan_time <= 0)
			{
				join_queue(time(nullptr));
			}
			else if (no_skip) 
			{
				join_queue(plan_time);
			}
			else if (time(nullptr) > plan_time)
			{
				skip_num = 1;
				close();
			}
			else 
			{
				join_queue(time(nullptr));
			}
			return true;
		}

		/**
		* \brief 检查几号
		*/
		bool plan_message::check_month()
		{
			ptime now = second_clock::universal_time();
			int day;
			ushort max = boost::gregorian::gregorian_calendar::end_of_month_day(now.date().year(), now.date().month());
			if (plan_value > 0) //几号
				day = plan_value > max ? max : plan_value;
			else
			{
				int vl = 0 - plan_value;
				day = vl <= max ? 1 : max - vl;
			}
			auto time = from_time_t(plan_time).time_of_day();
			ptime next;
			if (day > now.date().day())
			{
				next = ptime(boost::gregorian::date(now.date().year(), now.date().month(), static_cast<ushort>(day))) + time;
			}
			else if (day < now.date().day() || time < now.time_of_day())
			{
				var y = now.date().year() + (now.date().month() == 12 ? 1 : 0);
				var m = now.date().month() == 12 ? 1 : now.date().month() + 1;
				next = ptime(boost::gregorian::date(static_cast<ushort>(y), static_cast<ushort>(m),static_cast<ushort>(day))) + time;
			}
			else
			{
				next = ptime(boost::gregorian::date(now.date().year(), now.date().month(), static_cast<ushort>(day))) + time;
			}
			join_queue(to_time_t(next));
			return true;
		}

		/**
		* \brief 检查周几
		*/
		bool plan_message::check_week()
		{
			//无效设置,自动放弃
			if (plan_value < 0 || plan_value > 6)
			{
				return false;
			}
			ptime now = second_clock::universal_time();
			auto time = from_time_t(plan_time).time_of_day();
			int wk = now.date().day_of_week();
			if (wk == plan_value) //当天
			{
				ptime timeTemp = ptime(now.date(), time);
				if (timeTemp < now) //时间未过
					plan_time = to_time_t(ptime(now.date() + boost::gregorian::days(7), time));
				else
					plan_time = to_time_t(timeTemp);
			}
			else if (wk < plan_value) //还没到
			{
				plan_time = to_time_t(ptime(now.date() + boost::gregorian::days(plan_value - wk), time));
			}
			else //过了
			{
				plan_time = to_time_t(ptime(now.date() + boost::gregorian::days(7 + plan_value - wk), time));
			}
			join_queue(plan_time);
			return true;
		}
		/**
		* \brief 检查延时
		*/
		bool plan_message::check_delay(time_duration delay)
		{
			//无效设置,自动放弃
			if (plan_value > 32767 || plan_value <= 0)
			{
				return false;
			}
			ptime now = second_clock::universal_time();
			if (plan_time <= 0)
			{
				join_queue(to_time_t(now + delay));
				return true;
			}
			ptime time = from_time_t(plan_time);
			if (no_skip)
			{
				join_queue(to_time_t(time + delay));
				return true;
			}
			//空跳
			int cnt = real_repet;
			while (plan_repet < 0 || plan_repet > cnt)
			{
				time += delay;
				if (time >= now)
				{
					join_queue(to_time_t(time));
					return true;
				}
				cnt++;
				++skip_num;
			}
			//结束仍在当前时间之前
			close();
			return true;
		}

		/**
		* \brief 加入执行队列
		*/
		void plan_message::join_queue(time_t time)
		{
			if (skip_set == 0 || (skip_set > 0 && skip_set < skip_num))
				plan_state = plan_message_state::queue;
			else
				plan_state = plan_message_state::skip;
			plan_time = time;
			def_msg_key(key, this);
			map<acl::string, double> value;
			value.insert(make_pair(key, static_cast<double>(time)));
			redis_live_scope redis(json_config::redis_defdb);
			redis->zadd(zsco_key, value);
			save_message(false, false, true, false, false, false);
		}

		/**
		* \brief 删除一个计划
		*/
		bool plan_message::error()
		{
			plan_state = plan_message_state::error;
			def_msg_key(key, this);
			redis_live_scope redis(json_config::redis_defdb);
			save_message(false, false, false, false, false, true);
			//local_chche.erase(key);
			redis->zrem(zsco_key, key);
			return true;
		}

		/**
		* \brief 恢复执行
		*/
		bool plan_message::reset()
		{
			plan_state = plan_message_state::none;
			redis_live_scope redis(json_config::redis_defdb);
			def_msg_key(key, this);
			redis.t()->set_hash_val(key, "plan_state", static_cast<int>(plan_state));
			join_queue(plan_time);
			return true;
		}

		/**
		* \brief 暂停执行
		*/
		bool plan_message::pause()
		{
			plan_state = plan_message_state::pause;
			redis_live_scope redis(json_config::redis_defdb);
			def_msg_key(key, this);
			redis.t()->set_hash_val(key, "plan_state", static_cast<int>(plan_state));
			redis->zrem(zsco_key, key);
			plan_dispatcher::instance->zero_event(zero_net_event::event_plan_pause, this);
			return true;
		}
		/**
		* \brief 关闭一个消息
		*/
		bool plan_message::close()
		{
			plan_state = plan_message_state::close;
			redis_live_scope redis(json_config::redis_defdb);
			def_msg_key(key, this);
			redis->zrem(zsco_key, key);
			save_message(false, false, false, false, false, true);
			//local_chche.erase(key);
			return true;
		}
		/**
		* \brief 删除一个消息
		*/
		bool plan_message::remove() const
		{
			redis_live_scope redis(json_config::redis_defdb);
			def_msg_key(key, this);
			redis->del(key);
			//local_chche.erase(key);
			plan_dispatcher::instance->zero_event(zero_net_event::event_plan_remove, this);
			return true;
		}

		/**
		* \brief 读取消息
		*/
		shared_ptr<plan_message> plan_message::load_message(const char* key)
		{
			//auto iter = local_chche.find(key);
			//if (iter != local_chche.end())
			//{
			//	return iter->second;
			//}
			//while (local_chche.size() > static_cast<size_t>(json_config::plan_cache_size))
			//{
			//	local_chche.erase(local_chche.begin());
			//}
			shared_ptr<plan_message> message = make_shared<plan_message>();
			//local_chche[key] = message;

			redis_live_scope scope(json_config::redis_defdb);
			trans_redis* redis = scope.t();

			redis->get_hash_val(key, "caller", message->caller);
			redis->get_hash_val(key, "request_id", message->request_id);
			redis->get_hash_val(key, "plan_id", message->plan_id);
			redis->get_hash_val(key, "description", message->description);
			redis->get_hash_val(key, "station", message->station);
			redis->get_hash_val(key, "command", message->command);
			redis->get_hash_val(key, "station_type", message->station_type);
			redis->get_hash_val(key, "no_skip", message->no_skip);
			message->plan_type = static_cast<plan_date_type>(redis->get_hash_num(key, "plan_type"));
			redis->get_hash_val(key, "plan_value", message->plan_value);
			redis->get_hash_val(key, "plan_repet", message->plan_repet);
			redis->get_hash_val(key, "add_time", message->add_time);

			size_t size = redis->get_hash_num(key, "frames");
			char skey[32];
			for (size_t idx = 1; idx <= size; idx++)
			{
				sprintf(skey, "frames:%d", idx);
				message->frames.emplace_back(redis->get_hash_ptr(key, skey));
			}

			redis->get_hash_val(key, "exec_time", message->exec_time);
			redis->get_hash_val(key, "exec_state", message->exec_state);
			message->plan_state = static_cast<plan_message_state>(redis->get_hash_num(key, "plan_state"));

			redis->get_hash_val(key, "plan_time", message->plan_time);
			redis->get_hash_val(key, "real_repet", message->real_repet);
			redis->get_hash_val(key, "skip_set", message->skip_set);
			redis->get_hash_val(key, "skip_num", message->skip_num);

			return message;
		}

		/**
		* \brief 设置跳过
		*/
		bool plan_message::set_skip(int set)
		{
			if (set < 0)
				skip_set = -1;
			else
				skip_set = set;
			save_message(false, false, false, false, true, false);
			return true;
		}
		/**
		* \brief 保存消息
		*/
		bool plan_message::save_message(bool full, bool exec, bool plan, bool res, bool skip, bool close)
		{
			def_msg_key(key, this);
			redis_live_scope scope(json_config::redis_defdb);
			trans_redis* redis = scope.t();
			if (add_time == 0)
			{
				full = true;
				add_time = time(nullptr);
				redis->set_hash_val(key, "add_time", static_cast<int64>(add_time));
			}
			if (full)
			{
				redis->set_hash_val(key, "caller", caller);
				redis->set_hash_val(key, "request_id", request_id);
				redis->set_hash_val(key, "plan_id", plan_id);
				redis->set_hash_val(key, "description", description);
				redis->set_hash_val(key, "station", station);
				redis->set_hash_val(key, "command", command);
				redis->set_hash_val(key, "station_type", static_cast<int>(station_type));
				redis->set_hash_val(key, "no_skip", no_skip);
				redis->set_hash_val(key, "plan_repet", plan_repet);
				redis->set_hash_val(key, "plan_type", static_cast<int>(plan_type));
				redis->set_hash_val(key, "plan_value", plan_value);

				redis->set_hash_val(key, "frames", frames.size());
				int idx = 0;
				char skey[32];
				for (const auto& line : frames)
				{
					sprintf(skey, "frames:%d", ++idx);
					redis->set_hash_val(key, skey, line);
				}
			}
			redis->set_hash_val(key, "plan_state", static_cast<int>(plan_state));
			if (full || plan || exec || res)
			{
				redis->set_hash_val(key, "skip_set", skip_set);
			}
			if (full || skip || plan || exec || res)
			{
				redis->set_hash_val(key, "skip_num", skip_num);
			}
			if (full || plan)
			{
				redis->set_hash_val(key, "plan_time", static_cast<int64>(plan_time));
			}

			if (full || exec || res)
			{
				redis->set_hash_val(key, "exec_time", static_cast<int64>(exec_time));
				redis->set_hash_val(key, "exec_state", exec_state);
				redis->set_hash_val(key, "real_repet", real_repet);
			}
			if (res || skip)
				return true;
			if (exec)
				plan_dispatcher::instance->zero_event(zero_net_event::event_plan_exec, this);
			else if (plan)
				plan_dispatcher::instance->zero_event(zero_net_event::event_plan_queue, this);
			else if (close)
				plan_dispatcher::instance->zero_event(zero_net_event::event_plan_end, this);
			else if (full)
				plan_dispatcher::instance->zero_event(zero_net_event::event_plan_add, this);
			else
				plan_dispatcher::instance->zero_event(zero_net_event::event_plan_update, this);
			return true;
		}

		/**
		* \brief 保存消息参与者
		*/
		bool plan_message::save_message_worker(vector<const char*>& workers) const
		{
			if (station_type != STATION_TYPE_VOTE)
				return false;
			redis_live_scope redis(json_config::redis_defdb);
			def_msg_key(key, this);
			char skey[64];
			for (auto iter : workers)
			{
				sprintf(skey, "wroks:%s:size", iter);
				if (!redis->hexists(key, skey))
				{
					redis->hset(key, skey, "0");
				}
			}
			return true;
		}

		/**
		* \brief 保存消息参与者返回值
		*/
		bool plan_message::save_message_result(const char* worker, vector<shared_char>& response)
		{
			if (station_type == STATION_TYPE_VOTE)
			{
				redis_live_scope scope(json_config::redis_defdb);
				trans_redis* redis = scope.t();
				def_msg_key(key, this);
				char skey[64];
				sprintf(skey, "wroks:%s:size", worker);
				size_t size = redis->get_hash_num(key, skey);

				for (size_t idx = 1; idx <= size; idx++)
				{
					sprintf(skey, "wroks:%s:%d", worker, idx);
					scope->hdel(key, skey);
				}
				sprintf(skey, "wroks:%s:size", worker);
				redis->set_hash_val(key, skey, response.size());

				for (size_t idx = 0; idx < response.size(); idx++)
				{
					sprintf(skey, "wroks:%s:%d", worker, idx + 1);
					redis->set_hash_val(key, worker, response[idx]);
				}
			}

			acl::string json;
			for (size_t idx = 0; idx < response.size(); idx++)
			{
				if (response[idx][0] < ' ')
					json.append(desc_str(false, response[idx].get_buffer(), response[idx].size()));
				else
					json.append(*response[idx]);
				json.append("\r\n");
			}
			log_trace3(DEBUG_RESULT, 2, "[plan](%lld) %s \n%s", plan_id, worker, json.c_str());

			save_message(false, false, false, true, false, false);
			return true;
		}

		/**
		* \brief 取一个参与者的消息返回值
		*/
		vector<shared_char> plan_message::get_message_result(const char* worker) const
		{
			vector<shared_char> result;
			if (station_type != STATION_TYPE_VOTE)
				return result;
			redis_live_scope scope(json_config::redis_defdb);
			trans_redis* redis = scope.t();
			def_msg_key(key, this);
			char skey[64];
			sprintf(skey, "wroks:%s:size", worker);
			size_t size = redis->get_hash_num(key, skey);

			for (size_t idx = 1; idx <= size; idx++)
			{
				sprintf(skey, "wroks:%s:%d", worker, idx);
				result.emplace_back(redis->get_hash_ptr(key, skey));
			}
			return result;
		}

		/**
		* \brief 取全部参与者消息返回值
		*/
		map<acl::string, vector<shared_char>> plan_message::get_message_result() const
		{
			redis_live_scope scope(json_config::redis_defdb);
			trans_redis* redis = scope.t();
			map<acl::string, vector<shared_char>> results;
			char skey[64];
			def_msg_key(key, this);
			int cursor = 0;
			do
			{
				map<acl::string, acl::string> values;
				cursor = scope->hscan(key, cursor, values, "wroks:*:size");
				for (pair<const acl::string, acl::string>& iter : values)
				{
					vector<shared_char> result;
					size_t size = atol(iter.second);
					acl::string worker;
					iter.first.substr(worker, 6, iter.first.length() - 11);
					for (size_t idx = 1; idx <= size; idx++)
					{
						sprintf(skey, "wroks:%s:%d", worker.c_str(), idx);
						result.emplace_back(redis->get_hash_ptr(key, skey));
					}
					results.insert(make_pair(worker, result));
				}
			} while (cursor > 0);
			return results;
		}

		/**
		* \brief 载入现在到期的内容
		*/
		void plan_message::exec_now(std::function<void(shared_ptr<plan_message>&)> exec)
		{
			vector<acl::string> keys;
			{
				//快速关闭Redis
				redis_live_scope redis(json_config::redis_defdb);
				redis->zrangebyscore(zsco_key, 0, static_cast<double>(time(nullptr)), &keys);
			}
			vector<acl::string> err_keys;
			for (const acl::string& key : keys)
			{
				shared_ptr<plan_message> message = load_message(key.c_str());
				if (!message)
				{
					err_keys.emplace_back(key);
					continue;
				}
				if (message->exec_state == ZERO_STATUS_WAIT_ID)
				{
					++message->skip_num;
					message->save_message(false, false, false, false, true, false);
					continue;
				}
				else if (message->plan_state == plan_message_state::pause)
				{
					continue;
				}
				if (message->exec_state == ZERO_STATUS_PLAN_ERROR_ID || message->plan_id == 0 || message->frames.size() == 0)
				{
					plan_dispatcher::instance->get_config().error("error plan state remove from plan queue", *message->frames[0]);
					message->error();
					continue;
				}

				if (message->exec_state == ZERO_STATUS_RUNING_ID)
				{
					var span = second_clock::universal_time() - from_time_t(message->exec_time);
					//超时未到且还未执行完成,不重复下发
					if (span.seconds() < json_config::plan_exec_timeout)
					{
						plan_dispatcher::instance->get_config().error("plan delay to short", *message->frames[0]);
						continue;
					}
					//设置重试
					message->plan_state = plan_message_state::retry;
					message->skip_set = -2;
					message->skip_num = 1;
				}
				else if (message->skip_set == -2)
				{
					++message->skip_num;
				}
				else if (message->skip_set == -1 || (message->skip_set > 0 && message->skip_set > message->skip_num))
				{
					++message->skip_num;
					message->next();
					continue;
				}
				exec(message);
			}
			{
				redis_live_scope redis(json_config::redis_defdb);
				for (const acl::string& key : err_keys)
					redis->zrem(zsco_key, key);
			}
		}


		const char* plan_fields_1[] =
		{
			"plan_id",
			"plan_type",
			"plan_value",
			"plan_repet",
			"real_repet",
			"station",
			"request_id",
			"exec_state",
			"description",
			"caller",
			"plan_time",
			"frames",
			"no_skip",
			"exec_time",
			"skip_set",
			"skip_num",
			"station_type",
			"plan_state"
		};

		enum class plan_fields_2
		{
			plan_id,
			plan_type,
			plan_value,
			plan_repet,
			real_repet,
			station,
			request_id,
			exec_state,
			description,
			caller,
			plan_time,
			frames,
			no_skip,
			exec_time,
			skip_set,
			skip_num,
			station_type,
			plan_state
		};

		/**
		* \brief JSON读计划基本信息
		*/
		void plan_message::read_plan(const char* plan)
		{
			acl::json json;
			json.update(plan);
			acl::json_node* iter = json.first_node();
			while (iter)
			{
				int index = strmatchi(iter->tag_name(), plan_fields_1);
				switch (static_cast<plan_fields_2>(index))
				{
				case plan_fields_2::plan_type:
					plan_type = static_cast<plan_date_type>(json_read_int(iter));
					break;
				case plan_fields_2::plan_value:
					plan_value = json_read_int(iter);
					break;
				case plan_fields_2::plan_repet:
					plan_repet = json_read_int(iter);
					break;
				case plan_fields_2::plan_time:
					plan_time = json_read_num(iter);
					break;
				case plan_fields_2::description:
					description = iter->get_string();
					break;
				case plan_fields_2::no_skip:
					no_skip = *iter->get_bool();
					break;
				case plan_fields_2::skip_set:
					skip_set = json_read_int(iter);
					break;
				default: break;
				}
				iter = json.next_node();
			}
		}

		/**
		* \brief JSON序列化
		*/
		void plan_message::write_info(acl::json_node& node) const
		{
			json_add_str(node, "caller", caller);
			json_add_num(node, "plan_id", plan_id);
			json_add_str(node, "caller", caller);
			json_add_str(node, "request_id", request_id);
			json_add_num(node, "plan_type", static_cast<int>(plan_type));
			json_add_num(node, "no_skip", no_skip);
			json_add_num(node, "plan_value", plan_value);
			json_add_num(node, "plan_repet", plan_repet);
			json_add_num(node, "plan_time", plan_time);
			json_add_num(node, "real_repet", real_repet);
			json_add_num(node, "skip_set", skip_set);
			json_add_num(node, "skip_num", skip_num);
			json_add_num(node, "exec_time", exec_time);
			json_add_num(node, "exec_state", exec_state);
			json_add_num(node, "plan_state", static_cast<int>(plan_state));
		}

		/**
		* \brief JSON序列化
		*/
		acl::string plan_message::write_info() const
		{
			acl::json json;
			acl::json_node& node = json.create_node();
			write_info(node);
			return node.to_string();
		}

		/**
		* \brief JSON序列化
		*/
		acl::string plan_message::write_state() const
		{
			acl::json json;
			acl::json_node& node = json.create_node();
			json_add_num(node, "plan_id", plan_id);
			json_add_num(node, "exec_time", exec_time);
			json_add_num(node, "exec_state", exec_state);
			json_add_num(node, "plan_state", static_cast<int>(plan_state));
			json_add_num(node, "plan_time", plan_time);
			json_add_num(node, "real_repet", real_repet);
			json_add_num(node, "skip_set", skip_set);
			json_add_num(node, "skip_num", skip_num);
			return node.to_string();
		}

		/**
		* \brief JSON序列化
		*/
		acl::string plan_message::write_json() const
		{
			acl::json json;
			acl::json_node& node = json.create_node();
			json_add_str(node, "description", description);
			json_add_str(node, "station", station);
			json_add_num(node, "station_type", station_type);
			json_add_str(node, "command", command);
			write_info(node);
			acl::json_node& array = json.create_array();
			for (const auto& line : frames)
			{
				if (line.empty())
					array.add_array_null();
				else if (line[0] >= ' ')
					array.add_array_text(*line);
				else
					array.add_array_text(desc_str(false, line.get_buffer(), line.size()));
			}
			node.add_child("frames", array);
			return node.to_string();
		}
	}
}
