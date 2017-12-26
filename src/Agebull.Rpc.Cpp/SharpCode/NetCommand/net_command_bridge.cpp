#include "stdafx.h"
#ifdef CLR
#include "Proxy/NetCommandProxy.h"
#pragma unmanaged
#endif

using namespace std;


//C端的命令调用队列
queue<NetCommandCall> client_cmd_queue;
///C端命令调用队列锁
HANDLE client_cmd_mutex;

//C端的命令消息队列
std::queue<PNetCommand> client_msg_queue;
//C端命令息队列锁
HANDLE client_msg_mutex;

//客户端用户标识
char* request_user_token = nullptr;
//客户端用户标识
void set_user_token(char* token)
{
	request_user_token = token;
}
//客户端用户标识
char* get_user_token()
{
	return request_user_token;
}


DWORD WINAPI client_cmd(LPVOID arg)
{
	char *address = static_cast<char*>(arg);
	client_cmd_mutex = OpenMutex(SYNCHRONIZE, TRUE, nullptr);
	ZMQ_HANDLE socket = zmq_socket(get_zmq_context(), ZMQ_REQ);
	if (socket == nullptr)
	{
		std::cout << "构造SOCKET对象发生错误:" << zmq_strerror(errno) << std::endl;
		return 0;
	}
	int zmq_result = zmq_connect(socket, address);//"tcp://localhost:8888"
	if (zmq_result < 0)
	{
		std::cout << "client_cmd连接端口发生错误:" << zmq_strerror(errno) << std::endl;
		return 0;
	}
	zmq_setsockopt(socket, ZMQ_IDENTITY, request_user_token, strlen(request_user_token));
	int iRcvTimeout = 10000;
	zmq_setsockopt(socket, ZMQ_RCVTIMEO, &iRcvTimeout, sizeof(iRcvTimeout));
	zmq_setsockopt(socket, ZMQ_SNDTIMEO, &iRcvTimeout, sizeof(iRcvTimeout));

	bool restart = false;
	while (get_net_state() == NET_STATE_RUNING)
	{
		NetCommandCall cmd_call;
		if (client_cmd_queue.empty())
		{
			//std::cout << "命令空载" << endl;
			thread_sleep(1);
			continue;
		}
		{
			WaitForSingleObject(client_cmd_mutex, 1000);
			cmd_call = client_cmd_queue.front();
			client_cmd_queue.pop();
			ReleaseMutex(client_cmd_mutex);
		}

		PNetCommand cmd = cmd_call.cmd;
		//cmd_callback_fn* callback = cmd_call.callback;
		//复制用户头
		cout << request_user_token << endl;
		strcpy_s(cmd->user_token, request_user_token);
		//生成命令标识
		GUID cmd_key = create_guid();
		print_guid(cmd_key, cmd->cmd_identity);
		int len = get_cmd_len(cmd);
		//发送命令请求
		zmq_msg_t msg_call;
		zmq_result = zmq_msg_init(&msg_call);
		assert(zmq_result == 0);
		zmq_result = zmq_msg_init_data(&msg_call, cmd, len, nullptr, nullptr);
		assert(zmq_result == 0);
		zmq_result = zmq_msg_send(&msg_call, socket, ZMQ_DONTWAIT);


		if (zmq_result < 0)
		{
			cout << "用户" << cmd->user_token << "命令(" << cmd->cmd_id << "->" << cmd->cmd_identity << ")发送错误:" << zmq_strerror(errno) << std::endl;

			WaitForSingleObject(client_msg_mutex, 1000);
			cmd->cmd_state = NET_COMMAND_STATE_NETERROR;
			ReleaseMutex(client_msg_mutex);
			restart = true;
			break;
		}

		cout << "用户" << cmd->user_token << "命令(" << cmd->cmd_id << "->" << cmd->cmd_identity << ")发送成功!" << std::endl;
		WaitForSingleObject(client_msg_mutex, 1000);
		cmd->cmd_state = NET_COMMAND_STATE_SENDED;
		ReleaseMutex(client_msg_mutex);
		//接收处理反馈

		char result[10];
		zmq_result = zmq_recv(socket, result, 10, 0);
		if (zmq_result < 0)
		{
			cout << "用户" << cmd->user_token << "命令(" << cmd->cmd_id << "->" << cmd->cmd_identity << ")接收回执错误:" << zmq_strerror(errno) << std::endl;

			WaitForSingleObject(client_msg_mutex, 1000);
			cmd->cmd_state = NET_COMMAND_STATE_UNKNOW;
			ReleaseMutex(client_msg_mutex);
			restart = true;
			break;
		}
		cout << "用户" << cmd->user_token << "命令(" << cmd->cmd_id << "->" << cmd->cmd_identity << ")已收到回执!" << std::endl;

		//zmq_msg_t msg_result;
		//zmq_result = zmq_msg_init(&msg_result);
		//assert(zmq_result == 0);
		//zmq_result = zmq_msg_recv(&msg_result, socket, 0);
		//check_request_zmq_state(zmq_result, "接收返回", cmd, callback, msg_result);

		//PNetCommand cmd_msg = static_cast<PNetCommand>(zmq_msg_data(&msg_result));
		//cout << "命令" << cmd_msg->cmd_id << "调用状态:" << cmd_msg->cmd_state << std::endl;
		//len = get_cmd_len(cmd_msg);
		//PNetCommand cmd_cpy = reinterpret_cast<PNetCommand>(new char[len]);
		//memcpy(cmd_cpy, cmd_msg, len);
		//{
		//	WaitForSingleObject(client_msg_mutex, 1000);
		//	client_msg_queue.push(cmd_cpy);
		//	ReleaseMutex(client_msg_mutex);
		//}

		zmq_msg_close(&msg_call);
	}
	zmq_close(socket);
	CloseHandle(client_cmd_mutex);
	zmq_close(socket);
	CloseHandle(client_cmd_mutex);
	if (restart)
	{
		thread_sleep(1);
		start_thread(client_cmd, arg);
	}
	return 0;
}
DWORD WINAPI client_sub(LPVOID arg)
{
	char *address = static_cast<char*>(arg);
	ZMQ_HANDLE socket = zmq_socket(get_zmq_context(), ZMQ_SUB);
	zmq_connect(socket, address);//"tcp://127.0.0.1:7777"
	zmq_setsockopt(socket, ZMQ_SUBSCRIBE, request_user_token, 1);
	int iRcvTimeout = 10000;
	zmq_setsockopt(socket, ZMQ_RCVTIMEO, &iRcvTimeout, sizeof(iRcvTimeout));
	zmq_setsockopt(socket, ZMQ_SNDTIMEO, &iRcvTimeout, sizeof(iRcvTimeout));
	while (get_net_state() == NET_STATE_RUNING)
	{
		zmq_msg_t msg_sub;
		int zmq_result = zmq_msg_init(&msg_sub);
		assert(zmq_result == 0);
		zmq_result = zmq_msg_recv(&msg_sub, socket, 0);
		if (zmq_result <= 0)
		{
			zmq_msg_close(&msg_sub);
			thread_sleep(1);
			int error = errno;
			if (error != 0)
				std::cout << "接收订阅时发生错误:" << zmq_strerror(errno) << std::endl;
			continue;
		}
		std::cout << "接收订阅成功!" << std::endl;

		PNetCommand cmd_msg = static_cast<PNetCommand>(zmq_msg_data(&msg_sub));
		int len = get_cmd_len(cmd_msg);
		PNetCommand cmd_cpy = reinterpret_cast<PNetCommand>(new char[len]);
		memcpy(cmd_cpy, cmd_msg, len);
		{
			WaitForSingleObject(client_msg_mutex, 1000);
			client_msg_queue.push(cmd_cpy);
			ReleaseMutex(client_msg_mutex);
		}
		zmq_msg_close(&msg_sub);
	}
	zmq_close(socket);
	return 0;
}
//客户端命令调用
void request_net_cmmmand(PNetCommand cmd, cmd_callback_fn* callback)
{
	NetCommandCall call;
	call.cmd = cmd;
	call.callback = callback;
	WaitForSingleObject(client_cmd_mutex, 1000);
	client_cmd_queue.push(call);
	ReleaseMutex(client_cmd_mutex);
}
#ifdef CLR
#pragma managed
#endif
//客户端消息泵
DWORD WINAPI client_message_pump(LPVOID)
{
	client_msg_mutex = OpenMutex(SYNCHRONIZE, TRUE, nullptr);
	while (get_net_state() == NET_STATE_RUNING)
	{
		if (client_msg_queue.empty())
		{
			//std::cout << "消息空载" << endl;
			thread_sleep(1);
			continue;
		}
		PNetCommand cmd_msg;
		{
			WaitForSingleObject(client_msg_mutex, 1000);
			cmd_msg = client_msg_queue.front();
			client_msg_queue.pop();
			ReleaseMutex(client_msg_mutex);
		}
		cout << "命令" << cmd_msg->cmd_id << "调用状态:" << cmd_msg->cmd_state << std::endl;
#ifdef CLR
		GBS::Futures::Client::Trade::NetCommandProxy::get_Single()->FireEvents(cmd_msg);
#endif
		delete[] cmd_msg;
	}
	CloseHandle(client_msg_mutex);
	return 0;
}