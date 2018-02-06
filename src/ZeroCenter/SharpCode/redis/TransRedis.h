
#ifndef CLIENT
#ifndef CACHE_REDIS_H
#define CACHE_REDIS_H
#pragma once
#include <stdinc.h>
using namespace std;
namespace agebull
{
	//消息队列的消息内容
#define REDIS_DB_ZERO_VOTE 0x15
	//消息队列的消息内容
#define REDIS_DB_ZERO_PLAN 0x14
	//消息队列的消息内容
#define REDIS_DB_ZERO_MESSAGE 0x13
	//站点配置
#define REDIS_DB_ZERO_STATION 0x12
	//系统
#define REDIS_DB_ZERO_SYSTEM 0x11
	/**
	* \brief 事务Redis 在没有启用时,与普通使用一样,启用时调用begin_trans,提交调用commit,回退调用rollback,且必须成对调用
	*/
	class trans_redis
	{
		/**
		* \brief 启用事务的次数
		*/
		int m_trans_num;
		/**
		* \brief 是否有回退的调用,如果有所有即失败,除非显式设置为不回退
		*/
		bool m_failed;
		/**
		* \brief acl的redis客户端对象
		*/
		acl::redis_client m_redis_client;
		/**
		* \brief acl的redis命令对象
		*/
		acl::redis m_redis_cmd;
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
		* \brief 构造
		*/
		trans_redis();
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
		static void open_context();
		/**
		* \brief 关闭当前线程上下文的事务Redis对象
		*/
		static void close_context();
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
	private:
		/**
		* \brief 提交事务
		*/
		void trans_redis::commit_inner();
		bool get(const char*, acl::string&);
		void set(const char*, const char*);
		void set(const char*, const char*, size_t);
		void set(const char*, acl::string&);
	public:
		acl::redis* operator->()
		{
			return &m_redis_cmd;
		}
		acl::string read_str_from_redis(const char* key);
		template<class TArg1>
		acl::string read_str_from_redis(const char* key_fmt, TArg1 arg)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg);
			return read_str_from_redis(key);
		}
		template<class TArg1, class TArg2>
		acl::string read_str_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2);
			return read_str_from_redis(key);
		}
		template<class TArg1, class TArg2, class TArg3>
		acl::string read_str_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2, arg3);
			return read_str_from_redis(key);
		}

		std::vector<acl::string> find_redis_keys(const char* key);
		template<class TArg1>
		std::vector<acl::string> find_redis_keys(const char* key_fmt, TArg1 arg)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg);
			return find_redis_keys(key);
		}
		template<class TArg1, class TArg2>
		std::vector<acl::string> find_redis_keys(const char* key_fmt, TArg1 arg1, TArg2 arg2)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2);
			return find_redis_keys(key);
		}
		template<class TArg1, class TArg2, class TArg3>
		std::vector<acl::string> find_redis_keys(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2, arg3);
			return find_redis_keys(key);
		}

		std::vector<acl::string*> find_from_redis(const char* key);
		template<class TArg1>
		std::vector<acl::string*> find_from_redis(const char* key_fmt, TArg1 arg)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg);
			return find_from_redis(key);
		}
		template<class TArg1, class TArg2>
		std::vector<acl::string*> find_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2);
			return find_from_redis(key);
		}
		template<class TArg1, class TArg2, class TArg3>
		std::vector<acl::string*> find_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2, arg3);
			return find_from_redis(key);
		}
		acl::string read_from_redis(const char* key);
		template<class TArg1>
		acl::string read_from_redis(const char* key_fmt, TArg1 arg)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg);
			return read_from_redis(key);
		}
		template<class TArg1, class TArg2>
		acl::string read_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2);
			return read_from_redis(key);
		}
		template<class TArg1, class TArg2, class TArg3>
		acl::string read_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2, arg3);
			return read_from_redis(key);
		}

		void delete_from_redis(const char* key);

		template<class TArg1>
		void delete_from_redis(const char* key_fmt, TArg1 arg)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg);
			delete_from_redis(key);
		}
		template<class TArg1, class TArg2>
		void delete_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2);
			delete_from_redis(key);
		}
		template<class TArg1, class TArg2, class TArg3>
		void delete_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2, arg3);
			delete_from_redis(key);
		}

		size_t incr_redis(const char* key);
		template<class TArg1>
		size_t incr_redis(const char* key_fmt, TArg1 arg)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg);
			return incr_redis(key);
		}
		template<class TArg1, class TArg2>
		size_t incr_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2);
			return incr_redis(key);
		}
		template<class TArg1, class TArg2, class TArg3>
		size_t incr_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2, arg3);
			return incr_redis(key);
		}
		void write_to_redis(const char* key, const char* bin, size_t len);
		void write_json_to_redis(const char* key, const char* json);
		template<class TArg1>
		void write_to_redis(const char* bin, size_t len, const char* key_fmt, TArg1 arg)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg);
			write_to_redis(key, bin, len);
		}
		template<class TArg1, class TArg2>
		void write_to_redis(const char* bin, size_t len, const char* key_fmt, TArg1 arg1, TArg2 arg2)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2);
			write_to_redis(key, bin, len);
		}
		template<class TArg1, class TArg2, class TArg3>
		void write_to_redis(const char* bin, size_t len, const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2, arg3);
			write_to_redis(key, bin, len);
		}


		acl::string read_first_from_redis(const char* key);
		template<class TArg1>
		acl::string read_first_from_redis(const char* key_fmt, TArg1 arg)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg);
			return read_first_from_redis(key);
		}
		template<class TArg1, class TArg2>
		acl::string read_first_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2);
			return read_first_from_redis(key);
		}
		template<class TArg1, class TArg2, class TArg3>
		acl::string read_first_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2, arg3);
			return read_first_from_redis(key);
		}


		bool unlock_from_redis(const char* key);
		template<class TArg1>
		bool unlock_from_redis(const char* key_fmt, TArg1 arg)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg);
			return unlock_from_redis(key);
		}
		template<class TArg1, class TArg2>
		bool unlock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2);
			return unlock_from_redis(key);
		}
		template<class TArg1, class TArg2, class TArg3>
		bool unlock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2, arg3);
			return unlock_from_redis(key);
		}

		bool lock_from_redis(const char* key);
		template<class TArg1>
		bool lock_from_redis(const char* key_fmt, TArg1 arg)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg);
			return lock_from_redis(key);
		}
		template<class TArg1, class TArg2>
		bool lock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2);
			return lock_from_redis(key);
		}
		template<class TArg1, class TArg2, class TArg3>
		bool lock_from_redis(const char* key_fmt, TArg1 arg1, TArg2 arg2, TArg3 arg3)
		{
			char key[256];
			sprintf_s(key, key_fmt, arg1, arg2, arg3);
			return lock_from_redis(key);
		}

		bool get_hash(const char* key, const char* sub_key, acl::string& vl);
		bool set_hash(const char* key, const char* sub_key, const char* vl);
		bool del_hash(const char* key, const char* sub_key);
		bool get_hash(const char* key, std::map<acl::string, acl::string>& vl);
	};
	/**
	* \brief Redis当前上下文对象生存范围
	*/
	class redis_live_scope
	{
	public:
		/**
		* \brief 构造
		*/
		redis_live_scope()
		{
			trans_redis::open_context();
		}
		/**
		* \brief 构造
		*/
		explicit redis_live_scope(int db)
		{
			trans_redis::open_context();
			trans_redis::get_context()->select(db);
		}
		/**
		* \brief 析构
		*/
		~redis_live_scope()
		{
			agebull::trans_redis::close_context();
		}
	};
	/**
	* \brief 自动恢复的数据ID范围
	*/
	class RedisDbScope
	{
	public:
		/**
		* \brief 构造
		*/
		RedisDbScope(int db)
		{
			trans_redis::get_context()->select(db);
		}
		/**
		* \brief 析构
		*/
		~RedisDbScope();
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
		redis_trans_scope()
		{
			trans_redis::get_context().begin_trans();
		}
		/**
		* \brief 析构
		*/
		~redis_trans_scope()
		{
			trans_redis* context = trans_redis::get_current();
			if (context != nullptr)
				trans_redis::end_trans();
		}
	};
}
#endif
#endif
