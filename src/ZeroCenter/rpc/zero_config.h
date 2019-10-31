#pragma once
#ifndef _ZERO_CONFIG_H_
#define _ZERO_CONFIG_H_
#include "../stdinc.h"
#include "zero_net.h"
#include <utility>
#include "../log/mylogger.h"
#include "../ext/shared_char.h"

namespace agebull
{
	namespace zero_net
	{
		class zero_station;
		class station_config;

		/**
		* \brief 工作对象
		*/
		struct station_worker
		{
			/**
			* \brief 实名
			*/
			char worker_name[256];
			/**
			* \brief 上报的IP地址
			*/
			string worker_ip;

			/**
			* \brief 上次心跳的时间
			*/
			boost::posix_time::ptime worker_last;

			/**
			* \brief 健康等级（5级 5.2 | 4.4 | 3.8 | 2.16 | 1.32 | 0.64 | -1.> 64）
			*/
			int worker_health;

			/**
			* \brief 状态 0 准备 1 就绪 2 缓慢 3 失联 4 无法发送 5 退出
			*/
			int worker_state;

			/**
			 * \brief 构造
			 */
			station_worker()
				: worker_last(boost::posix_time::microsec_clock::local_time())
				, worker_health(5)
				, worker_state(0)
			{
				worker_name[0] = 0;
			}

			/**
			* \brief 激活
			*/
			void active()
			{
				worker_last = boost::posix_time::microsec_clock::local_time();
				worker_health = 5;
				worker_state = 1;
			}
		};

		/**
		* \brief ZMQ的网络站点配置
		*/
		class station_config
		{
			friend class zero_station;
			/**
			* \brief 已就绪的站点数量
			*/
			int ready_works_;
			/**
			* \brief 类型名称(冗余)
			*/
			const char* type_name_;
			/**
			* \brief 当前站点状态
			*/
			station_state station_state_;
			/**
			* \brief 当前站点状态
			*/
			station_state config_state_;
			/**
			* \brief 前一个工具者索引
			*/
			int worker_idx_;
			/**
			* \brief 检查
			*/
			static int check_worker(station_worker& station_worker);

		public:
			/**
			* \brief 实例队列访问锁
			*/
			boost::mutex mutex;
			/**
			* \brief 站点名称
			*/
			string station_name;
			/**
			* \brief 站点标题
			*/
			string station_caption;
			///**
			//* \brief 站点名称
			//*/
			//string short_name;
			/**
			* \brief 站点说明
			*/
			string station_description;
			///**
			//* \brief 站点别名
			//*/
			//vector<string> alias;

			/**
			* \brief 是否基础站点
			*/
			bool is_base;

			/**
			* \brief 是否做保真处理
			*/
			bool is_fidelity;

			/**
			* \brief 站点类型
			*/
			int station_type;

			/**
			* \brief 地址
			*/
			string station_address;

			/**
			* \brief 外部端口
			*/
			int request_port;
			
			/**
			* \brief 工作请求端口
			*/
			int worker_out_port;

			/**
			* \brief 工作返回端口
			*/
			int worker_in_port;

			/**
			* \brief 请求业务次数
			*/
			int64 request_in;
			/**
			* \brief 请求返回次数
			*/
			int64 request_out;
			/**
			* \brief 请求错误次数
			*/
			int64 request_err;
			/**
			* \brief 请求返回失败次数
			*/
			int64 request_deny;
			/**
			* \brief 工作返回次数
			*/
			int64 worker_in;
			/**
			* \brief 工作发出次数
			*/
			int64 worker_out;
			/**
			* \brief 工作错误次数
			*/
			int64 worker_err;
			/**
			* \brief 工作错误次数
			*/
			int64 worker_deny;

			vector<station_worker> workers;

			/**
			* \brief 构造
			*/
			station_config()
				: ready_works_(0)
				  , type_name_("ERR")
				  , station_state_(station_state::none)
				  , config_state_(station_state::run)
				  , worker_idx_(-1)
				  , is_base(false)
				  , is_fidelity(false)
				  , station_type(0)
				  , request_port(0)
				  , worker_out_port(0)
				  , worker_in_port(0)
				  , request_in(0)
				  , request_out(0)
				  , request_err(0)
				  , request_deny(0), worker_in(0)
				  , worker_out(0)
				  , worker_err(0), worker_deny(0)
			{
			}

			/**
			* \brief 构造
			* \param name
			* \param type
			*/
			station_config(const string& name, int type)
				: ready_works_(0)
				  , station_state_(station_state::none)
				  , config_state_(station_state::run)
				  , station_name(std::move(name))
				  , is_base(false)
				  , station_type(type)
				  , request_port(0)
				  , worker_out_port(0)
				  , worker_in_port(0)
				  , request_in(0)
				  , request_out(0)
				  , request_err(0)
				  , request_deny(0)
				  , worker_in(0)
				  , worker_out(0)
				  , worker_err(0)
				  , worker_deny(0)
			{
				check_type_name();
			}

			/**
			* \brief 工作站点加入
			*/
			void worker_join(const char* worker_name, const char* ip);

			/**
			* \brief 工作站点就绪
			*/
			void worker_ready(const char* worker_name);

			/**
			* \brief 心跳
			*/
			void worker_heartbeat(const char* worker_name);

			/**
			* \brief 心跳
			*/
			void worker_left(const char* worker_name);

			/**
			* \brief 检查工作对象
			*/
			void check_works();

			/**
			* \brief 是否有准备就绪的工作站(通知模式时都有)
			*/
			bool hase_ready_works() const
			{
				return zero_def::station_type::is_pub_station(station_type) || ready_works_ > 0;
			}

			/**
			* \brief 站点名称
			*/
			int get_station_type() const
			{
				return station_type;
			}

			/**
			* \brief 站点名称
			*/
			station_worker* get_workers();

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
				return station_name;
			}

			/**
			* \brief 调用地址
			*/
			string get_request_address() const
			{
				string addr("tcp://*:");
				addr += std::to_string(request_port);
				return addr;
			}

			/**
			* \brief 工作地址
			*/
			string get_work_out_address() const
			{
				string addr("tcp://*:");
				addr += std::to_string(worker_out_port);
				return addr;
			}

			/**
			* \brief 工作地址
			*/
			string get_work_in_address() const
			{
				string addr("tcp://*:");
				addr += std::to_string(worker_in_port);
				return addr;
			}

			/**
			* \brief 从JSON中读取
			*/
			void read_json(const char* val);

			/**
			* \brief 检查类型的可读名称
			*/
			void check_type_name()
			{
				switch (station_type)
				{
				case zero_def::station_type::api:
					type_name_ = "API";
					break;
				case zero_def::station_type::route_api:
					type_name_ = "ROUTE_API";
					break;
				case zero_def::station_type::vote:
					type_name_ = "VOTE";
					break;
				case zero_def::station_type::notify:
					type_name_ = "PUB";
					break;
				case zero_def::station_type::dispatcher:
					type_name_ = "DISP";
					break;
				case zero_def::station_type::plan:
					type_name_ = "PLAN";
					break;
				case zero_def::station_type::proxy:
					type_name_ = "PROXY";
					break;
				case zero_def::station_type::queue:
					type_name_ = "QUEUE";
					break;
				case zero_def::station_type::trace:
					type_name_ = "TRACE";
					break;
				default:
					type_name_ = "ERR";
					break;
				}
			}

			bool is_general() const
			{
				return zero_def::station_type::is_general_station(station_type);
			}

			bool is_state(station_state worker_state) const
			{
				return station_state_ == worker_state;
			}

			bool is_run() const
			{
				return station_state_ >= station_state::start && station_state_ <= station_state::pause;
			}

			station_state get_state() const
			{
				return station_state_;
			}

			station_state config_state() const
			{
				return config_state_;
			}

			void config_state(station_state worker_state)
			{
				config_state_ = worker_state;
			}

			void set_state(station_state worker_state)
			{
				switch (station_state_)
				{
				case station_state::none:
					log("worker_state : none");
					break;
				case station_state::re_start:
					if (config_state_ == station_state::closed)
						return;
					log("worker_state : re_start");
					break;
				case station_state::start:
					if (config_state_ == station_state::closed)
						return;
					log("worker_state : start");
					break;
				case station_state::run:
					if (config_state_ == station_state::closed)
						return;
					log("worker_state : run");
					break;
				case station_state::pause:
					if (config_state_ == station_state::closed)
						return;
					log("worker_state : pause");
					break;
				case station_state::failed:
					log("worker_state : failed");
					break;
				case station_state::closing:
					log("worker_state : closing");
					break;
				case station_state::closed:
					log("worker_state : closed");
					break;
				case station_state::destroy:
					log("worker_state : destroy");
					break;
				case station_state::stop:
					log("worker_state : stop");
					break;
				case station_state::unknow:
					log("worker_state : unknow");
					break;
				default: ;
				}
				station_state_ = worker_state;
			}

			bool runtime_state(station_state worker_state)
			{
				if (station_state_ >= station_state::destroy)
					return false;
				set_state(worker_state);
				return true;
			}

			/**
			* \brief 开机日志
			*/
			void start()
			{
				start_log(station_state_ == station_state::re_start || station_state_ == station_state::failed
					          ? "restart"
					          : "start");
				runtime_state(station_state::start);
			}

			/**
			* \brief 开机失败日志
			*/
			void failed(const char* msg)
			{
				error("con`t launch", msg);
				runtime_state(station_state::failed);
			}

			/**
			* \brief 开机正常日志
			*/
			void runing()
			{
				log("runing");
				runtime_state(station_state::run);
			}

			/**
			* \brief 正在关机日志
			*/
			void closing()
			{
				log("closing...");
				runtime_state(station_state::closing);
			}

			/**
			* \brief 重启日志
			*/
			void restart()
			{
				start_log("restart");
				runtime_state(station_state::re_start);
			}

			/**
			* \brief 关机日志
			*/
			void closed()
			{
				runtime_state(station_state::closed);
				log("closed");
			}

			/**
			* \brief 日志
			*/
			void start_log(const char* worker_state)
			{
				log_msg3("[%s] > %s \n%s", station_name.c_str(), worker_state, to_info_json().c_str());
			}

			/**
			* \brief 日志
			*/
			void log(const char* msg, bool works = false) const
			{
				if (works)
				log_msg3("[%s] > %s (ready_works:%d)", station_name.c_str(), msg, ready_works_)
				else
				log_msg2("[%s] > %s", station_name.c_str(), msg)
			}

			/**
			* \brief 日志
			*/
			void log(const char* title, const char* msg, bool works = false) const
			{
				if (works)
				log_msg4("[%s] > %s > %s (ready_works:%d)", station_name.c_str(), title, msg, ready_works_)
				else
				log_msg3("[%s] > %s > %s", station_name.c_str(), title, msg)
			}

			/**
			* \brief 日志
			*/
			void flow_log(int step, const char* title, const char* msg = nullptr) const
			{
				if (msg != nullptr)
				log_msg4("[%s] step %d. > %s  > %s", station_name.c_str(), step, title, msg)
				else
				log_msg3("[%s] step %d. > %s", station_name.c_str(), step, title)
			}

			/**
			* \brief 日志
			*/
			void error(const char* title, const char* msg, bool works = false) const
			{
				if (works)
				log_error4("[%s] > %s > %s (ready_works:%d)", station_name.c_str(), title, msg, ready_works_)
				else
				log_error3("[%s] > %s > %s", station_name.c_str(), title, msg)
			}

			/**
			* \brief 日志
			*/
			void error(const char* title, const char* msg, const char* msg2) const
			{
				log_error4("[%s] > %s > %s  > %s", station_name.c_str(), title, msg, msg2)
			}

			/**
			* \brief 日志
			*/
			void debug(const char* title, const char* msg, bool works = false) const
			{
				if (works)
				log_msg4("[%s] > %s > %s (ready_works:%d)", station_name.c_str(), title, msg, ready_works_)
				else
				log_msg3("[%s] > %s > %s", station_name.c_str(), title, msg)
			}

			/**
			* \brief 日志
			*/
			void error(const char* title, const int64 id) const
			{
				log_error3("[%s] > %s > %lld", station_name.c_str(), title, id);
			}

			/**
			* \brief 写入基本信息JSON
			*/
			acl::string to_info_json()
			{
				return to_json(1);
			}

			/**
			* \brief 写入状态JSON
			*/
			acl::string to_status_json()
			{
				return to_json(2);
			}

			/**
			* \brief 写入全部JSON
			*/
			acl::string to_full_json()
			{
				return to_json(0);
			}

		private:
			/**
			* \brief 写入JSON
			* \param type 记录类型 0 全量 1 状态信息 2 基本信息
			*/
			acl::string to_json(int type);
		};
	}
}
#endif //!_ZERO_CONFIG_H_
