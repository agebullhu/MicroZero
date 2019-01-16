#ifndef ZMQ_API_SQLITE_STORAGE_H
#define ZMQ_API_SQLITE_STORAGE_H
#pragma once
#include "../stdinc.h"
#include <sqlite3.h>

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 消息持久化存储(使用SQLite)
		*/
		class sqlite_storage
		{
			sqlite3_stmt *begin_trans_stmt_;
			sqlite3_stmt *end_trans_stmt_;
		protected:
			bool in_trans_;
			sqlite3* sqlite_db_;
			char name_[256],db_file_[256];
		public:
			/**
			 * \brief 构造
			 */
			sqlite_storage() : begin_trans_stmt_(nullptr), end_trans_stmt_(nullptr), in_trans_(false),
			                   sqlite_db_(nullptr)
			{
				name_[0] = '\0';
				db_file_[0] = '\0';
			}

			/**
			 * \brief 构造
			 */
			virtual  ~sqlite_storage()
			{
				destruct();
			}

			/**
			 * \brief 构造
			 */
			void destruct()
			{
				commit_trans();
				if (sqlite_db_ != nullptr)
					sqlite3_close(sqlite_db_);
				if (begin_trans_stmt_)
					sqlite3_finalize(begin_trans_stmt_);
				if (end_trans_stmt_)
					sqlite3_finalize(end_trans_stmt_);
			}

			/**
			 * \brief 打开DB
			 */
			bool open_db(const char* file);


			/**
			 * \brief 开始事务
			 */
			bool begin_trans();

			/**
			 * \brief 提交事务
			 */
			bool commit_trans();
			/**
			 * \brief 不存在就建立这个表
			 */
			bool try_create(const char* table, const char* create_sql, vector<const char *> ext_sql);
		};

#define bind_column_text(_stmt_,value,col) \
		if (value == nullptr || strlen(value) == 0)\
			sqlite3_bind_null(_stmt_, col);\
		else\
			sqlite3_bind_text(_stmt_, col, value, static_cast<int>(strlen(value)), nullptr)
	}
}
#endif//!ZMQ_API_SQLITE_STORAGE_H