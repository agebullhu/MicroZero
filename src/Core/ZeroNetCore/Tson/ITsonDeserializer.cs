using System;
using System.Collections.Generic;
using Agebull.Common.Base;

namespace Agebull.Common.Tson
{
    /// <summary>
    ///  Tson对象范围
    /// </summary>
    public sealed class TsonObjectScope : ScopeBase
    {
        private readonly ITsonDeserializer deserializer;
        public readonly byte Ver;
        public readonly TsonDataType DataType;

        private TsonObjectScope(ITsonDeserializer s)
        {
            deserializer = s;
            DataType = s.Begin(out Ver);
        }
        /// <summary>
        /// 生成范围
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static TsonObjectScope CreateScope(ITsonDeserializer s) => new TsonObjectScope(s);

        /// <summary>清理资源</summary>
        protected override void OnDispose()
        {
            deserializer.End();
        }
    }

    public interface ITsonDeserializer : IDisposable
    {

        /// <summary>
        /// 版本
        /// </summary>
        /// <returns></returns>
        byte Ver { get; }
        /// <summary>
        /// 类型
        /// </summary>
        /// <returns></returns>
        TsonDataType DataType { get; }

        /// <summary>
        /// 当前位置的值
        /// </summary>
        /// <returns></returns>
        byte Current { get; }

        /// <summary>
        /// 是否处于结束位置
        /// </summary>
        bool IsEof { get; }

        /// <summary>
        /// 是否处于错误位置
        /// </summary>
        bool IsBad { get; }

        /// <summary>
        /// 读取版本
        /// </summary>
        /// <returns></returns>
        byte ReadVer();

        /// <summary>
        /// 开始子级
        /// </summary>
        /// <returns></returns>
        TsonDataType Begin(out byte ver);
        /// <summary>
        /// 结束子级
        /// </summary>
        /// <returns></returns>
        void End();
        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        unsafe TsonDataType ReadType();

        /// <summary>
        /// 读取字段索引
        /// </summary>
        /// <returns></returns>
        int ReadIndex();

        /// <summary>
        /// 读取长度
        /// </summary>
        /// <returns></returns>
        int ReadLen();

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        unsafe float ReadFloat();

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        unsafe double ReadDouble();

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        unsafe decimal ReadDecimal();

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        bool ReadBool();

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        byte ReadByte();

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        unsafe sbyte ReadSByte();

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        DateTime ReadDateTime();

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        unsafe long ReadLong();

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        unsafe ulong ReadULong();

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        unsafe uint ReadUInt();

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        unsafe int ReadInt();

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        unsafe short ReadShort();

        /// <summary>
        ///   读取内容
        /// </summary>
        /// <returns> </returns>
        unsafe ushort ReadUShort();

        /// <summary>
        /// 读取文本
        /// </summary>
        /// <returns></returns>
        string ReadString();

        /// <summary>
        /// 读取GUID
        /// </summary>
        /// <returns></returns>
        Guid ReadGuid();


        /// <summary>
        /// 读一个列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="read"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        void ReadList<T>(IList<T> array, Func<T> read);

        /// <summary>
        /// 读一个数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="read"></param>
        /// <returns></returns>
        T[] ReadArray<T>(Func<T> read);


        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="read"></param>
        /// <returns></returns>
        List<T> ReadList<T>(Func<T> read);

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <typeparam name="TList"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="read"></param>
        /// <returns></returns>
        TList ReadList<TList, TValue>(Func<TValue> read)
            where TList : class, IList<TValue>, new();


        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        List<bool> ReadBoolList(bool emptyNil = false);

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        List<byte> ReadBytelList(bool emptyNil = false);

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        List<sbyte> ReadSBytelList(bool emptyNil = false);

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        List<short> ReadShortlList(bool emptyNil = false);

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        List<ushort> ReadUShortlList(bool emptyNil = false);

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        List<int> ReadIntList(bool emptyNil = false);

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        List<uint> ReadUIntList(bool emptyNil = false);

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        List<long> ReadLongList(bool emptyNil = false);

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        List<ulong> ReadULongList(bool emptyNil = false);

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        List<float> ReadFloatList(bool emptyNil = false);

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        List<double> ReadDoubleList(bool emptyNil = false);

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        List<decimal> ReadDecimalList(bool emptyNil = false);

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        List<string> ReadStringList(bool emptyNil = false);

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        List<DateTime> ReadDateTimeList(bool emptyNil = false);

        /// <summary>
        ///     读一个列表
        /// </summary>
        /// <param name="emptyNil">空值是否为null,否则返回空列表</param>
        /// <returns></returns>
        List<Guid> ReadGuidList(bool emptyNil = false);
    }
}