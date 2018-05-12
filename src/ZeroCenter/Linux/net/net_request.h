#ifndef _AGEBULL_NET_COMMAND_REQUEST_H
#define _AGEBULL_NET_COMMAND_REQUEST_H
#pragma once
#include "stdinc.h"
#include "redis/redis.h"

#ifdef WIN32
#include <objbase.h>  
#else
#include <uuid/uuid.h>  
typedef uuid_t GUID;
#endif

#include "command_def.h"
#include <tson/tson_serializer.h>
#include <Tson/tson_deserializer.h>
#include "InnerTrade/MqCommand.h"

using namespace std;


namespace agebull
{
	namespace Rpc
	{
		class CommandPump
		{
			//命令对象状态
			TRADECOMMAND_STATUS m_state;
			//最后交易状态
			TRADECOMMAND_STATUS m_last_trade_state;
		public:
			/**
			* @brief 构造
			*/
			CommandPump()
				: m_state(TRADECOMMAND_STATUS_NEED_INITIALIZE)
				, m_last_trade_state(0)
			{
			}

			/**
			* @brief 析构
			*/
			virtual ~CommandPump()
			{
			}
			/**
			* @brief 关闭
			*/
			void virtual close() const
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
			bool virtual Initialize()
			{
				m_state = TRADECOMMAND_STATUS_SUCCEED;

				return true;
			}

			/*
			* 绑定地址
			*/
			void set_address(const char* addr)
			{
				strcpy_s(m_address, addr);
			}
			/**
			* @brief 消息泵
			* @param {PNetCommand} cmd 命令对象
			*/
			PNetCommand virtual message_pump(NetCommandArgPtr& cmd_arg);

			/**
			* @brief 请求泵
			*/
			void request_pump();
		private:
			//返回消息队列
			queue<NetCommandArgPtr> m_queue;
			//返回消息队列锁
			boost::mutex m_lock;
			/*
			 * 绑定地址
			 */
			char m_address[128];
		};
	}
}

#endif