using System;
using System.Collections.Generic;

namespace Agebull.Common.Tson
{
    public class TsonDeserializer : TsonDeserializerBase, ITsonDeserializer
    {
        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="buffer"></param>
        public TsonDeserializer(byte[] buffer) : base(buffer)
        {
        }

        #region Type

        private enum TypeCheckState
        {
            Success,
            Empty,
            Nil,
            Error
        }

        /// <summary>
        ///     是否目标类型,如果不是直接跳过
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private TypeCheckState IsDestArrayType(TsonDataType type)
        {
            if (Ver == 1)
                return TypeCheckState.Success;
            var srcType = ReadType();
            if (srcType == TsonDataType.Empty) return TypeCheckState.Empty;
            if (srcType == TsonDataType.Nil) return TypeCheckState.Nil;
            if (srcType != TsonDataType.Array)
            {
                SkipByType(srcType);
                return TypeCheckState.Error;
            }

            if (ReadType() == type)
                return TypeCheckState.Success;
            Postion--;
            SkipByType(srcType);
            return TypeCheckState.Error;
        }

        /// <summary>
        ///     是否目标类型,如果不是直接跳过
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private TypeCheckState IsDestType(TsonDataType type)
        {
            var srcType = ReadType();
            if (srcType == TsonDataType.Empty) return TypeCheckState.Empty;
            if (srcType == TsonDataType.Nil) return TypeCheckState.Nil;
            if (srcType == type)
                return TypeCheckState.Success;
            SkipByType(srcType);
            return TypeCheckState.Error;
        }

        /// <summary>
        ///     跳到结尾处
        /// </summary>
        private void SkipToEnd()
        {
            while (!IsEof)
            {
                ReadIndex();
                SkipByType(ReadType());
            }
        }

        /// <summary>
        ///     根据类型跳过
        /// </summary>
        /// <param name="type"></param>
        private void SkipByType(TsonDataType type)
        {
            switch (type)
            {
                case TsonDataType.Bool:
                case TsonDataType.Byte:
                    ++Postion;
                    break;
                case TsonDataType.UShort:
                case TsonDataType.Short:
                    Postion += 2;
                    break;
                case TsonDataType.UInt:
                case TsonDataType.Int:
                case TsonDataType.Float:
                    Postion += 4;
                    break;
                case TsonDataType.Long:
                case TsonDataType.ULong:
                case TsonDataType.Decimal:
                case TsonDataType.Double:
                case TsonDataType.DateTime:
                    Postion += 8;
                    break;
                case TsonDataType.Guid:
                    Postion += 16;
                    break;
                case TsonDataType.String:
                    Postion += ReadLen();
                    break;
                case TsonDataType.Object:
                    ReadVer();
                    SkipToEnd();
                    break;
                case TsonDataType.Array:
                    type = ReadType();
                    var size = ReadLen();
                    for (var i = 0; i < size; i++)
                        SkipByType(type);
                    break;
            }
        }

        #endregion


        #region Read

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        float ITsonDeserializer.ReadFloat()
        {
            if (TypeCheckState.Success != IsDestType(TsonDataType.Float))
                return float.NaN;
            return ReadFloat();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        double ITsonDeserializer.ReadDouble()
        {
            if (TypeCheckState.Success != IsDestType(TsonDataType.Double))
                return double.NaN;
            return ReadDouble();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        decimal ITsonDeserializer.ReadDecimal()
        {
            if (TypeCheckState.Success != IsDestType(TsonDataType.Decimal))
                return 0M;
            return ReadDecimal();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        bool ITsonDeserializer.ReadBool()
        {
            if (TypeCheckState.Success != IsDestType(TsonDataType.Bool))
                return false;
            return ReadBool();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        byte ITsonDeserializer.ReadByte()
        {
            if (TypeCheckState.Success != IsDestType(TsonDataType.Byte))
                return 0;
            return ReadByte();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        DateTime ITsonDeserializer.ReadDateTime()
        {
            if (TypeCheckState.Success != IsDestType(TsonDataType.DateTime))
                return new DateTime();
            return ReadDateTime();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        long ITsonDeserializer.ReadLong()
        {
            if (TypeCheckState.Success != IsDestType(TsonDataType.Long))
                return 0;
            return ReadLong();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        ulong ITsonDeserializer.ReadULong()
        {
            if (TypeCheckState.Success != IsDestType(TsonDataType.ULong))
                return 0;
            return ReadULong();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        uint ITsonDeserializer.ReadUInt()
        {
            if (TypeCheckState.Success != IsDestType(TsonDataType.UInt))
                return 0;
            return ReadUInt();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        int ITsonDeserializer.ReadInt()
        {
            if (TypeCheckState.Success != IsDestType(TsonDataType.Int))
                return 0;
            return ReadInt();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        short ITsonDeserializer.ReadShort()
        {
            if (TypeCheckState.Success != IsDestType(TsonDataType.Short))
                return 0;
            return ReadShort();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        ushort ITsonDeserializer.ReadUShort()
        {
            if (TypeCheckState.Success != IsDestType(TsonDataType.UShort))
                return 0;
            return ReadUShort();
        }

        /// <summary>
        ///     读取文本
        /// </summary>
        /// <returns></returns>
        string ITsonDeserializer.ReadString()
        {
            if (TypeCheckState.Success != IsDestType(TsonDataType.String))
                return null;
            return ReadString();
        }

        /// <summary>
        ///     读取GUID
        /// </summary>
        /// <returns></returns>
        Guid ITsonDeserializer.ReadGuid()
        {
            if (TypeCheckState.Success != IsDestType(TsonDataType.Guid))
                return Guid.Empty;
            return ReadGuid();
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="read"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<T> ReadList<T>(Func<T> read, TsonDataType type)
        {
            var state = IsDestArrayType(type);
            if (state == TypeCheckState.Error || state == TypeCheckState.Nil)
                return null;
            var array = new List<T>();
            if (state == TypeCheckState.Empty)
                return array;
            ReadList(array, read);
            return array;
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <typeparam name="TList"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="read"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public TList ReadList<TList, TValue>(Func<TValue> read, TsonDataType type)
            where TList : class, IList<TValue>, new()
        {
            var state = IsDestArrayType(type);
            if (state == TypeCheckState.Error || state == TypeCheckState.Nil)
                return null;
            var array = new TList();
            if (state == TypeCheckState.Empty)
                return array;
            ReadList(array, read);
            return array;
        }

        /// <summary>
        ///     读一个数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="read"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public T[] ReadArray<T>(Func<T> read, TsonDataType type)
        {
            var state = IsDestArrayType(type);
            if (state == TypeCheckState.Error || state == TypeCheckState.Nil)
                return null;
            if (state == TypeCheckState.Empty)
                return new T[0];
            return ReadArray(read);
        }

        #endregion


        #region Read

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        public float? ReadFloatNil(float? def = null)
        {
            switch (IsDestType(TsonDataType.Float))
            {
                case TypeCheckState.Error:
                    return def;
                case TypeCheckState.Nil:
                    return null;
                case TypeCheckState.Empty:
                    return 0.0F;
            }

            return ReadFloat();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        public double? ReadDoubleNil(double? def = null)
        {
            switch (IsDestType(TsonDataType.Float))
            {
                case TypeCheckState.Error:
                    return def;
                case TypeCheckState.Nil:
                    return null;
                case TypeCheckState.Empty:
                    return 0.0D;
            }

            return ReadDouble();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        public decimal? ReadDecimalNil(decimal? def = null)
        {
            switch (IsDestType(TsonDataType.Float))
            {
                case TypeCheckState.Error:
                    return def;
                case TypeCheckState.Nil:
                    return null;
                case TypeCheckState.Empty:
                    return 0M;
            }

            return ReadDecimal();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        public bool? ReadBoolNil(bool? def = null)
        {
            switch (IsDestType(TsonDataType.Float))
            {
                case TypeCheckState.Error:
                    return def;
                case TypeCheckState.Nil:
                    return null;
                case TypeCheckState.Empty:
                    return false;
            }

            return ReadBool();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        public byte? ReadByteNil(byte? def = null)
        {
            switch (IsDestType(TsonDataType.Float))
            {
                case TypeCheckState.Error:
                    return def;
                case TypeCheckState.Nil:
                    return null;
                case TypeCheckState.Empty:
                    return 0;
            }

            return ReadByte();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        public DateTime? ReadDateTimeNil(DateTime? def = null)
        {
            switch (IsDestType(TsonDataType.Float))
            {
                case TypeCheckState.Error:
                    return def;
                case TypeCheckState.Nil:
                    return null;
                case TypeCheckState.Empty:
                    return DateTime.MinValue;
            }

            return ReadDateTime();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        public long? ReadLongNil(long? def = null)
        {
            switch (IsDestType(TsonDataType.Float))
            {
                case TypeCheckState.Error:
                    return def;
                case TypeCheckState.Nil:
                    return null;
                case TypeCheckState.Empty:
                    return 0;
            }

            return ReadLong();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        public ulong? ReadULongNil(ulong? def = null)
        {
            switch (IsDestType(TsonDataType.Float))
            {
                case TypeCheckState.Error:
                    return def;
                case TypeCheckState.Nil:
                    return null;
                case TypeCheckState.Empty:
                    return 0;
            }

            return ReadULong();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        public uint? ReadUIntNil(uint? def = null)
        {
            switch (IsDestType(TsonDataType.Float))
            {
                case TypeCheckState.Error:
                    return def;
                case TypeCheckState.Nil:
                    return null;
                case TypeCheckState.Empty:
                    return 0;
            }

            return ReadUInt();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        public int? ReadIntNil(int? def = null)
        {
            switch (IsDestType(TsonDataType.Float))
            {
                case TypeCheckState.Error:
                    return def;
                case TypeCheckState.Nil:
                    return null;
                case TypeCheckState.Empty:
                    return 0;
            }

            return ReadInt();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        public short? ReadShortNil(short? def = null)
        {
            switch (IsDestType(TsonDataType.Float))
            {
                case TypeCheckState.Error:
                    return def;
                case TypeCheckState.Nil:
                    return null;
                case TypeCheckState.Empty:
                    return 0;
            }

            return ReadShort();
        }

        /// <summary>
        ///     读取内容
        /// </summary>
        /// <returns> </returns>
        public ushort? ReadUShortNil(ushort? def = null)
        {
            switch (IsDestType(TsonDataType.Float))
            {
                case TypeCheckState.Error:
                    return def;
                case TypeCheckState.Nil:
                    return null;
                case TypeCheckState.Empty:
                    return 0;
            }

            return ReadUShort();
        }

        /// <summary>
        ///     读取GUID
        /// </summary>
        /// <returns></returns>
        public Guid? ReadGuidNil(Guid? def = null)
        {
            switch (IsDestType(TsonDataType.Float))
            {
                case TypeCheckState.Error:
                    return def;
                case TypeCheckState.Nil:
                    return null;
                case TypeCheckState.Empty:
                    return Guid.Empty;
            }

            return ReadGuid();
        }

        #endregion

        #region Array

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<bool> ReadBoolList(bool emptyNil = false)
        {
            return ReadList(ReadBool, TsonDataType.Bool);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<byte> ReadBytelList(bool emptyNil = false)
        {
            return ReadList(ReadByte, TsonDataType.Byte);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<sbyte> ReadSBytelList(bool emptyNil = false)
        {
            return ReadList(ReadSByte, TsonDataType.SByte);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<short> ReadShortlList(bool emptyNil = false)
        {
            return ReadList(ReadShort, TsonDataType.Short);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<ushort> ReadUShortlList(bool emptyNil = false)
        {
            return ReadList(ReadUShort, TsonDataType.UShort);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<int> ReadIntList(bool emptyNil = false)
        {
            return ReadList(ReadInt, TsonDataType.Int);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<uint> ReadUIntList(bool emptyNil = false)
        {
            return ReadList(ReadUInt, TsonDataType.UInt);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<long> ReadLongList(bool emptyNil = false)
        {
            return ReadList(ReadLong, TsonDataType.Long);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<ulong> ReadULongList(bool emptyNil = false)
        {
            return ReadList(ReadULong, TsonDataType.ULong);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<float> ReadFloatList(bool emptyNil = false)
        {
            return ReadList(ReadFloat, TsonDataType.Float);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<double> ReadDoubleList(bool emptyNil = false)
        {
            return ReadList(ReadDouble, TsonDataType.Double);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<decimal> ReadDecimalList(bool emptyNil = false)
        {
            return ReadList(ReadDecimal, TsonDataType.Decimal);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<string> ReadStringList(bool emptyNil = false)
        {
            return ReadList(ReadString, TsonDataType.String);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<DateTime> ReadDateTimeList(bool emptyNil = false)
        {
            return ReadList(ReadDateTime, TsonDataType.DateTime);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<Guid> ReadGuidList(bool emptyNil = false)
        {
            return ReadList(ReadGuid, TsonDataType.Guid);
        }

        #endregion
    }
}