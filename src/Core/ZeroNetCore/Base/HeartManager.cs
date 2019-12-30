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
        public Task<bool> HeartLeft(string station, string realName)
        {
            return !ZeroApplication.ZerCenterIsRun ? Task.FromResult(false): ByteCommand(ZeroByteCommand.HeartLeft, station, realName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public Task<bool> HeartReady(string station, string realName)
        {
            return !ZeroApplication.ZerCenterIsRun ? Task.FromResult(false): ByteCommand(ZeroByteCommand.HeartReady, station, realName);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public Task<bool> HeartJoin(string station, string realName)
        {
            return !ZeroApplication.ZerCenterIsRun ? Task.FromResult(false): ByteCommand(ZeroByteCommand.HeartJoin, station, realName, ZeroApplication.Config.LocalIpAddress);
        }

        /// <summary>
        ///     连接到
        /// </summary>
        public Task<bool> Heartbeat(string station, string realName)
        {
            return !ZeroApplication.ZerCenterIsRun ? Task.FromResult(false): ByteCommand(ZeroByteCommand.HeartPitpat, station, realName);
        }

    }
}