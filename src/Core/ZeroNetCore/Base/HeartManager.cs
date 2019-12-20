using System.Threading.Tasks;

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
        public async Task<bool> HeartLeft(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && await ByteCommand(ZeroByteCommand.HeartLeft, station, realName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public async Task<bool> HeartReady(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && await ByteCommand(ZeroByteCommand.HeartReady, station, realName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public async Task<bool> HeartJoin(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && await ByteCommand(ZeroByteCommand.HeartJoin, station, realName, ZeroApplication.Config.LocalIpAddress);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public async Task<bool> Heartbeat(string station, string realName)
        {
            return ZeroApplication.ZerCenterIsRun && await ByteCommand(ZeroByteCommand.HeartPitpat, station, realName);
        }

    }
}