#include "TransRedis.h"
#include "cfg/config.h"

/**
* @brief 当前线程静态唯一
*/
static __thread  TransRedis* _context = nullptr;

/**
* @brief 配置文件中的redis的地址
*/
string RedisIp = config::get_config("redis_add");
/**
* @brief 配置文件中的redis的db
*/
int RedisDb = config::get_int("redis_defdb");

/**
* @brief 析构
*/
RedisDbScope::~RedisDbScope()
{
	TransRedis* context = TransRedis::get_current();
	if (context != nullptr)
		(*context)->select(RedisDb);
}
/**
* @brief 生成当前线程上下文的事务Redis对象
*/
void TransRedis::open_context()
{
	if (_context == nullptr)
	{
		RedisIp = config::get_config("redis_add");
		RedisDb = config::get_int("redis_defdb");
		_context = new TransRedis();
	}
}
/**
* @brief 取得当前线程上下文的事务Redis对象
* @return 当前线程上下文的操作对象
*/
TransRedis& TransRedis::get_context()
{
	open_context();
	return *_context;
}
/**
* @brief 取得当前线程上下文的事务Redis对象
* @return 当前线程上下文的操作对象
*/
TransRedis* TransRedis::get_current()
{
	return _context;
}
/**
* @brief 关闭当前线程上下文的事务Redis对象
*/
void TransRedis::close_context()
{
	if (_context != nullptr)
		delete _context;
	_context = nullptr;
}
/**
* @brief 构造
*/
TransRedis::TransRedis()
	: m_trans_num(0)
	, m_failed(false)
	, m_redis_client(RedisIp.c_str(), 0, 0)
	, m_redis_cmd(&m_redis_client)
{
	m_redis_cmd.select(RedisDb);
}
/**
* @brief 析构
*/
TransRedis::~TransRedis()
{
	if (m_trans_num > 0)
	{
		m_trans_num = 0;
		if (!m_failed)
			commit_inner();
	}
	m_redis_cmd.quit();
	m_redis_client.close();
}

/**
* @brief 启用事务
* @return 当前线程上下文的操作对象
*/
TransRedis& TransRedis::begin_trans()
{
	get_context();
	_context->m_trans_num += 1;
	return *_context;
}
/**
* @brief 提交事务,如果不是最先启用事务的地方调用,只是减少事务启用次数,最后一次调用(对应最早调用begin_trans)时,如果之前m_failed已设置,内部还是会调用rollback,除非ignore_failed设置为true
* @param {bool} ignore_failed 忽略m_failed的设置,即绝对的调用提交
*/
void TransRedis::end_trans(bool ignore_failed)
{
	log_debug(DEBUG_BASE,5,"关闭redis事务");
	if (_context == nullptr)
		return;
	_context->m_trans_num--;
	if (_context->m_trans_num > 0)
		return;
	if (ignore_failed || !_context->m_failed)
		_context->commit_inner();
	delete _context;
	_context = nullptr;
}
/**
* @brief 回退事务
*/
void TransRedis::commit_inner()
{
	auto start = m_modifies.begin();
	auto end = m_modifies.end();
	log_debug(DEBUG_BASE, 5, "开始提交redis事务...");

	while (start != end)
	{
		acl::string&vl = m_local_values[start->first];

		if (m_redis_cmd.set(start->first.c_str(), start->first.length(), vl.c_str(), vl.length()) == false)
		{
			log_error3("(%s)write_to_redis(%s)时发生错误(%s)", RedisIp.c_str(), start->first.c_str(), m_redis_cmd.result_error());
		}
		++start;
	}
	log_debug(DEBUG_BASE, 5, "结束提交redis事务");
}
/**
* @brief 设置出错
*/
void TransRedis::set_failed()
{
	auto m_context = get_context();
	m_context.m_failed = true;
}

bool TransRedis::get(const char* key, acl::string& vl)
{
	if (m_trans_num > 0 && !m_local_values.empty())
	{
		auto it = m_local_values.find(key);
		if (it != m_local_values.end())
		{
			vl = it->second;
			log_debug2(DEBUG_BASE, 5, "redis->get %s(%s)", key, vl.c_str());
			return true;
		}
	}
	m_redis_cmd.clear();
	acl::string vl2;
	if (m_redis_cmd.get(key, vl2) == false)
	{
		log_error3("(%s)read_from_redis(%s)时发生错误(%s)", RedisIp, key, m_redis_cmd.result_error());
		return false;
	}
	m_local_values[key] = vl2;
	vl = vl2;
	log_debug2(DEBUG_BASE, 5, "redis->get %s(%s)", key, vl.c_str());
	return true;
}
void TransRedis::set(const char* key, acl::string&vl)
{
	log_debug2(DEBUG_BASE, 5, "redis->set %s(%s)", key, vl.c_str());
	
	if (m_trans_num > 0)
	{
		m_local_values[key] = vl;
		//m_modifies[key] = 1;
	}
	//else
	{
		if (m_redis_cmd.set(key, strlen(key), vl.c_str(), vl.length()) == false)
		{
			log_error3("(%s)write_to_redis(%s)时发生错误(%s)", RedisIp, key, m_redis_cmd.result_error());
		}
	}
}
void TransRedis::set(const char* key, const char* vl)
{
	log_debug2(DEBUG_BASE, 5, "redis->set %s(%s)", key, vl);
	
	if (m_trans_num > 0)
	{
		m_local_values[key] = vl;
		//m_modifies[key] = 1;
	}
	//else
	{
		if (m_redis_cmd.set(key, strlen(key), vl, strlen(vl)) == false)
		{
			log_error3("(%s)write_to_redis(%s)时发生错误(%s)", RedisIp, key, m_redis_cmd.result_error());
		}
	}
}
void TransRedis::set(const char* key, const char* vl, size_t len)
{
	log_debug2(DEBUG_BASE, 5, "redis->set %s(binary %d)", key, len);
	
	if (m_trans_num > 0)
	{
		acl::string svl;
		svl.set_bin(true);
		svl.copy(vl, len);
		m_local_values[key] = svl;
		//m_modifies[key] = 1;
	}
	//else
	{
		if (m_redis_cmd.set(key, strlen(key), vl, len) == false)
		{
			log_error3("(%s)write_to_redis(%s)时发生错误(%s)", RedisIp, key, m_redis_cmd.result_error());
		}
	}
}
acl::string TransRedis::read_str_from_redis(const char* key)
{
	acl::string str;
	get(key, str);
	return str;
}

acl::string TransRedis::read_from_redis(const char* key)
{
	acl::string str;
	str.set_bin(true);
	str.set_max(0x8000);
	get(key, str);
	return str;
}
acl::string TransRedis::read_first_from_redis(const char* key)
{
	m_redis_cmd.clear();
	acl::string str;
	str.set_bin(true);
	vector<acl::string> keys;
	if (m_redis_cmd.keys_pattern(key, &keys) < 0)
	{
		log_error3("(%s)keys_pattern(%s)时发生错误(%s)", RedisIp, key, m_redis_cmd.result_error());
	}
	if (!keys.empty())
	{
		if (get(keys.begin()->c_str(), str) == false)
		{
			log_error3("(%s)read_first_from_redis(%s)时发生错误(%s)", RedisIp, key, m_redis_cmd.result_error());
		}
	}
	return str;
}
void TransRedis::write_json_to_redis(const char* key, const char* json)
{
	m_redis_cmd.clear();
	if (json[strlen(json) - 1] != '}')
	{
		log_error3("(%s)write_json_to_redis(%s)时(%s)不是JSON", RedisIp, key, json);
	}
	else
	{
		set(key, json);
	}
}

void TransRedis::write_to_redis(const char* key, const char* bin, size_t len)
{
	acl::string vl;
	vl.set_bin(true);
	vl.copy(bin, len);
	set(key, vl);
}
vector<acl::string> TransRedis::find_redis_keys(const char* find_key)
{
	vector<acl::string> keys;
	m_redis_cmd.clear();
	if (m_redis_cmd.keys_pattern(find_key, &keys) < 0)
	{
		log_error3("(%s)keys_pattern(%s)时发生错误(%s)", RedisIp, find_key, m_redis_cmd.result_error());
	}
	return keys;
}
vector<acl::string*> TransRedis::find_from_redis(const char* find_key)
{
	vector<acl::string*> values;
	vector<acl::string> keys;
	//m_redis_cmd.clear();
	if (m_redis_cmd.keys_pattern(find_key, &keys) < 0)
	{
		log_error3("(%s)keys_pattern(%s)时发生错误(%s)", RedisIp, find_key, m_redis_cmd.result_error());
		return values;
	}
	if (!keys.empty())
	{
		for each(auto key in keys)
		{
			acl::string* str = new acl::string();
			if (get(key, *str) == false)
			{
				log_error3("(%s)get(%s)时发生错误(%s)", RedisIp, key, m_redis_cmd.result_error());
				continue;
			}
			values.push_back(str);
		}
	}
	return values;
}

void TransRedis::delete_from_redis(const char* find_key)
{
	vector<acl::string> keys;
	if (m_redis_cmd.keys_pattern(find_key, &keys) < 0)
	{
		log_error3("(%s)keys_pattern(%s)时发生错误(%s)", RedisIp, find_key, m_redis_cmd.result_error());
	}
	if (!keys.empty())
	{
		for each(auto key in keys)
		{
			m_redis_cmd.clear();
			if (m_redis_cmd.del(key) < 0)
			{
				log_error3("(%s)del(%s)时发生错误(%s)", RedisIp, key, m_redis_cmd.result_error());
			}
		}
	}
}

size_t TransRedis::incr_redis(const char* key)
{
	long long id;
	RedisDbScope scope(REDIS_DB_SYSTEM);
	if (m_redis_cmd.incr(key, &id) == false)
	{
		log_error3("(%s)incr(%s)时发生错误(%s)", RedisIp, key, m_redis_cmd.result_error());
		id = 0LL;
	}
	return static_cast<size_t>(id);
}


bool TransRedis::lock_from_redis(const char* key)
{
	RedisDbScope scope(REDIS_DB_SYSTEM);
	char vl[2] = "1";
	int re = m_redis_cmd.setnx(key, vl);
	if (re < 0)
	{
		log_error3("(%s)setnx(%s)时发生错误(%s)", RedisIp, key, m_redis_cmd.result_error());
	}
	if (re == 1)
	{
		m_redis_cmd.pexpire(key, 60000);//60秒锁定
	}
	return re == 1;
}
bool TransRedis::unlock_from_redis(const char* key)
{
	RedisDbScope scope(REDIS_DB_SYSTEM);
	if (m_redis_cmd.del(key) < 0)
	{
		log_error3("(%s)del(%s)时发生错误(%s)", RedisIp, key, m_redis_cmd.result_error());
	}
	return true;
}

bool TransRedis::get_hash(const char* key, const char* sub_key, acl::string& vl)
{
	m_redis_cmd.clear();
	if (m_redis_cmd.hget(key, sub_key, vl) == false)
	{
		log_error3("(%s)get_hash(%s)时发生错误(%s)", RedisIp, key, m_redis_cmd.result_error());
		return false;
	}
	log_debug3(DEBUG_BASE, 5, "redis->hget %s:%s(%s)", key, sub_key, vl.c_str());
	return true;
}

bool TransRedis::set_hash(const char* key, const char* sub_key, const char* vl)
{
	m_redis_cmd.clear();
	acl::string vl2;
	if (m_redis_cmd.hset(key, sub_key, vl) == false)
	{
		log_error3("(%s)set_hash(%s)时发生错误(%s)", RedisIp, key, m_redis_cmd.result_error());
		return false;
	}
	log_debug3(DEBUG_BASE, 5, "redis->hset %s:%s(%s)", key, sub_key, vl);
	return true;
}
bool TransRedis::get_hash(const char* key,std::map<acl::string, acl::string>& vl)
{
	m_redis_cmd.clear();
	if (m_redis_cmd.hgetall(key,vl) == false)
	{
		log_error3("(%s)get_hash(%s)时发生错误(%s)", RedisIp, key, m_redis_cmd.result_error());
		return false;
	}
	log_debug1(DEBUG_BASE, 5, "redis->hget %s", key);
	return true;
}

bool TransRedis::del_hash(const char* key, const char* sub_key)
{
	m_redis_cmd.clear();
	acl::string vl2;
	if (m_redis_cmd.hdel(key, sub_key) == false)
	{
		log_error3("(%s)set_hash(%s)时发生错误(%s)", RedisIp, key, m_redis_cmd.result_error());
		return false;
	}
	log_debug2(DEBUG_BASE, 5, "redis->hdel %s:%s", key, sub_key);
	return true;
}