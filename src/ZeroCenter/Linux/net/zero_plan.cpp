#include "../stdafx.h"
#include "zero_plan.h"
#include "zero_station.h"
#include "inner_socket.h"
#include <boost/timer.hpp>
#include <boost/progress.hpp>
#include <boost/date_time/gregorian/gregorian.hpp>
#include <boost/date_time/posix_time/posix_time.hpp>
#include "boost/date_time/gregorian/greg_duration_types.hpp"
#include "boost/date_time/posix_time/ptime.hpp"
#define BOOST_DATE_TIME_OPTIONAL_GREGORIAN_TYPES
#define BOOST_DATE_TIME_SOURCE 
using namespace boost::posix_time;

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 保存消息
		*/
		bool plan_message::save_message() const
		{
			def_msg_key(key, this);
			redis_live_scope redis(json_config::redis_defdb);
			return redis->set(key, write_json().c_str());
		}

		/**
		* \brief 保存下一次执行时间
		*/
		bool plan_message::save()
		{
			redis_live_scope redis(json_config::redis_defdb);
			return save_next() && save_message();
		}

		/**
		* \brief 计划下一次执行时间
		* \return
		*/
		bool plan_message::plan_next()
		{
			redis_live_scope redis(json_config::redis_defdb);
			if (real_repet >= plan_repet)
			{
				remove();
				return false;
			}
			save();
			return true;
		}

		/**
		* \brief 保存下一次执行时间
		*/
		bool plan_message::save_next()
		{
			switch (plan_type)
			{
			case plan_date_type::time:
				if (plan_time < 0 || real_repet > 0)
					return false;
				plan_repet = 0; //不重复
				return true;
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
		* \brief 检查几号
		*/
		bool plan_message::check_month()
		{
			//无效设置,自动放弃
			if (plan_repet == 0 || (plan_repet > 0 && plan_repet <= real_repet) || plan_value > 31 || plan_value < -31)
			{
				remove();
				return false;
			}
			ptime now = second_clock::universal_time();
			int day;
			ushort max = boost::gregorian::gregorian_calendar::end_of_month_day(now.date().year(), now.date().month());
			if (plan_value > 0) //几号
				day = plan_value > max ? max : plan_value;
			else
				day = plan_value <= max ? 1 : max - plan_value;
			auto time = from_time_t(plan_time).time_of_day();
			if (day > now.date().day())
			{
				ptime next = ptime(
					boost::gregorian::date(now.date().year(), now.date().month(), static_cast<ushort>(day))) + time;
				plan_time = to_time_t(next);
			}
			else if (day < now.date().day() || time < now.time_of_day())
			{
				var y = now.date().year() + (now.date().month() == 12 ? 1 : 0);
				var m = now.date().month() == 12 ? 1 : now.date().month() + 1;
				ptime next = ptime(boost::gregorian::date(static_cast<ushort>(y), static_cast<ushort>(m),
					static_cast<ushort>(day))) + time;
				plan_time = to_time_t(next);
			}
			else
			{
				ptime next = ptime(
					boost::gregorian::date(now.date().year(), now.date().month(), static_cast<ushort>(day))) + time;
				plan_time = to_time_t(next);
			}
			return join_queue(plan_time);
		}

		/**
		* \brief 检查周几
		*/
		bool plan_message::check_week()
		{
			//无效设置,自动放弃
			if (plan_repet == 0 || (plan_repet > 0 && plan_repet <= real_repet) || plan_value < 0 || plan_value > 6)
			{
				remove();
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
			return join_queue(plan_time);
		}
		/**
		* \brief 检查延时
		*/
		bool plan_message::check_delay(time_duration delay)
		{
			//无效设置,自动放弃
			if (plan_repet == 0 || (plan_repet > 0 && plan_repet <= real_repet) || plan_value > 32767 || plan_value <= 0)
			{
				remove();
				return false;
			}
			ptime now = second_clock::universal_time();
			if (plan_time <= 0)
			{
				join_queue(to_time_t(now + delay));
				return true;
			}
			ptime time = from_time_t(plan_time);
			if (no_keep)
			{
				join_queue(to_time_t(time + delay));
				return true;
			}
			//空跳
			int cnt = real_repet;
			while (plan_repet < 0 || plan_repet > real_repet)
			{
				time += delay;
				if (time >= now)
				{
					join_queue(to_time_t(time));
					return true;
				}
				cnt++;
			}
			//结束仍在当前时间之前
			remove();
			return false;
		}

		/**
		* \brief 加入执行队列
		*/
		bool plan_message::join_queue(time_t time)
		{
			plan_time = time;
			def_msg_key(key, this);
			map<acl::string, double> value;
			value.insert(make_pair(key, static_cast<double>(time)));
			redis_live_scope redis(json_config::redis_defdb);
			int re = redis->zadd(zsco_key, value);
			return re >= 0;
		}

		/**
		* \brief 读取消息
		*/
		bool plan_message::load_message(const char* key)
		{
			redis_live_scope redis(json_config::redis_defdb);
			acl::string val;
			if (!redis->get(key, val) || val.empty())
			{
				return false;
			}
			read_json(val);
			return true;
		}


		/**
		* \brief 删除一个计划
		*/
		bool plan_message::remove() const
		{
			return remove_message();
		}

		/**
		* \brief 删除一个计划
		*/
		bool plan_message::remove_next() const
		{
			char id[MAX_PATH];
			sprintf(id, "%lld", plan_id);
			redis_live_scope redis(json_config::redis_defdb);
			return redis->zrem(zsco_key, id) >= 0;
		}

		/**
		* \brief 删除一个消息
		*/
		bool plan_message::remove_message() const
		{
			redis_live_scope redis(json_config::redis_defdb);

			//1 删除计划消息
			def_msg_key(key, this);
			redis->del(key);
			//2 移出执行队列
			if (plan_type > plan_date_type::none)
			{
				redis->zrem(zsco_key, key);
			}
			//3 删除执行结果
			def_msg_worker_key(wkey, this);
			redis->del(wkey);
			return true;
		}

		/**
		* \brief 保存消息参与者
		*/
		bool plan_message::save_message_worker(vector<const char*>& workers) const
		{
			redis_live_scope redis(json_config::redis_defdb);
			def_msg_worker_key(wkey, this);
			for (auto iter : workers)
			{
				if (!redis->hexists(wkey, iter))
				{
					redis->hset(wkey, iter, "[]");
				}
			}
			return true;
		}

		/**
		* \brief 保存消息参与者返回值
		*/
		bool plan_message::save_message_result(const char* worker, vector<shared_char>& response) const
		{
			def_msg_worker_key(wkey, this);

			acl::json json;
			acl::json_node& array = json.create_array();
			for (const auto& line : response)
			{
				array.add_array_text(*line);
			}
			redis_live_scope redis(json_config::redis_defdb);
			redis->hset(wkey, worker, array.to_string());
			return true;
		}

		/**
		* \brief 取一个参与者的消息返回值
		*/
		vector<shared_char> plan_message::get_message_result(const char* worker) const
		{
			def_msg_worker_key(wkey, this);
			vector<shared_char> result;
			acl::string val;
			{
				redis_live_scope redis(json_config::redis_defdb);
				if (!redis->hget(wkey, worker, val))
					return result;
			}
			acl::json json;
			json.update(val);
			var ch = json.first_node()->first_child();
			var iter_arr = ch->first_child();
			while (iter_arr)
			{
				result.emplace_back(iter_arr->get_text());
				iter_arr = ch->next_child();
			}
			return result;
		}

		/**
		* \brief 取一个参与者的消息返回值
		*/
		void plan_message::get_message_result(vector<shared_char>& result, acl::string& val)
		{
			acl::json json;
			json.update(val);
			var ch = json.first_node()->first_child();
			var iter_arr = ch->first_child();
			while (iter_arr)
			{
				result.emplace_back(iter_arr->get_text());
				iter_arr = ch->next_child();
			}
		}

		/**
		* \brief 取全部参与者消息返回值
		*/
		map<acl::string, vector<shared_char>> plan_message::get_message_result() const
		{
			redis_live_scope redis(json_config::redis_defdb);
			map<acl::string, vector<shared_char>> results;
			def_msg_worker_key(wkey, this);
			int cursor = 0;
			do
			{
				map<acl::string, acl::string> values;
				if (redis->hscan(wkey, cursor, values) < 0)
					break;
				for (pair<const acl::string, acl::string>& iter : values)
				{
					vector<shared_char> result;
					get_message_result(result, iter.second);
					results.insert(make_pair(iter.first, result));
				}
			} while (cursor > 0);
			return results;
		}

		/**
		* \brief 载入现在到期的内容
		*/
		void plan_message::exec_now(std::function<int(plan_message&)> exec)
		{
			vector<acl::string> keys;
			{//快速关闭Redis
				redis_live_scope redis(json_config::redis_defdb);
				redis->zrangebyscore(zsco_key, 0, static_cast<double>(time(nullptr)), &keys);
			}
			for (const acl::string& key : keys)
			{
				plan_message message;
				if (!message.load_message(key.c_str()))
					continue;
				switch (exec(message))
				{
				case 2:
					message.save_message();
					break;
				}
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
			"last_state",
			"description",
			"caller",
			"plan_time",
			"messages",
			"no_keep",
			"exec_time"
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
			last_state,
			description,
			caller,
			plan_time,
			messages,
			no_keep,
			exec_time
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
					plan_type = static_cast<plan_date_type>(static_cast<int>(*iter->get_int64()));
					break;
				case plan_fields_2::plan_value:
					plan_value = static_cast<int>(*iter->get_int64());
					break;
				case plan_fields_2::plan_repet:
					plan_repet = static_cast<int>(*iter->get_int64());
					break;
				case plan_fields_2::plan_time:
					plan_time = *iter->get_int64();
					break;
				case plan_fields_2::description:
					description = iter->get_string();
					break;
				case plan_fields_2::no_keep:
					no_keep = *iter->get_bool();
					break;
				default: break;
				}
				iter = json.next_node();
			}
		}
		/**
		* \brief JSON反序列化
		*/
		void plan_message::read_json(acl::string& val)
		{
			acl::json json;
			json.update(val);
			acl::json_node* iter = json.first_node();
			while (iter)
			{
				int index = strmatchi(iter->tag_name(), plan_fields_1);
				switch (static_cast<plan_fields_2>(index))
				{
				case plan_fields_2::plan_id:
					plan_id = static_cast<int>(*iter->get_int64());
					break;
				case plan_fields_2::plan_time:
					plan_time = *iter->get_int64();
					break;
				case plan_fields_2::exec_time:
					exec_time = *iter->get_int64();
					break;
				case plan_fields_2::plan_type:
					plan_type = static_cast<plan_date_type>(static_cast<int>(*iter->get_int64()));
					break;
				case plan_fields_2::plan_value:
					plan_value = static_cast<int>(*iter->get_int64());
					break;
				case plan_fields_2::plan_repet:
					plan_repet = static_cast<int>(*iter->get_int64());
					break;
				case plan_fields_2::real_repet:
					real_repet = static_cast<int>(*iter->get_int64());
					break;
				case plan_fields_2::station:
					station = iter->get_string();
					break;
				case plan_fields_2::request_id:
					request_id = iter->get_string();
					break;
				case plan_fields_2::last_state:
					last_state = static_cast<int>(*iter->get_int64());
					break;
				case plan_fields_2::description:
					description = iter->get_string();
					break;
				case plan_fields_2::caller:
					caller = iter->get_string();
					break;
				case plan_fields_2::messages:
				{
					var ch = iter->first_child();
					var iter_arr = ch->first_child();
					while (iter_arr)
					{
						messages.emplace_back(iter_arr->get_text());
						iter_arr = ch->next_child();
					}
				}
				break;
				case plan_fields_2::no_keep:
					no_keep = *iter->get_bool();
					break;

				default: break;
				}
				iter = json.next_node();
			}
		}

		/**
		* \brief JSON序列化
		*/
		acl::string plan_message::write_info() const
		{
			acl::json json;
			acl::json_node& node = json.create_node();
			node.add_number("plan_id", plan_id);
			node.add_text("description", *description);
			node.add_text("caller", *caller);
			if (!request_id.empty())
				node.add_text("request_id", *request_id);
			if (plan_type > plan_date_type::none)
			{
				node.add_number("plan_type", static_cast<int>(plan_type));
				node.add_number("plan_value", plan_value);
				node.add_number("plan_repet", plan_repet);
			}
			node.add_number("plan_time", plan_time);
			node.add_number("exec_time", exec_time);
			node.add_number("last_state", last_state);
			node.add_number("real_repet", real_repet);
			return node.to_string();
		}
		/**
		* \brief JSON序列化
		*/
		acl::string plan_message::write_json() const
		{
			acl::json json;
			acl::json_node& node = json.create_node();
			node.add_number("plan_id", plan_id);
			node.add_text("description", *description);
			node.add_text("station", *station);
			node.add_text("caller", *caller);
			if (!request_id.empty())
				node.add_text("request_id", *request_id);
			if (plan_type > plan_date_type::none)
			{
				node.add_number("plan_type", static_cast<int>(plan_type));
				node.add_number("plan_value", plan_value);
				node.add_number("plan_repet", plan_repet);
			}
			node.add_number("plan_time", plan_time);
			node.add_number("exec_time", exec_time);
			node.add_number("last_state", last_state);
			node.add_number("real_repet", real_repet);
			acl::json_node& array = json.create_array();
			for (const auto& line : messages)
			{
				array.add_array_text(*line);
			}
			node.add_child("messages", array);
			return node.to_string();
		}
	}
}
