using System;
using System.IO;
using Agebull.Common.Base;

namespace Agebull.Common.Tson
{
    public unsafe class TsonSerializerBase : ScopeBase
    {
        protected TsonSerializerBase(byte ver, TsonDataType type)
        {
            Ver = ver;
            Begin(type);
        }
        /// <summary>
        /// 数据版本,0表示空数据,1表示无字段类型检查版本,2表示有字段类型检查版本
        /// </summary>
        public readonly byte Ver;
        /// <summary>
        ///     内部数据流
        /// </summary>
        public MemoryStream Stream = new MemoryStream();

        /// <summary>
        ///     结束
        /// </summary>
        /// <returns></returns>
        public byte[] Buffer { get; private set; }

        /// <summary>
        ///     开始
        /// </summary>
        public void Begin(TsonDataType type= TsonDataType.Object)
        {
            Stream.WriteByte(Ver);
            Stream.WriteByte((byte)type);
        }

        /// <summary>
        ///     写入结束符
        /// </summary>
        /// <returns></returns>
        public void End()
        {
            Stream.WriteByte(0xFF);
        }

        /// <summary>
        ///     关闭
        /// </summary>
        /// <returns></returns>
        public byte[] Close()
        {
            Stream.WriteByte(0xFF);
            Stream.WriteByte(0xFF);
            Buffer = Stream.ToArray();
            Stream.Dispose();
            return Buffer;
        }


        /// <summary>清理资源</summary>
        protected override void OnDispose()
        {
            Stream.Dispose();
        }

        /// <summary>
        ///     写入长度
        /// </summary>
        /// <param name="len"></param>
        public void WriteLen(int len)
        {
            WriteValue((short)len);
        }

        /// <summary>
        ///     写入序号
        /// </summary>
        /// <param name="idx"></param>
        public void WriteIndex(int idx)
        {
            Stream.WriteByte((byte)idx);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteBoolValue(bool value)
        {
            Stream.WriteByte((byte)(value ? 1 : 0));
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteByteValue(byte value)
        {
            Stream.WriteByte(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteStringValue(string value)
        {
            if (value == null)
            {
                WriteLen(0);
            }
            else if (value == string.Empty)
            {
                WriteLen(-1);
            }
            else
            {
                var by = value.ToUtf8Bytes();
                WriteLen(by.Length);
                Stream.Write(by, 0, by.Length);
            }
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteGuidValue(Guid value)
        {
            Stream.Write(value.ToByteArray(), 0, 16);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteDoubleValue(double value)
        {
            WriteValue((byte*)&value, 8);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteFloatValue(float value)
        {
            WriteValue((byte*)&value, 4);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteDecimalValue(decimal value)
        {
            WriteValue((byte*)&value, 8);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteLongValue(long value)
        {
            WriteValue((byte*)&value, 8);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteULongValue(ulong value)
        {
            WriteValue((byte*)&value, 8);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteIntValue(int value)
        {
            WriteValue((byte*)&value, 4);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteUIntValue(uint value)
        {
            WriteValue((byte*)&value, 4);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteShortValue(short value)
        {
            WriteValue((byte*)&value, 2);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteushortValue(ushort value)
        {
            WriteValue((byte*)&value, 2);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteValue(bool value)
        {
            Stream.WriteByte((byte)(value ? 1 : 0));
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteValue(byte value)
        {
            Stream.WriteByte(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteValue(string value)
        {
            if (value == null)
            {
                WriteLen(0);
            }
            else if (value == string.Empty)
            {
                WriteLen(-1);
            }
            else
            {
                var by = value.ToUtf8Bytes();
                WriteLen(by.Length);
                Stream.Write(by, 0, by.Length);
            }
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteValue(Guid value)
        {
            Stream.Write(value.ToByteArray(), 0, 16);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteValue(double value)
        {
            WriteValue((byte*)&value, 8);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteValue(float value)
        {
            WriteValue((byte*)&value, 4);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteValue(decimal value)
        {
            WriteValue((byte*)&value, 8);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteValue(long value)
        {
            WriteValue((byte*)&value, 8);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteValue(ulong value)
        {
            WriteValue((byte*)&value, 8);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteValue(int value)
        {
            WriteValue((byte*)&value, 4);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteValue(uint value)
        {
            WriteValue((byte*)&value, 4);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteValue(short value)
        {
            WriteValue((byte*)&value, 2);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value">值</param>
        public void WriteValue(ushort value)
        {
            WriteValue((byte*)&value, 2);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="size"></param>
        public void WriteValue(byte* addr, int size)
        {
            for (var idx = 0; idx < size; idx++) Stream.WriteByte(*(addr + idx));
        }
    }
}