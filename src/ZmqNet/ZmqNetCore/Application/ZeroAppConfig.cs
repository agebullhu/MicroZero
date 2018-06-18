using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Gboxt.Common.DataModel;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ZeroMQ.lib;

namespace Agebull.ZeroNet.Core
{

    /// <summary>
    /// 本地站点配置
    /// </summary>
    [Serializable]
    public class ZeroAppConfig
    {
        /// <summary>
        /// 站点孤立
        /// </summary>
        [DataMember]
        public bool StationIsolate { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        [DataMember]
        public string ServiceName { get; set; }

        /// <summary>
        /// 服务器唯一标识
        /// </summary>
        [DataMember]
        public string ServiceKey { get; set; }

        /// <summary>
        /// 短名称
        /// </summary>
        [DataMember]
        public string ShortName { get; set; }

        /// <summary>
        /// 站点名称，注意唯一性
        /// </summary>
        [DataMember]
        public string StationName { get; set; }

        /// <summary>
        /// ZeroCenter主机IP地址
        /// </summary>
        [DataMember]
        public string ZeroAddress { get; set; }

        /// <summary>
        /// ZeroCenter监测端口号
        /// </summary>
        [DataMember]
        public int ZeroMonitorPort { get; set; }

        /// <summary>
        /// ZeroCenter管理端口号
        /// </summary>
        [DataMember]
        public int ZeroManagePort { get; set; }

        /// <summary>
        /// 本地数据文件夹
        /// </summary>
        [DataMember]
        public string DataFolder { get; set; }

        /// <summary>
        /// 本地日志文件夹
        /// </summary>
        [DataMember]
        public string LogFolder { get; set; }

        /// <summary>
        /// ApiClient与ApiStation限速模式
        /// </summary>
        /// <remarks>
        /// Single：单线程无等待
        /// ThreadCount:按线程数限制,线程内无等待
        ///     线程数计算公式 : 机器CPU数量 X TaskCpuMultiple 最小为1,请合理设置并测试
        /// WaitCount: 单线程,每个请求起一个新Task,直到最高未完成数量达MaxWait时,
        ///     ApiClient休眠直到等待数量 低于 MaxWait
        ///     ApiStation返回服务器忙(熔断)
        /// </remarks>
        [DataMember]
        public SpeedLimitType SpeedLimitModel { get; set; }

        /// <summary>
        /// 最大等待数(0xFF-0xFFFFF)
        /// </summary>
        [DataMember]
        public int MaxWait { get; set; }


        /// <summary>
        /// 最大Task与Cpu核心数的倍数关系(0-128)
        /// </summary>
        [DataMember]
        public decimal TaskCpuMultiple { get; set; }

        /// <summary>
        /// 插件地址,如为空则与运行目录相同
        /// </summary>
        [DataMember]
        public string AddInPath { get; set; }

        /// <summary>
        /// 本机IP地址
        /// </summary>
        [IgnoreDataMember]
        public string LocalIpAddress { get; set; }

        /// <summary>
        /// 实例名称
        /// </summary>
        [IgnoreDataMember]
        public string RealName { get; set; }

        /// <summary>
        /// Zmq标识
        /// </summary>
        [IgnoreDataMember]
        public byte[] Identity { get; set; }

        /// <summary>
        ///     监测中心广播地址
        /// </summary>
        public string ZeroMonitorAddress { get; set; }

        /// <summary>
        ///     监测中心管理地址
        /// </summary>
        public string ZeroManageAddress { get; set; }

        /// <summary>
        ///     是否在Linux黑环境下
        /// </summary>
        public bool IsLinux { get; set; }

        #region 站点
        /// <summary>
        /// 快捷取站点配置
        /// </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        public StationConfig this[string station]
        {
            get
            {
                if (station == null)
                    return null;
                lock (_configs)
                {
                    _configs.TryGetValue(station, out var config);
                    return config;
                }
            }
            set
            {
                lock (_configs)
                {
                    if (value == null)
                    {
                        _configs.Remove(station);
                        return;
                    }
                    if (_configs.ContainsKey(station))
                        _configs[station].Copy(value);
                    else
                        _configs.Add(station, value);
                }
            }
        }
        /// <summary>
        ///     站点集合
        /// </summary>
        private readonly Dictionary<string, StationConfig> _configs = new Dictionary<string, StationConfig>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     站点集合
        /// </summary>
        public StationConfig[] GetConfigs()
        {
            lock (_configs)
                return _configs.Values.ToArray();
        }
        /// <summary>
        ///     站点集合
        /// </summary>
        public StationConfig[] GetConfigs(Func<StationConfig, bool> condition)
        {
            lock (_configs)
                return _configs.Values.Where(condition).ToArray();
        }

        /// <summary>
        /// 刷新配置
        /// </summary>
        /// <param name="json"></param>
        public bool FlushConfigs(string json)
        {
            try
            {
                var configs = JsonConvert.DeserializeObject<List<StationConfig>>(json);
                foreach (var config in configs)
                {
                    config.State = ZeroCenterState.Run;
                    this[config.StationName] = config;
                }
                ZeroTrace.WriteInfo("LoadAllConfig", json);
                return true;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("LoadAllConfig", e, json);
                return false;
            }
        }
        /// <summary>
        /// 遍历
        /// </summary>
        /// <param name="action"></param>
        public void Foreach(Action<StationConfig> action)
        {
            lock (_configs)
            {
                foreach (var config in _configs.Values)
                    action(config);
            }
        }
        /// <summary>
        /// 试着取配置
        /// </summary>
        /// <param name="stationName"></param>
        /// <param name="stationConfig"></param>
        /// <returns></returns>
        public bool TryGetConfig(string stationName, out StationConfig stationConfig)
        {
            if (stationName == null)
            {
                stationConfig = null;
                return false;
            }
            lock (_configs)
            {
                return _configs.TryGetValue(stationName, out stationConfig);
            }
        }
        /// <summary>
        /// 试着取配置
        /// </summary>
        /// <param name="stationName"></param>
        /// <param name="json"></param>
        /// <param name="stationConfig"></param>
        /// <returns></returns>
        public bool UpdateConfig(string stationName, string json, out StationConfig stationConfig)
        {
            if (stationName == null || string.IsNullOrEmpty(json) || json[0] != '{')
            {
                ZeroTrace.WriteError("UpdateConfig", "argument error", stationName, json);
                stationConfig = null;
                return false;
            }
            try
            {
                stationConfig = this[stationName] = JsonConvert.DeserializeObject<StationConfig>(json);
                return true;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("UpdateConfig", e, stationName, json);
                stationConfig = null;
                return false;
            }

        }
        #endregion
    }

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
        public static ZeroAppConfig Config { get; set; }

        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public static StationConfig GetConfig(string stationName)
        {
            if (Config.TryGetConfig(stationName, out var config)) return config;
            config = SystemManager.LoadConfig(stationName);
            if (config == null)
                return null;
            Config[stationName] = config;
            return config;
        }


        /// <summary>
        ///     配置校验
        /// </summary>
        private static void CheckConfig()
        {
            AppName = ConfigurationManager.Root["AppName"];
            var curPath = ConfigurationManager.Root.GetValue("contentRoot", Environment.CurrentDirectory);
            string rootPath;
            if (ConfigurationManager.Root["ASPNETCORE_ENVIRONMENT_"] == "Development")
            {
                ZeroTrace.WriteInfo("Option", "Development");
                rootPath = curPath;
            }
            else
            {
                ZeroTrace.WriteInfo("Option", RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" : "Windows");
                rootPath = Path.GetDirectoryName(curPath);
                if (string.IsNullOrWhiteSpace(AppName))
                {
                    ConfigurationManager.Root["AppName"] = AppName = Path.GetFileName(curPath);
                }
                // ReSharper disable once AssignNullToNotNullAttribute
                var file = Path.Combine(rootPath, "config", "zero.json");
                if (File.Exists(file))
                    ConfigurationManager.Load(file);
            }

            ConfigurationManager.Root["rootPath"] = rootPath;

            var sec = ConfigurationManager.Get("Zero");

            Config = string.IsNullOrWhiteSpace(AppName)
                ? sec.Child<ZeroAppConfig>("Station")
                : sec.Child<ZeroAppConfig>(AppName) ?? sec.Child<ZeroAppConfig>("Station");
             
            if (Config == null)
                throw new Exception($"无法找到主配置节点,路径为Zero.{AppName}或Zero.Station,在appsettings.json中设置");
            if (string.IsNullOrWhiteSpace(AppName))
                ConfigurationManager.Root["AppName"] = AppName = Config.StationName;

            Config.IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var global = sec.Child<ZeroAppConfig>("Global");
            global.LogFolder = string.IsNullOrWhiteSpace(global.LogFolder) ? IOHelper.CheckPath(rootPath, "logs") : global.LogFolder.Trim();
            global.DataFolder = string.IsNullOrWhiteSpace(global.DataFolder) ? IOHelper.CheckPath(rootPath, "datas") : global.DataFolder.Trim();
            global.ServiceName = string.IsNullOrWhiteSpace(global.ServiceName) ? Dns.GetHostName() : global.ServiceName.Trim();
            global.ServiceKey = string.IsNullOrWhiteSpace(global.ServiceKey) ? RandomOperate.Generate(8) : global.ServiceKey.Trim();

            global.ZeroAddress = string.IsNullOrWhiteSpace(global.ZeroAddress) ? "127.0.0.1" : global.ZeroAddress.Trim();
            if (global.ZeroManagePort <= 1024 || Config.ZeroManagePort >= 65000)
                global.ZeroManagePort = 8000;
            if (global.ZeroMonitorPort <= 1024 || Config.ZeroMonitorPort >= 65000)
                global.ZeroMonitorPort = 8001;


            if (global.StationIsolate || Config.StationIsolate)
            {
                Config.ServiceName = string.IsNullOrWhiteSpace(Config.ServiceName) ? global.ServiceName : Config.ServiceName.Trim();
                Config.ServiceKey = string.IsNullOrWhiteSpace(Config.ServiceKey) ? global.ServiceKey : Config.ServiceKey.Trim();
                Config.ZeroAddress = string.IsNullOrWhiteSpace(Config.ZeroAddress) ? global.ZeroAddress : Config.ZeroAddress.Trim();
                if (Config.ZeroManagePort <= 1024 || Config.ZeroManagePort >= 65000)
                    Config.ZeroManagePort = global.ZeroManagePort;
                if (Config.ZeroMonitorPort <= 1024 || Config.ZeroMonitorPort >= 65000)
                    Config.ZeroMonitorPort = global.ZeroMonitorPort;

                Config.DataFolder = IOHelper.CheckPath(global.DataFolder, AppName);
                Config.LogFolder = IOHelper.CheckPath(global.LogFolder, AppName);
            }
            else
            {
                Config.ServiceName = global.ServiceName;
                Config.ServiceKey = global.ServiceKey;
                Config.ZeroAddress = global.ZeroAddress;
                Config.ZeroManagePort = global.ZeroManagePort;
                Config.ZeroMonitorPort = global.ZeroMonitorPort;

                Config.DataFolder = global.DataFolder;
                Config.LogFolder = global.LogFolder;
            }
            TxtRecorder.LogPath = Config.LogFolder;
            ConfigurationManager.Get("LogRecorder")["txtPath"] = Config.LogFolder;

            Config.ZeroManageAddress = ZeroIdentityHelper.GetRequestAddress("SystemManage", Config.ZeroManagePort);
            Config.ZeroMonitorAddress = ZeroIdentityHelper.GetWorkerAddress("SystemMonitor", Config.ZeroMonitorPort);
            Config.LocalIpAddress = GetHostIps();
            Config.ShortName = string.IsNullOrWhiteSpace(Config.ShortName) ? Config.StationName : Config.ShortName.Trim();
            Config.RealName = ZeroIdentityHelper.CreateRealName(false);
            Config.Identity = Config.RealName.ToAsciiBytes();
            //模式选择

            if (Config.SpeedLimitModel < SpeedLimitType.Single || Config.SpeedLimitModel > SpeedLimitType.WaitCount)
                Config.SpeedLimitModel = SpeedLimitType.ThreadCount;
            
            if (Config.TaskCpuMultiple <= 0)
                Config.TaskCpuMultiple = 1;
            else if (Config.TaskCpuMultiple > 128)
                Config.TaskCpuMultiple = 128;

            if (Config.MaxWait < 0xFF)
                Config.MaxWait = 0xFF;
            else if (Config.MaxWait > 0xFFFFF)
                Config.MaxWait = 0xFFFFF;

            ShowOptionInfo(rootPath);
        }

        static void ShowOptionInfo(string root)
        {
            ZeroTrace.WriteInfo("Option", "ZeroMQ", zmq.LibraryVersion);
            ZeroTrace.WriteInfo("Option", "AppName", AppName);
            ZeroTrace.WriteInfo("Option", "RootPath", root);
            string model;
            switch (Config.SpeedLimitModel)
            {
                default:
                    model = "单线程:线程(1) 等待(0)";
                    break;
                case SpeedLimitType.ThreadCount:
                    var max = (int)(Environment.ProcessorCount * Config.TaskCpuMultiple);
                    if (max < 1)
                        max = 1;
                    model =
                        $"按线程数限制:线程({Environment.ProcessorCount}×{Config.TaskCpuMultiple}={max}) 等待({Config.MaxWait})";
                    break;
                case SpeedLimitType.WaitCount:
                    model = $"按等待数限制:线程(1) 等待({Config.MaxWait})";
                    break;
            }

            ZeroTrace.WriteInfo("Option", model);
            ZeroTrace.WriteInfo("Option", "ZeroCenter", Config.ZeroManageAddress, Config.ZeroMonitorAddress);
        }
    }
}