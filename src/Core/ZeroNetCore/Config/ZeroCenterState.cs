namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///   站点中心状态
    /// </summary>
    public enum ZeroCenterState
    {
        /// <summary>
        ///  无，刚构造
        /// </summary>
        None,
        /// <summary>
        ///  重新启动
        /// </summary>
        ReStart,
        /// <summary>
        ///  正在启动
        /// </summary>
        Start,
        /// <summary>
        ///  正在运行
        /// </summary>
        Run,
        /// <summary>
        ///  已暂停
        /// </summary>
        Pause,
        /// <summary>
        ///  错误状态
        /// </summary>
        Failed,
        /// <summary>
        ///  将要关闭
        /// </summary>
        Closing,
        /// <summary>
        ///  已关闭
        /// </summary>
        Closed,
        /// <summary>
        ///  已销毁，析构已调用
        /// </summary>
        Destroy,
        /// <summary>
        ///  已关停
        /// </summary>
        Stop,
        /// <summary>
        ///  删除
        /// </summary>
        Remove
    };
}