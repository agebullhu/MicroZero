#include "../stdafx.h"
#include "ZeroStation.h"

#define port_redis_key "net:port:next"

namespace agebull
{
	namespace zmq_net
	{

		/**
		* \brief 初始化
		*/
		bool zero_station::do_initialize()
		{
			if (this->zmq_state_ == zmq_socket_state::Succeed)
				log_msg3("Station:%s(%d | %d) start...", this->station_name_.c_str(), this->request_port_, this->response_port_)
			else
				log_msg3("Station:%s(%d | %d) restart...", this->station_name_.c_str(), this->request_port_, this->response_port_)
			if (!this->initialize())
			{
				log_msg3("Station:%s(%d | %d) conn`t start", this->station_name_.c_str(), this->request_port_, this->response_port_)
				return false;
			}
			log_msg3("Station:%s(%d | %d) ready", this->station_name_.c_str(), this->request_port_, this->response_port_);
			return true;
		}



		/**
		* \brief 初始化
		*/

		bool zero_station::initialize()
		{
			station_state_ = station_state::Start;
			zmq_state_ = zmq_socket_state::Succeed;
			poll_count_ = 0;

			if (request_zmq_type_ >= 0 && request_zmq_type_ != ZMQ_PUB)
			{
				poll_count_ += 2;
			}
			if (response_zmq_type_ >= 0 && response_zmq_type_ != ZMQ_PUB)
			{
				poll_count_++;
			}
			if (heart_zmq_type_ >= 0 && heart_zmq_type_ != ZMQ_PUB)
			{
				poll_count_++;
			}
			poll_items_ = new zmq_pollitem_t[poll_count_];
			memset(poll_items_, 0, sizeof(zmq_pollitem_t) * poll_count_);
			int cnt = 0;

			if (request_zmq_type_ >= 0)
			{
				request_scoket_tcp_ = create_res_socket_tcp(station_name_.c_str(), request_zmq_type_, request_port_);
				if (request_scoket_tcp_ == nullptr)
				{
					station_state_ = station_state::Failed;
					log_error2("%s initialize error(out) %s", station_name_.c_str(), zmq_strerror(zmq_errno()));
					set_command_thread_bad(station_name_.c_str());
					return false;
				}
				request_socket_ipc_ = create_res_socket_ipc(station_name_.c_str(), request_zmq_type_);
				if (request_socket_ipc_ == nullptr)
				{
					station_state_ = station_state::Failed;
					log_error2("%s initialize error(ipc) %s", station_name_.c_str(), zmq_strerror(zmq_errno()));
					set_command_thread_bad(station_name_.c_str());
					return false;
				}
				if (request_zmq_type_ != ZMQ_PUB)
				{
					poll_items_[cnt++] = { request_scoket_tcp_, 0, ZMQ_POLLIN, 0 };
					poll_items_[cnt++] = { request_socket_ipc_, 0, ZMQ_POLLIN, 0 };
				}
			}
			if (response_zmq_type_ >= 0)
			{
				response_socket_tcp_ = create_res_socket_tcp(station_name_.c_str(), response_zmq_type_, response_port_);
				if (response_socket_tcp_ == nullptr)
				{
					station_state_ = station_state::Failed;
					log_error2("%s initialize error(inner) %s", station_name_.c_str(), zmq_strerror(zmq_errno()));
					set_command_thread_bad(station_name_.c_str());
					return false;
				}
				if (response_zmq_type_ != ZMQ_PUB)
					poll_items_[cnt++] = { response_socket_tcp_, 0, ZMQ_POLLIN, 0 };
			}
			if (heart_zmq_type_ >= 0)
			{
				heart_socket_tcp_ = create_res_socket_tcp(station_name_.c_str(), heart_zmq_type_, heart_port_);
				if (heart_socket_tcp_ == nullptr)
				{
					station_state_ = station_state::Failed;
					log_error2("%s initialize error(heart) %s", station_name_.c_str(), zmq_strerror(zmq_errno()));
					set_command_thread_bad(station_name_.c_str());
					return false;
				}
				if (heart_zmq_type_ != ZMQ_PUB)
					poll_items_[cnt] = { heart_socket_tcp_, 0, ZMQ_POLLIN, 0 };
			}
			station_state_ = station_state::Run;
			return true;
		}

		/**
		* \brief 析构
		*/

		bool zero_station::destruct()
		{
			if (poll_items_ == nullptr)
				return true;
			delete[]poll_items_;
			poll_items_ = nullptr;
			station_state_ = station_state::Closing;
			if (request_scoket_tcp_ != nullptr)
			{
				close_res_socket(request_scoket_tcp_, get_out_address().c_str());
			}
			if (response_socket_tcp_ != nullptr)
			{
				close_res_socket(response_socket_tcp_, get_inner_address().c_str());
			}
			if (heart_socket_tcp_ != nullptr)
			{
				close_res_socket(heart_socket_tcp_, get_heart_address().c_str());
			}
			if (request_socket_ipc_ != nullptr)
			{
				char address[MAX_PATH];
				if (config::get_global_bool("use_ipc_protocol"))
					sprintf(address, "ipc://%s.ipc", station_name_.c_str());
				else
					sprintf(address, "inproc://%s", station_name_.c_str());
				close_res_socket(request_socket_ipc_, address);
			}
			//登记线程关闭
			set_command_thread_end(station_name_.c_str());
			station_state_ = station_state::Closed;
			return true;
		}
		/**
		* \brief
		* \return
		*/

		bool zero_station::poll()
		{
			set_command_thread_start(station_name_.c_str());
			//登记线程开始
			while (true)
			{
				if (!can_do())
				{
					zmq_state_ = zmq_socket_state::Intr;
					break;
				}
				if (check_pause())
					continue;
				if (!can_do())
				{
					zmq_state_ = zmq_socket_state::Intr;
					break;
				}

				const int state = zmq_poll(poll_items_, poll_count_, 1000);
				if (state == 0)//超时
					continue;
				if (station_state_ == station_state::Pause)
					continue;
				if (state < 0)
				{
					zmq_state_ = check_zmq_error();
					check_zmq_state()
				}
				for (int idx = 0; idx < poll_count_; idx++)
				{
					if (poll_items_[idx].revents & ZMQ_POLLIN)
					{
						if (poll_items_[idx].socket == request_scoket_tcp_)
							request(poll_items_[idx].socket,false);
						else if (poll_items_[idx].socket == request_socket_ipc_)
							request(poll_items_[idx].socket,true);
						else if (poll_items_[idx].socket == response_socket_tcp_)
							response();
						else if (poll_items_[idx].socket == heart_socket_tcp_)
							heartbeat();
						check_zmq_state()
					}
					else if (poll_items_[idx].revents & ZMQ_POLLERR)
					{
						cout << "error";
						//ERROR
					}
				}
			}
			return zmq_state_ < zmq_socket_state::Term && zmq_state_ > zmq_socket_state::Empty;
		}
		/**
		* \brief 暂停
		*/

		bool zero_station::pause(bool waiting)
		{
			if (station_state::Run != station_state_)
				return false;
			station_state_ = station_state::Pause;
			monitor_async(station_name_, "station_pause", "");
			return true;
		}

		/**
		* \brief 继续
		*/

		bool zero_station::resume(bool waiting)
		{
			if (station_state::Pause != station_state_)
				return false;
			station_state_ = station_state::Run;
			state_semaphore_.post();
			monitor_async(station_name_, "station_resume", "");
			return true;
		}

		/**
		* \brief 结束
		*/

		bool zero_station::close(bool waiting)
		{
			if (!can_do())
				return false;
			station_state_ = station_state::Closing;
			monitor_async(station_name_, "station_closing", "");
			while (waiting && station_state_ == station_state::Closing)
				thread_sleep(200);
			return true;
		}

	}
}