#ifndef CACHE_REDIS_H
#define CACHE_REDIS_H
#pragma once
#include "../stdinc.h"
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

	class redis_db_scope;
	class redis_trans_scope;
	/**
	* \brief 事务Redis 在没有启用时,与普通使用一样,启用时调用begin_trans,提交调用commit,回退调用rollback,且必须成对调用
	*/
	class trans_redis
	{
		friend class redis_db_scope;
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
	};

	/**
	* \brief Redis当前上下文对象生存范围
	*/
	class redis_live_scope
	{
		bool open_by_me_;
	public:
		/**
		* \brief 构造
		*/
		redis_live_scope();

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
	* \brief 自动恢复的数据ID范围
	*/
	class redis_db_scope
	{
		trans_redis& redis_;
	public:
		/**
		* \brief 构造
		*/
		redis_db_scope(int db);
		/**
		* \brief 析构
		*/
		~redis_db_scope();
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
}
#endif
