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


		size_t ZeroStation::load_now(vector<Message>& messages) const
		{
			char zkey[100];
			sprintf_s(zkey, "zero:plan:%s", _station_name.c_str());
			vector<acl::string> keys;
			{
				RedisLiveScope redis_live_scope;
				RedisDbScope db_scope(REDIS_DB_ZERO_MESSAGE);
				TransRedis::get_context()->zrangebyscore(zkey, 0, static_cast<double>(time(nullptr)), &keys);
			}
			for (acl::string key : keys)
			{
				Message message;
				load_message(static_cast<uint>(acl_atoll(key.c_str())), message);
				messages.push_back(message);
			}
			return messages.size();
		}

		/**
		* \brief 删除一个计划
		*/

		bool ZeroStation::remove_next(Message& message) const
		{
			RedisLiveScope redis_live_scope;
			RedisDbScope db_scope(REDIS_DB_ZERO_MESSAGE);
			char zkey[MAX_PATH];
			sprintf_s(zkey, "zero:plan:%s", _station_name.c_str());
			char id[MAX_PATH];
			_i64toa_s(message.plan_id, id, MAX_PATH, 10);
			return TransRedis::get_context()->zrem(zkey, id) >= 0;
		}


		bool ZeroStation::plan_next(Message& message, bool first) const
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


		bool ZeroStation::save_next(Message& message) const
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

			RedisLiveScope redis_live_scope;
			RedisDbScope db_scope(REDIS_DB_ZERO_MESSAGE);
			{
				return TransRedis::get_context()->zadd(zkey, value) >= 0;
			}
		}

		/**
		* \brief 保存消息
		*/

		bool ZeroStation::save_message(Message& message) const
		{
			char key[MAX_PATH];
			RedisLiveScope redis_live_scope;
			RedisDbScope db_scope(REDIS_DB_ZERO_MESSAGE);
			if (message.plan_id == 0)
			{
				sprintf_s(key, "zero:identity:%s", _station_name.c_str());
				message.plan_id = static_cast<uint32_t>(TransRedis::get_context().incr_redis(key)) + 1;
			}
			sprintf_s(key, "zero:message:%s:%8x", _station_name.c_str(), message.plan_id);
			return TransRedis::get_context()->set(key, message.write_json().c_str());
		}

		/**
		* \brief 读取消息
		*/

		bool ZeroStation::load_message(uint id, Message& message) const
		{
			char key[MAX_PATH];
			sprintf_s(key, "zero:message:%s:%8x", _station_name.c_str(), id);
			RedisLiveScope redis_live_scope;
			RedisDbScope db_scope(REDIS_DB_ZERO_MESSAGE);
			acl::string val;
			if (!TransRedis::get_context()->get(key, val) || val.empty())
			{
				return false;
			}
			message.read_json(val);
			
			return true;
		}


		/**
		* \brief 删除一个消息
		*/

		bool ZeroStation::remove_message(Message& message) const
		{
			RedisLiveScope redis_live_scope;
			RedisDbScope db_scope(REDIS_DB_ZERO_MESSAGE);
			TransRedis& redis = TransRedis::get_context();
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

		bool ZeroStation::save_message_worker(uint msgid, vector<const char*>& workers) const
		{
			RedisLiveScope redis_live_scope;
			RedisDbScope db_scope(REDIS_DB_ZERO_MESSAGE);
			char key[MAX_PATH];
			sprintf_s(key, "zero:worker:%s:%8x", _station_name.c_str(), msgid);
			TransRedis::get_context()->sadd(key, workers);
			char id[MAX_PATH];
			_itoa_s(msgid, id, MAX_PATH);
			for (auto work : workers)
			{
				sprintf_s(key, "zero:request:%s:%s", _station_name.c_str(), work);
				TransRedis::get_context()->sadd(key, id);
			}
			return true;
		}

		/**
		* \brief 保存消息参与者返回值
		*/

		bool ZeroStation::save_message_result(uint msgid, const string& worker, const string& response) const
		{
			RedisLiveScope redis_live_scope;
			RedisDbScope db_scope(REDIS_DB_ZERO_MESSAGE);

			char key[MAX_PATH];
			sprintf_s(key, "zero:worker:%s:%8x", _station_name.c_str(), msgid);
			TransRedis::get_context()->srem(key, worker.c_str());

			char id[MAX_PATH];
			_itoa_s(msgid, id, MAX_PATH);
			sprintf_s(key, "zero:request:%s:%s", _station_name.c_str(), worker.c_str());
			TransRedis::get_context()->srem(key, id);

			sprintf_s(key, "zero:result:%s:%8x", _station_name.c_str(), msgid);
			TransRedis::get_context()->hset(key, worker.c_str(), response.c_str());
			return true;
		}

		/**
		* \brief 取一个参与者的消息返回值
		*/

		acl::string ZeroStation::get_message_result(uint msgid, const char* worker) const
		{
			RedisLiveScope redis_live_scope;
			RedisDbScope db_scope(REDIS_DB_ZERO_MESSAGE);

			char key[MAX_PATH];

			sprintf_s(key, "zero:result:%s:%8x", _station_name.c_str(), msgid);
			acl::string val;
			TransRedis::get_context()->hget(key, worker, val);
			return val;
		}

		/**
		* \brief 取全部参与者消息返回值
		*/

		map<acl::string, acl::string> ZeroStation::get_message_result(uint msgid) const
		{
			RedisLiveScope redis_live_scope;
			RedisDbScope db_scope(REDIS_DB_ZERO_MESSAGE);

			char key[MAX_PATH];
			map<acl::string, acl::string> result;
			sprintf_s(key, "zero:result:%s:%8x", _station_name.c_str(), msgid);
			TransRedis::get_context()->hgetall(key, result);
			return result;
		}
	}
}
