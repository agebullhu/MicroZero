#pragma once
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
		class sharp_char
		{
			int* count_;
			char* buffer_;
			size_t size_;
		public:
			sharp_char() : count_(nullptr), buffer_(nullptr), size_(0)
			{
			}
			sharp_char(const sharp_char& fri) : count_(fri.count_), buffer_(fri.buffer_), size_(fri.size_)
			{
				if (count_ != nullptr)
					*count_ += 1;
			}

			sharp_char(char* buffer, int len) : count_(new int()), buffer_(buffer), size_(len)
			{
				*count_ = 1;
			}

			sharp_char(const char* buffer)
			{
				if (buffer == nullptr)
				{
					buffer_ = nullptr;
					count_ = nullptr;
					size_ = 0;
					return;
				}
				size_ = strlen(buffer);
				buffer_ = new char[size_ + 1];
				memcpy(buffer_, buffer, size_);
				buffer_[size_] = 0;
				count_ = new int();
				*count_ = 1;
			}

			sharp_char(zmq_msg_t& msg)
			{
				size_ = zmq_msg_size(&msg);
				if (size_ == 0)
				{
					buffer_ = nullptr;
					count_ = nullptr;
					return;
				}
				buffer_ = new char[size_ + 1];
				count_ = new int();
				*count_ = 1;
				memcpy(buffer_, zmq_msg_data(&msg), size_);
				buffer_[size_] = 0;
			}

			sharp_char(const std::string& msg)
			{
				size_ = msg.length();
				if (size_ == 0)
				{
					buffer_ = nullptr;
					count_ = nullptr;
					return;
				}
				buffer_ = new char[size_ + 1];
				count_ = new int();
				*count_ = 1;
				memcpy(buffer_, msg.c_str(), size_);
				buffer_[size_] = 0;
			}

			explicit sharp_char(size_t size)
			{
				size_ = size;
				if (size_ == 0)
				{
					buffer_ = nullptr;
					count_ = nullptr;
					return;
				}
				buffer_ = new char[size_ + 1];
				count_ = new int();
				*count_ = 1;
				memset(buffer_, 0, size_ + 1);
			}
			sharp_char(const acl::string& msg)
			{
				size_ = msg.length();
				if (size_ == 0)
				{
					buffer_ = nullptr;
					count_ = nullptr;
					return;
				}
				buffer_ = new char[size_ + 1];
				count_ = new int();
				*count_ = 1;
				memcpy(buffer_, msg.c_str(), size_);
				buffer_[size_] = 0;
			}
			~sharp_char()
			{
				free();
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
			sharp_char& alloc(size_t size)
			{
				free();
				buffer_ = new char[size + 1];
				memset(buffer_, 0, size + 1);
				count_ = new int();
				*count_ = 1;
				size_ = size;
				return *this;
			}
			sharp_char& operator = (zmq_msg_t& msg)
			{
				free();
				size_ = zmq_msg_size(&msg);
				if (size_ == 0)
					return *this;
				alloc(size_);
				memcpy(buffer_, zmq_msg_data(&msg), size_);
				buffer_[size_] = 0;
				count_ = new int();
				*count_ = 1;
				return *this;
			}
			sharp_char& operator = (std::string& msg)
			{
				free();
				size_ = msg.length();
				if (size_ == 0)
					return *this;
				alloc(size_);
				memcpy(buffer_, msg.c_str(), size_);
				buffer_[size_] = 0;
				count_ = new int();
				*count_ = 1;
				return *this;
			}
			sharp_char& operator = (acl::string& msg)
			{
				free();
				size_ = msg.length();
				if (size_ == 0)
					return *this;
				alloc(size_);
				memcpy(buffer_, msg.c_str(), size_);
				buffer_[size_] = 0;
				count_ = new int();
				*count_ = 1;
				return *this;
			}
			sharp_char& operator = (const char* msg)
			{
				free();
				if (msg == nullptr)
					return *this;
				size_ = strlen(msg);
				if (size_ == 0)
					return *this;
				alloc(size_);
				memcpy(buffer_, msg, size_);
				buffer_[size_] = 0;
				count_ = new int();
				*count_ = 1;
				return *this;
			}
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
			}
			sharp_char& binding(char* fri, size_t len)
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
			sharp_char& swap(sharp_char& fri) noexcept
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
				return *this;
			}
			sharp_char& operator = (const sharp_char& fri)
			{
				free();
				if (fri.buffer_ != nullptr)
				{
					buffer_ = fri.buffer_;
					size_ = fri.size_;
					count_ = fri.count_;
					*count_ += 1;
				}
				return *this;
			}

			sharp_char& operator = (char* fri)
			{
				binding(fri, fri == nullptr ? 0 : strlen(fri));
				return *this;
			}
			char* operator*() const
			{
				return buffer_;
			}
			operator std::string() const
			{
				return std::string(size_ == 0 || count_ == nullptr || buffer_ == nullptr ? "" : buffer_);
			}
			operator acl::string() const
			{
				return acl::string(size_ == 0 || count_ == nullptr || buffer_ == nullptr ? "" : buffer_);
			}
		};
	}
}
#endif
