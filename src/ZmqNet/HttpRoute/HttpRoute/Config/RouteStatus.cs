namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 一次路由执行状态
    /// </summary>
    public enum RouteStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        None,
        /// <summary>
        /// 缓存
        /// </summary>
        Cache,
        /// <summary>
        /// 远程错误
        /// </summary>
        RemoteError,
        /// <summary>
        /// 非法格式
        /// </summary>
        FormalError,
        /// <summary>
        /// 转向
        /// </summary>
        HttpRedirect,
        /// <summary>
        /// Http的OPTION协商
        /// </summary>
        HttpOptions,
        /// <summary>
        /// 非法请求
        /// </summary>
        DenyAccess
    }
}