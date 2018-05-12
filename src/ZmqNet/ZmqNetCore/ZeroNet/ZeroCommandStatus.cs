namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 命令状态
    /// </summary>
    public enum ZeroCommandStatus
    {
        /// <summary>
        /// 无状态
        /// </summary>
        None,
        /// <summary>
        /// 成功
        /// </summary>
        Success,
        /// <summary>
        /// 找不到
        /// </summary>
        NoFind,
        /// <summary>
        /// 不支持
        /// </summary>
        NoSupper,
        /// <summary>
        /// 出错
        /// </summary>
        Error,
        /// <summary>
        /// 本地异常
        /// </summary>
        Exception,
        /// <summary>
        /// 本地未启动
        /// </summary>
        NoRun
    }
}