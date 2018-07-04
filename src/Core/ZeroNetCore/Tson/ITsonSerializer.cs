using System;
using System.Collections.Generic;
using Agebull.Common.Base;

namespace Agebull.Common.Tson
{
    /// <summary>
    ///  Tson对象范围
    /// </summary>
    public sealed class TsonObjectSerializeScope : ScopeBase
    {
        private readonly ITsonSerializer serializer;

        private TsonObjectSerializeScope(ITsonSerializer s, TsonDataType type)
        {
            serializer = s;
            s.Begin(type);
        }

        /// <summary>
        /// 生成范围
        /// </summary>
        /// <param name="s"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ScopeBase CreateScope(ITsonSerializer s, TsonDataType type = TsonDataType.Object) => new TsonObjectSerializeScope(s, type);

        /// <summary>清理资源</summary>
        protected override void OnDispose()
        {
            serializer.End();
        }
    }
    /// <summary>
    /// 表示一个Tson序列化器
    /// </summary>
    public interface ITsonSerializer : IDisposable
    {
        /// <summary>
        ///     开始
        /// </summary>
        void Begin(TsonDataType type);

        /// <summary>
        ///     结束并
        /// </summary>
        /// <returns></returns>
        void End();

        /// <summary>
        /// 结束
        /// </summary>
        /// <returns></returns>
        byte[] Close();

        /// <summary>
        /// 写入长度
        /// </summary>
        /// <param name="len"></param>
        void WriteLen(int len);

        /// <summary>
        /// 写入序号
        /// </summary>
        /// <param name="idx"></param>
        void WriteIndex(int idx);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        void Write(byte index, bool value);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        void Write(byte index, byte value);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        void Write(byte index, DateTime value);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        void Write(byte index, double value);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        void Write(byte index, float value);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        void Write(byte index, decimal value);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        void Write(byte index, string value);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        void Write(byte index, Guid value);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        void Write(byte index, long value);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        void Write(byte index, ulong value);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        void Write(byte index, int value);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        void Write(byte index, uint value);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        void Write(byte index, short value);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        void Write(byte index, ushort value);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        /// <param name="type">类型</param>
        /// <param name="write">写入方法 </param>
        void Write<T>(byte index, List<T> value, TsonDataType type, Action<T> write);

        /// <summary>
        ///     写入值
        /// </summary>
        /// <param name="index"> 序号</param>
        /// <param name="value">值 </param>
        /// <param name="type">类型</param>
        /// <param name="write">写入方法 </param>
        void Write<T>(byte index, T[] value, TsonDataType type, Action<T> write);

    }
}