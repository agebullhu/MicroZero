#include "MqCommand.h"
#include "Business/DataNotifyBusiness.h"

namespace Agebull
{
	namespace Futures
	{
		namespace TradeManagement
		{
			/**
			* @brief 构造
			*/
			MqCommand::MqCommand()
				: m_state(TRADECOMMAND_STATUS_NEED_INITIALIZE)
				, m_last_trade_state(0)
			{
			}

			/**
			* @brief 析构
			*/
			MqCommand::~MqCommand()
			{
				free();
			}


			/**
			* @brief 初始化
			* @return 是否成功
			*/
			bool MqCommand::Initialize()
			{
				m_state = TRADECOMMAND_STATUS_SUCCEED;
				return true;
			}

			/**
			* @brief 消息泵
			* @param {PNetCommand} cmd 命令对象
			*/
			void MqCommand::message_pump(NetCommandArgPtr& cmd_arg)
			{
				switch (cmd_arg->cmd_id)
				{
				case NET_COMMAND_USER_SET_PASSWORD://修改客户密码
					UserSetPassword(cmd_arg);
					return;
				case NET_COMMAND_USER_LOGIN://登录
					UserLogin(cmd_arg);
					return;
				case NET_COMMAND_USER_CHANGE_COMMODITY://改变当前品种
					ChangedCommodity(cmd_arg);
					return;
				case NET_COMMAND_USER_LOGOUT://登出
					UserLogout(cmd_arg);
					return;
				case NET_COMMAND_ES_QRYMONEY://查询客户资金
					DoQryMoney(cmd_arg);
					return;
				case NET_COMMAND_ES_QRYORDER://查询客户委托
					DoQryOrder(cmd_arg);
					return;
				case NET_COMMAND_ES_QRYMATCH://查询客户成交
					DoQryMatch(cmd_arg);
					return;
				case NET_COMMAND_ES_QRYHOLD://查询持仓
					DoQryHold(cmd_arg);
					return;
				case NET_COMMAND_ES_QRYEXCHANGESTATE://查询交易所状态
					DoQryExchangeState(cmd_arg);
					return;
				case NET_COMMAND_ES_QRYCOMMODITY://查询交易商品
					DoQryCommodity(cmd_arg);
					return;
				case NET_COMMAND_ES_QRYCONTRACT://查询合约
					DoQryContract(cmd_arg);
					return;
				case NET_COMMAND_ES_ORDERINSERT://报单请求
					DoOrderInsert(cmd_arg);
					return;
				case NET_COMMAND_ES_ORDERMODIFY://改单请求
					DoOrderModify(cmd_arg);
					return;
				case NET_COMMAND_ES_ORDERDELETE://撤单请求
					DoOrderDelete(cmd_arg);
					return;
				case NET_COMMAND_ES_QRYHISTORDER://历史委托查询
					DoQryHistOrder(cmd_arg);
					return;
				case NET_COMMAND_ES_QRYHISTMATCH://历史成交查询
					DoQryHistMatch(cmd_arg);
					return;
				case NET_COMMAND_ES_QRYCASHOPER://出金入金查询
					DoQryCashOper(cmd_arg);
					return;
				case NET_COMMAND_ES_CASHCHECK://出金入金审核请求
					DoCashCheck(cmd_arg);
					return;
				case NET_COMMAND_ES_QRYCACHADJUST://资金调整查询
					DoQryCachAdjust(cmd_arg);
					return;
				case NET_COMMAND_ES_QRYHISTCASHOPER://历史出金入金查询
					DoQryHistCashOper(cmd_arg);
					return;
				case NET_COMMAND_ES_QRYHISTCACHADJUST://历史资金调整查询
					DoQryHistCachAdjust(cmd_arg);
					return;
				case NET_COMMAND_ES_HKMARKETORDEROPERATOR://下港交所做市商报单
					DoHKMarketOrderOperator(cmd_arg);
					return;
				case NET_COMMAND_ES_QRYCOUNTRENT://客户计算参数查询
					DoQryCountRent(cmd_arg);
					return;
				case NET_COMMAND_ES_QRQUOTE://查询历史行情
					DoQryCandleStick(cmd_arg);
					return;
				default:
					DataNotifyBusiness::Notify(cmd_arg.m_command);
					break;
				}
			}

		}
	}
}
