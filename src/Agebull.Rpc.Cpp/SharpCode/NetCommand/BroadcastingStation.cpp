/**
 * ZMQ广播代理类
 *
 *
 */

#include "stdafx.h"
#include "BroadcastingStation.h"
#include "SysCmdArg/StringArgument.h"
namespace agebull
{
	namespace zmq_net
	{
		map<string, shared_ptr<BroadcastingStation>> BroadcastingStation::examples;

#define send_result(code,type,msg,addr)\
	log_error3("%s(%s),正在%s",msg, addr,code);\
	response[0] = type;\
	result = zmq_send(_outSocket, response, 1, ZMQ_DONTWAIT);\
	if(result <= 0)\
	{\
		if (!check_zmq_error(_zmq_state, request_address, code))\
			break;\
		continue;\
	}\
	log_debug2(DEBUG_REQUEST, 6, "%(%s)成功",code, request_address);

		/**
		*消息泵
		*/
		bool BroadcastingStation::publish(string name, string type, string arg)
		{
			auto pub = examples.find(name);
			if (pub == examples.end() || !pub->second->can_do())
				return false;
			Rpc::Globals::StringArgument argument;
			strcpy_s(argument.Argument, arg.c_str());
			NetCommandArgPtr ptr(Rpc::Globals::SerializeToCommand(&argument));

			//交换令牌，因为广播方式不同
			strcpy(ptr->user_token, type.c_str());
			strcpy(ptr->cmd_identity, "$***$");
			//广播
			int result = zmq_send(pub->second->_innerSocket, ptr.m_buffer, ptr.m_len, ZMQ_DONTWAIT);
			if (result <= 0)
			{
				check_zmq_error(pub->second->_zmq_state, pub->second->_innerAddress.c_str(), "发出广播");
				return false;
			}
			log_debug1(DEBUG_REQUEST, 6, "发出广播(%s)成功", pub->second->_outAddress);
			return true;
		}


		bool BroadcastingStation::poll()
		{
			const char* publish_address = _innerAddress.c_str();
			const char* request_address = _outAddress.c_str();
			_outSocket = create_socket(ZMQ_REP, request_address);
			_innerSocket = create_socket(ZMQ_PUB, publish_address);
			//登记线程开始
			_zmq_state = 0;
			set_command_thread_start();
			_station_state = station_state::Run;
			log_msg3("%s(%s | %s)已启动", _station_name, request_address, publish_address);
			//RedisLiveScope redis_live_scope;
			while (can_do())
			{
				poll_check_pause();
				//接收命令请求
				zmq_msg_t msg_call;
				int result = zmq_msg_init(&msg_call);
				if (result != 0)
				{
					_zmq_state = 2;//出错了
					break;
				}
				result = zmq_msg_recv(&msg_call, _outSocket, 0);
				if (result <= 0)
				{
					zmq_msg_close(&msg_call);
					if (!check_zmq_error(_zmq_state, request_address, "命令接收"))
						break;
					continue;
				}
				size_t len = zmq_msg_size(&msg_call);
				log_debug2(DEBUG_REQUEST, 6, "接收请求(%s)成功:%d", request_address, len);
				NetCommandArgPtr ptr(result);
				memcpy(ptr.m_buffer, static_cast<PNetCommand>(zmq_msg_data(&msg_call)), len);
				zmq_msg_close(&msg_call);

				char response[1];
				if (!check_crc(ptr.m_command))//(ptr.m_buffer[0] != '$' || (len > sizeof(NetCommand) &&  ptr.m_buffer[len - 1] != OBJ_TYPE_END))
				{
					send_result("通知重发", '1', "接收到未法请求", request_address);
					continue;
				}
				log_debug4(DEBUG_REQUEST, 6, "(%s)接收到请求,用户%s,命令%d(%s)", request_address, ptr->user_token, ptr->cmd_id, ptr->cmd_identity);

				//交换令牌，因为广播方式不同
				char tmp[GUID_LEN];
				memcpy(tmp, ptr->cmd_identity, GUID_LEN);
				memcpy(ptr->cmd_identity, ptr->user_token, GUID_LEN);
				memcpy(ptr->user_token, tmp, GUID_LEN);
				//广播
				result = zmq_send(_innerSocket, ptr.m_buffer, len, ZMQ_DONTWAIT);
				if (result <= 0)
				{
					if (!check_zmq_error(_zmq_state, publish_address, "发出广播"))
					{
						send_result("广播失败", '2', "无法发出广播", request_address);
						break;
					}
					send_result("广播失败", '2', "无法发出广播", request_address);
					continue;
				}
				log_debug1(DEBUG_REQUEST, 6, "发出广播(%s)成功", request_address);
				send_result("广播成功", '0', "广播成功", request_address);
			}
			_station_state = station_state::Closing;
			zmq_unbind(_outSocket, request_address);
			zmq_close(_outSocket);
			_outSocket = nullptr;
			zmq_unbind(_innerSocket, publish_address);
			zmq_close(_innerSocket);
			_innerSocket = nullptr;
			//登记线程关闭
			set_command_thread_end();
			_station_state = station_state::Closed;
			return _zmq_state == 2 && get_net_state() == NET_STATE_RUNING;
		}
	}
}