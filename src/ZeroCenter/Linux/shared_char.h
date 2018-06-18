#pragma once
#include "net/net_default.h"
#ifndef _AGEBULL_SHRRP_CHAR_H_
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
			/**
			 * \bref 是否二进制,0 不确定,1 文本,2 二进制
			 */
			int is_binary_;
		public:
			shared_char() : count_(nullptr), buffer_(nullptr), size_(0), is_binary_(0)
			{
			}
			shared_char(const shared_char& fri) : count_(fri.count_), buffer_(fri.buffer_), size_(fri.size_), is_binary_(fri.is_binary_)
			{
				if (count_ != nullptr)
					*count_ += 1;
			}

			shared_char(char* buffer, int len) : count_(new int()), buffer_(buffer), size_(len), is_binary_(2)
			{
				*count_ = 1;
			}

			shared_char(const char* buffer) : buffer_(nullptr), is_binary_(1)
			{
				size_ = strlen(buffer);
				if (buffer == nullptr || size_ == 0)
				{
					buffer_ = nullptr;
					count_ = nullptr;
					size_ = 0;
					return;
				}
				copy_(size_, buffer);
			}

			shared_char(zmq_msg_t& msg) : buffer_(nullptr), is_binary_(0)
			{
				size_ = zmq_msg_size(&msg);
				if (size_ == 0)
				{
					buffer_ = nullptr;
					count_ = nullptr;
					return;
				}
				copy_(size_, zmq_msg_data(&msg));
			}

			shared_char(const std::string& msg) : buffer_(nullptr), is_binary_(1)
			{
				size_ = msg.length();
				if (size_ == 0)
				{
					buffer_ = nullptr;
					count_ = nullptr;
					return;
				}
				copy_(size_, msg.c_str());
			}
			/**
			 *\bref 用指定大小构造(注意不是等于)
			**/
			explicit shared_char(size_t size) : buffer_(nullptr), is_binary_(0)
			{
				size_ = size;
				if (size_ == 0)
				{
					buffer_ = nullptr;
					count_ = nullptr;
					return;
				}
				alloc_(size_);
			}
			shared_char(const acl::string& msg) : buffer_(nullptr), is_binary_(1)
			{
				size_ = msg.length();
				if (size_ == 0)
				{
					count_ = nullptr;
					return;
				}
				copy_(size_, msg.c_str());
			}

			~shared_char()
			{
				free();
			}
			void alloc(size_t size)
			{
				free();
				alloc_(size);
			}
			char* alloc_desc(size_t size, char desc = ZERO_BYTE_COMMAND_NONE)
			{
				free();
				alloc_(size + 2);
				buffer_[0] = static_cast<char>(size);
				buffer_[1] = desc;
				is_binary_ = 2;
				return buffer_;
			}
			void set_desc_type(char desc) const
			{
				assert(size_ > 2);
				buffer_[1] = desc;
			}
			size_t check_desc_size()
			{
				size_t max = buffer_[0] + 2;
				if (max > size_ && max < (size_ + 4))
					size_ = max;
				return size_;
			}
		private:
			void free()
			{
				if (count_ == nullptr || size_ == 0)
					return;
				*count_ -= 1;
				if (*count_ == 0)
				{
					delete count_;
					delete[] buffer_;
				}
				count_ = nullptr;
				buffer_ = nullptr;
				size_ = 0;
				is_binary_ = 0;
			}
			void alloc_(size_t size)
			{
				size_ = size;
				buffer_ = new char[size_ + 4];
				memset(buffer_, 0, size_ + 4);
				count_ = new int();
				*count_ = 1;
			}
			void copy_(size_t size, const void* src)
			{
				size_ = size;
				count_ = new int();
				*count_ = 1;
				buffer_ = new char[size_ + 4];
				memcpy(buffer_, src, size);
				memset(buffer_ + size, 0, 4);
			}
		public:
			/**
			* \brief 绑定游离内存
			*/
			shared_char & binding(char* fri, size_t len)
			{
				free();
				size_ = len;
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
			const char &operator[](size_t idx) const
			{
				return buffer_ == nullptr || idx >= size_ ? '\0' : buffer_[idx];
			}
			shared_char& operator = (zmq_msg_t& msg)
			{
				free();
				size_ = zmq_msg_size(&msg);
				if (size_ == 0)
					return *this;
				copy_(size_, zmq_msg_data(&msg));
				return *this;
			}
			shared_char& operator = (const std::string& msg)
			{
				free();
				is_binary_ = 1;
				size_ = msg.length();
				if (size_ == 0)
					return *this;
				copy_(size_, msg.c_str());
				return *this;
			}
			shared_char& operator = (const char* msg)
			{
				free();
				is_binary_ = 1;
				size_ = strlen(msg);
				if (size_ == 0)
					return *this;
				copy_(size_, msg);
				return *this;
			}
			shared_char& set_value(const int value)
			{
				if (size_ < 16)
				{
					alloc(16);
				}
				is_binary_ = 1;
				sprintf(buffer_, "%d", value);
				return *this;
			}
			shared_char& set_value(const uint value)
			{
				if (size_ < 16)
				{
					alloc(16);
				}
				is_binary_ = 1;
				sprintf(buffer_, "%d", value);
				return *this;
			}
			shared_char& set_value(const int64_t value)
			{
				if (size_ < 16)
				{
					alloc(16);
				}
				is_binary_ = 1;
				sprintf(buffer_, "%lld", value);
				return *this;
			}
			shared_char& set_value(const uint64 value)
			{
				if (size_ < 16)
				{
					alloc(16);
				}
				is_binary_ = 1;
				sprintf(buffer_, "%lld", value);
				return *this;
			}
			shared_char& operator = (const acl::string& msg)
			{
				free();
				is_binary_ = 1;
				size_ = msg.length();
				if (size_ == 0)
					return *this;
				copy_(size_, msg.c_str());
				return *this;
			}
			shared_char& operator = (const shared_char& fri)
			{
				free();
				if (fri.buffer_ != nullptr)
				{
					buffer_ = fri.buffer_;
					size_ = fri.size_;
					count_ = fri.count_;
					*count_ += 1;
					is_binary_ = fri.is_binary_;
				}
				return *this;
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
			/**
			* \bref 右值重载
			*/
			char operator[](shared_char& sc, size_t idx, char value)
			{
				buffer_[idx] = value;
				return value;
			}
		};
	}
}
#endif
