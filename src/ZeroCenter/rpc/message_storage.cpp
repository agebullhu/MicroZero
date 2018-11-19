/**
 * ZMQ通知代理类
 *
 *
 */

#include "../stdafx.h"
#include "station_warehouse.h"
#include "message_storage.h"

namespace agebull
{
	namespace zero_net
	{
		//const char* publiher, const char* title, const char* sub, const char* arg, const char* rid, const int64 gid
		const char* create_sql =
			"CREATE TABLE tb_message(" \
			"	local_id  BIGINT PRIMARY KEY," \
			"	global_id BIGINT, " \
			"	publiher  NVARCHAR(200)," \
			"	pri_title NVARCHAR(200)," \
			"	sub_title NVARCHAR(200)," \
			"	rid       NVARCHAR(200)," \
			"	arg       TEXT" \
			");";

		//const char* publiher, const char* title, const char* sub, const char* arg, const char* rid, const int64 gid
		const char* insert_sql = "insert into tb_message values(?,?,?,?,?,?,?);";

		const char* load_max_sql = "select max(local_id) from tb_message;";
		const char* load_sql = "select * from tb_message where local_id > ? AND local_id < ?;";
		/**
		 * \brief 准备存储
		 */
		bool message_storage::prepare_storage(shared_ptr<zero_config>& config)
		{
			_config = config;

			acl::string path;
			path.format("%s/datas/%s.db", global_config::root_path, config->station_name_.c_str());
			int result = sqlite3_open(path.c_str(), &_sqlite_db);
			if (result != SQLITE_OK)
			{
				log_error3("[%s] : db > Can't open database(%s):%s", _config->station_name_.c_str(), path.c_str(), sqlite3_errmsg(_sqlite_db));
				sqlite3_close(_sqlite_db);
				_sqlite_db = nullptr;
				return false;
			}
			sqlite3_exec(_sqlite_db, "PRAGMA synchronous = OFF; ", nullptr, nullptr, nullptr);
			if (!read_last_id())
			{
				sqlite3_close(_sqlite_db);
				_sqlite_db = nullptr;
				return false;
			}
			sqlite3_prepare_v2(_sqlite_db, insert_sql, static_cast<int>(strlen(insert_sql)), &_insert_stmt, nullptr);
			sqlite3_prepare_v2(_sqlite_db, load_sql, static_cast<int>(strlen(load_sql)), &_load_stmt, nullptr);
			log_msg2("[%s] : db > Open database(%s)", _config->station_name_.c_str(), path.c_str());
			return true;
		}
		bool message_storage::read_last_id()
		{
			char * errmsg = nullptr;
			char **db_result = nullptr;
			int row, column;
			auto ex_result = sqlite3_get_table(_sqlite_db, load_max_sql, &db_result, &row, &column, &errmsg);
			if (ex_result != SQLITE_OK)
			{
				sqlite3_free_table(db_result);
				//建立数据库
				ex_result = sqlite3_exec(_sqlite_db, create_sql, nullptr, nullptr, &errmsg);
				if (ex_result != SQLITE_OK)
				{
					log_error2("[%s] : db > Can't create table tb_message:%s", _config->station_name_.c_str(), errmsg);
					return false;
				}
				sqlite3_exec(_sqlite_db, "CREATE INDEX[main].[lid] ON[tb_message]([local_id]);", nullptr, nullptr, &errmsg);
				sqlite3_exec(_sqlite_db, "CREATE INDEX[main].[gid] ON[tb_message]([global_id]);", nullptr, nullptr, &errmsg);
				log_msg1("[%s] : db > Create table tb_message", _config->station_name_.c_str());



				ex_result = sqlite3_get_table(_sqlite_db, load_max_sql, &db_result, &row, &column, &errmsg);
				if (ex_result != SQLITE_OK)
				{
					log_error2("[%s] : db > Can't open table tb_message:%s", _config->station_name_.c_str(), errmsg);
					return false;
				}
			}
			char * id = db_result[column];
			if (id != nullptr)
				_last_id = atoll(id);
			sqlite3_free_table(db_result);
			return true;
		}
		/**
		*\brief 将数据写入数据库中
		*/
		int64 message_storage::save(const char* title, const char* sub, const char* arg, const char* reqid, const char* publiher, const int64 gid)
		{
			sqlite3_reset(_insert_stmt);
			int idx = 0;
			sqlite3_bind_int64(_insert_stmt, ++idx, ++_last_id);//local_id
			sqlite3_bind_int64(_insert_stmt, ++idx, gid);//global_id
			if (publiher == nullptr)
				sqlite3_bind_null(_insert_stmt, ++idx);
			else
				sqlite3_bind_text(_insert_stmt, ++idx, publiher, static_cast<int>(strlen(publiher)), nullptr);//publiher
			if (title == nullptr)
				sqlite3_bind_null(_insert_stmt, ++idx);
			else
				sqlite3_bind_text(_insert_stmt, ++idx, title, static_cast<int>(strlen(title)), nullptr);//pri_title
			if (sub == nullptr)
				sqlite3_bind_null(_insert_stmt, ++idx);
			else
				sqlite3_bind_text(_insert_stmt, ++idx, sub, static_cast<int>(strlen(title)), nullptr);//sub_title
			if (reqid == nullptr)
				sqlite3_bind_null(_insert_stmt, ++idx);
			else
				sqlite3_bind_text(_insert_stmt, ++idx, reqid, static_cast<int>(strlen(reqid)), nullptr);//rid
			if (arg == nullptr)
				sqlite3_bind_null(_insert_stmt, ++idx);
			else
				sqlite3_bind_text(_insert_stmt, ++idx, arg, static_cast<int>(strlen(arg)), nullptr);//arg
			if (sqlite3_step(_insert_stmt) == SQLITE_DONE)
				return _last_id;
			log_error2("[%s] : db > Can't save data:%s", _config->station_name_.c_str(), sqlite3_errmsg(_sqlite_db));
			return 0;
		}
		char frames4[] = { ZERO_FRAME_PUBLISHER,ZERO_FRAME_SUBTITLE, ZERO_FRAME_CONTENT,ZERO_FRAME_REQUEST_ID, ZERO_FRAME_GLOBAL_ID,ZERO_FRAME_LOCAL_ID };
		/**
		*\brief 取数据
		*/
		void message_storage::load(int64 min, int64 max, std::function<void(vector<shared_char>&)> exec)
		{
			sqlite3_reset(_load_stmt);
			sqlite3_bind_int64(_load_stmt, 1, min);
			sqlite3_bind_int64(_load_stmt, 2, max ==0 ? _last_id +1: max);
			while (sqlite3_step(_load_stmt) == SQLITE_ROW)
			{
				vector<shared_char> row;
				row.emplace_back(sqlite3_column_text(_load_stmt, 3));//pri_title
				shared_char description;
				description.alloc_frame(frames4);
				row.emplace_back(description);
				row.emplace_back(sqlite3_column_text(_load_stmt, 2));//pri_title
				row.emplace_back(sqlite3_column_text(_load_stmt, 4));//sub_title
				row.emplace_back(sqlite3_column_text(_load_stmt, 6));//arg
				row.emplace_back(sqlite3_column_text(_load_stmt, 5));//rid
				//global_id
				shared_char global_id;
				global_id.set_int64x(sqlite3_column_int64(_load_stmt, 1));
				row.emplace_back(global_id);
				//local_id
				shared_char local_id;
				local_id.set_int64x(sqlite3_column_int64(_load_stmt, 0));
				row.emplace_back(local_id);
				exec(row);
			}
		}
	}
}
