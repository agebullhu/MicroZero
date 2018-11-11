#pragma once
#ifndef _PLAN_DISPATCHER_H_
#define _PLAN_DISPATCHER_H_
#include "../stdinc.h"
#include "zero_plan.h"
#include "zero_station.h"
#include "inner_socket.h"

namespace agebull
{
	namespace zero_net
	{
		/**
		* \brief 表示计划任务调度服务
		*/
		class plan_dispatcher :public zero_station
		{
			friend plan_message;
			map<string, shared_ptr<inner_socket>> sockets_;
		public:
			/**
			* \brief 单例
			*/
			static plan_dispatcher* instance;

			/**
			* \brief 构造
			*/
			plan_dispatcher()
				:zero_station("PlanDispatcher", station_type_plan, ZMQ_ROUTER, ZMQ_PUB)
			{

			}
			/**
			* \brief 构造
			*/
			plan_dispatcher(shared_ptr<zero_config>& config)
				:zero_station(config, station_type_plan, ZMQ_ROUTER, ZMQ_PUB)
			{

			}
			/**
			* \brief 析构
			*/
			~plan_dispatcher() final = default;

			/**
			* \brief 运行一个通知线程
			*/
			static void run()
			{
				instance = new plan_dispatcher();
				boost::thread(boost::bind(launch, shared_ptr<plan_dispatcher>(instance)));
			}
		private:
			/**
			*\brief 运行
			*/
			static void run(shared_ptr<zero_config>& config)
			{
				instance = new plan_dispatcher(config);
				boost::thread(boost::bind(launch, shared_ptr<plan_dispatcher>(instance)));
			}
			/**
			* \brief 消息泵
			*/
			static void launch(shared_ptr<plan_dispatcher>& station);


			/**
			* \brief 计划轮询
			*/
			static void run_plan_poll(plan_dispatcher* station)
			{
				station->plan_poll();
			}
			/**
			* \brief 计划轮询
			*/
			void plan_poll();

			/**
			*\brief 发布消息
			*/
			bool result_event(shared_ptr<plan_message>& message, vector<shared_char>& result);
			
			/**
			*\brief 通知内容
			*/
			bool zero_event(zero_net_event event_type, const plan_message* message);

			/**
			* \brief 工作开始（发送到工作者）
			*/
			void job_start(zmq_handler socket, vector<shared_char>& list, bool inner) final;
			/**
			* \brief 工作结束(发送到请求者)
			*/
			void job_end(vector<shared_char>& list) final
			{
			}
			/**
			* \brief 执行计划
			*/
			void exec_plan(shared_ptr<plan_message>& msg);
			/**
			* \brief 计划执行返回
			*/
			void on_plan_result(vector<shared_char>& list);
			/**
			* \brief 计划执行返回
			*/
			void on_plan_result(shared_ptr<plan_message>& message, char state, vector<shared_char>& list);
			/**
			* \brief 计划进入
			*/
			bool on_plan_start(zmq_handler socket, vector<shared_char>& list);
			/**
			* \brief 计划管理
			*/
			void on_plan_manage(zmq_handler socket, vector<shared_char>& list);
			/**
			* \brief 执行命令
			*/
			static char exec_command(const char* command, vector<shared_char>& arguments, string& json);
			/**
			* \brief 计划列表
			*/
			static void plan_list(string& json);
		};
	}
}
#endif //!_PLAN_DISPATCHER_H_