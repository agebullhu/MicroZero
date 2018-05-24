using System;
using System.Text;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     命名辅助类
    /// </summary>
    public static class ZeroIdentityHelper
    {
        /// <summary>
        /// 是否本机
        /// </summary>
        /// <returns></returns>
        public static bool IsLocalhost => string.IsNullOrWhiteSpace(ZeroApplication.Config.ZeroAddress) ||
                                          ZeroApplication.Config.ZeroAddress == "127.0.0.1" ||
                                          ZeroApplication.Config.ZeroAddress == "::1" ||
                                          ZeroApplication.Config.ZeroAddress.Equals("localhost", StringComparison.OrdinalIgnoreCase);
        /// <summary>
        /// 格式化地址
        /// </summary>
        /// <param name="station"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string GetRemoteAddress(string station, int port)
        {
            return /*IsLocalhost
                ? $"ipc:///usr/zero/{station}.ipc"
                : */$"tcp://{ZeroApplication.Config.ZeroAddress}:{port}";
        }

        /// <summary>
        /// 格式化身份名称
        /// </summary>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public static string CreateRealName(params string[] ranges)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('+');//IsLocalhost ? "-" : "+"
            sb.Append(ZeroApplication.Config.ServiceName);
            sb.Append("-");
            sb.Append(ZeroApplication.Config.ShortName ?? ZeroApplication.Config.StationName);
            if (ZeroApplication.Config.ServiceKey != "*")
            {
                sb.Append("-");
                sb.Append(ZeroApplication.Config.ServiceKey);
            }
            foreach (var range in ranges)
            {
                if (range == null)
                    continue;
                sb.Append("-");
                sb.Append(range);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 格式化身份名称
        /// </summary>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public static byte[] ToZeroIdentity(params string[] ranges)
        {
            return CreateRealName(ranges).ToAsciiBytes();
        }
    }
}