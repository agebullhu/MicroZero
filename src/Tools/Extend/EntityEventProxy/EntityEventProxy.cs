using Agebull.ZeroNet.PubSub;

namespace Gboxt.Common.DataModel.ZeroNet
{
    /// <summary>
    /// 实体事件代理,实现网络广播功能
    /// </summary>
    internal class EntityEventProxy : SignlePublisher<EntityEventItem>, IEntityEventProxy
    {
        /// <summary>
        /// 防止构造
        /// </summary>
        EntityEventProxy()
        {

        }
        /// <summary>
        /// 单例
        /// </summary>
        internal static EntityEventProxy Instance = new EntityEventProxy();


        /// <summary>状态修改事件</summary>
        /// <param name="database">数据库</param>
        /// <param name="entity">实体</param>
        /// <param name="type">状态类型</param>
        /// <param name="value">对应实体</param>
        void IEntityEventProxy.OnStatusChanged(string database, string entity, DataOperatorType type, string value)
        {
            Publish(new EntityEventItem
            {
                DbName = database,
                EntityName = entity,
                EvenType = type,
                Value = value
            });
        }

    }
}
