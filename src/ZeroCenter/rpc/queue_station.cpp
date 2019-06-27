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
		bool queue_station::simple_command(zmq_handler socket, vector<shared_char>& list, shared_char& description, bool inner)
		{
			if (description.command() == zero_def::command::restart && list.size() >= 4)
			{
				send_request_status(socket, *list[0], zero_def::status::ok, false, false);
				int64 min = atoll(*list[2]);
				int64 max = atoll(*list[3]);
				boost::thread(async_replay, this, min, max);
				return true;
			}
			return zero_station::simple_command(socket, list, description, inner);
		}

		/**
		* \brief 内部命令
		*/
		void queue_station::async_replay(queue_station* queue, int64 min, int64 max)
		{
			char name[64];
			sprintf(name, "%ld", time(nullptr));
			zmq_handler socket = socket_ex::create_req_socket_inproc(queue->station_name_.c_str(), name);
			queue->storage_.load(min, max, [socket, queue](vector<shared_char>& data)
				{
					queue->send_response(socket, data, false);
				});
			make_inproc_address(address, queue->station_name_.c_str());
			socket_ex::close_req_socket(socket, address);
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
				send_request_status_by_trace(socket, list, description, zero_def::status::pause, true);
				return;
			}
			size_t request_id = 0, q_id = 0, global_id = 0, argument = 0, text = 0, context = 0, pub_title = 0, sub_title = 0, requester = 0;
			for (size_t idx = 2; idx <= description.desc_size() && idx < list.size(); idx++)
			{
				switch (description[idx])
				{
				case zero_def::frame::local_id:
					q_id = idx;
					break;
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
			//else if (sub_title == 0)
			//	sub_title = pub_title;
			if (pub_title == 0)
			{
				config_->error("job_start", "title can`t empty");
				config_->request_deny++;
				if (q_id == 0)
					send_request_status_by_trace(socket, *caller, zero_def::status::frame_invalid, list, global_id, request_id, requester);
				return;
			}
			if (q_id == 0)
			{
				send_request_status_by_trace(socket, *caller, zero_def::status::ok, list, global_id, request_id, requester);

				q_id = storage_.save(
					global_id > 0 ? atoll(*list[global_id]) : 0,
					*list[pub_title],
					sub_title > 0 ? *list[sub_title] : nullptr,
					request_id > 0 ? *list[request_id] : nullptr,
					requester > 0 ? *list[requester] : nullptr,
					context > 0 ? *list[context] : nullptr,
					argument > 0 ? *list[argument] : nullptr,
					text > 0 ? *list[text] : nullptr);

				description.append_frame(zero_def::frame::local_id);
				shared_char local_id;
				local_id.set_int64(q_id);
				list.emplace_back(local_id);
			}

			list[0] = list[pub_title];
			list[1] = description;
			zmq_socket_state state = send_response(list, true);

			if (state != zmq_socket_state::succeed)
			{
				storage_.set_state(q_id, zero_def::status::none);
			}
		}

		/**
		* \brief 工作结束(发送到请求者)
		*/
		void queue_station::job_end(vector<shared_char>& list)
		{
			if (list.size() < 3)
				return;
			shared_char description(list[2].c_str(), list[2].size());
			storage_.set_state(atoll(*list[3]), description[1]);
		}

		/**
		*\brief 运行一个通知线程
		*/
		void queue_station::launch(shared_ptr<queue_station> station)
		{
			station_config& config = station->get_config();
			if (!station->initialize())
			{
				config.failed("initialize");
				set_station_thread_bad(config.station_name.c_str());
				return;
			}
			if (!station_warehouse::join(station.get()))
			{
				config.failed("join warehouse");
				set_station_thread_bad(config.station_name.c_str());
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
			set_station_thread_end(config.station_name.c_str());
		}
	}
}
