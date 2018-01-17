#include "log.h"
#include "mydecimal.h"


///代理商 生成日志文本
string to_log_text(const Agent* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->AgentId != 0)
		ostr << ",\"代理商Id(AgentId)\":\"" << value->AgentId << '\"';
	if (strlen(value->AgentShortName) > 0)
		ostr << ",\"代理商简称(AgentShortName)\":\"" << value->AgentShortName << '\"';
	if (strlen(value->TreeName) > 0)
		ostr << ",\"树形全称(TreeName)\":\"" << value->TreeName << '\"';
	if (strlen(value->AgentNo) > 0)
		ostr << ",\"代理商编码(AgentNo)\":\"" << value->AgentNo << '\"';
	if (strlen(value->AgentFullName) > 0)
		ostr << ",\"代理商全称(AgentFullName)\":\"" << value->AgentFullName << '\"';
	if (value->AgentType != 0)
		ostr << ",\"代理类型(AgentType)\":\"" << value->AgentType << '\"';
	if (value->AgentLevel != 0)
		ostr << ",\"代理商级别(AgentLevel)\":\"" << value->AgentLevel << '\"';
	if (value->Rebate != 0)
		ostr << ",\"折扣(Rebate)\":\"" << value->Rebate << '\"';
	ostr << ",\"注册时间(RegisteDate)\":" << to_log_text(value->RegisteDate);
	if (value->Credit != 0)
		ostr << ",\"信用等级(Credit)\":\"" << value->Credit << '\"';
	if (value->ParentId != 0)
		ostr << ",\"上级代理商ID(ParentId)\":\"" << value->ParentId << '\"';
	ostr.put('}');
	return ostr.str();
}
///代理商 记录到日志
void log(const Agent* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///代理商客户信息 生成日志文本
string to_log_text(const AgentCustomer* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->CustomerId != 0)
		ostr << ",\"客户ID(CustomerId)\":\"" << value->CustomerId << '\"';
	if (strlen(value->CustomerNo) > 0)
		ostr << ",\"客户编码(CustomerNo)\":\"" << value->CustomerNo << '\"';
	if (value->AgentId != 0)
		ostr << ",\"代理商Id(AgentId)\":\"" << value->AgentId << '\"';
	if (strlen(value->RealName) > 0)
		ostr << ",\"姓名(RealName)\":\"" << value->RealName << '\"';
	if (value->CustomerType != Agebull::Futures::Globals::CustomerTypeClassify::None)
		ostr << ",\"客户类型(CustomerType)\":\"" << to_log_text(value->CustomerType) << '\"';
	if (strlen(value->LoginName) > 0)
		ostr << ",\"登录名称(LoginName)\":\"" << value->LoginName << '\"';
	if (strlen(value->Telephone) > 0)
		ostr << ",\"电话(Telephone)\":\"" << value->Telephone << '\"';
	ostr << ",\"证件类型(IdType)\":" << to_log_text(value->IdType);
	if (strlen(value->IdCode) > 0)
		ostr << ",\"证件号码(IdCode)\":\"" << value->IdCode << '\"';
	ostr << ",\"注册时间(RegisteDate)\":" << to_log_text(value->RegisteDate);
	if (value->Credit != 0)
		ostr << ",\"信用等级(Credit)\":\"" << value->Credit << '\"';
	if (strlen(value->Email) > 0)
		ostr << ",\"电子邮件(Email)\":\"" << value->Email << '\"';
	if (strlen(value->QQ) > 0)
		ostr << ",\"Q(QQ)\":\"" << value->QQ << '\"';
	if (strlen(value->Addr) > 0)
		ostr << ",\"地址(Addr)\":\"" << value->Addr << '\"';
	if (strlen(value->BankNo) > 0)
		ostr << ",\"银行卡号(BankNo)\":\"" << value->BankNo << '\"';
	if (strlen(value->BankName) > 0)
		ostr << ",\"银行名称(BankName)\":\"" << value->BankName << '\"';
	if (strlen(value->BranchBank) > 0)
		ostr << ",\"开户行(BranchBank)\":\"" << value->BranchBank << '\"';
	if (value->ExpireDate != 0)
		ostr << ",\"到期日(ExpireDate)\":\"" << value->ExpireDate << '\"';
	if (strlen(value->Remark) > 0)
		ostr << ",\"备注(Remark)\":\"" << value->Remark << '\"';
	if (strlen(value->SettleAccount) > 0)
		ostr << ",\"结算(SettleAccount)\":\"" << value->SettleAccount << '\"';
	
	ostr.put('}');
	return ostr.str();
}
///代理商客户信息 记录到日志
void log(const AgentCustomer* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///代理商隶属关系 生成日志文本
string to_log_text(const AgentSubAgent* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->ID != 0)
		ostr << ",\"身份证件(ID)\":\"" << value->ID << '\"';
	if (value->MasterId != 0)
		ostr << ",\"上级代理商ID(MasterId)\":\"" << value->MasterId << '\"';
	if (value->SlaveId != 0)
		ostr << ",\"有隶属关系的代理商ID(SlaveId)\":\"" << value->SlaveId << '\"';
	ostr.put('}');
	return ostr.str();
}
///代理商隶属关系 记录到日志
void log(const AgentSubAgent* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}


///商品 生成日志文本
string to_log_text(const Commodity* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->CommdityId != 0)
		ostr << ",\"商品ID(CommdityId)\":\"" << value->CommdityId << '\"';
	if (strlen(value->CommodityName) > 0)
		ostr << ",\"商品名称(CommodityName)\":\"" << value->CommodityName << '\"';
	
	if (strlen(value->ExchangeNo) > 0)
		ostr << ",\"交易所(ExchangeNo)\":\"" << value->ExchangeNo << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	
	if (value->Poundage != 0)
		ostr << ",\"手续费(Poundage)\":\"" << Int64ToDouble(value->Poundage) << '\"';
	if (value->Deposit != 0)
		ostr << ",\"保证金(Deposit)\":\"" << Int64ToDouble(value->Deposit) << '\"';
	
	
	if (value->ProductDot != 0)
		ostr << ",\"每手乘数(ProductDot)\":\"" << Int64ToDouble(value->ProductDot) << '\"';
	if (value->UpperTick != 0)
		ostr << ",\"最小变动价分子(UpperTick)\":\"" << Int64ToDouble(value->UpperTick) << '\"';
	if (value->LowerTick != 0)
		ostr << ",\"最小变动价分母(LowerTick)\":\"" << value->LowerTick << '\"';
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"商品使用币种(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	
	if (value->MaxSingleOrderVol != 0)
		ostr << ",\"单笔最大下单量(MaxSingleOrderVol)\":\"" << value->MaxSingleOrderVol << '\"';
	if (value->MaxHoldVol != 0)
		ostr << ",\"最大持仓量(MaxHoldVol)\":\"" << value->MaxHoldVol << '\"';
	if (strlen(value->AddOneTime) > 0)
		ostr << ",\"T+1时间(AddOneTime)\":\"" << value->AddOneTime << '\"';
	ostr << ",\"平仓方式(CoverMode)\":" << to_log_text(value->CoverMode);
	ostr.put('}');
	return ostr.str();
}
///商品 记录到日志
void log(const Commodity* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///合约 生成日志文本
string to_log_text(const Contract* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->ContractId != 0)
		ostr << ",\"合约ID(ContractId)\":\"" << value->ContractId << '\"';
	if (strlen(value->ContractName) > 0)
		ostr << ",\"合约名称(ContractName)\":\"" << value->ContractName << '\"';
	
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合约(ContractNo)\":\"" << value->ContractNo << '\"';
	if (strlen(value->ExpiryDate) > 0)
		ostr << ",\"到期日(ExpiryDate)\":\"" << value->ExpiryDate << '\"';
	if (strlen(value->LastTradeDate) > 0)
		ostr << ",\"最后交易日(LastTradeDate)\":\"" << value->LastTradeDate << '\"';
	if (strlen(value->FirstNoticeDate) > 0)
		ostr << ",\"首次通知日(FirstNoticeDate)\":\"" << value->FirstNoticeDate << '\"';
	ostr.put('}');
	return ostr.str();
}
///合约 记录到日志
void log(const Contract* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}


///币种 生成日志文本
string to_log_text(const Currency* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->CurrencyId != 0)
		ostr << ",\"货币ID(CurrencyId)\":\"" << value->CurrencyId << '\"';
	if (strlen(value->CurrencyName) > 0)
		ostr << ",\"货币名称(CurrencyName)\":\"" << value->CurrencyName << '\"';
	if (strlen(value->SettlementCurrency) > 0)
		ostr << ",\"结算基币(SettlementCurrency)\":\"" << value->SettlementCurrency << '\"';
	if (value->SettlementCurrencyId != 0)
		ostr << ",\"结算基币Id(SettlementCurrencyId)\":\"" << value->SettlementCurrencyId << '\"';
	
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编码(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	
	if (value->ExchangeRate != 0)
		ostr << ",\"兑换汇率(ExchangeRate)\":\"" << Int64ToDouble(value->ExchangeRate) << '\"';
	ostr.put('}');
	return ostr.str();
}
///币种 记录到日志
void log(const Currency* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///交易所 生成日志文本
string to_log_text(const Exchange* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->ExchangeID != 0)
		ostr << ",\"交易所ID(ExchangeID)\":\"" << value->ExchangeID << '\"';
	if (strlen(value->ExchangeName) > 0)
		ostr << ",\"交易所名称(ExchangeName)\":\"" << value->ExchangeName << '\"';
	if (strlen(value->ExchangeNo) > 0)
		ostr << ",\"交易所(ExchangeNo)\":\"" << value->ExchangeNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///交易所 记录到日志
void log(const Exchange* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}


///行情 生成日志文本
string to_log_text(const QuoteWhole* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->QuoteWholeId != 0)
		ostr << ",\"行情ID(QuoteWholeId)\":\"" << value->QuoteWholeId << '\"';
	if (value->ExchangeID != 0)
		ostr << ",\"交易所ID(ExchangeID)\":\"" << value->ExchangeID << '\"';
	if (strlen(value->ExchangeNo) > 0)
		ostr << ",\"交易所(ExchangeNo)\":\"" << value->ExchangeNo << '\"';
	if (value->ContractId != 0)
		ostr << ",\"合约ID(ContractId)\":\"" << value->ContractId << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合约(ContractNo)\":\"" << value->ContractNo << '\"';
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"币种编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';

	if (value->CommdityId != 0)
		ostr << ",\"商品ID(CommdityId)\":\"" << value->CommdityId << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->DateTimeStamp) > 0)
		ostr << ",\"时间戳(DateTimeStamp)\":\"" << value->DateTimeStamp << '\"';
	if (value->QPreClosingPrice != 0)
		ostr << ",\"昨收盘价(QPreClosingPrice)\":\"" << Int64ToDouble(value->QPreClosingPrice) << '\"';
	if (value->QPreSettlePrice != 0)
		ostr << ",\"昨结算价(QPreSettlePrice)\":\"" << Int64ToDouble(value->QPreSettlePrice) << '\"';
	if (value->QPrePositionQty != 0)
		ostr << ",\"昨持仓量(QPrePositionQty)\":\"" << value->QPrePositionQty << '\"';
	if (value->QOpeningPrice != 0)
		ostr << ",\"开盘价(QOpeningPrice)\":\"" << Int64ToDouble(value->QOpeningPrice) << '\"';
	if (value->QLastPrice != 0)
		ostr << ",\"最新价(QLastPrice)\":\"" << Int64ToDouble(value->QLastPrice) << '\"';
	if (value->QHighPrice != 0)
		ostr << ",\"最高价(QHighPrice)\":\"" << Int64ToDouble(value->QHighPrice) << '\"';
	if (value->QLowPrice != 0)
		ostr << ",\"最低价(QLowPrice)\":\"" << Int64ToDouble(value->QLowPrice) << '\"';
	if (value->QHisHighPrice != 0)
		ostr << ",\"历史最高价(QHisHighPrice)\":\"" << Int64ToDouble(value->QHisHighPrice) << '\"';
	if (value->QHisLowPrice != 0)
		ostr << ",\"历史最低价(QHisLowPrice)\":\"" << Int64ToDouble(value->QHisLowPrice) << '\"';
	if (value->QLimitUpPrice != 0)
		ostr << ",\"涨停价(QLimitUpPrice)\":\"" << Int64ToDouble(value->QLimitUpPrice) << '\"';
	if (value->QLimitDownPrice != 0)
		ostr << ",\"跌停价(QLimitDownPrice)\":\"" << Int64ToDouble(value->QLimitDownPrice) << '\"';
	if (value->QTotalQty != 0)
		ostr << ",\"当日总成交量(QTotalQty)\":\"" << value->QTotalQty << '\"';
	if (value->QTotalTurnover != 0)
		ostr << ",\"当日成交金额(QTotalTurnover)\":\"" << Int64ToDouble(value->QTotalTurnover) << '\"';
	if (value->QPositionQty != 0)
		ostr << ",\"持仓量(QPositionQty)\":\"" << value->QPositionQty << '\"';
	if (value->QAveragePrice != 0)
		ostr << ",\"均价(QAveragePrice)\":\"" << Int64ToDouble(value->QAveragePrice) << '\"';
	if (value->QClosingPrice != 0)
		ostr << ",\"收盘价(QClosingPrice)\":\"" << Int64ToDouble(value->QClosingPrice) << '\"';
	if (value->QSettlePrice != 0)
		ostr << ",\"结算价(QSettlePrice)\":\"" << Int64ToDouble(value->QSettlePrice) << '\"';
	if (value->QLastQty != 0)
		ostr << ",\"最新成交量(QLastQty)\":\"" << value->QLastQty << '\"';
	ostr << ",\"买价1-20档(QBidPrice)\":[";
	int idx;
	for (idx = 0;idx < 19;idx++)
	{
		ostr << Int64ToDouble(value->QBidPrice[idx]) << ',';
	}
	ostr << value->QBidPrice[idx] << ']';
	ostr << ",\"买量1-20档(QBidQty)\":[";
	for (idx = 0;idx < 19;idx++)
	{
		ostr << value->QBidQty[idx] << ',';
	}
	ostr << value->QBidQty[idx] << ']';
	ostr << ",\"卖价1-20档(QAskPrice)\":[";
	for (idx = 0;idx < 19;idx++)
	{
		ostr << Int64ToDouble(value->QAskPrice[idx]) << ',';
	}
	ostr << value->QAskPrice[idx] << ']';
	ostr << ",\"卖量1-20档(QAskQty)\":[";
	for (idx = 0;idx < 19;idx++)
	{
		ostr << value->QAskQty[idx] << ',';
	}
	ostr << value->QAskQty[idx] << ']';
	if (value->QImpliedBidPrice != 0)
		ostr << ",\"隐含买价(QImpliedBidPrice)\":\"" << Int64ToDouble(value->QImpliedBidPrice) << '\"';
	if (value->QImpliedBidQty != 0)
		ostr << ",\"隐含买量(QImpliedBidQty)\":\"" << value->QImpliedBidQty << '\"';
	if (value->QImpliedAskPrice != 0)
		ostr << ",\"隐含卖价(QImpliedAskPrice)\":\"" << Int64ToDouble(value->QImpliedAskPrice) << '\"';
	if (value->QImpliedAskQty != 0)
		ostr << ",\"隐含卖量(QImpliedAskQty)\":\"" << value->QImpliedAskQty << '\"';
	if (value->QPreDelta != 0)
		ostr << ",\"昨虚实度(QPreDelta)\":\"" << Int64ToDouble(value->QPreDelta) << '\"';
	if (value->QCurrDelta != 0)
		ostr << ",\"今虚实度(QCurrDelta)\":\"" << Int64ToDouble(value->QCurrDelta) << '\"';
	if (value->QInsideQty != 0)
		ostr << ",\"内盘量(QInsideQty)\":\"" << value->QInsideQty << '\"';
	if (value->QOutsideQty != 0)
		ostr << ",\"外盘量(QOutsideQty)\":\"" << value->QOutsideQty << '\"';
	if (value->QTurnoverRate != 0)
		ostr << ",\"换手率(QTurnoverRate)\":\"" << Int64ToDouble(value->QTurnoverRate) << '\"';
	if (value->Q5DAvgQty != 0)
		ostr << ",\"五日均量(Q5DAvgQty)\":\"" << value->Q5DAvgQty << '\"';
	if (value->QPERatio != 0)
		ostr << ",\"市盈率(QPERatio)\":\"" << Int64ToDouble(value->QPERatio) << '\"';
	if (value->QTotalValue != 0)
		ostr << ",\"总市值(QTotalValue)\":\"" << Int64ToDouble(value->QTotalValue) << '\"';
	if (value->QNegotiableValue != 0)
		ostr << ",\"流通市值(QNegotiableValue)\":\"" << Int64ToDouble(value->QNegotiableValue) << '\"';
	if (value->QPositionTrend != 0)
		ostr << ",\"持仓走势(QPositionTrend)\":\"" << value->QPositionTrend << '\"';
	if (value->QChangeSpeed != 0)
		ostr << ",\"涨速(QChangeSpeed)\":\"" << Int64ToDouble(value->QChangeSpeed) << '\"';
	if (value->QChangeRate != 0)
		ostr << ",\"涨幅(QChangeRate)\":\"" << Int64ToDouble(value->QChangeRate) << '\"';
	if (value->QChangeValue != 0)
		ostr << ",\"涨跌值(QChangeValue)\":\"" << Int64ToDouble(value->QChangeValue) << '\"';
	if (value->QSwing != 0)
		ostr << ",\"振幅(QSwing)\":\"" << Int64ToDouble(value->QSwing) << '\"';
	if (value->QTotalBidQty != 0)
		ostr << ",\"委买总量(QTotalBidQty)\":\"" << value->QTotalBidQty << '\"';
	if (value->QTotalAskQty != 0)
		ostr << ",\"委卖总量(QTotalAskQty)\":\"" << value->QTotalAskQty << '\"';
	ostr.put('}');
	return ostr.str();
}
///行情 记录到日志
void log(const QuoteWhole* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///客户资金信息 生成日志文本
string to_log_text(const CustomFunds* value)
{
	return string();
}
///客户资金信息 记录到日志
void log(const CustomFunds* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///资金基本信息 生成日志文本
string to_log_text(const FundBase* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->CustomerId != 0)
		ostr << ",\"客户ID(CustomerId)\":\"" << value->CustomerId << '\"';
	if (value->AccountMoney != 0)
		ostr << ",\"账面资金(AccountMoney)\":\"" << Int64ToDouble(value->AccountMoney) << '\"';
	if (value->FrozenFee != 0)
		ostr << ",\"冻结手续费(FrozenFee)\":\"" << Int64ToDouble(value->FrozenFee) << '\"';
	if (value->FrozenDeposit != 0)
		ostr << ",\"冻结保证金(FrozenDeposit)\":\"" << Int64ToDouble(value->FrozenDeposit) << '\"';
	if (value->Fee != 0)
		ostr << ",\"手续费(Fee)\":\"" << Int64ToDouble(value->Fee) << '\"';
	if (value->Deposit != 0)
		ostr << ",\"保证金(Deposit)\":\"" << Int64ToDouble(value->Deposit) << '\"';
	if (value->HoldProfit != 0)
		ostr << ",\"持仓浮盈(HoldProfit)\":\"" << Int64ToDouble(value->HoldProfit) << '\"';
	if (value->Profit != 0)
		ostr << ",\"已结浮盈(Profit)\":\"" << Int64ToDouble(value->Profit) << '\"';
	ostr.put('}');
	return ostr.str();
}
///资金基本信息 记录到日志
void log(const FundBase* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}


///文本的参数 生成日志文本
string to_log_text(const StringArgument* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->Argument) > 0)
		ostr << ",\"参数(Argument)\":\"" << value->Argument << '\"';
	ostr.put('}');
	return ostr.str();
}
///文本的参数 记录到日志
void log(const StringArgument* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///持仓 生成日志文本
string to_log_text(const CustomerHold* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->HoldId != 0)
		ostr << ",\"持仓ID(HoldId)\":\"" << value->HoldId << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合约(ContractNo)\":\"" << value->ContractNo << '\"';
	if (value->ContractId != 0)
		ostr << ",\"合约ID(ContractId)\":\"" << value->ContractId << '\"';
	if (value->CustomerId != 0)
		ostr << ",\"客户ID(CustomerId)\":\"" << value->CustomerId << '\"';
	if (strlen(value->CustomerName) > 0)
		ostr << ",\"客户号(CustomerName)\":\"" << value->CustomerName << '\"';
	if (value->CommdityId != 0)
		ostr << ",\"商品ID(CommdityId)\":\"" << value->CommdityId << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (value->HoldKeyId != 0)
		ostr << ",\"持仓关键字未使用(HoldKeyId)\":\"" << value->HoldKeyId << '\"';
	if (strlen(value->EsMatchNo) > 0)
		ostr << ",\"易盛成交号(EsMatchNo)\":\"" << value->EsMatchNo << '\"';
	char stm[42];
	time2string(stm, value->MatchDateTime);
	ostr << ",\"成交时间(MatchDateTime)\":\"" << stm << '\"';
	ostr << ",\"买卖方向(Direct)\":" << to_log_text(value->Direct);
	ostr << ",\"投机保值未使用(Hedge)\":" << to_log_text(value->Hedge);
	if (value->HoldProfit != 0)
		ostr << ",\"持仓浮盈(HoldProfit)\":\"" << Int64ToDouble(value->HoldProfit) << '\"';
	if (value->TradePrice != 0)
		ostr << ",\"持仓均价(TradePrice)\":\"" << Int64ToDouble(value->TradePrice) << '\"';
	if (value->TradeVol != 0)
		ostr << ",\"持仓量(TradeVol)\":\"" << value->TradeVol << '\"';
	if (value->Poundage != 0)
		ostr << ",\"手续费(Poundage)\":\"" << Int64ToDouble(value->Poundage) << '\"';
	if (value->Deposit != 0)
		ostr << ",\"保证金(Deposit)\":\"" << Int64ToDouble(value->Deposit) << '\"';
	if (value->KeepDeposit != 0)
		ostr << ",\"维持保证金(KeepDeposit)\":\"" << Int64ToDouble(value->KeepDeposit) << '\"';
	if (value->YSettlePrice != 0)
		ostr << ",\"昨结算价(YSettlePrice)\":\"" << Int64ToDouble(value->YSettlePrice) << '\"';
	if (value->NewPrice != 0)
		ostr << ",\"最新价(NewPrice)\":\"" << Int64ToDouble(value->NewPrice) << '\"';
	ostr.put('}');
	return ostr.str();
}
///持仓 记录到日志
void log(const CustomerHold* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///客户委托 生成日志文本
string to_log_text(const CustomerOrder* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OrderId != 0)
		ostr << ",\"客户委托ID(OrderId)\":\"" << value->OrderId << '\"';
	if (value->CustomerId != 0)
		ostr << ",\"客户标识(CustomerId)\":\"" << value->CustomerId << '\"';
	if (strlen(value->CustomerName) > 0)
		ostr << ",\"客户名称(CustomerName)\":\"" << value->CustomerName << '\"';
	if (strlen(value->LocalCustomerId) > 0)
		ostr << ",\"客户标识(LocalCustomerId)\":\"" << value->LocalCustomerId << '\"';
	if (strlen(value->EsLocalNo) > 0)
		ostr << ",\"易盛本地号(EsLocalNo)\":\"" << value->EsLocalNo << '\"';
	if (value->EsOrderId != 0)
		ostr << ",\"易盛委托号(EsOrderId)\":\"" << value->EsOrderId << '\"';
	if (value->EsOrderStreamId != 0)
		ostr << ",\"易盛委托流号(EsOrderStreamId)\":\"" << value->EsOrderStreamId << '\"';
	if (strlen(value->EsSystemNo) > 0)
		ostr << ",\"易盛系统号(EsSystemNo)\":\"" << value->EsSystemNo << '\"';
	if (strlen(value->EsActionLocalNo) > 0)
		ostr << ",\"易盛报单操作的本地号(EsActionLocalNo)\":\"" << value->EsActionLocalNo << '\"';
	if (strlen(value->ExchangeSystemNo) > 0)
		ostr << ",\"交易所系统号(ExchangeSystemNo)\":\"" << value->ExchangeSystemNo << '\"';
	if (strlen(value->EsParentSystemNo) > 0)
		ostr << ",\"父系统编号(EsParentSystemNo)\":\"" << value->EsParentSystemNo << '\"';
	if (value->ContractId != 0)
		ostr << ",\"合约ID(ContractId)\":\"" << value->ContractId << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合约代码(ContractNo)\":\"" << value->ContractNo << '\"';
	if (value->IsCancel != 0)
		ostr << ",\"是否辙单(IsCancel)\":\"" << value->IsCancel << '\"';
	if (value->CommdityId != 0)
		ostr << ",\"商品ID(CommdityId)\":\"" << value->CommdityId << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品代码(CommodityNo)\":\"" << value->CommodityNo << '\"';
	ostr << ",\"委托时间(Date)\":" << to_log_text(value->Date);
	ostr << ",\"委托类型(OrderType)\":" << to_log_text(value->OrderType);
	ostr << ",\"委托方式(OrderWay)\":" << to_log_text(value->OrderWay);
	ostr << ",\"委托模式(OrderMode)\":" << to_log_text(value->OrderMode);

	char stm[42];
	time2string(stm, value->ValidDateTime);
	ostr << ",\"有效日期(ValidDateTime)\":\"" << stm << '\"';
	time2string(stm, value->UpdateDateTime);
	ostr << ",\"操作时间(UpdateDateTime)\":\"" << stm << '\"';
	ostr << ",\"风险报单(IsRiskOrder)\":" << to_log_text(value->IsRiskOrder);
	ostr << ",\"委托状态(OrderState)\":" << to_log_text(value->OrderState);
	ostr << ",\"买入卖出(Direct)\":" << to_log_text(value->Direct);
	ostr << ",\"开仓平仓(Offset)\":" << to_log_text(value->Offset);
	ostr << ",\"投机保值(Hedge)\":" << to_log_text(value->Hedge);
	if (value->OrderPrice != 0)
		ostr << ",\"委托价格(OrderPrice)\":\"" << Int64ToDouble(value->OrderPrice) << '\"';
	if (value->TriggerPrice != 0)
		ostr << ",\"触发价格(TriggerPrice)\":\"" << Int64ToDouble(value->TriggerPrice) << '\"';
	if (value->OrderVol != 0)
		ostr << ",\"委托数量(OrderVol)\":\"" << value->OrderVol << '\"';
	if (value->MinMatchVol != 0)
		ostr << ",\"最小成交量(MinMatchVol)\":\"" << value->MinMatchVol << '\"';
	ostr << ",\"冰山单是否随机量发出(Randomise)\":" << to_log_text(value->Randomise);
	if (value->MinClipSize != 0)
		ostr << ",\"冰山单最小随机量(MinClipSize)\":\"" << value->MinClipSize << '\"';
	if (value->MaxClipSize != 0)
		ostr << ",\"冰山单最大随机量(MaxClipSize)\":\"" << value->MaxClipSize << '\"';
	if (value->MatchValue != 0)
		ostr << ",\"成交数量(MatchValue)\":\"" << value->MatchValue << '\"';
	if (value->CancelValue != 0)
		ostr << ",\"辙单数量(CancelValue)\":\"" << value->CancelValue << '\"';
	if (value->FrozenFee != 0)
		ostr << ",\"冻结手续费(FrozenFee)\":\"" << Int64ToDouble(value->FrozenFee) << '\"';
	if (value->FrozenDeposit != 0)
		ostr << ",\"冻结保证金(FrozenDeposit)\":\"" << Int64ToDouble(value->FrozenDeposit) << '\"';
	if (value->Fee != 0)
		ostr << ",\"手续费(Fee)\":\"" << Int64ToDouble(value->Fee) << '\"';
	if (value->Deposit != 0)
		ostr << ",\"保证金(Deposit)\":\"" << Int64ToDouble(value->Deposit) << '\"';
	ostr.put('}');
	return ostr.str();
}
///客户委托 记录到日志
void log(const CustomerOrder* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///客户成交记录 生成日志文本
string to_log_text(const CustomMatch* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->MatchId != 0)
		ostr << ",\"成交记录ID(MatchId)\":\"" << value->MatchId << '\"';
	if (value->HoldId != 0)
		ostr << ",\"持仓ID(HoldId)\":\"" << value->HoldId << '\"';
	if (value->OrderId != 0)
		ostr << ",\"客户委托ID(OrderId)\":\"" << value->OrderId << '\"';
	if (strlen(value->CustomerName) > 0)
		ostr << ",\"客户姓名(CustomerName)\":\"" << value->CustomerName << '\"';
	if (value->CustomerId != 0)
		ostr << ",\"客户标识(CustomerId)\":\"" << value->CustomerId << '\"';
	if (value->ContractId != 0)
		ostr << ",\"合约ID(ContractId)\":\"" << value->ContractId << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合约(ContractNo)\":\"" << value->ContractNo << '\"';
	if (value->CommdityId != 0)
		ostr << ",\"商品ID(CommdityId)\":\"" << value->CommdityId << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"币种(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	if (strlen(value->CommodityCurrencyNo) > 0)
		ostr << ",\"品种币种(CommodityCurrencyNo)\":\"" << value->CommodityCurrencyNo << '\"';
	ostr << ",\"成交模式(MatchMode)\":" << to_log_text(value->MatchMode);
	ostr << ",\"委托类型(OrderType)\":" << to_log_text(value->OrderType);
	ostr << ",\"风险报单(IsRiskOrder)\":" << to_log_text(value->IsRiskOrder);
	ostr << ",\"删除标志(Deleted)\":" << to_log_text(value->Deleted);
	ostr << ",\"成交方式(MatchWay)\":" << to_log_text(value->MatchWay);
	ostr << ",\"买卖方向(Direct)\":" << to_log_text(value->Direct);
	ostr << ",\"开平类型(Offset)\":" << to_log_text(value->Offset);
	if (value->MatchVol != 0)
		ostr << ",\"成交数量(MatchVol)\":\"" << value->MatchVol << '\"';
	if (value->MatchPrice != 0)
		ostr << ",\"成交价格(MatchPrice)\":\"" << Int64ToDouble(value->MatchPrice) << '\"';
	if (value->Turnover != 0)
		ostr << ",\"成交金额(Turnover)\":\"" << Int64ToDouble(value->Turnover) << '\"';
	if (value->ClientFee != 0)
		ostr << ",\"客户手续费未使用(ClientFee)\":\"" << Int64ToDouble(value->ClientFee) << '\"';
	if (strlen(value->GroupNo) > 0)
		ostr << ",\"结算分组(GroupNo)\":\"" << value->GroupNo << '\"';
	ostr << ",\"手续费类型(ManualFee)\":" << to_log_text(value->ManualFee);
	if (value->Poundage != 0)
		ostr << ",\"手续费(Poundage)\":\"" << Int64ToDouble(value->Poundage) << '\"';
	if (value->Deposit != 0)
		ostr << ",\"保证金(Deposit)\":\"" << Int64ToDouble(value->Deposit) << '\"';

	char stm[42];
	time2string(stm, value->MatchDateTime);
	ostr << ",\"成交时间(MatchDateTime)\":\"" << stm << '\"';
	if (value->Premium != 0)
		ostr << ",\"溢价(Premium)\":\"" << Int64ToDouble(value->Premium) << '\"';
	if (value->CoverPrice != 0)
		ostr << ",\"平仓价格(CoverPrice)\":\"" << Int64ToDouble(value->CoverPrice) << '\"';
	if (value->CoverProfit != 0)
		ostr << ",\"平仓盈亏(CoverProfit)\":\"" << Int64ToDouble(value->CoverProfit) << '\"';
	time2string(stm, value->Date);
	ostr << ",\"结算日期(Date)\":\"" << stm << '\"';
	if (value->EsOrderId != 0)
		ostr << ",\"易盛委托ID(EsOrderId)\":\"" << value->EsOrderId << '\"';
	if (value->EsMatchId != 0)
		ostr << ",\"易盛成交ID(EsMatchId)\":\"" << value->EsMatchId << '\"';
	if (strlen(value->EsSettleNo) > 0)
		ostr << ",\"易盛结算用成交编号(EsSettleNo)\":\"" << value->EsSettleNo << '\"';
	if (strlen(value->EsSystemNo) > 0)
		ostr << ",\"易盛系统号(EsSystemNo)\":\"" << value->EsSystemNo << '\"';
	if (strlen(value->EsMatchNo) > 0)
		ostr << ",\"易盛成交号(EsMatchNo)\":\"" << value->EsMatchNo << '\"';
	if (value->EsMatchStreamID != 0)
		ostr << ",\"易盛成交流号(EsMatchStreamID)\":\"" << value->EsMatchStreamID << '\"';
	ostr.put('}');
	return ostr.str();
}
///客户成交记录 记录到日志
void log(const CustomMatch* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

#ifdef EsTrade

///查询合约命令调用参数 生成日志文本
string to_log_text(const ContractQueryArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->LastDays != 0)
		ostr << ",\"临近日期数(LastDays)\":\"" << value->LastDays << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///查询合约命令调用参数 记录到日志
void log(const ContractQueryArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}
///交易员下属客户信息数据 生成日志文本
string to_log_text(const OperatorClientQueryRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OperatorClientQueryRspId != 0)
		ostr << ",\"交易员下属客户信息查询应答ID(OperatorClientQueryRspId)\":\"" << value->OperatorClientQueryRspId << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///交易员下属客户信息数据 记录到日志
void log(const OperatorClientQueryRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}
///商品状态变化数据 生成日志文本
string to_log_text(const CommodityStateModNotice* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->CommodityStateModNoticeId != 0)
		ostr << ",\"商品状态变化通知ID(CommodityStateModNoticeId)\":\"" << value->CommodityStateModNoticeId << '\"';
	ostr << ",\"商品状态(CommodityState)\":" << to_log_text(value->CommodityState);
	if (value->MaxSingleOrderVol != 0)
		ostr << ",\"单笔最大下单量(MaxSingleOrderVol)\":\"" << value->MaxSingleOrderVol << '\"';
	if (value->MaxHoldVol != 0)
		ostr << ",\"最大持仓量(MaxHoldVol)\":\"" << value->MaxHoldVol << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"上边三个字段为商品关键字(CommodityNo)\":\"" << value->CommodityNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///商品状态变化数据 记录到日志
void log(const CommodityStateModNotice* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///币种汇率变化数据 生成日志文本
string to_log_text(const ExchangeRateModifyNotice* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->ExchangeRateModifyNoticeId != 0)
		ostr << ",\"币种汇率变化通知ID(ExchangeRateModifyNoticeId)\":\"" << value->ExchangeRateModifyNoticeId << '\"';
	if (value->ExchangeRate != 0)
		ostr << ",\"汇率(ExchangeRate)\":\"" << Int64ToDouble(value->ExchangeRate) << '\"';
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///币种汇率变化数据 记录到日志
void log(const ExchangeRateModifyNotice* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///市场状态变更数据 生成日志文本
string to_log_text(const ExchangeStateModifyNotice* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->ExchangeStateModifyNoticeId != 0)
		ostr << ",\"市场状态变更推送通知ID(ExchangeStateModifyNoticeId)\":\"" << value->ExchangeStateModifyNoticeId << '\"';
	ostr << ",\"交换状态(ExchangeState)\":" << to_log_text(value->ExchangeState);
	if (strlen(value->ExchangeNo) > 0)
		ostr << ",\"交换不(ExchangeNo)\":\"" << value->ExchangeNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///市场状态变更数据 记录到日志
void log(const ExchangeStateModifyNotice* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}
///客户计算参数查询命令调用参数 生成日志文本
string to_log_text(const TClientCountRentQueryArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"匹配方式(MatchWay)\":" << to_log_text(value->MatchWay);
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品编号(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"可填空(ContractNo)\":\"" << value->ContractNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///客户计算参数查询命令调用参数 记录到日志
void log(const TClientCountRentQueryArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///查询计算参数应答 生成日志文本
string to_log_text(const TClientCountRentQueryRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->TClientCountRentQueryRspId != 0)
		ostr << ",\"查询计算参数应答ID(TClientCountRentQueryRspId)\":\"" << value->TClientCountRentQueryRspId << '\"';
	if (value->BFee != 0)
		ostr << ",\"B费(BFee)\":\"" << Int64ToDouble(value->BFee) << '\"';
	if (value->DFee != 0)
		ostr << ",\"D费用(DFee)\":\"" << Int64ToDouble(value->DFee) << '\"';
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	ostr << ",\"仅比例和定额(DepositMode)\":" << to_log_text(value->DepositMode);
	if (value->Deposit != 0)
		ostr << ",\"正常保证金(Deposit)\":\"" << Int64ToDouble(value->Deposit) << '\"';
	if (value->LockDeposit != 0)
		ostr << ",\"锁仓保证金(LockDeposit)\":\"" << Int64ToDouble(value->LockDeposit) << '\"';
	if (value->KeepDeposit != 0)
		ostr << ",\"维持保证金(KeepDeposit)\":\"" << Int64ToDouble(value->KeepDeposit) << '\"';
	ostr << ",\"请求数据(ReqData)\":" << to_log_text(&value->ReqData);
	ostr.put('}');
	return ostr.str();
}
///查询计算参数应答 记录到日志
void log(const TClientCountRentQueryRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///资金调整数据 生成日志文本
string to_log_text(const AdjustQueryRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->AdjustQueryRspId != 0)
		ostr << ",\"资金调整查询应答ID(AdjustQueryRspId)\":\"" << value->AdjustQueryRspId << '\"';
	ostr << ",\"资金调整状态(AdjustState)\":" << to_log_text(value->AdjustState);
	if (strlen(value->OperDateTime) > 0)
		ostr << ",\"操作时间(OperDateTime)\":\"" << value->OperDateTime << '\"';
	if (strlen(value->OperatorNo) > 0)
		ostr << ",\"操作人(OperatorNo)\":\"" << value->OperatorNo << '\"';
	if (strlen(value->CheckDateTime) > 0)
		ostr << ",\"审核时间(CheckDateTime)\":\"" << value->CheckDateTime << '\"';
	if (strlen(value->CheckOperatorNo) > 0)
		ostr << ",\"审核人(CheckOperatorNo)\":\"" << value->CheckOperatorNo << '\"';
	if (value->AdjustType != 0)
		ostr << ",\"资金调整类型(AdjustType)\":\"" << value->AdjustType << '\"';
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	if (value->AdjustValue != 0)
		ostr << ",\"操作金额(AdjustValue)\":\"" << Int64ToDouble(value->AdjustValue) << '\"';
	if (strlen(value->AdjustRemark) > 0)
		ostr << ",\"备注(AdjustRemark)\":\"" << value->AdjustRemark << '\"';
	if (strlen(value->ClientBank) > 0)
		ostr << ",\"银行标识(ClientBank)\":\"" << value->ClientBank << '\"';
	if (strlen(value->ClientAccount) > 0)
		ostr << ",\"银行账户(ClientAccount)\":\"" << value->ClientAccount << '\"';
	ostr << ",\"本外币账户标识(ClientLWFlag)\":" << to_log_text(value->ClientLWFlag);
	if (strlen(value->CompanyBank) > 0)
		ostr << ",\"银行标识(CompanyBank)\":\"" << value->CompanyBank << '\"';
	if (strlen(value->CompanyAccount) > 0)
		ostr << ",\"银行账户(CompanyAccount)\":\"" << value->CompanyAccount << '\"';
	ostr << ",\"本外币账户标识(CompanyLWFlag)\":" << to_log_text(value->CompanyLWFlag);
	if (value->SerialId != 0)
		ostr << ",\"出入金流水号(SerialId)\":\"" << value->SerialId << '\"';
	ostr.put('}');
	return ostr.str();
}
///资金调整数据 记录到日志
void log(const AdjustQueryRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///出金入金审核请求命令调用参数 生成日志文本
string to_log_text(const CashCheckArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->SerialId != 0)
		ostr << ",\"出入金流水号(SerialId)\":\"" << value->SerialId << '\"';
	ostr << ",\"出入金状态(CashState)\":" << to_log_text(value->CashState);
	ostr << ",\"是否强制出金标记(ForceCashOutFlag)\":" << to_log_text(value->ForceCashOutFlag);
	ostr.put('}');
	return ostr.str();
}
///出金入金审核请求命令调用参数 记录到日志
void log(const CashCheckArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///出入金 生成日志文本
string to_log_text(const CashOper* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->CashId != 0)
		ostr << ",\"出入金ID(CashId)\":\"" << value->CashId << '\"';
	if (value->CustomerId != 0)
		ostr << ",\"客户ID(CustomerId)\":\"" << value->CustomerId << '\"';
	if (strlen(value->RealName) > 0)
		ostr << ",\"姓名(RealName)\":\"" << value->RealName << '\"';
	if (strlen(value->SerialNo) > 0)
		ostr << ",\"内部编号(SerialNo)\":\"" << value->SerialNo << '\"';
	if (strlen(value->PaperNo) > 0)
		ostr << ",\"凭证号(PaperNo)\":\"" << value->PaperNo << '\"';
	ostr << ",\"出入金方式(CashMode)\":" << to_log_text(value->CashMode);
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	ostr << ",\"出入金类型(CashType)\":" << to_log_text(value->CashType);
	if (value->CashValue != 0)
		ostr << ",\"金额(CashValue)\":\"" << Int64ToDouble(value->CashValue) << '\"';
	if (strlen(value->ClientBank) > 0)
		ostr << ",\"客户开户行(ClientBank)\":\"" << value->ClientBank << '\"';
	if (strlen(value->ClientAccount) > 0)
		ostr << ",\"客户银行账户(ClientAccount)\":\"" << value->ClientAccount << '\"';
	ostr << ",\"客户本外币账户(ClientLWFlag)\":" << to_log_text(value->ClientLWFlag);
	if (strlen(value->CompanyBank) > 0)
		ostr << ",\"公司开户行(CompanyBank)\":\"" << value->CompanyBank << '\"';
	if (strlen(value->CompanyAccount) > 0)
		ostr << ",\"公司银行账户(CompanyAccount)\":\"" << value->CompanyAccount << '\"';
	ostr << ",\"公司本外币账户(CompanyLWFlag)\":" << to_log_text(value->CompanyLWFlag);
	ostr << ",\"入金时间(CashDate)\":" << to_log_text(value->CashDate);
	if (strlen(value->PaperImage) > 0)
		ostr << ",\"凭证图片(PaperImage)\":\"" << value->PaperImage << '\"';
	if (strlen(value->CashRemark) > 0)
		ostr << ",\"备注(CashRemark)\":\"" << value->CashRemark << '\"';
	ostr.put('}');
	return ostr.str();
}
///出入金 记录到日志
void log(const CashOper* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///出金入金数据 生成日志文本
string to_log_text(const CashOperQueryRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->CashOperQueryRspId != 0)
		ostr << ",\"出金入金查询应答ID(CashOperQueryRspId)\":\"" << value->CashOperQueryRspId << '\"';
	if (value->SerialId != 0)
		ostr << ",\"出入金流水号(SerialId)\":\"" << value->SerialId << '\"';
	ostr << ",\"出入金状态(CashState)\":" << to_log_text(value->CashState);
	if (strlen(value->OperDateTime) > 0)
		ostr << ",\"操作时间(OperDateTime)\":\"" << value->OperDateTime << '\"';
	if (strlen(value->OperatorNo) > 0)
		ostr << ",\"操作人(OperatorNo)\":\"" << value->OperatorNo << '\"';
	if (strlen(value->CheckDateTime) > 0)
		ostr << ",\"审核时间(CheckDateTime)\":\"" << value->CheckDateTime << '\"';
	if (strlen(value->CheckOperatorNo) > 0)
		ostr << ",\"审核人(CheckOperatorNo)\":\"" << value->CheckOperatorNo << '\"';
	ostr << ",\"出入金类型(CashType)\":" << to_log_text(value->CashType);
	ostr << ",\"出入金方式(CashMode)\":" << to_log_text(value->CashMode);
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	if (value->CashValue != 0)
		ostr << ",\"操作金额(CashValue)\":\"" << Int64ToDouble(value->CashValue) << '\"';
	if (strlen(value->CashRemark) > 0)
		ostr << ",\"备注(CashRemark)\":\"" << value->CashRemark << '\"';
	if (strlen(value->ClientBank) > 0)
		ostr << ",\"客户银行标识(ClientBank)\":\"" << value->ClientBank << '\"';
	if (strlen(value->ClientAccount) > 0)
		ostr << ",\"客户银行账户(ClientAccount)\":\"" << value->ClientAccount << '\"';
	ostr << ",\"客户本外币账户标识(ClientLWFlag)\":" << to_log_text(value->ClientLWFlag);
	if (strlen(value->CompanyBank) > 0)
		ostr << ",\"公司银行标识(CompanyBank)\":\"" << value->CompanyBank << '\"';
	if (strlen(value->CompanyAccount) > 0)
		ostr << ",\"公司银行账户(CompanyAccount)\":\"" << value->CompanyAccount << '\"';
	ostr << ",\"公司本外币账户标识(CompanyLWFlag)\":" << to_log_text(value->CompanyLWFlag);
	ostr.put('}');
	return ostr.str();
}
///出金入金数据 记录到日志
void log(const CashOperQueryRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///出金入金操作应答 生成日志文本
string to_log_text(const CashOperRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->CashOperRspId != 0)
		ostr << ",\"出金入金操作应答ID(CashOperRspId)\":\"" << value->CashOperRspId << '\"';
	if (value->SerialId != 0)
		ostr << ",\"出入金流水号(SerialId)\":\"" << value->SerialId << '\"';
	ostr << ",\"出入金状态(CashState)\":" << to_log_text(value->CashState);
	if (strlen(value->OperDateTime) > 0)
		ostr << ",\"操作时间(OperDateTime)\":\"" << value->OperDateTime << '\"';
	if (strlen(value->OperatorNo) > 0)
		ostr << ",\"操作人(OperatorNo)\":\"" << value->OperatorNo << '\"';
	if (strlen(value->CheckDateTime) > 0)
		ostr << ",\"审核时间(CheckDateTime)\":\"" << value->CheckDateTime << '\"';
	if (strlen(value->CheckOperatorNo) > 0)
		ostr << ",\"审核人(CheckOperatorNo)\":\"" << value->CheckOperatorNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///出金入金操作应答 记录到日志
void log(const CashOperRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///历史资金调整查询命令调用参数 生成日志文本
string to_log_text(const HisAdjustQueryArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->BeginDate) > 0)
		ostr << ",\"起始日期(BeginDate)\":\"" << value->BeginDate << '\"';
	if (strlen(value->EndDate) > 0)
		ostr << ",\"终止日期(EndDate)\":\"" << value->EndDate << '\"';
	ostr.put('}');
	return ostr.str();
}
///历史资金调整查询命令调用参数 记录到日志
void log(const HisAdjustQueryArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///历史资金调整数据 生成日志文本
string to_log_text(const HisAdjustQueryRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->HisAdjustQueryRspId != 0)
		ostr << ",\"历史资金调整查询应答ID(HisAdjustQueryRspId)\":\"" << value->HisAdjustQueryRspId << '\"';
	if (strlen(value->Date) > 0)
		ostr << ",\"日期(Date)\":\"" << value->Date << '\"';
	ostr << ",\"数据(Data)\":" << to_log_text(&value->Data);
	ostr.put('}');
	return ostr.str();
}
///历史资金调整数据 记录到日志
void log(const HisAdjustQueryRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///历史出金入金查询命令调用参数 生成日志文本
string to_log_text(const HisCashOperQueryArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->BeginDate) > 0)
		ostr << ",\"起始日期(BeginDate)\":\"" << value->BeginDate << '\"';
	if (strlen(value->EndDate) > 0)
		ostr << ",\"终止日期(EndDate)\":\"" << value->EndDate << '\"';
	ostr.put('}');
	return ostr.str();
}
///历史出金入金查询命令调用参数 记录到日志
void log(const HisCashOperQueryArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///历史出金入金数据 生成日志文本
string to_log_text(const HisCashOperQueryRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->HisCashOperQueryRspId != 0)
		ostr << ",\"历史出金入金查询应答ID(HisCashOperQueryRspId)\":\"" << value->HisCashOperQueryRspId << '\"';
	if (strlen(value->Date) > 0)
		ostr << ",\"日期(Date)\":\"" << value->Date << '\"';
	ostr << ",\"数据(Data)\":" << to_log_text(&value->Data);
	ostr.put('}');
	return ostr.str();
}
///历史出金入金数据 记录到日志
void log(const HisCashOperQueryRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///资金变化数据 生成日志文本
string to_log_text(const MoneyChgNotice* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->MoneyChgNoticeId != 0)
		ostr << ",\"资金变化推送通知ID(MoneyChgNoticeId)\":\"" << value->MoneyChgNoticeId << '\"';
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	if (value->MoneyChgNum != 0)
		ostr << ",\"资金变化项的个数(MoneyChgNum)\":\"" << value->MoneyChgNum << '\"';
	ostr << ",\"资金变化内容(MoneyItem)\":" << to_log_text(value->MoneyItem);
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///资金变化数据 记录到日志
void log(const MoneyChgNotice* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///资金数据 生成日志文本
string to_log_text(const MoneyQueryRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->MoneyQueryRspId != 0)
		ostr << ",\"资金查询应答ID(MoneyQueryRspId)\":\"" << value->MoneyQueryRspId << '\"';
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	if (value->YAvailable != 0)
		ostr << ",\"昨可用(YAvailable)\":\"" << Int64ToDouble(value->YAvailable) << '\"';
	if (value->YCanCashOut != 0)
		ostr << ",\"昨可提(YCanCashOut)\":\"" << Int64ToDouble(value->YCanCashOut) << '\"';
	if (value->YMoney != 0)
		ostr << ",\"昨账面(YMoney)\":\"" << Int64ToDouble(value->YMoney) << '\"';
	if (value->YBalance != 0)
		ostr << ",\"昨权益(YBalance)\":\"" << Int64ToDouble(value->YBalance) << '\"';
	if (value->YUnExpiredProfit != 0)
		ostr << ",\"昨未结平盈(YUnExpiredProfit)\":\"" << Int64ToDouble(value->YUnExpiredProfit) << '\"';
	if (value->Adjust != 0)
		ostr << ",\"资金调整0(Adjust)\":\"" << Int64ToDouble(value->Adjust) << '\"';
	if (value->CashIn != 0)
		ostr << ",\"入金1(CashIn)\":\"" << Int64ToDouble(value->CashIn) << '\"';
	if (value->CashOut != 0)
		ostr << ",\"出金2(CashOut)\":\"" << Int64ToDouble(value->CashOut) << '\"';
	if (value->Fee != 0)
		ostr << ",\"手续费3(Fee)\":\"" << Int64ToDouble(value->Fee) << '\"';
	if (value->Frozen != 0)
		ostr << ",\"冻结资金4(Frozen)\":\"" << Int64ToDouble(value->Frozen) << '\"';
	if (value->CoverProfit != 0)
		ostr << ",\"逐笔平盈5(CoverProfit)\":\"" << Int64ToDouble(value->CoverProfit) << '\"';
	if (value->DayCoverProfit != 0)
		ostr << ",\"盯市平盈6(DayCoverProfit)\":\"" << Int64ToDouble(value->DayCoverProfit) << '\"';
	if (value->FloatProfit != 0)
		ostr << ",\"逐笔浮盈7(FloatProfit)\":\"" << Int64ToDouble(value->FloatProfit) << '\"';
	if (value->DayFloatProfit != 0)
		ostr << ",\"盯市浮盈8(DayFloatProfit)\":\"" << Int64ToDouble(value->DayFloatProfit) << '\"';
	if (value->UnExpiredProfit != 0)
		ostr << ",\"未结平盈9(UnExpiredProfit)\":\"" << Int64ToDouble(value->UnExpiredProfit) << '\"';
	if (value->Premium != 0)
		ostr << ",\"权利金10(Premium)\":\"" << Int64ToDouble(value->Premium) << '\"';
	if (value->Deposit != 0)
		ostr << ",\"保证金11(Deposit)\":\"" << Int64ToDouble(value->Deposit) << '\"';
	if (value->KeepDeposit != 0)
		ostr << ",\"维持保证金12(KeepDeposit)\":\"" << Int64ToDouble(value->KeepDeposit) << '\"';
	if (value->Pledge != 0)
		ostr << ",\"质押资金13(Pledge)\":\"" << Int64ToDouble(value->Pledge) << '\"';
	if (value->TAvailable != 0)
		ostr << ",\"可用资金14(TAvailable)\":\"" << Int64ToDouble(value->TAvailable) << '\"';
	if (value->Discount != 0)
		ostr << ",\"贴现金额15(Discount)\":\"" << Int64ToDouble(value->Discount) << '\"';
	if (value->TradeFee != 0)
		ostr << ",\"交易手续费16(TradeFee)\":\"" << Int64ToDouble(value->TradeFee) << '\"';
	if (value->DeliveryFee != 0)
		ostr << ",\"交割手续费17(DeliveryFee)\":\"" << Int64ToDouble(value->DeliveryFee) << '\"';
	if (value->ExchangeFee != 0)
		ostr << ",\"汇兑手续费18(ExchangeFee)\":\"" << Int64ToDouble(value->ExchangeFee) << '\"';
	if (value->FrozenDeposit != 0)
		ostr << ",\"冻结保证金19(FrozenDeposit)\":\"" << Int64ToDouble(value->FrozenDeposit) << '\"';
	if (value->FrozenFee != 0)
		ostr << ",\"冻结手续费20(FrozenFee)\":\"" << Int64ToDouble(value->FrozenFee) << '\"';
	if (value->NewFloatProfit != 0)
		ostr << ",\"浮盈(无LME)21(NewFloatProfit)\":\"" << Int64ToDouble(value->NewFloatProfit) << '\"';
	if (value->LmeFloatProfit != 0)
		ostr << ",\"LME浮盈22(LmeFloatProfit)\":\"" << Int64ToDouble(value->LmeFloatProfit) << '\"';
	if (value->OptionMarketValue != 0)
		ostr << ",\"期权市值23(OptionMarketValue)\":\"" << Int64ToDouble(value->OptionMarketValue) << '\"';
	if (value->OriCash != 0)
		ostr << ",\"币种原始出入金24(非自动汇兑资金(OriCash)\":\"" << Int64ToDouble(value->OriCash) << '\"';
	if (value->TMoney != 0)
		ostr << ",\"今资金(TMoney)\":\"" << Int64ToDouble(value->TMoney) << '\"';
	if (value->TBalance != 0)
		ostr << ",\"今权益(TBalance)\":\"" << Int64ToDouble(value->TBalance) << '\"';
	if (value->TCanCashOut != 0)
		ostr << ",\"今可提(TCanCashOut)\":\"" << Int64ToDouble(value->TCanCashOut) << '\"';
	if (value->RiskRate != 0)
		ostr << ",\"风险率(RiskRate)\":\"" << Int64ToDouble(value->RiskRate) << '\"';
	if (value->AccountMarketValue != 0)
		ostr << ",\"账户市值(AccountMarketValue)\":\"" << Int64ToDouble(value->AccountMarketValue) << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///资金数据 记录到日志
void log(const MoneyQueryRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///用户密码验证请求应答 生成日志文本
string to_log_text(const ClientPasswordAuthRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->ClientPasswordAuthRspId != 0)
		ostr << ",\"用户密码验证请求应答ID(ClientPasswordAuthRspId)\":\"" << value->ClientPasswordAuthRspId << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户编号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///用户密码验证请求应答 记录到日志
void log(const ClientPasswordAuthRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///修改客户密码命令调用参数 生成日志文本
string to_log_text(const ClientPasswordModifyArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->OldPassword) > 0)
		ostr << ",\"旧密码(OldPassword)\":\"" << value->OldPassword << '\"';
	if (strlen(value->NewPassword) > 0)
		ostr << ",\"新密码(NewPassword)\":\"" << value->NewPassword << '\"';
	if (strlen(value->UserName) > 0)
		ostr << ",\"用户名(UserName)\":\"" << value->UserName << '\"';
	ostr.put('}');
	return ostr.str();
}
///修改客户密码命令调用参数 记录到日志
void log(const ClientPasswordModifyArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///客户密码修改应答结构 生成日志文本
string to_log_text(const ClientPasswordModifyRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->ClientPasswordModifyRspId != 0)
		ostr << ",\"ID(ClientPasswordModifyRspId)\":\"" << value->ClientPasswordModifyRspId << '\"';
	ostr << ",\"密码类型(PasswordType)\":" << to_log_text(value->PasswordType);
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///客户密码修改应答结构 记录到日志
void log(const ClientPasswordModifyRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///登录请求结构 生成日志文本
string to_log_text(const LoginArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->LoginType != 0)
		ostr << ",\"登录类型(LoginType)\":\"" << value->LoginType << '\"';
	if (strlen(value->UserName) > 0)
		ostr << ",\"用户名(UserName)\":\"" << value->UserName << '\"';
	if (strlen(value->PassWord) > 0)
		ostr << ",\"密码(PassWord)\":\"" << value->PassWord << '\"';
	ostr.put('}');
	return ostr.str();
}
///登录请求结构 记录到日志
void log(const LoginArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///密码修改请求结构 生成日志文本
string to_log_text(const ModifyPasswordArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->NewPassword) > 0)
		ostr << ",\"新密码(NewPassword)\":\"" << value->NewPassword << '\"';
	if (strlen(value->OldPassword) > 0)
		ostr << ",\"旧密码(OldPassword)\":\"" << value->OldPassword << '\"';
	ostr.put('}');
	return ostr.str();
}
///密码修改请求结构 记录到日志
void log(const ModifyPasswordArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///查询监控事件应答 生成日志文本
string to_log_text(const MonitorEventQueryRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->MonitorEventQueryRspId != 0)
		ostr << ",\"查询监控事件应答ID(MonitorEventQueryRspId)\":\"" << value->MonitorEventQueryRspId << '\"';
	ostr << ",\"事件级别(EventLevel)\":" << to_log_text(value->EventLevel);
	if (strlen(value->EventSource) > 0)
		ostr << ",\"事件源(EventSource)\":\"" << value->EventSource << '\"';
	if (strlen(value->EventContent) > 0)
		ostr << ",\"事件内容(EventContent)\":\"" << value->EventContent << '\"';
	if (value->SerialId != 0)
		ostr << ",\"序号编号(SerialId)\":\"" << value->SerialId << '\"';
	if (strlen(value->EventDateTime) > 0)
		ostr << ",\"活动日期时间(EventDateTime)\":\"" << value->EventDateTime << '\"';
	ostr << ",\"事件类型(EventType)\":" << to_log_text(value->EventType);
	ostr.put('}');
	return ostr.str();
}
///查询监控事件应答 记录到日志
void log(const MonitorEventQueryRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///网络服务的地址信息 生成日志文本
string to_log_text(const NetServiceAddress* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->Id != 0)
		ostr << ",\"地址ID(Id)\":\"" << value->Id << '\"';
	if (strlen(value->Name) > 0)
		ostr << ",\"名称(Name)\":\"" << value->Name << '\"';
	if (strlen(value->Decription) > 0)
		ostr << ",\"说明(Decription)\":\"" << value->Decription << '\"';
	if (strlen(value->Protocol) > 0)
		ostr << ",\"协议名称(Protocol)\":\"" << value->Protocol << '\"';
	if (strlen(value->Ip) > 0)
		ostr << ",\"IP地址(Ip)\":\"" << value->Ip << '\"';
	if (value->Port != 0)
		ostr << ",\"端口号(Port)\":\"" << value->Port << '\"';
	ostr.put('}');
	return ostr.str();
}
///网络服务的地址信息 记录到日志
void log(const NetServiceAddress* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///修改操作员密码命令调用参数 生成日志文本
string to_log_text(const OperatorPasswordModifyArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->OperatorNo) > 0)
		ostr << ",\"操作员号(OperatorNo)\":\"" << value->OperatorNo << '\"';
	if (strlen(value->OldPassword) > 0)
		ostr << ",\"旧密码(OldPassword)\":\"" << value->OldPassword << '\"';
	if (strlen(value->NewPassword) > 0)
		ostr << ",\"新密码(NewPassword)\":\"" << value->NewPassword << '\"';
	ostr.put('}');
	return ostr.str();
}
///修改操作员密码命令调用参数 记录到日志
void log(const OperatorPasswordModifyArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///修改操作员密码时应答 生成日志文本
string to_log_text(const OperatorPasswordModifyRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OperatorPasswordModifyRspId != 0)
		ostr << ",\"修改操作员密码时应答ID(OperatorPasswordModifyRspId)\":\"" << value->OperatorPasswordModifyRspId << '\"';
	if (strlen(value->OperatorNo) > 0)
		ostr << ",\"操作员号(OperatorNo)\":\"" << value->OperatorNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///修改操作员密码时应答 记录到日志
void log(const OperatorPasswordModifyRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}
///历史成交查询命令调用参数 生成日志文本
string to_log_text(const HisMatchQueryArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"是否包含合计值(IsContainTotle)\":" << to_log_text(value->IsContainTotle);
	if (strlen(value->BeginDate) > 0)
		ostr << ",\"起始日期(BeginDate)\":\"" << value->BeginDate << '\"';
	if (strlen(value->EndDate) > 0)
		ostr << ",\"结束日期(EndDate)\":\"" << value->EndDate << '\"';
	ostr.put('}');
	return ostr.str();
}
///历史成交查询命令调用参数 记录到日志
void log(const HisMatchQueryArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///历史委托查询命令调用参数 生成日志文本
string to_log_text(const HisOrderQueryArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->BeginDate) > 0)
		ostr << ",\"起始日期(BeginDate)\":\"" << value->BeginDate << '\"';
	if (strlen(value->EndDate) > 0)
		ostr << ",\"结束日期(EndDate)\":\"" << value->EndDate << '\"';
	ostr.put('}');
	return ostr.str();
}
///历史委托查询命令调用参数 记录到日志
void log(const HisOrderQueryArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///历史委托数据 生成日志文本
string to_log_text(const HisOrderQueryRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->HisOrderQueryRspId != 0)
		ostr << ",\"历史委托查询应答ID(HisOrderQueryRspId)\":\"" << value->HisOrderQueryRspId << '\"';
	if (strlen(value->Date) > 0)
		ostr << ",\"委托日期(Date)\":\"" << value->Date << '\"';
	ostr << ",\"委托数据(Data)\":" << to_log_text(&value->Data);
	ostr.put('}');
	return ostr.str();
}
///历史委托数据 记录到日志
void log(const HisOrderQueryRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///下港交所做市商报单命令调用参数 生成日志文本
string to_log_text(const HKMarketOrderArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->HKMarketOrderArgId != 0)
		ostr << ",\"下港交所做市商报单命令调用参数ID(HKMarketOrderArgId)\":\"" << value->HKMarketOrderArgId << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品编号(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合同号(ContractNo)\":\"" << value->ContractNo << '\"';
	ostr << ",\"委托类型(OrderType)\":" << to_log_text(value->OrderType);
	ostr << ",\"委托模式(OrderMode)\":" << to_log_text(value->OrderMode);
	if (strlen(value->ValidDateTime) > 0)
		ostr << ",\"有效日期时间(ValidDateTime)\":\"" << value->ValidDateTime << '\"';
	if (value->BuyPrice != 0)
		ostr << ",\"购买价格(BuyPrice)\":\"" << Int64ToDouble(value->BuyPrice) << '\"';
	if (value->SellPrice != 0)
		ostr << ",\"出售价格(SellPrice)\":\"" << Int64ToDouble(value->SellPrice) << '\"';
	if (value->BuyVol != 0)
		ostr << ",\"买卷(BuyVol)\":\"" << value->BuyVol << '\"';
	if (value->SellVol != 0)
		ostr << ",\"卖卷(SellVol)\":\"" << value->SellVol << '\"';
	ostr << ",\"购买操作(BuyOper)\":" << to_log_text(value->BuyOper);
	ostr << ",\"销售工作(SellOper)\":" << to_log_text(value->SellOper);
	if (value->BuyOrderID != 0)
		ostr << ",\"购买订单ID(BuyOrderID)\":\"" << value->BuyOrderID << '\"';
	if (value->SellOrderID != 0)
		ostr << ",\"销售订单ID(SellOrderID)\":\"" << value->SellOrderID << '\"';
	if (strlen(value->SaveString) > 0)
		ostr << ",\"保存字符串(SaveString)\":\"" << value->SaveString << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户编号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///下港交所做市商报单命令调用参数 记录到日志
void log(const HKMarketOrderArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///查询持仓命令调用参数 生成日志文本
string to_log_text(const HoldQueryArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合约(ContractNo)\":\"" << value->ContractNo << '\"';
	if (strlen(value->ExchangeNo) > 0)
		ostr << ",\"交易所(ExchangeNo)\":\"" << value->ExchangeNo << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///查询持仓命令调用参数 记录到日志
void log(const HoldQueryArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///持仓变化数据 生成日志文本
string to_log_text(const HoldQueryRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->HoldQueryRspId != 0)
		ostr << ",\"持仓查询应答ID(HoldQueryRspId)\":\"" << value->HoldQueryRspId << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合约(ContractNo)\":\"" << value->ContractNo << '\"';
	ostr << ",\"买卖方向(Direct)\":" << to_log_text(value->Direct);
	ostr << ",\"投机保值(Hedge)\":" << to_log_text(value->Hedge);
	if (value->TradePrice != 0)
		ostr << ",\"持仓均价(TradePrice)\":\"" << Int64ToDouble(value->TradePrice) << '\"';
	if (value->TradeVol != 0)
		ostr << ",\"持仓量(TradeVol)\":\"" << value->TradeVol << '\"';
	if (value->YSettlePrice != 0)
		ostr << ",\"昨结算价(YSettlePrice)\":\"" << Int64ToDouble(value->YSettlePrice) << '\"';
	if (value->TNewPrice != 0)
		ostr << ",\"最新价(TNewPrice)\":\"" << Int64ToDouble(value->TNewPrice) << '\"';
	if (strlen(value->MatchDateTime) > 0)
		ostr << ",\"成交时间(MatchDateTime)\":\"" << value->MatchDateTime << '\"';
	if (strlen(value->MatchNo) > 0)
		ostr << ",\"成交号(MatchNo)\":\"" << value->MatchNo << '\"';
	if (value->Deposit != 0)
		ostr << ",\"保证金(Deposit)\":\"" << Int64ToDouble(value->Deposit) << '\"';
	if (value->KeepDeposit != 0)
		ostr << ",\"维持保证金(KeepDeposit)\":\"" << Int64ToDouble(value->KeepDeposit) << '\"';
	if (value->HoldKeyId != 0)
		ostr << ",\"持仓关键字(HoldKeyId)\":\"" << value->HoldKeyId << '\"';
	ostr.put('}');
	return ostr.str();
}
///持仓变化数据 记录到日志
void log(const HoldQueryRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///成交删除数据 生成日志文本
string to_log_text(const MatchRemoveNotice* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->MatchRemoveNoticeId != 0)
		ostr << ",\"成交删除通知ID(MatchRemoveNoticeId)\":\"" << value->MatchRemoveNoticeId << '\"';
	if (strlen(value->SystemNo) > 0)
		ostr << ",\"系统编号(SystemNo)\":\"" << value->SystemNo << '\"';
	if (strlen(value->MatchNo) > 0)
		ostr << ",\"比赛编号(MatchNo)\":\"" << value->MatchNo << '\"';
	if (value->MatchId != 0)
		ostr << ",\"比赛ID(MatchId)\":\"" << value->MatchId << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户编号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///成交删除数据 记录到日志
void log(const MatchRemoveNotice* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///撤单请求的应答 生成日志文本
string to_log_text(const OrderDeleteRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OrderDeleteRspId != 0)
		ostr << ",\"撤单请求的应答ID(OrderDeleteRspId)\":\"" << value->OrderDeleteRspId << '\"';
	ostr << ",\"报单请求数据(ReqData)\":" << to_log_text(&value->ReqData);
	ostr << ",\"订单状态字段(OrderStateField)\":" << to_log_text(&value->OrderStateField);
	ostr.put('}');
	return ostr.str();
}
///撤单请求的应答 记录到日志
void log(const OrderDeleteRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///委托信息变化数据 生成日志文本
string to_log_text(const OrderInfoNotice* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OrderInfoNoticeId != 0)
		ostr << ",\"委托信息变化通知ID(OrderInfoNoticeId)\":\"" << value->OrderInfoNoticeId << '\"';
	if (value->CustomerId != 0)
		ostr << ",\"客户标识(CustomerId)\":\"" << value->CustomerId << '\"';
	if (value->OrderId != 0)
		ostr << ",\"客户委托ID(OrderId)\":\"" << value->OrderId << '\"';
	if (value->EsOrderStreamId != 0)
		ostr << ",\"委托流号(EsOrderStreamId)\":\"" << value->EsOrderStreamId << '\"';
	if (value->EsOrderId != 0)
		ostr << ",\"委托ID(EsOrderId)\":\"" << value->EsOrderId << '\"';
	if (strlen(value->EsLocalNo) > 0)
		ostr << ",\"本地号(EsLocalNo)\":\"" << value->EsLocalNo << '\"';
	if (strlen(value->EsSystemNo) > 0)
		ostr << ",\"系统号(EsSystemNo)\":\"" << value->EsSystemNo << '\"';
	if (strlen(value->ExchangeSystemNo) > 0)
		ostr << ",\"交易所系统号(ExchangeSystemNo)\":\"" << value->ExchangeSystemNo << '\"';
	if (strlen(value->ActionLocalNo) > 0)
		ostr << ",\"报单操作的本地号(ActionLocalNo)\":\"" << value->ActionLocalNo << '\"';
	if (strlen(value->ParentSystemNo) > 0)
		ostr << ",\"父系统编号(ParentSystemNo)\":\"" << value->ParentSystemNo << '\"';
	if (strlen(value->EsTradeNo) > 0)
		ostr << ",\"交易账号(EsTradeNo)\":\"" << value->EsTradeNo << '\"';
	if (strlen(value->InsertNo) > 0)
		ostr << ",\"录入操作员号(InsertNo)\":\"" << value->InsertNo << '\"';
	if (strlen(value->InsertDateTime) > 0)
		ostr << ",\"录入时间(InsertDateTime)\":\"" << value->InsertDateTime << '\"';
	if (strlen(value->UpdateNo) > 0)
		ostr << ",\"最后一次变更人(UpdateNo)\":\"" << value->UpdateNo << '\"';
	if (strlen(value->UpdateDateTime) > 0)
		ostr << ",\"最后一次变更时间(UpdateDateTime)\":\"" << value->UpdateDateTime << '\"';
	ostr << ",\"委托状态(OrderState)\":" << to_log_text(value->OrderState);
	if (value->MatchPrice != 0)
		ostr << ",\"成交价格(MatchPrice)\":\"" << Int64ToDouble(value->MatchPrice) << '\"';
	if (value->MatchVol != 0)
		ostr << ",\"成交数量(MatchVol)\":\"" << value->MatchVol << '\"';
	ostr << ",\"最后一次操作错误信息码(ErrorCode)\":" << to_log_text(value->ErrorCode);
	if (strlen(value->ErrorText) > 0)
		ostr << ",\"原始错误信息(ErrorText)\":\"" << value->ErrorText << '\"';
	ostr << ",\"是否录单(OrderInput)\":" << to_log_text(value->OrderInput);
	ostr << ",\"是否删除(Deleted)\":" << to_log_text(value->Deleted);
	ostr << ",\"T+1标志(AddOne)\":" << to_log_text(value->AddOne);
	ostr.put('}');
	return ostr.str();
}
///委托信息变化数据 记录到日志
void log(const OrderInfoNotice* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///报单请求的应答 生成日志文本
string to_log_text(const OrderInsertRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OrderInsertRspId != 0)
		ostr << ",\"报单请求的应答ID(OrderInsertRspId)\":\"" << value->OrderInsertRspId << '\"';
	if (value->OrderId != 0)
		ostr << ",\"客户委托ID(OrderId)\":\"" << value->OrderId << '\"';
	if (value->CustomerId != 0)
		ostr << ",\"客户标识(CustomerId)\":\"" << value->CustomerId << '\"';
	if (value->EsOrderId != 0)
		ostr << ",\"易盛委托号(EsOrderId)\":\"" << value->EsOrderId << '\"';
	if (strlen(value->LocalNo) > 0)
		ostr << ",\"本地号(LocalNo)\":\"" << value->LocalNo << '\"';
	if (strlen(value->TradeNo) > 0)
		ostr << ",\"客户交易帐号(TradeNo)\":\"" << value->TradeNo << '\"';
	if (strlen(value->InsertNo) > 0)
		ostr << ",\"下单人(InsertNo)\":\"" << value->InsertNo << '\"';
	if (strlen(value->InsertDateTime) > 0)
		ostr << ",\"下单时间(InsertDateTime)\":\"" << value->InsertDateTime << '\"';
	ostr << ",\"委托状态(OrderState)\":" << to_log_text(value->OrderState);
	if (value->OrderStreamId != 0)
		ostr << ",\"委托流号(OrderStreamId)\":\"" << value->OrderStreamId << '\"';
	ostr.put('}');
	return ostr.str();
}
///报单请求的应答 记录到日志
void log(const OrderInsertRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///改单请求命令调用参数 生成日志文本
string to_log_text(const OrderModifyArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OrderPrice != 0)
		ostr << ",\"委托价格(OrderPrice)\":\"" << Int64ToDouble(value->OrderPrice) << '\"';
	if (value->TriggerPrice != 0)
		ostr << ",\"触发价格(TriggerPrice)\":\"" << Int64ToDouble(value->TriggerPrice) << '\"';
	if (value->OrderVol != 0)
		ostr << ",\"委托数量(OrderVol)\":\"" << value->OrderVol << '\"';
	if (value->OrderId != 0)
		ostr << ",\"委托ID(OrderId)\":\"" << value->OrderId << '\"';
	ostr.put('}');
	return ostr.str();
}
///改单请求命令调用参数 记录到日志
void log(const OrderModifyArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///改单请求的应答 生成日志文本
string to_log_text(const OrderModifyRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OrderModifyRspId != 0)
		ostr << ",\"改单请求的应答ID(OrderModifyRspId)\":\"" << value->OrderModifyRspId << '\"';
	ostr << ",\"报单请求数据(ReqData)\":" << to_log_text(&value->ReqData);
	ostr << ",\"订单状态字段(OrderStateField)\":" << to_log_text(&value->OrderStateField);
	ostr.put('}');
	return ostr.str();
}
///改单请求的应答 记录到日志
void log(const OrderModifyRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///查询客户委托命令调用参数 生成日志文本
string to_log_text(const OrderQueryArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->LocalNo) > 0)
		ostr << ",\"本地号(LocalNo)\":\"" << value->LocalNo << '\"';
	if (strlen(value->SystemNo) > 0)
		ostr << ",\"系统号(SystemNo)\":\"" << value->SystemNo << '\"';
	if (strlen(value->OperNo) > 0)
		ostr << ",\"下单人或操作人号(OperNo)\":\"" << value->OperNo << '\"';
	if (strlen(value->BeginInsertDateTime) > 0)
		ostr << ",\"起始时间(BeginInsertDateTime)\":\"" << value->BeginInsertDateTime << '\"';
	if (strlen(value->EndInsertDateTime) > 0)
		ostr << ",\"结束时间(EndInsertDateTime)\":\"" << value->EndInsertDateTime << '\"';
	ostr << ",\"委托状态(OrderState)\":" << to_log_text(value->OrderState);
	if (value->OrderStreamId != 0)
		ostr << ",\"查询条件返回大于此流号的委托数据(OrderStreamId)\":\"" << value->OrderStreamId << '\"';
	if (strlen(value->ExchangeNo) > 0)
		ostr << ",\"交易所(ExchangeNo)\":\"" << value->ExchangeNo << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合约(ContractNo)\":\"" << value->ContractNo << '\"';
	ostr << ",\"委托类型(OrderType)\":" << to_log_text(value->OrderType);
	ostr << ",\"委托模式(OrderMode)\":" << to_log_text(value->OrderMode);
	ostr << ",\"风险报单(IsRiskOrder)\":" << to_log_text(value->IsRiskOrder);
	ostr << ",\"投机保值(Hedge)\":" << to_log_text(value->Hedge);
	if (value->OrderId != 0)
		ostr << ",\"委托ID(OrderId)\":\"" << value->OrderId << '\"';
	ostr.put('}');
	return ostr.str();
}
///查询客户委托命令调用参数 记录到日志
void log(const OrderQueryArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///委托删除数据 生成日志文本
string to_log_text(const OrderRemoveNotice* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OrderRemoveNoticeId != 0)
		ostr << ",\"委托删除通知ID(OrderRemoveNoticeId)\":\"" << value->OrderRemoveNoticeId << '\"';
	if (value->OrderId != 0)
		ostr << ",\"订单ID(OrderId)\":\"" << value->OrderId << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户编号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///委托删除数据 记录到日志
void log(const OrderRemoveNotice* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///委托变化数据 生成日志文本
string to_log_text(const OrderStateNotice* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OrderStateNoticeId != 0)
		ostr << ",\"委托变化通知ID(OrderStateNoticeId)\":\"" << value->OrderStateNoticeId << '\"';
	if (value->CustomerId != 0)
		ostr << ",\"客户标识(CustomerId)\":\"" << value->CustomerId << '\"';
	if (value->OrderId != 0)
		ostr << ",\"客户委托ID(OrderId)\":\"" << value->OrderId << '\"';
	if (value->EsOrderId != 0)
		ostr << ",\"易盛委托ID(EsOrderId)\":\"" << value->EsOrderId << '\"';
	if (value->OrderStreamId != 0)
		ostr << ",\"委托流号(OrderStreamId)\":\"" << value->OrderStreamId << '\"';
	if (strlen(value->LocalNo) > 0)
		ostr << ",\"本地号(LocalNo)\":\"" << value->LocalNo << '\"';
	if (strlen(value->SystemNo) > 0)
		ostr << ",\"系统号(SystemNo)\":\"" << value->SystemNo << '\"';
	if (strlen(value->ExchangeSystemNo) > 0)
		ostr << ",\"交易所系统号(ExchangeSystemNo)\":\"" << value->ExchangeSystemNo << '\"';
	if (strlen(value->ActionLocalNo) > 0)
		ostr << ",\"报单操作的本地号(ActionLocalNo)\":\"" << value->ActionLocalNo << '\"';
	ostr << ",\"开平类型(Offset)\":" << to_log_text(value->Offset);
	ostr << ",\"投机保值类型(Hedge)\":" << to_log_text(value->Hedge);
	if (value->OrderPrice != 0)
		ostr << ",\"委托价(OrderPrice)\":\"" << Int64ToDouble(value->OrderPrice) << '\"';
	if (value->TriggerPrice != 0)
		ostr << ",\"触发价(TriggerPrice)\":\"" << Int64ToDouble(value->TriggerPrice) << '\"';
	if (value->OrderVol != 0)
		ostr << ",\"委托数量(OrderVol)\":\"" << value->OrderVol << '\"';
	if (strlen(value->UpdateNo) > 0)
		ostr << ",\"操作员编号(UpdateNo)\":\"" << value->UpdateNo << '\"';
	if (strlen(value->UpdateDateTime) > 0)
		ostr << ",\"操作时间(UpdateDateTime)\":\"" << value->UpdateDateTime << '\"';
	ostr << ",\"委托状态(OrderState)\":" << to_log_text(value->OrderState);
	if (value->MatchPrice != 0)
		ostr << ",\"成交均价(MatchPrice)\":\"" << Int64ToDouble(value->MatchPrice) << '\"';
	if (value->MatchVol != 0)
		ostr << ",\"成交数量(MatchVol)\":\"" << value->MatchVol << '\"';
	ostr << ",\"委托类型(OrderType)\":" << to_log_text(value->OrderType);
	ostr << ",\"错误码(ErrorCode)\":" << to_log_text(value->ErrorCode);
	if (strlen(value->ErrorText) > 0)
		ostr << ",\"原始错误信息(ErrorText)\":\"" << value->ErrorText << '\"';
	ostr.put('}');
	return ostr.str();
}
///委托变化数据 记录到日志
void log(const OrderStateNotice* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///查询客户成交命令调用参数 生成日志文本
string to_log_text(const TMatchQueryArg* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->TMatchQueryArgId != 0)
		ostr << ",\"查询客户成交命令调用参数ID(TMatchQueryArgId)\":\"" << value->TMatchQueryArgId << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	if (strlen(value->SystemNo) > 0)
		ostr << ",\"系统号(SystemNo)\":\"" << value->SystemNo << '\"';
	if (strlen(value->MatchNo) > 0)
		ostr << ",\"成交号(MatchNo)\":\"" << value->MatchNo << '\"';
	if (strlen(value->BeginMatchDateTime) > 0)
		ostr << ",\"起始时间(BeginMatchDateTime)\":\"" << value->BeginMatchDateTime << '\"';
	if (strlen(value->EndMatchDateTime) > 0)
		ostr << ",\"结束时间(EndMatchDateTime)\":\"" << value->EndMatchDateTime << '\"';
	if (strlen(value->ExchangeNo) > 0)
		ostr << ",\"交易所(ExchangeNo)\":\"" << value->ExchangeNo << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"跨品种套利需要(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"跨期套利需要(ContractNo)\":\"" << value->ContractNo << '\"';
	if (value->MatchStreamId != 0)
		ostr << ",\"成交流号(MatchStreamId)\":\"" << value->MatchStreamId << '\"';
	ostr.put('}');
	return ostr.str();
}
///查询客户成交命令调用参数 记录到日志
void log(const TMatchQueryArg* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}
///客户计算参数查询请求结构 生成日志文本
string to_log_text(const TClientCountRentQryReq* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品编号(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"可填空(ContractNo)\":\"" << value->ContractNo << '\"';
	ostr << ",\"匹配方式(MatchWay)\":" << value->MatchWay;
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户编号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///客户计算参数查询请求结构 记录到日志
void log(const TClientCountRentQryReq* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///客户结算参数查询应答结构 生成日志文本
string to_log_text(const TClientCountRentQryRsp* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->BFee != 0)
		ostr << ",\"B费(BFee)\":\"" << value->BFee << '\"';
	if (value->DFee != 0)
		ostr << ",\"D费用(DFee)\":\"" << value->DFee << '\"';
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	ostr << ",\"仅比例和定额(DepositMode)\":" << value->DepositMode;
	if (value->Deposit != 0)
		ostr << ",\"正常保证金(Deposit)\":\"" << value->Deposit << '\"';
	if (value->LockDeposit != 0)
		ostr << ",\"锁仓保证金(LockDeposit)\":\"" << value->LockDeposit << '\"';
	if (value->KeepDeposit != 0)
		ostr << ",\"维持保证金(KeepDeposit)\":\"" << value->KeepDeposit << '\"';
	ostr << ",\"请求数据(ReqData)\":" << to_log_text(&value->ReqData);
	ostr.put('}');
	return ostr.str();
}
///客户结算参数查询应答结构 记录到日志
void log(const TClientCountRentQryRsp* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///基于IP连接的地址信息 生成日志文本
string to_log_text(const TEsAddressField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->Port != 0)
		ostr << ",\"端口号(Port)\":\"" << value->Port << '\"';
	if (strlen(value->Ip) > 0)
		ostr << ",\"IP地址(Ip)\":\"" << value->Ip << '\"';
	ostr.put('}');
	return ostr.str();
}
///基于IP连接的地址信息 记录到日志
void log(const TEsAddressField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///出金入金审核应答结构 生成日志文本
string to_log_text(const TEsAdjustQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///出金入金审核应答结构 记录到日志
void log(const TEsAdjustQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///资金调整查询应答结构 生成日志文本
string to_log_text(const TEsAdjustQryRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->SerialId != 0)
		ostr << ",\"出入金流水号(SerialId)\":\"" << value->SerialId << '\"';
	ostr << ",\"资金调整状态(AdjustState)\":" << value->AdjustState;
	if (strlen(value->OperDateTime) > 0)
		ostr << ",\"操作时间(OperDateTime)\":\"" << value->OperDateTime << '\"';
	if (strlen(value->OperatorNo) > 0)
		ostr << ",\"操作人(OperatorNo)\":\"" << value->OperatorNo << '\"';
	if (strlen(value->CheckDateTime) > 0)
		ostr << ",\"审核时间(CheckDateTime)\":\"" << value->CheckDateTime << '\"';
	if (strlen(value->CheckOperatorNo) > 0)
		ostr << ",\"审核人(CheckOperatorNo)\":\"" << value->CheckOperatorNo << '\"';
	ostr << ",\"资金调整类型(AdjustType)\":" << value->AdjustType;
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	if (value->AdjustValue != 0)
		ostr << ",\"操作金额(AdjustValue)\":\"" << value->AdjustValue << '\"';
	if (strlen(value->AdjustRemark) > 0)
		ostr << ",\"备注(AdjustRemark)\":\"" << value->AdjustRemark << '\"';
	if (strlen(value->ClientBank) > 0)
		ostr << ",\"银行标识(ClientBank)\":\"" << value->ClientBank << '\"';
	if (strlen(value->ClientAccount) > 0)
		ostr << ",\"银行账户(ClientAccount)\":\"" << value->ClientAccount << '\"';
	ostr << ",\"本外币账户标识(ClientLWFlag)\":" << value->ClientLWFlag;
	if (strlen(value->CompanyBank) > 0)
		ostr << ",\"银行标识(CompanyBank)\":\"" << value->CompanyBank << '\"';
	if (strlen(value->CompanyAccount) > 0)
		ostr << ",\"银行账户(CompanyAccount)\":\"" << value->CompanyAccount << '\"';
	ostr << ",\"本外币账户标识(CompanyLWFlag)\":" << value->CompanyLWFlag;
	ostr << ",\"查询请求数据(ReqData)\":" << to_log_text(&value->ReqData);
	ostr.put('}');
	return ostr.str();
}
///资金调整查询应答结构 记录到日志
void log(const TEsAdjustQryRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///出金入金操作通知结构 生成日志文本
string to_log_text(const TEsCashCheckReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"出入金状态(CashState)\":" << value->CashState;
	ostr << ",\"是否强制出金标记(ForceCashOutFlag)\":" << value->ForceCashOutFlag;
	if (value->SerialId != 0)
		ostr << ",\"出入金流水号(SerialId)\":\"" << value->SerialId << '\"';
	ostr.put('}');
	return ostr.str();
}
///出金入金操作通知结构 记录到日志
void log(const TEsCashCheckReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///出金入金查询请求结构 生成日志文本
string to_log_text(const TEsCashOperQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///出金入金查询请求结构 记录到日志
void log(const TEsCashOperQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///出金入金查询应答结构 生成日志文本
string to_log_text(const TEsCashOperQryRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->SerialId != 0)
		ostr << ",\"出入金流水号(SerialId)\":\"" << value->SerialId << '\"';
	ostr << ",\"出入金状态(CashState)\":" << value->CashState;
	if (strlen(value->OperDateTime) > 0)
		ostr << ",\"操作时间(OperDateTime)\":\"" << value->OperDateTime << '\"';
	if (strlen(value->OperatorNo) > 0)
		ostr << ",\"操作人(OperatorNo)\":\"" << value->OperatorNo << '\"';
	if (strlen(value->CheckDateTime) > 0)
		ostr << ",\"审核时间(CheckDateTime)\":\"" << value->CheckDateTime << '\"';
	if (strlen(value->CheckOperatorNo) > 0)
		ostr << ",\"审核人(CheckOperatorNo)\":\"" << value->CheckOperatorNo << '\"';
	ostr << ",\"出入金类型(CashType)\":" << value->CashType;
	ostr << ",\"出入金方式(CashMode)\":" << value->CashMode;
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	if (value->CashValue != 0)
		ostr << ",\"操作金额(CashValue)\":\"" << value->CashValue << '\"';
	if (strlen(value->CashRemark) > 0)
		ostr << ",\"备注(CashRemark)\":\"" << value->CashRemark << '\"';
	if (strlen(value->ClientBank) > 0)
		ostr << ",\"客户银行标识(ClientBank)\":\"" << value->ClientBank << '\"';
	if (strlen(value->ClientAccount) > 0)
		ostr << ",\"客户银行账户(ClientAccount)\":\"" << value->ClientAccount << '\"';
	ostr << ",\"客户本外币账户标识(ClientLWFlag)\":" << value->ClientLWFlag;
	if (strlen(value->CompanyBank) > 0)
		ostr << ",\"公司银行标识(CompanyBank)\":\"" << value->CompanyBank << '\"';
	if (strlen(value->CompanyAccount) > 0)
		ostr << ",\"公司银行账户(CompanyAccount)\":\"" << value->CompanyAccount << '\"';
	ostr << ",\"公司本外币账户标识(CompanyLWFlag)\":" << value->CompanyLWFlag;
	ostr << ",\"查询请求数据(ReqData)\":" << to_log_text(&value->ReqData);
	ostr.put('}');
	return ostr.str();
}
///出金入金查询应答结构 记录到日志
void log(const TEsCashOperQryRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///出金入金操作请求结构 生成日志文本
string to_log_text(const TEsCashOperReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"出入金类型(CashType)\":" << value->CashType;
	ostr << ",\"出入金方式(CashMode)\":" << value->CashMode;
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	if (value->CashValue != 0)
		ostr << ",\"操作金额(CashValue)\":\"" << value->CashValue << '\"';
	if (strlen(value->CashRemark) > 0)
		ostr << ",\"备注(CashRemark)\":\"" << value->CashRemark << '\"';
	if (strlen(value->ClientBank) > 0)
		ostr << ",\"客户银行标识(ClientBank)\":\"" << value->ClientBank << '\"';
	if (strlen(value->ClientAccount) > 0)
		ostr << ",\"客户银行账户(ClientAccount)\":\"" << value->ClientAccount << '\"';
	ostr << ",\"客户本外币账户标识(ClientLWFlag)\":" << value->ClientLWFlag;
	if (strlen(value->CompanyBank) > 0)
		ostr << ",\"公司银行标识(CompanyBank)\":\"" << value->CompanyBank << '\"';
	if (strlen(value->CompanyAccount) > 0)
		ostr << ",\"公司银行账户(CompanyAccount)\":\"" << value->CompanyAccount << '\"';
	ostr << ",\"公司本外币账户标识(CompanyLWFlag)\":" << value->CompanyLWFlag;
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///出金入金操作请求结构 记录到日志
void log(const TEsCashOperReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///出金入金操作应答结构 生成日志文本
string to_log_text(const TEsCashOperRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->SerialId != 0)
		ostr << ",\"出入金流水号(SerialId)\":\"" << value->SerialId << '\"';
	ostr << ",\"出入金状态(CashState)\":" << value->CashState;
	if (strlen(value->OperDateTime) > 0)
		ostr << ",\"操作时间(OperDateTime)\":\"" << value->OperDateTime << '\"';
	if (strlen(value->OperatorNo) > 0)
		ostr << ",\"操作人(OperatorNo)\":\"" << value->OperatorNo << '\"';
	if (strlen(value->CheckDateTime) > 0)
		ostr << ",\"审核时间(CheckDateTime)\":\"" << value->CheckDateTime << '\"';
	if (strlen(value->CheckOperatorNo) > 0)
		ostr << ",\"审核人(CheckOperatorNo)\":\"" << value->CheckOperatorNo << '\"';
	ostr << ",\"出入金操作请求数据(ReqData)\":" << to_log_text(&value->ReqData);
	ostr.put('}');
	return ostr.str();
}
///出金入金操作应答结构 记录到日志
void log(const TEsCashOperRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///客户认证密码验证请求结构 生成日志文本
string to_log_text(const TEsClientPasswordAuthReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->Password) > 0)
		ostr << ",\"密码(Password)\":\"" << value->Password << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户编号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///客户认证密码验证请求结构 记录到日志
void log(const TEsClientPasswordAuthReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///客户认证密码验证应答结构 生成日志文本
string to_log_text(const TEsClientPasswordAuthRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户编号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///客户认证密码验证应答结构 记录到日志
void log(const TEsClientPasswordAuthRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///客户密码修改请求结构 生成日志文本
string to_log_text(const TEsClientPasswordModifyReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"密码类型(PasswordType)\":" << value->PasswordType;
	if (strlen(value->OldPassword) > 0)
		ostr << ",\"操作员修改客户密码时(OldPassword)\":\"" << value->OldPassword << '\"';
	if (strlen(value->NewPassword) > 0)
		ostr << ",\"新密码(NewPassword)\":\"" << value->NewPassword << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///客户密码修改请求结构 记录到日志
void log(const TEsClientPasswordModifyReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///客户密码修改应答结构 生成日志文本
string to_log_text(const TEsClientPasswordModifyRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"密码类型(PasswordType)\":" << value->PasswordType;
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///客户密码修改应答结构 记录到日志
void log(const TEsClientPasswordModifyRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///商品查询请求结构 生成日志文本
string to_log_text(const TEsCommodityQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr.put('}');
	return ostr.str();
}
///商品查询请求结构 记录到日志
void log(const TEsCommodityQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///商品查询应答结构 生成日志文本
string to_log_text(const TEsCommodityQryRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"商品类型(CommodityType)\":" << value->CommodityType;
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->RelateCommodityNo) > 0)
		ostr << ",\"依赖合约(RelateCommodityNo)\":\"" << value->RelateCommodityNo << '\"';
	if (strlen(value->CommodityName) > 0)
		ostr << ",\"商品名称(CommodityName)\":\"" << value->CommodityName << '\"';
	if (strlen(value->CommodityAttribute) > 0)
		ostr << ",\"商品属性(CommodityAttribute)\":\"" << value->CommodityAttribute << '\"';
	ostr << ",\"商品状态(CommodityState)\":" << value->CommodityState;
	if (value->ProductDot != 0)
		ostr << ",\"每手乘数(ProductDot)\":\"" << value->ProductDot << '\"';
	if (value->UpperTick != 0)
		ostr << ",\"最小变动价分子(UpperTick)\":\"" << value->UpperTick << '\"';
	if (value->LowerTick != 0)
		ostr << ",\"最小变动价分母(LowerTick)\":\"" << value->LowerTick << '\"';
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"商品使用币种(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	ostr << ",\"交割方式(DeliveryMode)\":" << value->DeliveryMode;
	if (value->DeliveryDays != 0)
		ostr << ",\"交割日偏移(DeliveryDays)\":\"" << value->DeliveryDays << '\"';
	ostr << ",\"保证金计算方式(DepositCalculateMode)\":" << value->DepositCalculateMode;
	if (value->MaxSingleOrderVol != 0)
		ostr << ",\"单笔最大下单量(MaxSingleOrderVol)\":\"" << value->MaxSingleOrderVol << '\"';
	if (value->MaxHoldVol != 0)
		ostr << ",\"最大持仓量(MaxHoldVol)\":\"" << value->MaxHoldVol << '\"';
	if (strlen(value->AddOneTime) > 0)
		ostr << ",\"T+1时间(AddOneTime)\":\"" << value->AddOneTime << '\"';
	ostr << ",\"组合买卖方向(第一腿(CmbDirect)\":" << value->CmbDirect;
	ostr << ",\"平仓方式(CoverMode)\":" << value->CoverMode;
	if (strlen(value->ExchangeNo) > 0)
		ostr << ",\"交易所(ExchangeNo)\":\"" << value->ExchangeNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///商品查询应答结构 记录到日志
void log(const TEsCommodityQryRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///商品状态变化通知结构 生成日志文本
string to_log_text(const TEsCommodityStateModNoticeField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"商品状态(CommodityState)\":" << value->CommodityState;
	if (value->MaxSingleOrderVol != 0)
		ostr << ",\"单笔最大下单量(MaxSingleOrderVol)\":\"" << value->MaxSingleOrderVol << '\"';
	if (value->MaxHoldVol != 0)
		ostr << ",\"最大持仓量(MaxHoldVol)\":\"" << value->MaxHoldVol << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"上边三个字段为商品关键字(CommodityNo)\":\"" << value->CommodityNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///商品状态变化通知结构 记录到日志
void log(const TEsCommodityStateModNoticeField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///合约查询请求结构 生成日志文本
string to_log_text(const TEsContractQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->LastDays != 0)
		ostr << ",\"临近日期数(LastDays)\":\"" << value->LastDays << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///合约查询请求结构 记录到日志
void log(const TEsContractQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///合约查询应答结构 生成日志文本
string to_log_text(const TEsContractQryRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合约(ContractNo)\":\"" << value->ContractNo << '\"';
	if (strlen(value->ContractName) > 0)
		ostr << ",\"合约名称(ContractName)\":\"" << value->ContractName << '\"';
	ostr << ",\"合约类型(ContractType)\":" << value->ContractType;
	ostr << ",\"合约状态(ContractState)\":" << value->ContractState;
	if (strlen(value->ExpiryDate) > 0)
		ostr << ",\"到期日(ExpiryDate)\":\"" << value->ExpiryDate << '\"';
	if (strlen(value->LastTradeDate) > 0)
		ostr << ",\"最后交易日(LastTradeDate)\":\"" << value->LastTradeDate << '\"';
	if (strlen(value->FirstNoticeDate) > 0)
		ostr << ",\"首次通知日(FirstNoticeDate)\":\"" << value->FirstNoticeDate << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///合约查询应答结构 记录到日志
void log(const TEsContractQryRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///币种查询请求结构 生成日志文本
string to_log_text(const TEsCurrencyQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr.put('}');
	return ostr.str();
}
///币种查询请求结构 记录到日志
void log(const TEsCurrencyQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///币种查询应答结构 生成日志文本
string to_log_text(const TEsCurrencyQryRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->CurrencyName) > 0)
		ostr << ",\"货币名称(CurrencyName)\":\"" << value->CurrencyName << '\"';
	ostr << ",\"是否主货币(IsPrimary)\":" << value->IsPrimary;
	ostr << ",\"货币组别(CurrencyGroup)\":" << value->CurrencyGroup;
	if (value->ExchangeRate != 0)
		ostr << ",\"汇率(ExchangeRate)\":\"" << value->ExchangeRate << '\"';
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///币种查询应答结构 记录到日志
void log(const TEsCurrencyQryRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///市场查询请求结构 生成日志文本
string to_log_text(const TEsExchangeQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr.put('}');
	return ostr.str();
}
///市场查询请求结构 记录到日志
void log(const TEsExchangeQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///市场查询应答结构 生成日志文本
string to_log_text(const TEsExchangeQryRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ExchangeName) > 0)
		ostr << ",\"交易所名称(ExchangeName)\":\"" << value->ExchangeName << '\"';
	ostr << ",\"交易所状态(ExchangeState)\":" << value->ExchangeState;
	if (strlen(value->ExchangeNo) > 0)
		ostr << ",\"交易所(ExchangeNo)\":\"" << value->ExchangeNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///市场查询应答结构 记录到日志
void log(const TEsExchangeQryRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///汇率变更数据 生成日志文本
string to_log_text(const TEsExchangeRateModifyNoticeField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->ExchangeRate != 0)
		ostr << ",\"汇率(ExchangeRate)\":\"" << value->ExchangeRate << '\"';
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///汇率变更数据 记录到日志
void log(const TEsExchangeRateModifyNoticeField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///市场状态修改通知结构 生成日志文本
string to_log_text(const TEsExchangeStateModifyNoticeField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"交换状态(ExchangeState)\":" << value->ExchangeState;
	if (strlen(value->ExchangeNo) > 0)
		ostr << ",\"交换不(ExchangeNo)\":\"" << value->ExchangeNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///市场状态修改通知结构 记录到日志
void log(const TEsExchangeStateModifyNoticeField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///历史资金调整查询请求结构 生成日志文本
string to_log_text(const TEsHisAdjustQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->BeginDate) > 0)
		ostr << ",\"起始日期(BeginDate)\":\"" << value->BeginDate << '\"';
	if (strlen(value->EndDate) > 0)
		ostr << ",\"终止日期(EndDate)\":\"" << value->EndDate << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///历史资金调整查询请求结构 记录到日志
void log(const TEsHisAdjustQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///历史资金调整查询应答结构 生成日志文本
string to_log_text(const TEsHisAdjustQryRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->Date) > 0)
		ostr << ",\"日期(Date)\":\"" << value->Date << '\"';
	ostr << ",\"数据(Data)\":" << to_log_text(&value->Data);
	ostr.put('}');
	return ostr.str();
}
///历史资金调整查询应答结构 记录到日志
void log(const TEsHisAdjustQryRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///历史出入金查询请求结构 生成日志文本
string to_log_text(const TEsHisCashOperQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->BeginDate) > 0)
		ostr << ",\"起始日期(BeginDate)\":\"" << value->BeginDate << '\"';
	if (strlen(value->EndDate) > 0)
		ostr << ",\"终止日期(EndDate)\":\"" << value->EndDate << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///历史出入金查询请求结构 记录到日志
void log(const TEsHisCashOperQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///历史出入金查询应答结构 生成日志文本
string to_log_text(const TEsHisCashOperQryRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->Date) > 0)
		ostr << ",\"日期(Date)\":\"" << value->Date << '\"';
	ostr << ",\"数据(Data)\":" << to_log_text(&value->Data);
	ostr.put('}');
	return ostr.str();
}
///历史出入金查询应答结构 记录到日志
void log(const TEsHisCashOperQryRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///历史成交查询请求结构 生成日志文本
string to_log_text(const TEsHisMatchQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->BeginDate) > 0)
		ostr << ",\"起始日期(BeginDate)\":\"" << value->BeginDate << '\"';
	if (strlen(value->EndDate) > 0)
		ostr << ",\"结束日期(EndDate)\":\"" << value->EndDate << '\"';
	ostr << ",\"是否包含合计值(IsContainTotle)\":" << value->IsContainTotle;
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///历史成交查询请求结构 记录到日志
void log(const TEsHisMatchQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///历史成交查询应答结构 生成日志文本
string to_log_text(const TEsHisMatchQryRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	if (strlen(value->SettleNo) > 0)
		ostr << ",\"结算用成交编号(SettleNo)\":\"" << value->SettleNo << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合约(ContractNo)\":\"" << value->ContractNo << '\"';
	ostr << ",\"成交方式(MatchWay)\":" << value->MatchWay;
	ostr << ",\"买卖方向(Direct)\":" << value->Direct;
	ostr << ",\"开平类型(Offset)\":" << value->Offset;
	if (value->MatchVol != 0)
		ostr << ",\"成交数量(MatchVol)\":\"" << value->MatchVol << '\"';
	if (value->MatchPrice != 0)
		ostr << ",\"成交价格(MatchPrice)\":\"" << value->MatchPrice << '\"';
	if (value->Premium != 0)
		ostr << ",\"权利金(Premium)\":\"" << value->Premium << '\"';
	if (value->Turnover != 0)
		ostr << ",\"成交金额(Turnover)\":\"" << value->Turnover << '\"';
	if (value->ClientFee != 0)
		ostr << ",\"客户手续费(ClientFee)\":\"" << value->ClientFee << '\"';
	if (strlen(value->GroupNo) > 0)
		ostr << ",\"结算分组(GroupNo)\":\"" << value->GroupNo << '\"';
	if (value->ManualFee != 0)
		ostr << ",\"人工手续费(ManualFee)\":\"" << value->ManualFee << '\"';
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"币种(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	if (strlen(value->CommodityCurrencyNo) > 0)
		ostr << ",\"品种币种(CommodityCurrencyNo)\":\"" << value->CommodityCurrencyNo << '\"';
	ostr << ",\"是否风险报单(IsRiskOrder)\":" << value->IsRiskOrder;
	if (strlen(value->SystemNo) > 0)
		ostr << ",\"系统号(SystemNo)\":\"" << value->SystemNo << '\"';
	if (strlen(value->MatchNo) > 0)
		ostr << ",\"成交号(MatchNo)\":\"" << value->MatchNo << '\"';
	if (value->MatchStreamID != 0)
		ostr << ",\"成交流号(MatchStreamID)\":\"" << value->MatchStreamID << '\"';
	if (strlen(value->MatchDateTime) > 0)
		ostr << ",\"成交时间(MatchDateTime)\":\"" << value->MatchDateTime << '\"';
	ostr << ",\"是否风险报单(MatchMode)\":" << value->MatchMode;
	ostr << ",\"委托类型(OrderType)\":" << value->OrderType;
	if (value->CoverPrice != 0)
		ostr << ",\"平仓价格(CoverPrice)\":\"" << value->CoverPrice << '\"';
	if (value->CoverProfit != 0)
		ostr << ",\"平仓盈亏(CoverProfit)\":\"" << value->CoverProfit << '\"';
	if (strlen(value->Date) > 0)
		ostr << ",\"结算日期(Date)\":\"" << value->Date << '\"';
	ostr.put('}');
	return ostr.str();
}
///历史成交查询应答结构 记录到日志
void log(const TEsHisMatchQryRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///委托变更流程查询应答命令 生成日志文本
string to_log_text(const TEsHisOrderProcessQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->Date) > 0)
		ostr << ",\"查询日期(Date)\":\"" << value->Date << '\"';
	if (value->OrderId != 0)
		ostr << ",\"委托ID(OrderId)\":\"" << value->OrderId << '\"';
	ostr.put('}');
	return ostr.str();
}
///委托变更流程查询应答命令 记录到日志
void log(const TEsHisOrderProcessQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///历史委托流程查询应答结构 生成日志文本
string to_log_text(const TEsHisOrderProcessQryRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->Date) > 0)
		ostr << ",\"日期(Date)\":\"" << value->Date << '\"';
	ostr << ",\"委托流程数据(Data)\":" << to_log_text(&value->Data);
	ostr.put('}');
	return ostr.str();
}
///历史委托流程查询应答结构 记录到日志
void log(const TEsHisOrderProcessQryRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///历史委托查询请求结构 生成日志文本
string to_log_text(const TEsHisOrderQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->BeginDate) > 0)
		ostr << ",\"起始日期(BeginDate)\":\"" << value->BeginDate << '\"';
	if (strlen(value->EndDate) > 0)
		ostr << ",\"结束日期(EndDate)\":\"" << value->EndDate << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///历史委托查询请求结构 记录到日志
void log(const TEsHisOrderQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///历史委托查询应答结构 生成日志文本
string to_log_text(const TEsHisOrderQryRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->Date) > 0)
		ostr << ",\"委托日期(Date)\":\"" << value->Date << '\"';
	ostr << ",\"委托数据(Data)\":" << to_log_text(&value->Data);
	ostr.put('}');
	return ostr.str();
}
///历史委托查询应答结构 记录到日志
void log(const TEsHisOrderQryRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///监控事件通知结构 生成日志文本
string to_log_text(const TEsHKMarketOrderReq* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品编号(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合同号(ContractNo)\":\"" << value->ContractNo << '\"';
	ostr << ",\"委托类型(OrderType)\":" << value->OrderType;
	ostr << ",\"委托模式(OrderMode)\":" << value->OrderMode;
	if (strlen(value->ValidDateTime) > 0)
		ostr << ",\"有效日期时间(ValidDateTime)\":\"" << value->ValidDateTime << '\"';
	if (value->BuyPrice != 0)
		ostr << ",\"购买价格(BuyPrice)\":\"" << value->BuyPrice << '\"';
	if (value->SellPrice != 0)
		ostr << ",\"出售价格(SellPrice)\":\"" << value->SellPrice << '\"';
	if (value->BuyVol != 0)
		ostr << ",\"买卷(BuyVol)\":\"" << value->BuyVol << '\"';
	if (value->SellVol != 0)
		ostr << ",\"卖卷(SellVol)\":\"" << value->SellVol << '\"';
	ostr << ",\"购买操作(BuyOper)\":" << value->BuyOper;
	ostr << ",\"销售工作(SellOper)\":" << value->SellOper;
	if (value->BuyOrderID != 0)
		ostr << ",\"购买订单ID(BuyOrderID)\":\"" << value->BuyOrderID << '\"';
	if (value->SellOrderID != 0)
		ostr << ",\"销售订单ID(SellOrderID)\":\"" << value->SellOrderID << '\"';
	if (strlen(value->SaveString) > 0)
		ostr << ",\"保存字符串(SaveString)\":\"" << value->SaveString << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户编号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///监控事件通知结构 记录到日志
void log(const TEsHKMarketOrderReq* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///持仓查询请求结构 生成日志文本
string to_log_text(const TEsHoldQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ExchangeNo) > 0)
		ostr << ",\"交易所(ExchangeNo)\":\"" << value->ExchangeNo << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合约(ContractNo)\":\"" << value->ContractNo << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///持仓查询请求结构 记录到日志
void log(const TEsHoldQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///持仓查询应答结构 生成日志文本
string to_log_text(const TEsHoldQryRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合约(ContractNo)\":\"" << value->ContractNo << '\"';
	ostr << ",\"买卖方向(Direct)\":" << value->Direct;
	ostr << ",\"投机保值(Hedge)\":" << value->Hedge;
	if (value->TradePrice != 0)
		ostr << ",\"持仓均价(TradePrice)\":\"" << value->TradePrice << '\"';
	if (value->TradeVol != 0)
		ostr << ",\"持仓量(TradeVol)\":\"" << value->TradeVol << '\"';
	if (value->YSettlePrice != 0)
		ostr << ",\"昨结算价(YSettlePrice)\":\"" << value->YSettlePrice << '\"';
	if (value->TNewPrice != 0)
		ostr << ",\"最新价(TNewPrice)\":\"" << value->TNewPrice << '\"';
	if (strlen(value->MatchDateTime) > 0)
		ostr << ",\"成交时间(MatchDateTime)\":\"" << value->MatchDateTime << '\"';
	if (strlen(value->MatchNo) > 0)
		ostr << ",\"成交号(MatchNo)\":\"" << value->MatchNo << '\"';
	if (value->Deposit != 0)
		ostr << ",\"保证金(Deposit)\":\"" << value->Deposit << '\"';
	if (value->KeepDeposit != 0)
		ostr << ",\"维持保证金(KeepDeposit)\":\"" << value->KeepDeposit << '\"';
	if (value->HoldKeyId != 0)
		ostr << ",\"持仓关键字(HoldKeyId)\":\"" << value->HoldKeyId << '\"';
	ostr.put('}');
	return ostr.str();
}
///持仓查询应答结构 记录到日志
void log(const TEsHoldQryRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///登录请求结构 生成日志文本
string to_log_text(const TEsLoginReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"登录身份类型(Identity)\":" << value->Identity;
	ostr << ",\"是否强制修改密码(IsForcePwd)\":" << value->IsForcePwd;
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr << ",\"是否CA认证(IsCaLogin)\":" << value->IsCaLogin;
	ostr.put('}');
	return ostr.str();
}
///登录请求结构 记录到日志
void log(const TEsLoginReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///登录应答结构 生成日志文本
string to_log_text(const TEsLoginRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"是否强制修改密码(IsForcePwd)\":" << value->IsForcePwd;
	if (strlen(value->LoginNo) > 0)
		ostr << ",\"登录号(LoginNo)\":\"" << value->LoginNo << '\"';
	if (strlen(value->LoginName) > 0)
		ostr << ",\"登录端帐号简称(LoginName)\":\"" << value->LoginName << '\"';
	if (strlen(value->ReservedInfo) > 0)
		ostr << ",\"客户预留信息(ReservedInfo)\":\"" << value->ReservedInfo << '\"';
	if (strlen(value->LastLoginDateTime) > 0)
		ostr << ",\"上次登录时间(LastLoginDateTime)\":\"" << value->LastLoginDateTime << '\"';
	if (strlen(value->LastLogoutDateTime) > 0)
		ostr << ",\"上次登出时间(LastLogoutDateTime)\":\"" << value->LastLogoutDateTime << '\"';
	if (strlen(value->LastLoginIp) > 0)
		ostr << ",\"上次登录ip(LastLoginIp)\":\"" << value->LastLoginIp << '\"';
	if (value->LastLoginPort != 0)
		ostr << ",\"上次登录port(LastLoginPort)\":\"" << value->LastLoginPort << '\"';
	if (strlen(value->LastLoginMachineInfo) > 0)
		ostr << ",\"上次登录机器信息(LastLoginMachineInfo)\":\"" << value->LastLoginMachineInfo << '\"';
	if (strlen(value->ServerDateTime) > 0)
		ostr << ",\"系统当前时间(客户端校时(ServerDateTime)\":\"" << value->ServerDateTime << '\"';
	ostr << ",\"是否CA认证(IsCaLogin)\":" << value->IsCaLogin;
	ostr.put('}');
	return ostr.str();
}
///登录应答结构 记录到日志
void log(const TEsLoginRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///成交信息数据 生成日志文本
string to_log_text(const TEsMatchInfoNoticeField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"跨品种套利需要(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"跨期套利需要(ContractNo)\":\"" << value->ContractNo << '\"';
	ostr << ",\"套利报单需要(Direct)\":" << value->Direct;
	ostr << ",\"互换报单需要(Offset)\":" << value->Offset;
	ostr << ",\"成交数据(StateData)\":" << to_log_text(&value->StateData);
	ostr.put('}');
	return ostr.str();
}
///成交信息数据 记录到日志
void log(const TEsMatchInfoNoticeField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///删除成交通知结构 生成日志文本
string to_log_text(const TEsMatchRemoveNoticeField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->SystemNo) > 0)
		ostr << ",\"系统编号(SystemNo)\":\"" << value->SystemNo << '\"';
	if (strlen(value->MatchNo) > 0)
		ostr << ",\"比赛编号(MatchNo)\":\"" << value->MatchNo << '\"';
	if (value->MatchId != 0)
		ostr << ",\"比赛ID(MatchId)\":\"" << value->MatchId << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户编号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///删除成交通知结构 记录到日志
void log(const TEsMatchRemoveNoticeField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///成交状态数据 生成日志文本
string to_log_text(const TEsMatchStateNoticeField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	if (strlen(value->SystemNo) > 0)
		ostr << ",\"系统号(SystemNo)\":\"" << value->SystemNo << '\"';
	if (strlen(value->MatchNo) > 0)
		ostr << ",\"成交号(MatchNo)\":\"" << value->MatchNo << '\"';
	ostr << ",\"成交模式(MatchMode)\":" << value->MatchMode;
	ostr << ",\"成交方式式(MatchWay)\":" << value->MatchWay;
	if (value->MatchPrice != 0)
		ostr << ",\"成交价(MatchPrice)\":\"" << value->MatchPrice << '\"';
	if (value->MatchVol != 0)
		ostr << ",\"成交数量(MatchVol)\":\"" << value->MatchVol << '\"';
	if (strlen(value->MatchDateTime) > 0)
		ostr << ",\"成交时间(MatchDateTime)\":\"" << value->MatchDateTime << '\"';
	if (value->MatchFee != 0)
		ostr << ",\"成交费用(MatchFee)\":\"" << value->MatchFee << '\"';
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"手续费币种(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	ostr << ",\"T+1标记(AddOne)\":" << value->AddOne;
	ostr << ",\"手工手续费(ManualFee)\":" << value->ManualFee;
	ostr << ",\"删除标志(Deleted)\":" << value->Deleted;
	if (value->OrderId != 0)
		ostr << ",\"委托ID(OrderId)\":\"" << value->OrderId << '\"';
	if (value->MatchId != 0)
		ostr << ",\"成交ID(MatchId)\":\"" << value->MatchId << '\"';
	if (value->CoverPrice != 0)
		ostr << ",\"平仓价格(CoverPrice)\":\"" << value->CoverPrice << '\"';
	if (value->MatchStreamId != 0)
		ostr << ",\"成交流号(MatchStreamId)\":\"" << value->MatchStreamId << '\"';
	ostr.put('}');
	return ostr.str();
}
///成交状态数据 记录到日志
void log(const TEsMatchStateNoticeField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///资金变更消息推送 生成日志文本
string to_log_text(const TEsMoneyChgNoticeField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	if (value->MoneyChgNum != 0)
		ostr << ",\"资金变化项的个数(MoneyChgNum)\":\"" << value->MoneyChgNum << '\"';
	ostr << ",\"资金变化内容(MoneyItem)\":" << to_log_text(value->MoneyItem);
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///资金变更消息推送 记录到日志
void log(const TEsMoneyChgNoticeField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///资金查询请求结构 生成日志文本
string to_log_text(const TEsMoneyQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///资金查询请求结构 记录到日志
void log(const TEsMoneyQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///资金查询应答结构(注意次结构与资金变化通知结构的关系 生成日志文本
string to_log_text(const TEsMoneyQryRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"货币编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	if (value->YAvailable != 0)
		ostr << ",\"昨可用(YAvailable)\":\"" << value->YAvailable << '\"';
	if (value->YCanCashOut != 0)
		ostr << ",\"昨可提(YCanCashOut)\":\"" << value->YCanCashOut << '\"';
	if (value->YMoney != 0)
		ostr << ",\"昨账面(YMoney)\":\"" << value->YMoney << '\"';
	if (value->YBalance != 0)
		ostr << ",\"昨权益(YBalance)\":\"" << value->YBalance << '\"';
	if (value->YUnExpiredProfit != 0)
		ostr << ",\"昨未结平盈(YUnExpiredProfit)\":\"" << value->YUnExpiredProfit << '\"';
	if (value->Adjust != 0)
		ostr << ",\"资金调整0(Adjust)\":\"" << value->Adjust << '\"';
	if (value->CashIn != 0)
		ostr << ",\"入金(CashIn)\":\"" << value->CashIn << '\"';
	if (value->CashOut != 0)
		ostr << ",\"出金(CashOut)\":\"" << value->CashOut << '\"';
	if (value->Fee != 0)
		ostr << ",\"手续费(Fee)\":\"" << value->Fee << '\"';
	if (value->Frozen != 0)
		ostr << ",\"冻结资金(Frozen)\":\"" << value->Frozen << '\"';
	if (value->CoverProfit != 0)
		ostr << ",\"逐笔平盈(CoverProfit)\":\"" << value->CoverProfit << '\"';
	if (value->DayCoverProfit != 0)
		ostr << ",\"盯市平盈(DayCoverProfit)\":\"" << value->DayCoverProfit << '\"';
	if (value->FloatProfit != 0)
		ostr << ",\"逐笔浮盈(FloatProfit)\":\"" << value->FloatProfit << '\"';
	if (value->DayFloatProfit != 0)
		ostr << ",\"盯市浮盈8(DayFloatProfit)\":\"" << value->DayFloatProfit << '\"';
	if (value->UnExpiredProfit != 0)
		ostr << ",\"未结平盈9(UnExpiredProfit)\":\"" << value->UnExpiredProfit << '\"';
	if (value->Premium != 0)
		ostr << ",\"权利金10(Premium)\":\"" << value->Premium << '\"';
	if (value->Deposit != 0)
		ostr << ",\"保证金11(Deposit)\":\"" << value->Deposit << '\"';
	if (value->KeepDeposit != 0)
		ostr << ",\"维持保证金12(KeepDeposit)\":\"" << value->KeepDeposit << '\"';
	if (value->Pledge != 0)
		ostr << ",\"质押资金13(Pledge)\":\"" << value->Pledge << '\"';
	if (value->TAvailable != 0)
		ostr << ",\"可用资金14(TAvailable)\":\"" << value->TAvailable << '\"';
	if (value->Discount != 0)
		ostr << ",\"贴现金额15(Discount)\":\"" << value->Discount << '\"';
	if (value->TradeFee != 0)
		ostr << ",\"交易手续费16(TradeFee)\":\"" << value->TradeFee << '\"';
	if (value->DeliveryFee != 0)
		ostr << ",\"交割手续费17(DeliveryFee)\":\"" << value->DeliveryFee << '\"';
	if (value->ExchangeFee != 0)
		ostr << ",\"汇兑手续费18(ExchangeFee)\":\"" << value->ExchangeFee << '\"';
	if (value->FrozenDeposit != 0)
		ostr << ",\"冻结保证金19(FrozenDeposit)\":\"" << value->FrozenDeposit << '\"';
	if (value->FrozenFee != 0)
		ostr << ",\"冻结手续费20(FrozenFee)\":\"" << value->FrozenFee << '\"';
	if (value->NewFloatProfit != 0)
		ostr << ",\"浮盈(无LME)21(NewFloatProfit)\":\"" << value->NewFloatProfit << '\"';
	if (value->LmeFloatProfit != 0)
		ostr << ",\"LME浮盈22(LmeFloatProfit)\":\"" << value->LmeFloatProfit << '\"';
	if (value->OptionMarketValue != 0)
		ostr << ",\"期权市值23(OptionMarketValue)\":\"" << value->OptionMarketValue << '\"';
	if (value->OriCash != 0)
		ostr << ",\"币种原始出入金24(非自动汇兑资金(OriCash)\":\"" << value->OriCash << '\"';
	if (value->TMoney != 0)
		ostr << ",\"今资金(TMoney)\":\"" << value->TMoney << '\"';
	if (value->TBalance != 0)
		ostr << ",\"今权益(TBalance)\":\"" << value->TBalance << '\"';
	if (value->TCanCashOut != 0)
		ostr << ",\"今可提(TCanCashOut)\":\"" << value->TCanCashOut << '\"';
	if (value->RiskRate != 0)
		ostr << ",\"风险率(RiskRate)\":\"" << value->RiskRate << '\"';
	if (value->AccountMarketValue != 0)
		ostr << ",\"账户市值(AccountMarketValue)\":\"" << value->AccountMarketValue << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///资金查询应答结构(注意次结构与资金变化通知结构的关系 记录到日志
void log(const TEsMoneyQryRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///监控事件查询请求结构 生成日志文本
string to_log_text(const TEsMonitorEventQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr.put('}');
	return ostr.str();
}
///监控事件查询请求结构 记录到日志
void log(const TEsMonitorEventQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///监控事件查询应答结构 生成日志文本
string to_log_text(const TEsMonitorEventQryRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"事件级别(EventLevel)\":" << value->EventLevel;
	if (strlen(value->EventSource) > 0)
		ostr << ",\"事件源(EventSource)\":\"" << value->EventSource << '\"';
	if (strlen(value->EventContent) > 0)
		ostr << ",\"事件内容(EventContent)\":\"" << value->EventContent << '\"';
	if (value->SerialId != 0)
		ostr << ",\"序号编号(SerialId)\":\"" << value->SerialId << '\"';
	if (strlen(value->EventDateTime) > 0)
		ostr << ",\"活动日期时间(EventDateTime)\":\"" << value->EventDateTime << '\"';
	ostr << ",\"事件类型(EventType)\":" << value->EventType;
	ostr.put('}');
	return ostr.str();
}
///监控事件查询应答结构 记录到日志
void log(const TEsMonitorEventQryRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///操作员下属客户查询请求结构 生成日志文本
string to_log_text(const TEsOperatorClientQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->OperatorNo) > 0)
		ostr << ",\"操作员号(OperatorNo)\":\"" << value->OperatorNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///操作员下属客户查询请求结构 记录到日志
void log(const TEsOperatorClientQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///操作员下属客户查询应答结构 生成日志文本
string to_log_text(const TEsOperatorClientQryRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///操作员下属客户查询应答结构 记录到日志
void log(const TEsOperatorClientQryRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///操作员密码修改请求结构 生成日志文本
string to_log_text(const TEsOperatorPasswordModifyReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->OldPassword) > 0)
		ostr << ",\"旧密码(OldPassword)\":\"" << value->OldPassword << '\"';
	if (strlen(value->NewPassword) > 0)
		ostr << ",\"新密码(NewPassword)\":\"" << value->NewPassword << '\"';
	if (strlen(value->OperatorNo) > 0)
		ostr << ",\"操作员号(OperatorNo)\":\"" << value->OperatorNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///操作员密码修改请求结构 记录到日志
void log(const TEsOperatorPasswordModifyReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///操作员密码修改应答结构 生成日志文本
string to_log_text(const TEsOperatorPasswordModifyRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->OperatorNo) > 0)
		ostr << ",\"操作员号(OperatorNo)\":\"" << value->OperatorNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///操作员密码修改应答结构 记录到日志
void log(const TEsOperatorPasswordModifyRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///撤单请求结构 生成日志文本
string to_log_text(const TEsOrderDeleteReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OrderId != 0)
		ostr << ",\"委托ID(OrderId)\":\"" << value->OrderId << '\"';
	ostr.put('}');
	return ostr.str();
}
///撤单请求结构 记录到日志
void log(const TEsOrderDeleteReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///撤单应答结构 生成日志文本
string to_log_text(const TEsOrderDeleteRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"报单请求数据(ReqData)\":" << to_log_text(&value->ReqData);
	ostr << ",\"订单状态字段(OrderStateField)\":" << to_log_text(&value->OrderStateField);
	ostr.put('}');
	return ostr.str();
}
///撤单应答结构 记录到日志
void log(const TEsOrderDeleteRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///委托信息变更数据 生成日志文本
string to_log_text(const TEsOrderInfoNoticeField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OrderStreamId != 0)
		ostr << ",\"委托流号(OrderStreamId)\":\"" << value->OrderStreamId << '\"';
	if (value->OrderId != 0)
		ostr << ",\"委托ID(OrderId)\":\"" << value->OrderId << '\"';
	if (strlen(value->LocalNo) > 0)
		ostr << ",\"本地号(LocalNo)\":\"" << value->LocalNo << '\"';
	if (strlen(value->SystemNo) > 0)
		ostr << ",\"系统号(SystemNo)\":\"" << value->SystemNo << '\"';
	if (strlen(value->ExchangeSystemNo) > 0)
		ostr << ",\"交易所系统号(ExchangeSystemNo)\":\"" << value->ExchangeSystemNo << '\"';
	if (strlen(value->TradeNo) > 0)
		ostr << ",\"交易账号(TradeNo)\":\"" << value->TradeNo << '\"';
	if (strlen(value->InsertNo) > 0)
		ostr << ",\"录入操作员号(InsertNo)\":\"" << value->InsertNo << '\"';
	if (strlen(value->InsertDateTime) > 0)
		ostr << ",\"录入时间(InsertDateTime)\":\"" << value->InsertDateTime << '\"';
	if (strlen(value->UpdateNo) > 0)
		ostr << ",\"最后一次变更人(UpdateNo)\":\"" << value->UpdateNo << '\"';
	if (strlen(value->UpdateDateTime) > 0)
		ostr << ",\"最后一次变更时间(UpdateDateTime)\":\"" << value->UpdateDateTime << '\"';
	ostr << ",\"委托状态(OrderState)\":" << value->OrderState;
	if (value->MatchPrice != 0)
		ostr << ",\"成交价格(MatchPrice)\":\"" << value->MatchPrice << '\"';
	if (value->MatchVol != 0)
		ostr << ",\"成交数量(MatchVol)\":\"" << value->MatchVol << '\"';
	if (value->ErrorCode != 0)
		ostr << ",\"最后一次操作错误信息码(ErrorCode)\":\"" << value->ErrorCode << '\"';
	if (strlen(value->ErrorText) > 0)
		ostr << ",\"原始错误信息(ErrorText)\":\"" << value->ErrorText << '\"';
	if (strlen(value->ActionLocalNo) > 0)
		ostr << ",\"报单操作的本地号(ActionLocalNo)\":\"" << value->ActionLocalNo << '\"';
	ostr << ",\"是否录单(OrderInput)\":" << value->OrderInput;
	ostr << ",\"是否删除(Deleted)\":" << value->Deleted;
	ostr << ",\"T+1标志(AddOne)\":" << value->AddOne;
	if (strlen(value->ParentSystemNo) > 0)
		ostr << ",\"母系统(ParentSystemNo)\":\"" << value->ParentSystemNo << '\"';
	ostr << ",\"报单的请求数据(ReqData)\":" << to_log_text(&value->ReqData);
	ostr.put('}');
	return ostr.str();
}
///委托信息变更数据 记录到日志
void log(const TEsOrderInfoNoticeField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///报单请求结构 生成日志文本
string to_log_text(const TEsOrderInsertReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品代码(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合约代码(ContractNo)\":\"" << value->ContractNo << '\"';
	ostr << ",\"委托类型(OrderType)\":" << value->OrderType;
	ostr << ",\"委托方式(OrderWay)\":" << value->OrderWay;
	ostr << ",\"委托模式(OrderMode)\":" << value->OrderMode;
	if (strlen(value->ValidDateTime) > 0)
		ostr << ",\"有效日期(GTD情况下使用(ValidDateTime)\":\"" << value->ValidDateTime << '\"';
	ostr << ",\"风险报单(IsRiskOrder)\":" << value->IsRiskOrder;
	ostr << ",\"买入卖出(Direct)\":" << value->Direct;
	ostr << ",\"开仓平仓(Offset)\":" << value->Offset;
	ostr << ",\"投机保值(Hedge)\":" << value->Hedge;
	if (value->OrderPrice != 0)
		ostr << ",\"委托价格(OrderPrice)\":\"" << value->OrderPrice << '\"';
	if (value->TriggerPrice != 0)
		ostr << ",\"触发价格(TriggerPrice)\":\"" << value->TriggerPrice << '\"';
	if (value->OrderVol != 0)
		ostr << ",\"委托数量(OrderVol)\":\"" << value->OrderVol << '\"';
	if (value->MinMatchVol != 0)
		ostr << ",\"最小成交量(MinMatchVol)\":\"" << value->MinMatchVol << '\"';
	ostr << ",\"冰山单是否随机量发出(Randomise)\":" << value->Randomise;
	if (value->MinClipSize != 0)
		ostr << ",\"冰山单最小随机量(MinClipSize)\":\"" << value->MinClipSize << '\"';
	if (value->MaxClipSize != 0)
		ostr << ",\"冰山单最大随机量(MaxClipSize)\":\"" << value->MaxClipSize << '\"';
	if (value->SaveInt != 0)
		ostr << ",\"客户保留字段1(SaveInt)\":\"" << value->SaveInt << '\"';
	if (value->SaveDouble != 0)
		ostr << ",\"客户保留字段2(SaveDouble)\":\"" << value->SaveDouble << '\"';
	if (strlen(value->SaveString) > 0)
		ostr << ",\"客户保留字段3(SaveString)\":\"" << value->SaveString << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户编号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///报单请求结构 记录到日志
void log(const TEsOrderInsertReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///报单应答结构 生成日志文本
string to_log_text(const TEsOrderInsertRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OrderId != 0)
		ostr << ",\"委托号(OrderId)\":\"" << value->OrderId << '\"';
	if (strlen(value->LocalNo) > 0)
		ostr << ",\"本地号(LocalNo)\":\"" << value->LocalNo << '\"';
	if (strlen(value->TradeNo) > 0)
		ostr << ",\"客户交易帐号(TradeNo)\":\"" << value->TradeNo << '\"';
	if (strlen(value->InsertNo) > 0)
		ostr << ",\"下单人(InsertNo)\":\"" << value->InsertNo << '\"';
	if (strlen(value->InsertDateTime) > 0)
		ostr << ",\"下单时间(InsertDateTime)\":\"" << value->InsertDateTime << '\"';
	ostr << ",\"委托状态(OrderState)\":" << value->OrderState;
	ostr << ",\"报单请求数据(ReqData)\":" << to_log_text(&value->ReqData);
	if (value->OrderStreamId != 0)
		ostr << ",\"委托流号(OrderStreamId)\":\"" << value->OrderStreamId << '\"';
	ostr.put('}');
	return ostr.str();
}
///报单应答结构 记录到日志
void log(const TEsOrderInsertRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///改单请求结构 生成日志文本
string to_log_text(const TEsOrderModifyReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OrderPrice != 0)
		ostr << ",\"委托价格(OrderPrice)\":\"" << value->OrderPrice << '\"';
	if (value->TriggerPrice != 0)
		ostr << ",\"触发价格(TriggerPrice)\":\"" << value->TriggerPrice << '\"';
	if (value->OrderVol != 0)
		ostr << ",\"委托数量(OrderVol)\":\"" << value->OrderVol << '\"';
	if (value->OrderId != 0)
		ostr << ",\"委托ID(OrderId)\":\"" << value->OrderId << '\"';
	ostr.put('}');
	return ostr.str();
}
///改单请求结构 记录到日志
void log(const TEsOrderModifyReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///改单应答结构 生成日志文本
string to_log_text(const TEsOrderModifyRspField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"报单请求数据(ReqData)\":" << to_log_text(&value->ReqData);
	ostr << ",\"订单状态字段(OrderStateField)\":" << to_log_text(&value->OrderStateField);
	ostr.put('}');
	return ostr.str();
}
///改单应答结构 记录到日志
void log(const TEsOrderModifyRspField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///委托变更流程查询请求命令 生成日志文本
string to_log_text(const TEsOrderProcessQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OrderId != 0)
		ostr << ",\"委托ID(OrderId)\":\"" << value->OrderId << '\"';
	ostr.put('}');
	return ostr.str();
}
///委托变更流程查询请求命令 记录到日志
void log(const TEsOrderProcessQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///委托查询请求 生成日志文本
string to_log_text(const TEsOrderQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	if (strlen(value->ExchangeNo) > 0)
		ostr << ",\"交易所(ExchangeNo)\":\"" << value->ExchangeNo << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"商品(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"合约(ContractNo)\":\"" << value->ContractNo << '\"';
	ostr << ",\"委托类型(OrderType)\":" << value->OrderType;
	ostr << ",\"委托模式(OrderMode)\":" << value->OrderMode;
	ostr << ",\"风险报单(IsRiskOrder)\":" << value->IsRiskOrder;
	ostr << ",\"投机保值(Hedge)\":" << value->Hedge;
	if (value->OrderId != 0)
		ostr << ",\"委托ID(OrderId)\":\"" << value->OrderId << '\"';
	if (strlen(value->LocalNo) > 0)
		ostr << ",\"本地号(LocalNo)\":\"" << value->LocalNo << '\"';
	if (strlen(value->SystemNo) > 0)
		ostr << ",\"系统号(SystemNo)\":\"" << value->SystemNo << '\"';
	if (strlen(value->OperNo) > 0)
		ostr << ",\"下单人或操作人号(OperNo)\":\"" << value->OperNo << '\"';
	if (strlen(value->BeginInsertDateTime) > 0)
		ostr << ",\"起始时间(BeginInsertDateTime)\":\"" << value->BeginInsertDateTime << '\"';
	if (strlen(value->EndInsertDateTime) > 0)
		ostr << ",\"结束时间(EndInsertDateTime)\":\"" << value->EndInsertDateTime << '\"';
	ostr << ",\"委托状态(OrderState)\":" << value->OrderState;
	if (value->OrderStreamId != 0)
		ostr << ",\"查询条件返回大于此流号的委托数据(OrderStreamId)\":\"" << value->OrderStreamId << '\"';
	ostr.put('}');
	return ostr.str();
}
///委托查询请求 记录到日志
void log(const TEsOrderQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///删除委托通知结构 生成日志文本
string to_log_text(const TEsOrderRemoveNoticeField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->OrderId != 0)
		ostr << ",\"订单ID(OrderId)\":\"" << value->OrderId << '\"';
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户编号(ClientNo)\":\"" << value->ClientNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///删除委托通知结构 记录到日志
void log(const TEsOrderRemoveNoticeField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///委托状态变更数据 生成日志文本
string to_log_text(const TEsOrderStateNoticeField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"开平类型(Offset)\":" << value->Offset;
	ostr << ",\"投机保值类型(Hedge)\":" << value->Hedge;
	if (value->OrderPrice != 0)
		ostr << ",\"委托价(OrderPrice)\":\"" << value->OrderPrice << '\"';
	if (value->TriggerPrice != 0)
		ostr << ",\"触发价(TriggerPrice)\":\"" << value->TriggerPrice << '\"';
	if (value->OrderVol != 0)
		ostr << ",\"委托数量(OrderVol)\":\"" << value->OrderVol << '\"';
	if (value->OrderStreamId != 0)
		ostr << ",\"委托流号(OrderStreamId)\":\"" << value->OrderStreamId << '\"';
	if (value->OrderId != 0)
		ostr << ",\"委托ID(OrderId)\":\"" << value->OrderId << '\"';
	if (strlen(value->LocalNo) > 0)
		ostr << ",\"本地号(LocalNo)\":\"" << value->LocalNo << '\"';
	if (strlen(value->SystemNo) > 0)
		ostr << ",\"系统号(SystemNo)\":\"" << value->SystemNo << '\"';
	if (strlen(value->UpdateNo) > 0)
		ostr << ",\"操作员编号(UpdateNo)\":\"" << value->UpdateNo << '\"';
	if (strlen(value->UpdateDateTime) > 0)
		ostr << ",\"操作时间(UpdateDateTime)\":\"" << value->UpdateDateTime << '\"';
	ostr << ",\"委托状态(OrderState)\":" << value->OrderState;
	if (value->MatchPrice != 0)
		ostr << ",\"成交均价(MatchPrice)\":\"" << value->MatchPrice << '\"';
	if (value->MatchVol != 0)
		ostr << ",\"成交数量(MatchVol)\":\"" << value->MatchVol << '\"';
	if (value->ErrorCode != 0)
		ostr << ",\"包含错误时对应错误码(ErrorCode)\":\"" << value->ErrorCode << '\"';
	if (strlen(value->ActionLocalNo) > 0)
		ostr << ",\"报单操作的本地号(ActionLocalNo)\":\"" << value->ActionLocalNo << '\"';
	ostr << ",\"报单请求数据(ReqData)\":" << to_log_text(&value->ReqData);
	if (strlen(value->ExchangeSystemNo) > 0)
		ostr << ",\"交易所系统号(ExchangeSystemNo)\":\"" << value->ExchangeSystemNo << '\"';
	if (strlen(value->ErrorText) > 0)
		ostr << ",\"原始错误信息(ErrorText)\":\"" << value->ErrorText << '\"';
	ostr << ",\"委托类型(OrderType)\":" << value->OrderType;
	ostr.put('}');
	return ostr.str();
}
///委托状态变更数据 记录到日志
void log(const TEsOrderStateNoticeField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///委托数据应答命令 生成日志文本
string to_log_text(const TMatchQryReqField* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ClientNo) > 0)
		ostr << ",\"客户号(ClientNo)\":\"" << value->ClientNo << '\"';
	if (strlen(value->SystemNo) > 0)
		ostr << ",\"系统号(SystemNo)\":\"" << value->SystemNo << '\"';
	if (strlen(value->MatchNo) > 0)
		ostr << ",\"成交号(MatchNo)\":\"" << value->MatchNo << '\"';
	if (strlen(value->BeginMatchDateTime) > 0)
		ostr << ",\"起始时间(BeginMatchDateTime)\":\"" << value->BeginMatchDateTime << '\"';
	if (strlen(value->EndMatchDateTime) > 0)
		ostr << ",\"结束时间(EndMatchDateTime)\":\"" << value->EndMatchDateTime << '\"';
	if (strlen(value->ExchangeNo) > 0)
		ostr << ",\"交易所(ExchangeNo)\":\"" << value->ExchangeNo << '\"';
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"跨品种套利需要(CommodityNo)\":\"" << value->CommodityNo << '\"';
	if (strlen(value->ContractNo) > 0)
		ostr << ",\"跨期套利需要(ContractNo)\":\"" << value->ContractNo << '\"';
	if (value->MatchStreamId != 0)
		ostr << ",\"成交流号(MatchStreamId)\":\"" << value->MatchStreamId << '\"';
	ostr.put('}');
	return ostr.str();
}
///委托数据应答命令 记录到日志
void log(const TMatchQryReqField* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///资金变化通知结构 生成日志文本
string to_log_text(const TMoneyChgItem* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (value->MoneyValue != 0)
		ostr << ",\"钱的价值(MoneyValue)\":\"" << value->MoneyValue << '\"';
	if (value->MoneyChg != 0)
		ostr << ",\"金钱改变(MoneyChg)\":\"" << value->MoneyChg << '\"';
	ostr.put('}');
	return ostr.str();
}
///资金变化通知结构 记录到日志
void log(const TMoneyChgItem* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}



#ifdef EsQuote

///Application信息 生成日志文本
string to_log_text(const TapAPIApplicationInfo* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->AuthCode) > 0)
		ostr << ",\"授权码(AuthCode)\":\"" << value->AuthCode << '\"';
	if (strlen(value->KeyOperationLogPath) > 0)
		ostr << ",\"关键操作日志路径(KeyOperationLogPath)\":\"" << value->KeyOperationLogPath << '\"';
	ostr.put('}');
	return ostr.str();
}
///Application信息 记录到日志
void log(const TapAPIApplicationInfo* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///修改密码请求 生成日志文本
string to_log_text(const TapAPIChangePasswordReq* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->OldPassword) > 0)
		ostr << ",\"旧密码(OldPassword)\":\"" << value->OldPassword << '\"';
	if (strlen(value->NewPassword) > 0)
		ostr << ",\"新密码(NewPassword)\":\"" << value->NewPassword << '\"';
	ostr.put('}');
	return ostr.str();
}
///修改密码请求 记录到日志
void log(const TapAPIChangePasswordReq* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///品种编码结构 生成日志文本
string to_log_text(const TapAPICommodity* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ExchangeNo) > 0)
		ostr << ",\"交易所编码(ExchangeNo)\":\"" << value->ExchangeNo << '\"';
	ostr << ",\"品种类型(CommodityType)\":" << value->CommodityType;
	if (strlen(value->CommodityNo) > 0)
		ostr << ",\"品种编号(CommodityNo)\":\"" << value->CommodityNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///品种编码结构 记录到日志
void log(const TapAPICommodity* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///合约编码结构 生成日志文本
string to_log_text(const TapAPIContract* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"品种(Commodity)\":" << to_log_text(&value->Commodity);
	if (strlen(value->ContractNo1) > 0)
		ostr << ",\"合约代码1(ContractNo1)\":\"" << value->ContractNo1 << '\"';
	if (strlen(value->StrikePrice1) > 0)
		ostr << ",\"执行价1(StrikePrice1)\":\"" << value->StrikePrice1 << '\"';
	ostr << ",\"看涨看跌标示1(CallOrPutFlag1)\":" << value->CallOrPutFlag1;
	if (strlen(value->ContractNo2) > 0)
		ostr << ",\"合约代码2(ContractNo2)\":\"" << value->ContractNo2 << '\"';
	if (strlen(value->StrikePrice2) > 0)
		ostr << ",\"执行价2(StrikePrice2)\":\"" << value->StrikePrice2 << '\"';
	ostr << ",\"看涨看跌标示2(CallOrPutFlag2)\":" << value->CallOrPutFlag2;
	ostr.put('}');
	return ostr.str();
}
///合约编码结构 记录到日志
void log(const TapAPIContract* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///交易所信息 生成日志文本
string to_log_text(const TapAPIExchangeInfo* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->ExchangeNo) > 0)
		ostr << ",\"交易所编码(ExchangeNo)\":\"" << value->ExchangeNo << '\"';
	if (strlen(value->ExchangeName) > 0)
		ostr << ",\"交易所名称(ExchangeName)\":\"" << value->ExchangeName << '\"';
	ostr.put('}');
	return ostr.str();
}
///交易所信息 记录到日志
void log(const TapAPIExchangeInfo* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///品种信息 生成日志文本
string to_log_text(const TapAPIQuoteCommodityInfo* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"品种(Commodity)\":" << to_log_text(&value->Commodity);
	if (strlen(value->CommodityName) > 0)
		ostr << ",\"品种名称(CommodityName)\":\"" << value->CommodityName << '\"';
	if (strlen(value->CommodityEngName) > 0)
		ostr << ",\"品种英文名称(CommodityEngName)\":\"" << value->CommodityEngName << '\"';
	if (value->ContractSize != 0)
		ostr << ",\"每手乘数(ContractSize)\":\"" << value->ContractSize << '\"';
	if (value->CommodityTickSize != 0)
		ostr << ",\"最小变动价位(CommodityTickSize)\":\"" << value->CommodityTickSize << '\"';
	if (value->CommodityDenominator != 0)
		ostr << ",\"报价分母(CommodityDenominator)\":\"" << value->CommodityDenominator << '\"';
	ostr << ",\"组合方向(CmbDirect)\":" << value->CmbDirect;
	if (value->CommodityContractLen != 0)
		ostr << ",\"品种合约年限(CommodityContractLen)\":\"" << value->CommodityContractLen << '\"';
	ostr << ",\"是否夏令时(IsDST)\":" << value->IsDST;
	ostr << ",\"关联品种1(RelateCommodity1)\":" << to_log_text(&value->RelateCommodity1);
	ostr << ",\"关联品种2(RelateCommodity2)\":" << to_log_text(&value->RelateCommodity2);
	ostr.put('}');
	return ostr.str();
}
///品种信息 记录到日志
void log(const TapAPIQuoteCommodityInfo* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///行情合约信息 生成日志文本
string to_log_text(const TapAPIQuoteContractInfo* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"合约(Contract)\":" << to_log_text(&value->Contract);
	ostr << ",\"合约类型(ContractType)\":" << value->ContractType;
	if (strlen(value->QuoteUnderlyingContract) > 0)
		ostr << ",\"行情真实合约(QuoteUnderlyingContract)\":\"" << value->QuoteUnderlyingContract << '\"';
	if (strlen(value->ContractName) > 0)
		ostr << ",\"合约名称(ContractName)\":\"" << value->ContractName << '\"';
	if (strlen(value->ContractExpDate) > 0)
		ostr << ",\"合约到期日(ContractExpDate)\":\"" << value->ContractExpDate << '\"';
	if (strlen(value->LastTradeDate) > 0)
		ostr << ",\"最后交易日(LastTradeDate)\":\"" << value->LastTradeDate << '\"';
	if (strlen(value->FirstNoticeDate) > 0)
		ostr << ",\"首次通知日(FirstNoticeDate)\":\"" << value->FirstNoticeDate << '\"';
	ostr.put('}');
	return ostr.str();
}
///行情合约信息 记录到日志
void log(const TapAPIQuoteContractInfo* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///登录认证信息 生成日志文本
string to_log_text(const TapAPIQuoteLoginAuth* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->UserNo) > 0)
		ostr << ",\"用户名(UserNo)\":\"" << value->UserNo << '\"';
	ostr << ",\"是否修改密码(ISModifyPassword)\":" << value->ISModifyPassword;
	if (strlen(value->Password) > 0)
		ostr << ",\"用户密码(Password)\":\"" << value->Password << '\"';
	if (strlen(value->NewPassword) > 0)
		ostr << ",\"新密码(NewPassword)\":\"" << value->NewPassword << '\"';
	if (strlen(value->QuoteTempPassword) > 0)
		ostr << ",\"行情临时密码(QuoteTempPassword)\":\"" << value->QuoteTempPassword << '\"';
	ostr << ",\"是否需呀动态认证(ISDDA)\":" << value->ISDDA;
	if (strlen(value->DDASerialNo) > 0)
		ostr << ",\"动态认证码(DDASerialNo)\":\"" << value->DDASerialNo << '\"';
	ostr.put('}');
	return ostr.str();
}
///登录认证信息 记录到日志
void log(const TapAPIQuoteLoginAuth* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///行情全文 生成日志文本
string to_log_text(const TapAPIQuoteWhole* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	ostr << ",\"合约(Contract)\":" << to_log_text(&value->Contract);
	if (strlen(value->CurrencyNo) > 0)
		ostr << ",\"币种编号(CurrencyNo)\":\"" << value->CurrencyNo << '\"';
	ostr << ",\"交易状态(TradingState)\":" << value->TradingState;
	if (strlen(value->DateTimeStamp) > 0)
		ostr << ",\"时间戳(DateTimeStamp)\":\"" << value->DateTimeStamp << '\"';
	if (value->QPreClosingPrice != 0)
		ostr << ",\"昨收盘价(QPreClosingPrice)\":\"" << value->QPreClosingPrice << '\"';
	if (value->QPreSettlePrice != 0)
		ostr << ",\"昨结算价(QPreSettlePrice)\":\"" << value->QPreSettlePrice << '\"';
	if (value->QPrePositionQty != 0)
		ostr << ",\"昨持仓量(QPrePositionQty)\":\"" << value->QPrePositionQty << '\"';
	if (value->QOpeningPrice != 0)
		ostr << ",\"开盘价(QOpeningPrice)\":\"" << value->QOpeningPrice << '\"';
	if (value->QLastPrice != 0)
		ostr << ",\"最新价(QLastPrice)\":\"" << value->QLastPrice << '\"';
	if (value->QHighPrice != 0)
		ostr << ",\"最高价(QHighPrice)\":\"" << value->QHighPrice << '\"';
	if (value->QLowPrice != 0)
		ostr << ",\"最低价(QLowPrice)\":\"" << value->QLowPrice << '\"';
	if (value->QHisHighPrice != 0)
		ostr << ",\"历史最高价(QHisHighPrice)\":\"" << value->QHisHighPrice << '\"';
	if (value->QHisLowPrice != 0)
		ostr << ",\"历史最低价(QHisLowPrice)\":\"" << value->QHisLowPrice << '\"';
	if (value->QLimitUpPrice != 0)
		ostr << ",\"涨停价(QLimitUpPrice)\":\"" << value->QLimitUpPrice << '\"';
	if (value->QLimitDownPrice != 0)
		ostr << ",\"跌停价(QLimitDownPrice)\":\"" << value->QLimitDownPrice << '\"';
	if (value->QTotalQty != 0)
		ostr << ",\"当日总成交量(QTotalQty)\":\"" << value->QTotalQty << '\"';
	if (value->QTotalTurnover != 0)
		ostr << ",\"当日成交金额(QTotalTurnover)\":\"" << value->QTotalTurnover << '\"';
	if (value->QPositionQty != 0)
		ostr << ",\"持仓量(QPositionQty)\":\"" << value->QPositionQty << '\"';
	if (value->QAveragePrice != 0)
		ostr << ",\"均价(QAveragePrice)\":\"" << value->QAveragePrice << '\"';
	if (value->QClosingPrice != 0)
		ostr << ",\"收盘价(QClosingPrice)\":\"" << value->QClosingPrice << '\"';
	if (value->QSettlePrice != 0)
		ostr << ",\"结算价(QSettlePrice)\":\"" << value->QSettlePrice << '\"';
	if (value->QLastQty != 0)
		ostr << ",\"最新成交量(QLastQty)\":\"" << value->QLastQty << '\"';
	ostr << ",\"买价1-20档(QBidPrice)\":[";
	int idx;
	for (idx = 0;idx < 19;idx++)
	{
		ostr << value->QBidPrice[idx] << ',';
	}
	ostr << value->QBidPrice[idx] << ']';
	ostr << ",\"买量1-20档(QBidQty)\":[";
	for (idx = 0;idx < 19;idx++)
	{
		ostr << value->QBidQty[idx] << ',';
	}
	ostr << value->QBidQty[idx] << ']';
	ostr << ",\"卖价1-20档(QAskPrice)\":[";
	for (idx = 0;idx < 19;idx++)
	{
		ostr << value->QAskPrice[idx] << ',';
	}
	ostr << value->QAskPrice[idx] << ']';
	ostr << ",\"卖量1-20档(QAskQty)\":[";
	for (idx = 0;idx < 19;idx++)
	{
		ostr << value->QAskQty[idx] << ',';
	}
	ostr << value->QAskQty[idx] << ']';
	if (value->QImpliedBidPrice != 0)
		ostr << ",\"隐含买价(QImpliedBidPrice)\":\"" << value->QImpliedBidPrice << '\"';
	if (value->QImpliedBidQty != 0)
		ostr << ",\"隐含买量(QImpliedBidQty)\":\"" << value->QImpliedBidQty << '\"';
	if (value->QImpliedAskPrice != 0)
		ostr << ",\"隐含卖价(QImpliedAskPrice)\":\"" << value->QImpliedAskPrice << '\"';
	if (value->QImpliedAskQty != 0)
		ostr << ",\"隐含卖量(QImpliedAskQty)\":\"" << value->QImpliedAskQty << '\"';
	if (value->QPreDelta != 0)
		ostr << ",\"昨虚实度(QPreDelta)\":\"" << value->QPreDelta << '\"';
	if (value->QCurrDelta != 0)
		ostr << ",\"今虚实度(QCurrDelta)\":\"" << value->QCurrDelta << '\"';
	if (value->QInsideQty != 0)
		ostr << ",\"内盘量(QInsideQty)\":\"" << value->QInsideQty << '\"';
	if (value->QOutsideQty != 0)
		ostr << ",\"外盘量(QOutsideQty)\":\"" << value->QOutsideQty << '\"';
	if (value->QTurnoverRate != 0)
		ostr << ",\"换手率(QTurnoverRate)\":\"" << value->QTurnoverRate << '\"';
	if (value->Q5DAvgQty != 0)
		ostr << ",\"五日均量(Q5DAvgQty)\":\"" << value->Q5DAvgQty << '\"';
	if (value->QPERatio != 0)
		ostr << ",\"市盈率(QPERatio)\":\"" << value->QPERatio << '\"';
	if (value->QTotalValue != 0)
		ostr << ",\"总市值(QTotalValue)\":\"" << value->QTotalValue << '\"';
	if (value->QNegotiableValue != 0)
		ostr << ",\"流通市值(QNegotiableValue)\":\"" << value->QNegotiableValue << '\"';
	if (value->QPositionTrend != 0)
		ostr << ",\"持仓走势(QPositionTrend)\":\"" << value->QPositionTrend << '\"';
	if (value->QChangeSpeed != 0)
		ostr << ",\"涨速(QChangeSpeed)\":\"" << value->QChangeSpeed << '\"';
	if (value->QChangeRate != 0)
		ostr << ",\"涨幅(QChangeRate)\":\"" << value->QChangeRate << '\"';
	if (value->QChangeValue != 0)
		ostr << ",\"涨跌值(QChangeValue)\":\"" << value->QChangeValue << '\"';
	if (value->QSwing != 0)
		ostr << ",\"振幅(QSwing)\":\"" << value->QSwing << '\"';
	if (value->QTotalBidQty != 0)
		ostr << ",\"委买总量(QTotalBidQty)\":\"" << value->QTotalBidQty << '\"';
	if (value->QTotalAskQty != 0)
		ostr << ",\"委卖总量(QTotalAskQty)\":\"" << value->QTotalAskQty << '\"';
	ostr.put('}');
	return ostr.str();
}
///行情全文 记录到日志
void log(const TapAPIQuoteWhole* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}

///登录反馈信息 生成日志文本
string to_log_text(const TapAPIQuotLoginRspInfo* value)
{
	if (value == nullptr)
		return "";

	ostringstream ostr;
	ostr << "{\"__i__\":0";
	if (strlen(value->UserNo) > 0)
		ostr << ",\"用户名(UserNo)\":\"" << value->UserNo << '\"';
	if (value->UserType != 0)
		ostr << ",\"用户类型(UserType)\":\"" << value->UserType << '\"';
	if (strlen(value->UserName) > 0)
		ostr << ",\"昵称(UserName)\":\"" << value->UserName << '\"';
	if (strlen(value->QuoteTempPassword) > 0)
		ostr << ",\"行情临时密码(QuoteTempPassword)\":\"" << value->QuoteTempPassword << '\"';
	if (strlen(value->ReservedInfo) > 0)
		ostr << ",\"用户自己设置的预留信息(ReservedInfo)\":\"" << value->ReservedInfo << '\"';
	if (strlen(value->LastLoginIP) > 0)
		ostr << ",\"上次登录的地址(LastLoginIP)\":\"" << value->LastLoginIP << '\"';
	if (value->LastLoginProt != 0)
		ostr << ",\"上次登录使用的端口(LastLoginProt)\":\"" << value->LastLoginProt << '\"';
	if (strlen(value->LastLoginTime) > 0)
		ostr << ",\"上次登录的时间(LastLoginTime)\":\"" << value->LastLoginTime << '\"';
	if (strlen(value->LastLogoutTime) > 0)
		ostr << ",\"上次退出的时间(LastLogoutTime)\":\"" << value->LastLogoutTime << '\"';
	if (strlen(value->TradeDate) > 0)
		ostr << ",\"当前交易日期(TradeDate)\":\"" << value->TradeDate << '\"';
	if (strlen(value->LastSettleTime) > 0)
		ostr << ",\"上次结算时间(LastSettleTime)\":\"" << value->LastSettleTime << '\"';
	if (strlen(value->StartTime) > 0)
		ostr << ",\"系统启动时间(StartTime)\":\"" << value->StartTime << '\"';
	if (strlen(value->InitTime) > 0)
		ostr << ",\"系统初始化时间(InitTime)\":\"" << value->InitTime << '\"';
	ostr.put('}');
	return ostr.str();
}
///登录反馈信息 记录到日志
void log(const TapAPIQuotLoginRspInfo* value, int type, int level)
{
	log_debug(type, level, to_log_text(value).c_str());
}
#endif
#endif