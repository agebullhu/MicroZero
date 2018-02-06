#include "stdafx.h"
#include "ZeroStation.h"

namespace agebull
{
	namespace zmq_net
	{

		/**
		* \brief 保存计划
		*/
		void zero_station::save_plan(ZMQ_HANDLE socket, vector<sharp_char> list)
		{
			Message message;
			message.request_caller = list[0];
			acl::json json;
			json.update(*list[3]);
			acl::json_node* iter = json.first_node();
			while (iter)
			{
				int idx = strmatchi(5, iter->tag_name(), "plan_type", "plan_value", "plan_repet");
				switch (idx)
				{
				case 0:
					message.plan_type = static_cast<plan_date_type>(static_cast<int>(*iter->get_int64()));
					break;
				case 1:
					message.plan_value = static_cast<int>(*iter->get_int64());
					break;
				case 2:
					message.plan_repet = static_cast<int>(*iter->get_int64());
					break;
				default: break;
				}
				iter = json.next_node();
			}
			//后续内容为真实消息主体
			size_t i = 4;
			while (i < list.size())
			{
				message.messages.push_back(list[i++]);
			}
			_zmq_state = send_addr(socket, *list[0]);
			if (plan_next(message, true))
			{
				_zmq_state = send_late(socket, "+plan");
			}
			else
			{
				_zmq_state = send_late(socket, "-plan");
			}
		}
		void zero_station::plan_poll()
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
				vector<Message> messages;
				load_now(messages);
				for (Message msg : messages)
				{
					command(msg.request_caller.c_str(), msg.messages);
					if (_zmq_state == ZmqSocketState::Succeed)
					{
						plan_next(msg, false);
					}
				}
			}
			_in_plan_poll = false;
		}


		size_t zero_station::load_now(vector<Message>& messages) const
		{
			char zkey[100];
			sprintf_s(zkey, "zero:plan:%s", _station_name.c_str());
			vector<acl::string> keys;
			Message message;
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_PLAN);
			trans_redis::get_context()->zrangebyscore(zkey, 0, static_cast<double>(time(nullptr)), &keys);
			for (const acl::string& key : keys)
			{
				load_message(static_cast<uint>(acl_atoll(key.c_str())), message);
				messages.push_back(message);
			}
			return messages.size();
		}

		/**
		* \brief 删除一个计划
		*/

		bool zero_station::remove_next(Message& message) const
		{
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_PLAN);
			char zkey[MAX_PATH];
			sprintf_s(zkey, "zero:plan:%s", _station_name.c_str());
			char id[MAX_PATH];
			_i64toa_s(message.plan_id, id, MAX_PATH, 10);
			return trans_redis::get_context()->zrem(zkey, id) >= 0;
		}


		bool zero_station::plan_next(Message& message, bool first) const
		{
			if (!first && message.plan_repet >= 0 && message.real_repet >= message.plan_repet)
			{
				remove_message(message);
				remove_next(message);
				return false;
			}
			if (!first)
				message.real_repet += 1;
			save_message(message);
			save_next(message);
			return true;
		}


		bool zero_station::save_next(Message& message) const
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
			char zkey[MAX_PATH];
			sprintf_s(zkey, "msg:time:%s", _station_name.c_str());

			char id[MAX_PATH];
			_i64toa_s(message.plan_id, id, MAX_PATH,10);
			map<acl::string, double> value;
			value.insert(make_pair(id, static_cast<double>(t)));

			redis_live_scope redis_live_scope(REDIS_DB_ZERO_PLAN);
			return trans_redis::get_context()->zadd(zkey, value) >= 0;
		}

		/**
		* \brief 保存消息
		*/

		bool zero_station::save_message(Message& message) const
		{
			char key[MAX_PATH];
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_PLAN);
			if (message.plan_id == 0)
			{
				sprintf_s(key, "zero:identity:%s", _station_name.c_str());
				message.plan_id = static_cast<uint32_t>(trans_redis::get_context().incr_redis(key)) + 1;
			}
			acl::json json;
			acl::json_node& node = json.create_node();
			node.add_number("plan_id", message.plan_id);
			if (message.plan_type > plan_date_type::None)
			{
				node.add_number("plan_type", static_cast<int>(message.plan_type));
				node.add_number("plan_value", message.plan_value);
				node.add_number("plan_repet", message.plan_repet);
				node.add_number("real_repet", message.real_repet);
			}
			node.add_text("request_id", message.request_id.c_str());
			node.add_text("request_caller", message.request_caller.c_str());
			acl::json_node& array = node.add_array(true);
			array.set_tag("messages");
			for (auto line : message.messages)
			{
				array.add_array_text(*line);
			}
			sprintf_s(key, "zero:message:%s:%8x", _station_name.c_str(), message.plan_id);
			return trans_redis::get_context()->set(key, node.to_string().c_str());
		}

		/**
		* \brief 读取消息
		*/

		bool zero_station::load_message(uint id, Message& message) const
		{
			char key[MAX_PATH];
			sprintf_s(key, "zero:message:%s:%8x", _station_name.c_str(), id);
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_PLAN);
			acl::string val;
			if (!trans_redis::get_context()->get(key, val) || val.empty())
			{
				return false;
			}

			acl::json json;
			json.update(val);
			acl::json_node* iter = json.first_node();
			while (iter)
			{
				int idx = strmatchi(5, iter->tag_name(), "plan_id", "plan_type", "plan_value", "plan_repet", "real_repet", "request_caller", "request_id", "messages");
				switch (idx)
				{
				case 0:
					message.plan_id = static_cast<int>(*iter->get_int64());
					break;
				case 1:
					message.plan_type = static_cast<plan_date_type>(static_cast<int>(*iter->get_int64()));
					break;
				case 2:
					message.plan_value = static_cast<int>(*iter->get_int64());
					break;
				case 3:
					message.plan_repet = static_cast<int>(*iter->get_int64());
					break;
				case 4:
					message.real_repet = static_cast<int>(*iter->get_int64());
					break;
				case 5:
					message.request_caller = iter->get_string();
					break;
				case 6:
					message.request_id = iter->get_string();
					break;
				case 7:
				{
					acl::json arr = iter->get_json();
					acl::json_node* iter_arr = arr.first_node();
					while (iter_arr)
					{
						message.messages.emplace_back(iter_arr->get_string());
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


		/**
		* \brief 删除一个消息
		*/

		bool zero_station::remove_message(Message& message) const
		{
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_PLAN);
			trans_redis& redis = trans_redis::get_context();
			char key[MAX_PATH];
			char id[MAX_PATH];
			_itoa_s(message.plan_id, id, MAX_PATH);
			//1 删除消息
			sprintf_s(key, "zero:message:%s:%8x", _station_name.c_str(), message.plan_id);
			redis->del(key);
			//2 删除计划
			if (message.plan_type > plan_date_type::None)
			{
				sprintf_s(key, "zero:plan:%s", _station_name.c_str());
				redis->zrem(key, id);
			}
			//3 删除参与者
			sprintf_s(key, "zero:worker:%s:%8x", _station_name.c_str(), message.plan_id);
			acl::string val;
			while (redis->spop(key, val))
			{
				sprintf_s(key, "zero:request:%s:%s", _station_name.c_str(), val.c_str());
				redis->srem(key, id);
			}
			redis->del(key);
			//4 删除返回值
			sprintf_s(key, "zero:result:%s:%8x", _station_name.c_str(), message.plan_id);
			redis->del(key);
			return true;
		}

		/**
		* \brief 保存消息参与者
		*/

		bool zero_station::save_message_worker(uint msgid, vector<const char*>& workers) const
		{
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_PLAN);
			char key[MAX_PATH];
			sprintf_s(key, "zero:worker:%s:%8x", _station_name.c_str(), msgid);
			trans_redis::get_context()->sadd(key, workers);
			char id[MAX_PATH];
			_itoa_s(msgid, id, MAX_PATH);
			for (auto work : workers)
			{
				sprintf_s(key, "zero:request:%s:%s", _station_name.c_str(), work);
				trans_redis::get_context()->sadd(key, id);
			}
			return true;
		}

		/**
		* \brief 保存消息参与者返回值
		*/

		bool zero_station::save_message_result(uint msgid, const string& worker, const string& response) const
		{
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_PLAN);

			char key[MAX_PATH];
			sprintf_s(key, "zero:worker:%s:%8x", _station_name.c_str(), msgid);
			trans_redis::get_context()->srem(key, worker.c_str());

			char id[MAX_PATH];
			_itoa_s(msgid, id, MAX_PATH);
			sprintf_s(key, "zero:request:%s:%s", _station_name.c_str(), worker.c_str());
			trans_redis::get_context()->srem(key, id);

			sprintf_s(key, "zero:result:%s:%8x", _station_name.c_str(), msgid);
			trans_redis::get_context()->hset(key, worker.c_str(), response.c_str());
			return true;
		}

		/**
		* \brief 取一个参与者的消息返回值
		*/

		acl::string zero_station::get_message_result(uint msgid, const char* worker) const
		{
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_PLAN);

			char key[MAX_PATH];

			sprintf_s(key, "zero:result:%s:%8x", _station_name.c_str(), msgid);
			acl::string val;
			trans_redis::get_context()->hget(key, worker, val);
			return val;
		}

		/**
		* \brief 取全部参与者消息返回值
		*/

		map<acl::string, acl::string> zero_station::get_message_result(uint msgid) const
		{
			redis_live_scope redis_live_scope(REDIS_DB_ZERO_PLAN);

			char key[MAX_PATH];
			map<acl::string, acl::string> result;
			sprintf_s(key, "zero:result:%s:%8x", _station_name.c_str(), msgid);
			trans_redis::get_context()->hgetall(key, result);
			return result;
		}
	}
}
