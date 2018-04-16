#pragma once
#ifndef AGEBULL_COMMAND_H_
#define  AGEBULL_COMMAND_H_
namespace agebull
{
	namespace zmq_net
	{
		//正常状态
#define  zero_status_success '+'
		//错误状态
#define zero_status_bad  '-'
//正常状态
#define zero_command_ok  "+ok"
#define zero_command_plan  "+plan"
#define zero_command_runing  "+runing"
#define zero_command_bye  "+bye"
#define zero_command_wecome  "+wecome"
#define zero_vote_sended  "+send"
#define zero_vote_closed  "+close"
#define zero_vote_bye  "+bye"
#define zero_vote_waiting  "+waiting"
#define zero_vote_start  "+start"
#define zero_vote_end  "+end"

#define zero_command_no_find  "-no find"
#define zero_command_invalid  "-invalid" 
#define zero_command_timeout  "-time out"
#define zero_command_net_error  "-net error"
#define zero_command_no_support  "-no support"
#define zero_command_failed  "-failed"
#define zero_command_error  "-error"

#define zero_command_arg_error  "-ArgumentError! must like : call[name][command][argument]"
#define zero_command_install_arg_error  "-ArgumentError! must like :install [type] [name]"
#define zero_api_unknow_error  "-error"
#define zero_api_not_worker  "-not work"
#define zero_vote_unknow_error  "-error"

#define zero_command_name_worker_join  '@'
#define zero_command_name_start  '*'

//终止符号
#define zero_frame_end  'E'
//执行计划
#define zero_frame_plan  'P'
//参数
#define zero_frame_arg  'A'
//请求ID
#define zero_frame_request_id  'I'
//请求者/生产者
#define zero_frame_requester  'R'
//发布者/生产者
#define zero_frame_publisher  zero_frame_requester
//回复者/浪费者
#define zero_frame_responser  'G'
//订阅者/浪费者
#define zero_frame_subscriber  zero_responser
//广播主题
//#define zero_pub_title  '*'
//广播副题
#define zero_frame_subtitle  'S'
//网络上下文信息
#define zero_frame_context  'T'
	}
}
#endif