#ifndef ZMQ_API_TRACE_STORAGE_H
#define ZMQ_API_TRACE_STORAGE_H
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
		class trace_storage :public sqlite_storage
		{
			tm trace_day;
			sqlite3_stmt *trace_insert_stmt_;
			sqlite3_stmt *trace_last_stmt_;
			sqlite3_stmt *frame_insert_stmt_;
			/**
			* \brief 配置
			*/
			shared_ptr<zero_config> config_;
			static const char* trace_create_sql_;
			static const char* trace_insert_sql_;
			static const char* trace_last_sql_;
			static const char* frame_create_sql_;
			static const char* frame_insert_sql_;
		public:
			/**
			 * \brief 构造
			 */
			trace_storage() : trace_insert_stmt_(nullptr), trace_last_stmt_(nullptr), frame_insert_stmt_(nullptr)
			{
				memset(&trace_day, 0, sizeof(tm));
				strcpy(name_, "zero_trace");
			}

			/**
			 * \brief 构造
			 */
			virtual  ~trace_storage()
			{
				sqlite3_finalize(frame_insert_stmt_);
				sqlite3_finalize(trace_last_stmt_);
				sqlite3_finalize(trace_insert_stmt_); 
			}

			/**
			 * \brief 准备存储
			 */
			bool prepare_storage();

			/**
			*\brief 将数据写入数据库中
			*/
			void save(int in_out, const char* reqid, const int64 par_id, const int64 my_id,const char* station_name, const char*  station_type, const char*  state, vector<shared_char>& frames);
		};
	}
}
#endif//!ZMQ_API_QUEUE_STORAGE_H