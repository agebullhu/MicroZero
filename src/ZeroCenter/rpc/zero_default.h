#pragma once
#ifndef ZERO_DEFAULT_H_
#define  ZERO_DEFAULT_H_

namespace agebull
{
	namespace zmq_net
	{
		typedef void* ZMQ_HANDLE;

		typedef uchar* TSON_BUFFER;

		/*!
		* 站点类型
		*/
		typedef int STATION_TYPE;
		const STATION_TYPE STATION_TYPE_DISPATCHER = 1;//系统调度
		const STATION_TYPE STATION_TYPE_PUBLISH = 2;//发布订阅
		const STATION_TYPE STATION_TYPE_API = 3;//普通API
		const STATION_TYPE STATION_TYPE_VOTE = 4;//投票即并发机制
		const STATION_TYPE STATION_TYPE_ROUTE_API = 5;//2018.08.03:新增,定向路由API
		const STATION_TYPE STATION_TYPE_SPECIAL = 0xA0;//专用站点分割标记,不使用
		const STATION_TYPE STATION_TYPE_PLAN = 0xFF;//计划任务

		/*!
		* 网络状态
		*/
		typedef int NET_STATE;
		const NET_STATE NET_STATE_NONE = 0;
		const NET_STATE NET_STATE_RUNING = 1;
		const NET_STATE NET_STATE_CLOSING = 2;
		const NET_STATE NET_STATE_CLOSED = 3;
		const NET_STATE NET_STATE_DISTORY = 4;
		const NET_STATE NET_STATE_FAILED = 5;

		/*!
		*以下为状态文字
		*/
		/*!
		* 正常状态
		*/

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

		/*!
		* 错误状态
		*/
#define ZERO_STATUS_BAD  '-'
#define ZERO_STATUS_FAILED  "-failed"
#define ZERO_STATUS_ERROR  "-error"
#define ZERO_STATUS_NOT_SUPPORT  "-not support"
#define ZERO_STATUS_NOT_FIND  "-not find"
#define ZERO_STATUS_NOT_WORKER  "-not work"
#define ZERO_STATUS_FRAME_INVALID  "-invalid frame"
#define ZERO_STATUS_ARG_INVALID  "-invalid argument"
#define ZERO_STATUS_TIMEOUT  "-time out"
#define ZERO_STATUS_NET_ERROR  "-net error"
//#define ZERO_STATUS_MANAGE_ARG_ERROR  "-ArgumentError! must like : call[name][command][argument]"
//#define ZERO_STATUS_MANAGE_INSTALL_ARG_ERROR  "-ArgumentError! must like :install [type] [name]"
#define ZERO_STATUS_PLAN_INVALID  "-plan invalid"
#define ZERO_STATUS_PLAN_ERROR  "-plan error"

/*!
		 *以下为帧类型说明符号
		 */
		 //终止符号
#define ZERO_FRAME_END  '\0'
		//全局标识
#define ZERO_FRAME_GLOBAL_ID  '\1'
		//站点
#define ZERO_FRAME_STATION_ID  '\2'
		//状态
#define ZERO_FRAME_STATUS  '\3'
		//请求ID
#define ZERO_FRAME_REQUEST_ID  '\4'
		//执行计划
#define ZERO_FRAME_PLAN  '\5'
		//本地标识
#define ZERO_FRAME_LOCAL_ID  '\6'
		//消息内容
#define ZERO_FRAME_MESSAGE  '\7'
		//命令
#define ZERO_FRAME_COMMAND  '$'
		//参数
#define ZERO_FRAME_ARG  '%'
		//广播主题
#define ZERO_FRAME_PUB_TITLE  '*'
		//广播副题
#define ZERO_FRAME_SUBTITLE  '@'
		//网络上下文信息
#define ZERO_FRAME_CONTEXT  '#'
		//请求者/生产者
#define ZERO_FRAME_REQUESTER  '>'
		//发布者/生产者
#define ZERO_FRAME_PUBLISHER  ZERO_FRAME_REQUESTER
		//回复者/浪费者
#define ZERO_FRAME_RESPONSER  '<'
		//订阅者/浪费者
#define ZERO_FRAME_SUBSCRIBER  ZERO_FRAME_RESPONSER
		//内容
#define ZERO_FRAME_CONTENT  'T'
#define ZERO_FRAME_CONTENT_TEXT  'T'
#define ZERO_FRAME_CONTENT_JSON  'J'
#define ZERO_FRAME_CONTENT_BIN  'B'
#define ZERO_FRAME_CONTENT_XML  'X'

		/*!
		 * 以下为返回时的快捷状态:说明帧的第二节字([1])
		 */

#define ZERO_STATUS_OK_ID uchar(0x1)
#define ZERO_STATUS_PLAN_ID uchar(0x2)
#define ZERO_STATUS_RUNING_ID uchar(0x3)
#define ZERO_STATUS_BYE_ID uchar(0x4)
#define ZERO_STATUS_WECOME_ID uchar(0x5)
#define ZERO_STATUS_WAIT_ID uchar(0x6)

#define ZERO_STATUS_VOTE_SENDED_ID uchar(0x70)
#define ZERO_STATUS_VOTE_BYE_ID uchar(0x71)
#define ZERO_STATUS_VOTE_WAITING_ID uchar(0x72)
#define ZERO_STATUS_VOTE_START_ID uchar(0x73)
#define ZERO_STATUS_VOTE_END_ID uchar(0x74)
#define ZERO_STATUS_VOTE_CLOSED_ID uchar(0x75)

#define ZERO_STATUS_FAILED_ID uchar(0x80)
#define ZERO_STATUS_PAUSE_ID uchar(0x81)

#define ZERO_STATUS_BUG_ID uchar(0xD0)
#define ZERO_STATUS_FRAME_INVALID_ID uchar(0xD1)
#define ZERO_STATUS_ARG_INVALID_ID uchar(0xD2)

#define ZERO_STATUS_ERROR_ID uchar(0xF0)
#define ZERO_STATUS_NOT_FIND_ID uchar(0xF1)
#define ZERO_STATUS_NOT_WORKER_ID uchar(0xF2)
#define ZERO_STATUS_NOT_SUPPORT_ID uchar(0xF3)
#define ZERO_STATUS_TIMEOUT_ID uchar(0xF4)
#define ZERO_STATUS_NET_ERROR_ID uchar(0xF5)
#define ZERO_STATUS_PLAN_ERROR_ID uchar(0xF6)
#define ZERO_STATUS_SEND_ERROR_ID uchar(0xF7)
#define ZERO_STATUS_RECV_ERROR_ID uchar(0xF8)

		 /*!
		  * 以下为请求时的快捷命令:说明帧的第二节字([1])
		  */

		  /**
		  * \brief 无特殊说明
		  */
#define  ZERO_BYTE_COMMAND_NONE  '\1'

		  /**
		  * \brief 进入计划
		  */
#define ZERO_BYTE_COMMAND_PLAN '\2'

		  /**
		  * \brief 代理执行
		  */
#define ZERO_BYTE_COMMAND_PROXY '\3'

		  /**
		  * \brief 取全局标识
		  */
#define  ZERO_BYTE_COMMAND_GLOBAL_ID  '>'

		  /**
		  * \brief 等待结果
		  */
#define ZERO_BYTE_COMMAND_WAITING '#'

		  /**
		  * \brief 查找结果
		  */
#define ZERO_BYTE_COMMAND_FIND_RESULT '%'

		  /**
		  * \brief 关闭结果
		  */
#define ZERO_BYTE_COMMAND_CLOSE_REQUEST '-'

		  /**
		  * \brief Ping
		  */
#define ZERO_BYTE_COMMAND_PING '*'

		  /**
		  * \brief 心跳加入
		  */
#define  ZERO_BYTE_COMMAND_HEART_JOIN  'J'

		  /**
		  * \brief 心跳已就绪
		  */
#define  ZERO_BYTE_COMMAND_HEART_READY  'R'

		  /**
		  * \brief 心跳进行
		  */
#define  ZERO_BYTE_COMMAND_HEART_PITPAT  'P'

		  /**
		  * \brief 心跳退出
		  */
#define  ZERO_BYTE_COMMAND_HEART_LEFT  'L'

		  /**
		  * \brief 说明帧解析
		  */
		inline acl::string desc_str(bool in, char* desc, size_t len)
		{
			if (desc == nullptr || len == 0)
				return "[EMPTY]";
			acl::string str;
			str.format_append("{\"size\":%d", desc[0]);
			uchar state = *reinterpret_cast<uchar*>(desc + 1);
			if (in)
			{
				str.append(R"(,"command":")");
				switch (state)
				{
				case ZERO_BYTE_COMMAND_NONE: //!\1 无特殊说明
					str.append("none");
					break;
				case ZERO_BYTE_COMMAND_PLAN: //!\2  取全局标识
					str.append("plan");
					break;
				case ZERO_BYTE_COMMAND_GLOBAL_ID: //!>  
					str.append("global_id");
					break;
				case ZERO_BYTE_COMMAND_WAITING: //!# 等待结果
					str.append("waiting");
					break;
				case ZERO_BYTE_COMMAND_FIND_RESULT: //!% 关闭结果
					str.append("find result");
					break;
				case ZERO_BYTE_COMMAND_CLOSE_REQUEST: //!- Ping
					str.append("close request");
					break;
				case ZERO_BYTE_COMMAND_PING: //!* 心跳加入
					str.append("ping");
					break;
				case ZERO_BYTE_COMMAND_HEART_JOIN: //!J  心跳已就绪
					str.append("heart join");
					break;
				case ZERO_BYTE_COMMAND_HEART_READY: //!R  心跳进行
					str.append("heart ready");
					break;
				case ZERO_BYTE_COMMAND_HEART_PITPAT: //!P  心跳退出
					str.append("heart pitpat");
					break;
				case ZERO_BYTE_COMMAND_HEART_LEFT: //!L  
					str.append("heart left");
					break;
				}
			}
			else
			{
				str.append(R"(,"state":")");
				switch (state)
				{
				case ZERO_STATUS_OK_ID: //!(0x1)
					str.append(ZERO_STATUS_OK);
					break;
				case ZERO_STATUS_PLAN_ID: //!(0x2)
					str.append(ZERO_STATUS_PLAN);
					break;
				case ZERO_STATUS_RUNING_ID: //!(0x3)
					str.append(ZERO_STATUS_RUNING);
					break;
				case ZERO_STATUS_BYE_ID: //!(0x4)
					str.append(ZERO_STATUS_BYE);
					break;
				case ZERO_STATUS_WECOME_ID: //!(0x5)
					str.append(ZERO_STATUS_WECOME);
					break;
				case ZERO_STATUS_VOTE_SENDED_ID: //!(0x20)
					str.append(ZERO_STATUS_VOTE_SENDED);
					break;
				case ZERO_STATUS_VOTE_BYE_ID: //!(0x21)
					str.append(ZERO_STATUS_VOTE_BYE);
					break;
				case ZERO_STATUS_WAIT_ID: //!(0x22)
					str.append(ZERO_STATUS_WAITING);
					break;
				case ZERO_STATUS_VOTE_WAITING_ID: //!(0x22)
					str.append(ZERO_STATUS_WAITING);
					break; 
				case ZERO_STATUS_VOTE_START_ID: //!(0x23)
					str.append(ZERO_STATUS_VOTE_START);
					break;
				case ZERO_STATUS_VOTE_END_ID: //!(0x24)
					str.append(ZERO_STATUS_VOTE_END);
					break;
				case ZERO_STATUS_VOTE_CLOSED_ID: //!(0x25)
					str.append(ZERO_STATUS_VOTE_CLOSED);
					break;
				case ZERO_STATUS_ERROR_ID: //!(0x81)
					str.append(ZERO_STATUS_ERROR);
					break;
				case ZERO_STATUS_FAILED_ID: //!(0x82)
					str.append(ZERO_STATUS_FAILED);
					break;
				case ZERO_STATUS_NOT_FIND_ID: //!(0x83)
					str.append(ZERO_STATUS_NOT_FIND);
					break;
				case ZERO_STATUS_NOT_SUPPORT_ID: //!(0x84)
					str.append(ZERO_STATUS_NOT_SUPPORT);
					break;
				case ZERO_STATUS_FRAME_INVALID_ID: //!(0x85)
					str.append(ZERO_STATUS_FRAME_INVALID);
					break;
				case ZERO_STATUS_ARG_INVALID_ID: //!(0x85)
					str.append(ZERO_STATUS_ARG_INVALID);
					break;
				case ZERO_STATUS_TIMEOUT_ID: //!(0x86)
					str.append(ZERO_STATUS_TIMEOUT);
					break;
				case ZERO_STATUS_NET_ERROR_ID: //!(0x87)
					str.append(ZERO_STATUS_NET_ERROR);
					break;
				case ZERO_STATUS_NOT_WORKER_ID: //!(0x88)
					str.append(ZERO_STATUS_NOT_WORKER);
					break;
				case ZERO_STATUS_PLAN_ERROR_ID: //!(0x8B)
					str.append(ZERO_STATUS_PLAN_ERROR);
					break; 
				}
			}
			str.append(R"(","frames":[)");

			str.append(R"("Caller","FrameDescr")");
			for (size_t idx = 2; idx < len; idx++)
			{
				switch (desc[idx])
				{
				case ZERO_FRAME_END:
					str.append(",\"End\"");
					break;
					//全局标识
				case ZERO_FRAME_GLOBAL_ID:
					str.append(",\"GLOBAL_ID\"");
					break;
					//站点
				case ZERO_FRAME_STATION_ID:
					str.append(",\"STATION_ID\"");
					break;
					//执行计划
				case ZERO_FRAME_PLAN:
					str.append(",\"PLAN\"");
					break;
					//参数
				case ZERO_FRAME_ARG:
					str.append(",\"ARG\"");
					break;
					//参数
				case ZERO_FRAME_COMMAND:
					str.append(",\"COMMAND\"");
					break;
					//请求ID
				case ZERO_FRAME_REQUEST_ID:
					str.append(",\"REQUEST_ID\"");
					break;
					//请求者/生产者
				case ZERO_FRAME_REQUESTER:
					str.append(",\"REQUESTER\"");
					break;
					//回复者/浪费者
				case ZERO_FRAME_RESPONSER:
					str.append(",\"RESPONSER\"");
					break;
					//广播主题
				case ZERO_FRAME_PUB_TITLE:
					str.append(",\"PUB_TITLE\"");
					break;
					//广播副题
				case ZERO_FRAME_SUBTITLE:
					str.append(",\"SUBTITLE\"");
					break;
				case ZERO_FRAME_STATUS:
					str.append(",\"STATUS\"");
					break;
					//网络上下文信息
				case ZERO_FRAME_CONTEXT:
					str.append(",\"CONTEXT\"");
					break;
				case ZERO_FRAME_CONTENT_TEXT:
					str.append(",\"CONTENT\"");
					break;
				case ZERO_FRAME_CONTENT_JSON:
					str.append(",\"JSON\"");
					break;
				case ZERO_FRAME_CONTENT_BIN:
					str.append(",\"BIN\"");
					break;
				case ZERO_FRAME_CONTENT_XML:
					str.append(",\"XML\"");
					break;
				default:
					str.append(",\"Arg\"");
					break;
				}
			}
			str.append("]}");
			return str;
		}

	}
}
#endif