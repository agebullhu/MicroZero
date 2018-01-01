#include "stdafx.h"
#include "ApiStation.h"
namespace agebull
{
	namespace zmq_net
	{
		ApiStation::ApiStation(string name)
			: RouteStation<ApiStation, string, STATION_TYPE_API>(name)
			, _nowWorkIndex(0)
		{
		}

		/**
		 *取下一个可用的工作者名称
		 *
		 LRU逻辑
		 -一直从_innerSocket中获取消息；当有超过一个worker空闲时才从_outSocket获取消息。
		 - 当woker回应时，会将该worker标记为已准备好，并转发woker的回应给client
		 - 如果client发送了请求，就将该请求转发给下一个worker
		 */
		char* ApiStation::getNextWorker()
		{
			if (_workers.size() == 0)
				return nullptr;
			if (_workers.size() == 1)
				return _strdup(_hosts[0].c_str());
			while (_hosts.size() == 0)
			{
				thread_sleep(1);
			}
			auto host = _hosts.back();
			_hosts.pop_back();
			return _strdup(host.c_str());
		}
		/**
		 * 当远程调用进入时的处理
		 */
		void ApiStation::onCallerPollIn()
		{
			// 获取下一个client的请求，交给空闲的worker处理
			// client请求的消息格式是：[client地址][空帧][请求内容]
			char* client_addr = s_recv(_outSocket, 0);
			recv_empty(_outSocket);
			char* request = s_recv(_outSocket);

			while (true)
			{
				char* work = getNextWorker();
				if (snedToWorker(work, client_addr, request))
					break;
				left(work);
			}
			free(client_addr);
			free(request);
		}
		bool ApiStation::snedToWorker(char* work, char* client_addr, char* request)
		{
			//路由到其中一个工作对象
			if (work == nullptr)
			{
				s_sendmore(_outSocket, client_addr);
				s_sendmore(_outSocket, "");
				s_send(_outSocket, "NoWorker");
				return true;
			}
			else
			{
				int state = s_sendmore(_innerSocket, work);
				if (state < 0)
					return false;
				state = s_sendmore(_innerSocket, "");
				if (state < 0)
					return false;
				state = s_sendmore(_innerSocket, client_addr);
				if (state < 0)
					return false;
				state = s_sendmore(_innerSocket, "");
				if (state < 0)
					return false;
				state = s_send(_innerSocket, request);//真实发送
				return state > 0;
			}
		}
		/**
		 * 当工作操作返回时的处理
		 */
		void ApiStation::onWorkerPollIn()
		{
			// 将worker的地址入队
			char* worker_addr = s_recv(_innerSocket);
			recv_empty(_innerSocket);
			char* client_addr = s_recv(_innerSocket);
			recv_empty(_innerSocket);
			char* reply = s_recv(_innerSocket);
			// 如果是一个应答消息，则转发给client
			if (strcmp(client_addr, "READY") == 0)
			{
				_hosts.push_back(worker_addr);
				join(worker_addr, reply, true);
			}
			else
			{
				s_sendmore(_outSocket, client_addr);
				s_sendmore(_outSocket, "");
				s_send(_outSocket, reply);//真实发送
				_hosts.insert(_hosts.begin(), worker_addr);
			}
			free(worker_addr);
			free(client_addr);
			free(reply);
		}

		/**
		* 当工作操作返回时的处理
		*/
		void ApiStation::left(char* name)
		{
			auto iter = _hosts.begin();
			while (iter != _hosts.end())
			{
				if (*iter != name)
				{
					++iter;
					continue;
				}
				_hosts.erase(iter);
				break;
			}
			__super::left(name);
		}
	}
}