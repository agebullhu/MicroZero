#ifndef ZERO_DEF_FRAME_H
#define ZERO_DEF_FRAME_H
#pragma once

namespace agebull
{
	namespace zero_net
	{
		namespace zero_def
		{
			/**
			 * \brief 帧类型说明符号
			*/
			namespace frame
			{
#define vector_str(ls,_idx_) (_idx_  <= 0 || _idx_ >= ls.size() ? nullptr : *ls[_idx_])
#define vector_int(ls,_idx_) (_idx_  <= 0 || _idx_ >= ls.size() ? -1 : atoi(*ls[_idx_]))
#define vector_ptr(ls,_idx_) (_idx_  <= 0 || _idx_ >= ls.size() ? shared_char() : ls[_idx_])
				
				typedef unsigned char uchar;
				//一般终止符号
				constexpr uchar  general_end = '\0';
				//扩展命令终止符号
				constexpr uchar  extend_end = 0xFF;
				//扩展命令终止符号
				constexpr uchar  result_end = 0xFE;
				//全局标识
				constexpr uchar  global_id = 0x1;
				//站点
				constexpr uchar  station_id = 0x2;
				//状态
				constexpr uchar  status = 0x3;
				//请求ID
				constexpr uchar  request_id = 0x4;
				//执行计划
				constexpr uchar  plan = 0x5;
				//执行计划
				constexpr uchar  plan_time = 0x6;
				//服务认证标识
				constexpr uchar  service_key = 0x7;
				//本地标识
				constexpr uchar  local_id = 0x8;

				//调用方的站点类型
				constexpr uchar  station_type = 0x9;
				//调用方的全局标识
				constexpr uchar  call_id = 0xB;
				//数据方向
				constexpr uchar  data_direction = 0xC;
				//原样参数
				constexpr uchar  original_1 = 0x10;
				//原样参数
				constexpr uchar  original_2 = 0x11;
				//原样参数
				constexpr uchar  original_3 = 0x12;
				//原样参数
				constexpr uchar  original_4 = 0x13;
				//原样参数
				constexpr uchar  original_5 = 0x14;
				//原样参数
				constexpr uchar  original_6 = 0x15;
				//原样参数
				constexpr uchar  original_7 = 0x16;
				//原样参数
				constexpr uchar  original_8 = 0x17;
				//命令
				constexpr uchar  command = '$';
				//参数
				constexpr uchar  arg = '%';
				//通知主题
				constexpr uchar  pub_title = '*';
				//通知副题
				constexpr uchar  sub_title = command;
				//网络上下文信息
				constexpr uchar  context = '#';
				//请求者/生产者
				constexpr uchar  requester = '>';
				//发布者/生产者
				constexpr uchar  publisher = requester;
				//回复者/浪费者
				constexpr uchar  responser = '<';
				//订阅者/浪费者
				constexpr uchar  subscriber = responser;
				//内容:一般文本
				constexpr uchar  content = 'T';
				//内容:一般文本
				constexpr uchar  content_text = content;
				//内容:json
				constexpr uchar  content_json = 'J';
				//内容:二进制
				constexpr uchar  content_bin = 'B';
				//内容：XML
				constexpr uchar  content_xml = 'X';
				
			}
			/**
			* \brief 说明帧解析
			*/
			acl::string desc_str(bool in, uchar* desc, size_t len);
		}
	}
}
#endif