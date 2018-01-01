#pragma once
#include <stdinc.h>
#include "NetStation.h"
namespace agebull
{
	namespace zmq_net
	{

		class ApiStation :public RouteStation<ApiStation, string, STATION_TYPE_API>
		{
			/**
			* @brief API主机集合
			*/
			vector<string> _hosts;

			/**
			* @brief 当前工作者下标
			*/
			int _nowWorkIndex;
		public:
			/**
			* @brief 构造
			*/
			ApiStation(string name);
			/**
			* @brief 析构
			*/
			virtual ~ApiStation() {}
			/**
			*消息泵
			*/
			static void run(string name)
			{
				ApiStation* netobj = new ApiStation(name);
				boost::thread thrds_s1(boost::bind(start, shared_ptr<ApiStation>(netobj)));
			}
			/**
			* @brief 执行
			*/
			static void start(shared_ptr<ApiStation> netobj)
			{
				station_run(netobj);
			}
		private:
			/**
			* @brief 工作集合的响应
			*/
			void onWorkerPollIn() override;
			/**
			* @brief 调用集合的响应
			*/
			void onCallerPollIn() override;
			/**
			* @brief 工作集合的响应
			*/
			string create_item(const char* addr, const char * value)override
			{
				return value;
			}
			void ApiStation::left(char* name)override;
			/**
			* @brief 发送到工作者
			*/
			bool snedToWorker(char* work, char* client_addr, char* request);

			/**
			* @brief 取下一个工作对象
			*/
			char* getNextWorker();
		};
	}
}