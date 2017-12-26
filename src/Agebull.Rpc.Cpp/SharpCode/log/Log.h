#ifndef ENTITY_TLOG_H
#define ENTITY_TLOG_H

#include <stdinc.h>
#include <entity.h>
using namespace std;

///代理商 生成日志文本
string to_log_text(const Agent* value);

///代理商 记录到日志
void log(const Agent* value, int type, int level);
///代理商客户信息 生成日志文本
string to_log_text(const AgentCustomer* value);

///代理商客户信息 记录到日志
void log(const AgentCustomer* value, int type, int level);
///代理商隶属关系 生成日志文本
string to_log_text(const AgentSubAgent* value);

///代理商隶属关系 记录到日志
void log(const AgentSubAgent* value, int type, int level);
///商品 生成日志文本
string to_log_text(const Commodity* value);

///商品 记录到日志
void log(const Commodity* value, int type, int level);
///合约 生成日志文本
string to_log_text(const Contract* value);

///合约 记录到日志
void log(const Contract* value, int type, int level);
///币种 生成日志文本
string to_log_text(const Currency* value);

///币种 记录到日志
void log(const Currency* value, int type, int level);
///交易所 生成日志文本
string to_log_text(const Exchange* value);

///交易所 记录到日志
void log(const Exchange* value, int type, int level);
///行情 生成日志文本
string to_log_text(const QuoteWhole* value);

///行情 记录到日志
void log(const QuoteWhole* value, int type, int level);
///客户资金信息 生成日志文本
string to_log_text(const CustomFunds* value);

///客户资金信息 记录到日志
void log(const CustomFunds* value, int type, int level);
///资金基本信息 生成日志文本
string to_log_text(const FundBase* value);
///文本的参数 生成日志文本
string to_log_text(const StringArgument* value);

///文本的参数 记录到日志
void log(const StringArgument* value, int type, int level);
///持仓 生成日志文本
string to_log_text(const CustomerHold* value);

///持仓 记录到日志
void log(const CustomerHold* value, int type, int level);
///客户委托 生成日志文本
string to_log_text(const CustomerOrder* value);

///客户委托 记录到日志
void log(const CustomerOrder* value, int type, int level);
///客户成交记录 生成日志文本
string to_log_text(const CustomMatch* value);

///客户成交记录 记录到日志
void log(const CustomMatch* value, int type, int level);
#ifdef EsQuote
///Application信息 生成日志文本
string to_log_text(const TapAPIApplicationInfo* value);

///Application信息 记录到日志
void log(const TapAPIApplicationInfo* value, int type, int level);
///修改密码请求 生成日志文本
string to_log_text(const TapAPIChangePasswordReq* value);

///修改密码请求 记录到日志
void log(const TapAPIChangePasswordReq* value, int type, int level);
///品种编码结构 生成日志文本
string to_log_text(const TapAPICommodity* value);

///品种编码结构 记录到日志
void log(const TapAPICommodity* value, int type, int level);
///合约编码结构 生成日志文本
string to_log_text(const TapAPIContract* value);

///合约编码结构 记录到日志
void log(const TapAPIContract* value, int type, int level);
///交易所信息 生成日志文本
string to_log_text(const TapAPIExchangeInfo* value);

///交易所信息 记录到日志
void log(const TapAPIExchangeInfo* value, int type, int level);
///品种信息 生成日志文本
string to_log_text(const TapAPIQuoteCommodityInfo* value);

///品种信息 记录到日志
void log(const TapAPIQuoteCommodityInfo* value, int type, int level);
///行情合约信息 生成日志文本
string to_log_text(const TapAPIQuoteContractInfo* value);

///行情合约信息 记录到日志
void log(const TapAPIQuoteContractInfo* value, int type, int level);
///登录认证信息 生成日志文本
string to_log_text(const TapAPIQuoteLoginAuth* value);

///登录认证信息 记录到日志
void log(const TapAPIQuoteLoginAuth* value, int type, int level);
///行情全文 生成日志文本
string to_log_text(const TapAPIQuoteWhole* value);

///行情全文 记录到日志
void log(const TapAPIQuoteWhole* value, int type, int level);
///登录反馈信息 生成日志文本
string to_log_text(const TapAPIQuotLoginRspInfo* value);

///登录反馈信息 记录到日志
void log(const TapAPIQuotLoginRspInfo* value, int type, int level);
#endif
#ifdef EsTrade

///交易员下属客户信息数据 生成日志文本
string to_log_text(const OperatorClientQueryRsp* value);

///交易员下属客户信息数据 记录到日志
void log(const OperatorClientQueryRsp* value, int type, int level);
///商品状态变化数据 生成日志文本
string to_log_text(const CommodityStateModNotice* value);

///商品状态变化数据 记录到日志
void log(const CommodityStateModNotice* value, int type, int level);
///查询合约命令调用参数 生成日志文本
string to_log_text(const ContractQueryArg* value);

///查询合约命令调用参数 记录到日志
void log(const ContractQueryArg* value, int type, int level);
///币种汇率变化数据 生成日志文本
string to_log_text(const ExchangeRateModifyNotice* value);

///币种汇率变化数据 记录到日志
void log(const ExchangeRateModifyNotice* value, int type, int level);
///市场状态变更数据 生成日志文本
string to_log_text(const ExchangeStateModifyNotice* value);

///市场状态变更数据 记录到日志
void log(const ExchangeStateModifyNotice* value, int type, int level);
///客户计算参数查询命令调用参数 生成日志文本
string to_log_text(const TClientCountRentQueryArg* value);

///客户计算参数查询命令调用参数 记录到日志
void log(const TClientCountRentQueryArg* value, int type, int level);
///查询计算参数应答 生成日志文本
string to_log_text(const TClientCountRentQueryRsp* value);

///查询计算参数应答 记录到日志
void log(const TClientCountRentQueryRsp* value, int type, int level);
///资金调整数据 生成日志文本
string to_log_text(const AdjustQueryRsp* value);

///资金调整数据 记录到日志
void log(const AdjustQueryRsp* value, int type, int level);
///出金入金审核请求命令调用参数 生成日志文本
string to_log_text(const CashCheckArg* value);

///出金入金审核请求命令调用参数 记录到日志
void log(const CashCheckArg* value, int type, int level);
///出入金 生成日志文本
string to_log_text(const CashOper* value);

///出入金 记录到日志
void log(const CashOper* value, int type, int level);
///出金入金数据 生成日志文本
string to_log_text(const CashOperQueryRsp* value);

///出金入金数据 记录到日志
void log(const CashOperQueryRsp* value, int type, int level);
///出金入金操作应答 生成日志文本
string to_log_text(const CashOperRsp* value);

///出金入金操作应答 记录到日志
void log(const CashOperRsp* value, int type, int level);

///资金基本信息 记录到日志
void log(const FundBase* value, int type, int level);
///历史资金调整查询命令调用参数 生成日志文本
string to_log_text(const HisAdjustQueryArg* value);

///历史资金调整查询命令调用参数 记录到日志
void log(const HisAdjustQueryArg* value, int type, int level);
///历史资金调整数据 生成日志文本
string to_log_text(const HisAdjustQueryRsp* value);

///历史资金调整数据 记录到日志
void log(const HisAdjustQueryRsp* value, int type, int level);
///历史出金入金查询命令调用参数 生成日志文本
string to_log_text(const HisCashOperQueryArg* value);

///历史出金入金查询命令调用参数 记录到日志
void log(const HisCashOperQueryArg* value, int type, int level);
///历史出金入金数据 生成日志文本
string to_log_text(const HisCashOperQueryRsp* value);

///历史出金入金数据 记录到日志
void log(const HisCashOperQueryRsp* value, int type, int level);
///资金变化数据 生成日志文本
string to_log_text(const MoneyChgNotice* value);

///资金变化数据 记录到日志
void log(const MoneyChgNotice* value, int type, int level);
///资金数据 生成日志文本
string to_log_text(const MoneyQueryRsp* value);

///资金数据 记录到日志
void log(const MoneyQueryRsp* value, int type, int level);
///用户密码验证请求应答 生成日志文本
string to_log_text(const ClientPasswordAuthRsp* value);

///用户密码验证请求应答 记录到日志
void log(const ClientPasswordAuthRsp* value, int type, int level);
///修改客户密码命令调用参数 生成日志文本
string to_log_text(const ClientPasswordModifyArg* value);

///修改客户密码命令调用参数 记录到日志
void log(const ClientPasswordModifyArg* value, int type, int level);
///客户密码修改应答结构 生成日志文本
string to_log_text(const ClientPasswordModifyRsp* value);

///客户密码修改应答结构 记录到日志
void log(const ClientPasswordModifyRsp* value, int type, int level);
///登录请求结构 生成日志文本
string to_log_text(const LoginArg* value);

///登录请求结构 记录到日志
void log(const LoginArg* value, int type, int level);
///密码修改请求结构 生成日志文本
string to_log_text(const ModifyPasswordArg* value);

///密码修改请求结构 记录到日志
void log(const ModifyPasswordArg* value, int type, int level);
///查询监控事件应答 生成日志文本
string to_log_text(const MonitorEventQueryRsp* value);

///查询监控事件应答 记录到日志
void log(const MonitorEventQueryRsp* value, int type, int level);
///网络服务的地址信息 生成日志文本
string to_log_text(const NetServiceAddress* value);

///网络服务的地址信息 记录到日志
void log(const NetServiceAddress* value, int type, int level);
///修改操作员密码命令调用参数 生成日志文本
string to_log_text(const OperatorPasswordModifyArg* value);

///修改操作员密码命令调用参数 记录到日志
void log(const OperatorPasswordModifyArg* value, int type, int level);
///修改操作员密码时应答 生成日志文本
string to_log_text(const OperatorPasswordModifyRsp* value);

///修改操作员密码时应答 记录到日志
void log(const OperatorPasswordModifyRsp* value, int type, int level);
///历史成交查询命令调用参数 生成日志文本
string to_log_text(const HisMatchQueryArg* value);

///历史成交查询命令调用参数 记录到日志
void log(const HisMatchQueryArg* value, int type, int level);
///历史委托查询命令调用参数 生成日志文本
string to_log_text(const HisOrderQueryArg* value);

///历史委托查询命令调用参数 记录到日志
void log(const HisOrderQueryArg* value, int type, int level);
///历史委托数据 生成日志文本
string to_log_text(const HisOrderQueryRsp* value);

///历史委托数据 记录到日志
void log(const HisOrderQueryRsp* value, int type, int level);
///下港交所做市商报单命令调用参数 生成日志文本
string to_log_text(const HKMarketOrderArg* value);

///下港交所做市商报单命令调用参数 记录到日志
void log(const HKMarketOrderArg* value, int type, int level);
///查询持仓命令调用参数 生成日志文本
string to_log_text(const HoldQueryArg* value);

///查询持仓命令调用参数 记录到日志
void log(const HoldQueryArg* value, int type, int level);
///持仓变化数据 生成日志文本
string to_log_text(const HoldQueryRsp* value);

///持仓变化数据 记录到日志
void log(const HoldQueryRsp* value, int type, int level);
///成交删除数据 生成日志文本
string to_log_text(const MatchRemoveNotice* value);

///成交删除数据 记录到日志
void log(const MatchRemoveNotice* value, int type, int level);
///撤单请求的应答 生成日志文本
string to_log_text(const OrderDeleteRsp* value);

///撤单请求的应答 记录到日志
void log(const OrderDeleteRsp* value, int type, int level);
///委托信息变化数据 生成日志文本
string to_log_text(const OrderInfoNotice* value);

///委托信息变化数据 记录到日志
void log(const OrderInfoNotice* value, int type, int level);
///报单请求的应答 生成日志文本
string to_log_text(const OrderInsertRsp* value);

///报单请求的应答 记录到日志
void log(const OrderInsertRsp* value, int type, int level);
///改单请求命令调用参数 生成日志文本
string to_log_text(const OrderModifyArg* value);

///改单请求命令调用参数 记录到日志
void log(const OrderModifyArg* value, int type, int level);
///改单请求的应答 生成日志文本
string to_log_text(const OrderModifyRsp* value);

///改单请求的应答 记录到日志
void log(const OrderModifyRsp* value, int type, int level);
///查询客户委托命令调用参数 生成日志文本
string to_log_text(const OrderQueryArg* value);

///查询客户委托命令调用参数 记录到日志
void log(const OrderQueryArg* value, int type, int level);
///委托删除数据 生成日志文本
string to_log_text(const OrderRemoveNotice* value);

///委托删除数据 记录到日志
void log(const OrderRemoveNotice* value, int type, int level);
///委托变化数据 生成日志文本
string to_log_text(const OrderStateNotice* value);

///委托变化数据 记录到日志
void log(const OrderStateNotice* value, int type, int level);
///查询客户成交命令调用参数 生成日志文本
string to_log_text(const TMatchQueryArg* value);

///查询客户成交命令调用参数 记录到日志
void log(const TMatchQueryArg* value, int type, int level);
///客户计算参数查询请求结构 生成日志文本
string to_log_text(const TClientCountRentQryReq* value);

///客户计算参数查询请求结构 记录到日志
void log(const TClientCountRentQryReq* value, int type, int level);
///客户结算参数查询应答结构 生成日志文本
string to_log_text(const TClientCountRentQryRsp* value);

///客户结算参数查询应答结构 记录到日志
void log(const TClientCountRentQryRsp* value, int type, int level);
///基于IP连接的地址信息 生成日志文本
string to_log_text(const TEsAddressField* value);

///基于IP连接的地址信息 记录到日志
void log(const TEsAddressField* value, int type, int level);
///出金入金审核应答结构 生成日志文本
string to_log_text(const TEsAdjustQryReqField* value);

///出金入金审核应答结构 记录到日志
void log(const TEsAdjustQryReqField* value, int type, int level);
///资金调整查询应答结构 生成日志文本
string to_log_text(const TEsAdjustQryRspField* value);

///资金调整查询应答结构 记录到日志
void log(const TEsAdjustQryRspField* value, int type, int level);
///出金入金操作通知结构 生成日志文本
string to_log_text(const TEsCashCheckReqField* value);

///出金入金操作通知结构 记录到日志
void log(const TEsCashCheckReqField* value, int type, int level);
///出金入金查询请求结构 生成日志文本
string to_log_text(const TEsCashOperQryReqField* value);

///出金入金查询请求结构 记录到日志
void log(const TEsCashOperQryReqField* value, int type, int level);
///出金入金查询应答结构 生成日志文本
string to_log_text(const TEsCashOperQryRspField* value);

///出金入金查询应答结构 记录到日志
void log(const TEsCashOperQryRspField* value, int type, int level);
///出金入金操作请求结构 生成日志文本
string to_log_text(const TEsCashOperReqField* value);

///出金入金操作请求结构 记录到日志
void log(const TEsCashOperReqField* value, int type, int level);
///出金入金操作应答结构 生成日志文本
string to_log_text(const TEsCashOperRspField* value);

///出金入金操作应答结构 记录到日志
void log(const TEsCashOperRspField* value, int type, int level);
///客户认证密码验证请求结构 生成日志文本
string to_log_text(const TEsClientPasswordAuthReqField* value);

///客户认证密码验证请求结构 记录到日志
void log(const TEsClientPasswordAuthReqField* value, int type, int level);
///客户认证密码验证应答结构 生成日志文本
string to_log_text(const TEsClientPasswordAuthRspField* value);

///客户认证密码验证应答结构 记录到日志
void log(const TEsClientPasswordAuthRspField* value, int type, int level);
///客户密码修改请求结构 生成日志文本
string to_log_text(const TEsClientPasswordModifyReqField* value);

///客户密码修改请求结构 记录到日志
void log(const TEsClientPasswordModifyReqField* value, int type, int level);
///客户密码修改应答结构 生成日志文本
string to_log_text(const TEsClientPasswordModifyRspField* value);

///客户密码修改应答结构 记录到日志
void log(const TEsClientPasswordModifyRspField* value, int type, int level);
///商品查询请求结构 生成日志文本
string to_log_text(const TEsCommodityQryReqField* value);

///商品查询请求结构 记录到日志
void log(const TEsCommodityQryReqField* value, int type, int level);
///商品查询应答结构 生成日志文本
string to_log_text(const TEsCommodityQryRspField* value);

///商品查询应答结构 记录到日志
void log(const TEsCommodityQryRspField* value, int type, int level);
///商品状态变化通知结构 生成日志文本
string to_log_text(const TEsCommodityStateModNoticeField* value);

///商品状态变化通知结构 记录到日志
void log(const TEsCommodityStateModNoticeField* value, int type, int level);
///合约查询请求结构 生成日志文本
string to_log_text(const TEsContractQryReqField* value);

///合约查询请求结构 记录到日志
void log(const TEsContractQryReqField* value, int type, int level);
///合约查询应答结构 生成日志文本
string to_log_text(const TEsContractQryRspField* value);

///合约查询应答结构 记录到日志
void log(const TEsContractQryRspField* value, int type, int level);
///币种查询请求结构 生成日志文本
string to_log_text(const TEsCurrencyQryReqField* value);

///币种查询请求结构 记录到日志
void log(const TEsCurrencyQryReqField* value, int type, int level);
///币种查询应答结构 生成日志文本
string to_log_text(const TEsCurrencyQryRspField* value);

///币种查询应答结构 记录到日志
void log(const TEsCurrencyQryRspField* value, int type, int level);
///市场查询请求结构 生成日志文本
string to_log_text(const TEsExchangeQryReqField* value);

///市场查询请求结构 记录到日志
void log(const TEsExchangeQryReqField* value, int type, int level);
///市场查询应答结构 生成日志文本
string to_log_text(const TEsExchangeQryRspField* value);

///市场查询应答结构 记录到日志
void log(const TEsExchangeQryRspField* value, int type, int level);
///汇率变更数据 生成日志文本
string to_log_text(const TEsExchangeRateModifyNoticeField* value);

///汇率变更数据 记录到日志
void log(const TEsExchangeRateModifyNoticeField* value, int type, int level);
///市场状态修改通知结构 生成日志文本
string to_log_text(const TEsExchangeStateModifyNoticeField* value);

///市场状态修改通知结构 记录到日志
void log(const TEsExchangeStateModifyNoticeField* value, int type, int level);
///历史资金调整查询请求结构 生成日志文本
string to_log_text(const TEsHisAdjustQryReqField* value);

///历史资金调整查询请求结构 记录到日志
void log(const TEsHisAdjustQryReqField* value, int type, int level);
///历史资金调整查询应答结构 生成日志文本
string to_log_text(const TEsHisAdjustQryRspField* value);

///历史资金调整查询应答结构 记录到日志
void log(const TEsHisAdjustQryRspField* value, int type, int level);
///历史出入金查询请求结构 生成日志文本
string to_log_text(const TEsHisCashOperQryReqField* value);

///历史出入金查询请求结构 记录到日志
void log(const TEsHisCashOperQryReqField* value, int type, int level);
///历史出入金查询应答结构 生成日志文本
string to_log_text(const TEsHisCashOperQryRspField* value);

///历史出入金查询应答结构 记录到日志
void log(const TEsHisCashOperQryRspField* value, int type, int level);
///历史成交查询请求结构 生成日志文本
string to_log_text(const TEsHisMatchQryReqField* value);

///历史成交查询请求结构 记录到日志
void log(const TEsHisMatchQryReqField* value, int type, int level);
///历史成交查询应答结构 生成日志文本
string to_log_text(const TEsHisMatchQryRspField* value);

///历史成交查询应答结构 记录到日志
void log(const TEsHisMatchQryRspField* value, int type, int level);
///委托变更流程查询应答命令 生成日志文本
string to_log_text(const TEsHisOrderProcessQryReqField* value);

///委托变更流程查询应答命令 记录到日志
void log(const TEsHisOrderProcessQryReqField* value, int type, int level);
///历史委托流程查询应答结构 生成日志文本
string to_log_text(const TEsHisOrderProcessQryRspField* value);

///历史委托流程查询应答结构 记录到日志
void log(const TEsHisOrderProcessQryRspField* value, int type, int level);
///历史委托查询请求结构 生成日志文本
string to_log_text(const TEsHisOrderQryReqField* value);

///历史委托查询请求结构 记录到日志
void log(const TEsHisOrderQryReqField* value, int type, int level);
///历史委托查询应答结构 生成日志文本
string to_log_text(const TEsHisOrderQryRspField* value);

///历史委托查询应答结构 记录到日志
void log(const TEsHisOrderQryRspField* value, int type, int level);
///监控事件通知结构 生成日志文本
string to_log_text(const TEsHKMarketOrderReq* value);

///监控事件通知结构 记录到日志
void log(const TEsHKMarketOrderReq* value, int type, int level);
///持仓查询请求结构 生成日志文本
string to_log_text(const TEsHoldQryReqField* value);

///持仓查询请求结构 记录到日志
void log(const TEsHoldQryReqField* value, int type, int level);
///持仓查询应答结构 生成日志文本
string to_log_text(const TEsHoldQryRspField* value);

///持仓查询应答结构 记录到日志
void log(const TEsHoldQryRspField* value, int type, int level);
///登录请求结构 生成日志文本
string to_log_text(const TEsLoginReqField* value);

///登录请求结构 记录到日志
void log(const TEsLoginReqField* value, int type, int level);
///登录应答结构 生成日志文本
string to_log_text(const TEsLoginRspField* value);

///登录应答结构 记录到日志
void log(const TEsLoginRspField* value, int type, int level);
///成交信息数据 生成日志文本
string to_log_text(const TEsMatchInfoNoticeField* value);

///成交信息数据 记录到日志
void log(const TEsMatchInfoNoticeField* value, int type, int level);
///删除成交通知结构 生成日志文本
string to_log_text(const TEsMatchRemoveNoticeField* value);

///删除成交通知结构 记录到日志
void log(const TEsMatchRemoveNoticeField* value, int type, int level);
///成交状态数据 生成日志文本
string to_log_text(const TEsMatchStateNoticeField* value);

///成交状态数据 记录到日志
void log(const TEsMatchStateNoticeField* value, int type, int level);
///资金变更消息推送 生成日志文本
string to_log_text(const TEsMoneyChgNoticeField* value);

///资金变更消息推送 记录到日志
void log(const TEsMoneyChgNoticeField* value, int type, int level);
///资金查询请求结构 生成日志文本
string to_log_text(const TEsMoneyQryReqField* value);

///资金查询请求结构 记录到日志
void log(const TEsMoneyQryReqField* value, int type, int level);
///资金查询应答结构(注意次结构与资金变化通知结构的关系 生成日志文本
string to_log_text(const TEsMoneyQryRspField* value);

///资金查询应答结构(注意次结构与资金变化通知结构的关系 记录到日志
void log(const TEsMoneyQryRspField* value, int type, int level);
///监控事件查询请求结构 生成日志文本
string to_log_text(const TEsMonitorEventQryReqField* value);

///监控事件查询请求结构 记录到日志
void log(const TEsMonitorEventQryReqField* value, int type, int level);
///监控事件查询应答结构 生成日志文本
string to_log_text(const TEsMonitorEventQryRspField* value);

///监控事件查询应答结构 记录到日志
void log(const TEsMonitorEventQryRspField* value, int type, int level);
///操作员下属客户查询请求结构 生成日志文本
string to_log_text(const TEsOperatorClientQryReqField* value);

///操作员下属客户查询请求结构 记录到日志
void log(const TEsOperatorClientQryReqField* value, int type, int level);
///操作员下属客户查询应答结构 生成日志文本
string to_log_text(const TEsOperatorClientQryRspField* value);

///操作员下属客户查询应答结构 记录到日志
void log(const TEsOperatorClientQryRspField* value, int type, int level);
///操作员密码修改请求结构 生成日志文本
string to_log_text(const TEsOperatorPasswordModifyReqField* value);

///操作员密码修改请求结构 记录到日志
void log(const TEsOperatorPasswordModifyReqField* value, int type, int level);
///操作员密码修改应答结构 生成日志文本
string to_log_text(const TEsOperatorPasswordModifyRspField* value);

///操作员密码修改应答结构 记录到日志
void log(const TEsOperatorPasswordModifyRspField* value, int type, int level);
///撤单请求结构 生成日志文本
string to_log_text(const TEsOrderDeleteReqField* value);

///撤单请求结构 记录到日志
void log(const TEsOrderDeleteReqField* value, int type, int level);
///撤单应答结构 生成日志文本
string to_log_text(const TEsOrderDeleteRspField* value);

///撤单应答结构 记录到日志
void log(const TEsOrderDeleteRspField* value, int type, int level);
///委托信息变更数据 生成日志文本
string to_log_text(const TEsOrderInfoNoticeField* value);

///委托信息变更数据 记录到日志
void log(const TEsOrderInfoNoticeField* value, int type, int level);
///报单请求结构 生成日志文本
string to_log_text(const TEsOrderInsertReqField* value);

///报单请求结构 记录到日志
void log(const TEsOrderInsertReqField* value, int type, int level);
///报单应答结构 生成日志文本
string to_log_text(const TEsOrderInsertRspField* value);

///报单应答结构 记录到日志
void log(const TEsOrderInsertRspField* value, int type, int level);
///改单请求结构 生成日志文本
string to_log_text(const TEsOrderModifyReqField* value);

///改单请求结构 记录到日志
void log(const TEsOrderModifyReqField* value, int type, int level);
///改单应答结构 生成日志文本
string to_log_text(const TEsOrderModifyRspField* value);

///改单应答结构 记录到日志
void log(const TEsOrderModifyRspField* value, int type, int level);
///委托变更流程查询请求命令 生成日志文本
string to_log_text(const TEsOrderProcessQryReqField* value);

///委托变更流程查询请求命令 记录到日志
void log(const TEsOrderProcessQryReqField* value, int type, int level);
///委托查询请求 生成日志文本
string to_log_text(const TEsOrderQryReqField* value);

///委托查询请求 记录到日志
void log(const TEsOrderQryReqField* value, int type, int level);
///删除委托通知结构 生成日志文本
string to_log_text(const TEsOrderRemoveNoticeField* value);

///删除委托通知结构 记录到日志
void log(const TEsOrderRemoveNoticeField* value, int type, int level);
///委托状态变更数据 生成日志文本
string to_log_text(const TEsOrderStateNoticeField* value);

///委托状态变更数据 记录到日志
void log(const TEsOrderStateNoticeField* value, int type, int level);
///委托数据应答命令 生成日志文本
string to_log_text(const TMatchQryReqField* value);

///委托数据应答命令 记录到日志
void log(const TMatchQryReqField* value, int type, int level);

///资金变化通知结构 生成日志文本
string to_log_text(const TMoneyChgItem* value);

///资金变化通知结构 记录到日志
void log(const TMoneyChgItem* value, int type, int level);
#endif

///T+1成交类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::AddOneTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::AddOneTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::AddOneTypeClassify::Yes:
		return string("T+1成交");
	case Agebull::Futures::Globals::AddOneTypeClassify::No:
		return string("非T+1成交");
	};
	return string();
}

///日期类型 生成日志文本
inline string to_log_text(const tm& t)
{
	char str[128];
	sprintf_s(str, "%d-%d-%d %d:%d:%d", 1900 + t.tm_year, t.tm_mon + 1, t.tm_mday, t.tm_hour, t.tm_min, t.tm_sec);
	return string(str);
}

///资金调整状态类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::AdjustStateTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::AdjustStateTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::AdjustStateTypeClassify::NotCheck:
		return string("未审核");
	case Agebull::Futures::Globals::AdjustStateTypeClassify::Check:
		return string("已审核");
	case Agebull::Futures::Globals::AdjustStateTypeClassify::Fail:
		return string("审核未通过");
	};
	return string();
}

///入金方式枚举类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::CashModeTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::CashModeTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::CashModeTypeClassify::Cash:
		return string("现金支付");
	case Agebull::Futures::Globals::CashModeTypeClassify::Bank:
		return string("银行转账");
	case Agebull::Futures::Globals::CashModeTypeClassify::Inline:
		return string("在线支付");
	};
	return string();
}

///出入金状态类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::CashStateTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::CashStateTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::CashStateTypeClassify::NotCheck:
		return string("未审核");
	case Agebull::Futures::Globals::CashStateTypeClassify::Check:
		return string("已审核");
	case Agebull::Futures::Globals::CashStateTypeClassify::Fail:
		return string("审核未通过");
	};
	return string();
}

///出入金类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::CashTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::CashTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::CashTypeClassify::Out:
		return string("出金");
	case Agebull::Futures::Globals::CashTypeClassify::In:
		return string("入金");
	};
	return string();
}

///出入金类型枚举类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::CashTypeTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::CashTypeTypeClassify::CashIn:
		return string("入金");
	case Agebull::Futures::Globals::CashTypeTypeClassify::CashOut:
		return string("出金");
	};
	return string();
}
///证件类型枚举类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::CustomerTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::CustomerTypeClassify::None:
		return string("普通客户");
	case Agebull::Futures::Globals::CustomerTypeClassify::Admin:
		return string("管理员");
	case Agebull::Futures::Globals::CustomerTypeClassify::Manager:
		return string("负责人");
	case Agebull::Futures::Globals::CustomerTypeClassify::SystemAdmin:
		return string("系统管理员");
	};
	return string();
}

///证件类型枚举类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::CertificateTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::CertificateTypeClassify::None:
		return string("无");
	case Agebull::Futures::Globals::CertificateTypeClassify::ID:
		return string("身份证");
	case Agebull::Futures::Globals::CertificateTypeClassify::DriverLicense:
		return string("驾驶证");
	case Agebull::Futures::Globals::CertificateTypeClassify::CertificateOfOfficers:
		return string("军官证");
	case Agebull::Futures::Globals::CertificateTypeClassify::Passport:
		return string("护照");
	case Agebull::Futures::Globals::CertificateTypeClassify::BusinessLicense:
		return string("营业执照");
	case Agebull::Futures::Globals::CertificateTypeClassify::OtherDocuments:
		return string("其它证件");
	};
	return string();
}

///商品状态类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::CommodityStateTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::CommodityStateTypeClassify::None:
		return string("空");
	case Agebull::Futures::Globals::CommodityStateTypeClassify::Yes:
		return string("商品允许交易");
	case Agebull::Futures::Globals::CommodityStateTypeClassify::No:
		return string("商品禁止交易");
	case Agebull::Futures::Globals::CommodityStateTypeClassify::Cover:
		return string("商品只可平仓");
	};
	return string();
}

///商品类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::CommodityTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::CommodityTypeClassify::None:
		return string("空");
	case Agebull::Futures::Globals::CommodityTypeClassify::Goods:
		return string("现货");
	case Agebull::Futures::Globals::CommodityTypeClassify::Future:
		return string("期货");
	case Agebull::Futures::Globals::CommodityTypeClassify::Option:
		return string("期权");
	case Agebull::Futures::Globals::CommodityTypeClassify::SpreadMonth:
		return string("跨期套利");
	case Agebull::Futures::Globals::CommodityTypeClassify::SpreadCommodity:
		return string("跨品种套利");
	};
	return string();
}

///是否包含合计值类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::ContainTotleTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::ContainTotleTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::ContainTotleTypeClassify::ContaintotleYes:
		return string("包含");
	case Agebull::Futures::Globals::ContainTotleTypeClassify::ContaintotleNo:
		return string("不包含");
	};
	return string();
}

///合约状态类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::ContractStateTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::ContractStateTypeClassify::None:
		return string("空");
	case Agebull::Futures::Globals::ContractStateTypeClassify::Yes:
		return string("合约允许交易");
	case Agebull::Futures::Globals::ContractStateTypeClassify::No:
		return string("合约禁止交易");
	case Agebull::Futures::Globals::ContractStateTypeClassify::Cover:
		return string("合约只可平仓");
	};
	return string();
}

///合约类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::ContractTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::ContractTypeClassify::None:
		return string("空");
	case Agebull::Futures::Globals::ContractTypeClassify::Single:
		return string("单腿合约");
	case Agebull::Futures::Globals::ContractTypeClassify::Spread:
		return string("跨期套利");
	case Agebull::Futures::Globals::ContractTypeClassify::Swap:
		return string("互换套利");
	case Agebull::Futures::Globals::ContractTypeClassify::Commodity:
		return string("跨品种套利");
	};
	return string();
}

///平仓方式类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::CoverModeTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::CoverModeTypeClassify::None:
		return string("不区分开平");
	case Agebull::Futures::Globals::CoverModeTypeClassify::Unfinished:
		return string("平仓未了结");
	case Agebull::Futures::Globals::CoverModeTypeClassify::Opencover:
		return string("开仓和平仓");
	case Agebull::Futures::Globals::CoverModeTypeClassify::Covertoday:
		return string("开仓、平仓和平今");
	};
	return string();
}

///币种组标志(同一币种组，资金共享)类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::CurrencyGroupFlagTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::CurrencyGroupFlagTypeClassify::None:
		return string("空");
	case Agebull::Futures::Globals::CurrencyGroupFlagTypeClassify::CurrencyGroupA:
		return string("币种组A");
	case Agebull::Futures::Globals::CurrencyGroupFlagTypeClassify::CurrencyGroupB:
		return string("币种组B");
	case Agebull::Futures::Globals::CurrencyGroupFlagTypeClassify::CurrencyGroupC:
		return string("币种组C");
	case Agebull::Futures::Globals::CurrencyGroupFlagTypeClassify::CurrencyGroupD:
		return string("币种组D");
	case Agebull::Futures::Globals::CurrencyGroupFlagTypeClassify::CurrencyGroupE:
		return string("币种组E");
	};
	return string();
}

///委托成交删除标记类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::DeletedTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::DeletedTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::DeletedTypeClassify::DelYes:
		return string("是");
	case Agebull::Futures::Globals::DeletedTypeClassify::DelNo:
		return string("否");
	case Agebull::Futures::Globals::DeletedTypeClassify::DelDisappear:
		return string("隐藏");
	};
	return string();
}

///交割方式类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::DeliveryModeTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::DeliveryModeTypeClassify::None:
		return string("空");
	case Agebull::Futures::Globals::DeliveryModeTypeClassify::Goods:
		return string("实物交割");
	case Agebull::Futures::Globals::DeliveryModeTypeClassify::Cash:
		return string("现金交割");
	case Agebull::Futures::Globals::DeliveryModeTypeClassify::Execute:
		return string("期权行权");
	};
	return string();
}

///持仓计算方式类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::DepositCalculateModeTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::DepositCalculateModeTypeClassify::None:
		return string("空");
	case Agebull::Futures::Globals::DepositCalculateModeTypeClassify::Normal:
		return string("正常");
	case Agebull::Futures::Globals::DepositCalculateModeTypeClassify::Clean:
		return string("合约净持仓");
	case Agebull::Futures::Globals::DepositCalculateModeTypeClassify::Lock:
		return string("品种锁仓");
	};
	return string();
}

///保证金计算方式类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::DepositModeTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::DepositModeTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::DepositModeTypeClassify::B:
		return string("比例");
	case Agebull::Futures::Globals::DepositModeTypeClassify::D:
		return string("定额");
	case Agebull::Futures::Globals::DepositModeTypeClassify::Cb:
		return string("差值比例");
	case Agebull::Futures::Globals::DepositModeTypeClassify::Cd:
		return string("差值定额");
	case Agebull::Futures::Globals::DepositModeTypeClassify::Z:
		return string("折扣");
	};
	return string();
}

///买卖类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::DirectTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::DirectTypeClassify::None:
		return string("无效");
	case Agebull::Futures::Globals::DirectTypeClassify::Buy:
		return string("买入");
	case Agebull::Futures::Globals::DirectTypeClassify::Sell:
		return string("卖出");
	};
	return string();
}

///错误号 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::ErrorCodeTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::ErrorCodeTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::ErrorCodeTypeClassify::ErrorSucceed:
		return string("成功");
	};
	return string();
}

///监控事件等级类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::EventLevelTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::EventLevelTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::EventLevelTypeClassify::Normal:
		return string("正常");
	case Agebull::Futures::Globals::EventLevelTypeClassify::Warnning:
		return string("警报");
	case Agebull::Futures::Globals::EventLevelTypeClassify::Error:
		return string("错误");
	};
	return string();
}

///监控事件类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::EventTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::EventTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::EventTypeClassify::Y:
		return string("常规事件");
	case Agebull::Futures::Globals::EventTypeClassify::N:
		return string("非常规事件");
	};
	return string();
}

///市场状态类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::ExchangeStateTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::ExchangeStateTypeClassify::None:
		return string("空");
	case Agebull::Futures::Globals::ExchangeStateTypeClassify::Yes:
		return string("市场允许交易");
	case Agebull::Futures::Globals::ExchangeStateTypeClassify::No:
		return string("市场禁止交易");
	case Agebull::Futures::Globals::ExchangeStateTypeClassify::Cover:
		return string("市场只可平仓");
	};
	return string();
}

///是否强制出金标记类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::ForceCashOutFlagTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::ForceCashOutFlagTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::ForceCashOutFlagTypeClassify::ForceCashOutYes:
		return string("强制出金，资金不足时，允许出金");
	case Agebull::Futures::Globals::ForceCashOutFlagTypeClassify::ForceCashOutNo:
		return string("不强制出金,资金不足时，不允许出金");
	};
	return string();
}

///投机保值类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::HedgeTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::HedgeTypeClassify::None:
		return string("无");
	case Agebull::Futures::Globals::HedgeTypeClassify::Speculation:
		return string("投机");
	case Agebull::Futures::Globals::HedgeTypeClassify::Hedge:
		return string("保值");
	};
	return string();
}

///港交所报单操作类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::HKMarketOperTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::HKMarketOperTypeClassify::None:
		return string("无");
	case Agebull::Futures::Globals::HKMarketOperTypeClassify::Insert:
		return string("下单");
	case Agebull::Futures::Globals::HKMarketOperTypeClassify::Modify:
		return string("改单");
	case Agebull::Futures::Globals::HKMarketOperTypeClassify::Delete:
		return string("撤单");
	};
	return string();
}

///是否基币类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::IsPrimaryCurrencyTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::IsPrimaryCurrencyTypeClassify::None:
		return string("空");
	case Agebull::Futures::Globals::IsPrimaryCurrencyTypeClassify::CurrencyPrimaryYes:
		return string("是基币");
	case Agebull::Futures::Globals::IsPrimaryCurrencyTypeClassify::CurrencyPrimaryNo:
		return string("不是基币");
	};
	return string();
}

///是否风险报单类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::IsRiskOrderTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::IsRiskOrderTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::IsRiskOrderTypeClassify::RiskOrderYes:
		return string("是风险报单");
	case Agebull::Futures::Globals::IsRiskOrderTypeClassify::RiskOrderNo:
		return string("不是风险报单");
	};
	return string();
}

///本外币标识类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::LWFlagTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::LWFlagTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::LWFlagTypeClassify::LwflagL:
		return string("境内人民币账户");
	case Agebull::Futures::Globals::LWFlagTypeClassify::LwflagW:
		return string("客户境内外币账户");
	case Agebull::Futures::Globals::LWFlagTypeClassify::LjfflagJ:
		return string("公司境内外币账户");
	case Agebull::Futures::Globals::LWFlagTypeClassify::LjfflagF:
		return string("公司境外外币账户");
	};
	return string();
}

///人工填写手续费类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::ManualFeeTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::ManualFeeTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::ManualFeeTypeClassify::ManualfeeYes:
		return string("人工");
	case Agebull::Futures::Globals::ManualFeeTypeClassify::ManualfeeNo:
		return string("自动");
	};
	return string();
}

///成交方式类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::MatchModeTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::MatchModeTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::MatchModeTypeClassify::Normal:
		return string("正常");
	case Agebull::Futures::Globals::MatchModeTypeClassify::Update:
		return string("更新委托");
	case Agebull::Futures::Globals::MatchModeTypeClassify::Other:
		return string("其他");
	};
	return string();
}

///成交方式类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::MatchWayTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::MatchWayTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::MatchWayTypeClassify::All:
		return string("所有");
	case Agebull::Futures::Globals::MatchWayTypeClassify::SelfEtrader:
		return string("自助电子单");
	case Agebull::Futures::Globals::MatchWayTypeClassify::ProxyEtrader:
		return string("代理电子单");
	case Agebull::Futures::Globals::MatchWayTypeClassify::Jtrader:
		return string("外部电子单");
	case Agebull::Futures::Globals::MatchWayTypeClassify::Manual:
		return string("人工录入单");
	case Agebull::Futures::Globals::MatchWayTypeClassify::Carry:
		return string("carry单");
	case Agebull::Futures::Globals::MatchWayTypeClassify::Delivery:
		return string("交割单");
	case Agebull::Futures::Globals::MatchWayTypeClassify::Program:
		return string("程式化单");
	};
	return string();
}

///开平类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::OffsetTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::OffsetTypeClassify::None:
		return string("无");
	case Agebull::Futures::Globals::OffsetTypeClassify::Open:
		return string("开仓");
	case Agebull::Futures::Globals::OffsetTypeClassify::Cover:
		return string("平仓");
	case Agebull::Futures::Globals::OffsetTypeClassify::CoverToday:
		return string("平今");
	};
	return string();
}

///委托录入类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::OrderInputTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::OrderInputTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::OrderInputTypeClassify::Yes:
		return string("是");
	case Agebull::Futures::Globals::OrderInputTypeClassify::No:
		return string("否");
	};
	return string();
}

///委托模式类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::OrderModeTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::OrderModeTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::OrderModeTypeClassify::Fok:
		return string("霍");
	case Agebull::Futures::Globals::OrderModeTypeClassify::Fak:
		return string("FAK或IOC");
	case Agebull::Futures::Globals::OrderModeTypeClassify::Gfd:
		return string("当日有效");
	case Agebull::Futures::Globals::OrderModeTypeClassify::Gtc:
		return string("取消前有效");
	case Agebull::Futures::Globals::OrderModeTypeClassify::Gtd:
		return string("指定日期前有效");
	};
	return string();
}

///委托状态类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::OrderStateTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::OrderStateTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::OrderStateTypeClassify::Fail:
		return string("指令失败");
	case Agebull::Futures::Globals::OrderStateTypeClassify::Finished:
		return string("完全成交");
	case Agebull::Futures::Globals::OrderStateTypeClassify::Accept:
		return string("已受理");
	case Agebull::Futures::Globals::OrderStateTypeClassify::Suppended:
		return string("已挂起");
	case Agebull::Futures::Globals::OrderStateTypeClassify::Queued:
		return string("已排队");
	case Agebull::Futures::Globals::OrderStateTypeClassify::Deleteing:
		return string("待撤消(排队临时状态");
	case Agebull::Futures::Globals::OrderStateTypeClassify::Modifying:
		return string("待修改(排队临时状态");
	case Agebull::Futures::Globals::OrderStateTypeClassify::Partdeleted:
		return string("部分撤单");
	case Agebull::Futures::Globals::OrderStateTypeClassify::Deleted:
		return string("完全撤单");
	case Agebull::Futures::Globals::OrderStateTypeClassify::Partfinished:
		return string("部分成交");
	};
	return string();
}

///委托类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::OrderTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::OrderTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::OrderTypeClassify::Limit:
		return string("限价");
	case Agebull::Futures::Globals::OrderTypeClassify::Market:
		return string("市价");
	case Agebull::Futures::Globals::OrderTypeClassify::StopLimit:
		return string("限价止损");
	case Agebull::Futures::Globals::OrderTypeClassify::StopMarket:
		return string("市价止损");
	case Agebull::Futures::Globals::OrderTypeClassify::Iceberg:
		return string("冰山单");
	case Agebull::Futures::Globals::OrderTypeClassify::Ghost:
		return string("影子单");
	};
	return string();
}

///委托方式类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::OrderWayTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::OrderWayTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::OrderWayTypeClassify::SelfEtrader:
		return string("自助电子单");
	case Agebull::Futures::Globals::OrderWayTypeClassify::ProxyEtrader:
		return string("代理电子单");
	case Agebull::Futures::Globals::OrderWayTypeClassify::Manual:
		return string("人工录入单");
	case Agebull::Futures::Globals::OrderWayTypeClassify::Carry:
		return string("carry单");
	case Agebull::Futures::Globals::OrderWayTypeClassify::Program:
		return string("程式化报单");
	};
	return string();
}

///密码类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::PasswordTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::PasswordTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::PasswordTypeClassify::Trade:
		return string("交易密码");
	case Agebull::Futures::Globals::PasswordTypeClassify::Quote:
		return string("行情密码");
	case Agebull::Futures::Globals::PasswordTypeClassify::Auth:
		return string("认证密码");
	};
	return string();
}

///冰山单是否随机量发送类型 生成日志文本
inline string to_log_text(Agebull::Futures::Globals::RandomiseTypeClassify value)
{
	switch (value)
	{
	case Agebull::Futures::Globals::RandomiseTypeClassify::None:
		return string("未知");
	case Agebull::Futures::Globals::RandomiseTypeClassify::RandomYes:
		return string("是");
	case Agebull::Futures::Globals::RandomiseTypeClassify::RandomNo:
		return string("否");
	};
	return string();
}
#endif
