using System;
using System.Collections.Generic;
using System.Text;
using Agebull.Common.Base;

namespace Agebull.Common.Tson
{
    public unsafe class TsonDeserializerBase : ScopeBase
    {
        /// <summary>
        /// 数据版本,0表示空数据,1表示无字段类型检查版本,2表示有字段类型检查版本
        /// </summary>
        public Byte Ver { get; }

        /// <summary>
        /// 类型
        /// </summary>
        public TsonDataType DataType { get; }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="buffer"></param>
        public TsonDeserializerBase(byte[] buffer)
        {
            if (buffer == null || buffer.Length < 3 || buffer[buffer.Length - 2] != 0xFF || buffer[buffer.Length - 1] != 0xFF)
                throw new NullReferenceException("buffer不能为空且必须为有效的TSON数据");
            Buffer = buffer;
            Postion = 0;
            Ver = ReadVer();
            if (Ver > 2)
                throw new NullReferenceException("buffer必须为有效的TSON数据");
            var type = ReadByte();
            if (type > (byte) TsonDataType.DateTime && type < (byte) TsonDataType.Object)
                throw new NullReferenceException("buffer必须为有效的TSON数据");
            DataType= (TsonDataType)type;
        }
        /// <summary>
        /// 开始子级
        /// </summary>
        /// <returns></returns>
        public TsonDataType Begin(out byte ver)
        {
            ver = ReadVer();
            if (ver > 2)
                throw new NullReferenceException("buffer必须为有效的TSON数据");
            var type = ReadByte();
            if (type > (byte) TsonDataType.DateTime && type < (byte) TsonDataType.Object)
                throw new NullReferenceException("buffer必须为有效的TSON数据");
            return (TsonDataType)type;
        }

        /// <summary>
        /// 结束子级
        /// </summary>
        /// <returns></returns>
        public void End()
        {
            ReadByte();
        }

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        public TsonDataType ReadType()
        {
            return (TsonDataType)ReadByte();
        }

        /// <summary>
        /// 数据
        /// </summary>
        public readonly byte[] Buffer;
        /// <summary>
        /// 当前位置
        /// </summary>
        public int Postion;

        /// <summary>
        /// 当前位置的值
        /// </summary>
        /// <returns></returns>
        public byte Current => Buffer[Postion];

        /// <summary>
        /// 是否处于结束位置
        /// </summary>
        public bool IsEof => Ver == 0 || Postion >= Buffer.Length || Buffer[Postion] == 0xFF;

        /// <summary>
        /// 是否处于错误位置
        /// </summary>
        public bool IsBad => Postion >= Buffer.Length || Postion < 0;

        /// <summary>
        /// 读取版本
        /// </summary>
        /// <returns></returns>
        public byte ReadVer()
        {
            return ReadByte();
        }

        /// <summary>清理资源</summary>
        protected override void OnDispose()
        {
            Postion++;//跳过结束符号
        }
        /// <summary>
        /// 读取字段索引
        /// </summary>
        /// <returns></returns>
        public int ReadIndex()
        {
            return ReadByte();
        }
        /// <summary>
        /// 读取长度
        /// </summary>
        /// <returns></returns>
        public int ReadLen()
        {
            return ReadShort();
        }
        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        public float ReadFloat()
        {
            fixed (byte* addr = &Buffer[Postion])
            {
                Postion += 4;
                return *((float*)addr);
            }
        }

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        public double ReadDouble()
        {
            fixed (byte* addr = &Buffer[Postion])
            {
                Postion += 8;
                return *((double*)addr);
            }
        }

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        public decimal ReadDecimal()
        {
            fixed (byte* addr = &Buffer[Postion])
            {
                Postion += 8;
                return *((decimal*)addr);
            }
        }
        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        public bool ReadBool()
        {
            return Buffer[Postion++] == 1;
        }
        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        public byte ReadByte()
        {
            return Buffer[Postion++];
        }
        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        public sbyte ReadSByte()
        {
            fixed (byte* addr = &Buffer[Postion])
            {
                Postion++;
                return *((sbyte*)addr);
            }
        }

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        public DateTime ReadDateTime()
        {
            return new DateTime(ReadLong());
        }

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        public long ReadLong()
        {
            fixed (byte* addr = &Buffer[Postion])
            {
                Postion += 8;
                return *((long*)addr);
            }
        }

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        public ulong ReadULong()
        {
            fixed (byte* addr = &Buffer[Postion])
            {
                Postion += 8;
                return *((ulong*)addr);
            }
        }
        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        public uint ReadUInt()
        {
            fixed (byte* addr = &Buffer[Postion])
            {
                Postion += 4;
                return *((uint*)addr);
            }
        }
        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        public int ReadInt()
        {
            fixed (byte* addr = &Buffer[Postion])
            {
                Postion += 4;
                return *((int*)addr);
            }
        }

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        public short ReadShort()
        {
            fixed (byte* addr = &Buffer[Postion])
            {
                Postion += 2;
                return *((short*)addr);
            }
        }

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        public ushort ReadUShort()
        {
            fixed (byte* addr = &Buffer[Postion])
            {
                Postion += 2;
                return *((ushort*)addr);
            }
        }

        /// <summary>
        /// 读取文本
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            int size = ReadLen();
            if (size == 0)
                return null;
            if (size < 0)
                return string.Empty;
            string str = Encoding.UTF8.GetString(Buffer, Postion, size);
            Postion += size;
            return str;
        }
        /// <summary>
        /// 读取GUID
        /// </summary>
        /// <returns></returns>
        public Guid ReadGuid()
        {
            byte[] buf = new byte[16];
            for (int idx = 0; idx < 16; idx++, Postion++)
                buf[idx] = Buffer[Postion];
            return new Guid(buf);
        }
        /// <summary>
        /// 读一个列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="read"></param>
        /// <returns></returns>
        public List<T> ReadList<T>(Func<T> read)
        {
            var array = new List<T>();
            int size = ReadLen();
            for (int idx = 0; idx < size; idx++)
            {
                array.Add(read());
            }
            return array;
        }
        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <typeparam name="TList"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="read"></param>
        /// <returns></returns>
        public TList ReadList<TList, TValue>(Func<TValue> read)
            where TList : class, IList<TValue>, new()
        {
            var array = new TList();
            int size = ReadLen();
            for (int idx = 0; idx < size; idx++)
            {
                array.Add(read());
            }
            return array;
        }
        /// <summary>
        /// 读一个列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="read"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        public void ReadList<T>(IList<T> array, Func<T> read)
        {
            int size = ReadLen();
            for (int idx = 0; idx < size; idx++)
            {
                array.Add(read());
            }
        }
        /// <summary>
        /// 读一个数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="read"></param>
        /// <returns></returns>
        public T[] ReadArray<T>(Func<T> read)
        {
            int size = ReadLen();
            T[] array = new T[size];
            for (int idx = 0; idx < size; idx++)
            {
                array[idx] = read();
            }
            return array;
        }
    }
}