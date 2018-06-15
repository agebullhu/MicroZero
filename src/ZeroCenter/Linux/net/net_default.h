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
		//等待关闭(仅限station_dispatcher结束时使用一次)
		void wait_close();



		/// <summary>
		/// 中心事件
		/// </summary>
		enum class zero_net_event
		{
			/// <summary>
			/// 
			/// </summary>
			event_none = 0x0,
			/// <summary>
			/// 
			/// </summary>
			event_system_start = 0x1,
			/// <summary>
			/// 
			/// </summary>
			event_system_closing,
			/// <summary>
			/// 
			/// </summary>
			event_system_stop,
			/// <summary>
			/// 
			/// </summary>
			event_worker_sound_off,
			/// <summary>
			/// 
			/// </summary>
			event_station_join,
			/// <summary>
			/// 
			/// </summary>
			event_station_left,
			/// <summary>
			/// 
			/// </summary>
			event_station_pause,
			/// <summary>
			/// 
			/// </summary>
			event_station_resume,
			/// <summary>
			/// 
			/// </summary>
			event_station_closing,
			/// <summary>
			/// 
			/// </summary>
			event_station_install,
			/// <summary>
			/// 
			/// </summary>
			event_station_uninstall,
			/// <summary>
			/// 
			/// </summary>
			event_station_state
		};
		/**
		*\brief 事件广播(异步)
		*/
		bool zero_event_async(string publiher, zero_net_event event_type, string content);
		/**
		*\brief 事件广播(同步)
		*/
		bool zero_event_sync(string publiher, zero_net_event event_type, string content);

		/**
		* \brief  站点状态
		*/
		enum class station_state
		{
			/**
			* \brief 无，刚构造
			*/
			None,
			/**
			* \brief 重新启动
			*/
			ReStart,
			/**
			* \brief 正在启动
			*/
			Start,
			/**
			* \brief 正在运行
			*/
			Run,
			/**
			* \brief 已暂停
			*/
			Pause,
			/**
			* \brief 错误状态
			*/
			Failed,
			/**
			* \brief 将要关闭
			*/
			Closing,
			/**
			* \brief 已关闭
			*/
			Closed,
			/**
			* \brief 已销毁，析构已调用
			*/
			Destroy,
			/**
			* \brief 已卸载
			*/
			Uninstall,
			/**
			* \brief 未知
			*/
			Unknow
		};

		/**
		* \brief ZMQ套接字状态
		*/
		enum class zmq_socket_state
		{
			/**
			* \brief 没问题
			*/
			Succeed,
			/**
			* \brief 后续还有消息
			*/
			More,

			/**
			* \brief 空帧
			*/
			Empty,

			/**
			* \brief 主机不可达
			*/
			HostUnReach,
			/**
			* \brief 网络关闭
			*/
			NetDown,

			/**
			* \brief 网络不可达
			*/
			NetUnReach,

			/**
			* \brief 网络重置
			*/
			NetReset,

			/**
			* \brief 未连接
			*/
			NotConn,
			/**
			* \brief 连接已在使用中？
			*/
			ConnRefUsed,
			/**
			* \brief 连接中断
			*/
			ConnAborted,

			/**
			* \brief 连接重置
			*/
			ConnReset,

			/**
			* \brief 超时
			*/
			TimedOut,

			/**
			* \brief 正在处理中？
			*/
			InProgress,

			/**
			* \brief 跨线程调用？
			*/
			Mthread,

			/**
			* \brief 指定的socket不可用
			*/
			NotSocket,

			/**
			* \brief 内存不足
			*/
			NoBufs,

			/**
			* \brief 消息大小不合适？
			*/
			MsgSize,

			/**
			* \brief 指定的socket相关联的context已关闭
			*/
			Term,

			/**
			* \brief 系统信号中断
			*/
			Intr,

			/**
			* \brief 不支持？
			*/
			NotSup,

			/**
			* \brief 不支持的协议
			*/
			ProtoNoSupport,

			/**
			* \brief 协议不兼容
			*/
			NoCompatProto,

			/**
			* \brief ？
			*/
			AfNoSupport,

			/**
			* \brief 地址问题？
			*/
			AddrNotAvAll,
			/**
			* \brief 地址已被使用
			*/
			AddrInUse,
			/**
			* \brief ？
			*/
			Fsm,

			/**
			* \brief 重启
			*/
			Again,
			/**
			* \brief 其它错误
			*/
			Unknow
		};


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

		/*! 以下为帧类型说明符号*/
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

//以下为返回时的快捷状态:说明帧的第二节字([1])

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
#define ZERO_STATUS_FRAME_PLANERROR_ID char(0x8B)

		//以下为请求时的快捷命令:说明帧的第二节字([1])
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