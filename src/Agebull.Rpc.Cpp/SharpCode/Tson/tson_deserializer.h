#ifndef _AGEBULL_TSON_DESERIALIZER_H
#define _AGEBULL_TSON_DESERIALIZER_H
#pragma once
#include "tson_def.h"

#pragma unmanaged
namespace Agebull
{
	namespace Tson
	{
		class Deserializer
		{
			bool m_buffer_new;
			bool m_succeed;
			char* m_bufer;
			size_t m_postion;
			size_t m_buffer_len;
			size_t m_data_len;
			bool m_data_full;//是否全量写入
			OBJ_TYPEID m_data_type;
			OBJ_VERSION m_data_ver;
		public:
			bool IsEof() const
			{
				return !m_succeed || m_data_len > 2048 || m_postion >= m_data_len || static_cast<OBJ_TYPE>(m_bufer[m_postion]) == OBJ_TYPE_EOF;
			}

			bool IsBof() const
			{
				return m_postion == 0;
			}

			void Reset()
			{
				m_postion = 0;
			}

			void Begin()
			{
				m_succeed = true;
				m_postion = 0;
				Read(m_data_len);
				Read(m_data_type);
				Read(m_data_ver);
				Read(m_data_full);
			}

			void End()
			{
				m_postion = m_data_len;
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

			OBJ_TYPEID GetDataType() const
			{
				return m_data_type;
			}

			OBJ_VERSION GetDataVersion() const
			{
				return m_data_ver;
			}

			char* GetBuffer() const
			{
				return m_bufer;
			}

			size_t GetBufLen() const
			{
				return m_buffer_len;
			}

		public:
			Deserializer::Deserializer()
				: m_buffer_new(false)
				, m_succeed(true)
				, m_bufer(nullptr)
				, m_postion(0)
				, m_buffer_len(0)
				, m_data_len(0)
				, m_data_full(false)
				, m_data_type(0)
				, m_data_ver(0)
			{
			}

			bool CreateBuffer(size_t len)
			{
				if (m_buffer_new)
					delete[] m_bufer;
				if (len <= 8)
					return false;
				m_data_len = m_buffer_len = len;
				m_bufer = new char[len];
				m_buffer_new = true;
				return true;
			}

			Deserializer::Deserializer(char* bufer, bool auto_del = false)
				: m_buffer_new(auto_del)
				, m_succeed(true)
				, m_bufer(bufer)
				, m_postion(0)
				, m_data_full(false)
				, m_data_type(0)
				, m_data_ver(0)
			{
				Begin();
			}

			Deserializer::Deserializer(char* bufer, size_t len, bool auto_del = false)
				: m_buffer_new(auto_del)
				, m_succeed(true)
				, m_bufer(bufer)
				, m_postion(0)
				, m_buffer_len(len)
				, m_data_len(len)
				, m_data_full(false)
				, m_data_type(0)
				, m_data_ver(0)
			{
				Begin();
			}

			Deserializer::~Deserializer()
			{
				if (m_buffer_new)
					delete[] m_bufer;
			}

			/*/     跳过空数据
			bool SkipEmpty(OBJ_TYPE type)
			{
				switch (type)
				{
				case OBJ_TYPE_BOOLEN:
				case OBJ_TYPE_CHAR:
					ReadByte();
					break;
				case OBJ_TYPE_INT16:
				case OBJ_TYPE_UINT16:
					ReadInt16();
					break;
				case OBJ_TYPE_INT32:
				case OBJ_TYPE_UINT32:
					ReadInt32();
					break;
				case OBJ_TYPE_INT64:
				case OBJ_TYPE_UINT64:
					ReadInt32();
					break;
				case OBJ_TYPE_FLOAT:
					ReadSingle();
					break;
				case OBJ_TYPE_DOUBLE:
					ReadInt32();
					break;
				case OBJ_TYPE_STRING:
					ReadString();
					break;
				case OBJ_TYPE_BINARY:
					ReadBinrary();
					break;
				default:
					return false;
				}
				return true;
			}*/
		public:

			inline void read_to(char* bf, size_t len)
			{
				for (size_t i = 0; i < len; i++)
				{
					assert(m_postion < m_data_len);
					if (m_postion < m_data_len)
					{
						bf[i] = m_bufer[m_postion++];
					}
					else
					{
						bf[i] = '\0';
						this->m_succeed = false;
					}
				}
			}

			void Read(bool& buffer)
			{
				assert(m_postion < m_data_len);
				if (m_postion < m_data_len)
					buffer = m_bufer[m_postion++] != '0';
				else
				{
					buffer = false;
					this->m_succeed = false;
				}
			}

			void Read(char& buffer)
			{
				assert(m_postion < m_data_len);
				if (m_postion < m_data_len)
					buffer = m_bufer[m_postion++];
				else
				{
					buffer = '0';
					this->m_succeed = false;
				}
			}

			void Read(unsigned char& buffer)
			{
				assert(m_postion < m_data_len);
				if (m_postion < m_data_len)
					buffer = m_bufer[m_postion++];
				else
				{
					buffer = '0';
					this->m_succeed = false;
				}
			}

			void Read(short& buffer)
			{
				read_to(reinterpret_cast<char*>(&buffer), sizeof(short));
			}

			void Read(ushort& buffer)
			{
				read_to(reinterpret_cast<char*>(&buffer), sizeof(ushort));
			}

			void Read(int& buffer)
			{
				read_to(reinterpret_cast<char*>(&buffer), sizeof(int));
			}

			void Read(uint& buffer)
			{
				read_to(reinterpret_cast<char*>(&buffer), sizeof(uint));
			}

			void Read(int64& buffer)
			{
				read_to(reinterpret_cast<char*>(&buffer), sizeof(int64));
			}

			void Read(uint64& buffer)
			{
				read_to(reinterpret_cast<char*>(&buffer), sizeof(uint64));
			}

			void Read(tm& buffer)
			{
				time_t tm;
				read_to(reinterpret_cast<char*>(&tm), sizeof(time_t));
				localtime_s(&buffer, &tm);
			}

			void Read(float& buffer)
			{
				read_to(reinterpret_cast<char*>(&buffer), sizeof(float));
			}

			void Read(double& buffer)
			{
				read_to(reinterpret_cast<char*>(&buffer), sizeof(double));
			}

		public:
			char ReadChar()
			{
				assert(m_postion < m_data_len);
				if (m_postion < m_data_len)
					return m_bufer[m_postion++];

				this->m_succeed = false;
				return '\0';
			}


			unsigned char ReadByte()
			{
				assert(m_postion < m_data_len);
				if (m_postion < m_data_len)
					return static_cast<unsigned char>(m_bufer[m_postion++]);

				this->m_succeed = false;
				return '\0';
			}

			bool ReadBoolean()
			{
				assert(m_postion < m_data_len);
				if (m_postion < m_data_len)
					return m_bufer[m_postion++] != '\0';

				this->m_succeed = false;
				return false;
			}
			template <class T>
			void Read(T& buffer)
			{
				read_to(reinterpret_cast<char*>(&buffer), sizeof(T));
			}

			template <class T>
			T Read()
			{
				T number;
				read_to(reinterpret_cast<char*>(&number), sizeof(T));
				return number;
			}

			float ReadSingle()
			{
				float number;
				read_to(reinterpret_cast<char*>(&number), sizeof(float));
				return number;
			}

			short ReadInt16()
			{
				short number;
				read_to(reinterpret_cast<char*>(&number), sizeof(short));
				return number;
			}


			int ReadInt32()
			{
				int number;
				read_to(reinterpret_cast<char*>(&number), sizeof(int));
				return number;
			}

			int64 ReadInt64()
			{
				int64 number;
				read_to(reinterpret_cast<char*>(&number), sizeof(int64));
				return number;
			}

			ushort ReadUInt16()
			{
				short number;
				read_to(reinterpret_cast<char*>(&number), sizeof(ushort));
				return number;
			}

			uint ReadUInt32()
			{
				int number;
				read_to(reinterpret_cast<char*>(&number), sizeof(uint));
				return number;
			}

			uint64 ReadUInt64()
			{
				int64 number;
				read_to(reinterpret_cast<char*>(&number), sizeof(uint64));
				return number;
			}


			char* ReadString()
			{
				ushort len;
				Read(len);
				char* str = new char[len + 1];
				ReadArray(str, len, len + 1);
				return str;
			}

			void ReadString(char* str, int ptrLen)
			{
				ushort len;
				Read(len);
				ReadArray(str, len, ptrLen);
			}

			template <size_t _Nm>
			void ReadStr(char(&str)[_Nm])
			{
				ushort len;
				Read(len);
				ReadArray(str, len, _Nm);
			}

			template <class T, size_t _Nm>
			void Read(T(&t)[_Nm])
			{
				ushort len;
				Read(len);
				size_t i = 0;
				if (_Nm < len)
				{
					this->m_succeed = false;
					len = _Nm;
				}
				if ((len + m_postion) > m_data_len)
				{
					this->m_succeed = false;
					len = 0;
				}
				for (; i < len && i < _Nm; i++)
				{
					Read<T>(t[i]);
				}
				for (; i < _Nm; i++)
				{
					t[i] = static_cast<T>(0);
				}
			}

			char* ReadBinrary()
			{
				ushort len;
				Read(len);
				char* str = new char[len];
				ReadArray(str, len, len);
				return str;
			}
			char* ReadBinrary(size_t* len)
			{
				Read(*len);
				char* str = new char[*len];
				ReadArray(str, *len, *len);
				return str;
			}

			char* ReadBinrary(size_t len)
			{
				char* buffer = new char[len];
				ReadArray(buffer, len, len);
				return buffer;
			}

		private:

			void ReadArray(char* buf, size_t len, size_t len2)
			{
				if ((len + m_postion) > m_data_len)
				{
					this->m_succeed = false;
					len = 0;
				}
				size_t i = 0;
				for (; i < len && i < len2; i++)
				{
					assert(m_postion < m_data_len);
					if (m_postion < m_data_len)
					{
						buf[i] = m_bufer[m_postion++];
					}
					else
					{
						this->m_succeed = false;
						break;
					}
				}
				for (; i < len2; i++)
				{
					buf[len] = '\0';
				}
			}
			template <class T>
			T* ReadArray()
			{
				ushort len;
				Read(len);
				if ((len + m_postion) > m_data_len)
				{
					this->m_succeed = false;
					return nullptr;
				}
				T* buffer = new T[len];
				for (size_t pos = 0; m_postion < m_data_len && pos < len; pos++)
				{
					read_to(reinterpret_cast<char*>(&buffer[pos]), sizeof(T));
					if (!this->m_succeed)
						break;
				}
				return buffer;
			}
			template <class T>
			void ReadArray(T* buffer, size_t bufLen)
			{
				ushort len;
				Read(len);
				size_t pos = 0;
				if (bufLen < len)
				{
					this->m_succeed = false;
					len = bufLen;
				}
				if ((len + m_postion) > m_data_len)
				{
					this->m_succeed = false;
					len = 0;
				}
				for (; pos < len; pos++)
				{
					Read(buffer[pos]);
					if (!this->m_succeed)
						break;
				}
				//if (pos < len)
				//	memeset(&buffer[pos], 0, sizeof(T) * (len - pos));
			}

			//template<class T> void Read(T* buffer)
			//{
			//	char* bf = reinterpret_cast<char*>(buffer);
			//	for (size_t i = 0; i < sizeof(T); i++)
			//	{
			//		bf[i] = m_bufer[m_postion++];
			//	}
			//}
		};
	}
}
#endif
