namespace Agebull.EntityModel
{
    /// <summary>
    /// 工作模式
    /// </summary>
    public enum WorkModel
    {
        /// <summary>
        /// 不确定
        /// </summary>
        None,
        /// <summary>
        /// 正在设计
        /// </summary>
        Design,
        /// <summary>
        /// 正在生成代码
        /// </summary>
        Coder,
        /// <summary>
        /// 正在呈现
        /// </summary>
        Show,
        /// <summary>
        /// 保存
        /// </summary>
        Saving,
        /// <summary>
        /// 载入
        /// </summary>
        Loding,
        /// <summary>
        /// 修复
        /// </summary>
        Repair
    }
}