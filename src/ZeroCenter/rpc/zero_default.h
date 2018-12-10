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
				const int proxy = 0xFE;//反向代理
				const int plan = 0xFF;//计划任务

				inline bool is_pub_station(int type)
				{
					return type == dispatcher || type == notify || type == plan || type == queue;
				}
				inline bool  is_sys_station(int type)
				{
					return (type == dispatcher || type == plan || type == proxy);
				}
				inline bool  is_general_station(int type)
				{
					return (type == api || type == notify || type == queue || type == vote || type == route_api);
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

				namespace head
				{
					constexpr auto tcp = "+";
					constexpr auto inproc = '-';
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