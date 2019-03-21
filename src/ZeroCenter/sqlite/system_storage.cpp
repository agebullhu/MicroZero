/**
 * ZMQ通知代理类
 *
 *
 */

#include "../stdafx.h"
#include "system_storage.h"

namespace agebull
{
	namespace zero_net
	{
		const char* system_storage::station_create_sql_ =
			"CREATE TABLE tb_station("
			"	[name]	  NVARCHAR(200),"
			"	[config]  TEXT"
			");";

		const char* system_storage::station_insert_sql_ = "INSERT INTO tb_station([name],[config]) VALUES(?,?);";
		const char* system_storage::station_update_sql_ = "UPDATE tb_station SET config=? WHERE name=?;";

		const char* system_storage::station_find_sql_ = "SELECT rowid FROM tb_station WHERE name=?;";

		const char* system_storage::station_load_sql_1 = "SELECT [config] FROM tb_station";

		const char* system_storage::station_load_sql_2 = "SELECT [config] FROM tb_station WHERE name = ?;";

		const char* system_storage::station_delete_sql_ = "DELETE FROM tb_station WHERE name = ?;";


		const char* system_storage::config_create_sql_ =
			"CREATE TABLE tb_config("
			"	[port]			INT,"
			"	[reboot_num]	INT"
			");";

		/**
		 * \brief 准备存储
		 */
		bool system_storage::prepare_storage()
		{
			acl::string path;
			path.format("%s/config/zero_center.db", global_config::root_path);
			if (!open_db(path.c_str()))
			{
				return false;
			}
			acl::string sql;
			sql.format("INSERT INTO tb_config(port,reboot_num) VALUES(%d,0);", next_port);
			try_create("tb_config", config_create_sql_,
				{
					sql.c_str()
				});

			try_create("tb_station", station_create_sql_,
				{
					"CREATE INDEX [station].[id] ON[tb_station]([id]);",
					"CREATE INDEX [station].[name] ON[tb_station]([name]);"
				});

			if (sqlite3_prepare_v2(sqlite_db_, station_insert_sql_, static_cast<int>(strlen(station_insert_sql_)),
				&station_insert_stmt_, nullptr) != SQLITE_OK)
			{
				log_error1("[system_storage] : prepare(station_insert_sql_) : %s", sqlite3_errmsg(sqlite_db_));
				return false;
			}
			if (sqlite3_prepare_v2(sqlite_db_, station_update_sql_, static_cast<int>(strlen(station_update_sql_)),
				&station_update_stmt_, nullptr) != SQLITE_OK)
			{
				log_error1("[system_storage] : prepare(station_update_sql_) : %s", sqlite3_errmsg(sqlite_db_));
				return false;
			}
			load_config();
			return true;
		}

		/**
		 * \brief 保存站点配置
		 */
		bool system_storage::save_station(shared_ptr<station_config>& config)
		{
			char * errmsg = nullptr;
			char **db_result = nullptr;
			int row, column, state;
			auto ex_result = sqlite3_get_table(sqlite_db_, station_find_sql_, &db_result, &row, &column, &errmsg);
			sqlite3_free_table(db_result);
			if (ex_result != SQLITE_OK)
			{
				log_error2("[system_storage] : save_station(%s) find : %s", config->station_name.c_str(), errmsg);
				return false;
			}
			auto json = config->to_full_json();
			if (db_result[column] != nullptr)
			{
				bind_column_str(station_insert_stmt_, config->station_name, 1);
				bind_column_str(station_insert_stmt_, json, 2);
				state = sqlite3_step(station_insert_stmt_);
				sqlite3_reset(station_insert_stmt_);
				if (state != SQLITE_DONE)
				{
					log_error2("[system_storage] : save_station(%s) insert: %s", config->station_name.c_str(), errmsg);
					return false;
				}
			}
			else
			{
				bind_column_str(station_update_stmt_, json, 1);
				bind_column_str(station_update_stmt_, config->station_name, 2);
				state = sqlite3_step(station_update_stmt_);
				sqlite3_reset(station_update_stmt_);
				if (state != SQLITE_DONE)
				{
					log_error2("[system_storage] : save_station(%s) update: %s", config->station_name.c_str(), errmsg);
					return false;
				}
			}
			return true;
		}


		/**
		 * \brief 读取站点配置
		 */
		bool system_storage::load_station(std::function<void(shared_ptr<station_config>&)> exec)
		{
			char * errmsg = nullptr;
			char **db_result = nullptr;
			int row, column;
			auto ex_result = sqlite3_get_table(sqlite_db_, station_load_sql_1, &db_result, &row, &column, &errmsg);
			if (ex_result != SQLITE_OK)
			{
				sqlite3_free_table(db_result);
				log_error1("[system_storage] : load_station(all) : %s", errmsg);
				return false;
			}
			if (row > 0)
			{
				for (int idx = 1; idx <= row; idx++)
				{
					shared_ptr<station_config> config = make_shared<station_config>();
					config->read_json(db_result[idx]);
					exec(config);
				}
			}
			sqlite3_free_table(db_result);
			return row > 0;
		}
		/**
		 * \brief 读取站点配置
		 */
		shared_ptr<station_config> system_storage::load_station(const char* station)
		{
			char * errmsg = nullptr;
			char **db_result = nullptr;
			int row, column;
			auto ex_result = sqlite3_get_table(sqlite_db_, station_load_sql_1, &db_result, &row, &column, &errmsg);
			if (ex_result != SQLITE_OK)
			{
				sqlite3_free_table(db_result);
				log_error2("[system_storage] : load_station(%s) : %s", station, errmsg);
				return nullptr;
			}
			if (row <= 0)
			{
				sqlite3_free_table(db_result);
				return nullptr;
			}
			shared_ptr<station_config> config = make_shared<station_config>();
			config->read_json(db_result[1]);
			sqlite3_free_table(db_result);
			return config;
		}
		/**
		 * \brief 删除站点配置
		 */
		bool system_storage::delete_station(const char* station)
		{
			char * errmsg = nullptr;
			if (sqlite3_exec(sqlite_db_, station_delete_sql_, nullptr, nullptr, &errmsg) != SQLITE_OK)
			{
				log_error2("[system_storage] : delete_station(%s) : %s", station, errmsg);
				return false;
			}
			return true;
		}

		/**
		 * \brief 读取配置
		 */
		bool system_storage::load_config()
		{
			char * errmsg = nullptr;
			char **db_result = nullptr;
			int row, column;
			const char * sql = "SELECT port FROM tb_config;";
			auto ex_result = sqlite3_get_table(sqlite_db_, sql, &db_result, &row, &column, &errmsg);
			if (ex_result != SQLITE_OK)
			{
				sqlite3_free_table(db_result);
				log_error1("[system_storage] : load_config : %s", errmsg);
				return false;
			}
			if (row <= 0)
			{
				sqlite3_free_table(db_result);
				return false;
			}
			next_port = atoi(db_result[1]);
			sqlite3_free_table(db_result);
			return true;
		}
		/**
		 * \brief 更新后一个端口设置
		 */
		bool  system_storage::update_next_port()
		{

			acl::string sql;
			sql.format("UPDATE tb_config SET port = %d;", next_port);
			char * errmsg = nullptr;
			if (sqlite3_exec(sqlite_db_, sql.c_str(), nullptr, nullptr, &errmsg) != SQLITE_OK)
			{
				log_error1("[system_storage] : update_next_port : %s", errmsg);
				return false;
			}
			return true;
		}

		/**
		 * \brief 更新后一个端口设置
		 */
		int system_storage::update_reboot_num()
		{
			char * errmsg = nullptr;
			acl::string sql;
			sql.format("UPDATE tb_config SET reboot_num = reboot_num + 1;", reboot_num);
			if (sqlite3_exec(sqlite_db_, sql.c_str(), nullptr, nullptr, &errmsg) != SQLITE_OK)
			{
				log_error1("[system_storage] : update_reboot_num : %s", errmsg);
				return 1;
			}
			char **db_result = nullptr;
			int row, column;
			auto ex_result = sqlite3_get_table(sqlite_db_, "SELECT reboot_num FROM tb_config", &db_result, &row, &column, &errmsg);
			if (ex_result != SQLITE_OK)
			{
				sqlite3_free_table(db_result);
				log_error1("[system_storage] : update_reboot_num : %s", errmsg);
				return 1;
			}
			sqlite3_free_table(db_result);
			if (row <= 0)
			{
				return 1;
			}
			sqlite3_free_table(db_result);
			reboot_num = atoi(db_result[1]);
			return reboot_num;
		}
	}
}
