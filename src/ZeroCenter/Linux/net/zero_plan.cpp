#include "../stdafx.h"
#include "zero_plan.h"
#include "zero_station.h"
#include "ipc_request_socket.h"
#include <boost/timer.hpp>
#include <boost/progress.hpp> 
#include <boost/date_time/gregorian/gregorian.hpp>
#include <boost/date_time/posix_time/posix_time.hpp> 
#include "boost/date_time/gregorian/greg_duration_types.hpp"
#include "boost/date_time/posix_time/ptime.hpp"
#define BOOST_DATE_TIME_OPTIONAL_GREGORIAN_TYPES
#define BOOST_DATE_TIME_SOURCE 
using namespace ::boost::posix_time;
namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 保存消息
		*/
		bool plan_message::save_message()
		{
			def_msg_key(key, this);
			redis_live_scope redis(REDIS_DB_ZERO_MESSAGE);
			return redis->set(key, write_json().c_str());
		}

		/**
		* \brief 保存下一次执行时间
		*/
		bool plan_message::save()
		{
			redis_live_scope redis(REDIS_DB_ZERO_MESSAGE);
			return save_message() && save_next();
		}

		/**
		* \brief 计划下一次执行时间
		* \return
		*/
		bool plan_message::plan_next()
		{
			redis_live_scope redis(REDIS_DB_ZERO_MESSAGE);
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
			//无效时间,自动放弃
			if (plan_value > 32767 || plan_value < -32767)
			{
				remove();
				return false;
			}
			ptime now = second_clock::local_time();

			switch (plan_type)
			{
			default:
				plan_time = time_t(nullptr);//现在,会立即开始
				plan_repet = 0;//不重复
				break;
			case plan_date_type::time:
				if (plan_time > 0)
				{
					if (from_time_t(plan_time) < now && !no_keep)
					{
						remove();
						return false;//无效时间,自动放弃
					}
				}
				else
				{
					plan_time = to_time_t(now);
				}
				plan_repet = 0;//不重复
				break;
			case plan_date_type::second:
				if (plan_repet == 0 || plan_value < 0)
				{
					remove();
					return false;
				}
				if (plan_time <= 0)
				{
					plan_time = to_time_t(now + seconds(plan_value));
					break;
				}
				if (!check_delay(now, seconds(plan_value)))
				{
					return false;
				}
				break;
			case plan_date_type::minute:
				if (plan_repet == 0 || plan_value < 0)
				{
					remove();
					return false;
				}
				if (plan_time <= 0)
				{
					plan_time = to_time_t(now + minutes(plan_value));
					break;
				}
				if (!check_delay(now, minutes(plan_value)))
				{
					return false;
				}
				break;
			case plan_date_type::hour:
				if (plan_repet == 0 || plan_value < 0)
				{
					remove();
					return false;
				}
				if (plan_time <= 0)
				{
					plan_time = to_time_t(now + hours(plan_value));
					break;
				}
				if (!check_delay(now, hours(plan_value)))
				{
					return false;
				}
				break;
			case plan_date_type::day:
				if (plan_repet == 0 || plan_value < 0)
				{
					remove();
					return false;
				}
				if (plan_time <= 0)
				{
					plan_time = to_time_t(now + boost::gregorian::days(plan_value));
					break;
				}
				if (!check_delay(now, hours(plan_value * 24)))
				{
					return false;
				}

				break;
			case plan_date_type::week:
			{
				if (plan_value < 0 || plan_value > 6)
				{
					remove();
					return false;//无效时间,自动放弃
				}
				auto time = from_time_t(plan_time).time_of_day();
				int wk = now.date().day_of_week();
				if (wk == plan_value)//当天
				{
					ptime timeTemp = ptime(now.date(), time);
					if (timeTemp < now)//时间未过
						plan_time = to_time_t(ptime(now.date() + boost::gregorian::days(7), time));
					else
						plan_time = to_time_t(timeTemp);
				}
				else if (wk < plan_value)//还没到
				{
					plan_time = to_time_t(ptime(now.date() + boost::gregorian::days(plan_value - wk), time));
				}
				else//过了
				{
					plan_time = to_time_t(ptime(now.date() + boost::gregorian::days(7 + plan_value - wk), time));
				}
			}
			break;
			case plan_date_type::month:
			{
				int day;
				ushort max = boost::gregorian::gregorian_calendar::end_of_month_day(now.date().year(), now.date().month());
				if (plan_value > 0)//几号
					day = plan_value > max ? max : plan_value;
				else
					day = plan_value <= max ? 1 : max - plan_value;
				auto time = from_time_t(plan_time).time_of_day();
				if (day > now.date().day())
				{
					ptime next = ptime(boost::gregorian::date(now.date().year(), now.date().month(), static_cast<ushort>(day))) + time;
					plan_time = to_time_t(next);
				}
				else if (day < now.date().day() || time < now.time_of_day())
				{
					var y = now.date().year() + (now.date().month() == 12 ? 1 : 0);
					var m = now.date().month() == 12 ? 1 : now.date().month() + 1;
					ptime next = ptime(boost::gregorian::date(static_cast<ushort>(y), static_cast<ushort>(m), static_cast<ushort>(day))) + time;
					plan_time = to_time_t(next);
				}
				else
				{
					ptime next = ptime(boost::gregorian::date(now.date().year(), now.date().month(), static_cast<ushort>(day))) + time;
					plan_time = to_time_t(next);
				}
			}
			break;
			}
			return join_queue(plan_time);
		}

		/**
		* \brief 检查延时
		*/
		bool plan_message::check_delay(boost::posix_time::ptime& now, boost::posix_time::time_duration delay)
		{
			boost::posix_time::ptime time = from_time_t(plan_time);
			int cnt = real_repet;
			while (plan_repet < 0 || plan_repet > cnt)
			{
				time += delay;
				plan_time = to_time_t(time);
				if (time >= now)
				{
					plan_time = to_time_t(time);
					return true;
				}
				cnt++;
				if (no_keep)
					join_queue(plan_time);
			}
			plan_time = to_time_t(time);
			return no_keep;
		}

		/**
		* \brief 加入执行队列
		*/
		bool plan_message::join_queue(time_t time) const
		{
			def_msg_key(key, this);
			map<acl::string, double> value;
			value.insert(make_pair(key, static_cast<double>(time)));
			redis_live_scope redis(REDIS_DB_ZERO_MESSAGE);
			return redis->zadd(zsco_key, value) >= 0;
		}

		/**
		* \brief 读取消息
		*/
		bool plan_message::load_message(const char* key)
		{
			redis_live_scope redis(REDIS_DB_ZERO_MESSAGE);
			acl::string val;
			if (!redis->get(key, val) || val.empty())
			{
				cout << endl << key << " =>  empty";
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
			return remove_next() && remove_message();
		}

		/**
		* \brief 删除一个计划
		*/
		bool plan_message::remove_next() const
		{
			char id[MAX_PATH];
			sprintf(id, "%lld", plan_id);
			redis_live_scope redis(REDIS_DB_ZERO_MESSAGE);
			return redis->zrem(zsco_key, id) >= 0;
		}

		/**
		* \brief 删除一个消息
		*/
		bool plan_message::remove_message() const
		{
			redis_live_scope redis(REDIS_DB_ZERO_MESSAGE);

			//1 删除消息
			def_msg_key(key, this);
			redis->del(key);
			//2 删除计划
			if (plan_type > plan_date_type::none)
			{
				redis->zrem(zsco_key, key);
			}
			//3 删除参与者
			def_msg_worker_key(wkey, this);
			redis->del(wkey);
			return true;
		}

		/**
		* \brief 保存消息参与者
		*/
		bool plan_message::save_message_worker(vector<const char*>& workers) const
		{
			redis_live_scope redis(REDIS_DB_ZERO_MESSAGE);
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
			redis_live_scope redis(REDIS_DB_ZERO_MESSAGE);
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
				redis_live_scope redis(REDIS_DB_ZERO_MESSAGE);
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
			redis_live_scope redis(REDIS_DB_ZERO_MESSAGE);
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
		void plan_message::load_now(std::function<int(plan_message&)> exec)
		{
			vector<acl::string> keys;
			redis_live_scope redis(REDIS_DB_ZERO_MESSAGE);
			if (redis->exists(zsco_key))
			{
				redis->zrangebyscore(zsco_key, 0, static_cast<double>(time(nullptr)), &keys);
				for (const acl::string& key : keys)
				{
					plan_message message;
					message.load_message(key.c_str());
					if (message.last_state == ZERO_STATUS_FRAME_PLANERROR_ID)
						continue;
					switch(exec(message))
					{
					case 1:
						message.save_next();
					case 2:
						message.save_message();
						break;
					}
				}
			}
		}

		const char* fields[] =
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

		enum class plan_fields
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
				switch (static_cast<plan_fields>(strmatchi(iter->tag_name(), fields)))
				{
				case plan_fields::plan_type:
					plan_type = static_cast<plan_date_type>(static_cast<int>(*iter->get_int64()));
					break;
				case plan_fields::plan_value:
					plan_value = static_cast<int>(*iter->get_int64());
					break;
				case plan_fields::plan_repet:
					plan_repet = static_cast<int>(*iter->get_int64());
					break;
				case plan_fields::plan_time:
					plan_time = *iter->get_int64();
					break;
				case plan_fields::description:
					description = iter->get_string();
					break;
				case plan_fields::no_keep:
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
				switch (static_cast<plan_fields>(strmatchi(iter->tag_name(), fields)))
				{
				case plan_fields::plan_id:
					plan_id = static_cast<int>(*iter->get_int64());
					break;
				case plan_fields::plan_time:
					plan_time = *iter->get_int64();
					break;
				case plan_fields::exec_time:
					exec_time = *iter->get_int64();
					break; 
				case plan_fields::plan_type:
					plan_type = static_cast<plan_date_type>(static_cast<int>(*iter->get_int64()));
					break;
				case plan_fields::plan_value:
					plan_value = static_cast<int>(*iter->get_int64());
					break;
				case plan_fields::plan_repet:
					plan_repet = static_cast<int>(*iter->get_int64());
					break;
				case plan_fields::real_repet:
					real_repet = static_cast<int>(*iter->get_int64());
					break;
				case plan_fields::station:
					station = iter->get_string();
					break;
				case plan_fields::request_id:
					request_id = iter->get_string();
					break;
				case plan_fields::last_state:
					last_state = static_cast<int>(*iter->get_int64());
					break;
				case plan_fields::description:
					description = iter->get_string();
					break;
				case plan_fields::caller:
					caller = iter->get_string();
					break;
				case plan_fields::messages:
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
				case plan_fields::no_keep:
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
		acl::string plan_message::write_json() const
		{
			acl::json json;
			acl::json_node& node = json.create_node();
			node.add_number("plan_id", plan_id);
			node.add_text("station", *station);
			node.add_text("caller", *caller);
			node.add_text("description", *description);
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
