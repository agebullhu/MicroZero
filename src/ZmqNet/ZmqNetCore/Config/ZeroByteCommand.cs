namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// Zmq帮助类
    /// </summary>
    public static class ZeroByteCommand
    {
        /// <summary>
        /// 标准调用
        /// </summary>
        public const byte General = (byte)0;

        /// <summary>
        /// 等待结果
        /// </summary>
        public const byte GetGlobalId = (byte)'>';

        /// <summary>
        /// 等待结果
        /// </summary>
        public const byte Waiting = (byte)'#';

        /// <summary>
        /// 查找结果
        /// </summary>
        public const byte Find = (byte)'%';

        /// <summary>
        /// 关闭结果
        /// </summary>
        public const byte Close = (byte)'-';

        /// <summary>
        /// Ping
        /// </summary>
        public const byte Ping = (byte)'*';

        /// <summary>
        /// 心跳加入
        /// </summary>
        public const byte HeartJoin = (byte)'@';

        /// <summary>
        /// 心跳进行
        /// </summary>
        public const byte HeartPitpat = (byte)'$';

        /// <summary>
        /// 心跳退出
        /// </summary>
        public const byte HeartLeft = (byte)'!';
    }
}