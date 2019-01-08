/**
 * ZMQ队列类
 */

#include "../stdafx.h"
#include "station_warehouse.h"
#include "queue_station.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 内部命令
		*/
		bool queue_station::simple_command_ex(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner)
		{
			if (description.command() == zero_def::command::restart && list.size() >= 4)
			{
				send_request_status(socket, *list[0], zero_def::status::ok, false);
				int64 min = atoll(*list[2]);
				int64 max = atoll(*list[3]);

				storage_.load(min, max, [this](vector<shared_char>& data)
				{
					send_response(data, true);
				});
				return true;
			}
			return false;
		}

		/**
		* \brief 处理请求
		*/
		/**
		* \brief 工作开始 : 处理请求数据
		*/
		void queue_station::job_start(zmq_handler socket, vector<shared_char>& list, bool inner, bool old)
		{
			trace(1, list, nullptr);
			const shared_char caller = list[0];
			if (inner)
				list.erase(list.begin());
			shared_char description(list[1].c_str(), list[1].size());
			if (config_->get_state() == station_state::pause)
			{
				config_->request_err++;
				send_request_status_by_trace(socket, list, description, zero_def::status::pause);
				return;
			}
			size_t request_id = 0, global_id = 0, argument = 0, text = 0, context = 0, pub_title = 0, sub_title = 0, requester = 0;
			for (size_t idx = 2; idx <= description.desc_size() && idx < list.size(); idx++)
			{
				switch (description[idx])
				{
				case zero_def::frame::request_id:
					request_id = idx;
					break;
				case zero_def::frame::requester:
					requester = idx;
					break;
				case zero_def::frame::global_id:
					global_id = idx;
					break;
				case zero_def::frame::pub_title:
					pub_title = idx;
					break;
				case zero_def::frame::sub_title:
					sub_title = idx;
					break;
				case zero_def::frame::arg:
					argument = idx;
					break;
				case zero_def::frame::content:
					text = idx;
					break;
				case zero_def::frame::context:
					context = idx;
					break;
				}
			}
			if (pub_title == 0)
				pub_title = sub_title;
			else if (sub_title == 0)
				sub_title = pub_title;
			if (pub_title == 0)
			{
				config_->error("job_start", "title can`t empty");
				config_->request_deny++;
				send_request_status_by_trace(socket, *caller, zero_def::status::frame_invalid, list, global_id, requester, request_id);
				return;
			}
			send_request_status_by_trace(socket, *caller, zero_def::status::ok, list, global_id, requester, request_id);

			const int64 id = storage_.save(
				global_id > 0 ? atoll(*list[global_id]) : 0,
				*list[pub_title],
				*list[sub_title],
				request_id > 0 ? *list[request_id] : nullptr,
				requester > 0 ? *list[requester] : nullptr,
				context > 0 ? *list[context] : nullptr,
				argument > 0 ? *list[argument] : nullptr,
				text > 0 ? *list[text] : nullptr);

			description.append_frame(zero_def::frame::local_id);
			shared_char local_id;
			local_id.set_int64(id);
			list.emplace_back(local_id);

			list[0] = list[pub_title];
			list[1] = description;
			send_response(list, true);
		}

		/**
		*\brief 运行一个通知线程
		*/
		void queue_station::launch(shared_ptr<queue_station> station)
		{
			zero_config& config = station->get_config();
			if (!station->initialize())
			{
				config.failed("initialize");
				set_command_thread_bad(config.station_name.c_str());
				return;
			}
			if (!station_warehouse::join(station.get()))
			{
				config.failed("join warehouse");
				set_command_thread_bad(config.station_name.c_str());
				return;
			}
			station->storage_.prepare_storage(station->config_);
			station->poll();
			station_warehouse::left(station.get());
			station->destruct();
			if (!config.is_state(station_state::stop) && get_net_state() == zero_def::net_state::runing)
			{
				config.restart();
				run(station->get_config_ptr());
			}
			else
			{
				config.closed();
			}
			set_command_thread_end(config.station_name.c_str());
		}
	}
}
