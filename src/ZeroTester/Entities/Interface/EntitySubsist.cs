namespace Agebull.EntityModel
{
    /// <summary>
    /// 对象生态状态
    /// </summary>
    public enum EntitySubsist
    {
        /// <summary>
        /// 未知,只读对象被识别为存在
        /// </summary>
        None,
        /// <summary>
        /// 新增未保存
        /// </summary>
        Adding,
        /// <summary>
        /// 新增已保存,相当于Exist,但可用于处理新增保存的后期事件
        /// </summary>
        Added,
        /// <summary>
        /// 已存在
        /// </summary>
        Exist,
        /// <summary>
        /// 将要删除
        /// </summary>
        Deleting,
        /// <summary>
        /// 已经删除
        /// </summary>
        Deleted
    }
}