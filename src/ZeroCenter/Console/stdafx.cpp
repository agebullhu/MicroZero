// stdafx.cpp : 只包括标准包含文件的源文件
// WindowsService.pch 将作为预编译头
// stdafx.obj 将包含预编译类型信息

#include "stdafx.h"

#ifdef CONSOLE_TEST

//
// 使用LRU算法的装置
// client和worker处于不同的线程中
//
#include <zeromq/zhelpers.h>

#define NBR_CLIENTS 10
#define NBR_WORKERS 3

// 出队操作，使用一个可存储任何类型的数组实现
#define DEQUEUE(q) memmove (&(q)[0], &(q)[1], sizeof (q) - sizeof (q [0]))

// 使用REQ套接字实现基本的请求-应答模式
// 由于s_send()和s_recv()不能处理0MQ的二进制套接字标识，
// 所以这里会生成一个可打印的字符串标识。
//
static DWORD client_task(LPVOID arg)
{
	void *context = zmq_init(1);
	void *client = zmq_socket(context, ZMQ_REQ);

	int id = (int)arg + 1;
	s_set_id(client, id); // 设置可打印的标识
	int re = zmq_connect(client, "tcp://127.0.0.1:6666");
	char identity[10];
	sprintf(identity, "%04X", (int)id);
	cout << "client connect" << re << "--" << identity << endl;

	// 发送请求并获取应答信息
	s_send(client, "HELLO");
	cout << endl << "client send" << endl;
	char *reply = s_recv(client);
	cout << "-------------------Client:" << identity <<",Rep:" << reply << endl;
	free(reply);
	zmq_close(client);
	zmq_term(context);
	return 0;
}

// worker使用REQ套接字实现LRU算法
//
static DWORD worker_task(LPVOID arg)
{
	void *context = zmq_init(1);
	void *worker = zmq_socket(context, ZMQ_REQ);

	int id = (int)arg + 1;
	s_set_id(worker, id); // 设置可打印的标识
	int re = zmq_connect(worker, "tcp://127.0.0.1:7777");

	char identity[10];
	sprintf(identity, "%04X", (int)id);
	cout << "worker connect" << re << "--" << identity << endl;
	// 告诉代理worker已经准备好
	s_send(worker, "READY");

	cout << "worker ready" << re << endl;
	while (1) {
		// 将消息中空帧之前的所有内容（信封）保存起来，
		// 本例中空帧之前只有一帧，但可以有更多。
		char *address = s_recv(worker);      //1、获取信封地址
		char *empty = s_recv(worker);        //2、获取 空帧
		assert(*empty == 0);
		free(empty);

		// 获取请求，并发送回应
		char *request = s_recv(worker);      //3、获取 信息
		cout << "``````````````````Worker:"<< identity << ",Client:" << address << ",Request:" << request << endl;
		free(request);
		if (strcmp(address, "Bye") == 0)
		{
			free(address);
			break;
		}
		//封装消息
		s_sendmore(worker, address);      //地址信封
		s_sendmore(worker, "");              //空帧
		s_send(worker, "OK");                //真实消息
		free(address);
	}
	zmq_close(worker);
	zmq_term(context);
	return 0;
}
int client_nbr;
int worker_nbr;


DWORD route(LPVOID arg)
{
	auto name = static_cast<char*>(arg);

	// 准备0MQ上下文和套接字
	void *context = zmq_init(1);
	void *frontend = zmq_socket(context, ZMQ_ROUTER);
	void *backend = zmq_socket(context, ZMQ_ROUTER);
	zmq_bind(frontend, "tcp://127.0.0.1:6666");
	zmq_bind(backend, "tcp://127.0.0.1:7777");

	// LRU逻辑
	// - 一直从backend中获取消息；当有超过一个worker空闲时才从frontend获取消息。
	// - 当woker回应时，会将该worker标记为已准备好，并转发woker的回应给client
	// - 如果client发送了请求，就将该请求转发给下一个worker

	zmq_pollitem_t items[] = {
		{ backend, 0, ZMQ_POLLIN, 0 },
		{ frontend, 0, ZMQ_POLLIN, 0 }
	};
	// 存放可用worker的队列
	int available_workers = 0;
	char *worker_queue[10];
	cout << endl << "route ready" << endl;
	while (1) {
		zmq_poll(items, available_workers ? 2 : 1, -1);

		cout << "route poll";
		// 处理backend中worker的队列
		if (items[0].revents & ZMQ_POLLIN) {
			// 将worker的地址入队
			char *worker_addr = s_recv(backend);
			assert(available_workers < NBR_WORKERS);

			// 跳过空帧
			char *empty = s_recv(backend);
			assert(empty[0] == 0);
			free(empty);

			// 第三帧是“READY”或是一个client的地址
			char *client_addr = s_recv(backend);
			// 如果是一个应答消息，则转发给client
			if (strcmp(client_addr, "READY") == 0)
			{
				worker_queue[available_workers++] = worker_addr;
			}
			else if (strcmp(client_addr, "BYE") == 0)
			{
				worker_queue[available_workers++] = worker_addr;
			}
			else
			{
				empty = s_recv(backend);
				assert(empty[0] == 0);
				free(empty);
				char *reply = s_recv(backend);
				s_sendmore(frontend, client_addr);
				s_sendmore(frontend, "");
				s_send(frontend, reply);      
				cout << "******Result:" << client_addr<<",Res:"<< reply << endl;
				free(reply);
				if (--client_nbr == 0)
					break; // 处理N条消息后退出
			}
			cout << endl;
			free(client_addr);
		}

		if (items[1].revents & ZMQ_POLLIN) {
			// 获取下一个client的请求，交给空闲的worker处理
			// client请求的消息格式是：[client地址][空帧][请求内容]
			char *client_addr = s_recv(frontend);
			cout << "*2*" << client_addr << endl;
			char *empty = s_recv(frontend);
			assert(empty[0] == 0);
			free(empty);
			char *request = s_recv(frontend);

			//下面会是一个消息
			s_sendmore(backend, worker_queue[0]);
			s_sendmore(backend, "");
			s_sendmore(backend, client_addr);
			s_sendmore(backend, "");
			s_send(backend, request);      
			cout << "******Request:" << client_addr << ",Arg:" << request << endl;

			free(client_addr);
			free(request);

			// 将该worker的地址出队
			free(worker_queue[0]);
			DEQUEUE(worker_queue);
			available_workers--;
		}
	}
	zmq_close(frontend);
	zmq_close(backend);
	zmq_term(context);
	cout << "route end" << endl;
	return 0;
}
int main(void)
{
	start_thread(route, LPVOID(NULL));
	thread_sleep(100);
	for (client_nbr = 0; client_nbr < NBR_CLIENTS; client_nbr++) {
		start_thread(client_task, LPVOID(client_nbr));
	}
	for (worker_nbr = 0; worker_nbr < NBR_WORKERS; worker_nbr++) {
		start_thread(worker_task, LPVOID(worker_nbr));
	}
	getchar();
	return 0;
}
#endif
