/**
 * ZMQ通知代理类
 *
 *
 */
#ifdef _ZERO_PLAN
#include "../stdafx.h"
#include "plan_storage.h"

namespace agebull
{
	namespace zero_net
	{
		const char* plan_storage::create_plan_sql_ =
			"CREATE TABLE tb_plan(" \
			"	plan_id   INTEGER," \
			"	plan_type INTEGER," \
			" request_id  TEXT," \
			"	station   TEXT," \
			"	command   TEXT," \
			"	json      TEXT" \
			");";

		const char* plan_storage::create_frame_sql_ =
			"CREATE TABLE tb_frame(" \
			"	[plan_id]   INTEGER," \
			"	[type]      INTEGER," \
			"	[index]     INTEGER," \
			"	[frame]     BLOB" \
			");";

		const char* plan_storage::create_log_sql_ =
			"CREATE TABLE tb_log(" \
			"	[plan_id]   INTEGER," \
			"	[date]      INTEGER," \
			"	[state]     INTEGER," \
			"	[json]      TEXT" \
			");";

		const char* plan_storage::insert_plan_sql_ = "insert into tb_plan values(?,?,?,?,?,?);";
		const char* plan_storage::insert_frame_sql_ = "insert into tb_frame values(?,?,?,?);";
		const char* plan_storage::insert_log_sql_ = "insert into tb_plan values(?,?,?,?);";
		/**
		 * \brief 准备存储
		 */
		bool plan_storage::prepare(shared_ptr<station_config>& config)
		{
			strcpy(name_, config->station_name.c_str());
			acl::string path;
			path.format("%s/datas/%s.db", global_config::root_path, name_);
			if (!open_db(path.c_str()))
			{
				return false;
			}
			try_create("tb_plan", create_plan_sql_,
				{
					"CREATE INDEX[main].[plan_id] ON[tb_plan]([plan_id]);",
					"CREATE INDEX[main].[plan_type] ON[tb_plan]([plan_type]);",
					"CREATE INDEX[main].[plan_station] ON[tb_plan]([plan_station]);",
					"CREATE INDEX[main].[plan_request_id] ON[tb_plan]([plan_request_id]);",
				});
			sqlite3_prepare_v2(sqlite_db_, insert_plan_sql_, static_cast<int>(strlen(insert_plan_sql_)), &insert_plan_stmt_, nullptr);
			try_create("tb_frame", create_frame_sql_,
				{
					"CREATE INDEX[main].[frame_plan_id] ON[tb_frame]([plan_id]);",
					"CREATE INDEX[main].[frame_type] ON[tb_frame]([type]);",
					"CREATE INDEX[main].[frame_index] ON[tb_frame]([index]);"
				});
			sqlite3_prepare_v2(sqlite_db_, insert_frame_sql_, static_cast<int>(strlen(insert_frame_sql_)), &insert_frame_stmt_, nullptr);
			try_create("tb_log", create_log_sql_,
				{
					"CREATE INDEX[main].[log_plan_id] ON[tb_log]([plan_id]);",
					"CREATE INDEX[main].[log_date] ON[tb_log]([date]);",
					"CREATE INDEX[main].[log_state] ON[tb_log]([state]);"
				});
			sqlite3_prepare_v2(sqlite_db_, insert_log_sql_, static_cast<int>(strlen(insert_log_sql_)), &insert_log_stmt_, nullptr);
			return true;
		}
		/**
		*\brief 将数据写入数据库中
		*/
		bool plan_storage::save_plan(plan_message& message)
		{
			sqlite3_reset(insert_plan_stmt_);

			sqlite3_bind_int64(insert_plan_stmt_, 1, message.plan_id);
			sqlite3_bind_int(insert_plan_stmt_, 2, static_cast<int>(message.plan_type));
			if (message.request_id.empty())
				sqlite3_bind_null(insert_plan_stmt_, 3);
			else
				sqlite3_bind_text(insert_plan_stmt_, 3, *message.request_id, static_cast<int>(message.request_id.size()), nullptr);
			if (message.station.empty())
				sqlite3_bind_null(insert_plan_stmt_, 4);
			else
				sqlite3_bind_text(insert_plan_stmt_, 4, *message.station, static_cast<int>(message.station.size()), nullptr);
			if (message.command.empty())
				sqlite3_bind_null(insert_plan_stmt_, 5);
			else
				sqlite3_bind_text(insert_plan_stmt_, 5,*message.command, static_cast<int>(message.command.size()), nullptr);
			var json = message.write_info();
			sqlite3_bind_text(insert_plan_stmt_, 6, json.c_str(), static_cast<int>(json.size()), nullptr);
			if (sqlite3_step(insert_plan_stmt_) != SQLITE_DONE)
			{
				log_error2("[%s] : db > Can't save plan:%s", name_, sqlite3_errmsg(sqlite_db_));
				return false;
			}
			int idx = 0;
			for (const auto& line : message.frames)
			{
				sqlite3_reset(insert_frame_stmt_);
				sqlite3_bind_int64(insert_frame_stmt_, 1, message.plan_id);
				sqlite3_bind_int(insert_frame_stmt_, 2, 0);
				sqlite3_bind_int(insert_frame_stmt_, 3, ++idx);
				sqlite3_bind_blob(insert_frame_stmt_, 4, line.get_buffer(), static_cast<int>(message.command.size()), nullptr);
				if (sqlite3_step(insert_frame_stmt_) != SQLITE_DONE)
				{
					log_error3("[%s] : db > Can't save frame(%d):%s", name_, idx, sqlite3_errmsg(sqlite_db_));
				}
			}
			return true;
		}
		/**
		*\brief 将数据写入数据库中
		*/
		bool plan_storage::save_log(plan_message& message)
		{
			var json = message.write_state();

			sqlite3_reset(insert_log_stmt_);
			sqlite3_bind_int64(insert_log_stmt_, 1, message.plan_id);
			sqlite3_bind_int64(insert_log_stmt_, 2, message.exec_time);
			sqlite3_bind_int(insert_log_stmt_, 3, message.exec_state);
			sqlite3_bind_text(insert_log_stmt_, 4, json.c_str(), static_cast<int>(json.size()), nullptr);
			if (sqlite3_step(insert_log_stmt_) == SQLITE_DONE)
			{
				return true;
			}
			log_error2("[%s] : db > Can't save log:%s", name_, sqlite3_errmsg(sqlite_db_));
			return false;
		}

		/**
		*\brief 将数据写入数据库中
		*/
		bool plan_storage::save_log(plan_message& message, vector<shared_char>& resulst)
		{
			acl::json json;
			acl::json_node& node = json.create_node();
			message.write_state(node);
			acl::json_node& array = json.create_array();
			for (const auto& line : resulst)
			{
				if (line.empty())
					array.add_array_null();
				else
				{
					acl::string str;
					for (size_t i = 0; i < line.size(); i++)
					{
						str.format_append("%02X", line[i]);
					}
					array.add_array_text(str.c_str());
				}
			}
			node.add_child("resulst", array);
			var str = node.to_string();

			sqlite3_reset(insert_log_stmt_);
			sqlite3_bind_int64(insert_log_stmt_, 1, message.plan_id);
			sqlite3_bind_int64(insert_log_stmt_, 2, message.exec_time);
			sqlite3_bind_int(insert_log_stmt_, 3, message.exec_state);
			sqlite3_bind_text(insert_log_stmt_, 4, str.c_str(), static_cast<int>(str.size()), nullptr);
			if (sqlite3_step(insert_log_stmt_) != SQLITE_DONE)
			{
				log_error2("[%s] : db > Can't save log:%s", name_, sqlite3_errmsg(sqlite_db_));
				return false;
			}

			int idx = 0;
			for (const auto& line : resulst)
			{
				sqlite3_reset(insert_frame_stmt_);
				sqlite3_bind_int64(insert_frame_stmt_, 1, message.plan_id);
				sqlite3_bind_int(insert_frame_stmt_, 2, 0);
				sqlite3_bind_int(insert_frame_stmt_, 3, ++idx);
				sqlite3_bind_blob(insert_frame_stmt_, 4, line.get_buffer(), static_cast<int>(message.command.size()), nullptr);
				if (sqlite3_step(insert_frame_stmt_) != SQLITE_DONE)
				{
					log_error3("[%s] : db > Can't save frame(%d):%s", name_, idx, sqlite3_errmsg(sqlite_db_));
				}
			}
			return true;
		}
	}
}
#endif