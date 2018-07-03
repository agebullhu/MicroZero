#ifndef AGEBULL_REDIS_H
#define AGEBULL_REDIS_H
#pragma once
#include "../cfg/json_config.h"
#include "../shared_char.h"
#ifndef CLIENT
#include "../stdinc.h"
namespace agebull
{
	class redis_live_scope;
	class redis_trans_scope;
	/**
	* \brief 事务Redis 在没有启用时,与普通使用一样,启用时调用begin_trans,提交调用commit,回退调用rollback,且必须成对调用
	*/
	class trans_redis
	{
		friend class redis_live_scope;
		friend class redis_trans_scope;


		/**
		* \brief 启用事务的次数
		*/
		int m_trans_num;
		/**
		* \brief 是否有回退的调用,如果有所有即失败,除非显式设置为不回退
		*/
		bool m_failed;
		/**
		* \brief 最后一次操作是否成功
		*/
		bool m_last_status;
		/**
		* \brief acl的redis客户端对象
		*/
		acl::redis_client* m_redis_client;
		/**
		* \brief acl的redis命令对象
		*/
		acl::redis* m_redis_cmd;
		/**
		* \brief acl的redis命令对象
		*/
		int m_cur_db_;
		/**
		* \brief 事务中修改的内容
		*/
		map<acl::string, int> m_modifies;
		/**
		* \brief 本地缓存对象
		*/
		map<acl::string, acl::string> m_local_values;
	public:
		/**
		* \brief 配置文件中的redis的地址
		*/
		static const char* redis_ip()
		{
			return json_config::redis_addr;
		}

		/**
		* \brief 配置文件中的redis的db
		*/
		int cur_db() const
		{
			return m_cur_db_;
		}
		/**
		* \brief 构造
		*/
		trans_redis(int db);
		/**
		* \brief 析构
		*/
		~trans_redis();
		/**
		* \brief 取得当前线程上下文的事务Redis对象
		* @return 当前线程上下文的操作对象
		*/
		static trans_redis& get_context();
		/**
		* \brief 取得当前线程上下文的事务Redis对象
		* @return 当前线程上下文的操作对象
		*/
		static trans_redis* get_current();
		/**
		* \brief 生成当前线程上下文的事务Redis对象
		*/
		static bool open_context();
		/**
		* \brief 生成当前线程上下文的事务Redis对象
		*/
		static	bool open_context(int db);
		/**
		* \brief 关闭当前线程上下文的事务Redis对象
		*/
		static void close_context();

		/**
		* 选择 redis-server 中的数据库 ID
		* SELECT command to select the DB id in redis-server
		* @param dbnum {int} redis 数据库 ID
		*  the DB id
		* @return {bool} 操作是否成功
		*  return true if success, or false for failed.
		*/
		bool select(int dbnum);
		/**
		* \brief 启用事务
		* @return 当前线程上下文的操作对象
		*/
		static trans_redis& begin_trans();
		/**
		* \brief 提交事务,如果不是最先启用事务的地方调用,只是减少事务启用次数,最后一次调用(对应最早调用begin_trans)时,如果之前m_failed已设置,内部还是会调用rollback,除非ignore_failed设置为true
		* @param {bool} ignore_failed 忽略m_failed的设置,即绝对的调用提交
		*/
		static void end_trans(bool ignore_failed = false);
		/**
		* \brief 设置出错
		*/
		static void set_failed();
		/**
		* \brief 最后一次操作是否成功
		*/
		bool last_status() const;
	private:
		/**
		* \brief 提交事务
		*/
		void commit_inner();
	public:
		bool get(const char*, acl::string&);
		void set(const char*, const char*);
		void set(const char*, const char*, size_t);
		void set(const char*, acl::string&);
		acl::redis* operator->() const;
		acl::string read_str_from_redis(const char* key);
		template<class TArg1>
		acl::string read_str_from_redis(const char* key_fmt, TArg1 arg);

		template<class TArg1, class TArg2>
		acl::string read_str_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2);

		template<class TArg1, class TArg2, class TArg3>
		acl::string read_str_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3);

		std::vector<acl::string> find_redis_keys(const char* find_key) const;
		template<class TArg1>
		std::vector<acl::string> find_redis_keys(const char* key_fmt, TArg1 arg);

		template<class TArg1, class TArg2>
		std::vector<acl::string> find_redis_keys(const char* key_fmt, TArg1 arg1, TArg2 arg2);

		template<class TArg1, class TArg2, class TArg3>
		std::vector<acl::string> find_redis_keys(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3);

		std::vector<acl::string*> find_from_redis(const char* find_key);
		template<class TArg1>
		std::vector<acl::string*> find_from_redis(const char* key_fmt, TArg1 arg);

		template<class TArg1, class TArg2>
		std::vector<acl::string*> find_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2);

		template<class TArg1, class TArg2, class TArg3>
		std::vector<acl::string*> find_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3);
		acl::string read_from_redis(const char* key);
		template<class TArg1>
		acl::string read_from_redis(const char* key_fmt, TArg1 arg);

		template<class TArg1, class TArg2>
		acl::string read_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2);

		template<class TArg1, class TArg2, class TArg3>
		acl::string read_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3);

		void delete_from_redis(const char* find_key) const;

		template<class TArg1>
		void delete_from_redis(const char* key_fmt, TArg1 arg);

		template<class TArg1, class TArg2>
		void delete_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2);

		template<class TArg1, class TArg2, class TArg3>
		void delete_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3);

		size_t incr_redis(const char* key) const;
		template<class TArg1>
		size_t incr_redis(const char* key_fmt, TArg1 arg);

		template<class TArg1, class TArg2>
		size_t incr_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2);

		template<class TArg1, class TArg2, class TArg3>
		size_t incr_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3);
		void write_to_redis(const char* key, const char* bin, size_t len);
		void write_json_to_redis(const char* key, const char* json);
		template<class TArg1>
		void write_to_redis(const char* bin, size_t len, const char* key_fmt, TArg1 arg);

		template<class TArg1, class TArg2>
		void write_to_redis(const char* bin, size_t len, const char* key_fmt, TArg1 arg1, TArg2 arg2);

		template<class TArg1, class TArg2, class TArg3>
		void write_to_redis(const char* bin, size_t len, const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3);


		acl::string read_first_from_redis(const char* key);
		template<class TArg1>
		acl::string read_first_from_redis(const char* key_fmt, TArg1 arg);

		template<class TArg1, class TArg2>
		acl::string read_first_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2);

		template<class TArg1, class TArg2, class TArg3>
		acl::string read_first_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3);


		bool unlock_from_redis(const char* key) const;
		template<class TArg1>
		bool unlock_from_redis(const char* key_fmt, TArg1 arg);

		template<class TArg1, class TArg2>
		bool unlock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2);

		template<class TArg1, class TArg2, class TArg3>
		bool unlock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3);

		bool lock_from_redis(const char* key) const;
		template<class TArg1>
		bool lock_from_redis(const char* key_fmt, TArg1 arg);

		template<class TArg1, class TArg2>
		bool lock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2);

		template<class TArg1, class TArg2, class TArg3>
		bool lock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3);

		bool get_hash(const char* key, const char* sub_key, acl::string& vl) const;
		bool set_hash(const char* key, const char* sub_key, const char* vl) const;
		bool del_hash(const char* key, const char* sub_key) const;
		bool get_hash(const char* key, std::map<acl::string, acl::string>& vl) const;

		bool set_hash_val(const char* key, const char* sub_key, const agebull::zmq_net::shared_char& ptr) const
		{
			return m_redis_cmd->hset(key, sub_key, *ptr, ptr.size()) >= 0;
		}
		void set_hash_val(const char* key, const char* sub_key, int64 number)
		{
			char buf[32];
			sprintf(buf, "%lld", number);
			m_redis_cmd->hset(key, sub_key, buf);
		}
		void set_hash_val(const char* key, const char* sub_key,uint64 number)
		{
			char buf[32];
			sprintf(buf, "%llu", number);
			m_redis_cmd->hset(key, sub_key, buf);
		}
		void set_hash_val(const char* key, const char* sub_key, bool number)
		{
			m_redis_cmd->hset(key, sub_key, number ? "1":"0");
		}
		void set_hash_val(const char* key, const char* sub_key, int number)
		{
			char buf[32];
			sprintf(buf, "%d", number);
			m_redis_cmd->hset(key, sub_key, buf);
		}
		void set_hash_val(const char* key, const char* sub_key, int64_t number)
		{
			char buf[32];
			sprintf(buf, "%lld", number);
			m_redis_cmd->hset(key, sub_key, buf);
		}
		void set_hash_val(const char* key, const char* sub_key, uint number)
		{
			char buf[32];
			sprintf(buf, "%u", number);
			m_redis_cmd->hset(key, sub_key, buf);
		}
		void set_hash_val(const char* key, const char* sub_key, ulong number)
		{
			char buf[32];
			sprintf(buf, "%lu", number);
			m_redis_cmd->hset(key, sub_key, buf);
		}

		bool get_hash_val(const char* key, const char* sub_key, agebull::zmq_net::shared_char& ptr) const
		{
			acl::string val;
			if (!m_redis_cmd->hget(key, sub_key, val))
			{
				return false;
			}
			ptr = val;
			return true;
		}
		bool get_hash_val(const char* key, const char* sub_key, bool& num) const
		{
			acl::string val;
			if (!m_redis_cmd->hget(key, sub_key, val))
			{
				return false;
			}
			num ="1" == val;
			return true;
		}
		bool get_hash_val(const char* key, const char* sub_key, int& num) const
		{
			acl::string val;
			if (!m_redis_cmd->hget(key, sub_key, val))
			{
				return false;
			}
			num = atoi(val.c_str());
			return true;
		}
		bool get_hash_val(const char* key, const char* sub_key, uint& num) const
		{
			acl::string val;
			if (!m_redis_cmd->hget(key, sub_key, val))
			{
				return false;
			}
			num = atoi(val.c_str());
			return true;
		}
		bool get_hash_val(const char* key, const char* sub_key, long& num) const
		{
			acl::string val;
			if (!m_redis_cmd->hget(key, sub_key, val))
			{
				return false;
			}
			num = atol(val.c_str());
			return true;
		}
		bool get_hash_val(const char* key, const char* sub_key, ulong& num) const
		{
			acl::string val;
			if (!m_redis_cmd->hget(key, sub_key, val))
			{
				return false;
			}
			num = atol(val.c_str());
			return true;
		}
		bool get_hash_val(const char* key, const char* sub_key, int64& num) const
		{
			acl::string val;
			if (!m_redis_cmd->hget(key, sub_key, val))
			{
				return false;
			}
			num = atoll(val.c_str());
			return true;
		}
		bool get_hash_val(const char* key, const char* sub_key, uint64& num) const
		{
			acl::string val;
			if (!m_redis_cmd->hget(key, sub_key, val))
			{
				return false;
			}
			num = atoll(val.c_str());
			return true;
		}
		agebull::zmq_net::shared_char get_hash_ptr(const char* key, const char* sub_key) const
		{
			acl::string val;
			if (!m_redis_cmd->hget(key, sub_key, val))
			{
				return agebull::zmq_net::shared_char();
			}
			return agebull::zmq_net::shared_char(val);
		}
		long get_hash_num(const char* key, const char* sub_key) const
		{
			acl::string val;
			if (!m_redis_cmd->hget(key, sub_key, val))
			{
				return 0L;
			}
			return atol(val.c_str());
		}
		long long get_hash_int64(const char* key, const char* sub_key) const
		{
			acl::string val;
			if (!m_redis_cmd->hget(key, sub_key, val))
			{
				return 0L;
			}
			return atoll(val.c_str());
		}

	};

	/**
	* \brief Redis当前上下文对象生存范围
	*/
	class redis_live_scope
	{
		trans_redis* redis_;
		bool open_by_me_;
		int old_db_;
	public:
		/**
		* \brief 构造
		*/
		redis_live_scope();
		/**
		* \brief 对象获取
		*/
		acl::redis* operator->() const
		{
			return redis_->m_redis_cmd;
		}
		/**
		* \brief 对象获取
		*/
		trans_redis* t() const
		{
			return redis_;
		}
		/**
		* \brief 构造
		*/
		redis_live_scope(int db);

		/**
		* \brief 析构
		*/
		~redis_live_scope();
	};
	/**
	* \brief 自动开启和提交的事务范围
	*/
	class redis_trans_scope
	{
	public:
		/**
		* \brief 构造
		*/
		redis_trans_scope();

		/**
		* \brief 析构
		*/
		~redis_trans_scope();
	};

	inline bool ping_redis()
	{
		return trans_redis::get_context()->ping();
	}
	inline acl::string read_str_from_redis(const char* key)
	{
		return trans_redis::get_context().read_str_from_redis(key);
	}
	template<class TArg1>
	inline acl::string read_str_from_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return read_str_from_redis(key);
	}
	template<class TArg1, class TArg2>
	inline acl::string read_str_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return read_str_from_redis(key);
	}
	template<class TArg1, class TArg2, class TArg3>
	inline acl::string read_str_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return read_str_from_redis(key);
	}

	inline std::vector<acl::string> find_redis_keys(const char* key)
	{
		return trans_redis::get_context().find_redis_keys(key);
	}
	template<class TArg1>
	inline std::vector<acl::string> find_redis_keys(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return find_redis_keys(key);
	}
	template<class TArg1, class TArg2>
	inline std::vector<acl::string> find_redis_keys(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return find_redis_keys(key);
	}
	template<class TArg1, class TArg2, class TArg3>
	inline std::vector<acl::string> find_redis_keys(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return find_redis_keys(key);
	}

	inline std::vector<acl::string*> find_from_redis(const char* key)
	{
		return trans_redis::get_context().find_from_redis(key);
	}
	template<class TArg1>
	inline std::vector<acl::string*> find_from_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return find_from_redis(key);
	}
	template<class TArg1, class TArg2>
	inline std::vector<acl::string*> find_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return find_from_redis(key);
	}
	template<class TArg1, class TArg2, class TArg3>
	inline std::vector<acl::string*> find_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return find_from_redis(key);
	}
	inline acl::string read_from_redis(const char* key)
	{
		return trans_redis::get_context().read_from_redis(key);
	}
	template<class TArg1>
	inline acl::string read_from_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return read_from_redis(key);
	}
	template<class TArg1, class TArg2>
	inline acl::string read_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return read_from_redis(key);
	}
	template<class TArg1, class TArg2, class TArg3>
	inline acl::string read_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return read_from_redis(key);
	}

	inline void delete_from_redis(const char* key)
	{
		return trans_redis::get_context().delete_from_redis(key);
	}

	template<class TArg1>
	inline void delete_from_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		delete_from_redis(key);
	}
	template<class TArg1, class TArg2>
	inline void delete_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		delete_from_redis(key);
	}
	template<class TArg1, class TArg2, class TArg3>
	inline void delete_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		delete_from_redis(key);
	}

	inline size_t incr_redis(const char* key)
	{
		return trans_redis::get_context().incr_redis(key);
	}
	template<class TArg1>
	inline size_t incr_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return incr_redis(key);
	}
	template<class TArg1, class TArg2>
	inline size_t incr_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return incr_redis(key);
	}
	template<class TArg1, class TArg2, class TArg3>
	inline size_t incr_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return incr_redis(key);
	}
	inline void write_to_redis(const char* key, const char* bin, size_t len)
	{
		return trans_redis::get_context().write_to_redis(key, bin, len);
	}
	inline void write_json_to_redis(const char* key, const char* json)
	{
		return trans_redis::get_context().write_json_to_redis(key, json);
	}
	template<class TArg1>
	inline void write_to_redis(const char* bin, size_t len, const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		write_to_redis(key, bin, len);
	}
	template<class TArg1, class TArg2>
	inline void write_to_redis(const char* bin, size_t len, const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		write_to_redis(key, bin, len);
	}
	template<class TArg1, class TArg2, class TArg3>
	inline void write_to_redis(const char* bin, size_t len, const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		write_to_redis(key, bin, len);
	}


	inline acl::string read_first_from_redis(const char* key)
	{
		return trans_redis::get_context().read_first_from_redis(key);
	}
	template<class TArg1>
	inline acl::string read_first_from_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return read_first_from_redis(key);
	}
	template<class TArg1, class TArg2>
	inline acl::string read_first_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return read_first_from_redis(key);
	}
	template<class TArg1, class TArg2, class TArg3>
	inline acl::string read_first_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return read_first_from_redis(key);
	}

	inline int append_redis(const char* key, const char* value)
	{
		return trans_redis::get_context()->append(key, value);
	}


	inline bool unlock_from_redis(const char* key)
	{
		return trans_redis::get_context().unlock_from_redis(key);
	}
	template<class TArg1>
	inline bool unlock_from_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return unlock_from_redis(key);
	}
	template<class TArg1, class TArg2>
	inline bool unlock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return unlock_from_redis(key);
	}
	template<class TArg1, class TArg2, class TArg3>
	inline bool unlock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return unlock_from_redis(key);
	}

	inline bool lock_from_redis(const char* key)
	{
		return trans_redis::get_context().lock_from_redis(key);
	}
	template<class TArg1>
	inline bool lock_from_redis(const char* key_fmt, TArg1 arg)
	{
		char key[256];
		sprintf(key, key_fmt, arg);
		return lock_from_redis(key);
	}
	template<class TArg1, class TArg2>
	inline bool lock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2);
		return lock_from_redis(key);
	}
	template<class TArg1, class TArg2, class TArg3>
	inline bool lock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
	{
		char key[256];
		sprintf(key, key_fmt, arg1, arg2, arg3);
		return lock_from_redis(key);
	}
}
#endif
#endif