#pragma once
#ifndef _ZERO_FRAMES_H_
#define _ZERO_FRAMES_H_
#include "../stdinc.h"
#include "zero_net.h"
#include <utility>
#include "../log/mylogger.h"
#include "../ext/shared_char.h"
#include "station_warehouse.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		 * \brief 网络数据帧对象
		 */
		class zero_frames
		{
			shared_char empty_;
		public:
			bool local;
			vector<shared_char> frames;
			shared_char local_caller;
			size_t rqid_index, glid_index, rqer_index, cmd_index;

			zero_frames(bool is_local) : local(is_local), rqid_index(0), glid_index(0), rqer_index(0), cmd_index(0)
			{
			}

			int size()
			{
				return frames.size();
			}
			bool is_zmtp_ping()
			{
				return strcmp(*frames[1], "\004PING") == 0;
			}
			shared_char& description()
			{
				return frames[1];
			}

			shared_char& request_id()
			{
				return rqid_index <= 0 ? empty_ : frames[rqid_index];
			}
			shared_char& global_id()
			{
				return glid_index <= 0 ? empty_ : frames[glid_index];
			}
			shared_char& requester()
			{
				return rqer_index <= 0 ? empty_ : frames[rqer_index];
			}

			shared_char descript_copy()
			{
				return shared_char(frames[1].get_buffer(), frames[1].size());
			}

			void check_global_id()
			{
				if (glid_index > 0)
					return;
				frames[1].append_frame(zero_def::frame::global_id);
				shared_char global_id;
				global_id.set_int64(station_warehouse::get_glogal_id());
				frames.push_back(global_id);
				glid_index = frames.size() - 1;
			}

			void check_in_frames_for_plan()
			{
				const char* buf = *frames[1];
				vector<shared_char> arg;
				const auto desc_size = frames[1].desc_size();
				for (size_t idx = 2; idx <= desc_size; idx++)
				{
					switch (buf[idx])
					{
					case zero_def::frame::command:
						cmd_index = idx;
						break;
					case zero_def::frame::request_id:
						rqid_index = idx;
						break;
					case zero_def::frame::requester:
						rqer_index = idx;
						break;
					case zero_def::frame::arg:
						arg.emplace_back(frames[idx]);
						break;
					case zero_def::frame::global_id:
						glid_index = idx;
						break;
					}
				}
			}


			void check_in_frames()
			{
				if(local)
				{
					local_caller = frames[0];
					frames.erase(frames.begin());
				}
				const char* buf = *frames[1];
				vector<shared_char> arg;
				const auto desc_size = frames[1].desc_size();
				for (size_t idx = 2; idx <= desc_size; idx++)
				{
					switch (buf[idx])
					{
					case zero_def::frame::command:
						cmd_index = idx;
						break;
					case zero_def::frame::request_id:
						rqid_index = idx;
						break;
					case zero_def::frame::requester:
						rqer_index = idx;
						break;
					case zero_def::frame::arg:
						arg.emplace_back(frames[idx]);
						break;
					case zero_def::frame::global_id:
						glid_index = idx;
						break;
					}
				}
			}
		};
	}
}
#endif //!_ZERO_FRAMES_H_
