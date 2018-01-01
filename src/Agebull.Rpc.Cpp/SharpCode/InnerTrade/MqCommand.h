#pragma once
#include "stdafx.h"
#include "redis/entity_redis.h"
//命令对象状态
typedef int TRADECOMMAND_STATUS;
//未初始化
const TRADECOMMAND_STATUS TRADECOMMAND_STATUS_NEED_INITIALIZE = -1;
//正常
const TRADECOMMAND_STATUS TRADECOMMAND_STATUS_SUCCEED = 0;
//服务器无法打开
const TRADECOMMAND_STATUS TRADECOMMAND_STATUS_ES_SERVER_OPEN_FAILURE = 1;
//服务器登录失败
const TRADECOMMAND_STATUS TRADECOMMAND_STATUS_ES_LOGIN_FAILURE = 2;

namespace agebull
{
	namespace Rpc
	{

		class MqCommand
		{
			//命令对象状态
			TRADECOMMAND_STATUS m_state;
			//最后交易状态
			TRADECOMMAND_STATUS m_last_trade_state;
		public:
			/**
			* @brief 构造
			*/
			MqCommand()
				: m_state(TRADECOMMAND_STATUS_NEED_INITIALIZE)
				, m_last_trade_state(0)
			{
			}

			/**
			* @brief 析构
			*/
			~MqCommand()
			{
				free();
			}
			/**
			* @brief 关闭
			*/
			void close() const
			{
			}
			/**
			* @brief 清理
			*/
			void free() const
			{
			}
			/**
			* @brief 是否正常
			*/
			bool succeed() const
			{
				return m_state == TRADECOMMAND_STATUS_SUCCEED;
			}


			/**
			* @brief 初始化
			* @return 是否成功
			*/
			bool Initialize()
			{
				m_state = TRADECOMMAND_STATUS_SUCCEED;

				return true;
			}
			/**
			* @brief 消息泵
			* @param {PNetCommand} cmd 命令对象
			*/
			void message_pump(NetCommandArgPtr& cmd_arg)
			{

				switch (cmd_arg->cmd_id)
				{
				case NET_COMMAND_SYSTEM_NOTIFY:
					OnSystemEvent(cmd_arg);
					return;
				case NET_COMMAND_BUSINESS_NOTIFY:
					OnBusinessEvent(cmd_arg);
					return;
				case NET_COMMAND_CALL:
					OnCall(cmd_arg);
					return;
				case NET_COMMAND_RESULT:
					OnResult(cmd_arg);
					return;
				case NET_COMMAND_FLOW_START:
					OnResult(cmd_arg);
					return;
				case NET_COMMAND_FLOW_END:
					OnResult(cmd_arg);
					return;
				case NET_COMMAND_FLOW_STEP_SUCCEESS:
					OnResult(cmd_arg);
					return;
				case NET_COMMAND_FLOW_STEP_FAILED:
					OnResult(cmd_arg);
					return;
				case NET_COMMAND_FLOW_STTATE:
					OnResult(cmd_arg);
					return;
				}
			}

		private:
			/**
			* @brief 系统事件已接收
			* @param {NetCommandArgPtr&} cmd_arg 命令对象
			* @return 无
			*/
			void OnSystemEvent(NetCommandArgPtr& cmd_arg)
			{
				//save_cmd_arg(cmd_arg.m_command);
				notify_message(cmd_arg);
			}
			/**
			* @brief 业务事件已发出
			* @param {NetCommandArgPtr&} cmd_arg 命令对象
			* @return 无
			*/
			void OnBusinessEvent(NetCommandArgPtr& cmd_arg)
			{
				//save_cmd_arg(cmd_arg.m_command);
				notify_message(cmd_arg);
			}
			/**
			* @brief 命令请求
			* @param {NetCommandArgPtr&} cmd_arg 命令对象
			* @return 无
			*/
			void OnCall(NetCommandArgPtr& cmd_arg)
			{
				save_cmd_arg(cmd_arg.m_command);
				notify_message(cmd_arg);
			}
			/**
			* @brief 命令请求
			* @param {NetCommandArgPtr&} cmd_arg 命令对象
			* @return 无
			*/
			void OnResult(NetCommandArgPtr& cmd_arg)
			{
				delete_cmd_arg(cmd_arg.m_command);
				cmd_arg->cmd_state = NET_COMMAND_STATE_SUCCEED;
				command_answer(cmd_arg);
			}
			/**
			* @brief 命令请求
			* @param {NetCommandArgPtr&} cmd_arg 命令对象
			* @return 无
			*/
			void OnFlow(NetCommandArgPtr& cmd_arg)
			{
				delete_cmd_arg(cmd_arg.m_command);
				command_answer(cmd_arg);
			}
		};
	}
}
