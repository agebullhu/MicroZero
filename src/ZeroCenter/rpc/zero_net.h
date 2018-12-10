#pragma once
#ifndef ZERO_NET_H_
#define ZERO_NET_H_
#include "zero_default.h"
namespace agebull
{
	namespace zero_net
	{

		/**
		* \brief  站点状态
		*/
		enum class station_state
		{
			/**
			* \brief 无，刚构造
			*/
			none,
			/**
			* \brief 重新启动
			*/
			re_start,
			/**
			* \brief 正在启动
			*/
			start,
			/**
			* \brief 正在运行
			*/
			run,
			/**
			* \brief 已暂停
			*/
			pause,
			/**
			* \brief 错误状态
			*/
			failed,
			/**
			* \brief 将要关闭
			*/
			closing,
			/**
			* \brief 已关闭
			*/
			closed,
			/**
			* \brief 已销毁，析构已调用
			*/
			destroy,
			/**
			* \brief 已关停
			*/
			stop,
			/**
			* \brief 未知
			*/
			unknow
		};


		/**
		*\brief ZMQ上下文对象
		*/
		zmq_handler get_zmq_context();

		/**
		*\brief 运行状态
		*/
		int get_net_state();

		/**
		*\brief 线程计数清零
		*/
		void reset_command_thread(int count);

		/**
		*\brief 登记线程失败
		*/
		void set_command_thread_bad(const char* name);

		/**
		*\brief 登记线程开始
		*/
		void set_command_thread_run(const char* name);

		/**
		*\brief 登记线程关闭
		*/
		void set_command_thread_end(const char* name);

		/**
		*\brief 等待关闭(仅限station_dispatcher结束时使用一次)
		*/
		void wait_close();

		/**
		*\brief  中心事件
		*/
		enum class zero_net_event
		{
			/**
			*\brief
			*/
			event_none = 0x0,
			/**
			*\brief
			*/
			event_system_start = 0x1,
			/**
			*\brief
			*/
			event_system_closing,
			/**
			*\brief
			*/
			event_system_stop,
			/**
			*\brief
			*/
			event_worker_sound_off,
			/**
			*\brief
			*/
			event_station_join,
			/**
			*\brief
			*/
			event_station_left,
			/**
			*\brief
			*/
			event_station_pause,
			/**
			*\brief
			*/
			event_station_resume,
			/**
			*\brief
			*/
			event_station_closing,
			/**
			*\brief
			*/
			event_station_install,
			/**
			*\brief
			*/
			event_station_stop,
			/**
			*\brief
			*/
			event_station_remove,
			/**
			*\brief
			*/
			event_station_state,

			/**
			*\brief
			*/
			event_station_update,

			/**
			*\brief 站点文档
			*/
			event_station_doc,

			/**
			*\brief 客户端加入
			*/
			event_client_join,

			/**
			*\brief 客户端退出
			*/
			event_client_left,

			/**
			*\brief 准备连接
			*/
			event_monitor_net_try,

			/**
			*\brief 连接失败
			*/
			event_monitor_net_failed,

			/**
			*\brief 连接成功
			*/
			event_monitor_net_connected,

			/**
			*\brief 关闭
			*/
			event_monitor_net_close,

			/**
			*\brief 计划加入
			*/
			event_plan_add = 0x1,

			/**
			*\brief 计划更新
			*/
			event_plan_update,

			/**
			*\brief 计划进入队列
			*/
			event_plan_queue,

			/**
			*\brief 计划正在执行
			*/
			event_plan_exec,

			/**
			*\brief 计划执行完成
			*/
			event_plan_result,

			/**
			*\brief 计划暂停
			*/
			event_plan_pause,

			/**
			*\brief 计划已结束
			*/
			event_plan_end,

			/**
			*\brief 计划已删除
			*/
			event_plan_remove,

			/**
			*\brief 自定义事件
			*/
			event_custom = 0xFF
		};

		/**
		*\brief 系统事件通知
		*/
		bool system_event(zero_net_event event_type, const char* sub = nullptr, const char* content = nullptr);

		/**
		*\brief 事件通知
		*/
		bool zero_event(zero_net_event event_type, const char* title, const char* sub, const char* content);

		
	}
}
#endif