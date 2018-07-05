namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 站点类型
    /// </summary>
    public enum ZeroStationType 
    {
        /// <summary>
        /// 调度器
        /// </summary>
        Dispatcher = 1,
        /// <summary>
        /// 广播
        /// </summary>
        Publish = 2,
        /// <summary>
        /// API
        /// </summary>
        Api = 3,
        /// <summary>
        /// 投票器
        /// </summary>
        Vote = 4,
        /// <summary>
        /// 计划任务
        /// </summary>
        Plan = 0xFF
    }
}