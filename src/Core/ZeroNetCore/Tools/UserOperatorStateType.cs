namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    /// 一次路由执行状态
    /// </summary>
    public enum UserOperatorStateType
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
        /// 逻辑错误
        /// </summary>
        LogicalError,
        /// <summary>
        /// 本地异常
        /// </summary>
        LocalException,
        /// <summary>
        /// 本地错误
        /// </summary>
        LocalError,
        /// <summary>
        /// 远程错误
        /// </summary>
        RemoteError,
        /// <summary>
        /// 远程异常
        /// </summary>
        RemoteException,
        /// <summary>
        /// 网络错误
        /// </summary>
        NetWorkError,
        /// <summary>
        /// 拒绝服务
        /// </summary>
        Unavailable,
        /// <summary>
        /// 未准备好
        /// </summary>
        NotReady,
        /// <summary>
        /// 页面不存在
        /// </summary>
        NotFind,
        /// <summary>
        /// 非法请求
        /// </summary>
        DenyAccess
    }
}