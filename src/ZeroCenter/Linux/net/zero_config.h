#pragma once
#ifndef _ZERO_CONFIG_H_
#define _ZERO_CONFIG_H_
#include "../stdinc.h"
#include "net_default.h"
#include <utility>
#include "../log/mylogger.h"

#include<boost/unordered_map.hpp>
namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 工作对象
		*/
		struct worker
		{
			/**
			* \brief 实名
			*/
			string real_name;
			/**
			* \brief 上报的IP地址
			*/
			string ip_address;

			/**
			* \brief 上次心跳的时间
			*/
			time_t pre_time;

			/**
			* \brief 健康等级
			*/
			int level;

			/**
			* \brief 状态 -1 已失联 0 正在准备中 1 已就绪 3 已退出
			*/
			int state;

			/**
			 * \brief 构造
			 */
			worker()
				: pre_time(time(nullptr))
				, level(5)
				, state(0)
			{

			}
			/**
			* \brief 激活
			*/
			void active()
			{
				pre_time = time(nullptr);
				level = 5;
			}
			/**
			* \brief 检查
			*/
			int check();
		};

		class zero_station;
		/**
		* \brief ZMQ的网络站点配置
		*/
		class zero_config
		{
			friend class zero_station;
			/**
			* \brief 已就绪的站点数量
			*/
			int ready_works_;
			const char* type_name_;
		public:
			/**
			* \brief 实例队列访问锁
			*/
			boost::mutex mutex_;
			/**
			* \brief 站点名称
			*/
			string station_name_;
			/**
			* \brief 站点名称
			*/
			string short_name;
			/**
			* \brief 站点标题
			*/
			string station_caption_;
			/**
			* \brief 站点说明
			*/
			string station_description_;
			/**
			* \brief 站点别名
			*/
			vector<string> alias_;

			/**
			* \brief 站点类型
			*/
			int station_type_;

			/**
			* \brief 外部地址
			*/
			int request_port_;

			/**
			* \brief 工作出站地址
			*/
			int worker_out_port_;

			/**
			* \brief 工作返回地址
			*/
			int worker_in_port_;

			/**
			* \brief 当前站点状态
			*/
			station_state station_state_;
			/**
			* \brief 总请求次数
			*/
			int64 request_in, request_out, request_err;
			/**
			* \brief 总返回次数
			*/
			int64 worker_in, worker_out, worker_err;

			map<string, worker> workers;

			/**
			* \brief 构造
			*/
			zero_config()
				: ready_works_(0)
				, type_name_("ERR")
				, station_type_(0)
				, request_port_(0)
				, worker_out_port_(0)
				, worker_in_port_(0)
				, station_state_(station_state::None)
				, request_in(0)
				, request_out(0)
				, request_err(0)
				, worker_in(0)
				, worker_out(0)
				, worker_err(0)
			{
			}

			/**
			* \brief 构造
			* \param name
			* \param type
			*/
			zero_config(const string& name, int type)
				: ready_works_(0)
				, station_name_(std::move(name))
				, station_type_(type)
				, request_port_(0)
				, worker_out_port_(0)
				, worker_in_port_(0)
				, station_state_(station_state::None)
				, request_in(0)
				, request_out(0)
				, request_err(0)
				, worker_in(0)
				, worker_out(0)
				, worker_err(0)
			{
				check_type_name();
			}

			/**
			* \brief 工作站点加入
			*/
			void worker_join(const char* real_name, const char* ip);


			/**
			* \brief 工作站点就绪
			*/
			void worker_ready(const char* real_name);

			/**
			* \brief 心跳
			*/
			void worker_heartbeat(const char* real_name)
			{
				worker_ready(real_name);
			}
			/**
			* \brief 心跳
			*/
			void worker_left(const char* real_name);

			/**
			* \brief 检查工作对象
			*/
			void check_works();

			/**
			* \brief 是否有准备就绪的工作站(广播模式时都有)
			*/
			bool hase_ready_works() const
			{
				return station_type_ <= STATION_TYPE_PUBLISH || ready_works_ > 0;
			}

			/**
			* \brief 站点名称
			*/
			int get_station_type() const
			{
				return station_type_;
			}

			/**
			* \brief 当前站点状态
			*/
			station_state get_station_state() const
			{
				return station_state_;
			}

			/**
			* \brief 站点名称
			*/
			const string& get_station_name() const
			{
				return station_name_;
			}

			/**
			* \brief 调用地址
			*/
			string get_request_address() const
			{
				string addr("tcp://*:");
				addr += std::to_string(request_port_);
				return addr;
			}

			/**
			* \brief 工作地址
			*/
			string get_work_out_address() const
			{
				string addr("tcp://*:");
				addr += std::to_string(worker_out_port_);
				return addr;
			}

			/**
			* \brief 工作地址
			*/
			string get_work_in_address() const
			{
				string addr("tcp://*:");
				addr += std::to_string(worker_in_port_);
				return addr;
			}
			/**
			* \brief 从JSON中读取
			*/
			void read_json(const char* val);

			void check_type_name()
			{
				switch (station_type_)
				{
				case STATION_TYPE_API:
					type_name_ = "API";
					break;
				case STATION_TYPE_VOTE:
					type_name_ = "VOTE";
					break;
				case STATION_TYPE_PUBLISH:
					type_name_ = "PUBLISH";
					break;
				case  STATION_TYPE_DISPATCHER:
					type_name_ = "DISPATCHER";
					break;
				default:
					type_name_ = "ERR";
					break;
				}
			}

			/**
			* \brief 写入JSON
			* \param type 记录类型 0 全量 1 心跳时的动态信息 2 配置保存时无动态信息
			*/
			acl::string to_json(int type);

			/**
			* \brief 开机日志
			*/
			void start()
			{
				full_log(station_state_ == station_state::ReStart ? "restart" : "start");
				station_state_ = station_state::Start;
			}

			/**
			* \brief 开机失败日志
			*/
			void failed(const char* msg)
			{
				error("con`t launch", msg);
				station_state_ = station_state::Failed;
			}

			/**
			* \brief 开机正常日志
			*/
			void runing()
			{
				full_log("runing");
				station_state_ = station_state::Run;
			}

			/**
			* \brief 正在关机日志
			*/
			void closing()
			{
				station_state_ = station_state::Closing;
				full_log("closing...");
			}

			/**
			* \brief 重启日志
			*/
			void restart()
			{
				full_log("restart");
				station_state_ = station_state::ReStart;
			}

			/**
			* \brief 关机日志
			*/
			void closed()
			{
				station_state_ = station_state::Closed;
				full_log("closed");
			}

			/**
			* \brief 日志
			*/
			void full_log(const char* state) const
			{
				if (worker_in_port_ > 0)
					log_msg6("[%s]: > %s (type:%s prot:%d | %d<=>%d)", station_name_.c_str(), state, type_name_, request_port_, worker_out_port_, worker_in_port_)
				else
					log_msg5("[%s]: > %s (type:%s prot:%d | %d)", station_name_.c_str(), state, type_name_, request_port_, worker_out_port_)
			}

			/**
			* \brief 日志
			*/
			void log(const char* msg) const
			{
				log_msg3("[%s]: > %s (ready_works:%d)", station_name_.c_str(), msg, ready_works_);
			}

			/**
			* \brief 日志
			*/
			void log(const char* title, const char* msg) const
			{
				log_msg4("[%s] > %s > %s (ready_works:%d)", station_name_.c_str(), title, msg, ready_works_);
			}

			/**
			* \brief 日志
			*/
			void error(const char* title, const char* msg) const
			{
				log_error4("[%s] > %s > %s (ready_works:%d)", station_name_.c_str(), title, msg, ready_works_);
			}
		};
	}
}
#endif //!_ZERO_CONFIG_H_