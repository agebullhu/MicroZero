#include "../stdafx.h"

namespace agebull
{
	boost::mutex lock_mutex_;
	/**
	* \brief 当前线程静态唯一
	*/
	static thread_local trans_redis* thread_context_ = nullptr;


	redis_live_scope::redis_live_scope() : redis_live_scope(global_config::redis_defdb)
	{
	}

	redis_live_scope::redis_live_scope(int db)
	{
		boost::lock_guard<boost::mutex> guard(lock_mutex_);
		redis_ = thread_context_;
		if (redis_ == nullptr)
		{
			thread_context_ = redis_ = new trans_redis(db);
#if _DEBUG
			thread_context_->m_last_status = thread_context_->m_redis_cmd->ping();
#endif
			old_db_ = db;
			open_by_me_ = true;
		}
		else
		{
#if _DEBUG
			thread_context_->m_last_status = thread_context_->m_redis_cmd->ping();
#endif
			old_db_ = thread_context_->cur_db();
			thread_context_->select(db);
			open_by_me_ = false;
		}
	}

	redis_live_scope::~redis_live_scope()
	{
		if (open_by_me_)
			agebull::trans_redis::close_context();
		else
			thread_context_->select(old_db_);
	}

	redis_trans_scope::redis_trans_scope()
	{
		trans_redis::get_context().begin_trans();
	}

	redis_trans_scope::~redis_trans_scope()
	{
		if (thread_context_ != nullptr)
			trans_redis::end_trans();
	}

	/**
	* \brief 生成当前线程上下文的事务Redis对象
	*/
	bool trans_redis::open_context()
	{
		return open_context(global_config::redis_defdb);
	}
	/**
	* \brief 生成当前线程上下文的事务Redis对象
	*/
	bool trans_redis::open_context(int db)
	{
		boost::lock_guard<boost::mutex> guard(lock_mutex_);
		if (thread_context_ == nullptr)
		{
			thread_context_ = new trans_redis(db);
#if _DEBUG
			thread_context_->m_last_status = thread_context_->m_redis_cmd->ping();
#endif
			return true;
		}
		else
		{
#if _DEBUG
			thread_context_->m_last_status = thread_context_->m_redis_cmd->ping();
#endif
			(*thread_context_).select(db);
		}
		return false;
	}
	/**
	* \brief 取得当前线程上下文的事务Redis对象
	* @return 当前线程上下文的操作对象
	*/
	trans_redis& trans_redis::get_context()
	{
		open_context();
		return *thread_context_;
	}
	/**
	* \brief 取得当前线程上下文的事务Redis对象
	* @return 当前线程上下文的操作对象
	*/
	trans_redis* trans_redis::get_current()
	{
		return thread_context_;
	}
	/**
	* \brief 关闭当前线程上下文的事务Redis对象
	*/
	void trans_redis::close_context()
	{
		if (thread_context_ != nullptr)
		{
			delete thread_context_;
			thread_context_ = nullptr;
		}
	}
	/**
	* \brief 构造
	*/
	trans_redis::trans_redis(int db)
		: m_trans_num(0)
		, m_failed(false)
		, m_last_status(true)
		, m_cur_db_(db)
	{
		m_redis_client = new acl::redis_client(redis_ip());
		m_redis_cmd = new acl::redis(m_redis_client);
		m_redis_cmd->select(db);
	}

	/**
	* 选择 redis-server 中的数据库 ID
	* SELECT command to select the DB id in redis-server
	* @param dbnum {int} redis 数据库 ID
	*  the DB id
	* @return {bool} 操作是否成功
	*  return true if success, or false for failed.
	*/
	bool trans_redis::select(int dbnum)
	{
		if (m_cur_db_ == dbnum)
			return true;
		m_cur_db_ = dbnum;
		return m_redis_cmd->select(dbnum);
	}
	/**
	* \brief 析构
	*/
	trans_redis::~trans_redis()
	{
		if (m_trans_num > 0)
		{
			m_trans_num = 0;
			if (!m_failed)
				commit_inner();
		}
		if (m_redis_cmd == nullptr)
			return;
		m_redis_cmd->quit();
		delete m_redis_cmd;
		m_redis_cmd = nullptr;
		m_redis_client->close();
		delete m_redis_client;
		m_redis_client = nullptr;
		if (thread_context_ == this)
			thread_context_ = nullptr;
	}

	/**
	* \brief 启用事务
	* @return 当前线程上下文的操作对象
	*/
	trans_redis& trans_redis::begin_trans()
	{
		get_context();
		thread_context_->m_trans_num += 1;
		return *thread_context_;
	}
	/**
	* \brief 提交事务,如果不是最先启用事务的地方调用,只是减少事务启用次数,最后一次调用(对应最早调用begin_trans)时,如果之前m_failed已设置,内部还是会调用rollback,除非ignore_failed设置为true
	* @param {bool} ignore_failed 忽略m_failed的设置,即绝对的调用提交
	*/
	void trans_redis::end_trans(bool ignore_failed)
	{
		log_debug(DEBUG_BASE, 5, "关闭redis事务");
		if (thread_context_ == nullptr)
			return;
		thread_context_->m_trans_num--;
		if (thread_context_->m_trans_num > 0)
			return;
		if (ignore_failed || !thread_context_->m_failed)
			thread_context_->commit_inner();
		delete thread_context_;
		thread_context_ = nullptr;
	}
	/**
	* \brief 回退事务
	*/
	void trans_redis::commit_inner()
	{
		auto start = m_modifies.begin();
		const auto end = m_modifies.end();
		log_debug(DEBUG_BASE, 5, "开始提交redis事务...");

		while (start != end)
		{
			acl::string&vl = m_local_values[start->first];
			if (!m_redis_cmd->set(start->first.c_str(), start->first.length(), vl.c_str(), vl.length()))
			{
				log_error3("(%s)write_to_redis(%s)时发生错误(%s)", redis_ip(), start->first.c_str(), m_redis_cmd->result_error());
				break;
			}
			++start;
		}
		log_debug(DEBUG_BASE, 5, "结束提交redis事务");
	}
	/**
	* \brief 设置出错
	*/
	void trans_redis::set_failed()
	{
		get_context().m_failed = true;
	}

	bool trans_redis::last_status() const
	{
		return m_last_status;
	}

	acl::redis* trans_redis::operator->() const
	{
		return m_redis_cmd;
	}

	template <class TArg1>
	acl::string trans_redis::read_str_from_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return read_str_from_redis(key);
	}

	template <class TArg1, class TArg2>
	acl::string trans_redis::read_str_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return read_str_from_redis(key);
	}

	template <class TArg1, class TArg2, class TArg3>
	acl::string trans_redis::read_str_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return read_str_from_redis(key);
	}

	template <class TArg1>
	std::vector<acl::string> trans_redis::find_redis_keys(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return find_redis_keys(key);
	}

	template <class TArg1, class TArg2>
	std::vector<acl::string> trans_redis::find_redis_keys(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return find_redis_keys(key);
	}

	template <class TArg1, class TArg2, class TArg3>
	std::vector<acl::string> trans_redis::find_redis_keys(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return find_redis_keys(key);
	}

	template <class TArg1>
	std::vector<acl::string*> trans_redis::find_from_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return find_from_redis(key);
	}

	template <class TArg1, class TArg2>
	std::vector<acl::string*> trans_redis::find_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return find_from_redis(key);
	}

	template <class TArg1, class TArg2, class TArg3>
	std::vector<acl::string*> trans_redis::find_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return find_from_redis(key);
	}

	template <class TArg1>
	acl::string trans_redis::read_from_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return read_from_redis(key);
	}

	template <class TArg1, class TArg2>
	acl::string trans_redis::read_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return read_from_redis(key);
	}

	template <class TArg1, class TArg2, class TArg3>
	acl::string trans_redis::read_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return read_from_redis(key);
	}

	template <class TArg1>
	void trans_redis::delete_from_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		delete_from_redis(key);
	}

	template <class TArg1, class TArg2>
	void trans_redis::delete_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		delete_from_redis(key);
	}

	template <class TArg1, class TArg2, class TArg3>
	void trans_redis::delete_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		delete_from_redis(key);
	}

	template <class TArg1>
	size_t trans_redis::incr_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return incr_redis(key);
	}

	template <class TArg1, class TArg2>
	size_t trans_redis::incr_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return incr_redis(key);
	}

	template <class TArg1, class TArg2, class TArg3>
	size_t trans_redis::incr_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return incr_redis(key);
	}

	template <class TArg1>
	void trans_redis::write_to_redis(const char* bin, size_t len, const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		write_to_redis(key, bin, len);
	}

	template <class TArg1, class TArg2>
	void trans_redis::write_to_redis(const char* bin, size_t len, const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		write_to_redis(key, bin, len);
	}

	template <class TArg1, class TArg2, class TArg3>
	void trans_redis::write_to_redis(const char* bin, size_t len, const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		write_to_redis(key, bin, len);
	}

	template <class TArg1>
	acl::string trans_redis::read_first_from_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return read_first_from_redis(key);
	}

	template <class TArg1, class TArg2>
	acl::string trans_redis::read_first_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return read_first_from_redis(key);
	}

	template <class TArg1, class TArg2, class TArg3>
	acl::string trans_redis::read_first_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return read_first_from_redis(key);
	}

	template <class TArg1>
	bool trans_redis::unlock_from_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return unlock_from_redis(key);
	}

	template <class TArg1, class TArg2>
	bool trans_redis::unlock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return unlock_from_redis(key);
	}

	template <class TArg1, class TArg2, class TArg3>
	bool trans_redis::unlock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return unlock_from_redis(key);
	}

	template <class TArg1>
	bool trans_redis::lock_from_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return lock_from_redis(key);
	}

	template <class TArg1, class TArg2>
	bool trans_redis::lock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return lock_from_redis(key);
	}

	template <class TArg1, class TArg2, class TArg3>
	bool trans_redis::lock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return lock_from_redis(key);
	}

	bool trans_redis::get(const char* key, acl::string& vl)
	{
		if (m_trans_num > 0 && !m_local_values.empty())
		{
			const map<acl::string, acl::string>::iterator it = m_local_values.find(key);
			if (it != m_local_values.end())
			{
				vl = it->second;
				log_debug2(DEBUG_BASE, 5, "redis->get %s(%s)", key, vl.c_str());
				return true;
			}
		}
		m_redis_cmd->clear();
		acl::string vl2;
		if (!m_redis_cmd->get(key, vl2))
		{
			log_error3("(%s)read_from_redis(%s)时发生错误(%s)", redis_ip(), key, m_redis_cmd->result_error());
			return false;
		}
		m_local_values[key] = vl2;
		vl = vl2;
		return true;
	}
	void trans_redis::set(const char* key, acl::string&vl)
	{
		if (m_trans_num > 0)
		{
			m_local_values[key] = vl;
			//m_modifies[key] = 1;
		}
		//else
		{
			if (!m_redis_cmd->set(key, strlen(key), vl.c_str(), vl.length()))
			{
				log_error3("(%s)write_to_redis(%s)时发生错误(%s)", redis_ip(), key, m_redis_cmd->result_error());
			}
		}
	}
	void trans_redis::set(const char* key, const char* vl)
	{
		if (m_trans_num > 0)
		{
			m_local_values[key] = vl;
			//m_modifies[key] = 1;
		}
		//else
		{
			if (!m_redis_cmd->set(key, strlen(key), vl, strlen(vl)))
			{
				log_error3("(%s)write_to_redis(%s)时发生错误(%s)", redis_ip(), key, m_redis_cmd->result_error());
			}
		}
	}
	void trans_redis::set(const char* key, const char* vl, size_t len)
	{
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
			if (!m_redis_cmd->set(key, strlen(key), vl, len))
			{
				log_error3("(%s)write_to_redis(%s)时发生错误(%s)", redis_ip(), key, m_redis_cmd->result_error());
			}
		}
	}
	acl::string trans_redis::read_str_from_redis(const char* key)
	{
		acl::string str;
		get(key, str);
		return str;
	}

	acl::string trans_redis::read_from_redis(const char* key)
	{
		acl::string str;
		str.set_bin(true);
		str.set_max(0x8000);
		get(key, str);
		return str;
	}
	acl::string trans_redis::read_first_from_redis(const char* key)
	{
		m_redis_cmd->clear();
		acl::string str;
		str.set_bin(true);
		vector<acl::string> keys;
		if (m_redis_cmd->keys_pattern(key, &keys) < 0)
		{
			log_error3("(%s)keys_pattern(%s)时发生错误(%s)", redis_ip(), key, m_redis_cmd->result_error());
		}
		if (!keys.empty())
		{
			if (!get(keys.begin()->c_str(), str))
			{
				log_error3("(%s)read_first_from_redis(%s)时发生错误(%s)", redis_ip(), key, m_redis_cmd->result_error());
			}
		}
		return str;
	}
	void trans_redis::write_json_to_redis(const char* key, const char* json)
	{
		m_redis_cmd->clear();
		if (json[strlen(json) - 1] != '}')
		{
			log_error3("(%s)write_json_to_redis(%s)时(%s)不是JSON", redis_ip(), key, json);
		}
		else
		{
			set(key, json);
		}
	}

	void trans_redis::write_to_redis(const char* key, const char* bin, size_t len)
	{
		acl::string vl;
		vl.set_bin(true);
		vl.copy(bin, len);
		set(key, vl);
	}
	vector<acl::string> trans_redis::find_redis_keys(const char* find_key) const
	{
		vector<acl::string> keys;
		m_redis_cmd->clear();
		if (m_redis_cmd->keys_pattern(find_key, &keys) < 0)
		{
			log_error3("(%s)keys_pattern(%s)时发生错误(%s)", redis_ip(), find_key, m_redis_cmd->result_error());
		}
		return keys;
	}
	vector<acl::string*> trans_redis::find_from_redis(const char* find_key)
	{
		vector<acl::string*> values;
		vector<acl::string> keys;
		//m_redis_cmd->clear();
		if (m_redis_cmd->keys_pattern(find_key, &keys) < 0)
		{
			log_error3("(%s)keys_pattern(%s)时发生错误(%s)", redis_ip(), find_key, m_redis_cmd->result_error());
			return values;
		}
		if (!keys.empty())
		{
			for (auto key : keys)
			{
				acl::string* str = new acl::string();
				if (get(key, *str) == false)
				{
					log_error3("(%s)get(%s)时发生错误(%s)", redis_ip(), key.c_str(), m_redis_cmd->result_error());
					continue;
				}
				values.emplace_back(str);
			}
		}
		return values;
	}

	void trans_redis::delete_from_redis(const char* find_key) const
	{
		vector<acl::string> keys;
		if (m_redis_cmd->keys_pattern(find_key, &keys) < 0)
		{
			log_error3("(%s)keys_pattern(%s)时发生错误(%s)", redis_ip(), find_key, m_redis_cmd->result_error());
		}
		if (!keys.empty())
		{
			for (auto&key : keys)
			{
				m_redis_cmd->clear();
				if (m_redis_cmd->del(key) < 0)
				{
					log_error3("(%s)del(%s)时发生错误(%s)", redis_ip(), key.c_str(), m_redis_cmd->result_error());
				}
			}
		}
	}

	size_t trans_redis::incr_redis(const char* key) const
	{
		long long id;
		if (!m_redis_cmd->incr(key, &id))
		{
			log_error3("(%s)incr(%s)时发生错误(%s)", redis_ip(), key, m_redis_cmd->result_error());
			id = 0LL;
		}
		return static_cast<size_t>(id);
	}


	bool trans_redis::lock_from_redis(const char* key) const
	{
		char vl[2] = "1";
		const int re = m_redis_cmd->setnx(key, vl);
		if (re < 0)
		{
			log_error3("(%s)setnx(%s)时发生错误(%s)", redis_ip(), key, m_redis_cmd->result_error());
		}
		if (re == 1)
		{
			m_redis_cmd->pexpire(key, 60000);//60秒锁定
		}
		return re == 1;
	}
	bool trans_redis::unlock_from_redis(const char* key) const
	{
		if (m_redis_cmd->del(key) < 0)
		{
			log_error3("(%s)del(%s)时发生错误(%s)", redis_ip(), key, m_redis_cmd->result_error());
		}
		return true;
	}

	bool trans_redis::get_hash(const char* key, const char* sub_key, acl::string& vl) const
	{
		m_redis_cmd->clear();
		if (!m_redis_cmd->hget(key, sub_key, vl))
		{
			log_error3("(%s)get_hash(%s)时发生错误(%s)", redis_ip(), key, m_redis_cmd->result_error());
			return false;
		}
		log_debug3(DEBUG_BASE, 5, "redis->hget %s:%s(%s)", key, sub_key, vl.c_str());
		return true;
	}

	bool trans_redis::set_hash(const char* key, const char* sub_key, const char* vl) const
	{
		m_redis_cmd->clear();
		acl::string vl2;
		if (m_redis_cmd->hset(key, sub_key, vl) < 0)
		{
			log_error3("(%s)set_hash(%s)时发生错误(%s)", redis_ip(), key, m_redis_cmd->result_error());
			return false;
		}
		log_debug3(DEBUG_BASE, 5, "redis->hset %s:%s(%s)", key, sub_key, vl);
		return true;
	}
	bool trans_redis::get_hash(const char* key, std::map<acl::string, acl::string>& vl) const
	{
		m_redis_cmd->clear();
		if (!m_redis_cmd->hgetall(key, vl))
		{
			log_error3("(%s)get_hash(%s)时发生错误(%s)", redis_ip(), key, m_redis_cmd->result_error());
			return false;
		}
		log_debug1(DEBUG_BASE, 5, "redis->hget %s", key);
		return true;
	}


	bool trans_redis::del_hash(const char* key, const char* sub_key) const
	{
		m_redis_cmd->clear();
		acl::string vl2;
		if (m_redis_cmd->hdel(key, sub_key) < 0)
		{
			log_error3("(%s)set_hash(%s)时发生错误(%s)", redis_ip(), key, m_redis_cmd->result_error());
			return false;
		}
		log_debug2(DEBUG_BASE, 5, "redis->hdel %s:%s", key, sub_key);
		return true;
	}

}
