#pragma once
#ifndef ZERO_NET_H_
#define ZERO_NET_H_
#include "zero_default.h"
namespace agebull
{
	namespace zero_net
	{



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