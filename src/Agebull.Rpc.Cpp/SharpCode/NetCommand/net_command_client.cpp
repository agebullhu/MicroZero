#include <stdafx.h>
#include "entity.h"
#include "debug/TraceStack.h"
#ifdef  WEB
#define USER_TOKEN "$WA"
#define CMD_TOKEN "@WA"
#define INNER_CLIENT
#endif

using namespace std;


//C端的命令调用队列
queue<NetCommandArgPtr> call_queue;
///C端命令调用队列锁
HANDLE call_mutex;

//C端的命令消息队列
std::queue<NetCommandArgPtr> back_queue;
//C端命令息队列锁
HANDLE back_mutex;
//行情消息队列
std::queue<NetCommandArgPtr> quote_queue;
//C端命令息队列锁
HANDLE quote_mutex;

#define front_queue(cmd_msg,queue,mutex)\
	if (WaitForSingleObject(mutex, 100) == WAIT_TIMEOUT)\
	{\
		continue;\
    }\
	if (queue.empty())\
	{\
		ReleaseMutex(mutex);\
		thread_sleep(1);\
		continue;\
	}\
	cmd_msg = queue.front();\
	queue.pop();\
	ReleaseMutex(mutex)

#define push_queue(cmd_arg,queue,mutex)\
	WaitForSingleObject(mutex, INFINITE);\
	queue.push(NetCommandArgPtr(cmd_arg));\
	ReleaseMutex(mutex)

#ifdef INNER_SERVER
//设置命令头
void set_command_head(PNetCommand call_arg, NET_COMMAND cmd)
{
	call_arg->cmd_id = cmd;
}
#endif
#ifdef CLIENT_COMMAND
//客户端用户标识
char request_user_token[GUID_LEN];
//客户端用户标识
char* get_user_token()
{
	return request_user_token;
}
//设置命令头
void set_command_head(PNetCommand arg, NET_COMMAND cmd)
{
	memset(arg, 0, NETCOMMAND_HEAD_LEN);
	arg->cmd_id = cmd;
	//复制用户头
	strcpy_s(arg->user_token, request_user_token);
	//生成命令标识
	arg->cmd_identity[0] = '*';
	GUID cmd_key = create_guid();
	print_guid(cmd_key, arg->cmd_identity + 1);
}
DWORD WINAPI client_cmd(LPVOID arg)
{
	if (get_net_state() != NET_STATE_RUNING)
		return 1;
	request_cmd(static_cast<char*>(arg));
	return 0;
}

DWORD WINAPI client_sub(LPVOID arg)
{
	notify_sub(static_cast<char*>(arg));
	return 0;
}
DWORD WINAPI quote_sub(LPVOID arg)
{
	quote_notify_sub(static_cast<char*>(arg));
	return 0;
}
//客户端消息泵
DWORD WINAPI client_message_pump(LPVOID)
{
	log_msg("客户端消息泵已启动");
	notify_message_pump();
	log_msg("客户端消息泵已关闭");
	return 0;
}
#endif
//初始化客户端
void init_client()
{
	//客户端用户标识
#ifdef CLIENT_COMMAND
	GUID guid = create_guid();
	print_guid(guid, request_user_token + 1);
	request_user_token[0] = '$';
	log_msg1("用户令牌%s",request_user_token);
#endif
	call_mutex = CreateMutex(nullptr, FALSE, nullptr);
	back_mutex = CreateMutex(nullptr, FALSE, nullptr);
	quote_mutex = CreateMutex(nullptr, FALSE, nullptr);
}
//清理客户端
void distory_client()
{
	CloseHandle(call_mutex);
	CloseHandle(back_mutex);
	CloseHandle(quote_mutex);
}
#ifdef INNER_SETTLEMENT
#pragma warning(disable: 4996)
int quote_sub()
{
	int cnt = 0;
	string& cfg = config::get_config("quote_addr");
	std::vector<std::string> address;
	boost::split(address, cfg, boost::is_any_of(_T(",")));
	for (string& addr : address)
	{
		size_t len = addr.length() + 1;
		char* ca = new char[len];
		strcpy_s(ca, len, addr.c_str());

		boost::thread thrds_r1(boost::bind(&quote_notify_sub, ca));
		cnt++;
	}
	return cnt;
}
#pragma warning(default: 4996)
#endif
#ifdef INNER_SERVER
void proxy_sub()
{
	notify_sub(config::get_config("px_sub_addr").c_str());
}

void proxy_cmd()
{
	request_cmd(config::get_config("px_cmd_addr").c_str());
}

void proxy_message_pump()
{
	notify_message_pump();
}
#endif
//命令调用
void request_net_cmmmand(NetCommandArgPtr& arg)
{
#ifndef CLIENT_COMMAND
	if (arg->cmd_id == 0)
		arg->cmd_id = NET_COMMAND_BUSINESS_NOTIFY;
#endif
	if (arg.m_command->cmd_state == NET_COMMAND_STATE_SERVER_UNKNOW)
	{
		log_error2("消息返回异常(错误代码%d)\r\n%s", arg.m_command->cmd_state, get_call_stack());
	}
	else if (arg.m_command->cmd_state == NET_COMMAND_STATE_DATA && (arg.m_command->data_len <= 0 || arg.m_command->data_len > 0x4000))
	{
		log_error1("消息返回异常(数据返回为空)\r\n%s", get_call_stack());
	}
	else if (arg.m_command->cmd_state != NET_COMMAND_STATE_DATA && arg.m_command->data_len != 0)
	{
		log_error1("消息返回异常(状态返回却带了数据)\r\n%s", get_call_stack());
	}
	push_queue(arg, call_queue, call_mutex);
}

//发送出错的重试处理
void on_retry(const char* address, NetCommandArgPtr call_arg, const char* state)
{
	if (call_arg->try_num >= 5)
	{
		call_arg->cmd_state = NET_COMMAND_STATE_NETERROR;
		push_queue(call_arg, back_queue, back_mutex);
		log_error4("%s:命令%d(%s)%s,且重试次数太多(5次),已中止命令", 
			address, 
			call_arg->cmd_id, 
			call_arg->cmd_identity, 
			state);
	}
	else
	{
		call_arg->cmd_state = NET_COMMAND_STATE_UNKNOW;
		push_queue(call_arg, call_queue, call_mutex);
		log_error5("%s:命令%d(%s)%s,正在排队重发(%d)", 
			address, 
			call_arg->cmd_id, 
			call_arg->cmd_identity,
			state,
			call_arg->try_num);
	}
}

void request_cmd(const char* address)
{
	log_msg1("命令请求泵(%s)正在启动", address);
	ZMQ_HANDLE socket = zmq_socket(get_zmq_context(), ZMQ_REQ);
	if (socket == nullptr)
	{
		log_error2("命令请求泵(%s),构造SOCKET对象发生错误:%s", address, zmq_strerror(errno));
		return;
	}
	int zmq_result/* = zmq_socket_monitor(socket, "inproc://request_cmd.rep", ZMQ_EVENT_ALL)*/;
	//assert(zmq_result == 0);
	zmq_result = zmq_connect(socket, address);
	if (zmq_result < 0)
	{
		log_error2("命令请求泵(%s)连接发生错误:%s", address, zmq_strerror(errno));
		return;
	}
#ifdef CLIENT_COMMAND
	zmq_setsockopt(socket, ZMQ_IDENTITY, request_user_token, strlen(request_user_token));
#endif
	int iZMQ_IMMEDIATE = 1;//列消息只作用于已完成的链接
	zmq_setsockopt(socket, ZMQ_LINGER, &iZMQ_IMMEDIATE, sizeof(int));
	int iLINGER = 50;//关闭设置停留时间,毫秒
	zmq_setsockopt(socket, ZMQ_LINGER, &iLINGER, sizeof(int));
	int iRcvTimeout = 500;
	zmq_setsockopt(socket, ZMQ_RCVTIMEO, &iRcvTimeout, sizeof(int));
	int iSndTimeout = 500;
	zmq_setsockopt(socket, ZMQ_SNDTIMEO, &iSndTimeout, sizeof(int));

	log_msg1("命令请求泵(%s)已启动", address);
	//登记线程开始
	set_command_thread_start();
	int state = 0;
	while (get_net_state() == NET_STATE_RUNING)
	{
		if (call_queue.empty())
		{
			thread_sleep(1);
			continue;
		}
		NetCommandArgPtr call_arg;
		front_queue(call_arg, call_queue, call_mutex);
#ifdef INNER_CLIENT
		for (int idx = 0;idx < GUID_LEN;idx++)
		{
			call_arg->user_token[0] = '\0';
			call_arg->cmd_identity[0] = '\0';
		}
		strcpy_s(call_arg->user_token, USER_TOKEN);
		strcpy_s(call_arg->cmd_identity, CMD_TOKEN);
#endif
		call_arg->try_num += 1;
		write_crc(call_arg.m_command);

		int len = get_cmd_len(call_arg);
		//发送命令请求
		zmq_msg_t msg_call;
		zmq_result = zmq_msg_init(&msg_call);
		if (zmq_result != 0)
		{
			state = 2;//出错了
			break;
		}
		zmq_result = zmq_msg_init_data(&msg_call, call_arg.m_buffer, len, nullptr, nullptr);
		if (zmq_result != 0)
		{
			state = 2;//出错了
			break;
		}
		zmq_result = zmq_msg_send(&msg_call, socket, 0);
		zmq_msg_close(&msg_call);
		if (zmq_result < 0)
		{
			switch (errno)
			{
			case ETERM:
				log_error2("命令%d(%s)发送错误[与指定的socket相关联的context被终结了],自动关闭", call_arg->cmd_id, call_arg->cmd_identity);
				state = 1;
				break;
			case ENOTSOCK:
				log_error2("命令%d(%s)发送错误[指定的socket不可用],自动重启", call_arg->cmd_id, call_arg->cmd_identity);
				state = 2;
				break;
			case EINTR:
				log_error2("命令%d(%s)发送错误[在接接收到消息之前，这个操作被系统信号中断了],自动重启", call_arg->cmd_id, call_arg->cmd_identity);
				state = 2;
				break;
			case EAGAIN:
				//使用非阻塞方式接收消息的时候没有接收到任何消息。
			default:
				state = 0;
				on_retry(address, call_arg, "发送失败");
				break;
			}
			if (state > 0)
				break;
			continue;
		}
		log_debug4(DEBUG_CALL, 3, "%s:命令%d(%s)发送成功(%d)", address, call_arg->cmd_id, call_arg->cmd_identity, zmq_result);
		call_arg->cmd_state = NET_COMMAND_STATE_SENDED;
		//接收处理反馈
		char result[10];
		zmq_result = zmq_recv(socket, result, 10, 0);
		if (zmq_result < 0)
		{
			switch (errno)
			{
			case ETERM:
				log_error2("命令%d(%s)接收回执错误[与指定的socket相关联的context被终结了],自动关闭", call_arg->cmd_id, call_arg->cmd_identity);
				state = 1;
				break;
			case ENOTSOCK:
				log_error2("命令%d(%s)接收回执错误[指定的socket不可用],自动重启", call_arg->cmd_id, call_arg->cmd_identity);
				state = 2;
				break;
			case EINTR:
				log_error2("命令%d(%s)接收回执错误[在接接收到消息之前，这个操作被系统信号中断了],自动重启", call_arg->cmd_id, call_arg->cmd_identity);
				state = 2;
				break;
			case EAGAIN://使用非阻塞方式接收消息的时候没有接收到任何消息。
			default:
				state = 2;
				break;
			}
			on_retry(address, call_arg, "未收到回执");
			break;
		}
		if (result[0] == '0')
		{
			if (get_net_state() != NET_STATE_RUNING)
			{
				zmq_msg_close(&msg_call);
				break;
			}
			on_retry(address, call_arg, "收到错误回执");
		}
		else
		{
			log_debug3(DEBUG_CALL, 3, "%s:命令%d(%s)已收到正确回执", address, call_arg->cmd_id, call_arg->cmd_identity);
		}
		zmq_msg_close(&msg_call);
	}
	try
	{
		zmq_disconnect(socket, address);
	}
	catch (const std::exception& ex)
	{
		log_error1("%s:命令关闭异常", address);
	}
	zmq_close(socket);
	//登记线程关闭
	set_command_thread_end();
	if (state == 2 && get_net_state() == NET_STATE_RUNING)
	{
		log_msg("客户端命令泵正在重启");
#ifdef CLIENT_COMMAND
		start_thread(client_cmd, LPVOID(address));
#else
		boost::thread thrds(boost::bind(&proxy_cmd));
#endif
	}
	else
	{
		log_msg("客户端命令泵已关闭");
	}
}

DWORD quote_notify_sub(const char* address)
{
	log_msg1("行情订阅(%s)正在启动", address);
	ZMQ_HANDLE socket = zmq_socket(get_zmq_context(), ZMQ_SUB);

	if (socket == nullptr)
	{
		log_error2("行情订阅(%s),构造SOCKET对象发生错误:%s", address, zmq_strerror(errno));
		return 0;
	}
	int zmq_result = zmq_connect(socket, address);
	if (zmq_result < 0)
	{
		log_error2("行情订阅(%s)连接发生错误:%s", address, zmq_strerror(errno));
		return 0;
	}
	zmq_setsockopt(socket, ZMQ_SUBSCRIBE, nullptr, 0);
	int iRcvTimeout = 500;
	zmq_setsockopt(socket, ZMQ_RCVTIMEO, &iRcvTimeout, sizeof(iRcvTimeout));
	log_msg1("行情订阅(%s)已启动", address);
	//登记线程开始
	set_command_thread_start();
	while (get_net_state() == NET_STATE_RUNING)
	{
		zmq_msg_t msg_sub;
		zmq_result = zmq_msg_init(&msg_sub);
		assert(zmq_result == 0);
		zmq_result = zmq_msg_recv(&msg_sub, socket, 0);
		if (zmq_result <= 0)
		{
			zmq_msg_close(&msg_sub);
			thread_sleep(1);
			int error = errno;
			if (error != 0)
				log_error2("接收行情订阅(%s)时发生错误(%s)", address, zmq_strerror(errno));
			continue;
		}
		char* msg = static_cast<char*>(zmq_msg_data(&msg_sub));

		if (!check_crc(reinterpret_cast<PNetCommand>(msg)))// (msg[len - 1] != '#')
		{
			log_error1("接收行情订阅(%s)时发生错误:数据不正确", address);
			zmq_msg_close(&msg_sub);
			continue;
		}
#ifdef CLIENT_TEST
		PNetCommand cmd_msg = static_cast<PNetCommand>(zmq_msg_data(&msg_sub));
		void* data = Agebull::Futures::DataModel::DataFactory::get_command_data(cmd_msg);
		delete data;
#endif 
#if (defined INNER_SETTLEMENT) || (defined CLIENT_COMMAND)
		size_t len = zmq_msg_size(&msg_sub);
		char* buf = new char[len];
		memcpy(buf, msg, len);
		push_queue(reinterpret_cast<PNetCommand>(buf), quote_queue, quote_mutex);
#endif
		zmq_msg_close(&msg_sub);
	}
	zmq_disconnect(socket, address);
	log_msg1("行情订阅(%s)已关闭!", address);
	zmq_close(socket);
	//登记线程关闭
	set_command_thread_end();
	return 1;
}

#ifdef INNER_SETTLEMENT
void quote_pump()
{
	while (get_net_state() != NET_STATE_DISTORY)
	{
		NetCommandArgPtr cmd_msg;
		front_queue(cmd_msg, quote_queue, quote_mutex);
#ifdef RISK_CONTROL
		Agebull::Futures::RiskControl::RiskControler::check_quote(reinterpret_cast<PNetCommand>(cmd_msg.m_command));
#endif
#ifdef TRADE_SETTLEMENT
		Agebull::Futures::TradeSettlement::RealSettlement::check_quote(reinterpret_cast<PNetCommand>(cmd_msg.m_command));
#endif
	}
}
#endif
void notify_sub(const char* address)
{
	log_msg1("客户端消息订阅(%s)正在启动", address);
	ZMQ_HANDLE socket = zmq_socket(get_zmq_context(), ZMQ_SUB);
	if (socket == nullptr)
	{
		log_error2("客户端消息订阅(%s),构造SOCKET对象发生错误:%s", address, zmq_strerror(errno));
		return;
	}
	int zmq_result/* = zmq_socket_monitor(socket, "inproc://notify_sub.rep", ZMQ_EVENT_ALL);
	assert(zmq_result == 0)*/;
	zmq_result = zmq_connect(socket, address);
	if (zmq_result < 0)
	{
		log_error2("客户端消息订阅(%s)连接发生错误:%s", address, zmq_strerror(errno));
		return;
	}
#ifdef CLIENT
	log_msg2("客户端消息订阅(%s)过滤器为(%s)", address, request_user_token);
	zmq_setsockopt(socket, ZMQ_SUBSCRIBE, request_user_token, strlen(request_user_token));
#else
	zmq_setsockopt(socket, ZMQ_SUBSCRIBE, nullptr, 0);
#endif
#ifdef CLIENT_TEST
	Agebull::Futures::TradeManagement::TestNotify notify;
#endif
	int iRcvTimeout = 500;
	zmq_setsockopt(socket, ZMQ_RCVTIMEO, &iRcvTimeout, sizeof(iRcvTimeout));
	log_msg1("客户端消息订阅(%s)已启动", address);
	//登记线程开始
	set_command_thread_start();
	while (get_net_state() == NET_STATE_RUNING)
	{
		zmq_msg_t msg_sub;
		zmq_result = zmq_msg_init(&msg_sub);
		assert(zmq_result == 0);
		zmq_result = zmq_msg_recv(&msg_sub, socket, 0);
		if (zmq_result <= 0)
		{
			zmq_msg_close(&msg_sub);
			thread_sleep(1);
			int error = errno;
			if (error != 0)
				log_error2("接收订阅(%s)时发生错误(%s)", address, zmq_strerror(errno));
			continue;
		}
		size_t len = zmq_msg_size(&msg_sub);
		PNetCommand cmd_msg = static_cast<PNetCommand>(zmq_msg_data(&msg_sub));
		if (!check_crc(cmd_msg))// (cmd_msg->user_token[0] != '$')
		{
			zmq_msg_close(&msg_sub);
			log_error("收到错误数据帧");
			continue;
		}
#ifdef CLIENT_TEST
		notify.message_pump(cmd_msg);
#else
#ifdef WEB
		//自己发的
		if (cmd_msg->cmd_identity[0] == '@' && cmd_msg->cmd_identity[1] == 'W' && cmd_msg->cmd_identity[2] == 'A' && cmd_msg->cmd_identity[3] == '\0')
		{
			zmq_msg_close(&msg_sub);
			continue;
		}
		//if (cmd_msg->cmd_id <= NET_COMMAND_USER_SET_PASSWORD || cmd_msg->cmd_id == NET_COMMAND_DATA_PUSH)
		//{
		//	zmq_msg_close(&msg_sub);
		//	continue;
		//}
#endif
#ifdef INNER_SETTLEMENT
		//期望的是数据且为发送给客户的
		if (cmd_msg->cmd_id < NET_COMMAND_DATA_CHANGED || strcmp(cmd_msg->cmd_identity, CMD_TOKEN) == 0)
		{
			zmq_msg_close(&msg_sub);
			continue;
		}
#else
#ifdef _DEBUG
		log_trace3(DEBUG_RESULT, 3, "接收到来自(%s)的命令(%d)返回的状态(%d)", address, cmd_msg->cmd_id, cmd_msg->cmd_state);
#endif
#endif
		char* buf = new char[len];
		memcpy(buf, zmq_msg_data(&msg_sub), len);
		push_queue(reinterpret_cast<PNetCommand>(buf), back_queue, back_mutex);
#endif
		zmq_msg_close(&msg_sub);
	}
	zmq_disconnect(socket, address);
	log_msg1("客户端消息订阅(%s)已关闭!", address);
	zmq_close(socket);
	//登记线程关闭
	set_command_thread_end();
}

#ifdef CLR
#pragma managed
DWORD WINAPI quote_pump(LPVOID arg)
{
	while (get_net_state() != NET_STATE_DISTORY)
	{
		NetCommandArgPtr cmd_msg;
		front_queue(cmd_msg, quote_queue, quote_mutex);
		Agebull::Futures::Globals::Client::CommandProxy::get_Single()->FireQuote(cmd_msg.m_command);
	}
	return 1;
}
void notify_message_pump()
{
	log_msg("通知消息泵已启动");
	while (get_net_state() != NET_STATE_DISTORY)
	{
		NetCommandArgPtr cmd_msg;
		front_queue(cmd_msg, back_queue, back_mutex);
		Agebull::Futures::Globals::Client::CommandProxy::get_Single()->FireEvents(cmd_msg);
	}
	log_msg("通知消息泵已关闭");
}
#endif

#ifdef GBS_TRADE
void notify_message_pump()
{
	log_msg("通知消息泵已启动");
	/*Agebull::Futures::TradeManagement::EsNotify notify;
	while (get_net_state() != NET_STATE_DISTORY)
	{
		NetCommandArgPtr cmd_msg;
		front_queue(cmd_msg, back_queue, back_mutex);
		RedisTransScope scope;
		notify.message_pump(cmd_msg);
	}*/
	log_msg("通知消息泵已关闭");
}
#endif
#ifdef RISK_CONTROL
void notify_message_pump()
{
	log_msg("通知消息泵已启动");
	Agebull::Futures::RiskControl::RiskControler notify;
	notify.Initialize();
	while (get_net_state() != NET_STATE_DISTORY)
	{
		NetCommandArgPtr cmd_msg;
		front_queue(cmd_msg, back_queue, back_mutex);
		RedisTransScope scope;
		notify.message_pump(cmd_msg);
	}
	log_msg("通知消息泵已关闭");
}
#endif
#ifdef TRADE_SETTLEMENT
void notify_message_pump()
{
	log_msg("通知消息泵已启动");
	Agebull::Futures::TradeSettlement::RealSettlement notify;
	notify.Initialize();
	while (get_net_state() != NET_STATE_DISTORY)
	{
		NetCommandArgPtr cmd_msg;
		front_queue(cmd_msg, back_queue, back_mutex);
		RedisTransScope scope;
		notify.message_pump(cmd_msg);
	}
	log_msg("通知消息泵已关闭");
}
#endif