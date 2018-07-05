using System;
using System.Collections.Generic;

namespace Agebull.Common.Tson
{
    public class TsonDeserializerV1 : TsonDeserializerBase, ITsonDeserializer
    {
        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="buffer"></param>
        public TsonDeserializerV1(byte[] buffer) : base(buffer)
        {
        }

        #region Array

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<bool> ReadBoolList(bool emptyNil = false)
        {
            return ReadList(ReadBool);//Bool);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<byte> ReadBytelList(bool emptyNil = false)
        {
            return ReadList(ReadByte);//Byte);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<sbyte> ReadSBytelList(bool emptyNil = false)
        {
            return ReadList(ReadSByte);//SByte);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<short> ReadShortlList(bool emptyNil = false)
        {
            return ReadList(ReadShort);//Short);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<ushort> ReadUShortlList(bool emptyNil = false)
        {
            return ReadList(ReadUShort);//UShort);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<int> ReadIntList(bool emptyNil = false)
        {
            return ReadList(ReadInt);//Int);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<uint> ReadUIntList(bool emptyNil = false)
        {
            return ReadList(ReadUInt);//UInt);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<long> ReadLongList(bool emptyNil = false)
        {
            return ReadList(ReadLong);//Long);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<ulong> ReadULongList(bool emptyNil = false)
        {
            return ReadList(ReadULong);//ULong);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<float> ReadFloatList(bool emptyNil = false)
        {
            return ReadList(ReadFloat);//Float);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<double> ReadDoubleList(bool emptyNil = false)
        {
            return ReadList(ReadDouble);//Double);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<decimal> ReadDecimalList(bool emptyNil = false)
        {
            return ReadList(ReadDecimal);//Decimal);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<string> ReadStringList(bool emptyNil = false)
        {
            return ReadList(ReadString);//String);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<DateTime> ReadDateTimeList(bool emptyNil = false)
        {
            return ReadList(ReadDateTime);//DateTime);
        }

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        public List<Guid> ReadGuidList(bool emptyNil = false)
        {
            return ReadList(ReadGuid);//Guid);
        }

        #endregion
    }
}