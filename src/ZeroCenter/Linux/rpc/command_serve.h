#pragma once
#ifdef SERVER
#include "net_command.h"

#ifndef PROXY


/**
* @brief 登录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void UserLogin(const PNetCommand cmd);

/**
* @brief 登出
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void UserLogout(const PNetCommand cmd);

/**
* @brief 修改密码
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void UserSetPassword(const PNetCommand cmd);
#endif

/**
* @brief 查询交易所
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void SysQueryExchange(const PNetCommand cmd);

/**
* @brief 查询交易商品
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void SysQueryCommodity(const PNetCommand cmd);

/**
* @brief 查询合约
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void SysQueryContract(const PNetCommand cmd);

/**
* @brief 查询货币币种信息
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void SysQueryCurrency(const PNetCommand cmd);

/**
* @brief 查询客户资金记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryMoney(const PNetCommand cmd);

/**
* @brief 查询持仓记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryHold(const PNetCommand cmd);

/**
* @brief 查询客户当日委托记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryTodayOrder(const PNetCommand cmd);

/**
* @brief 查询客户历史委托记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryHistoryOrder(const PNetCommand cmd);

/**
* @brief 查询客户当日成交记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryTodayMatch(const PNetCommand cmd);

/**
* @brief 查询客户历史成交记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryHistoryMatch(const PNetCommand cmd);

/**
* @brief 查询客户当日出入金记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryTodayCash(const PNetCommand cmd);

/**
* @brief 查询客户历史出入金记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryHistoryCash(const PNetCommand cmd);

/**
* @brief 查询客户资金当日调整记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryTodayCashAdjust(const PNetCommand cmd);

/**
* @brief 查询客户资金历史调整记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryHistoryCashAdjust(const PNetCommand cmd);

/**
* @brief 报单请求
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void OrderInsert(const PNetCommand cmd);

/**
* @brief 改单请求
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void OrderModify(const PNetCommand cmd);

/**
* @brief 撤单请求
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void OrderCancel(const PNetCommand cmd);

#endif
