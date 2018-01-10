#ifndef _AGEBULL_TSON_SERIALIZER_H
#define _AGEBULL_TSON_SERIALIZER_H
#pragma once
#include "stdinc.h"
#include "tson_def.h"

#pragma unmanaged
namespace agebull
{
	namespace Tson
	{
		//头长度10
		class Serializer
		{
			char* m_bufer;
			size_t m_postion;
			size_t m_buffer_len;
			size_t m_data_len;
			bool m_data_full;//是否全量写入
			bool m_buffer_new;
		public:
			Serializer::Serializer()
				: m_bufer(nullptr)
				, m_postion(0)
				, m_buffer_len(0)
				, m_data_len(0)
				, m_data_full(true)
				, m_buffer_new(false)
			{
			}

			Serializer::Serializer(char* bufer, size_t len, bool auto_del = false)
				: m_bufer(bufer)
				, m_postion(0)
				, m_buffer_len(len)
				, m_data_len(len)
				, m_data_full(true)
				, m_buffer_new(auto_del)
			{
			}

			Serializer::~Serializer()
			{
				if (m_buffer_new)
					delete[] m_bufer;
			}

		public:

			bool CreateBuffer(size_t len, bool auto_del = true)
			{
				if (len <= SERIALIZE_BASE_LEN)
					return false;
				if (m_buffer_new)
					delete[] m_bufer;
				m_buffer_len = len + 8;
				m_bufer = new char[m_buffer_len];
				m_data_len = len;
				m_buffer_new = auto_del;
				return true;
			}

			void Reset()
			{
				m_postion = 0;
			}

			void Begin(OBJ_TYPEID type, OBJ_VERSION ver)
			{
				m_postion = 4;//预留写入总长度
				Write(type);//数据类型
				Write(ver);//数据版本
				Write(m_data_full);//数据是否增量
			}

			void End()
			{
				int mod = (4 - m_postion % 4) + 3;//对齐(因为后面还写一字节,所以是加3)
				for (int i = 0; i < mod; i++)
					m_bufer[m_postion++] = OBJ_TYPE_EOF;
				m_bufer[m_postion++] = OBJ_TYPE_END;
				m_data_len = m_postion;
				m_postion = 0;
				WriteValue<size_t>(m_data_len);
				m_postion = 0;
			}

			//全量写入
			bool get_full_data() const
			{
				return m_data_full;
			}

			//全量写入
			void set_full_data(bool full)
			{
				m_data_full = full;
			}

			size_t GetDataLen() const
			{
				return m_data_len;
			}

			char* GetBuffer() const
			{
				return m_bufer;
			}

		public:
			template <size_t _Nm>
			bool str_is_empty(const char(&t)[_Nm]) const
			{
				return t[0] == '\0';
			}

			bool is_empty(const char* str) const
			{
				return str == nullptr || str[0] == '\0';
			}

			template <class T, size_t _Nm>
			bool is_empty(const T(&t)[_Nm]) const
			{
				return t[0] == static_cast<T>(0);
			}

			template <class T>
			bool is_empty(const T& v) const
			{
				return v == static_cast<T>(0);
			}

			bool is_empty(const tm& time) const
			{
				return time.tm_year <= 1;
			}

		public:
			void WriteIndex(const unsigned char& chr)
			{
				assert(m_postion < m_buffer_len);
				m_bufer[m_postion++] = static_cast<char>(chr);
			}

		public:
			void Write(bool b)
			{
				assert(m_postion < m_buffer_len);
				m_bufer[m_postion++] = b ? '\1' : '\0';
			}

			void Write(char chr)
			{
				assert(m_postion < m_buffer_len);
				m_bufer[m_postion++] = chr;
			}

			void Write(unsigned char chr)
			{
				assert(m_postion < m_buffer_len);
				m_bufer[m_postion++] = static_cast<char>(chr);
			}

			void Write(short number)
			{
				write_to((char*)(&number), sizeof(short));
			}

			void Write(ushort number)
			{
				write_to((char*)(&number), sizeof(ushort));
			}

			void Write(int number)
			{
				write_to((char*)(&number), sizeof(int));
			}

			void Write(uint number)
			{
				write_to((char*)(&number), sizeof(uint));
			}

			void Write(int64 number)
			{
				write_to((char*)(&number), sizeof(int64));
			}

			void Write(uint64 number)
			{
				write_to((char*)(&number), sizeof(uint64));
			}


			void Write(float number)
			{
				write_to((char*)(&number), sizeof(float));
			}

			void Write(double number)
			{
				write_to((char*)(&number), sizeof(double));
			}

			template <class T>
			void WriteValue(T& t)
			{
				write_to((char*)(&t), sizeof(T));
			}

			void Write(tm t)
			{
				if (t.tm_year > 1900)
					t.tm_year -= 1900;
				time_t tt = mktime(&t);
				write_to((char*)(&tt), sizeof(time_t));
			}

		public:
			template <size_t _Nm>
			void WriteStr(const char(&str)[_Nm])
			{
				auto len = strlen(str);
				write_len(len);
				write_to(str, len < _Nm ? len : _Nm);
			}

			template <class T, size_t _Nm>
			void WriteArray(T(&t)[_Nm])
			{
				write_len(_Nm);
				for (size_t i = 0; i < _Nm; i++)
				{
					write_to((char*)(&t[i]), sizeof(T));
				}
			}

			template <class T>
			void WriteTo(T t)
			{
				write_to((char*)(&t), sizeof(T));
			}

			template <class T>
			void WriteTo(T* t, size_t len)
			{
				write_len(len);
				for (size_t i = 0; i < len; i++)
				{
					write_to((char*)(&t[i]), sizeof(T));
				}
			}

			void WriteBinary(const char* binary, size_t len)
			{
				write_len(len);
				write_to(binary, len);
			}

			void WriteObject(Serializer& saver)
			{
				write_to(saver.GetBuffer(), saver.GetDataLen());
			}

			void Write(Serializer& saver)
			{
				write_to(saver.GetBuffer(), saver.GetDataLen());
			}
		private:
			void write_to(const char* bf, size_t len)
			{
				for (size_t i = 0; i < len; i++)
				{
					assert(m_postion < m_buffer_len);
					m_bufer[m_postion++] = static_cast<char>(bf[i]);
				}
			}
			void write_len(int len)
			{
				Write(static_cast<ushort>(len));
			}
			void write_len(size_t len)
			{
				Write(static_cast<ushort>(len));
			}
		};
	}
}
#endif
