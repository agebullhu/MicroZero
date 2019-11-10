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
			vector<shared_char>& frames;
			shared_char& description;
			size_t rqid_index, glid_index, rqer_index, pub_title_index, cmd_index;

			zero_frames(vector<shared_char>& list, shared_char& des) : frames(list), description(des), rqid_index(0), glid_index(0), rqer_index(0), pub_title_index(0), cmd_index(0)
			{
			}

			size_t size() const
			{
				return frames.size();
			}
			
			bool is_zmtp_ping()
			{
				return strcmp(*description, "\004PING") == 0;
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

			shared_char copy_description()
			{
				return shared_char(description.get_buffer(), description.size());
			}

			void check_global_id()
			{
				if (glid_index > 0)
					return;
				description.append_frame(zero_def::frame::global_id);
				glid_index = frames.size();
				shared_char global_id;
				global_id.set_int64(station_warehouse::get_glogal_id());
				frames.push_back(global_id);
				glid_index = frames.size();
			}

			void check_in_frames_for_plan()
			{
				const char* buf = *description;

				const auto desc_size = description.desc_size();
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
					case zero_def::frame::global_id:
						glid_index = idx;
						break;
					case zero_def::frame::pub_title:
						pub_title_index = idx;
						break;
					}
				}
			}


			void check_in_frames()
			{
				const char* buf = *description;
				const auto desc_size = description.desc_size();
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
					case zero_def::frame::global_id:
						glid_index = idx;
						break;
					case zero_def::frame::pub_title:
						pub_title_index = idx;
						break;
					}
				}
			}
		};
	}
}
#endif //!_ZERO_FRAMES_H_
