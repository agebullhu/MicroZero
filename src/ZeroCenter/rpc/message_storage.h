#ifndef ZMQ_API_MESSAGE_STORAGE_H
#define ZMQ_API_MESSAGE_STORAGE_H
#pragma once
#include "../stdinc.h"
#include <sqlite3.h>
#include <utility>
#include "zero_station.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 消息持久化存储(使用SQLite)
		*/
		class message_storage
		{
			int64 _last_id;
			sqlite3* _sqlite_db;
			sqlite3_stmt *_insert_stmt;
			sqlite3_stmt *_load_stmt;
			/**
			* \brief 配置
			*/
			shared_ptr<zero_config> _config;
		public:
			/**
			 * \brief 构造
			 */
			message_storage() : _last_id(0), _sqlite_db(nullptr), _insert_stmt(nullptr), _load_stmt(nullptr)
			{
			}

			/**
			 * \brief 构造
			 */
			~message_storage()
			{
				if (_sqlite_db == nullptr)
					return;
				sqlite3_close(_sqlite_db);
				sqlite3_finalize(_insert_stmt);
				sqlite3_finalize(_load_stmt);
			}

			/**
			 * \brief 最大ID
			 */
			int64 get_last_id() const
			{
				return _last_id;
			}
			/**
			 * \brief 准备存储
			 */
			bool prepare_storage(shared_ptr<zero_config>& config);

			/**
			*\brief 将数据写入数据库中
			*/
			int64 save(const char* title, const char* sub, const char* arg, const char* reqid, const char* publiher, const int64 gid);
			/**
			*\brief 取数据
			*/
			void load(int64 min, int64 max,vector<vector<shared_char>>& datas) const;
		private:
			bool read_last_id();
		};
	}
}
#endif//!ZMQ_API_MESSAGE_STORAGE_H