#pragma once
#ifndef _ZERO_PLAN_H_
#define _ZERO_PLAN_H_
#include "../stdinc.h"
#include "../ext/shared_char.h"
#include "../cfg/json_config.h"

namespace agebull
{
	namespace zero_net
	{

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
		* \brief 计划状态
		*/
		enum class plan_message_state
		{
			/**
			* \brief 无状态
			*/
			none,
			/**
			* \brief 排队
			*/
			queue,
			/**
			* \brief 正常执行
			*/
			execute,
			/**
			* \brief 重试执行
			*/
			retry,
			/**
			* \brief 跳过
			*/
			skip,
			/**
			* \brief 暂停
			*/
			pause,
			/**
			* \brief 错误关闭
			*/
			error,
			/**
			* \brief 正常关闭
			*/
			close
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
			* \brief 命令
			*/
			shared_char command;

			/**
			* \brief 站点
			*/
			int station_type;

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
			bool no_skip;

			/**
			* \brief 跳过设置次数(-1暂停执行,0无效,1-n 跳过次数)
			*/
			int skip_set;

			/**
			* \brief 跳过次数计数,1 当no_skip=true时,空跳也会参与计数. 2 此计数在执行时发生,2.1 skip_set < 0 直接计算下一次执行时间, 2.2 在skip_set > 0时,skip_set < skip_num时直接计算下一次执行时间,否则正常执行
			*/
			int skip_num;

			/**
			* \brief 最后一次执行状态
			*/
			int exec_state;

			/**
			* \brief 计划状态
			*/
			plan_message_state plan_state;

			/**
			* \brief 加入时间
			*/
			time_t add_time;

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
			vector<shared_char> frames;


			/**
			* \brief 构造
			*/
			plan_message()
				: plan_id(0)
				  , station_type(0)
				  , plan_type()
				  , plan_value(0)
				  , plan_repet(0)
				  , real_repet(0)
				  , no_skip(false)
				  , skip_set(0)
				  , skip_num(0)
				  , exec_state(0)
				  , plan_state(plan_message_state::none)
				  , add_time(0)
				  , plan_time(0)
				  , exec_time(0)
			{
			}

			/**
			* \brief JSON读计划基本信息
			*/
			void read_plan(const char* plan);

			/**
			* \brief JSON序列化
			*/
			void write_info(acl::json_node& node) const;
			/**
			* \brief JSON序列化
			*/
			acl::string write_info() const;
			/**
			* \brief JSON序列化
			*/
			acl::string write_state() const;
			/**
			* \brief JSON序列化
			*/
			acl::string write_json() const;
			/**
			* \brief 加入本地缓存
			*/
			static void add_local(shared_ptr<plan_message>& msg);
			/**
			* \brief 保存下一次执行时间
			*/
			bool next();
			/**
			* \brief 计算下一次执行时间
			*/
			bool check_next();
			/**
			* \brief 读取消息
			*/
			static shared_ptr<plan_message> load_message(const char* key);

			/**
			* \brief 恢复执行
			*/
			bool reset();
			/**
			* \brief 暂停执行,同时移出计划队列
			*/
			bool pause();
			/**
			* \brief 进入错误状态,同时移出计划队列
			*/
			bool error();
			/**
			* \brief 关闭消息
			*/
			bool close();
			/**
			* \brief 删除一个消息
			*/
			bool remove() const;

			/**
			* \brief 保存消息
			*/
			bool save_message(bool full, bool exec, bool plan, bool res, bool skip, bool close);
			/**
			* \brief 保存消息参与者
			*/
			bool save_message_worker(vector<const char*>& workers) const;

			/**
			* \brief 保存消息参与者返回值
			*/
			bool save_message_result(const char* worker, vector<shared_char>& response);

			/**
			* \brief 取一个参与者的消息返回值
			*/
			vector<shared_char> get_message_result(const char* worker) const;

			/**
			* \brief 取全部参与者消息返回值
			*/
			map<acl::string, vector<shared_char>> get_message_result() const;

			/**
			* \brief 加入执行队列
			*/
			void join_queue(time_t time);

			/**
			* \brief 检查时间
			*/
			bool check_time();
			/**
			* \brief 检查几号
			*/
			bool check_month();

			/**
			* \brief 检查周几
			*/
			bool check_week();
			/**
			* \brief 检查延时
			*/
			bool check_delay(boost::posix_time::time_duration delay);

			/**
			* \brief 执行到期任务
			*/
			static void exec_now(std::function<void(shared_ptr<plan_message>&)> exec);


			/**
			* \brief 设置跳过
			*/
			bool set_skip(int set);
		};
	}
}

#endif //!_ZERO_PLAN_H_
