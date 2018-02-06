#ifndef ZMQ_API_VOTE_STATION_H
#pragma once
#include <stdinc.h>
#include <utility>
#include "ZeroStation.h"
#include "BalanceStation.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		 * 投票者定义
		 */
		struct voter
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
			bool must{ false };
			/**
			* 是否预定义
			*/
			bool predefinition{ false };
			/**
			 * 是否就绪
			 */
			bool ready{ false };
		};

		/**
		* \brief 表示一个网络投票站（并行任务或并行工作流）
		* 投票请求格式：
		* 1 请求标识（建议用GUID/UUID）,必须全网唯一
		* 2 请求状态
		* －* 请求投票者名单, 返回：投票者为&,内容名单的JSON格式的数组
		* －$ 取投票状态,返回：投票者为$, 按每个投票者名称、状态（各一帧）返回状态，最后两帧为(* + 总的状态)。
		* －@ 开始投票,此时投票状态为-start,返回内容为此时的投票状态（格式同$)。
		* －% 继续等待投票结束,接收到投票者反馈的反应。返回：投票者为*,内容为-waiting
		* －> 对未发送成功或无返回的投票者重新推送,返回：投票者为$, 按每个投票者名称、状态（各一帧）返回状态，最后两帧为(* + 总的状态)。
		* －v 完成投票,返回：投票者为*,内容为-end
		* －x 投票关闭, 返回：投票者为*,内容为-close
		* 3 请求参数
		* 
		* 投票返回格式：
		* 1 请求标识
		* 2 投票者：*表示系统（中间人）,$表示完整状态（系统+所有投票者），&表示内容为所有已注册投票者，具体名称为具体投票者
		* 3 内容：一或多个帧，*或具体投票者为一帧， $为多帧
		* 
		* 数据存储，使用Redis的Hash，格式如下：
		* 1 key: vote:[请求标识]
		* 2 field:
		*	* 总的状态，值有：-start,-waiting,-end,-close,+error
		*	# 投票内容
		*	@ 发起者
		*	[voter] 投票者状态：-send,+error或具体返回值
		*/
		class vote_station :public balance_station<vote_station, voter, STATION_TYPE_VOTE>
		{
		public:
			/**
			* \brief 构造
			*/
			explicit vote_station(string name)
				: balance_station<vote_station, voter, STATION_TYPE_VOTE>(std::move(name))
			{
			}

			/**
			* \brief 析构
			*/
			virtual ~vote_station()
				= default;

		private:
			/**
			 * \brief 构造节点
			 * \param addr 
			 * \param value 
			 * \return 
			 */
			voter create_item(const char* addr, const char* value) override
			{
				config cfg(value);
				voter v;
				v.net_name = addr;
				v.ready = true;
				v.flow_name = cfg["flow_name"];
				v.must = cfg["must"] == "true";
				v.predefinition = cfg["predefinition"] == "true";
				return v;
			}

			/**
			* \brief 工作集合的响应
			*/
			void response() override;
			/**
			* \brief 调用集合的响应
			*/
			void request(ZMQ_HANDLE socket) override;

			/**
			* \brief 执行一条命令
			*/
			sharp_char command(const char* caller, vector<sharp_char> lines) override;

			/**
			* \brief 向发起者推送投票状态
			*/
			bool send_state(const char* client_addr, const char* request_token, const char* voter, const char* state);
			/**
			* \brief 向发起者推送投票状态
			*/
			bool send_state(const char* client_addr, const char* request_token, const char* state, std::map<acl::string, acl::string>& values);

			/**
			* \brief 开始投票
			*/
			bool start_vote(const char* client_addr, const char* request_token, const char* request_argument);
			/**
			* \brief 向未投票者重新推送投票
			*/
			bool re_push_vote(const char* client_addr, const char* request_token);
			/**
			* \brief 向发起者推送投票者列表
			*/
			bool get_voters(const char* client_addr, const char* request_token);

		public:
			/**
			*消息泵
			*/
			static void run(string name)
			{
				vote_station* route = new vote_station(std::move(name));
				boost::thread thrds_s1(boost::bind(launch, shared_ptr<vote_station>(route)));
			}

			/**
			* \brief 执行
			*/
			static void launch(const shared_ptr<vote_station>& arg);
		};
	}
}
#endif
