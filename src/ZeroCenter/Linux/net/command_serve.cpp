#include "stdafx.h"
#ifdef SERVER
#include "SysCmdArg/LoginArg.h"
using namespace Agebull::Tson;
using namespace Agebull::Futures::Globals::DataModel;
using namespace Agebull::Futures::Globals;

//S端的内存回收队列
queue<void*> money_queue;

//ZMQ需要的内存销毁
void free_cmd_money(void* data_ptr, void* base_ptr)
{
	//while ((money_queue.size() % 50) == 0)
	//{
	//	while (money_queue.size() > 25)
	//	{
	//		delete[] reinterpret_cast<char*>(money_queue.front());
	//		money_queue.pop();
	//	}
	//}
	//money_queue.push(base_ptr);
}
#ifndef PROXY
/**
* @brief 登录完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnUserLogin(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = state;
	server_message_send(result);
}

/**
* @brief 登录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void UserLogin(const PNetCommand cmd)
{
	const boost::format fmt_def = boost::format("cus:%s:pwd");
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		deserialize_cmd_arg(cmd,LoginArg, cmd_arg);//类型名称为cmd_arg
		boost::format fmt(fmt_def);
		fmt % cmd_arg.UserName;
		auto pwd = read_str_from_redis(fmt.str().c_str());
		if (pwd.empty() || !pwd.equal(cmd_arg.PassWord))
		{
			state = NET_COMMAND_STATE_ARGUMENT_INVALID;//用户名密码不对
		}
		else
		{
			acl::string id = read_str_from_redis("r:lc:cus:name:%s", cmd_arg.UserName);
			write_to_redis(id.c_str(),id.length(),"r:lc:cus:token:%s", cmd->user_token);
		}
	}
	catch (...)
	{
		state = NET_COMMAND_STATE_SERVER_UNKNOW;
	}

	OnUserLogin(cmd, state);
}

/**
* @brief 登出完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnUserLogout(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 登出
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void UserLogout(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnUserLogout(cmd, state);
}

/**
* @brief 修改密码完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnUserSetPassword(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 修改密码
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void UserSetPassword(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnUserSetPassword(cmd, state);
}

#endif

/**
* @brief 查询交易所完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnSysQueryExchange(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 查询交易所
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void SysQueryExchange(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnSysQueryExchange(cmd, state);
}

/**
* @brief 查询交易商品完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnSysQueryCommodity(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 查询交易商品
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void SysQueryCommodity(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnSysQueryCommodity(cmd, state);
}

/**
* @brief 查询合约完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnSysQueryContract(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 查询合约
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void SysQueryContract(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnSysQueryContract(cmd, state);
}

/**
* @brief 查询货币币种信息完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnSysQueryCurrency(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 查询货币币种信息
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void SysQueryCurrency(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnSysQueryCurrency(cmd, state);
}

/**
* @brief 查询客户资金记录完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnQueryMoney(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 查询客户资金记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryMoney(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnQueryMoney(cmd, state);
}

/**
* @brief 查询持仓记录完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnQueryHold(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 查询持仓记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryHold(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnQueryHold(cmd, state);
}

/**
* @brief 查询客户当日委托记录完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnQueryTodayOrder(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 查询客户当日委托记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryTodayOrder(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnQueryTodayOrder(cmd, state);
}

/**
* @brief 查询客户历史委托记录完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnQueryHistoryOrder(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 查询客户历史委托记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryHistoryOrder(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnQueryHistoryOrder(cmd, state);
}

/**
* @brief 查询客户当日成交记录完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnQueryTodayMatch(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 查询客户当日成交记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryTodayMatch(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnQueryTodayMatch(cmd, state);
}

/**
* @brief 查询客户历史成交记录完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnQueryHistoryMatch(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 查询客户历史成交记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryHistoryMatch(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnQueryHistoryMatch(cmd, state);
}

/**
* @brief 查询客户当日出入金记录完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnQueryTodayCash(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 查询客户当日出入金记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryTodayCash(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnQueryTodayCash(cmd, state);
}

/**
* @brief 查询客户历史出入金记录完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnQueryHistoryCash(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 查询客户历史出入金记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryHistoryCash(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnQueryHistoryCash(cmd, state);
}

/**
* @brief 查询客户资金当日调整记录完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnQueryTodayCashAdjust(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 查询客户资金当日调整记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryTodayCashAdjust(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnQueryTodayCashAdjust(cmd, state);
}

/**
* @brief 查询客户资金历史调整记录完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnQueryHistoryCashAdjust(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 查询客户资金历史调整记录
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void QueryHistoryCashAdjust(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnQueryHistoryCashAdjust(cmd, state);
}

/**
* @brief 报单请求完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnOrderInsert(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 报单请求
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void OrderInsert(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnOrderInsert(cmd, state);
}

/**
* @brief 改单请求完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnOrderModify(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 改单请求
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void OrderModify(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnOrderModify(cmd, state);
}

/**
* @brief 撤单请求完成的消息回发
* @param {PNetCommand} cmd 命令对象
* @param {COMMAND_STATE} state 执行状态
* @return 无
*/
void OnOrderCancel(const PNetCommand cmd, COMMAND_STATE state)
{
	PNetCommand result = new NetCommand();
	//memset(&result, 0, sizeof(NetCommandResult));
	memcpy(result, cmd, sizeof(NetCommand));
	result->cmd_state = 0 - state;
	server_message_send(result);
}

/**
* @brief 撤单请求
* @param {PNetCommand} cmd 命令对象
* @return 无
*/
void OrderCancel(const PNetCommand cmd)
{
	COMMAND_STATE state = NET_COMMAND_STATE_SUCCEED;
	try
	{
		//deserialize_cmd_arg(cmd,DATA_TYPE,cmd_arg);//类型名称为cmd_arg
		//TO DO:处理方法
	}
	catch (...)
	{
		state = 65536;
	}
	OnOrderCancel(cmd, state);
}
#endif