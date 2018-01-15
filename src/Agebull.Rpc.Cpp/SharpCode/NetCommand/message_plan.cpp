
#include "stdafx.h"
#include "ZeroStation.h"
namespace agebull
{
	namespace zmq_net
	{

		void ZeroStation::plan_poll_()
		{
			_in_plan_poll = true;
			while (can_do())
			{
				bool doit = true;
				for (int i = 0; i < 60; i++)
				{
					sleep(1);
					if (!can_do())
					{
						doit = false;
						break;
					}
				}
				if (!doit)
					break;
				vector<PlanMessage> messages;
				load_now(messages);
				for (PlanMessage msg : messages)
				{
					command(msg.caller.c_str(), msg.messages);
					if (_zmq_state == ZmqSocketState::Succeed)
					{
						plan_next(msg, false);
					}
				}
			}
			_in_plan_poll = false;
		}
		int ZeroStation::load_now(vector<PlanMessage>& messages) const
		{
			char zkey[100];
			sprintf_s(zkey, "msg:time:%s", _station_name.c_str());
			vector<acl::string> keys;
			{
				RedisLiveScope redis_live_scope;
				RedisDbScope db_scope(REDIS_DB_ZERO_MESSAGE);
				TransRedis::get_context()->zrangebyscore(zkey, 0, static_cast<double>(time(nullptr)), &keys);
			}
			for (acl::string key : keys)
			{
				PlanMessage message;
				load_message(key.c_str(), message);
				messages.push_back(message);
			}
			return messages.size();
		}

		bool ZeroStation::remove(PlanMessage& message) const
		{
			char zkey[100];
			sprintf_s(zkey, "msg:time:%s", _station_name.c_str());

			char key[64];
			sprintf_s(key, "msg:value:%d", message.plan_id);
			RedisLiveScope redis_live_scope;
			{
				RedisDbScope db_scope(REDIS_DB_ZERO_MESSAGE);
				TransRedis::get_context()->del(key);
				return TransRedis::get_context()->zrem(zkey, key) >= 0;
			}
		}

		bool ZeroStation::plan_next(PlanMessage& message, bool first) const
		{
			if (!first && message.plan_repet >= 0 && message.real_repet >= message.plan_repet)
			{
				remove(message);
				return false;
			}
			if (!first)
				message.real_repet += 1;
			save_message(message);
			save_next(message);
			return true;
		}

		bool ZeroStation::save_next(PlanMessage& message) const
		{
			time_t t = time(nullptr);
			switch (message.plan_type)
			{
			case plan_date_type::Time:
				t = message.plan_value;
				break;
			case plan_date_type::Minute:
				t += message.plan_value * 60;
				break;
			case plan_date_type::Hour:
				t += message.plan_value * 3600;
				break;
			case plan_date_type::Day:
				t += message.plan_value * 24 * 3600;
				break;
			default: return false;
			}
			char zkey[100];
			sprintf_s(zkey, "msg:time:%s", _station_name.c_str());

			char key[64];
			sprintf_s(key, "msg:value:%d", message.plan_id);
			{
				RedisLiveScope redis_live_scope;
				RedisDbScope db_scope(REDIS_DB_ZERO_MESSAGE);
				map<acl::string, double> value;
				value.insert(make_pair(key, t));
				return TransRedis::get_context()->zadd(zkey, value) >= 0;
			}
		}

		bool ZeroStation::save_message(PlanMessage& message)
		{
			RedisLiveScope redis_live_scope;
			RedisDbScope db_scope(REDIS_DB_ZERO_MESSAGE);
			if (message.plan_id == 0)
				message.plan_id = TransRedis::get_context().incr_redis("msg:identity") + 1;
			acl::json json;
			acl::json_node& node = json.create_node();
			node.add_number("plan_id", message.plan_id);
			node.add_number("plan_type", static_cast<int>(message.plan_type));
			node.add_number("plan_value", message.plan_value);
			node.add_number("plan_repet", message.plan_repet);
			node.add_number("real_repet", message.real_repet);
			node.add_text("real_repet", message.caller.c_str());
			acl::json_node& array = node.add_array(true);
			array.set_tag("messages");
			for (auto line : message.messages)
			{
				array.add_array_text(*line);
			}

			char key[64];
			sprintf_s(key, "msg:value:%d", message.plan_id);
			return TransRedis::get_context()->set(key, node.to_string().c_str());
		}
		bool ZeroStation::load_message(const char* key, PlanMessage& message)
		{
			RedisLiveScope redis_live_scope;
			RedisDbScope db_scope(REDIS_DB_ZERO_MESSAGE);
			acl::string val;
			if (!TransRedis::get_context()->get(key, val) || val.empty())
			{
				return false;
			}

			acl::json json;
			json.update(val);
			acl::json_node* iter = json.first_node();
			while (iter)
			{
				int idx = strmatchi(5, iter->tag_name(), "plan_id", "plan_type", "plan_value", "plan_repet", "real_repet", "caller", "messages");
				switch (idx)
				{
				case 0:
					message.plan_id = reinterpret_cast<int>(iter->get_int64());
					break;
				case 1:
					message.plan_type = static_cast<plan_date_type>(reinterpret_cast<int>(iter->get_int64()));
					break;
				case 2:
					message.plan_value = reinterpret_cast<int>(iter->get_int64());
					break;
				case 3:
					message.plan_repet = reinterpret_cast<int>(iter->get_int64());
					break;
				case 4:
					message.real_repet = reinterpret_cast<int>(iter->get_int64());
					break;
				case 5:
					message.caller = iter->get_string();
					break;
				case 6:
				{
					acl::json arr = iter->get_json();
					acl::json_node* iter_arr = arr.first_node();
					while (iter_arr)
					{
						message.messages.push_back(iter_arr->get_string());
						iter_arr = arr.next_node();
					}
				}
				break;
				default: break;
				}
				iter = json.next_node();
			}
			return true;
		}
	}
}