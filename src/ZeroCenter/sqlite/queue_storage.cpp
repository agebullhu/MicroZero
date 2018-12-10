/**
 * ZMQ通知代理类
 *
 *
 */

#include "../stdafx.h"
#include "queue_storage.h"

namespace agebull
{
	namespace zero_net
	{
		const char* queue_storage::create_sql_ =
			"CREATE TABLE tb_message(" \
			"	local_id  BIGINT PRIMARY KEY," \
			"	global_id BIGINT, " \
			"	publiher  NVARCHAR(200)," \
			"	pri_title NVARCHAR(200)," \
			"	sub_title NVARCHAR(200)," \
			"	rid       NVARCHAR(200)," \
			"	arg       TEXT" \
			");";
		const char* queue_storage::insert_sql_ = "insert into tb_message values(?,?,?,?,?,?,?);";
		const char* queue_storage::load_max_sql_ = "select max(local_id) from tb_message;";
		const char* queue_storage::load_sql_ = "select * from tb_message where local_id > ? AND local_id < ?;";
		/**
		 * \brief 准备存储
		 */
		bool queue_storage::prepare_storage(shared_ptr<zero_config>& config)
		{
			config_ = config;
			strcpy(name_, config->station_name.c_str());
			acl::string path;
			path.format("%s/datas/%s.db", global_config::root_path, name_);
			if (!open_db(path.c_str()))
			{
				return false;
			}
			if (!read_last_id())
			{
				sqlite3_close(sqlite_db_);
				sqlite_db_ = nullptr;
				return false;
			}
			sqlite3_prepare_v2(sqlite_db_, insert_sql_, static_cast<int>(strlen(insert_sql_)), &insert_stmt_, nullptr);
			sqlite3_prepare_v2(sqlite_db_, load_sql_, static_cast<int>(strlen(load_sql_)), &load_stmt_, nullptr);
			return true;
		}
		/**
		*\brief 取最大ID
		*/
		bool queue_storage::read_last_id()
		{
			char * errmsg = nullptr;
			char **db_result = nullptr;
			int row, column;
			auto ex_result = sqlite3_get_table(sqlite_db_, load_max_sql_, &db_result, &row, &column, &errmsg);
			if (ex_result != SQLITE_OK)
			{
				sqlite3_free_table(db_result);
				//建立数据库
				ex_result = sqlite3_exec(sqlite_db_, create_sql_, nullptr, nullptr, &errmsg);
				if (ex_result != SQLITE_OK)
				{
					log_error2("[%s] : db > Can't create table tb_message:%s", name_, errmsg);
					return false;
				}
				sqlite3_exec(sqlite_db_, "CREATE INDEX[main].[lid] ON[tb_message]([local_id]);", nullptr, nullptr, &errmsg);
				sqlite3_exec(sqlite_db_, "CREATE INDEX[main].[gid] ON[tb_message]([global_id]);", nullptr, nullptr, &errmsg);
				log_msg1("[%s] : db > Create table tb_message", name_);

				ex_result = sqlite3_get_table(sqlite_db_, load_max_sql_, &db_result, &row, &column, &errmsg);
				if (ex_result != SQLITE_OK)
				{
					log_error2("[%s] : db > Can't open table tb_message:%s", name_, errmsg);
					return false;
				}
			}
			char * id = db_result[column];
			if (id != nullptr)
				last_id_ = atoll(id);
			sqlite3_free_table(db_result);
			return true;
		}
		/**
		*\brief 将数据写入数据库中
		*/
		int64 queue_storage::save(const char* title, const char* sub, const char* arg, const char* reqid, const char* publiher, const int64 gid)
		{
			sqlite3_reset(insert_stmt_);
			int idx = 0;
			sqlite3_bind_int64(insert_stmt_, ++idx, ++last_id_);//local_id
			sqlite3_bind_int64(insert_stmt_, ++idx, gid);//global_id
			if (publiher == nullptr)
				sqlite3_bind_null(insert_stmt_, ++idx);
			else
				sqlite3_bind_text(insert_stmt_, ++idx, publiher, static_cast<int>(strlen(publiher)), nullptr);//publiher
			if (title == nullptr)
				sqlite3_bind_null(insert_stmt_, ++idx);
			else
				sqlite3_bind_text(insert_stmt_, ++idx, title, static_cast<int>(strlen(title)), nullptr);//pri_title
			if (sub == nullptr)
				sqlite3_bind_null(insert_stmt_, ++idx);
			else
				sqlite3_bind_text(insert_stmt_, ++idx, sub, static_cast<int>(strlen(title)), nullptr);//sub_title
			if (reqid == nullptr)
				sqlite3_bind_null(insert_stmt_, ++idx);
			else
				sqlite3_bind_text(insert_stmt_, ++idx, reqid, static_cast<int>(strlen(reqid)), nullptr);//rid
			if (arg == nullptr)
				sqlite3_bind_null(insert_stmt_, ++idx);
			else
				sqlite3_bind_text(insert_stmt_, ++idx, arg, static_cast<int>(strlen(arg)), nullptr);//arg
			if (sqlite3_step(insert_stmt_) == SQLITE_DONE)
				return last_id_;
			log_error2("[%s] : db > Can't save data:%s", name_, sqlite3_errmsg(sqlite_db_));
			return 0;
		}
		char queue_storage::frames4_[] = { zero_def::frame::publisher,zero_def::frame::sub_title, zero_def::frame::content,zero_def::frame::request_id, zero_def::frame::global_id,zero_def::frame::local_id };
		/**
		*\brief 取数据
		*/
		void queue_storage::load(int64 min, int64 max, std::function<void(vector<shared_char>&)> exec)
		{
			if (max == 0)
				max = last_id_ + 1;
			if (min >= max || max - 1 == min)
				return;
			sqlite3_reset(load_stmt_);
			sqlite3_bind_int64(load_stmt_, 1, min);
			sqlite3_bind_int64(load_stmt_, 2, max ==0 ? last_id_ +1: max);
			while (sqlite3_step(load_stmt_) == SQLITE_ROW)
			{
				vector<shared_char> row;
				row.emplace_back(sqlite3_column_text(load_stmt_, 3));//pri_title
				shared_char description;
				description.alloc_desc(frames4_);
				row.emplace_back(description);
				row.emplace_back(sqlite3_column_text(load_stmt_, 2));//pri_title
				row.emplace_back(sqlite3_column_text(load_stmt_, 4));//sub_title
				row.emplace_back(sqlite3_column_text(load_stmt_, 6));//arg
				row.emplace_back(sqlite3_column_text(load_stmt_, 5));//rid
				//global_id
				shared_char global_id;
				global_id.set_int64x(sqlite3_column_int64(load_stmt_, 1));
				row.emplace_back(global_id);
				//local_id
				shared_char local_id;
				local_id.set_int64x(sqlite3_column_int64(load_stmt_, 0));
				row.emplace_back(local_id);
				exec(row);
			}
		}
	}
}
