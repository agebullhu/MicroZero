#pragma once
#ifndef  _AGEBULL_station_warehouse_H_
#include <stdinc.h>
namespace agebull
{
	namespace zmq_net
	{
		class zero_station;

		/**
		* \brief 网络站点实例管理（站点仓库，是不是很脑洞的名字）
		*/
		class station_warehouse
		{
			/**
			* \brief 实例队列访问锁
			*/
			static boost::mutex mutex_;
		public:
			/**
			* \brief 实例集合
			*/
			static map<string, zero_station*> examples_;
			/**
			* \brief 清除所有服务
			*/
			static void clear();
			/**
			* \brief 还原服务
			*/
			static int restore();
			/**
			* \brief 初始化服务
			*/
			static acl::string install(int station_type, const char* station_name);
			/**
			* \brief 还原服务
			*/
			static bool restore(acl::string& value);
			/**
			* \brief 加入服务
			*/
			static bool join(zero_station* station);
			/**
			* \brief 加入服务
			*/
			static bool left(zero_station* station);
			/**
			* \brief 加入服务
			*/
			static zero_station* find(const string& name);
		};
	}
}
#endif
