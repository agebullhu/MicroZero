namespace Agebull.Common.Tson
{
    /// <summary>
    /// TSON操作器
    /// </summary>
    /// <typeparam name="TData">操作的数据类型</typeparam>
    public interface ITsonOperator<in TData>
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="value"></param>
        void ToTson(ITsonSerializer serializer, TData value);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="value"></param>
        void FromTson(ITsonDeserializer serializer, TData value);
    }
}