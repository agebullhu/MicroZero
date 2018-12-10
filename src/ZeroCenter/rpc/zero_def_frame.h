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
				typedef unsigned char uchar;
				//终止符号
				constexpr uchar  end = '\0';
				//全局标识
				constexpr uchar  global_id = '\001';
				//站点
				constexpr uchar  station_id = '\002';
				//状态
				constexpr uchar  status = '\003';
				//请求ID
				constexpr uchar  request_id = '\004';
				//执行计划
				constexpr uchar  plan = '\005';
				//执行计划
				constexpr uchar  plan_time = '\006';
				//服务认证标识
				constexpr uchar  service_key = '\007';
				//本地标识
				constexpr uchar  local_id = '\010';
				//原样参数
				constexpr uchar  original_1 = '\011';
				//原样参数
				constexpr uchar  original_2 = '\012';
				//原样参数
				constexpr uchar  original_3 = '\013';
				//原样参数
				constexpr uchar  original_4 = '\014';
				//原样参数
				constexpr uchar  original_5 = '\015';
				//原样参数
				constexpr uchar  original_6 = '\016';
				//原样参数
				constexpr uchar  original_7 = '\017';
				//原样参数
				constexpr uchar  original_8 = '\020';
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
				//内容
				constexpr uchar  content = 'T';
				constexpr uchar  content_text = 'T';
				constexpr uchar  content_json = 'J';
				constexpr uchar  content_bin = 'B';
				constexpr uchar  content_xml = 'X';
				
			}
			/**
			* \brief 说明帧解析
			*/
			acl::string desc_str(bool in, char* desc, size_t len);
		}
	}
}
#endif