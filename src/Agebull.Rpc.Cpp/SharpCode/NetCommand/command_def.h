#ifndef _COMMAND_DEF_H
#define _COMMAND_DEF_H

#pragma once



//网络状态
typedef int NET_STATE;
const NET_STATE NET_STATE_NONE = 0;
const NET_STATE NET_STATE_RUNING = 1;
const NET_STATE NET_STATE_CLOSING = 2;
const NET_STATE NET_STATE_CLOSED = 3;
const NET_STATE NET_STATE_DISTORY = 4;

typedef void* ZMQ_HANDLE;

typedef char* TSON_BUFFER;

//网络命令
typedef unsigned short NET_COMMAND;

//系统通知
const NET_COMMAND NET_COMMAND_SYSTEM_NOTIFY = NET_COMMAND(0x1);
//业务通知
const NET_COMMAND NET_COMMAND_BUSINESS_NOTIFY = NET_COMMAND(0x2);
//命令请求
const NET_COMMAND NET_COMMAND_CALL = NET_COMMAND(0x3);
//命令返回
const NET_COMMAND NET_COMMAND_RESULT = NET_COMMAND(0x4);
//流程处理起始
const NET_COMMAND NET_COMMAND_FLOW_START = NET_COMMAND(0x10);
//流程节点处理成功
const NET_COMMAND NET_COMMAND_FLOW_STEP_SUCCEESS = NET_COMMAND(0x11);
//流程节点处理成功
const NET_COMMAND NET_COMMAND_FLOW_STEP_FAILED = NET_COMMAND(0x12);
//流程节点处理状态读取
const NET_COMMAND NET_COMMAND_FLOW_STTATE = NET_COMMAND(0x13);
//流程处理结束
const NET_COMMAND NET_COMMAND_FLOW_END = NET_COMMAND(0x17);

//网络命令状态 -0x10之后为自定义错误
typedef int COMMAND_STATE;
//数据
const COMMAND_STATE NET_COMMAND_STATE_DATA = 0x0;
//命令发送中
const COMMAND_STATE NET_COMMAND_STATE_SENDING = 0x7;
//命令已发送
const COMMAND_STATE NET_COMMAND_STATE_SENDED = 0x8;
//命令已在服务端排队
const COMMAND_STATE NET_COMMAND_STATE_WAITING = 0x9;
//命令已执行完成
const COMMAND_STATE NET_COMMAND_STATE_SUCCEED = 0xA;
//命令发送出错
const COMMAND_STATE NET_COMMAND_STATE_NETERROR = 0x6;
//未知错误(未收到回执)
const COMMAND_STATE NET_COMMAND_STATE_UNKNOW = 0x5;
//服务器未知错误(系统异常)
const COMMAND_STATE NET_COMMAND_STATE_SERVER_UNKNOW = 0x4;
//本地未知错误(系统异常)
const COMMAND_STATE NET_COMMAND_STATE_CLIENT_UNKNOW = 0x3;
//数据重复处理
const COMMAND_STATE NET_COMMAND_STATE_DATA_REPEAT = 0x2;
//命令不允许执行
const COMMAND_STATE NET_COMMAND_STATE_CANNOT = 0x1;

//参数错误
const COMMAND_STATE NET_COMMAND_STATE_ARGUMENT_INVALID = -1;
//逻辑错误
const COMMAND_STATE NET_COMMAND_STATE_LOGICAL_ERROR = -2;


#define GUID_LEN 34 

//命令的网络调用参数结构
typedef struct {
	char user_token[GUID_LEN];//用户标识(用户标识用户，同时也是0MQ的发布的筛选器)
	char cmd_identity[GUID_LEN];//命令标识(用于异步回发后的命令状态对应)
	NET_COMMAND cmd_id;//命令(调用方设置)
	unsigned short try_num;//命令(重发次数)
	COMMAND_STATE cmd_state;//命令状态(0为数据)
	size_t crc_code;//命令头的CRC16校验码
	size_t data_len;//参数长度--此处为TSON的头部
}NetCommand, *PNetCommand;


//命令的网络头长度(不包含CRC字段)
#define NETCOMMAND_BODY_LEN sizeof(NetCommand) - sizeof(size_t) - sizeof(size_t)
//命令的网络头长度
#define NETCOMMAND_HEAD_LEN sizeof(NetCommand) - sizeof(size_t)
//取得命令对象的数据地址
#define get_cmd_buffer(cmd) (TSON_BUFFER)(((char*)cmd) + sizeof(NetCommand) - sizeof(size_t))
//取得命令对象的真实内存长度
#define get_cmd_len(cmd_call) cmd_call->data_len == 0 ? sizeof(NetCommand) : cmd_call->data_len + sizeof(NetCommand) - sizeof(size_t)
//取得命令对象的数据类型
#define get_cmd_data_type(cmd_call) (*((int*)(((char*)cmd_call) + sizeof(NetCommand))))

//表示一个命令参数指针,复制构造后会指针会丢失
class NetCommandArgPtr
{
	int* m_control;
public:
	char* m_buffer;
	size_t m_len;
	PNetCommand m_command;
	NetCommandArgPtr()
		: m_control(nullptr)
		, m_buffer(nullptr)
		, m_len(0)
		, m_command(nullptr)
	{
	}
	NetCommandArgPtr(NetCommandArgPtr& ptr)
		: m_control(ptr.m_control)
		, m_buffer(ptr.m_buffer)
		, m_len(ptr.m_len)
		, m_command(ptr.m_command)
	{
		*m_control += 1;
	}
	NetCommandArgPtr(size_t len)
		: m_control(new int)
		, m_buffer(new char[len])
		, m_len(len)
	{
		m_command = reinterpret_cast<PNetCommand>(m_buffer);
		*m_control = 1;
	}
	NetCommandArgPtr(PNetCommand cmd)
		: m_control(new int)
		, m_buffer(reinterpret_cast<char*>(cmd))
		, m_len(get_cmd_len(cmd))
		, m_command(cmd)
	{
		*m_control = 1;
	}
	NetCommandArgPtr(char* buffer, size_t len)
		: m_control(new int)
		, m_buffer(buffer)
		, m_len(len)
		, m_command(reinterpret_cast<PNetCommand>(buffer))
	{
		*m_control = 1;
	}
	~NetCommandArgPtr()
	{
		if (m_control == nullptr || (*m_control) < 0)//小于0表示此智能指针已无用
			return;
		(*m_control) -= 1;
		if (*m_control == 0)
		{
			(*m_control) = -1;
			delete m_control;
			delete[] m_buffer;
		}
	}
	NetCommandArgPtr& operator=(NetCommandArgPtr& right)
	{
		m_buffer = right.m_buffer;
		m_command = right.m_command;
		m_len = right.m_len;
		m_control = right.m_control;
		if (m_control != nullptr)
			*m_control += 1;
		return *this;
	}
	//NetCommandArgPtr& operator=(PNetCommand arg)
	//{
	//	m_buffer = reinterpret_cast<char*>(arg);
	//	m_command = arg;
	//	m_len = get_cmd_len(arg);
	//	*m_control = 1;
	//	return *this;
	//}
	PNetCommand& operator->()
	{
		return m_command;
	}
	PNetCommand& operator*()
	{
		return m_command;
	}
};

#endif // !_COMMAND_DEF_H