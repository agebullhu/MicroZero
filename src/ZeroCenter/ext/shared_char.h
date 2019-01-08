#pragma once
#ifndef AGEBULL_SHRRP_CHAR_H_
#define AGEBULL_SHRRP_CHAR_H_
#include "../rpc/zero_default.h"
#include <acl/acl_cpp/stdlib/string.hpp>
#include <zeromq/zmq.h>

namespace agebull
{
	namespace zero_net
	{
		
		/**
		 * \brief 字节智能指针
		 */
		class shared_char
		{
		protected:
			int* count_;
			uchar* buffer_;
			size_t size_;
			size_t alloc_size_;
			/**
			* \bref 是否二进制,0 不确定,1 文本,2 二进制
			*/
			int is_binary_;
			/**
			* \bref 是否固定值
			*/
			bool is_const_;
		public:
			static int using_count_;
			/**
			 * \bref 构造
			 */
			shared_char() : count_(nullptr), buffer_(nullptr), size_(0), alloc_size_(0), is_binary_(0), is_const_(false)
			{
			}

			/**
			 * \bref 构造
			 */
			shared_char(const shared_char& fri) : count_(fri.count_), buffer_(fri.buffer_), size_(fri.size_),
				alloc_size_(fri.alloc_size_), is_binary_(fri.is_binary_), is_const_(fri.is_const_)
			{
				if (count_ != nullptr)
					*count_ += 1;
			}

			/**
			 * \bref 构造
			 */
			shared_char(char* buffer, size_t len) : shared_char()
			{
				copy_(len, buffer);
			}

			/**
			 * \bref 构造
			 */
			shared_char(uchar* buffer, size_t len) : shared_char()
			{
				copy_(len, buffer);
			}

			/**
			 * \bref 构造
			 */
			shared_char(const unsigned char* buffer)
			{
				if (buffer == nullptr)
				{
					set_empty();
					return;
				}
				size_ = strlen(reinterpret_cast<const char*>(buffer));
				if (size_ == 0)
				{
					set_empty();
					return;
				}
				copy_(size_, buffer);
				is_binary_ = 1;
			}

			/**
			 * \bref 构造
			 */
			shared_char(const char* buffer)
			{
				if (buffer == nullptr)
				{
					set_empty();
					return;
				}
				size_ = strlen(buffer);
				if (size_ == 0)
				{
					set_empty();
					return;
				}
				copy_(size_, buffer);
				is_binary_ = 1;
			}

			/**
			 * \bref 构造
			 */
			shared_char(zmq_msg_t& msg)
			{
				size_ = zmq_msg_size(&msg);
				if (size_ == 0)
				{
					set_empty();
					return;
				}
				copy_(size_, zmq_msg_data(&msg));
			}

			/**
			 * \bref 构造
			 */
			shared_char(const std::string& msg)
			{
				size_ = msg.length();
				if (size_ == 0)
				{
					set_empty();
					return;
				}
				copy_(size_, msg.c_str());
			}

			/**
			 * \bref 用指定大小构造(注意不是等于)
			**/
			explicit shared_char(size_t size) : buffer_(nullptr), is_binary_(0), is_const_(false)
			{
				size_ = size;
				if (size_ == 0)
				{
					set_empty();
					return;
				}
				alloc_(size_);
			}

			/**
			 * \bref 构造
			 */
			shared_char(const acl::string& msg) : is_binary_(1), is_const_(false)
			{
				size_ = msg.length();
				if (size_ == 0)
				{
					set_empty();
					return;
				}
				copy_(size_, msg.c_str());
			}

			/**
			 * \bref 析构
			 */
			~shared_char()
			{
				free();
			}

			/**
			 * \bref 分配新内存
			 */
			void alloc(size_t size)
			{
				free();
				alloc_(size);
			}
			/**
			 * \bref 释放内存
			 */
			void free()
			{
				free_();
				set_empty();
			}
		private:
			/**
			 * \bref 释放内存
			 */
			void free_();

			void alloc_(size_t size);

			void copy_(size_t size, const void* src);


			void set_empty()
			{
				size_ = 0;
				buffer_ = nullptr;
				count_ = nullptr;
				alloc_size_ = 0;
				is_binary_ = 0;
				is_const_ = false;
			}
		public:
			/**
			* \brief 绑定游离内存
			*/
			shared_char& binding(char* fri, size_t len)
			{
				free();
				size_ = len;
				alloc_size_ = len;
				buffer_ = reinterpret_cast<uchar*>(fri);
				count_ = new int();
				*count_ = 1;
				return *this;
			}

			/**
			* \brief 交换
			*/
			shared_char& swap(shared_char& fri) noexcept
			{
				{
					uchar* tmp = buffer_;
					buffer_ = fri.buffer_;
					fri.buffer_ = tmp;
				}
				{
					int* tmp = count_;
					count_ = fri.count_;
					fri.count_ = tmp;
				}
				{
					const size_t tmp = size_;
					size_ = fri.size_;
					fri.size_ = tmp;
				}
				{
					const size_t tmp = alloc_size_;
					alloc_size_ = fri.alloc_size_;
					fri.alloc_size_ = tmp;
				}
				{
					const int tmp = is_binary_;
					is_binary_ = fri.is_binary_;
					fri.is_binary_ = tmp;
				}
				return *this;
			}


			int user_count() const
			{
				return count_ == nullptr ? 0 : *count_;
			}

			size_t size() const
			{
				return size_;
			}
			size_t alloc_size() const
			{
				return alloc_size_;
			}

			bool empty() const
			{
				return buffer_ == nullptr || size_ == 0;
			}

			uchar operator[](size_t idx) const
			{
				return buffer_ == nullptr || idx >= size_ ? 0 : buffer_[idx];
			}

			shared_char& operator =(zmq_msg_t& msg)
			{
				free();
				size_ = zmq_msg_size(&msg);
				if (size_ == 0)
					return *this;
				copy_(size_, zmq_msg_data(&msg));
				return *this;
			}

			shared_char& operator =(const std::string& msg)
			{
				free();
				is_binary_ = 1;
				size_ = msg.length();
				if (size_ == 0)
					return *this;
				copy_(size_, msg.c_str());
				return *this;
			}

			shared_char& operator =(const char* msg)
			{
				if (buffer_ == (uchar*)msg)
					return *this;
				free();
				size_ = strlen(msg);
				if (size_ == 0)
					return *this;
				copy_(size_, msg);
				is_binary_ = 1;
				return *this;
			}

			shared_char& set_time(const time_t value)
			{
				if (size_ < 16)
				{
					alloc(16);
				}
				is_binary_ = 1;
				sprintf(reinterpret_cast<char*>(buffer_), "%d", value);
				return *this;
			}

			shared_char& set_int(const int value)
			{
				if (size_ < 16)
				{
					alloc(16);
				}
				is_binary_ = 1;
				sprintf(reinterpret_cast<char*>(buffer_), "%d", value);
				return *this;
			}
			shared_char& set_intx(const int value)
			{
				if (size_ < 16)
				{
					alloc(16);
				}
				is_binary_ = 1;
				sprintf(reinterpret_cast<char*>(buffer_), "%x", value);
				return *this;
			}
			shared_char& set_int64(const int64 value)
			{
				if (size_ < 16)
				{
					alloc(16);
				}
				is_binary_ = 1;
				sprintf(reinterpret_cast<char*>(buffer_), "%lld", value);
				return *this;
			}

			shared_char& set_int64x(const int64 value)
			{
				if (size_ < 16)
				{
					alloc(16);
				}
				is_binary_ = 1;
				sprintf(reinterpret_cast<char*>(buffer_), "%llx", value);
				return *this;
			}

			shared_char& operator =(const acl::string& msg)
			{
				free();
				is_binary_ = 1;
				size_ = msg.length();
				if (size_ == 0)
					return *this;
				copy_(size_, msg.c_str());
				return *this;
			}

			shared_char& operator =(const shared_char& fri)
			{
				if (&fri == this)
					return *this;
				if (buffer_ == fri.buffer_)
					return *this;
				free();
				if (fri.buffer_ != nullptr)
				{
					buffer_ = fri.buffer_;
					size_ = fri.size_;
					count_ = fri.count_;
					*count_ += 1;
					is_binary_ = fri.is_binary_;
					alloc_size_ = fri.alloc_size_;
				}
				return *this;
			}

			bool is_string() const
			{
				return is_binary_ == 1;
			}

			bool is_const() const
			{
				return is_const_;
			}

			const char* operator*() const
			{
				return buffer_ == nullptr ? "" : reinterpret_cast<char*>(buffer_);
			}

			uchar* get_buffer() const
			{
				return buffer_;
			}

			char* c_str() const
			{
				return reinterpret_cast<char*>(buffer_);
			}

			operator std::string() const
			{
				if (empty())
				{
					return std::string();
				}
				return std::string(reinterpret_cast<char*>(buffer_));
			}

			operator acl::string() const
			{
				if (empty())
				{
					return acl::string();
				}
				if (is_binary_ == 1)
					return acl::string(reinterpret_cast<char*>(buffer_));
				var val = acl::string(reinterpret_cast<char*>(buffer_));
				val.set_bin(true);
				return val;
			}


			//对说明帧的支持
			shared_char& alloc_desc(size_t size, uchar state = zero_def::command::none)
			{
				free();
				alloc_(size + 2);
				is_binary_ = 2;
				buffer_[1] = state;
				return *this;
			}

			template <size_t TSize>
			shared_char& alloc_desc(char(&frames)[TSize])
			{
				free();
				alloc_(TSize + 2);
				is_binary_ = 2;
				buffer_[0] = static_cast<char>(TSize);
				buffer_[1] = zero_def::command::none;
				memcpy(buffer_ + 2, frames, TSize);
				return *this;
			}

			shared_char& alloc_frame_desc(uchar state, char frame)
			{
				free();
				alloc_(4);
				is_binary_ = 2;
				buffer_[0] = static_cast<char>(1);
				buffer_[1] = state;
				buffer_[2] = frame;
				buffer_[3] = state;
				return *this;
			}

			shared_char& alloc_frame_desc(uchar state, char frame1, char frame2)
			{
				free();
				alloc_(5);
				is_binary_ = 2;
				buffer_[0] = static_cast<char>(2);
				buffer_[1] = state;
				buffer_[2] = frame1;
				buffer_[3] = frame2;
				return *this;
			}

			shared_char& alloc_frame_desc(uchar state, char frame1, char frame2, char frame3)
			{
				free();
				alloc_(6);
				is_binary_ = 2;
				buffer_[0] = static_cast<char>(3);
				buffer_[1] = state;
				buffer_[2] = frame1;
				buffer_[3] = frame2;
				buffer_[4] = frame3;
				return *this;
			}

			shared_char& alloc_frame_desc(uchar state, char frame1, char frame2, char frame3, char frame4)
			{
				free();
				alloc_(7);
				is_binary_ = 2;
				buffer_[0] = static_cast<char>(4);
				buffer_[1] = state;
				buffer_[2] = frame1;
				buffer_[3] = frame2;
				buffer_[4] = frame3;
				buffer_[5] = frame4;
				return *this;
			}

			shared_char& alloc_frame_desc(uchar state, char frame1, char frame2, char frame3, char frame4, char frame5)
			{
				free();
				alloc_(8);
				is_binary_ = 2;
				buffer_[0] = static_cast<char>(5);
				buffer_[1] = state;
				buffer_[2] = frame1;
				buffer_[3] = frame2;
				buffer_[4] = frame3;
				buffer_[5] = frame4;
				buffer_[6] = frame5;
				return *this;
			}

			size_t desc_size() const
			{
				return buffer_ == nullptr ? 0 : buffer_[0] + 2;
			}

			size_t frame_size() const
			{
				return buffer_ == nullptr ? 0 : static_cast<size_t>(buffer_[0]);
			}

			void frame_size(size_t size)
			{
				buffer_[0] = static_cast<char>(size);
			}

			void state(uchar s) const
			{
				memcpy(buffer_ + 1, &s, 1);
			}

			uchar state() const
			{
				return size_ == 0 ? 0 : *reinterpret_cast<uchar*>(buffer_ + 1);
			}

			uchar command() const
			{
				return size_ == 0 ? 0 : *reinterpret_cast<uchar*>(buffer_ + 1);
			}

			uchar tag() const
			{
				return size_ == 0 ? 0 : static_cast<uchar>(buffer_[size_ - 1]);
			}
			void tag(uchar t)
			{
				frame_type(buffer_[0], t);
			}

			char frame_type(size_t index) const
			{
				return alloc_size_ < index ? 0 : buffer_[index + 2];
			}

			void append_frame(uchar type)
			{
				frame_type(buffer_[0], type);
				buffer_[0] = static_cast<char>(buffer_[0] + 1);
			}
			bool hase_frame(uchar type)const
			{
				for (size_t idx = 2; idx < static_cast<size_t>(buffer_[0] + 2); idx++)
				{
					if (buffer_[idx] == type)
						return true;
				}
				return false;
			}

			void frame_type(size_t index, uchar type);

			size_t check_size()
			{
				size_t max = buffer_[0] + 2;
				if (max > size_ && max < (size_ + 4))
					size_ = max;
				return size_;
			}
		};
	}
}
#endif
