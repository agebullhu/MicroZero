#pragma once
#ifndef _AGEBULL_SHRRP_CHAR_H_
#include <acl/acl_cpp/stdlib/string.hpp>
#include <zeromq/zmq.h>

namespace agebull
{
	namespace zmq_net
	{
		/**
		 * @brief 字节智能指针
		 */
		class sharp_char
		{
			int* _count;
			char* _buffer;
			size_t _size;
		public:
			sharp_char() : _count(nullptr), _buffer(nullptr), _size(0)
			{
			}
			sharp_char(const sharp_char& fri) : _count(fri._count), _buffer(fri._buffer), _size(fri._size)
			{
				if (_count != nullptr)
					*_count += 1;
			}
			sharp_char(char* buffer, int len) : _count(new int()), _buffer(buffer), _size(len)
			{
				*_count = 1;
			}
			sharp_char(const char* buffer)
			{
				if (buffer == nullptr)
				{
					_buffer = nullptr;
					_count = nullptr;
					_size = 0;
					return;
				}
				_size = strlen(buffer);
				_buffer = new char[_size + 1];
				memcpy(_buffer, buffer, _size);
				_buffer[_size] = 0;
				_count = new int();
				*_count = 1;
			}
			sharp_char(zmq_msg_t& msg)
			{
				_size = zmq_msg_size(&msg);
				if (_size == 0)
				{
					_buffer = nullptr;
					_count = nullptr;
					return;
				}
				_buffer = new char[_size + 1];
				_count = new int();
				*_count = 1;
				memcpy(_buffer, zmq_msg_data(&msg), _size);
				_buffer[_size] = 0;
			}
			sharp_char(std::string& msg)
			{
				_size = msg.length();
				if (_size == 0)
				{
					_buffer = nullptr;
					_count = nullptr;
					return;
				}
				_buffer = new char[_size + 1];
				_count = new int();
				*_count = 1;
				memcpy(_buffer, msg.c_str(), _size);
				_buffer[_size] = 0;
			}
			sharp_char(acl::string& msg)
			{
				_size = msg.length();
				if (_size == 0)
				{
					_buffer = nullptr;
					_count = nullptr;
					return;
				}
				_buffer = new char[_size + 1];
				_count = new int();
				*_count = 1;
				memcpy(_buffer, msg.c_str(), _size);
				_buffer[_size] = 0;
			}
			~sharp_char()
			{
				free();
			}
			char* get_buffer() const
			{
				return _buffer;
			}
			int user_count() const
			{
				return _count == nullptr ? 0 : *_count;
			}
			size_t size() const
			{
				return _size;
			}
			bool empty() const
			{
				return _buffer == nullptr || _size == 0;
			}
			char operator[](size_t idx) const
			{
				return _buffer == nullptr || idx < 0 || idx >= _size ? '\0' : _buffer[idx];
			}
			sharp_char& alloc(size_t size)
			{
				free();
				_buffer = new char[size + 1];
				_count = new int();
				*_count = 1;
				_size = size;
				return *this;
			}
			sharp_char& operator = (zmq_msg_t& msg)
			{
				free();
				_size = zmq_msg_size(&msg);
				if (_size == 0)
					return *this;
				alloc(_size);
				memcpy(_buffer, zmq_msg_data(&msg), _size);
				_buffer[_size] = 0;
				_count = new int();
				*_count = 1;
				return *this;
			}
			sharp_char& operator = (std::string& msg)
			{
				free();
				_size = msg.length();
				if (_size == 0)
					return *this;
				alloc(_size);
				memcpy(_buffer, msg.c_str(), _size);
				_buffer[_size] = 0;
				_count = new int();
				*_count = 1;
				return *this;
			}
			sharp_char& operator = (acl::string& msg)
			{
				free();
				_size = msg.length();
				if (_size == 0)
					return *this;
				alloc(_size);
				memcpy(_buffer, msg.c_str(), _size);
				_buffer[_size] = 0;
				_count = new int();
				*_count = 1;
				return *this;
			}
			sharp_char& operator = (const char* msg)
			{
				free();
				if (msg == nullptr)
					return *this;
				_size = strlen(msg);
				if (_size == 0)
					return *this;
				alloc(_size);
				memcpy(_buffer, msg, _size);
				_buffer[_size] = 0;
				_count = new int();
				*_count = 1;
				return *this;
			}
			void free()
			{
				if (_count == nullptr)
					return;
				*_count -= 1;
				if (*_count == 0)
				{
					delete[] _count;
					delete[] _buffer;
				}
				_count = nullptr;
				_buffer = nullptr;
				_size = 0;
			}
			sharp_char& binding(char* fri,size_t len)
			{
				free();
				_size = len;
				_buffer = fri;
				_count = new int();
				*_count = 1;
				return *this;
			}
			/**
			 * @brief 交换
			 */
			sharp_char& swap(sharp_char& fri) noexcept
			{
				{
					char* tmp = _buffer;
					_buffer = fri._buffer;
					fri._buffer = tmp;
				}
				{
					int* tmp = _count;
					_count = fri._count;
					fri._count = tmp;
				}
				{
					size_t tmp = _size;
					_size = fri._size;
					fri._size = tmp;
				}
				return *this;
			}
			sharp_char& operator = (const sharp_char& fri)
			{
				free();
				if (fri._buffer != nullptr)
				{
					_buffer = fri._buffer;
					_size = fri._size;
					_count = fri._count;
					*_count += 1;
				}
				return *this;
			}
			sharp_char& operator = (char* fri)
			{
				return binding(fri, fri == nullptr ? 0 : strlen(fri));
			}
			const char* operator*() const
			{
				return _buffer == nullptr ? "" : _buffer;
			}
			operator std::string() const
			{
				return std::string(_buffer == nullptr ? "" : _buffer);
			}
			operator acl::string() const
			{
				return acl::string(_buffer == nullptr ? "" : _buffer);
			}
		};
	}
}
#endif
