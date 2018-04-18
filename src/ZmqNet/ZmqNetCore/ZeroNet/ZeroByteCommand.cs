namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// Zmq帮助类
    /// </summary>
    public static class ZeroByteCommand
    {
        /// <summary>
        /// 工作站点加入
        /// </summary>
        public const string WorkerJoin = "@";
        /// <summary>
        /// 工作站点加入
        /// </summary>
        public const string WorkerLeft = "!";
        /// <summary>
        /// 工作站点等待工作
        /// </summary>
        public const string WorkerListen = "$";

        /// <summary>
        /// 心跳加入
        /// </summary>
        public const string HeartJoin = "@";

        /// <summary>
        /// 心跳进行
        /// </summary>
        public const string HeartPitpat = "$";

        /// <summary>
        /// 心跳退出
        /// </summary>
        public const string HeartLeft = "!";
    }
}