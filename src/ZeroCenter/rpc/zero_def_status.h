#ifndef ZERO_DEF_STATUS_H
#define ZERO_DEF_STATUS_H
#pragma once

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
		namespace zero_def
		{
			/**
			 * \brief 返回时的状态:说明帧的第二节字([1])
			*/
			namespace status
			{
				typedef unsigned char uchar;
				/*!
				 * 以下为返回时的快捷状态:说明帧的第二节字([1])
				 */

				constexpr uchar succeed = uchar(0x1);
				constexpr uchar ok = uchar(0x1);
				constexpr uchar jion_plan = uchar(0x2);
				constexpr uchar runing = uchar(0x3);
				constexpr uchar bye = uchar(0x4);
				constexpr uchar wecome = uchar(0x5);
				constexpr uchar wait = uchar(0x6);

				constexpr uchar vote_sended = uchar(0x70);
				constexpr uchar vote_bye = uchar(0x71);
				constexpr uchar vote_waiting = uchar(0x72);
				constexpr uchar vote_start = uchar(0x73);
				constexpr uchar vote_end = uchar(0x74);
				constexpr uchar vote_closed = uchar(0x75);

				constexpr uchar failed = uchar(0x80);
				constexpr uchar pause = uchar(0x81);

				constexpr uchar bug = uchar(0xD0);
				constexpr uchar frame_invalid = uchar(0xD1);
				constexpr uchar arg_invalid = uchar(0xD2);

				constexpr uchar error = uchar(0xF0);
				constexpr uchar not_find = uchar(0xF1);
				constexpr uchar not_worker = uchar(0xF2);
				constexpr uchar not_support = uchar(0xF3);
				constexpr uchar timeout = uchar(0xF4);
				constexpr uchar net_error = uchar(0xF5);
				constexpr uchar plan_error = uchar(0xF6);
				constexpr uchar send_error = uchar(0xF7);
				constexpr uchar recv_error = uchar(0xF8);
				constexpr uchar deny_error = uchar(0xF9);
			}

			/**
			 * \brief 状态文字
			 */
			namespace status_text
			{
				/*!
				* 正常状态
				*/
				constexpr auto  SUCCESS = '+';
				constexpr auto  OK = "+ok";
				constexpr auto  PLAN = "+plan";
				constexpr auto  RUNING = "+runing";
				constexpr auto  BYE = "+bye";
				constexpr auto  WECOME = "+wecome";
				constexpr auto  WAITING = "+waiting";
				constexpr auto  VOTE_SENDED = "+send";
				constexpr auto  VOTE_CLOSED = "+close";
				constexpr auto  VOTE_BYE = "+bye";
				constexpr auto  VOTE_START = "+start";
				constexpr auto  VOTE_END = "+end";

				/*!
				* 错误状态
				*/
				constexpr auto  BAD = '-';
				constexpr auto  DENY_ACCESS = "-deny";
				constexpr auto  FAILED = "-failed";
				constexpr auto  ERROR = "-error";
				constexpr auto  NOT_SUPPORT = "-not support";
				constexpr auto  NOT_FIND = "-not find";
				constexpr auto  NOT_WORKER = "-not work";
				constexpr auto  FRAME_INVALID = "-invalid frame";
				constexpr auto  ARG_INVALID = "-invalid argument";
				constexpr auto  TIMEOUT = "-time out";
				constexpr auto  NET_ERROR = "-net error";
				//constexpr auto  MANAGE_ARG_ERROR =   "-ArgumentError! must like : call[name][command][argument]";
				//constexpr auto  MANAGE_INSTALL_ARG_ERROR =   "-ArgumentError! must like :install [type] [name]";
				constexpr auto  PLAN_INVALID = "-plan invalid";
				constexpr auto  PLAN_ERROR = "-plan error";


			}
		}
	}
}
#endif