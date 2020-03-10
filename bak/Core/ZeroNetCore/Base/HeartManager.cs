namespace Agebull.MicroZero
{
    /// <summary>
    /// 心跳
    /// </summary>
    public class HeartManager : ZSimpleCommand
    {
        /// <summary>
        ///     连接到
        /// </summary>
        public bool HeartLeft(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && ByteCommand(ZeroByteCommand.HeartLeft, station, realName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public bool HeartReady(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && ByteCommand(ZeroByteCommand.HeartReady, station, realName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public bool HeartJoin(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && ByteCommand(ZeroByteCommand.HeartJoin, station, realName, ZeroApplication.Config.LocalIpAddress);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public bool Heartbeat(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && ByteCommand(ZeroByteCommand.HeartPitpat, station, realName);
        }

    }
}