namespace Agebull.MicroZero
{
    /// <summary>
    ///     命名辅助类
    /// </summary>
    internal static class ZeroAddressHelper
    {

        /*
        /// <summary>
        /// 是否本机
        /// </summary>
        /// <returns></returns>
        public static bool UseIpc { get; set; }

        string.IsNullOrWhiteSpace(ZeroApplication.Config.ZeroAddress) ||
                                          ZeroApplication.Config.ZeroAddress == "127.0.0.1" ||
                                          ZeroApplication.Config.ZeroAddress == "::1" ||
                                          ZeroApplication.Config.ZeroAddress.Equals("localhost", StringComparison.OrdinalIgnoreCase)
     

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
        /// 格式化地址
        /// </summary>
        /// <param name="station"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string GetAddress(string station, int port)
        {
            var cfg = ZeroApplication.Config[station];
            if (cfg == null)
                return null;
            if (string.IsNullOrWhiteSpace(cfg.Address))
                return $"tcp://{ZeroApplication.Config.ZeroAddress}:{port}";
            return $"tcp://{cfg.Address}:{port}";
        }*/
    }
}