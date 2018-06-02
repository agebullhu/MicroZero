namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 限速模式（0 按线程数限制 1 按等待数限制）
    /// </summary>
    public enum SpeedLimitType
    {
        /// <summary>
        /// 单线程数处理
        /// </summary>
        Single,
        /// <summary>
        /// 按线程数限制
        /// </summary>
        ThreadCount,
        /// <summary>
        /// 按等待数限制
        /// </summary>
        WaitCount
    }
}