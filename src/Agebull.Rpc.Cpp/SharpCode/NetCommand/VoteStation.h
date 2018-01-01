#pragma once
#include <stdinc.h>
#include "NetStation.h"
namespace agebull
{
	namespace zmq_net
	{
		/**
		 * 投票者定义
		 */
		typedef struct
		{
			/**
			* 流程名称
			*/
			string flow_name;
			/**
			* 网络名称
			*/
			string net_name;
			/**
			 * 是否必须
			 */
			bool must;
			/**
			* 是否预定义
			*/
			bool predefinition;
			/**
			 * 是否就绪
			 */
			bool ready;
		}Voter, *PVoter;

		/**
		* @brief 表示一个网络投票站（并行任务或并行工作流）
		* 投票请求格式：
		* 1 请求标识（建议用GUID/UUID）
		* 2 请求状态
		* －voters 请求投票者名单, 返回名单的JSON格式的数组
		* －start 开始投票
		* －continue 继续等待投票结束，接收到投票者反馈的反应
		* －end 完成投票, 返回bye
		* －bad 投票失败, 返回bye
		* 3 请求参数
		* 投票返回格式：
		* 1 请求标识
		* 2 投票者，*表示系统（中间人）
		* 3 投票结果（建议使用JSON格式）
		*/
		class VoteStation :public RouteStation<VoteStation, Voter, STATION_TYPE_VOTE>
		{
		public:
			/**
			* @brief 构造
			*/
			VoteStation(string name);
			/**
			* @brief 析构
			*/
			virtual ~VoteStation() {}
		private:
			Voter create_item(const char* addr, const char * value) override
			{
				config cfg(value);
				Voter vote;
				vote.net_name = addr;
				vote.ready = true;
				vote.flow_name = cfg["flow_name"];
				vote.must = cfg["must"] == "true";
				vote.predefinition = cfg["predefinition"] == "true";
				return vote;
			}
			/**
			* @brief 工作集合的响应
			*/
			void onWorkerPollIn()override;
			/**
			* @brief 调用集合的响应
			*/
			void onCallerPollIn() override;

			/**
			* @brief 向发起者推送投票状态
			*/
			bool send_state(const char* client_addr, const  char* request_token, const char* voter, const char* state);

			/**
			* @brief 开始投票
			*/
			bool start_vote(const char* client_addr, const  char* request_token, const  char* request_argument);
			/**
			* @brief 向发起者推送投票者列表
			*/
			bool get_voters(const char* client_addr, const  char* request_token);

		public:
			/**
			*消息泵
			*/
			static void run(string name)
			{
				VoteStation* route = new VoteStation(name);
				boost::thread thrds_s1(boost::bind(VoteStation::start, shared_ptr<VoteStation>(route)));
			}
			/**
			* @brief 执行
			*/
			static void start(shared_ptr<VoteStation> netobj)
			{
				station_run(netobj);
			}
		};
	}
}