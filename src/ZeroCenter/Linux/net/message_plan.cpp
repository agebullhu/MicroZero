#include "../stdafx.h"
#include "ZeroStation.h"

namespace agebull
{
	namespace zmq_net
	{

		/**
		* \brief 计划轮询
		*/
		void zero_station::plan_poll()
		{
			in_plan_poll_ = true;
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
				vector<plan_message> messages;
				load_now(messages);
				for (plan_message msg : messages)
				{
					command(msg.request_caller.c_str(), msg.messages);
					if (zmq_state_ == zmq_socket_state::Succeed)
					{
						plan_next(msg, false);
					}
				}
			}
			in_plan_poll_ = false;
		}


		/**
		* \brief 载入现在到期的内容
		*/
		void zero_station::load_now(vector<plan_message>& messages) const
		{
			char zkey[100];
			sprintf(zkey, "zero:plan:%s", station_name_.c_str());
			vector<acl::string> keys;
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);
			trans_redis::get_context()->zrangebyscore(zkey, 0, static_cast<double>(time(nullptr)), &keys);
			for (const acl::string& key : keys)
			{
				plan_message message;
				load_message(static_cast<uint>(acl_atoll(key.c_str())), message);
				messages.push_back(message);
			}
		}

		/**
		* \brief 删除一个计划
		*/

		bool zero_station::remove_next(plan_message& message) const
		{
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);
			char zkey[MAX_PATH];
			sprintf(zkey, "zero:plan:%s", station_name_.c_str());
			char id[MAX_PATH];

			sprintf(id, "%d", message.plan_id);
			return trans_redis::get_context()->zrem(zkey, id) >= 0;
		}


		/**
		 * \brief 计划下一次执行时间
		 * \param message 
		 * \param first 
		 * \return 
		 */
		bool zero_station::plan_next(plan_message& message, const bool first) const
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


		/**
		* \brief 保存下一次执行时间
		*/
		bool zero_station::save_next(plan_message& message) const
		{
			time_t t = time(nullptr);
			switch (message.plan_type)
			{
			case plan_date_type::time:
				t = message.plan_value;
				break;
			case plan_date_type::minute:
				t += message.plan_value * 60;
				break;
			case plan_date_type::hour:
				t += message.plan_value * 3600;
				break;
			case plan_date_type::day:
				t += message.plan_value * 24 * 3600;
				break;
			default: return false;
			}
			char zkey[MAX_PATH];
			sprintf(zkey, "msg:time:%s", station_name_.c_str());

			char id[MAX_PATH];
			sprintf(id, "%d", message.plan_id);
			map<acl::string, double> value;
			value.insert(make_pair(id, static_cast<double>(t)));

			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);
			{
				return trans_redis::get_context()->zadd(zkey, value) >= 0;
			}
		}

		/**
		* \brief 保存消息
		*/

		bool zero_station::save_message(plan_message& message) const
		{
			char key[MAX_PATH];
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);
			if (message.plan_id == 0)
			{
				sprintf(key, "zero:identity:%s", station_name_.c_str());
				message.plan_id = static_cast<uint32_t>(trans_redis::get_context().incr_redis(key)) + 1;
			}
			sprintf(key, "zero:message:%s:%8x", station_name_.c_str(), message.plan_id);
			return trans_redis::get_context()->set(key, message.write_json().c_str());
		}

		/**
		* \brief 读取消息
		*/

		bool zero_station::load_message(uint id, plan_message& message) const
		{
			char key[MAX_PATH];
			sprintf(key, "zero:message:%s:%8x", station_name_.c_str(), id);
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);
			acl::string val(128);
			if (!trans_redis::get_context()->get(key, val) || val.empty())
			{
				return false;
			}
			message.read_json(val);
			
			return true;
		}


		/**
		* \brief 删除一个消息
		*/

		bool zero_station::remove_message(plan_message& message) const
		{
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);
			trans_redis& redis = trans_redis::get_context();
			char key[MAX_PATH];
			char id[MAX_PATH];
			sprintf(id, "%d", message.plan_id);
			//1 删除消息
			sprintf(key, "zero:message:%s:%8x", station_name_.c_str(), message.plan_id);
			redis->del(key);
			//2 删除计划
			if (message.plan_type > plan_date_type::none)
			{
				sprintf(key, "zero:plan:%s", station_name_.c_str());
				redis->zrem(key, id);
			}
			//3 删除参与者
			sprintf(key, "zero:worker:%s:%8x", station_name_.c_str(), message.plan_id);
			acl::string val(128);
			while (redis->spop(key, val))
			{
				sprintf(key, "zero:request:%s:%s", station_name_.c_str(), val.c_str());
				redis->srem(key, id);
			}
			redis->del(key);
			//4 删除返回值
			sprintf(key, "zero:result:%s:%8x", station_name_.c_str(), message.plan_id);
			redis->del(key);
			return true;
		}

		/**
		* \brief 保存消息参与者
		*/

		bool zero_station::save_message_worker(uint msgid, vector<const char*>& workers) const
		{
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);
			char key[MAX_PATH];
			sprintf(key, "zero:worker:%s:%8x", station_name_.c_str(), msgid);
			trans_redis::get_context()->sadd(key, workers);
			char id[MAX_PATH];

			sprintf(id, "%d", msgid);
			for (auto work : workers)
			{
				sprintf(key, "zero:request:%s:%s", station_name_.c_str(), work);
				trans_redis::get_context()->sadd(key, id);
			}
			return true;
		}

		/**
		* \brief 保存消息参与者返回值
		*/

		bool zero_station::save_message_result(uint msgid, const string& worker, const string& response) const
		{
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);

			char key[MAX_PATH];
			sprintf(key, "zero:worker:%s:%8x", station_name_.c_str(), msgid);
			trans_redis::get_context()->srem(key, worker.c_str());

			char id[MAX_PATH];
			sprintf(id, "%d", msgid);
			sprintf(key, "zero:request:%s:%s", station_name_.c_str(), worker.c_str());
			trans_redis::get_context()->srem(key, id);

			sprintf(key, "zero:result:%s:%8x", station_name_.c_str(), msgid);
			trans_redis::get_context()->hset(key, worker.c_str(), response.c_str());
			return true;
		}

		/**
		* \brief 取一个参与者的消息返回值
		*/

		acl::string zero_station::get_message_result(uint msgid, const char* worker) const
		{
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);

			char key[MAX_PATH];

			sprintf(key, "zero:result:%s:%8x", station_name_.c_str(), msgid);
			acl::string val(128);
			trans_redis::get_context()->hget(key, worker, val);
			return val;
		}

		/**
		* \brief 取全部参与者消息返回值
		*/

		map<acl::string, acl::string> zero_station::get_message_result(uint msgid) const
		{
			redis_live_scope redis_live_scope;
			redis_db_scope db_scope(REDIS_DB_ZERO_MESSAGE);

			char key[MAX_PATH];
			map<acl::string, acl::string> result;
			sprintf(key, "zero:result:%s:%8x", station_name_.c_str(), msgid);
			trans_redis::get_context()->hgetall(key, result);
			return result;
		}
	}
}
