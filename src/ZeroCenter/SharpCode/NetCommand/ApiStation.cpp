#include "stdafx.h"
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
			inproc_request_socket<ZMQ_REQ> socket(caller, _station_name.c_str());
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
		 * 当远程调用进入时的处理
		 */
		void api_station::request(ZMQ_HANDLE socket, bool inner)
		{
			vector<sharp_char> list;
			//0 请求地址 1 空帧 2命令 3请求标识 4上下文 5参数
			_zmq_state = recv(socket, list);
			if (_zmq_state == zmq_socket_state::TimedOut)
			{
				return;
			}
			if (_zmq_state != zmq_socket_state::Succeed)
			{
				log_error3("接收消息失败%s(%d)%s", _station_name.c_str(), _inner_port, state_str(_zmq_state));
				return;
			}
			const size_t list_size = list.size();
			if (list_size <= 3)
			{
				_zmq_state = send_addr(socket, *list[0]);
				_zmq_state = send_late(socket, ZERO_STATUS_FRAME_INVALID);
				return;
			}
			sharp_char& description = list[2];
			//内部命令通知退出
			if (inner && description[1] == 'B')
			{
				worker_left(*list[3]);
				_zmq_state = send_addr(socket, *list[0]);
				_zmq_state = send_late(socket, ZERO_STATUS_OK);
				return;
			}
			const size_t frame_size = static_cast<size_t>(description[0]);
			if(list_size <= 3 || frame_size >= description.size() || (frame_size + 4) != list_size)
			{
				_zmq_state = send_addr(socket, *list[0]);
				_zmq_state = send_late(socket, ZERO_STATUS_FRAME_INVALID);
				return;
			}

			char* const buf = description.get_buffer();
			for (size_t idx = 1; idx <= frame_size; idx++)
			{
				if (buf[idx] == ZERO_FRAME_PLAN)
				{
					job_plan(socket, list);
					return;
				}
			}
			job_start(socket, list);
		}
		/**
		* \brief 工作进入计划
		*/
		bool api_station::job_plan(ZMQ_HANDLE socket,vector<sharp_char>& list)
		{
			const sharp_char& caller = list[0];
			sharp_char& description = list[2];
			char* const buf = description.get_buffer();
			const size_t frame_size = static_cast<size_t>(buf[0]);

			sharp_char plan, context, arg, command, requester, id;
			for (size_t idx = 1; idx <= frame_size; idx++)
			{
				switch (buf[idx])
				{
				case ZERO_FRAME_PLAN:
					plan = list[3 + idx];
					break;
				case ZERO_FRAME_REQUESTER:
					requester = list[3 + idx];
					break;
				case ZERO_FRAME_CONTEXT:
					context = list[3 + idx];
					break;
				case ZERO_FRAME_ARG:
					arg = list[3 + idx];
					break;
				case ZERO_FRAME_REQUEST_ID:
					id = list[3 + idx];
					break;
				case ZERO_FRAME_COMMAND:
					command = list[3 + idx];
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
			buf[0] = cnt;

			message.read_plan(plan.get_buffer());
			message.request_caller = caller;
			message.messages_description = description;
			plan_next(message, true);


			_zmq_state = send_addr(socket, *caller);
			_zmq_state = send_late(socket, ZERO_STATUS_PLAN);
			return true;

		}
		/**
		* \brief 工作开始（发送到工作者）
		*/
		bool api_station::job_start(ZMQ_HANDLE socket, vector<sharp_char>& list)
		{
			const sharp_char& caller = list[0];
			//路由到其中一个工作对象
			const char* worker = _balance.get_host();
			if (worker == nullptr)
			{
				_zmq_state = send_addr(socket, *caller);
				_zmq_state = send_late(socket, ZERO_STATUS_API_NOT_WORKER);
				return _zmq_state == zmq_socket_state::Succeed;
			}
			sharp_char& description = list[2];
			char* const buf = description.get_buffer();
			const size_t frame_size = static_cast<size_t>(buf[0]);

			sharp_char plan, context, arg, command, requester, id;
			for (size_t idx = 1; idx <= frame_size; idx++)
			{
				switch (buf[idx])
				{
				case ZERO_FRAME_PLAN:
					plan = list[3 + idx];
					break;
				case ZERO_FRAME_REQUESTER:
					requester = list[3 + idx];
					break;
				case ZERO_FRAME_CONTEXT:
					context = list[3 + idx];
					break;
				case ZERO_FRAME_ARG:
					arg = list[3 + idx];
					break;
				case ZERO_FRAME_REQUEST_ID:
					id = list[3 + idx];
					break;
				case ZERO_FRAME_COMMAND:
					command = list[3 + idx];
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
			buf[0] = cnt;

			message.read_plan(plan.get_buffer());
			message.request_caller = caller;
			message.messages_description = description;

			_zmq_state = send_addr(_inner_socket, worker);
			_zmq_state = send(_inner_socket, list);
			return _zmq_state == zmq_socket_state::Succeed;
		}

		/**
		* \brief 工作结束(发送到请求者)
		*/
		bool api_station::job_end(vector<sharp_char>& list)
		{
			_zmq_state = send(list[1][0] == '_' ? _out_socket_inproc : _out_socket, list , 2);
			return _zmq_state == zmq_socket_state::Succeed;
		}

		/**
		 * 当工作操作返回时的处理
		 */
		void api_station::response()
		{
			vector<sharp_char> list;
			//0 worker地址 1空帧 2请求者地址
			_zmq_state = recv(_inner_socket, list);
			if (_zmq_state == zmq_socket_state::TimedOut)
			{
				return;
			}
			if (_zmq_state != zmq_socket_state::Succeed)
			{
				log_error3("接收结果失败%s(%d)%s", _station_name.c_str(), _inner_port, state_str(_zmq_state));
				return;
			}
			/*
###### 状态标识
- +：表示成功状态
- -：表示失败状态
- @：表示加入API或是Vote工作组 
- $：表示继续等待回复  
- !：表示退出API或是Vote工作组
			 */
			switch (list[2][0])
			{
			case ZERO_WORKER_JOIN:
				worker_join(*list[0], *list[3], true);
				send_addr(_inner_socket, *list[0]);
				send_late(_inner_socket, ZERO_STATUS_WECOME);
				return;
			case ZERO_WORKER_LEFT:
				worker_left(*list[0]);
				send_addr(_inner_socket, *list[0]);
				send_late(_inner_socket, ZERO_STATUS_BYE);
				return;
			case ZERO_WORKER_LISTEN:
				return;
			default:
				_zmq_state = send(list[1][0] == '_' ? _out_socket_inproc : _out_socket, list, 2);
				break;
			}
		}
	}
}