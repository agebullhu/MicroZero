using Gboxt.Common.DataModel;

namespace Gboxt.Common.DataModel.ZeroNet
{
    /// <summary>
    /// 实体事件节点
    /// </summary>
    public class EntityEventItem
    {
        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DbName { get; set; }

        /// <summary>
        /// 实体名称
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// 事件类型
        /// </summary>
        public DataOperatorType EvenType { get; set; }

        /// <summary>
        /// 事件值
        /// </summary>
        public string Value { get; set; }
    }
}