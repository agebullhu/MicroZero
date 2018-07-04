using System;
using System.Collections.Generic;

namespace Agebull.Common.Tson
{
    /// <summary>
    /// TSON序列化器
    /// </summary>
    public class TsonSerializer : TsonSerializerBase, ITsonSerializer
    {
        /// <summary>
        /// 构造
        /// </summary>
        public TsonSerializer(TsonDataType type = TsonDataType.Object) : base(2,type)
        {

        }

        #region 快捷方法

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        public void Write(byte index, bool value)
        {
            if (!value)
                return;
            WriteIndex(index);
            Write(true);
        }


        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        public void Write(byte index, byte value)
        {
            if (value == 0)
                return;
            WriteIndex(index);
            Write(value);
        }


        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        public void Write(byte index, DateTime value)
        {
            if (value == DateTime.MinValue)
                return;
            WriteIndex(index);
            Write(value);
        }


        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        public void Write(byte index, double value)
        {
            if (double.IsNaN(value))
                return;
            WriteIndex(index);
            Write(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        public void Write(byte index, float value)
        {
            if (float.IsNaN(value))
                return;
            WriteIndex(index);
            Write(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        public void Write(byte index, decimal value)
        {
            if (value == 0M)
                return;
            WriteIndex(index);
            Write(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        public void Write(byte index, string value)
        {
            if (value == null)
                return;
            WriteIndex(index);
            Write(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        public void Write(byte index, Guid value)
        {
            if (value == Guid.Empty)
                return;
            WriteIndex(index);
            Write(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        public void Write(byte index, long value)
        {
            if (value == 0)
                return;
            WriteIndex(index);
            Write(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        public void Write(byte index, ulong value)
        {
            if (value == 0)
                return;
            WriteIndex(index);
            Write(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        public void Write(byte index, int value)
        {
            if (value == 0)
                return;
            WriteIndex(index);
            Write(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        public void Write(byte index, uint value)
        {
            if (value == 0)
                return;
            WriteIndex(index);
            Write(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        public void Write(byte index, short value)
        {
            if (value == 0)
                return;
            WriteIndex(index);
            Write(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        public void Write(byte index, ushort value)
        {
            if (value == 0)
                return;
            WriteIndex(index);
            Write(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        /// <param name="type">类型</param>
        /// <param name="write">写入方法 </param>
        public void Write<T>(byte index, List<T> value, TsonDataType type, Action<T> write)
        {
            if (value == null || value.Count == 0)
                return;
            WriteIndex(index);
            Write(value, type, write);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        /// <param name="type">类型</param>
        /// <param name="write">写入方法 </param>
        public void Write<T>(byte index, T[] value, TsonDataType type, Action<T> write)
        {
            if (value == null || value.Length == 0)
                return;
            WriteIndex(index);
            Write(value, type, write);
        }

        #endregion


        #region 写入值

        /// <summary>
        ///     写入字段类型
        /// </summary>
        /// <param name="type"></param>
        public void WriteType(TsonDataType type)
        {
            WriteValue((byte)type);
        }

        public void Write(bool value)
        {
            WriteType(TsonDataType.Bool);
            WriteValue(value);
        }


        public void Write(byte value)
        {
            WriteType(TsonDataType.Byte);
            WriteValue(value);
        }


        public void Write(DateTime value)
        {
            WriteType(TsonDataType.DateTime);
            WriteValue(value.Ticks);
        }


        public void Write(string value)
        {
            WriteType(TsonDataType.String);
            WriteValue(value);
        }

        public void Write(Guid value)
        {
            WriteType(TsonDataType.Guid);
            WriteValue(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        public void Write(double value)
        {
            WriteType(TsonDataType.Double);
            WriteValue(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        public void Write(float value)
        {
            WriteType(TsonDataType.Float);
            WriteValue(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        public void Write(decimal value)
        {
            WriteType(TsonDataType.Decimal);
            WriteValue(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        public void Write(long value)
        {
            WriteType(TsonDataType.Long);
            WriteValue(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        public void Write(ulong value)
        {
            WriteType(TsonDataType.ULong);
            WriteValue(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        public void Write(int value)
        {
            WriteType(TsonDataType.Int);
            WriteValue(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        public void Write(uint value)
        {
            WriteType(TsonDataType.UInt);
            WriteValue(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        public void Write(short value)
        {
            WriteType(TsonDataType.Short);
            WriteValue(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        public void Write(ushort value)
        {
            WriteType(TsonDataType.UShort);
            WriteValue(value);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="array"> </param>
        /// <param name="type"></param>
        /// <param name="write"></param>
        /// <returns> </returns>
        public void Write<T>(List<T> array, TsonDataType type, Action<T> write)
        {
            if (array == null || array.Count == 0)
            {
                WriteType(TsonDataType.Nil);
                return;
            }

            WriteType(TsonDataType.Array);
            WriteType(type);
            WriteLen(array.Count);
            foreach (var item in array)
                write(item);
        }

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="array"> </param>
        /// <param name="type"></param>
        /// <param name="write"></param>
        public void Write<T>(T[] array, TsonDataType type, Action<T> write)
        {
            if (array == null || array.Length == 0)
            {
                WriteType(TsonDataType.Nil);
                return;
            }

            WriteType(TsonDataType.Array);
            WriteType(type);
            WriteLen(array.Length);
            foreach (var item in array)
                write(item);
        }

        #endregion

    }
}