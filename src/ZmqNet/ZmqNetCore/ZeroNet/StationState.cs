namespace ZmqNet.Rpc.Core.ZeroNet
{
    /// <summary>
    /// 站点状态
    /// </summary>
    public enum StationState
    {
        /// <summary>
        /// 无，刚构造
        /// </summary>
        None,
        /// <summary>
        /// 正在启动
        /// </summary>
        Start,
        /// <summary>
        /// 错误状态
        /// </summary>
        Failed,
        /// <summary>
        /// 正在运行
        /// </summary>
        Run,
        /// <summary>
        /// 已暂停
        /// </summary>
        Pause,
        /// <summary>
        /// 将要关闭
        /// </summary>
        Closing,
        /// <summary>
        /// 已关闭
        /// </summary>
        Closed,
        /// <summary>
        /// 已销毁，析构已调用
        /// </summary>
        Destroy
    }
}