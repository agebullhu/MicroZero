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


		bool VoteStation::get_voters(const char* client_addr, const char* request_token)
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
		* @brief 执行一条命令
		*/
		sharp_char VoteStation::command(const char* caller, vector<string> lines)
		{
			if (send_state(caller, lines[0].c_str(), lines[1].c_str(), lines[2].c_str()))
				return sharp_char("vote start");
			return sharp_char("bad");
		}

		/**
		* @brief 当远程调用进入时的处理
		*/
		void VoteStation::request(ZMQ_HANDLE socket)
		{
			vector<sharp_char> list;
			//0 路由到的地址 1 空帧 2 请求标识 3 命令 4 参数
			_zmq_state = recv(_out_socket, list);

			RedisLiveScope redis_live_scope;
			switch (list[3][0])
			{
			case '*': //请求投票者名单
				get_voters(*list[0], *list[2]);
				break;
			case '@': //开始投票
				TransRedis::get_context()->set(*list[2], "start");
				TransRedis::get_context()->pexpire(*list[2], 3600000);//3600秒过期
				if (start_vote(*list[0], *list[3], *list[4]))
					send_state(*list[0], *list[2], "*", "start");
				else
					send_state(*list[0], *list[2], "*", "error");
				break;
			case '%': //继续等待投票结束
				TransRedis::get_context()->set(*list[2], "waiting");
				TransRedis::get_context()->pexpire(*list[2], 3600000);//3600秒过期
				send_state(*list[0], *list[2], "*", "waiting");
				break;
			case 'v': //结束投票
				TransRedis::get_context()->set(*list[2], "end");
				send_state(*list[0], *list[2], "*", "end");
				break;
			case 'x': //结束投票
				TransRedis::get_context()->set(*list[2], "close");
				send_state(*list[0], *list[2], "*", "close");
				break;
			default:
				send_state(*list[0], *list[2], "*", "error");
				break;
			}
		}

		bool VoteStation::start_vote(const char* client_addr, const char* request_token, const char* request_argument)
		{
			//路由到所有工作对象
			for (auto voter : _workers)
			{
				_zmq_state = send_addr(_inner_socket, voter.second.net_name.c_str());
				_zmq_state = send_more(_inner_socket, client_addr);
				_zmq_state = send_more(_inner_socket, request_token);
				_zmq_state = send_late(_inner_socket, request_argument);
			}
			return _zmq_state == ZmqSocketState::Succeed;
		}

		bool VoteStation::send_state(const char* client_addr, const char* request_token, const char* voter, const char* state)
		{
			_zmq_state = send_addr(_out_socket, client_addr);
			_zmq_state = send_more(_out_socket, request_token);
			_zmq_state = send_more(_out_socket, voter);
			_zmq_state = send_late(_out_socket, state);
			return _zmq_state == ZmqSocketState::Succeed;
		}

		/**
		* @brief 当工作操作返回时的处理
		*/
		void VoteStation::response()
		{
			vector<sharp_char> list;
			//0 路由到的地址 1 空帧 2 原始者请求地址 3 请求标识 4 结果
			_zmq_state = recv(_inner_socket, list);
			if (_zmq_state == ZmqSocketState::TimedOut)
			{
				return;
			}
			if (_zmq_state != ZmqSocketState::Succeed)
			{
				log_error3("接收结果失败%s(%d)%s", _station_name.c_str(), _inner_port, state_str(_zmq_state));
				return;
			}

			switch (list[2][0])
			{
			case '@'://加入
				worker_join(*list[0], *list[3], true);
				send_addr(_inner_socket, *list[0]);
				send_late(_inner_socket, "wecome");
				return;
			case '*'://开始工作
				return;
			default: break;
			}
			RedisLiveScope redis_live_scope;
			if (TransRedis::get_context()->exists(*list[3]))//如果投票还有效，返回结投票者
			{
				auto vote = _workers[*list[0]];
				acl::string ver("|");
				ver.append(vote.flow_name.c_str());
				TransRedis::get_context()->append(*list[3], ver.c_str());
				send_state(*list[0], *list[3], vote.flow_name.c_str(), *list[4]);
			}
		}
	}
}
