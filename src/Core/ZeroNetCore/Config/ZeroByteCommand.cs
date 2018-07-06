namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// Zmq帮助类
    /// </summary>
    public enum ZeroByteCommand : byte
    {
        /// <summary>
        /// 标准调用
        /// </summary>
        General = (byte)1,

        /// <summary>
        /// 计划任务
        /// </summary>
        Plan = (byte)2,

        /// <summary>
        /// 代理执行
        /// </summary>
        Proxy = (byte)3,

        /// <summary>
        /// 等待结果
        /// </summary>
        GetGlobalId = (byte)'>',

        /// <summary>
        /// 等待结果
        /// </summary>
        Waiting = (byte)'#',

        /// <summary>
        /// 查找结果
        /// </summary>
        Find = (byte)'%',

        /// <summary>
        /// 关闭结果
        /// </summary>
        Close = (byte)'-',

        /// <summary>
        /// Ping
        /// </summary>
        Ping = (byte)'*',

        /// <summary>
        /// 心跳加入
        /// </summary>
        HeartJoin = (byte)'J',

        /// <summary>
        /// 心跳加入
        /// </summary>
        HeartReady = (byte)'R',

        /// <summary>
        /// 心跳进行
        /// </summary>
        HeartPitpat = (byte)'P',

        /// <summary>
        /// 心跳退出
        /// </summary>
        HeartLeft = (byte)'L',
    }
}