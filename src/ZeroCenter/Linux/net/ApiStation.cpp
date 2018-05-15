#include "../stdafx.h"
#include "ApiStation.h"
namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 开始执行一条命令
		*/
		sharp_char api_station::command(const char* caller, vector<sharp_char> lines)
		{
			vector<sharp_char> response;
			ipc_request_socket<ZMQ_REQ> socket(caller, station_name_.c_str());
			if(socket.request(lines, response))
				return response.empty() ? ZERO_STATUS_ERROR : response[0];
			switch(socket.get_state())
			{
			case zmq_socket_state::TimedOut:
				return ZERO_STATUS_TIMEOUT;
			default:
				return ZERO_STATUS_NET_ERROR;
			}
		}

		/**
		* \brief 工作集合的响应
		*/
		void api_station::response()
		{
			vector<sharp_char> list;
			//0 worker地址 1空帧 2请求者地址
			zmq_state_ = recv(response_socket_tcp_, list);
			if (zmq_state_ == zmq_socket_state::TimedOut)
			{
				return;
			}
			if (zmq_state_ != zmq_socket_state::Succeed)
			{
				log_error3("接收结果失败%s(%d)%s", station_name_.c_str(), response_port_, state_str(zmq_state_));
				return;
			}
			/*switch (list[2][0])
			{
			case ZERO_WORKER_JOIN:
				worker_join(*list[0]);
				send_addr(response_socket_tcp_, *list[0]);
				send_late(response_socket_tcp_, ZERO_STATUS_WECOME);
				return;
			case ZERO_WORKER_LEFT:
				worker_left(*list[0]);
				send_addr(response_socket_tcp_, *list[0]);
				send_late(response_socket_tcp_, ZERO_STATUS_BYE);
				return;
			case ZERO_WORKER_LISTEN:
				return;
			default:
				zmq_state_ = send(list[1][0] == '_' ? request_socket_ipc_ : request_scoket_tcp_, list, 2);
				break;
			}*/

			zmq_state_ = send(list[0][0] == '-' ? request_socket_ipc_ : request_scoket_tcp_, list);
		}
		/**
		* \brief 调用集合的响应
		*/
		void api_station::request(ZMQ_HANDLE socket, bool inner)
		{
			vector<sharp_char> list;
			//0 请求地址 1 空帧 2命令 3请求标识 4上下文 5参数
			zmq_state_ = recv(socket, list);
			if (zmq_state_ == zmq_socket_state::TimedOut)
			{
				return;
			}
			if (zmq_state_ != zmq_socket_state::Succeed)
			{
				log_error3("接收消息失败%s(%d)%s", station_name_.c_str(), response_port_, state_str(zmq_state_));
				return;
			}
			const size_t list_size = list.size();
			if (list_size <= 3)
			{
				zmq_state_ = send_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID);
				return;
			}
			sharp_char& description = list[2];
			//内部命令通知退出
			if (inner && description[1] == 'B')
			{
				worker_left(*list[3]);
				zmq_state_ = send_status(socket, *list[0], ZERO_STATUS_OK);
				return;
			}
			const auto frame_size = static_cast<size_t>(description[0]);
			if(frame_size >= description.size() || (frame_size + 3) != list_size)
			{
				zmq_state_ = send_status(socket, *list[0], ZERO_STATUS_FRAME_INVALID);
				return;
			}

			//char* const buf = description.get_buffer();
			//for (size_t idx = 1; idx <= frame_size; idx++)
			//{
			//	if (buf[idx] == ZERO_FRAME_PLAN)
			//	{
			//		job_plan(socket, list);
			//		return;
			//	}
			//}
			if(!job_start(socket, list))
			{
				zmq_state_ = send_status(socket, *list[0], ZERO_STATUS_API_NOT_WORKER);
			}
		}
		/**
		* \brief 工作进入计划
		*/
		bool api_station::job_plan(ZMQ_HANDLE socket,vector<sharp_char>& list)
		{
			const sharp_char& caller = list[0];
			sharp_char& description = list[2];
			char* const buf = description.get_buffer();
			const auto frame_size = static_cast<size_t>(buf[0]);

			sharp_char plan, context, arg, command, requester, id;
			for (size_t idx = 1; idx <= frame_size; idx++)
			{
				switch (buf[idx])
				{
				case ZERO_FRAME_PLAN:
					plan = list[2 + idx];
					break;
				case ZERO_FRAME_REQUESTER:
					requester = list[2 + idx];
					break;
				case ZERO_FRAME_CONTEXT:
					context = list[2 + idx];
					break;
				case ZERO_FRAME_ARG:
					arg = list[2 + idx];
					break;
				case ZERO_FRAME_REQUEST_ID:
					id = list[2 + idx];
					break;
				case ZERO_FRAME_COMMAND:
					command = list[2 + idx];
					break;
				}
			}
			int cnt = 0;
			buf[++cnt] = ZERO_FRAME_PUBLISHER;
			plan_message message;
			message.messages.push_back(description);
			message.messages.push_back(requester);
			if (!id.empty())
			{
				buf[++cnt] = ZERO_FRAME_REQUEST_ID;
				message.messages.push_back(id);
			}
			if (!context.empty())
			{
				buf[++cnt] = ZERO_FRAME_CONTEXT;
				message.messages.push_back(context);
			}
			if (!command.empty())
			{
				buf[++cnt] = ZERO_FRAME_COMMAND;
				message.messages.push_back(command);
			}
			if (!arg.empty())
			{
				buf[++cnt] = ZERO_FRAME_ARG;
				message.messages.push_back(arg);
			}
			buf[0] = static_cast<char>(cnt);

			message.read_plan(plan.get_buffer());
			message.request_caller = caller;
			message.messages_description = description;
			plan_next(message, true);


			zmq_state_ = send_addr(socket, *caller);
			zmq_state_ = send_late(socket, ZERO_STATUS_PLAN);
			return true;

		}
		/**
		* \brief 工作开始（发送到工作者）
		*/
		bool api_station::job_start(ZMQ_HANDLE socket, vector<sharp_char>& list)
		{
			const sharp_char& caller = list[0];
			//路由到其中一个工作对象
			//const char* worker;
			//while(!_balance.get_host(worker))
			//{
			//	worker_left(worker);
			//}
			//if (worker == nullptr)
			//{
			//	zmq_state_ = send_status(socket, *caller, ZERO_STATUS_API_NOT_WORKER);
			//	return zmq_state_ == zmq_socket_state::Succeed;
			//}
			//zmq_state_ = send_addr(response_socket_tcp_, worker);
			zmq_state_ = send_more(response_socket_tcp_, *caller);
			if (zmq_state_ != zmq_socket_state::Succeed)
				return false;
			zmq_state_ = send(response_socket_tcp_, list, 2);
			return zmq_state_ == zmq_socket_state::Succeed;
		}

		/**
		* \brief 工作结束(发送到请求者)
		*/
		bool api_station::job_end(vector<sharp_char>& list)
		{
			zmq_state_ = send(list[1][0] == '_' ? request_socket_ipc_ : request_scoket_tcp_, list , 2);
			return zmq_state_ == zmq_socket_state::Succeed;
		}

	}
}