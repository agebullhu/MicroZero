/**
 * ZMQ广播代理类
 *
 *
 */

#include "stdafx.h"
#include "VoteStation.h"
#include <zeromq/zhelpers.h>
namespace agebull
{
	namespace zmq_net
	{
		VoteStation::VoteStation(string name)
			: RouteStation<VoteStation, Voter, STATION_TYPE_VOTE>(name)
		{
		}

		bool  VoteStation::get_voters(const char* client_addr, const  char* request_token)
		{
			acl::string strs("[");
			bool first = true;
			for (auto iter : _workers)
			{
				if (!first)
				{
					strs.append("\"");
					first = false;
				}
				else
					strs.append(",\"");
				strs.append(iter.second.flow_name.c_str());
				strs.append("\"");
			}
			strs.append("]");
			return send_state(client_addr, request_token, "*", strs.c_str());
		}
		bool  VoteStation::send_state(const char* client_addr, const  char* request_token, const  char* voter, const  char* state)
		{
			_zmq_state = s_sendmore(_outSocket, client_addr);
			if (_zmq_state < 0)
				return false;
			_zmq_state = s_sendmore(_outSocket, "");
			if (_zmq_state < 0)
				return false;
			_zmq_state = s_sendmore(_outSocket, request_token);
			if (_zmq_state < 0)
				return false;
			_zmq_state = s_sendmore(_outSocket, "");
			if (_zmq_state < 0)
				return false;
			_zmq_state = s_sendmore(_outSocket, voter);
			if (_zmq_state < 0)
				return false;
			_zmq_state = s_sendmore(_outSocket, "");
			if (_zmq_state < 0)
				return false;
			_zmq_state = s_send(_outSocket, state);//真实发送
			return _zmq_state > 0;
		}
		/**
		* 当远程调用进入时的处理
		*/
		void VoteStation::onCallerPollIn()
		{
			// 获取下一个client的请求，交给空闲的worker处理
			// client请求的消息格式是：[client地址][空帧][请求内容]
			char* client_addr = s_recv(_outSocket, 0);
			recv_empty(_outSocket);
			char* request_token = s_recv(_outSocket);
			recv_empty(_outSocket);
			char* request_state = s_recv(_outSocket);
			recv_empty(_outSocket);
			char* request_argument = s_recv(_outSocket);
			RedisLiveScope redis_live_scope;
			if (strcmp(request_state, "voters"))//请求投票者名单
			{
				TransRedis::get_context()->set(request_token, "*");//3600秒过期
				TransRedis::get_context()->pexpire(request_token, 3600000);//3600秒过期
				get_voters(client_addr, request_argument);
			}
			else if (strcmp(request_state, "start"))//开始投票
			{
				TransRedis::get_context()->set(request_token, "#");//3600秒过期
				start_vote(client_addr, request_token, request_argument);
			}
			else if (strcmp(request_state, "continue"))//继续等待投票结束
			{
				TransRedis::get_context()->pexpire(request_token, 3600000);//3600秒过期
			}
			else if (strcmp(request_state, "end") || strcmp(request_state, "bye"))//结束投票
			{
				delete_from_redis(request_token);
				send_state(client_addr, request_token, "*", "bye");
			}
			else
			{
				send_state(client_addr, request_token, "*", "fuck");
			}

			free(client_addr);
			free(request_token);
			free(request_argument);
		}

		bool VoteStation::start_vote(const char* client_addr, const  char* request_token, const  char* request_argument)
		{
			//路由到所有工作对象
			for (auto voter : _workers)
			{
				_zmq_state = s_sendmore(_innerSocket, voter.second.net_name.c_str());
				if (_zmq_state < 0)
					return false;
				_zmq_state = s_sendmore(_innerSocket, "");
				if (_zmq_state < 0)
					return false;
				_zmq_state = s_sendmore(_innerSocket, client_addr);
				if (_zmq_state < 0)
					return false;
				_zmq_state = s_sendmore(_innerSocket, "");
				if (_zmq_state < 0)
					return false;
				_zmq_state = s_send(_innerSocket, request_token);//真实发送
				if (_zmq_state < 0)
					return false;
				_zmq_state = s_sendmore(_innerSocket, "");
				if (_zmq_state < 0)
					return false;
				_zmq_state = s_send(_innerSocket, request_argument);//真实发送
				if (_zmq_state < 0)
					return false;
			}
			return true;
		}
		/**
		* 当工作操作返回时的处理
		*/
		void VoteStation::onWorkerPollIn()
		{
			// 将worker的地址入队
			char* worker_addr = s_recv(_innerSocket);
			recv_empty(_innerSocket);
			char* client_addr = s_recv(_innerSocket);
			recv_empty(_innerSocket);
			char* request_token = s_recv(_outSocket);
			recv_empty(_outSocket);
			char* reply = s_recv(_innerSocket);
			// 如果是一个应答消息，则转发给client
			if (strcmp(request_token, "READY") == 0)
			{
				join(worker_addr, reply, true);
			}
			else
			{
				RedisLiveScope redis_live_scope;
				if (TransRedis::get_context()->exists(request_token))//如果投票还有效，返回结投票者
				{
					auto vote = _workers[worker_addr];
					acl::string ver("|");
					ver.append(vote.flow_name.c_str());
					TransRedis::get_context()->append(request_token, ver.c_str());
					send_state(client_addr, request_token, vote.flow_name.c_str(), reply);
				}
			}
			free(worker_addr);
			free(client_addr);
			free(request_token);
			free(reply);
		}
	}
}