#pragma once
#ifndef  _AGEBULL_STATION_WAREHOUSE_H_
#define _AGEBULL_STATION_WAREHOUSE_H_
#include "zero_config.h"
namespace agebull
{
	namespace zero_net
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
			static int64 glogal_id_, reboot_num_;
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
			static map<string, zero_station*> examples;

		public:
			/**
			* \brief 取全局ID
			*/
			static int64 get_glogal_id()
			{
				boost::lock_guard<boost::mutex> grard(examples_mutex_);
				if (++glogal_id_ >= 0xFFFFFFFFFFFFF)
					glogal_id_ = 0;
				return (reboot_num_ << 48) | glogal_id_;
			}
			/**
			* \brief 清除所有站点
			*/
			static void clear();
			/**
			* \brief 恢复站点
			*/
			static int restore();

			/**
			* \brief 恢复站点
			*/
			static bool restore(shared_ptr<zero_config>& value);

			/**
			* \brief 保存配置
			*/
			static void insert_config(shared_ptr<zero_config> config);

			/**
			* \brief 保存配置
			*/
			static void save_configs();
			/**
			* \brief 取得站点总数
			*/
			static size_t get_station_count()
			{
				int cnt = 0;
				for (auto& kv : configs_)
				{
					if (kv.second->is_state(station_state::stop))
						continue;
					cnt++;
				}
				return cnt;
			}

			/**
			* \brief 取得配置
			*/
			static shared_ptr<zero_config>& get_config(const char* station_name, bool find_redis = true);

			/**
			* \brief 加入站点
			*/
			static bool join(zero_station* station_name);
			/**
			* \brief 加入站点
			*/
			static bool left(zero_station* station_name);
			/**
			* \brief 设置关闭
			*/
			static void set_all_destroy();
		public:
			/**
			* \brief 遍历配置
			*/
			static void foreach_configs(std::function<void(shared_ptr<zero_config>&)> look);

			/**
			* \brief 取机器信息
			*/
			static char host_info(const string& station_name, string& json);

			/**
			* \brief 上传文档
			*/
			static bool upload_doc(const char* station_name, shared_char& doc);

			/**
			* \brief 获取文档
			*/
			static bool get_doc(const char* station_name, string& doc);

			/**
			* \brief 安装一个站点
			*/
			static bool install(const char* json_str);

			/**
			* \brief 安装站点
			*/
			static bool install(const char* station_name, const char* type_name, const char* short_name, const char* desc);
			/**
			* \brief 初始化
			*/
			static bool initialize();
			/**
			* \brief 站点关停
			*/
			static	bool stop(const string& station_name);

			/**
			* \brief 还原已关停站点
			*/
			static bool recover(const char* station_name);
			/**
			* \brief 删除已关停站点
			*/
			static bool remove(const char* station_name);
			/**
			* \brief 站点更新
			*/
			static bool update(const char* json);
			/**
			* \brief 安装站点
			*/
			static bool install(const char* station_name, int type, const char* short_name, const char* desc, bool is_base)
			{
				shared_ptr<zero_config> config = make_shared<zero_config>();
				config->station_type = type;
				config->station_name = station_name;
				config->short_name = short_name;
				config->station_description = desc;
				config->is_base = is_base;
				return install(config);
			}
			/**
			* \brief 安装一个站点
			*/
			static bool install(shared_ptr<zero_config>& config);
			/**
			* \brief 保存站点
			*/
			static acl::string save(shared_ptr<zero_config>& config);
		public:
			/**
			* \brief 查找已运行站点
			*/
			static zero_station* instance(const string& name);
			/**
			*  \brief 启动站点
			*/
			static char start_station(string station_name);
			/**
			* \brief 暂停站点
			*/
			static char pause_station(const string& station_name);
			/**
			* \brief 继续站点
			*/
			static char resume_station(const string& station_name);
			/**
			* \brief 关闭站点
			*/
			static char close_station(const string& station_name);
			/**
			* 心跳的响应
			*/
			static bool heartbeat(uchar cmd, vector<shared_char>& lines);
		};
	}
}
#endif //!_AGEBULL_STATION_WAREHOUSE_H_
