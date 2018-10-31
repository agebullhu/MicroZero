using System.Text;
using Gboxt.Common.DataModel;

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
            return !UseIpc 
                ? $"tcp://{ZeroApplication.Config.ZeroAddress}:{port}" 
                : $"ipc://{ZeroApplication.Config.RootPath}/ipc/{station}_req.ipc";
        }
        /// <summary>
        /// 格式化地址
        /// </summary>
        /// <param name="station"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string GetSubscriberAddress(string station, int port)
        {
            return !UseIpc 
                ? $"tcp://{ZeroApplication.Config.ZeroAddress}:{port}" 
                : $"ipc://{ZeroApplication.Config.RootPath}/ipc/{station}_sub.ipc";
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
            //sb.Append(ZeroApplication.Config.ServiceName);
            //sb.Append("-");
            sb.Append(ZeroApplication.Config.ShortName);
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
            sb.Append(RandomOperate.Generate(4));
            return sb.ToString();
        }
    }
}