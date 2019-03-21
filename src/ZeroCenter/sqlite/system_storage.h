#ifndef ZMQ_API_SYSTEM_STORAGE_H
#define ZMQ_API_SYSTEM_STORAGE_H
#pragma once
#include "../rpc/zero_station.h"
#include "sqlite_storage.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 跟踪持久化存储(使用SQLite)
		*/
		class system_storage :public sqlite_storage
		{
			sqlite3_stmt *station_insert_stmt_;
			sqlite3_stmt *station_update_stmt_;
			/**
			* \brief 配置
			*/
			shared_ptr<station_config> config_;
			static const char* station_create_sql_; 
			static const char* station_insert_sql_;
			static const char* station_update_sql_;
			static const char* station_find_sql_;
			static const char* station_load_sql_1;
			static const char* station_load_sql_2;
			static const char* station_delete_sql_;
			static const char* config_create_sql_;
			static const char* config_port_sql;
		public:
			int next_port, reboot_num;
			/**
			 * \brief 构造
			 */
			system_storage() : station_insert_stmt_(nullptr), station_update_stmt_(nullptr), reboot_num(0)
			{
				next_port = global_config::base_tcp_port;
				strcpy(name_, "system_storage");
			}

			/**
			 * \brief 构造
			 */
			virtual  ~system_storage()
			{
				if (station_insert_stmt_)
					sqlite3_finalize(station_insert_stmt_);
				if (station_update_stmt_)
					sqlite3_finalize(station_update_stmt_);
			}

			/**
			 * \brief 准备存储
			 */
			bool prepare_storage();
			/**
			 * \brief 读取配置
			 */
			bool load_config();
			/**
			 * \brief 保存站点配置
			 */
			bool save_station(shared_ptr<station_config>& config);
			/**
			 * \brief 读取站点配置
			 */
			bool load_station(std::function<void(shared_ptr<station_config>&)> exec);
			/**
			 * \brief 读取站点配置
			 */
			shared_ptr<station_config> load_station(const char* station);
			/**
			 * \brief 删除站点配置
			 */
			bool delete_station(const char* station);
			/**
			 * \brief 更新后一个端口设置
			 */
			bool update_next_port();

			/**
			 * \brief 更新重启次数
			 */
			int update_reboot_num();
			
		};
	}
}
#endif//!ZMQ_API_QUEUE_STORAGE_H