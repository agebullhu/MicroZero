namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 一次路由执行状态
    /// </summary>
    public enum OperatorStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        Success,
        /// <summary>
        /// 非法格式
        /// </summary>
        FormalError,
        /// <summary>
        /// 非法请求
        /// </summary>
        DenyAccess,
        /// <summary>
        /// 本地错误
        /// </summary>
        LocalError,
        /// <summary>
        /// 远程错误
        /// </summary>
        RemoteError
    }
}