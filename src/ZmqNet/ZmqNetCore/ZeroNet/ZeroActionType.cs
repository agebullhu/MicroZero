namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 请求行为类型
    /// </summary>
    public enum ZeroActionType : sbyte
    {
        /// <summary>
        /// 无特殊说明
        /// </summary>
        None = 0,
        /// <summary>
        /// 工作站点加入
        /// </summary>
        WorkerJoin = 1,
        /// <summary>
        /// 工作站点退出
        /// </summary>
        WorkerLeft = 2,
        /// <summary>
        /// 工作站点等待工作
        /// </summary>
        WrokerListen = 3
    }
}