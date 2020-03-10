using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using ZeroMQ;
using Agebull.Common.Context;
using System.Threading;
using ZeroMQ.lib;
using Agebull.Common.Ioc;

namespace Agebull.MicroZero
{
    /// <summary>
    ///     站点应用
    /// </summary>
    partial class ZeroApplication
    {
        /// <summary>
        ///     当前应用名称
        /// </summary>
        public static string AppName { get; set; }

        /// <summary>
        ///     站点配置
        /// </summary>
        public static ZeroAppConfigRuntime Config { get; set; }

        /// <summary>
        /// 工作模式
        /// </summary>
        public static ZeroWorkModel WorkModel { get; set; }


        /// <summary>
        ///     配置校验
        /// </summary>
        private static void CheckConfig()
        {
            #region 配置组合

            var name = ConfigurationManager.Root["AppName"];
            if (name != null)
                AppName = name;
            if (string.IsNullOrWhiteSpace(AppName))
                throw new Exception("无法找到配置[AppName],请在appsettings.json或代码中设置");

            var curPath = Environment.CurrentDirectory;
            string rootPath;
            SocketOption opt = null;
            if (ConfigurationManager.Root["ASPNETCORE_ENVIRONMENT_"] == "Development")
            {
                rootPath = curPath;
            }
            else
            {
                rootPath = Path.GetDirectoryName(curPath);
                var s = ConfigurationManager.Get("Zero");
                if (s != null)
                {
                    opt = s.Child<SocketOption>("socketOption");
                }

                // ReSharper disable once AssignNullToNotNullAttribute
                var file = Path.Combine(rootPath, "config", "zero.json");
                if (File.Exists(file))
                    ConfigurationManager.Load(file);
                file = Path.Combine(rootPath, "config", $"{AppName}.json");
                if (File.Exists(file))
                    ConfigurationManager.Load(file);
            }
            ConfigurationManager.BasePath = ConfigurationManager.Root["rootPath"] = rootPath;

            var sec = ConfigurationManager.Get("Zero");
            if (sec == null)
                throw new Exception("无法找到主配置节点,路径为Zero,在zero.json或appsettings.json中设置");

            if (opt == null)
                opt = sec.Child<SocketOption>("socketOption");
            if (opt != null)
                ZSocket.Option = opt;
            ZSocket.Option.CheckOption();
            Config = new ZeroAppConfigRuntime
            {
                BinPath = curPath,
                RootPath = rootPath,
                IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            };
            var cfg = sec.Child<ZeroAppConfig>(AppName);
            if (cfg != null)
                Config.CopyByEmpty(cfg);
            cfg = sec.Child<ZeroAppConfig>("default");
            if (cfg != null)
                Config.CopyByEmpty(cfg);
            if (string.IsNullOrWhiteSpace(Config.StationName))
                Config.StationName = AppName;

            var glc = sec.Child<ZeroAppConfig>("Global");
            if (glc != null)
                Config.CopyByEmpty(glc);

            #endregion

            #region ServiceName

            if (string.IsNullOrWhiteSpace(Config.LocalIpAddress))
                Config.LocalIpAddress = GetHostIps();

            Config.ShortName = string.IsNullOrWhiteSpace(Config.ShortName)
                ? Config.StationName
                : Config.ShortName.Trim();

            if (string.IsNullOrWhiteSpace(Config.ServiceName))
            {
                try
                {
                    Config.ServiceName = Dns.GetHostName();
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    Config.ServiceName = Config.StationName;
                }
            }

            #endregion

            #region Folder

            if (Config.StationIsolate == true)
            {
                if (string.IsNullOrWhiteSpace(Config.DataFolder))
                    Config.DataFolder = IOHelper.CheckPath(rootPath, "datas", AppName);

                if (string.IsNullOrWhiteSpace(Config.LogFolder))
                    Config.LogFolder = IOHelper.CheckPath(rootPath, "logs", AppName);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Config.DataFolder))
                    Config.DataFolder = IOHelper.CheckPath(rootPath, "datas");

                if (string.IsNullOrWhiteSpace(Config.LogFolder))
                    Config.LogFolder = IOHelper.CheckPath(rootPath, "logs");

            }
            if (string.IsNullOrWhiteSpace(Config.ConfigFolder))
                Config.ConfigFolder = IOHelper.CheckPath(rootPath, "config");
            #endregion

            #region ZeroCenter

            ZeroCommandExtend.AppNameBytes = AppName.ToZeroBytes();

            Config.Master = Config.ZeroGroup[0];
            if (Config.ApiTimeout <= 1)
                Config.ApiTimeout = 60;
            //if (WorkModel == ZeroWorkModel.Bridge)
            //    return;

            //if (string.IsNullOrWhiteSpace(Config.ZeroAddress))
            //    Config.ZeroAddress = "127.0.0.1";

            //if (Config.ZeroManagePort <= 1024 || Config.ZeroManagePort >= 65000)
            //    Config.ZeroManagePort = 8000;

            //if (Config.ZeroMonitorPort <= 1024 || Config.ZeroMonitorPort >= 65000)
            //    Config.ZeroMonitorPort = 8001;

            //if (Config.PoolSize > 4096 || Config.PoolSize < 10)
            //    Config.PoolSize = 100;

            //Config.ZeroManageAddress = $"tcp://{Config.Master.Address}:{Config.Master.ManagePort}";
            //Config.ZeroMonitorAddress = $"tcp://{Config.Master.Address}:{Config.Master.MonitorPort}";

            #endregion

            ZeroCommandExtend.AppNameBytes = AppName.ToZeroBytes();
        }

        private static string GetHostIps()
        {
            var ips = new StringBuilder();
            try
            {
                var first = true;
                string hostName = Dns.GetHostName();
                LogRecorder.Trace("HostName:{0}", hostName);
                foreach (var address in Dns.GetHostAddresses(hostName))
                {
                    if (address.IsIPv4MappedToIPv6 || address.IsIPv6LinkLocal || address.IsIPv6Multicast ||
                        address.IsIPv6SiteLocal || address.IsIPv6Teredo)
                        continue;
                    var ip = address.ToString();
                    if (ip == "127.0.0.1" || ip == "127.0.1.1" || ip == "::1" || ip == "-1")
                        continue;
                    if (first)
                        first = false;
                    else
                        ips.Append(" , ");
                    ips.Append(ip);
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }

            return ips.ToString();
        }

        /// <summary>
        ///     配置校验
        /// </summary>
        public static ZeroStationOption GetApiOption(string station)
        {
            return GetStationOption(station);
        }


        /// <summary>
        ///     配置校验
        /// </summary>
        public static ZeroStationOption GetClientOption(string station)
        {
            return GetStationOption(station);
        }

        /// <summary>
        ///     配置校验
        /// </summary>
        private static ZeroStationOption GetStationOption(string station)
        {
            var sec = ConfigurationManager.Get("Zero");
            var option = sec.Child<ZeroStationOption>(station) ?? new ZeroStationOption();
            option.CopyByEmpty(Config);
            if (option.ApiTimeout <= 1)
                option.ApiTimeout = 60;

            if (option.SpeedLimitModel != SpeedLimitType.WaitCount)
                option.SpeedLimitModel = SpeedLimitType.Single;

            //if (option.TaskCpuMultiple <= 0)
            //    option.TaskCpuMultiple = 1;
            //else if (option.TaskCpuMultiple > 128)
            //    option.TaskCpuMultiple = 128;

            if (option.MaxWait < 0xFF)
                option.MaxWait = 0xFF;
            else if (option.MaxWait > 0xFFFFF)
                option.MaxWait = 0xFFFFF;

            return option;
        }

        private static void ShowOptionInfo()
        {
            StringBuilder info = new StringBuilder();
            info.AppendLine($"  ZMQ : {zmq.LibraryVersion}");
            info.AppendLine($"  OS : {(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" : "Windows")}");
            info.AppendLine($"  RunModel : {(Config.CanRaiseEvent == true ? "Service" : "Monitor")} : {ConfigurationManager.Root["ASPNETCORE_ENVIRONMENT_"]}");
            info.AppendLine($"  Name : {Config.StationName} {GlobalContext.ServiceName} {GlobalContext.ServiceRealName} {Config.LocalIpAddress}");

            info.AppendLine($"  RootPath : {Config.RootPath}");
            info.AppendLine($"  LogPath : {LogRecorder.LogPath}");

            var item = Config.ZeroGroup[0];
            info.AppendLine($"  Master({item.Name}) : {item.Address}:{item.ManagePort} / {item.Address}:{item.MonitorPort}");
            for (int i = 1; i < Config.ZeroGroup.Count; i++)
            {
                item = Config.ZeroGroup[i];
                info.AppendLine($"  Salver({item.Name}) : {item.Address}:{item.ManagePort} / {item.Address}:{item.MonitorPort}");
            }
            //info.AppendLine($"  PoolSize : {Config.PoolSize);

            switch (Config.SpeedLimitModel)
            {
                //case SpeedLimitType.ThreadCount:
                //    var max = (int)(Environment.ProcessorCount * Config.TaskCpuMultiple);
                //    if (max < 1)
                //        max = 1;
                //    model = $"按线程数限制:线程({Environment.ProcessorCount}×{Config.TaskCpuMultiple}={max}) 等待({Config.MaxWait})";
                //    break;
                case SpeedLimitType.WaitCount:
                    info.AppendLine($"  SpeedLimitModel : 按等待数限制:线程(1) 等待({Config.MaxWait})");
                    break;
                default:
                    info.AppendLine($"  SpeedLimitModel : 单线程:线程(1) 等待(0)");
                    break;
            }
            ThreadPool.GetMaxThreads(out var worker, out var io);
            info.AppendLine($"  Worker threads: {worker:N0}Asynchronous I / O threads: { io:N0}");
            LogRecorder.SystemLog(info.ToString());
        }
    }
}