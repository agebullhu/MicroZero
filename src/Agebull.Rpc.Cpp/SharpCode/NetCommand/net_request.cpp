#include "stdinc.h"
#include "net_request.h"
#include "net_command.h"


using namespace std;
namespace Agebull
{
	namespace Rpc
	{
		/**
		* @brief 消息泵
		* @param {PNetCommand} cmd 命令对象
		*/
		PNetCommand CommandPump::message_pump(NetCommandArgPtr& cmd_arg)
		{
			cmd_arg->cmd_state = NET_COMMAND_STATE_SUCCEED;
			return cmd_arg.m_command;
		}
		void CommandPump::request_pump()
		{
			log_msg1("服务端命令泵(%s)正在启动", m_address);
			ZMQ_HANDLE socket = zmq_socket(get_zmq_context(), ZMQ_REP);
			if (socket == nullptr)
			{
				auto err = zmq_strerror(errno);
				log_error2("构造SOCKET对象(%s)发生错误:%s", m_address, err);
				return;
			}

			int iZMQ_IMMEDIATE = 1;//列消息只作用于已完成的链接
			zmq_setsockopt(socket, ZMQ_LINGER, &iZMQ_IMMEDIATE, sizeof(int));
			int iLINGER = 50;//关闭设置停留时间,毫秒
			zmq_setsockopt(socket, ZMQ_LINGER, &iLINGER, sizeof(int));
			int iRcvTimeout = 500;
			zmq_setsockopt(socket, ZMQ_RCVTIMEO, &iRcvTimeout, sizeof(int));
			int iSndTimeout = 500;
			zmq_setsockopt(socket, ZMQ_SNDTIMEO, &iSndTimeout, sizeof(int));
			auto zmq_result = zmq_bind(socket, m_address);
			if (zmq_result < 0)
			{
				auto err = zmq_strerror(errno);
				log_error2("绑定端口(%s)发生错误:%s", m_address, err);
				return;
			}
			log_msg1("服务端命令泵(%s)已启动", m_address);
			//登记线程开始
			int state = 0;
			set_command_thread_start();
			RedisLiveScope redis_live_scope;
			while (get_net_state() == NET_STATE_RUNING)
			{
				//接收命令请求
				zmq_msg_t msg_call;
				zmq_result = zmq_msg_init(&msg_call);
				if (zmq_result != 0)
				{
					state = 2;//出错了
					break;
				}
				zmq_result = zmq_msg_recv(&msg_call, socket, 0);
				if (zmq_result <= 0)
				{
					switch (errno)
					{
					case ETERM:
						log_error1("命令接收(%s)错误[与指定的socket相关联的context被终结了],自动关闭", m_address);
						state = 1;
						break;
					case ENOTSOCK:
						log_error1("命令接收(%s)错误[指定的socket不可用],自动重启", m_address);
						state = 2;
						break;
					case EINTR:
						log_error1("命令接收(%s)错误[在接接收到消息之前，这个操作被系统信号中断了],自动重启", m_address);
						state = 2;
						break;
					case EAGAIN:
						//使用非阻塞方式接收消息的时候没有接收到任何消息。
					default:
						state = 0;
						break;
					}
					zmq_msg_close(&msg_call);
					if (state > 0)
						break;
					continue;
				}
				size_t len = zmq_msg_size(&msg_call);
				log_debug2(DEBUG_REQUEST, 6, "接收请求(%s)成功:%d", m_address, len);
				NetCommandArgPtr ptr(zmq_result);
				memcpy(ptr.m_buffer, static_cast<PNetCommand>(zmq_msg_data(&msg_call)), len);
				zmq_msg_close(&msg_call);

				if (!check_crc(ptr.m_command))
				{
					log_error1("接收到非法请求(%s),正在通知重发", m_address);
					ptr->cmd_state = NET_COMMAND_STATE_ARGUMENT_INVALID;
					write_crc(ptr.m_command);
					zmq_result = zmq_send(socket, ptr.m_buffer, len, ZMQ_DONTWAIT);
				}
				else
				{
					log_debug4(DEBUG_REQUEST, 6, "(%s)接收到请求,地址%s,命令%d(%s)", m_address, ptr->user_token, ptr->cmd_id, ptr->cmd_identity);
					//事务处理
					PNetCommand result = message_pump(ptr);
					//服务器回发
					get_cmd_len(result);
					write_crc(result);
					//zmq_send(socket, cmd_call->user_token, strlen(cmd_call->user_token), ZMQ_SNDMORE);
					//zmq_send(socket, empty, 0, ZMQ_SNDMORE);
					zmq_result = zmq_send(socket, ptr.m_buffer, len, ZMQ_DONTWAIT);
				}

				if (zmq_result <= 0)
				{
					switch (errno)
					{
					case ETERM:
						log_error1("发送回执(%s)错误[与指定的socket相关联的context被终结了],自动关闭", m_address);
						state = 1;
						break;
					case ENOTSOCK:
						log_error1("发送回执(%s)错误[指定的socket不可用],自动重启", m_address);
						state = 2;
						break;
					case EINTR:
						log_error1("发送回执(%s)错误[在接接收到消息之前，这个操作被系统信号中断了],自动重启", m_address);
						state = 2;
						break;
					case EAGAIN:
						//使用非阻塞方式接收消息的时候没有接收到任何消息。
					default:
						state = 0;
						continue;
					}
					break;
				}
				log_debug1(DEBUG_REQUEST, 6, "发送回执(%s)成功", m_address);
			}
			zmq_unbind(socket, m_address);
			zmq_close(socket);
			//登记线程关闭
			set_command_thread_end();
			if (state == 2 && get_net_state() == NET_STATE_RUNING)
			{
				log_msg("服务端命令泵正在重启");
				boost::thread thrds(boost::bind(&server_request));
			}
			else
			{
				log_msg1("服务端命令泵(%s)已关闭", m_address);
			}
		}
	}
}
