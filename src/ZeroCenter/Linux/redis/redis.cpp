#include "redis.h" 
#include "cfg/config.h"

string RedisIp = config::get_config("redis_add");

acl::string read_str_from_redis(const char* key)
{
	acl::redis_client client(RedisIp.c_str(), 0, 0);
	acl::redis cmd(&client);
	acl::string str;
	if (cmd.get(key, str) == false)
	{
		log_error3("(%s)read_str_from_redis(%s)时发生错误(%s)", RedisIp, key, cmd.result_error());
	}
	cmd.quit();
	client.close();
	return str;
}

acl::string read_from_redis(const char* key)
{
	acl::redis_client client(RedisIp.c_str(), 0, 0);
	acl::redis cmd(&client);
	acl::string str;
	str.set_bin(true);
	str.set_max(4096);
	if (cmd.get(key, str) == false)
	{
		log_error3("(%s)read_from_redis(%s)时发生错误(%s)", RedisIp, key, cmd.result_error());
	}
	cmd.quit();
	client.close();
	return str;
}
acl::string read_first_from_redis(const char* key)
{
	acl::string str;
	str.set_bin(true);
	acl::redis_client client(RedisIp.c_str(), 0, 0);
	acl::redis cmd(&client);
	std::vector<acl::string> keys;
	if (cmd.keys_pattern(key, &keys) < 0)
	{
		log_error3("(%s)keys_pattern(%s)时发生错误(%s)", RedisIp, key, cmd.result_error());
	}
	if (!keys.empty())
	{
		if (cmd.get(keys.begin()->c_str(), str) == false)
		{
			log_error3("(%s)read_first_from_redis(%s)时发生错误(%s)", RedisIp, key, cmd.result_error());
		}
	}
	cmd.quit();
	client.close();
	return str;
}
void write_json_to_redis(const char* key, const char* json)
{
	acl::redis_client client(RedisIp.c_str(), 0, 0);

	acl::redis cmd(&client);

	if (json[strlen(json) - 1] != '}')
	{
		log_error3("(%s)write_json_to_redis(%s)时(%s)不是JSON", RedisIp, key, json);
	}
	else if (cmd.set(key, strlen(key), json, strlen(json)) == false)
	{
		log_error3("(%s)write_json_to_redis(%s)时发生错误(%s)", RedisIp, key, cmd.result_error());
	}
	cmd.quit();
	client.close();
}

void write_to_redis(const char* key, const char* bin, size_t len)
{
	acl::redis_client client(RedisIp.c_str(), 0, 0);
	acl::redis cmd(&client);
	if (cmd.set(key, strlen(key), bin, len) == false)
	{
		log_error3("(%s)write_to_redis(%s)时发生错误(%s)", RedisIp, key, cmd.result_error());
	}
	cmd.quit();
	client.close();
}
std::vector<acl::string> find_redis_keys(const char* find_key)
{
	acl::redis_client client(RedisIp.c_str(), 0, 0);
	acl::redis cmd(&client);

	std::vector<acl::string> keys;
	if (cmd.keys_pattern(find_key, &keys) < 0)
	{
		log_error3("(%s)keys_pattern(%s)时发生错误(%s)", RedisIp, find_key, cmd.result_error());
	}
	cmd.quit();
	client.close();
	return keys;
}
std::vector<acl::string*> find_from_redis(const char* find_key)
{
	std::vector<acl::string*> values;
	acl::redis_client client(RedisIp.c_str(), 0, 0);
	acl::redis cmd(&client);

	std::vector<acl::string> keys;
	if (cmd.keys_pattern(find_key, &keys) < 0)
	{
		log_error3("(%s)keys_pattern(%s)时发生错误(%s)", RedisIp, find_key, cmd.result_error());
		return values;
	}
	if (!keys.empty())
	{
		for each(auto key in keys)
		{
			cmd.clear();
			acl::string* str = new acl::string();
			str->set_bin(true);
			str->set_max(4096);
			if (cmd.get(key, *str) == false)
			{
				log_error3("(%s)get(%s)时发生错误(%s)", RedisIp, key, cmd.result_error());
				continue;
			}
			values.push_back(str);
		}
	}
	cmd.quit();
	client.close();
	return values;
}

void delete_from_redis(const char* find_key)
{
	acl::redis_client client(RedisIp.c_str(), 0, 0);
	acl::redis cmd(&client);
	std::vector<acl::string> keys;
	if (cmd.keys_pattern(find_key, &keys) < 0)
	{
		log_error3("(%s)keys_pattern(%s)时发生错误(%s)", RedisIp, find_key, cmd.result_error());
	}
	if (!keys.empty())
	{
		for each(auto key in keys)
		{
			cmd.clear();
			if (cmd.del(key) <0)
			{
				log_error3("(%s)del(%s)时发生错误(%s)", RedisIp, key, cmd.result_error());
			}
		}
	}
	cmd.quit();
	client.close();
}

size_t incr_redis(const char* key)
{
	acl::redis_client client(RedisIp.c_str(), 0, 0);
	acl::redis cmd(&client);
	long long id;
	cmd.select(1);
	if (cmd.incr(key, &id) == false)
	{
		log_error3("(%s)incr(%s)时发生错误(%s)", RedisIp, key, cmd.result_error());
		id = 0LL;
	}
	cmd.quit();
	client.close();
	return static_cast<size_t>(id);
}


bool lock_from_redis(const char* key)
{
	acl::redis_client client(RedisIp.c_str(), 0, 0);
	acl::redis cmd(&client);
	cmd.select(1);
	char vl[2] = "1";
	int re = cmd.setnx(key, vl);
	if (re < 0)
	{
		log_error3("(%s)setnx(%s)时发生错误(%s)", RedisIp, key, cmd.result_error());
	}
	if(re == 1)
	{
		cmd.pexpire(key, 60000);//60秒锁定
	}
	cmd.quit();
	client.close();
	return re == 1;
}
bool unlock_from_redis(const char* key)
{
	acl::redis_client client(RedisIp.c_str(), 0, 0);
	acl::redis cmd(&client);
	cmd.select(1);
	char vl[2] = "1";
	if (cmd.del(key) < 0)
	{
		log_error3("(%s)setnx(%s)时发生错误(%s)", RedisIp, key, cmd.result_error());
	}
	cmd.quit();
	client.close();
	return true;
}