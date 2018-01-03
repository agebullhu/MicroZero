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
			: BalanceStation<VoteStation, Voter, STATION_TYPE_VOTE>(name)
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
		/**
		* 当远程调用进入时的处理
		*/
		void VoteStation::request()
		{
			char* client_addr = s_recv(_out_socket, 0);
			recv_empty(_out_socket);
			char* request_token = s_recv(_out_socket);
			recv_empty(_out_socket);
			char* request_state = s_recv(_out_socket);
			recv_empty(_out_socket);
			char* request_argument = s_recv(_out_socket);
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

		/**
		* @brief 开始执行一条命令
		*/
		void VoteStation::command_start(const char* caller, vector< string> lines)
		{
			send_state(caller, lines[0].c_str(), lines[1].c_str(), lines[2].c_str());
		}
		/**
		* @brief 结束执行一条命令
		*/
		void VoteStation::command_end(const char* caller, vector<string> lines)
		{
			start_vote(caller, lines[0].c_str(), lines[1].c_str());
		}
		bool VoteStation::start_vote(const char* client_addr, const  char* request_token, const  char* request_argument)
		{
			//路由到所有工作对象
			for (auto voter : _workers)
			{
				_zmq_state = s_sendmore(_inner_socket, voter.second.net_name.c_str());
				if (_zmq_state < 0)
					return false;
				_zmq_state = s_sendmore(_inner_socket, "");
				if (_zmq_state < 0)
					return false;
				_zmq_state = s_sendmore(_inner_socket, client_addr);
				if (_zmq_state < 0)
					return false;
				_zmq_state = s_sendmore(_inner_socket, "");
				if (_zmq_state < 0)
					return false;
				_zmq_state = s_send(_inner_socket, request_token);//真实发送
				if (_zmq_state < 0)
					return false;
				_zmq_state = s_sendmore(_inner_socket, "");
				if (_zmq_state < 0)
					return false;
				_zmq_state = s_send(_inner_socket, request_argument);//真实发送
				if (_zmq_state < 0)
					return false;
			}
			return true;
		}

		bool  VoteStation::send_state(const char* client_addr, const  char* request_token, const  char* voter, const  char* state)
		{
			_zmq_state = s_sendmore(_out_socket, client_addr);
			if (_zmq_state < 0)
				return false;
			_zmq_state = s_sendmore(_out_socket, "");
			if (_zmq_state < 0)
				return false;
			_zmq_state = s_sendmore(_out_socket, request_token);
			if (_zmq_state < 0)
				return false;
			_zmq_state = s_sendmore(_out_socket, "");
			if (_zmq_state < 0)
				return false;
			_zmq_state = s_sendmore(_out_socket, voter);
			if (_zmq_state < 0)
				return false;
			_zmq_state = s_sendmore(_out_socket, "");
			if (_zmq_state < 0)
				return false;
			_zmq_state = s_send(_out_socket, state);//真实发送
			return _zmq_state > 0;
		}
		/**
		* 当工作操作返回时的处理
		*/
		void VoteStation::response()
		{
			// 将worker的地址入队
			char* worker_addr = s_recv(_inner_socket);
			recv_empty(_inner_socket);
			char* client_addr = s_recv(_inner_socket);
			recv_empty(_inner_socket);
			char* request_token = s_recv(_out_socket);
			recv_empty(_out_socket);
			char* reply = s_recv(_inner_socket);
			// 如果是一个应答消息，则转发给client
			if (strcmp(request_token, "READY") == 0)
			{
				worker_join(worker_addr, reply, true);
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