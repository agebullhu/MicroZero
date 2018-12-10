namespace ZeroNet.Http.Gateway
{
    /// <summary>
    ///     一次路由执行状态
    /// </summary>
    public enum RouteStatus
    {
        /// <summary>
        ///     正常
        /// </summary>
        None,

        /// <summary>
        ///     缓存
        /// </summary>
        Cache,

        /// <summary>
        ///     Http的OPTION协商
        /// </summary>
        HttpOptions,

        /// <summary>
        ///     非法格式
        /// </summary>
        FormalError,

        /// <summary>
        ///     逻辑错误
        /// </summary>
        LogicalError,

        /// <summary>
        ///     本地异常
        /// </summary>
        LocalException,

        /// <summary>
        ///     本地错误
        /// </summary>
        LocalError,

        /// <summary>
        ///     远程错误
        /// </summary>
        RemoteError,

        /// <summary>
        ///     拒绝服务
        /// </summary>
        Unavailable,

        /// <summary>
        ///     页面不存在
        /// </summary>
        NotFind,

        /// <summary>
        ///     非法请求
        /// </summary>
        DenyAccess
    }
}