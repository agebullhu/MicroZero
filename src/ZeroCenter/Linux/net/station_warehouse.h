#pragma once
#ifndef  _AGEBULL_STATION_WAREHOUSE_H_
#define _AGEBULL_STATION_WAREHOUSE_H_
#include "zero_config.h"
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
			* \brief 配置集合
			*/
			static map<string, shared_ptr<zero_config>> configs_;
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
			* \brief 保存配置
			*/
			static zero_config& insert_config(shared_ptr<zero_config>& config, bool save);

			/**
			* \brief 保存配置
			*/
			static void save_configs();

			/**
			* \brief 取得配置
			*/
			static shared_ptr<zero_config> get_config(const string& station_name,bool find_redis=true);

			/**
			* \brief 安装一个站点
			*/
			static bool install( const char* station_name,int station_type);

			/**
			* \brief 初始化
			*/
			static bool initialize();
			/**
			* \brief 站点卸载
			*/
			static	bool uninstall(const string& station_name);

			/**
			* \brief 还原站点
			*/
			static bool restore(shared_ptr<zero_config>& value);
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
#endif //!_AGEBULL_STATION_WAREHOUSE_H_
