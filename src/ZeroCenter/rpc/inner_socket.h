#pragma once
#ifndef _IPC_REQUEST_SOCKET_H
#define _IPC_REQUEST_SOCKET_H
#include "zero_default.h"
#include "zero_config.h"
#include "zmq_extend.h"

namespace agebull
{
	namespace zmq_net
	{
		/**
		* \brief 连接的SOCKET简单封装
		*/
		class inner_socket
		{
			zmq_socket_state state_;
			char station_[MAX_PATH];
			char caller_[MAX_PATH];
		public:
			ZMQ_HANDLE socket_;

			/**
			* \brief 构造
			* \param caller
			* \param station
			*/
			inner_socket(const char* caller, const char* station) : state_(zmq_socket_state::Succeed)
			{
				strcpy(station_, station);
				strcpy(caller_, caller);
				socket_ = socket_ex::create_req_socket_inproc(station, caller_);
			}


			/**
			* \brief 析构
			*/
			~inner_socket()
			{
				make_inproc_address(address, station_);
				zmq_disconnect(socket_, address);
				zmq_close(socket_);
			}

			/**
			* \brief 状态
			*/
			zmq_socket_state get_state() const
			{
				return state_;
			}

			/**
			* \brief 接收
			*/
			zmq_socket_state recv(shared_char& data, int flag = 0) const
			{
				return socket_ex::recv(socket_, data, flag);
			}

			/**
			* \brief 接收
			*/
			zmq_socket_state recv(vector<shared_char>& ls, int flag = 0) const
			{
				return socket_ex::recv(socket_, ls, flag);
			}

			/**
			* \brief 发送
			*/
			zmq_socket_state send_late(const char* string) const
			{
				return socket_ex::send_late(socket_, string);
			}

			/**
			* \brief 发送帧
			*/
			zmq_socket_state send_more(const char* string) const
			{
				return socket_ex::send_more(socket_, string);
			}

			/**
			* \brief 发送
			*/
			zmq_socket_state send(vector<shared_char>& ls) const
			{
				return socket_ex::send(socket_, ls);
			}

			/**
			* \brief 发送
			*/
			zmq_socket_state send(vector<string>& ls) const
			{
				return socket_ex::send(socket_, ls);
			}

			/**
			* \brief 进行一次请求
			* @return 如果返回为false,请检查state.
			*/
			bool request(vector<shared_char>& arguments, vector<shared_char>& result, int retry = 3)
			{
#ifdef TIMER
				boost::posix_time::ptime start = boost::posix_time::microsec_clock::universal_time();
#endif
				state_ = send(arguments);

#ifdef TIMER
				log_debug2(DEBUG_TIMER, 3, "[%s] send-%d-ns", station_, (boost::posix_time::microsec_clock::universal_time() - start).total_microseconds());
#endif

				if (state_ != zmq_socket_state::Succeed)
					return false;

#ifdef TIMER
				start = boost::posix_time::microsec_clock::universal_time();
#endif
				int cnt = 0;
				do
				{
					state_ = recv(result);
					if (state_ == zmq_socket_state::Succeed)
					{
						break;
					}
				}
				while (state_ == zmq_socket_state::TimedOut && ++cnt < retry);

#ifdef TIMER
				log_debug2(DEBUG_TIMER, 3, "[%s] recv-%d-ns", station_, (boost::posix_time::microsec_clock::universal_time() - start).total_microseconds());
#endif
				return state_ == zmq_socket_state::Succeed;
			}

			/**
			* \brief 进行一次请求
			* @return 如果返回为false,请检查state.
			*/
			bool request(vector<string>& arguments, vector<shared_char>& result, int retry = 3)
			{
#ifdef TIMER
				boost::posix_time::ptime start = boost::posix_time::microsec_clock::universal_time();
#endif
				state_ = send(arguments);

#ifdef TIMER
				log_debug2(DEBUG_TIMER, 3, "[%s] send-%d-ns", station_, (boost::posix_time::microsec_clock::universal_time() - start).total_microseconds());
#endif

				if (state_ != zmq_socket_state::Succeed)
					return false;

#ifdef TIMER
				start = boost::posix_time::microsec_clock::universal_time();
#endif
				int cnt = 0;
				do
				{
					state_ = recv(result);
					if (state_ == zmq_socket_state::Succeed)
					{
						break;
					}
				}
				while (state_ == zmq_socket_state::TimedOut && ++cnt < retry);

#ifdef TIMER
				log_debug2(DEBUG_TIMER, 3, "[%s] recv-%d-ns", station_, (boost::posix_time::microsec_clock::universal_time() - start).total_microseconds());
#endif
				return state_ == zmq_socket_state::Succeed;
			}
		};
	}
}
#endif //!_IPC_REQUEST_SOCKET_H
