#pragma once
#ifndef  _AGEBULL_STATIONWAREHOUSE_H_
#include <stdinc.h>
namespace agebull
{
	namespace zmq_net
	{
		class ZeroStation;

		/**
		* @brief 网络站点实例管理（站点仓库，是不是很脑洞的名字）
		*/
		class StationWarehouse
		{
			/**
			* @brief 实例队列访问锁
			*/
			static boost::mutex _mutex;
		public:
			/**
			* @brief 实例集合
			*/
			static map<string, ZeroStation*> examples;
			/**
			* @brief 清除所有服务
			*/
			static void clear();
			/**
			* @brief 还原服务
			*/
			static int restore();
			/**
			* @brief 初始化服务
			*/
			static acl::string install(int station_type, const char* station_name);
			/**
			* @brief 还原服务
			*/
			static bool restore(acl::string& value);
			/**
			* @brief 加入服务
			*/
			static bool join(ZeroStation* station);
			/**
			* @brief 加入服务
			*/
			static bool left(ZeroStation* station);
			/**
			* @brief 加入服务
			*/
			static ZeroStation* find(string name);
		};
	}
}
#endif
