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
		protected:
			sqlite3* sqlite_db_;
			char name_[256],db_file_[256];
		public:
			/**
			 * \brief 构造
			 */
			sqlite_storage() : sqlite_db_(nullptr)
			{
				name_[0] = '\0';
				db_file_[0] = '\0';
			}

			/**
			 * \brief 构造
			 */
			virtual  ~sqlite_storage()
			{
				if (sqlite_db_ == nullptr)
					return;
				sqlite3_close(sqlite_db_);
			}

			/**
			 * \brief 打开DB
			 */
			bool open_db(const char* file);

			/**
			 * \brief 不存在就建立这个表
			 */
			bool try_create(const char* table, const char* create_sql, vector<const char *> ext_sql);
		};
	}
}
#endif//!ZMQ_API_SQLITE_STORAGE_H