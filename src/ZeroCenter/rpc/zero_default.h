#pragma once
#ifndef ZERO_DEFAULT_H
#define ZERO_DEFAULT_H
#include "zero_def_frame.h"
#include "zero_def_status.h"
#include "zero_def_command.h"
namespace agebull
{
	namespace zero_net
	{
		typedef void* zmq_handler;

		typedef uchar* tson_buffer;

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
			*\brief 站点动态信息
			*/
			event_station_trends,

			/**
			*\brief 站点信息更新
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
		 * \brief 预定义名称
		*/
		namespace zero_def
		{
			/**
			* \brief 网络状态
			*/
			namespace net_state
			{
				const int none = 0;
				const int runing = 1;
				const int closing = 2;
				const int closed = 3;
				const int distory = 4;
				const int failed = 5;
			}

			/**
			* \brief 站点类型
			*/
			namespace station_type
			{
				const int dispatcher = 1;//系统调度
				const int notify = 2;//发布订阅
				const int api = 3;//普通API
				const int vote = 4;//投票即并发机制
				const int route_api = 5;//2018.08.03:新增,定向路由API
				const int queue = 6;//2018.11.10:新增,队列任务(发完请求后进队列处理)
				const int extend_station = 0x80;//系统扩展站点分隔
				const int trace = 0xFD;////2019.01.06:新增,网络跟踪
				const int proxy = 0xFE;//反向代理
				const int plan = 0xFF;//计划任务

				inline bool is_pub_station(int type)
				{
					return type == dispatcher || type == trace || type == notify || type == plan || type == queue;
				}
				inline bool is_api_station(int type)
				{
					return type == api || type == vote || type == route_api;
				}
				inline bool is_sys_station(int type)
				{
					return (type == dispatcher || type > extend_station);
				}
				inline bool is_general_station(int type)
				{
					return (type > dispatcher && type < extend_station);
				}
			}


			/**
			 * \brief 预定义名称
			*/
			namespace name
			{
				constexpr auto system_manage = "SystemManage";
				constexpr auto plan_dispatcher = "PlanDispatcher";
				constexpr auto proxy_dispatcher = "ProxyDispatcher";
				constexpr auto trace_dispatcher = "TraceDispatcher";

				namespace head
				{
					constexpr auto tcp = '+';
					constexpr auto inproc = '-';
					constexpr auto plan = '*';
					constexpr auto proxy = '#';
					constexpr auto service = '<';
					constexpr auto client = '>';
				}
			}
			/**
			 * \brief Redis预定义KEY
			*/
			namespace redis_key
			{
				/**
				* \brief 端口自动分配的Redis键名
				*/
				constexpr auto next_port = "net:port:next";
			}
		}
	}
}
#endif