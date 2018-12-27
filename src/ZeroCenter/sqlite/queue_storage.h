#ifndef ZMQ_API_QUEUE_STORAGE_H
#define ZMQ_API_QUEUE_STORAGE_H
#pragma once
#include "../rpc/zero_station.h"
#include "sqlite_storage.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 消息持久化存储(使用SQLite)
		*/
		class queue_storage :public sqlite_storage
		{
			int64 last_id_;
			sqlite3_stmt *insert_stmt_;
			sqlite3_stmt *load_stmt_;
			/**
			* \brief 配置
			*/
			shared_ptr<zero_config> config_;
			static const char* create_sql_;
			static const char* insert_sql_;
			static const char* load_max_sql_;
			static const char* load_sql_;
			static char queue_frames_[];
		public:
			/**
			 * \brief 构造
			 */
			queue_storage() : last_id_(0), insert_stmt_(nullptr), load_stmt_(nullptr)
			{
			}

			/**
			 * \brief 构造
			 */
			virtual  ~queue_storage()
			{
				if (sqlite_db_ == nullptr)
					return;
				sqlite3_finalize(insert_stmt_);
				sqlite3_finalize(load_stmt_);
			}

			/**
			 * \brief 最大ID
			 */
			int64 get_last_id() const
			{
				return last_id_;
			}
			/**
			 * \brief 准备存储
			 */
			bool prepare_storage(shared_ptr<zero_config>& config);

			/**
			*\brief 将数据写入数据库中
			*/
			int64 save(const int64 gid, const char* title, const char* sub, const char* reqid, const char* publiher, const char* ctx, const char* arg, const char* arg2);
			/**
			*\brief 取数据
			*/
			void load(int64 min, int64 max, std::function<void(vector<shared_char>&)> exec);
		private:
			/**
			*\brief 取最大ID
			*/
			bool read_last_id();
		};
	}
}
#endif//!ZMQ_API_QUEUE_STORAGE_H