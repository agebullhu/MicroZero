#pragma once
#ifndef  _AGEBULL_STATION_WAREHOUSE_H_
#define _AGEBULL_STATION_WAREHOUSE_H_
#include "zero_config.h"
namespace agebull
{
	namespace zmq_net
	{
		class zero_station;
		class station_dispatcher;
		/**
		* \brief 网络站点实例管理（站点仓库，是不是很脑洞的名字）
		*/
		class station_warehouse
		{
			//friend class zero_station;
			friend class station_dispatcher;
			/**
			* \brief 全局ID
			*/
			static int64 glogal_id_, glogal_id_1;
			/**
			* \brief 实例队列访问锁
			*/
			static boost::mutex config_mutex_;
			/**
			* \brief 实例队列访问锁
			*/
			static boost::mutex examples_mutex_;
			/**
			* \brief 配置集合
			*/
			static map<string, shared_ptr<zero_config>> configs_;
			/**
			* \brief 实例集合
			*/
			static map<string, zero_station*> examples_;
		public:

			/**
			* \brief 取全局ID
			*/
			static int64 get_glogal_id()
			{
				if (glogal_id_1 == 0)
				{
					redis_live_scope redis(json_config::redis_defdb);
					redis->incr("sys:gid",&glogal_id_1);
					if (glogal_id_1 > 0xFFFF)
					{
						glogal_id_1 = 1;
						redis->set("sys:gid", "1");
					}
				}
				boost::lock_guard<boost::mutex> grard(examples_mutex_);
				if (glogal_id_ >= 0xFFFFFFFFFFFFF)
					glogal_id_ = 0;
				return ((++glogal_id_) << 16) | glogal_id_1;
			}
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
			* \brief 取得站点总数
			*/
			static size_t get_station_count()
			{
				return configs_.size();
			}

			/**
			* \brief 取得配置
			*/
			static shared_ptr<zero_config>& get_config(const string& station_name, bool find_redis = true);

			/**
			* \brief 安装一个站点
			*/
			static bool install(const char* station_name, int station_type, const char* short_name);

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
			/**
			* \brief 取机器信息
			*/
			static char host_info(const string& stattion, string& json);
			/**
			* \brief 设置关闭
			*/
			static void set_close_info();
		public:
			/**
			* \brief 遍历配置
			*/
			static void foreach_configs(std::function<void(shared_ptr<zero_config>&)> look);

		};
	}
}
#endif //!_AGEBULL_STATION_WAREHOUSE_H_
