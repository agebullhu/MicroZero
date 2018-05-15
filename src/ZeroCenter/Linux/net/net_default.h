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
		const STATION_TYPE STATION_TYPE_MONITOR = 2;
		const STATION_TYPE STATION_TYPE_API = 3;
		const STATION_TYPE STATION_TYPE_VOTE = 4;
		const STATION_TYPE STATION_TYPE_PUBLISH = 5;

		//网络状态
		typedef int NET_STATE;
		const NET_STATE NET_STATE_NONE = 0;
		const NET_STATE NET_STATE_RUNING = 1;
		const NET_STATE NET_STATE_CLOSING = 2;
		const NET_STATE NET_STATE_CLOSED = 3;
		const NET_STATE NET_STATE_DISTORY = 4;

		//正常状态
#define  ZERO_STATUS_SUCCESS '+'
		//错误状态
#define ZERO_STATUS_BAD  '-'
//正常状态
#define ZERO_STATUS_OK  "+ok"
#define ZERO_STATUS_PLAN  "+plan"
#define ZERO_STATUS_RUNING  "+runing"
#define ZERO_STATUS_BYE  "+bye"
#define ZERO_STATUS_WECOME  "+wecome"
#define ZERO_STATUS_VOTE_SENDED  "+send"
#define ZERO_STATUS_VOTE_CLOSED  "+close"
#define ZERO_STATUS_VOTE_BYE  "+bye"
#define ZERO_STATUS_VOTE_WAITING  "+waiting"
#define ZERO_STATUS_VOTE_START  "+start"
#define ZERO_STATUS_VOTE_END  "+end"

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


//终止符号
#define ZERO_FRAME_END  'E'
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
#define ZERO_FRAME_SUBSCRIBER  zero_responser
//广播主题
//#define zero_pub_title  '*'
//广播副题
#define ZERO_FRAME_SUBTITLE  'S'
//网络上下文信息
#define ZERO_FRAME_CONTEXT  'T'
//状态
#define ZERO_FRAME_STATUS  'S'




/**
* \brief 工作站点加入
*/
#define  ZERO_WORKER_JOIN  '@' 
/**
* \brief 工作站点加入
*/
#define  ZERO_WORKER_LEFT  '!' 
/**
* \brief 工作站点等待工作
*/
#define  ZERO_WORKER_LISTEN  '$' 

/**
* \brief 心跳加入
*/
#define  ZERO_HEART_JOIN  '@' 

/**
* \brief 心跳进行
*/
#define  ZERO_HEART_PITPAT  '$' 

/**
* \brief 心跳退出
*/
#define  ZERO_HEART_LEFT  '!' 

	}
}
#endif