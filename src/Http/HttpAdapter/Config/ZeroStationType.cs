namespace Agebull.MicroZero
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
        Notify = 2,
        /// <summary>
        /// API
        /// </summary>
        Api = 3,
        /// <summary>
        /// 投票器
        /// </summary>
        Vote = 4,
        /// <summary>
        /// 定向路由API
        /// </summary>
        RouteApi = 5,
        /// <summary>
        /// 队列任务
        /// </summary>
        Queue = 6,
        /// <summary>
        /// 流程跟踪
        /// </summary>
        Trace = 0xFD,
        /// <summary>
        /// 代理
        /// </summary>
        Proxy = 0xFE,
        /// <summary>
        /// 计划任务
        /// </summary>
        Plan = 0xFF
    }
}