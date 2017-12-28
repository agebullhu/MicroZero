namespace Agebull.Zmq.Rpc
{
    /// <summary>
    /// 网络状态
    /// </summary>
    public enum ZmqNetStatus
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 初始化完成
        /// </summary>
        Initialized = 1,
        /// <summary>
        /// 执行中
        /// </summary>
        Runing = 2,
        /// <summary>
        /// 关闭中
        /// </summary>
        Closing = 3,
        /// <summary>
        /// 已关闭
        /// </summary>
        Closed = 4,
        /// <summary>
        /// 已销毁
        /// </summary>
        Destroy = 5
    }
}