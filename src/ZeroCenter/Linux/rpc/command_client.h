#ifndef _AGEBULL_NET_COMMAND_TRADECOMMAND_CLIENT_H
#define _AGEBULL_NET_COMMAND_TRADECOMMAND_CLIENT_H
#pragma once
#include "stdafx.h"
#ifdef CLR
#pragma unmanaged
#endif

#include "entity.h"
using namespace Agebull::Futures::Globals;

namespace Agebull
{
	namespace Futures
	{
		namespace Globals
		{
			namespace Client
			{
				/**
				* @brief 查询交易所状态
				* @return 无
				*/
				COMMAND_STATE QryExchangeState();

				/**
				* @brief 查询交易商品
				* @return 无
				*/
				COMMAND_STATE QryCommodity();

				/**
				* @brief 查询合约
				* @param {ContractQueryArg} arg 调用参数
				* @return 无
				*/
				COMMAND_STATE QryContract(ContractQueryArg& arg);

				/**
				* @brief 查询货币币种信息
				* @return 无
				*/
				COMMAND_STATE QryCurrency();
#ifndef WEB
				/**
				* @brief 修改当前品种
				* @return 无
				*/
				COMMAND_STATE ChangeCommodity(const char* commodity);


				/**
				* @brief 报单请求
				* @param {OrderInsertArg} arg 调用参数
				* @return 无
				*/
				COMMAND_STATE OrderInsert(CustomerOrder& arg);
				/**
				* @brief 查询客户资金
				* @return 无
				*/
				COMMAND_STATE QryMoney();

				/**
				* @brief 查询客户委托
				* @param {OrderQueryArg} arg 调用参数
				* @return 无
				*/
				COMMAND_STATE QryOrder(OrderQueryArg& arg);

				/**
				* @brief 查询客户成交
				* @param {TMatchQueryArg} arg 调用参数
				* @return 无
				*/
				COMMAND_STATE QryMatch(TMatchQueryArg& arg);

				/**
				* @brief 查询持仓
				* @param {HoldQueryArg} arg 调用参数
				* @return 无
				*/
				COMMAND_STATE QryHold(HoldQueryArg& arg);

				/**
				* @brief 查询K线行情
				* @return 无
				*/
				COMMAND_STATE QryCandleStick(StringArgument& arg);
				/**
				* @brief 改单请求
				* @param {OrderModifyArg} arg 调用参数
				* @return 无
				*/
				COMMAND_STATE OrderModify(OrderModifyArg& arg);

				/**
				* @brief 撤单请求
				* @param {int} arg 调用参数
				* @return 无
				*/
				COMMAND_STATE OrderDelete(int& arg);

				/**
				* @brief 历史委托查询
				* @param {HisOrderQueryArg} arg 调用参数
				* @return 无
				*/
				COMMAND_STATE QryHistOrder(HisOrderQueryArg& arg);

				/**
				* @brief 历史成交查询
				* @param {HisMatchQueryArg} arg 调用参数
				* @return 无
				*/
				COMMAND_STATE QryHistMatch(HisMatchQueryArg& arg);

				/**
				* @brief 出金入金查询
				* @return 无
				*/
				COMMAND_STATE QryCashOper();

				/**
				* @brief 出金入金审核请求
				* @param {CashCheckArg} arg 调用参数
				* @return 无
				*/
				COMMAND_STATE CashCheck(CashCheckArg& arg);

				/**
				* @brief 资金调整查询
				* @return 无
				*/
				COMMAND_STATE QryCachAdjust();

				/**
				* @brief 历史出金入金查询
				* @param {HisCashOperQueryArg} arg 调用参数
				* @return 无
				*/
				COMMAND_STATE QryHistCashOper(HisCashOperQueryArg& arg);

				/**
				* @brief 历史资金调整查询
				* @param {HisAdjustQueryArg} arg 调用参数
				* @return 无
				*/
				COMMAND_STATE QryHistCachAdjust(HisAdjustQueryArg& arg);

				/**
				* @brief 下港交所做市商报单
				* @param {HKMarketOrderArg} arg 调用参数
				* @return 无
				*/
				COMMAND_STATE HKMarketOrderOperator(HKMarketOrderArg& arg);

				/**
				* @brief 客户计算参数查询
				* @param {TClientCountRentQueryArg} arg 调用参数
				* @return 无
				*/
				COMMAND_STATE QryCountRent(TClientCountRentQueryArg& arg);
#endif
			}
		}
	}
}
#endif
