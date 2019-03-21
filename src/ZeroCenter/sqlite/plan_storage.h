#ifndef ZMQ_API_PLAN_STORAGE_H
#define ZMQ_API_PLAN_STORAGE_H
#pragma once
#ifdef PLAN
#include "../rpc/zero_station.h"
#include "sqlite_storage.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 消息持久化存储(使用SQLite)
		*/
		class plan_storage :public sqlite_storage
		{
			sqlite3_stmt *insert_plan_stmt_;
			sqlite3_stmt *insert_log_stmt_;
			sqlite3_stmt *insert_frame_stmt_;
			static const char* create_log_sql_;
			static const char* create_plan_sql_;
			static const char* create_frame_sql_;
			static const char* insert_plan_sql_;
			static const char* insert_log_sql_;
			static const char* insert_frame_sql_;
		public:
			/**
			 * \brief 构造
			 */
			plan_storage() : insert_plan_stmt_(nullptr), insert_log_stmt_(nullptr), insert_frame_stmt_(nullptr)
			{
			}

			/**
			 * \brief 构造
			 */
			virtual  ~plan_storage()
			{
				if (insert_plan_stmt_ != nullptr)
					sqlite3_finalize(insert_plan_stmt_);
				if (insert_plan_stmt_ != nullptr)
					sqlite3_finalize(insert_log_stmt_);
				if (insert_frame_stmt_ != nullptr)
					sqlite3_finalize(insert_frame_stmt_);
			}

			/**
			 * \brief 准备存储
			 */
			bool prepare(shared_ptr<zero_config>& config);

			/**
			*\brief 将数据写入数据库中
			*/
			bool save_plan(plan_message& message);
			/**
			*\brief 将数据写入数据库中
			*/
			bool save_log(plan_message& message);
			/**
			*\brief 将数据写入数据库中
			*/
			bool save_log(plan_message& message, vector<shared_char>& resulst);
		};
	}
}
#endif
#endif//!ZMQ_API_PLAN_STORAGE_H