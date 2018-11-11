namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// Zmq帮助类
    /// </summary>
    public enum ZeroByteCommand : sbyte
    {
        /// <summary>
        /// 标准调用
        /// </summary>
        General = (sbyte)1,

        /// <summary>
        /// 计划任务
        /// </summary>
        Plan = (sbyte)2,

        /// <summary>
        /// 代理执行
        /// </summary>
        Proxy = (sbyte)3,

        /// <summary>
        /// 重启
        /// </summary>
        Restart = (sbyte)4,

        /// <summary>
        /// 全局标识
        /// </summary>
        GetGlobalId = (sbyte)'>',

        /// <summary>
        /// 等待结果
        /// </summary>
        Waiting = (sbyte)'#',

        /// <summary>
        /// 查找结果
        /// </summary>
        Find = (sbyte)'%',

        /// <summary>
        /// 关闭结果
        /// </summary>
        Close = (sbyte)'-',

        /// <summary>
        /// Ping
        /// </summary>
        Ping = (sbyte)'*',

        /// <summary>
        /// 心跳加入
        /// </summary>
        HeartJoin = (sbyte)'J',

        /// <summary>
        /// 心跳加入
        /// </summary>
        HeartReady = (sbyte)'R',

        /// <summary>
        /// 心跳进行
        /// </summary>
        HeartPitpat = (sbyte)'P',

        /// <summary>
        /// 心跳退出
        /// </summary>
        HeartLeft = (sbyte)'L',
    }
}