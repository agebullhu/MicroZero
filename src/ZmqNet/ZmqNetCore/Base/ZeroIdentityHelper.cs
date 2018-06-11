using System;
using System.IO;
using System.Text;
using Agebull.Common.Configuration;
using Gboxt.Common.DataModel;
using Microsoft.Extensions.Configuration;

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
        public static bool UseIpc {get ; set; }
        
        /*string.IsNullOrWhiteSpace(ZeroApplication.Config.ZeroAddress) ||
                                          ZeroApplication.Config.ZeroAddress == "127.0.0.1" ||
                                          ZeroApplication.Config.ZeroAddress == "::1" ||
                                          ZeroApplication.Config.ZeroAddress.Equals("localhost", StringComparison.OrdinalIgnoreCase)*/
        /// <summary>
        /// 格式化地址
        /// </summary>
        /// <param name="station"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string GetRequestAddress(string station, int port)
        {
            if (!UseIpc)
                return $"tcp://{ZeroApplication.Config.ZeroAddress}:{port}";
            var path = Path.GetDirectoryName(ConfigurationManager.Root.GetValue("contentRoot", Environment.CurrentDirectory));
            return $"ipc://{path}/ipc/{station}_req.ipc";
        }
        /// <summary>
        /// 格式化地址
        /// </summary>
        /// <param name="station"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string GetSubscriberAddress(string station, int port)
        {
            if (!UseIpc)
                return $"tcp://{ZeroApplication.Config.ZeroAddress}:{port}";
            var path = Path.GetDirectoryName(ConfigurationManager.Root.GetValue("contentRoot", Environment.CurrentDirectory));
            return $"ipc://{path}/ipc/{station}_sub.ipc";
        }
        /// <summary>
        /// 格式化地址
        /// </summary>
        /// <param name="station"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string GetWorkerAddress(string station, int port)
        {
            return $"tcp://{ZeroApplication.Config.ZeroAddress}:{port}";
        }
        /// <summary>
        /// 格式化身份名称
        /// </summary>
        /// <param name="isService"></param>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public static string CreateRealName(bool isService, params string[] ranges)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(UseIpc ? '-' : '+');
            sb.Append(isService ? "<" : ">");
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
            sb.Append("-");
            sb.Append(RandomOperate.Generate(6));
            return sb.ToString();
        }

        /// <summary>
        /// 格式化身份名称
        /// </summary>
        /// <param name="isService"></param>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public static byte[] ToZeroIdentity(bool isService, params string[] ranges)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(UseIpc ? "-" : "+");
            sb.Append(isService ? "<" : ">");
            sb.Append(ZeroApplication.Config.ServiceName);
            sb.Append("-");
            sb.Append(ZeroApplication.Config.ShortName ?? ZeroApplication.Config.StationName);
            //if (ZeroApplication.Config.ServiceKey != "*")
            //{
            //    sb.Append("-");
            //    sb.Append(ZeroApplication.Config.ServiceKey);
            //}
            //foreach (var range in ranges)
            //{
            //    if (range == null)
            //        continue;
            //    sb.Append("-");
            //    sb.Append(range);
            //}
            sb.Append("-");
            sb.Append(RandomOperate.Generate(8));
            return sb.ToString().ToAsciiBytes();
        }
    }
}