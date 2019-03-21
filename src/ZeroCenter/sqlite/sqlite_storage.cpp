/**
 * ZMQ通知代理类
 *
 *
 */

#include "../stdafx.h"
#include "sqlite_storage.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		 * \brief 打开DB
		 */
		bool sqlite_storage::open_db(const char* file)
		{
			strcpy(db_file_, file);
			const int result = sqlite3_open(db_file_, &sqlite_db_);
			if (result != SQLITE_OK)
			{
				log_error3("[%s] : db > Can't open database(%s):%s", name_, db_file_, sqlite3_errmsg(sqlite_db_));
				sqlite3_close(sqlite_db_);
				sqlite_db_ = nullptr;
				return false;
			}
			log_msg2("[%s] : db > Open database(%s)", name_, db_file_);
			sqlite3_exec(sqlite_db_, "PRAGMA synchronous = OFF; ", nullptr, nullptr, nullptr);
			//sqlite3_exec(sqlite_db_, "PRAGMA temp_store = MEMORY; ", nullptr, nullptr, nullptr);
			//sqlite3_exec(sqlite_db_, "PRAGMA cache_size = 20000; ", nullptr, nullptr, nullptr);

			sqlite3_prepare_v2(sqlite_db_, "BEGIN", 5, &begin_trans_stmt_, nullptr);
			sqlite3_prepare_v2(sqlite_db_, "COMMIT", 6, &end_trans_stmt_, nullptr);
			return true;
		}
		/**
		 * \brief 开始事务
		 */
		bool sqlite_storage::begin_trans()
		{
			if (in_trans_)
				return false;
			auto state = sqlite3_step(begin_trans_stmt_);
			if (state != SQLITE_DONE)
			{
				log_error2("[%s] : db > Can't begin transaction:%s", name_, sqlite3_errmsg(sqlite_db_));
				return false;
			}
			in_trans_ = true;
			return true;
		}

		/**
		 * \brief 提交事务
		 */
		bool sqlite_storage::commit_trans()
		{
			if (!in_trans_)
				return false;
			auto state = sqlite3_step(end_trans_stmt_);
			if (state != SQLITE_DONE)
			{
				log_error2("[%s] : db > Can't commit transaction:%s", name_, sqlite3_errmsg(sqlite_db_));
				return false;
			}
			in_trans_ = false;
			return true;
		}

		/**
		 * \brief 不存在就建立这个表
		 */
		bool sqlite_storage::try_create(const char* table, const char* create_sql, vector<const char *> ext_sql)
		{
			char * errmsg = nullptr;
			char **db_result = nullptr;
			int row, column;
			acl::string sql;
			sql.format("select rowid from %s", table);
			auto ex_result = sqlite3_get_table(sqlite_db_, sql.c_str(), &db_result, &row, &column, &errmsg);
			sqlite3_free_table(db_result);
			if (ex_result == SQLITE_OK)
				return true;
			//建立数据库
			ex_result = sqlite3_exec(sqlite_db_, create_sql, nullptr, nullptr, &errmsg);
			if (ex_result != SQLITE_OK)
			{
				log_error3("[%s] : db > Can't create table %s:%s", name_, table, errmsg);
				return false;
			}
			for(auto ext : ext_sql)
				sqlite3_exec(sqlite_db_, ext, nullptr, nullptr, &errmsg);
			return true;
		}
	}
}
