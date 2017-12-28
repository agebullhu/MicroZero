namespace Agebull.Common.DataModel
{
    /// <summary>
    /// 表示实现自助TSON操作的类
    /// </summary>
    public interface ITson
    {
        /// <summary>
        /// 类型ID
        /// </summary>
        int TypeId { get; }

        /// <summary>
        /// 安全的缓存长度
        /// </summary>
        int SafeBufferLength { get; }

        /// <summary>
        /// 从TSON反序列化
        /// </summary>
        void Deserialize(TsonDeserializer deserializer);

        /// <summary>
        /// 序列化到Tson
        /// </summary>
        void Serialize(TsonSerializer serializer);
    }
}
