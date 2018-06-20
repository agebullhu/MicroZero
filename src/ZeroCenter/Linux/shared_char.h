#pragma once
#ifndef _AGEBULL_SHRRP_CHAR_H_
#include "net/net_default.h"
#include <acl/acl_cpp/stdlib/string.hpp>
#include <zeromq/zmq.h>

namespace agebull
{
	namespace zmq_net
	{
		/**
		 * \brief 字节智能指针
		 */
		class shared_char
		{
			int* count_;
			char* buffer_;
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

			shared_char() : count_(nullptr), buffer_(nullptr), size_(0), alloc_size_(0), is_binary_(0), is_const_(false)
			{
			}

			shared_char(const shared_char& fri) : count_(fri.count_), buffer_(fri.buffer_), size_(fri.size_),
			                                      alloc_size_(fri.alloc_size_), is_binary_(fri.is_binary_), is_const_(false)
			{
				if (count_ != nullptr)
					*count_ += 1;
			}

			shared_char(char* buffer, int len) : count_(new int()), buffer_(buffer), size_(len), alloc_size_(len),
			                                     is_binary_(2), is_const_(false)
			{
				*count_ = 1;
			}

			shared_char(const char* buffer)
			{
				size_ = strlen(buffer);
				if (buffer == nullptr || size_ == 0)
				{
					set_empty();
					return;
				}
				copy_(size_, buffer);
				is_binary_ = 1;
			}

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
			 *\bref 用指定大小构造(注意不是等于)
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

			~shared_char()
			{
				free();
			}

		private:
			void set_empty()
			{
				size_ = 0;
				buffer_ = nullptr;
				count_ = nullptr;
				alloc_size_ = 0;
				is_binary_ = 0;
			}

			void alloc(size_t size)
			{
				free();
				alloc_(size);
			}

			void free()
			{
				if (count_ == nullptr || size_ == 0)
					return;
				int cnt = --(*count_);
				if (cnt == 0)
				{
					delete count_;
					delete[] buffer_;
				}
				count_ = nullptr;
				buffer_ = nullptr;
				size_ = 0;
				is_binary_ = 0;
				alloc_size_ = 0;
				is_const_ = false;
			}

			void alloc_(size_t size)
			{
				size_ = size;
				alloc_size_ = size_ + 8;
				buffer_ = new char[alloc_size_];
				memset(buffer_, 0, alloc_size_);
				count_ = new int();
				*count_ = 1;
				is_const_ = false;
			}

			void copy_(size_t size, const void* src)
			{
				size_ = size;
				count_ = new int();
				*count_ = 1;
				is_const_ = false;
				alloc_size_ = size_ + 8;
				buffer_ = new char[alloc_size_];
				memcpy(buffer_, src, size);
				memset(buffer_ + size, 0, 4);
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
				buffer_ = fri;
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
					char* tmp = buffer_;
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

			char* get_buffer() const
			{
				return buffer_;
			}

			int user_count() const
			{
				return count_ == nullptr ? 0 : *count_;
			}

			size_t size() const
			{
				return size_;
			}

			bool empty() const
			{
				return buffer_ == nullptr || size_ == 0;
			}

			char operator[](size_t idx) const
			{
				return buffer_ == nullptr || idx >= size_ ? '\0' : buffer_[idx];
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
				sprintf(buffer_, "%d", value);
				return *this;
			}

			shared_char& set_int(const int value)
			{
				if (size_ < 16)
				{
					alloc(16);
				}
				is_binary_ = 1;
				sprintf(buffer_, "%d", value);
				return *this;
			}
			shared_char& set_intx(const int value)
			{
				if (size_ < 16)
				{
					alloc(16);
				}
				is_binary_ = 1;
				sprintf(buffer_, "%x", value);
				return *this;
			}
			shared_char& set_int64(const int64 value)
			{
				if (size_ < 16)
				{
					alloc(16);
				}
				is_binary_ = 1;
				sprintf(buffer_, "%lld", value);
				return *this;
			}

			shared_char& set_int64x(const int64 value)
			{
				if (size_ < 16)
				{
					alloc(16);
				}
				is_binary_ = 1;
				sprintf(buffer_, "%llx", value);
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
				return buffer_ == nullptr ? "" : buffer_;
			}

			operator std::string() const
			{
				if (empty())
				{
					return std::string();
				}
				return std::string(buffer_);
			}

			operator acl::string() const
			{
				if (empty())
				{
					return acl::string();
				}
				if (is_binary_ == 1)
					return acl::string(buffer_);
				var val = acl::string(buffer_);
				val.set_bin(true);
				return val;
			}

			shared_char& alloc_frame(size_t size, char status = ZERO_BYTE_COMMAND_NONE)
			{
				free();
				alloc_(size + 2);
				is_binary_ = 2;
				buffer_[0] = static_cast<char>(size);
				buffer_[1] = status;
				return *this;
			}

			template <size_t TSize>
			shared_char& alloc_frame(char (&frames)[TSize])
			{
				return alloc_frame(ZERO_BYTE_COMMAND_NONE, frames);
			}

			shared_char& alloc_frame_1(char status, char frame)
			{
				free();
				alloc_(4);
				is_binary_ = 2;
				buffer_[0] = static_cast<char>(1);
				buffer_[1] = status;
				buffer_[2] = frame;
				return *this;
			}

			shared_char& alloc_frame_2(char status, char frame1, char frame2)
			{
				free();
				alloc_(5);
				is_binary_ = 2;
				buffer_[0] = static_cast<char>(2);
				buffer_[1] = status;
				buffer_[2] = frame1;
				buffer_[3] = frame2;
				return *this;
			}

			shared_char& alloc_frame_3(char status, char frame1, char frame2, char frame3)
			{
				free();
				alloc_(6);
				is_binary_ = 2;
				buffer_[0] = static_cast<char>(3);
				buffer_[1] = status;
				buffer_[2] = frame1;
				buffer_[3] = frame2;
				buffer_[4] = frame3;
				return *this;
			}

			shared_char& alloc_frame_4(char status, char frame1, char frame2, char frame3, char frame4)
			{
				free();
				alloc_(7);
				is_binary_ = 2;
				buffer_[0] = static_cast<char>(4);
				buffer_[1] = status;
				buffer_[2] = frame1;
				buffer_[3] = frame2;
				buffer_[4] = frame3;
				buffer_[5] = frame4;
				return *this;
			}

			template <size_t TSize>
			shared_char& alloc_frame(char status, char (&frames)[TSize])
			{
				free();
				alloc_(TSize + 2);
				is_binary_ = 2;
				buffer_[0] = static_cast<char>(TSize);
				buffer_[1] = status;
				memcpy(buffer_ + 2, frames, TSize);
				return *this;
			}

			size_t desc_size() const
			{
				return buffer_[0] + 2;
			}

			size_t frame_size() const
			{
				return buffer_ == nullptr ? 0 : static_cast<size_t>(buffer_[0]);
			}

			void frame_size(size_t size)
			{
				if (size_ == 0)
					alloc_frame(size);
				else
					buffer_[0] = static_cast<char>(size);
			}

			void status(char status)
			{
				if (size_ == 0)
					alloc_frame(10, status);
				else
					buffer_[1] = status;
			}

			char status() const
			{
				return buffer_[1];
			}

			char frame_type(size_t index) const
			{
				return alloc_size_ < index ? 0 : buffer_[index + 2];
			}

			void append_frame(char type)
			{
				frame_type(buffer_[0], type);
				buffer_[0] = static_cast<char>(buffer_[0] + 1);
			}

			void frame_type(size_t index, char type)
			{
				index += 2;
				if (alloc_size_ > index)
				{
					if (index >= size_)
						size_ = index + 1;
				}
				else
				{
					char* old = buffer_;
					buffer_ = new char[index + 8];
					memcpy(buffer_, old, alloc_size_);
					memset(buffer_ + alloc_size_, 0, index + 8 - alloc_size_);
					delete[] old;
					size_ = index + 4;
					alloc_size_ = index + 8;
				}
				buffer_[index] = type;
			}

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
