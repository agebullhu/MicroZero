
namespace Agebull.EntityModel
{
    /// <summary>
    /// 下拉列表节点
    /// </summary>
    public class ComboItem<TValue>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public TValue value { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name => name;
        /// <summary>
        /// 值
        /// </summary>
        public TValue Value => value;
    }
}