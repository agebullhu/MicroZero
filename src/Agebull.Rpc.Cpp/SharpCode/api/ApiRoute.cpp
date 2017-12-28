#include "stdafx.h"
#include "ApiRoute.h"
#include <zeromq/zhelpers.h>

#define recv_empty(socket) \
	{\
		char* empty = s_recv(socket, 0);\
		if(empty != nullptr){\
			/*assert(empty[0] == 0);*/\
			free(empty);\
		}\
	}

ApiRoute::ApiRoute(const char* service, const char* routeAddress, const char* workerAddress, const char* heartAddress)
	: _serviceName(service)
	, _routeAddress(routeAddress)
	, _workerAddress(workerAddress)
	, _heartAddress(heartAddress)
	, _nowWorkIndex(0)
	, _routeSocket(nullptr)
	, _heartSocket(nullptr)
	, _workSocket(nullptr)
	, _isRuning(false)
{
}

/*
 *取下一个可用的工作者名称
 *
 LRU逻辑
 -一直从_workSocket中获取消息；当有超过一个worker空闲时才从_routeSocket获取消息。
 - 当woker回应时，会将该worker标记为已准备好，并转发woker的回应给client
 - 如果client发送了请求，就将该请求转发给下一个worker
 */
char* ApiRoute::getNextWorker()
{
	if (_workerAddress.size() == 0)
		return nullptr;
	if (_workerAddress.size() == 1)
		return _strdup(_hosts[0].c_str());
	while (_hosts.size() == 0)
	{
		thread_sleep(1);
	}
	auto host = _hosts.back();
	_hosts.pop_back();
	return _strdup(host.c_str());
}
/*
 * 当远程调用进入时的处理
 */
void ApiRoute::onCallerPollIn()
{
	// 获取下一个client的请求，交给空闲的worker处理
	// client请求的消息格式是：[client地址][空帧][请求内容]
	char* client_addr = s_recv(_routeSocket, 0);
	recv_empty(_routeSocket);
	char* request = s_recv(_routeSocket);

	while (true)
	{
		char* work = getNextWorker();
		if (snedToWorker(work,client_addr, request))
			break;
		leftWorker(work);
	}

	free(client_addr);
	free(request);
}
bool ApiRoute::snedToWorker(char* work ,char* client_addr, char* request)
{
	//路由到其中一个工作对象
	if (work == nullptr)
	{
		s_sendmore(_routeSocket, client_addr);
		s_sendmore(_routeSocket, "");
		s_send(_routeSocket, "NoWorker");
		return true;
	}
	else
	{
		int state = s_sendmore(_workSocket, work);
		if (state < 0)
			return false;
		state = s_sendmore(_workSocket, "");
		if (state < 0)
			return false;
		state = s_sendmore(_workSocket, client_addr);
		if (state < 0)
			return false;
		state = s_sendmore(_workSocket, "");
		if (state < 0)
			return false;
		state = s_send(_workSocket, request);//真实发送
		return state > 0;
	}
}
/*
 * 当工作操作返回时的处理
 */
void ApiRoute::onWorkerPollIn()
{
	// 将worker的地址入队
	char* worker_addr = s_recv(_workSocket);
	recv_empty(_workSocket);
	char* client_addr = s_recv(_workSocket);
	recv_empty(_workSocket);
	char* reply = s_recv(_workSocket);
	// 如果是一个应答消息，则转发给client
	if (strcmp(client_addr, "READY") == 0)
	{
		joinWorker(worker_addr, reply,true);
	}
	else
	{
		s_sendmore(_routeSocket, client_addr);
		s_sendmore(_routeSocket, "");
		s_send(_routeSocket, reply);//真实发送
		_hosts.insert(_hosts.begin(), worker_addr);
	}
	free(worker_addr);
	free(client_addr);
	free(reply);
}

/*
* 当工作操作返回时的处理
*/
void ApiRoute::joinWorker(char* name, char* address, bool ready)
{
	auto old = workers.find(name);
	if (old == workers.end())
	{
		workers.insert(make_pair(string(name), string(address)));
		_hosts.push_back(string(name));
		cout << name << "(" << address << ")已加入(通过" << (ready ? "启动)" : "心跳)") << endl;
	}
	else
	{
		old->second = address;
		cout << name << "(" << address << ")还活着(通过心跳)" << endl;
	}
}
/*
* 当工作操作返回时的处理
*/
void ApiRoute::leftWorker(char* name)
{
	auto old = workers.find(name);
	if (old != workers.end())
	{
		workers.erase(name);
		auto iter = _hosts.begin();
		while (iter != _hosts.end())
		{
			if (*iter == name)
			{
				_hosts.erase(iter);
				break;
			}
			iter++;
		}
		cout << name << "已退出" << endl;
	}
}

/*
* 当工作操作返回时的处理
*/
void ApiRoute::onHeartbeat()
{
	// 将worker的地址入队
	char* worker_addr = s_recv(_heartSocket);
	recv_empty(_heartSocket);
	char* client_addr = s_recv(_heartSocket);
	recv_empty(_heartSocket);
	char* reply = s_recv(_heartSocket);
	// 如果是一个应答消息，则转发给client
	if (strcmp(client_addr, "PAPA") == 0)
	{
		joinWorker(worker_addr, reply);
	}
	else if (strcmp(client_addr, "MAMA") == 0)
	{
		joinWorker(worker_addr, reply);
	}
	else if (strcmp(client_addr, "LAOWANG") == 0)
	{
		leftWorker(worker_addr);
	}
	size_t size = s_sendmore(_heartSocket, worker_addr);
	if (size <= 0)
		cout << worker_addr << endl;
	size = s_sendmore(_heartSocket, "");
	size = s_send(_heartSocket, "OK");//真实发送
	if (size <= 0)
		cout << worker_addr << endl;

	free(worker_addr);
	free(client_addr);
	free(reply);
}
#define init_socket(socket) \
	zmq_setsockopt(socket, ZMQ_LINGER, &iZMQ_IMMEDIATE, sizeof(int));\
	zmq_setsockopt(socket, ZMQ_LINGER, &iLINGER, sizeof(int));\
	zmq_setsockopt(socket, ZMQ_RCVTIMEO, &iRcvTimeout, sizeof(int));\
	zmq_setsockopt(socket, ZMQ_SNDTIMEO, &iSndTimeout, sizeof(int));

void ApiRoute::poll()
{
	auto context = get_zmq_context();
	_routeSocket = zmq_socket(context, ZMQ_ROUTER);
	_workSocket = zmq_socket(context, ZMQ_ROUTER);
	_heartSocket = zmq_socket(context, ZMQ_ROUTER);

	int iZMQ_IMMEDIATE = 1;//列消息只作用于已完成的链接
	int iLINGER = 50;//关闭设置停留时间,毫秒
	int iRcvTimeout = 500;
	int iSndTimeout = 500;

	init_socket(_routeSocket);
	init_socket(_workSocket);
	init_socket(_heartSocket);

	zmq_bind(_routeSocket, _routeAddress.c_str());
	zmq_bind(_workSocket, _workerAddress.c_str());
	zmq_bind(_heartSocket, _heartAddress.c_str());

	zmq_pollitem_t items[] = {
		{ _workSocket, 0, ZMQ_POLLIN, 0 },
		{ _routeSocket, 0, ZMQ_POLLIN, 0 },
		{ _heartSocket, 0, ZMQ_POLLIN, 0 }
	};
	log_msg1("Api服务(%s)路由已启动", _serviceName);
	//登记线程开始
	set_command_thread_start();
	_isRuning = true;
	while (get_net_state() == NET_STATE_RUNING && _isRuning)
	{
		zmq_poll(items, 3, 1000);
		// 处理_workSocket中worker的队列
		if (items[0].revents & ZMQ_POLLIN) {
			onWorkerPollIn();
		}

		if (items[1].revents & ZMQ_POLLIN) {
			onCallerPollIn();
		}

		if (items[2].revents & ZMQ_POLLIN) {
			onHeartbeat();
		}
	}
	_isRuning = false;
	zmq_unbind(_routeSocket, _routeAddress.c_str());
	zmq_close(_routeSocket);
	zmq_unbind(_workSocket, _workerAddress.c_str());
	zmq_close(_workSocket);
	zmq_unbind(_heartSocket, _heartAddress.c_str());
	zmq_close(_heartSocket);
	//登记线程关闭
	set_command_thread_end();
	log_msg1("Api服务(%s)路由已关闭", _serviceName);
}

ApiRoute::~ApiRoute()
{
	if (!_isRuning)
		return;
	_isRuning = false;
	thread_sleep(1000);
}
