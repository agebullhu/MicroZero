namespace Agebull.EntityModel
{
    /// <summary>
    /// 通知对象的状态类型
    /// </summary>
    public enum NotificationStatusType
    {
        /// <summary>
        /// 原始状态
        /// </summary>
        None,
        /// <summary>
        /// 已刷新
        /// </summary>
        Refresh,
        /// <summary>
        /// 已修改
        /// </summary>
        Modified,

        /// <summary>
        /// 新增完成
        /// </summary>
        Added,

        /// <summary>
        /// 保存完成
        /// </summary>
        Saved,

        /// <summary>
        /// 已实际刪除
        /// </summary>
        Deleted,
        /// <summary>
        /// 正在内部操作
        /// </summary>
        Inner,
        /// <summary>
        /// 正在同步
        /// </summary>
        Synchronous,
        /// <summary>
        /// 可以重做
        /// </summary>
        CanReDo,
        /// <summary>
        /// 可以撤销
        /// </summary>
        CanUnDo,
        /// <summary>
        /// 重做
        /// </summary>
        ReDo,
        /// <summary>
        /// 撤销
        /// </summary>
        UnDo
    }
}