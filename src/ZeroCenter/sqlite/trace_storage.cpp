/**
 * ZMQ通知代理类
 *
 *
 */

#include "../stdafx.h"
#include "trace_storage.h"

namespace agebull
{
	namespace zero_net
	{
		const char* trace_storage::trace_create_sql_ =
			"CREATE TABLE tb_trace(" \
			"	id		  INTEGER PRIMARY KEY AUTOINCREMENT," \
			"	rquest_id NVARCHAR(200) NULL," \
			"	call_id   BIGINT, " \
			"	global_id BIGINT, " \
			"	station	  NVARCHAR(200) NULL," \
			"	type	  NVARCHAR(20) NULL," \
			"	state	  TEXT," \
			"	in_out	  INTEGER" \
			");";
		const char* trace_storage::trace_insert_sql_ = "insert into tb_trace values(NULL,?,?,?,?,?,?,?);";
		const char* trace_storage::trace_last_sql_ = "select last_insert_rowid() from tb_trace;";

		const char* trace_storage::frame_create_sql_ =
			"CREATE TABLE tb_frames(" \
			"	id		  INTEGER PRIMARY KEY AUTOINCREMENT," \
			"	trace_id  BIGINT," \
			"	[index]   INTEGER," \
			"	frame	  BLOB  NULL" \
			");";
		const char* trace_storage::frame_insert_sql_ = "insert into tb_frames values(NULL,?,?,?);";

		/**
		 * \brief 准备存储
		 */
		bool trace_storage::prepare_storage()
		{
			time_t tt = time(nullptr);
			tm day = *(localtime(&tt));
			acl::string path;
			path.format("%s/datas/zero_trace.%04d%02d%02d.db", global_config::root_path, day.tm_year + 1900, day.tm_mon + 1, day.tm_mday);
			if (!open_db(path.c_str()))
			{
				return false;
			}
			char * errmsg = nullptr;
			//建立数据库
			if (sqlite3_exec(sqlite_db_, trace_create_sql_, nullptr, nullptr, &errmsg) == SQLITE_OK)
			{
				//sqlite3_exec(sqlite_db_, "CREATE INDEX[main].[trace_id] ON[tb_trace]([id]);", nullptr, nullptr, &errmsg);
				//sqlite3_exec(sqlite_db_, "CREATE INDEX[main].[trace_global_id] ON[tb_trace]([global_id]);", nullptr, nullptr, &errmsg);
				//sqlite3_exec(sqlite_db_, "CREATE INDEX[main].[trace_call_id] ON[tb_trace]([call_id]);", nullptr, nullptr, &errmsg);
				//sqlite3_exec(sqlite_db_, "CREATE INDEX[main].[trace_call_id] ON[tb_trace]([call_id]);", nullptr, nullptr, &errmsg);
				log_msg1("[%s] : db > Create table tb_trace", name_);
			}
			if (sqlite3_exec(sqlite_db_, frame_create_sql_, nullptr, nullptr, &errmsg) == SQLITE_OK)
			{
				//sqlite3_exec(sqlite_db_, "CREATE INDEX[main].[frames_id] ON[tb_frames]([id]);", nullptr, nullptr, &errmsg);
				//sqlite3_exec(sqlite_db_, "CREATE INDEX[main].[frames_trace_id] ON[tb_frames]([trace_id]);", nullptr, nullptr, &errmsg);
				//sqlite3_exec(sqlite_db_, "CREATE INDEX[main].[frames_index] ON[tb_frames]([index]);", nullptr, nullptr, &errmsg);
				log_msg1("[%s] : db > Create table tb_frames", name_);
			}
			if (sqlite3_prepare_v2(sqlite_db_, trace_insert_sql_, static_cast<int>(strlen(trace_insert_sql_)), &trace_insert_stmt_, nullptr) != SQLITE_OK)
			{
				log_error2("[%s] : db > Can't open tb_trace :%s", name_, sqlite3_errmsg(sqlite_db_));
				return false;
			}
			if (sqlite3_prepare_v2(sqlite_db_, trace_last_sql_, static_cast<int>(strlen(trace_last_sql_)), &trace_last_stmt_, nullptr) != SQLITE_OK)
			{
				log_error2("[%s] : db > Can't open tb_trace :%s", name_, sqlite3_errmsg(sqlite_db_));
				return false;
			}
			if (sqlite3_prepare_v2(sqlite_db_, frame_insert_sql_, static_cast<int>(strlen(frame_insert_sql_)), &frame_insert_stmt_, nullptr) != SQLITE_OK)
			{
				log_error2("[%s] : db > Can't open tb_frames :%s", name_, sqlite3_errmsg(sqlite_db_));
				return false;
			}
			trace_day = day;
			return true;
		}

		/**
		*\brief 将数据写入数据库中
		*/
		void trace_storage::save(int in_out, const char* reqid, const int64 par_id, const int64 my_id, const char* station_name, const char*  station_type, const char*  state_str, vector<shared_char>& frames)
		{
			time_t tt = time(nullptr);
			tm day = *(localtime(&tt));
			if (day.tm_year != trace_day.tm_year || day.tm_mon != trace_day.tm_mon || day.tm_mday != trace_day.tm_mday)
			{
				sqlite3_finalize(frame_insert_stmt_);
				sqlite3_finalize(trace_insert_stmt_);
				sqlite3_close(sqlite_db_);
				if (!prepare_storage())
					return;
			}
			sqlite3_reset(trace_insert_stmt_);
			int idx = 0;
			bind_column_text(trace_insert_stmt_, reqid, ++idx);
			sqlite3_bind_int64(trace_insert_stmt_, ++idx, par_id);
			sqlite3_bind_int64(trace_insert_stmt_, ++idx, my_id);
			bind_column_text(trace_insert_stmt_, station_name, ++idx);
			bind_column_text(trace_insert_stmt_, station_type, ++idx);
			bind_column_text(trace_insert_stmt_, state_str, ++idx);
			sqlite3_bind_int(trace_insert_stmt_, ++idx, in_out);
			auto state = sqlite3_step(trace_insert_stmt_);
			if (state != SQLITE_DONE)
			{
				log_error2("[%s] : db > Can't insert tb_trace :%s", name_, sqlite3_errmsg(sqlite_db_));
				return;
			}
			state = sqlite3_step(trace_last_stmt_);
			if (state != SQLITE_ROW)
			{
				log_error2("[%s] : db > Can't insert tb_trace :%s", name_, sqlite3_errmsg(sqlite_db_));
				return;
			}
			//state = sqlite3_next_stmt(sqlite_db_, trace_insert_stmt_);
			//if (state != SQLITE_ROW)
			//{
			//	log_error2("[%s] : db > Can't insert tb_trace :%s", name_, sqlite3_errmsg(sqlite_db_));
			//	return;
			//}
			int id = sqlite3_column_int(trace_last_stmt_, 0);
			/*char * errmsg = nullptr;
			char **db_result = nullptr;
			int row, column;
			auto ex_result = sqlite3_get_table(sqlite_db_, "select last_insert_rowid() from tb_trace", &db_result, &row, &column, &errmsg);
			if (ex_result != SQLITE_OK)
			{
				log_error2("[%s] : db > Can't insert tb_trace :%s", name_, errmsg);
				return;
			}
			char * ids = db_result[column];
			if (ids == nullptr)
			{
				log_error1("[%s] : db > Can't insert tb_trace : identity error", name_);
				return;
			}
			int64 id = atoll(ids);*/
			//sqlite3_free_table(db_result);

			for (int i = 0; i < static_cast<int>(frames.size()); i++)
			{
				sqlite3_reset(frame_insert_stmt_);
				sqlite3_bind_int64(frame_insert_stmt_, 1, id);
				sqlite3_bind_int(frame_insert_stmt_, 2, i + 1);
				sqlite3_bind_blob(frame_insert_stmt_, 3, *frames[i], static_cast<int>(frames[i].size()), nullptr);
				if (sqlite3_step(frame_insert_stmt_) != SQLITE_DONE)
					log_error2("[%s] : db > Can't insert tb_frame:%s", name_, sqlite3_errmsg(sqlite_db_));
			}
		}
	}
}
