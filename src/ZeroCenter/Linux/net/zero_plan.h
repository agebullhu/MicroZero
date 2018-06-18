#pragma once
#ifndef _ZERO_PLAN_H_
#define _ZERO_PLAN_H_
#include "../stdinc.h"
#include "../shared_char.h"
#include "../cfg/json_config.h"

namespace agebull
{
	namespace zmq_net
	{

#define zsco_key "plan:time:set"

#define def_msg_key(key,msg)\
			char key[MAX_PATH];\
			sprintf(key, "zero:message:%s:%lld",*((msg)->station), (msg)->plan_id);

#define def_msg_worker_key(key,msg)\
			char key[MAX_PATH];\
			sprintf(key, "zero:worker:%s:%lld", *((msg)->station), (msg)->plan_id);

		/**
		* \brief 计划类型
		*/
		enum class plan_date_type
		{
			/**
			* \brief 无计划，立即发送
			*/
			none,
			/**
			* \brief 在指定的时间发送
			*/
			time,
			/**
			* \brief 秒间隔后发送
			*/
			second,
			/**
			* \brief 分钟间隔后发送
			*/
			minute,
			/**
			* \brief 小时间隔后发送
			*/
			hour,
			/**
			* \brief 日间隔后发送
			*/
			day,
			/**
			* \brief 每周几
			*/
			week,
			/**
			* \brief 每月几号
			*/
			month
		};
		/**
		* \brief 消息
		*/
		class plan_message
		{
		public:
			/**
			* \brief 消息标识
			*/
			int64_t plan_id;

			/**
			* \brief 发起者提供的标识
			*/
			shared_char request_id;

			/**
			* \brief 站点
			*/
			shared_char station;

			/**
			* \brief 原始请求者
			*/
			shared_char caller;

			/**
			* \brief 计划说明
			*/
			shared_char description;

			/**
			* \brief 计划类型
			*/
			plan_date_type plan_type;

			/**
			* \brief 类型值
			*/
			int plan_value;

			/**
			* \brief 重复次数,0不重复 >0重复次数,-1永久重复
			*/
			int plan_repet;

			/**
			* \brief 执行次数
			*/
			int real_repet;
			/**
			* \brief 是否空跳
			*/
			bool no_keep;

			/**
			* \brief 最后一次执行状态
			*/
			int last_state;

			/**
			* \brief 计划时间
			*/
			time_t plan_time;

			/**
			* \brief 执行时间
			*/
			time_t exec_time;

			/**
			* \brief 消息内容
			*/
			vector<shared_char> messages;


			/**
			* \brief 构造
			*/
			plan_message(): plan_id(0), plan_type(), plan_value(0), plan_repet(0), real_repet(0), no_keep(false), last_state(0),
			                plan_time(0), exec_time(0)
			{
			}

			/**
			* \brief JSON读计划基本信息
			*/
			void read_plan(const char* plan);

			/**
			* \brief JSON反序列化
			*/
			void read_json(acl::string& val);

			/**
			* \brief JSON序列化
			*/
			acl::string write_json() const;
			/**
			* \brief 保存下一次执行时间
			*/
			bool save() ;
			/**
			* \brief 保存消息
			*/
			bool save_message() ;
			/**
			* \brief 保存下一次执行时间
			*/
			bool save_next() ;
			/**
			* \brief 读取消息
			*/
			bool load_message(const char* key);

			/**
			* \brief 删除一个计划
			*/
			bool remove() const;
			/**
			* \brief 删除一个计划
			*/
			bool remove_next() const;
			/**
			* \brief 删除消息
			*/
			bool remove_message() const;

			/**
			* \brief 保存消息参与者
			*/
			bool save_message_worker(vector<const char*>& workers) const;

			/**
			* \brief 保存消息参与者返回值
			*/
			bool save_message_result(const char* worker, vector<shared_char>& response) const;

			/**
			* \brief 取一个参与者的消息返回值
			*/
			vector<shared_char> get_message_result(const char* worker) const;

			/**
			* \brief 取一个参与者的消息返回值
			*/
			static void get_message_result(vector<shared_char>& result, acl::string& val);

			/**
			* \brief 取全部参与者消息返回值
			*/
			map<acl::string, vector<shared_char>> get_message_result() const;

			/**
			* \brief 计划下一次执行时间
			*/
			bool plan_next();

			/**
			* \brief 加入执行队列
			*/
			bool join_queue(time_t time) const;


			/**
			* \brief 检查延时
			*/
			bool check_delay(boost::posix_time::ptime& now ,boost::posix_time::time_duration delay);

			/**
			* \brief 载入现在到期的内容并回调
			*/
			static void load_now(std::function<int(plan_message&)> exec);
		};

	}
}
#endif //!_ZERO_PLAN_H_