#pragma once
#ifndef ZERO_DEFAULT_H_
#define  ZERO_DEFAULT_H_
namespace agebull
{
	namespace zmq_net
	{
		typedef void* ZMQ_HANDLE;

		typedef char* TSON_BUFFER;

		//站点类型
		typedef int STATION_TYPE;
		const STATION_TYPE STATION_TYPE_DISPATCHER = 1;
		const STATION_TYPE STATION_TYPE_PUBLISH = 2;
		const STATION_TYPE STATION_TYPE_API = 3;
		const STATION_TYPE STATION_TYPE_VOTE = 4;

		//网络状态
		typedef int NET_STATE;
		const NET_STATE NET_STATE_NONE = 0;
		const NET_STATE NET_STATE_RUNING = 1;
		const NET_STATE NET_STATE_CLOSING = 2;
		const NET_STATE NET_STATE_CLOSED = 3;
		const NET_STATE NET_STATE_DISTORY = 4;
		const NET_STATE NET_STATE_FAILED = 5;

		//ZMQ上下文对象
		ZMQ_HANDLE get_zmq_context();
		//运行状态
		NET_STATE get_net_state();
		//线程计数清零
		void reset_command_thread(int count);
		//登记线程失败
		void set_command_thread_bad(const char* name);
		//登记线程开始
		void set_command_thread_run(const char* name);
		//登记线程关闭
		void set_command_thread_end(const char* name);

		//正常状态
#define ZERO_STATUS_SUCCESS '+'

#define ZERO_STATUS_OK  "+ok"
#define ZERO_STATUS_PLAN  "+plan"
#define ZERO_STATUS_RUNING  "+runing"
#define ZERO_STATUS_BYE  "+bye"
#define ZERO_STATUS_WECOME  "+wecome"
#define ZERO_STATUS_WAITING  "+waiting"
#define ZERO_STATUS_VOTE_SENDED  "+send"
#define ZERO_STATUS_VOTE_CLOSED  "+close"
#define ZERO_STATUS_VOTE_BYE  "+bye"
#define ZERO_STATUS_VOTE_START  "+start"
#define ZERO_STATUS_VOTE_END  "+end"

#define ZERO_STATUS_OK_ID char(0)
#define ZERO_STATUS_PLAN_ID char(1)
#define ZERO_STATUS_RUNING_ID char(2)
#define ZERO_STATUS_BYE_ID char(3)
#define ZERO_STATUS_WECOME_ID char(4)
#define ZERO_STATUS_VOTE_SENDED_ID char(5)
#define ZERO_STATUS_VOTE_BYE_ID char(3)
#define ZERO_STATUS_WAITING_ID char(6)
#define ZERO_STATUS_VOTE_START_ID char(7)
#define ZERO_STATUS_VOTE_END_ID char(8)
#define ZERO_STATUS_VOTE_CLOSED_ID char(9)

//错误状态
#define ZERO_STATUS_BAD  '-'
#define ZERO_STATUS_NO_FIND  "-no find"
#define ZERO_STATUS_FRAME_INVALID  "-invalid" 
#define ZERO_STATUS_TIMEOUT  "-time out"
#define ZERO_STATUS_NET_ERROR  "-net error"
#define ZERO_STATUS_NO_SUPPORT  "-no support"
#define ZERO_STATUS_FAILED  "-failed"
#define ZERO_STATUS_ERROR  "-error"
#define ZERO_STATUS_API_NOT_WORKER  "-not work"
#define ZERO_STATUS_MANAGE_ARG_ERROR  "-ArgumentError! must like : call[name][command][argument]"
#define ZERO_STATUS_MANAGE_INSTALL_ARG_ERROR  "-ArgumentError! must like :install [type] [name]"

#define ZERO_STATUS_ERROR_ID char(0x81)
#define ZERO_STATUS_FAILED_ID char(0x82)
#define ZERO_STATUS_NO_FIND_ID char(0x83)
#define ZERO_STATUS_NO_SUPPORT_ID char(0x84)
#define ZERO_STATUS_FRAME_INVALID_ID char(0x85)
#define ZERO_STATUS_TIMEOUT_ID char(0x86)
#define ZERO_STATUS_NET_ERROR_ID char(0x87)
#define ZERO_STATUS_NOT_WORKER_ID char(0x88)
#define ZERO_STATUS_MANAGE_ARG_ERROR_ID char(0x89)
#define ZERO_STATUS_MANAGE_INSTALL_ARG_ERROR_ID char(0x8A)

//终止符号
#define ZERO_FRAME_END  '\0'
//全局标识
#define ZERO_FRAME_GLOBAL_ID  '\1'
//执行计划
#define ZERO_FRAME_PLAN  'P'
//参数
#define ZERO_FRAME_ARG  'A'
//参数
#define ZERO_FRAME_COMMAND  'C'
//请求ID
#define ZERO_FRAME_REQUEST_ID  'I'
//请求者/生产者
#define ZERO_FRAME_REQUESTER  'R'
//发布者/生产者
#define ZERO_FRAME_PUBLISHER  ZERO_FRAME_REQUESTER
//回复者/浪费者
#define ZERO_FRAME_RESPONSER  'G'
//订阅者/浪费者
#define ZERO_FRAME_SUBSCRIBER  ZERO_FRAME_RESPONSER
//广播主题
#define ZERO_FRAME_PUB_TITLE  '*'
//广播副题
#define ZERO_FRAME_SUBTITLE  'S'
//网络上下文信息
#define ZERO_FRAME_CONTEXT  'T'
//状态
#define ZERO_FRAME_STATUS  'S'
//内容
#define ZERO_FRAME_TEXT  'T'
#define ZERO_FRAME_JSON  'J'
#define ZERO_FRAME_BIN  'B'
#define ZERO_FRAME_XML  'X'

/**
* \brief 心跳退出
*/
#define  ZERO_COMMAND_GLOBAL_ID  '>' 

/**
* \brief 等待结果
*/
#define ZERO_COMMAND_WAITING '#'

/**
* \brief 查找结果
*/
#define ZERO_COMMAND_FIND_RESULT '%'

/**
* \brief 关闭结果
*/
#define ZERO_COMMAND_CLOSE_RESULT '-'

/**
* \brief Ping
*/
#define ZERO_COMMAND_PING '*'

/**
* \brief 心跳加入
*/
#define  ZERO_COMMAND_HEART_JOIN  '@' 

/**
* \brief 心跳进行
*/
#define  ZERO_COMMAND_HEART_PITPAT  '$' 

/**
* \brief 心跳退出
*/
#define  ZERO_COMMAND_HEART_LEFT  '!' 

//状态说明符

//1 请求
/**
* \brief 无特殊说明
*/
#define  ZERO_STATE_CODE_NONE  '\0' 

//1 请求
/**
* \brief 计划任务
*/
#define  ZERO_STATE_CODE_PLAN  char(1) 
//1 请求
/**
* \brief 工作站点退出
*/
#define  ZERO_STATE_CODE_WORKER_LEFT  char(2) 
/**
* \brief 工作站点等待工作
*/
#define  ZERO_STATE_CODE_WORKER_LISTEN  char(3) 

	}
}
#endif