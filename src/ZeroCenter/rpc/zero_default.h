#pragma once
#ifndef ZERO_DEFAULT_H_
#define ZERO_DEFAULT_H_

namespace agebull
{
	namespace zero_net
	{
		typedef void* zmq_handler;

		typedef uchar* tson_buffer;

		/*!
		* 站点类型
		*/
		typedef int station_type;
		const station_type station_type_dispatcher = 1;//系统调度
		const station_type station_type_notify = 2;//发布订阅
		const station_type station_type_api = 3;//普通API
		const station_type station_type_vote = 4;//投票即并发机制
		const station_type station_type_route_api = 5;//2018.08.03:新增,定向路由API
		const station_type station_type_queue = 6;//2018.11.10:新增,队列任务(发完请求后进队列处理)
		const station_type station_type_proxy = 0xFE;//反向代理
		const station_type station_type_plan = 0xFF;//计划任务
#define IS_PUB_STATION(type) type == station_type_dispatcher ||type == station_type_notify ||type == station_type_plan ||type == station_type_queue
#define IS_SYS_STATION(type) (type == station_type_dispatcher ||type == station_type_plan ||type == station_type_proxy)
#define IS_GENERAL_STATION(type) (type == station_type_api || type == station_type_notify || type == station_type_queue || type == station_type_vote || type == station_type_route_api)

		const int net_state_none = 0;
		const int net_state_runing = 1;
		const int net_state_closing = 2;
		const int net_state_closed = 3;
		const int net_state_distory = 4;
		const int net_state_failed = 5;

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
#define ZERO_STATUS_DENY_ACCESS  "-deny"
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
#define ZERO_FRAME_GLOBAL_ID  '\001'
		//站点
#define ZERO_FRAME_STATION_ID  '\002'
		//状态
#define ZERO_FRAME_STATUS  '\003'
		//请求ID
#define ZERO_FRAME_REQUEST_ID  '\004'
		//执行计划
#define ZERO_FRAME_PLAN  '\005'
		//执行计划
#define ZERO_FRAME_PLAN_TIME  '\006'
		//服务认证标识
#define ZERO_FRAME_SERVICE_KEY  '\007'
		//本地标识
#define ZERO_FRAME_LOCAL_ID  '\010'
		//原样参数
#define ZERO_FRAME_ORIGINAL_1  '\011'
		//原样参数
#define ZERO_FRAME_ORIGINAL_2  '\012'
		//原样参数
#define ZERO_FRAME_ORIGINAL_3  '\013'
		//原样参数
#define ZERO_FRAME_ORIGINAL_4  '\014'
		//原样参数
#define ZERO_FRAME_ORIGINAL_5  '\015'
		//原样参数
#define ZERO_FRAME_ORIGINAL_6  '\016'
		//原样参数
#define ZERO_FRAME_ORIGINAL_7  '\017'
		//原样参数
#define ZERO_FRAME_ORIGINAL_8  '\020'
		//命令
#define ZERO_FRAME_COMMAND  '$'
		//参数
#define ZERO_FRAME_ARG  '%'
		//通知主题
#define ZERO_FRAME_PUB_TITLE  '*'
		//通知副题
#define ZERO_FRAME_SUBTITLE  ZERO_FRAME_COMMAND
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
#define ZERO_STATUS_DENY_ERROR_ID uchar(0xF9)

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
		  * \brief 取得未处理数据
		  */
#define ZERO_BYTE_COMMAND_RESTART '\4'

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
          acl::string desc_str(bool in, char* desc, size_t len);
	}
}
#endif