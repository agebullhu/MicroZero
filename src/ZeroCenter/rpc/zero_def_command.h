#ifndef ZERO_DEFAULT_COMMAND_H
#define ZERO_DEFAULT_COMMAND_H
#pragma once
namespace agebull
{
	namespace zero_net
	{
		namespace zero_def
		{
			/**
			 * \brief 请求时的快捷命令:说明帧的第二节字([1])
			 */
			namespace command
			{
				typedef unsigned char uchar;
				/**
				* \brief 无特殊说明
				*/
				constexpr uchar none = uchar('\1');

				/**
				* \brief 进入计划
				*/
				constexpr uchar plan = uchar('\2');

				/**
				* \brief 代理执行
				*/
				constexpr uchar proxy = uchar('\3');
				/**
				* \brief 取得未处理数据
				*/
				constexpr uchar restart = uchar('\4');

				/**
				* \brief 取全局标识
				*/
				constexpr uchar  global_id = uchar('>');

				/**
				* \brief 等待结果
				*/
				constexpr uchar waiting = uchar('#');

				/**
				* \brief 查找结果
				*/
				constexpr uchar find_result = uchar('%');

				/**
				* \brief 关闭结果
				*/
				constexpr uchar close_request = uchar('-');

				/**
				* \brief Ping
				*/
				constexpr uchar ping = uchar('*');

				/**
				* \brief 心跳加入
				*/
				constexpr uchar  heart_join = uchar('J');

				/**
				* \brief 心跳已就绪
				*/
				constexpr uchar  heart_ready = uchar('R');

				/**
				* \brief 心跳进行
				*/
				constexpr uchar  heart_pitpat = uchar('P');

				/**
				* \brief 心跳退出
				*/
				constexpr uchar  heart_left = uchar('L');

			}
		}
	}
}
#endif