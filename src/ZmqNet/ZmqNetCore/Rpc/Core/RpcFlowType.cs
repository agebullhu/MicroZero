namespace Agebull.Zmq.Rpc
{
    /// <summary>
    ///     RPC流程状态类型
    /// </summary>
    public enum RpcFlowType
    {
        /// <summary>
        ///     未初始化
        /// </summary>
        None,

        /// <summary>
        ///     正在初始化全局环境
        /// </summary>
        GlobalInitializing,

        /// <summary>
        ///     全局环境初始化成功
        /// </summary>
        GlobalInitialized,

        /// <summary>
        ///     正在登录(显示登录页面)
        /// </summary>
        OnLogin,

        /// <summary>
        ///     正在进行登录
        /// </summary>
        Logining,

        /// <summary>
        ///     登录成功
        /// </summary>
        LoginSucceed,

        /// <summary>
        ///     正在初始化用户环境
        /// </summary>
        UserInitializing,

        /// <summary>
        ///     用户环境初始化完成
        /// </summary>
        UserInitialized,

        /// <summary>
        ///     用户正在浏览中
        /// </summary>
        Browsing,

        /// <summary>
        ///     下单事务处理中
        /// </summary>
        OrderProcessing,

        /// <summary>
        ///     辙单事务处理中
        /// </summary>
        CancelProcessing,

        /// <summary>
        ///     平仓事务处理中
        /// </summary>
        CoverProcessing,

        /// <summary>
        ///     系统强制事务处理中
        /// </summary>
        EnforceProcessing,

        /// <summary>
        ///     登出中
        /// </summary>
        Exiting,

        /// <summary>
        ///     系统已关闭
        /// </summary>
        Exited,

        /// <summary>
        ///     系统销毁
        /// </summary>
        Destroied
    }
}