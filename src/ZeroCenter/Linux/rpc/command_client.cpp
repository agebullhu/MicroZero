#include "stdafx.h"
#include "command_client.h"
#include <TradeCommon/TradeCommand.h>
#include <log/log.h>
#ifdef CLR
#pragma unmanaged
#endif

namespace Agebull
{
	namespace Futures
	{
		namespace Globals
		{
			namespace Client
			{
				/**
				* @brief 查询货币币种信息
				* @return 无
				*/
				COMMAND_STATE QryCurrency()
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						PNetCommand cmd_call = new NetCommand();
						cmd_call->data_len = 0;
						set_command_head(cmd_call, NET_COMMAND_ES_QRYCURRENCY);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 查询交易所状态
				* @return 无
				*/
				COMMAND_STATE QryExchangeState()
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						PNetCommand cmd_call = new NetCommand();
						cmd_call->data_len = 0;
						set_command_head(cmd_call, NET_COMMAND_ES_QRYEXCHANGESTATE);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 查询交易商品
				* @return 无
				*/
				COMMAND_STATE QryCommodity()
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						PNetCommand cmd_call = new NetCommand();
						cmd_call->data_len = 0;
						set_command_head(cmd_call, NET_COMMAND_ES_QRYCOMMODITY);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 查询合约
				* @return 无
				*/
				COMMAND_STATE QryContract(ContractQueryArg& arg)
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						auto cmd_call = SerializeToCommand(&arg);
						set_command_head(cmd_call, NET_COMMAND_ES_QRYCONTRACT);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
#ifndef WEB
				/**
				* @brief 修改当前品种
				* @return 无
				*/
				COMMAND_STATE ChangeCommodity(const char* commodity)
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;

					//参数检查
					if (commodity == nullptr || strlen(commodity) == 0)
					{
						log_msg1("修改当前品种:%s,因为品种为空而失败", commodity);
						return NET_COMMAND_STATE_ARGUMENT_INVALID;
					}
					log_msg1("修改当前品种:%s", commodity);
					StringArgument args;
					memset(&args, 0, sizeof(StringArgument));
					strcpy_s(args.Argument, commodity);


					PNetCommand net_cmd = SerializeToCommand(&args);
					set_command_head(net_cmd, NET_COMMAND_USER_CHANGE_COMMODITY);
					try
					{
						//TO DO:处理方法
						request_net_cmmmand(net_cmd);
					}
					catch (...)
					{
						log_msg1("修改当前品种:%s,发生异常", commodity);
						delete[] net_cmd;
					}
					return state;
				}
				/**
				* @brief 查询客户资金
				* @return 无
				*/
				COMMAND_STATE QryMoney()
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						PNetCommand cmd_call = new NetCommand();
						cmd_call->data_len = 0;
						set_command_head(cmd_call, NET_COMMAND_ES_QRYMONEY);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 查询客户委托
				* @return 无
				*/
				COMMAND_STATE QryOrder(OrderQueryArg& arg)
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						auto cmd_call = SerializeToCommand(&arg);
						set_command_head(cmd_call, NET_COMMAND_ES_QRYORDER);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 查询客户成交
				* @return 无
				*/
				COMMAND_STATE QryMatch(TMatchQueryArg& arg)
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						auto cmd_call = SerializeToCommand(&arg);
						set_command_head(cmd_call, NET_COMMAND_ES_QRYMATCH);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 查询持仓
				* @return 无
				*/
				COMMAND_STATE QryHold(HoldQueryArg& arg)
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						auto cmd_call = SerializeToCommand(&arg);
						set_command_head(cmd_call, NET_COMMAND_ES_QRYHOLD);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 撤单请求
				* @return 无
				*/
				COMMAND_STATE OrderDelete(int& arg)
				{
					log_msg1("撤单请求:%d", arg);
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						StringArgument str_arg;
						sprintf_s(str_arg.Argument, "%d", arg);
						auto cmd_call = SerializeToCommand(&str_arg);
						set_command_head(cmd_call, NET_COMMAND_ES_ORDERDELETE);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						log_msg1("撤单请求:%d,发生异常", arg);
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 报单请求
				* @return 无
				*/
				COMMAND_STATE OrderInsert(CustomerOrder& arg)
				{
					log_msg5("报单请求:%s%s%s%d手%s", arg.CommodityNo, arg.ContractNo, (to_log_text(arg.Direct).c_str()), arg.OrderVol, (arg.HoldId > 0 ? "平仓" : "开仓"));
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						auto cmd_call = SerializeToCommand(&arg);
						set_command_head(cmd_call, NET_COMMAND_ES_ORDERINSERT);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						log_msg5("报单请求:%s%s%s%d手%s,发生异常", arg.CommodityNo, arg.ContractNo, (to_log_text(arg.Direct).c_str()), arg.OrderVol, (arg.HoldId > 0 ? "平仓" : "开仓"));
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 改单请求
				* @return 无
				*/
				COMMAND_STATE OrderModify(OrderModifyArg& arg)
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						auto cmd_call = SerializeToCommand(&arg);
						set_command_head(cmd_call, NET_COMMAND_ES_ORDERMODIFY);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 历史委托查询
				* @return 无
				*/
				COMMAND_STATE QryHistOrder(HisOrderQueryArg& arg)
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						auto cmd_call = SerializeToCommand(&arg);
						set_command_head(cmd_call, NET_COMMAND_ES_QRYHISTORDER);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 历史成交查询
				* @return 无
				*/
				COMMAND_STATE QryHistMatch(HisMatchQueryArg& arg)
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						auto cmd_call = SerializeToCommand(&arg);
						set_command_head(cmd_call, NET_COMMAND_ES_QRYHISTMATCH);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 出金入金查询
				* @return 无
				*/
				COMMAND_STATE QryCashOper()
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						PNetCommand cmd_call = new NetCommand();
						cmd_call->data_len = 0;
						set_command_head(cmd_call, NET_COMMAND_ES_QRYCASHOPER);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 出金入金审核请求
				* @return 无
				*/
				COMMAND_STATE CashCheck(CashCheckArg& arg)
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						auto cmd_call = SerializeToCommand(&arg);
						set_command_head(cmd_call, NET_COMMAND_ES_CASHCHECK);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 资金调整查询
				* @return 无
				*/
				COMMAND_STATE QryCachAdjust()
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						PNetCommand cmd_call = new NetCommand();
						cmd_call->data_len = 0;
						set_command_head(cmd_call, NET_COMMAND_ES_QRYCACHADJUST);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 历史出金入金查询
				* @return 无
				*/
				COMMAND_STATE QryHistCashOper(HisCashOperQueryArg& arg)
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						auto cmd_call = SerializeToCommand(&arg);
						set_command_head(cmd_call, NET_COMMAND_ES_QRYHISTCASHOPER);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 历史资金调整查询
				* @return 无
				*/
				COMMAND_STATE QryHistCachAdjust(HisAdjustQueryArg& arg)
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						auto cmd_call = SerializeToCommand(&arg);
						set_command_head(cmd_call, NET_COMMAND_ES_QRYHISTCACHADJUST);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 下港交所做市商报单
				* @return 无
				*/
				COMMAND_STATE HKMarketOrderOperator(HKMarketOrderArg& arg)
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						auto cmd_call = SerializeToCommand(&arg);
						set_command_head(cmd_call, NET_COMMAND_ES_HKMARKETORDEROPERATOR);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 客户计算参数查询
				* @return 无
				*/
				COMMAND_STATE QryCountRent(TClientCountRentQueryArg& arg)
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						auto cmd_call = SerializeToCommand(&arg);
						set_command_head(cmd_call, NET_COMMAND_ES_QRYCOUNTRENT);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
				/**
				* @brief 查询K线行情
				* @return 无
				*/
				COMMAND_STATE QryCandleStick(StringArgument& arg)
				{
					COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
					try
					{
						auto cmd_call = SerializeToCommand(&arg);
						set_command_head(cmd_call, NET_COMMAND_ES_QRQUOTE);
						request_net_cmmmand(cmd_call);
					}
					catch (...)
					{
						state = NET_COMMAND_STATE_SERVER_UNKNOW;
					}
					return state;
				}
#endif
			}
		}
	}
}