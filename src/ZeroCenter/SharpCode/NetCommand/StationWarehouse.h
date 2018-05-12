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
			* \brief 清除所有站点
			*/
			static void clear();
			/**
			* \brief 还原站点
			*/
			static int restore();
			/**
			* \brief 安装一个站点
			*/
			static bool install( const char* station_name,int station_type, acl::string& config);

			/**
			* \brief 站点卸载
			*/
			static	bool uninstall_station(const string& stattion);

			/**
			* \brief 还原站点
			*/
			static bool restore(acl::string& value);
			/**
			* \brief 加入站点
			*/
			static bool join(zero_station* station);
			/**
			* \brief 加入站点
			*/
			static bool left(zero_station* station);
			/**
			* \brief 查找已运行站点
			*/
			static zero_station* instance(const string& name);
		};
	}
}
#endif
