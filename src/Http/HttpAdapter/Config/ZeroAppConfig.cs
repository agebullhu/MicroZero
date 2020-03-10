using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common;
using Agebull.MicroZero.ApiDocuments;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;

using Newtonsoft.Json;
using ZeroMQ;
using Agebull.Common.Context;

namespace Agebull.MicroZero
{
    /// <summary>
    ///     本地站点配置
    /// </summary>
    [Serializable]
    [DataContract]
    public class ZeroAppConfig : ZeroStationOption
    {
        /// <summary>
        ///   服务中心组，第一个为主
        /// </summary>
        [DataMember]
        public List<ZeroItem> ZeroGroup { get; set; }


        /// <summary>
        ///   线程池最大工作线程数
        /// </summary>
        [DataMember] public int MaxWorkThreads;

        /// <summary>
        ///   线程池最大IO线程数
        /// </summary>
        [DataMember] public int MaxIOThreads;

        /// <summary>
        ///   是否需要发出事件
        /// </summary>
        [DataMember]
        public bool? CanRaiseEvent { get; set; }

        /// <summary>
        ///     站点数据使用AppName为文件夹
        /// </summary>
        [DataMember]
        public bool? StationIsolate { get; set; }

        /// <summary>
        ///     服务名称
        /// </summary>
        [DataMember]
        public string ServiceName { get; set; }


        /// <summary>
        ///     短名称
        /// </summary>
        [DataMember]
        public string ShortName { get; set; }

        /// <summary>
        ///     站点名称，注意唯一性
        /// </summary>
        [DataMember]
        public string StationName { get; set; }

        /// <summary>
        ///     本地数据文件夹
        /// </summary>
        [DataMember]
        public string DataFolder { get; set; }

        /// <summary>
        ///     本地日志文件夹
        /// </summary>
        [DataMember]
        public string LogFolder { get; set; }

        /// <summary>
        ///     本地配置文件夹
        /// </summary>
        [DataMember]
        public string ConfigFolder { get; set; }

        /// <summary>
        ///     应用所在的顶级目录
        /// </summary>
        [DataMember]
        public string RootPath { get; set; }

        /// <summary>
        ///     插件地址,如为空则与运行目录相同
        /// </summary>
        [DataMember]
        public string AddInPath { get; set; }

        /// <summary>
        /// 如果目标配置存在,则复制之
        /// </summary>
        /// <param name="option"></param>
        internal void CopyByHase(ZeroAppConfig option)
        {
            base.CopyByHase(option);
            if (!string.IsNullOrWhiteSpace(option.AddInPath))
                AddInPath = option.AddInPath;
            if (!string.IsNullOrWhiteSpace(option.AddInPath))
                ConfigFolder = option.ConfigFolder;
            if (!string.IsNullOrWhiteSpace(option.LogFolder))
                LogFolder = option.LogFolder;
            if (!string.IsNullOrWhiteSpace(option.DataFolder))
                DataFolder = option.DataFolder;
            if (!string.IsNullOrWhiteSpace(option.StationName))
                StationName = option.StationName;
            if (!string.IsNullOrWhiteSpace(option.ShortName))
                ShortName = option.ShortName;
            if (!string.IsNullOrWhiteSpace(option.ServiceName))
                ServiceName = option.ServiceName;
            if (option.StationIsolate != null)
                StationIsolate = option.StationIsolate;

            if (option.SpeedLimitModel > SpeedLimitType.None)
                SpeedLimitModel = option.SpeedLimitModel;
            //if (option.TaskCpuMultiple > 0)
            //    TaskCpuMultiple = option.TaskCpuMultiple;
            if (option.MaxWait > 0)
                MaxWait = option.MaxWait;
            if (option.CanRaiseEvent != null)
                CanRaiseEvent = option.CanRaiseEvent;

            if (ZeroGroup == null || option.ZeroGroup == null)
                ZeroGroup = option.ZeroGroup;
            else if (option.ZeroGroup.Count > 0)
            {
                foreach (var item in option.ZeroGroup)
                {
                    if (ZeroGroup.Any(p => p.Address == item.Address))
                        continue;
                    ZeroGroup.Add(item);
                }
            }
            /*
            if (option.PoolSize > 0)
                PoolSize = option.PoolSize;
            if (!string.IsNullOrWhiteSpace(option.ZeroAddress))
                ZeroAddress = option.ZeroAddress;
            if (option.ZeroMonitorPort > 0)
                ZeroMonitorPort = option.ZeroMonitorPort;
            if (option.ZeroManagePort > 0)
                ZeroManagePort = option.ZeroManagePort;
            if (!string.IsNullOrWhiteSpace(option.ServiceKey))
                ServiceKey = option.ServiceKey;

            BridgeLocalAddress = option.BridgeLocalAddress;
            BridgeCallAddress = option.BridgeCallAddress;
            BridgeResultAddress = option.BridgeResultAddress;
            */
        }

        /// <summary>
        /// 如果本配置内容为空则用目标配置补全
        /// </summary>
        /// <param name="option"></param>
        public void CopyByEmpty(ZeroAppConfig option)
        {
            base.CopyByEmpty(option);
            if (string.IsNullOrWhiteSpace(AddInPath))
                AddInPath = option.AddInPath;
            if (string.IsNullOrWhiteSpace(ConfigFolder))
                ConfigFolder = option.ConfigFolder;
            if (string.IsNullOrWhiteSpace(LogFolder))
                LogFolder = option.LogFolder;
            if (string.IsNullOrWhiteSpace(DataFolder))
                DataFolder = option.DataFolder;
            if (string.IsNullOrWhiteSpace(StationName))
                StationName = option.StationName;
            if (string.IsNullOrWhiteSpace(ShortName))
                ShortName = option.ShortName;
            if (string.IsNullOrWhiteSpace(ServiceName))
                ServiceName = option.ServiceName;
            if (StationIsolate == null)
                StationIsolate = option.StationIsolate;

            if (SpeedLimitModel == SpeedLimitType.None)
                SpeedLimitModel = option.SpeedLimitModel;
            //if (TaskCpuMultiple == 0)
            //    TaskCpuMultiple = option.TaskCpuMultiple;
            if (MaxWait == 0)
                MaxWait = option.MaxWait;

            if (CanRaiseEvent == null)
                CanRaiseEvent = option.CanRaiseEvent;

            if (ZeroGroup == null || option.ZeroGroup == null)
                ZeroGroup = option.ZeroGroup;
            else if (option.ZeroGroup.Count > 0)
            {
                foreach (var item in option.ZeroGroup)
                {
                    if (ZeroGroup.Any(p => p.Address == item.Address))
                        continue;
                    ZeroGroup.Add(item);
                }
            }
        }
    }
    /*// <summary>
    ///     本地站点配置
    /// </summary>
    [Serializable]
    [DataContract]
    public class ZeroAppConfig : ZeroStationOption
    {
        /// <summary>
        ///   是否需要发出事件
        /// </summary>
        [DataMember]
        public bool? CanRaiseEvent { get; set; }

        /// <summary>
        ///     站点孤立
        /// </summary>
        [DataMember]
        public bool? StationIsolate { get; set; }

        /// <summary>
        ///     服务名称
        /// </summary>
        [DataMember]
        public string ServiceName { get; set; }

        /// <summary>
        ///     服务器唯一标识
        /// </summary>
        [DataMember]
        public string ServiceKey { get; set; }

        /// <summary>
        ///     短名称
        /// </summary>
        [DataMember]
        public string ShortName { get; set; }

        /// <summary>
        ///     站点名称，注意唯一性
        /// </summary>
        [DataMember]
        public string StationName { get; set; }

        /// <summary>
        ///     ZeroCenter主机IP地址
        /// </summary>
        [DataMember]
        public string ZeroAddress { get; set; }

        /// <summary>
        ///     ZeroCenter监测端口号
        /// </summary>
        [DataMember]
        public int ZeroMonitorPort { get; set; }

        /// <summary>
        ///     ZeroCenter管理端口号
        /// </summary>
        [DataMember]
        public int ZeroManagePort { get; set; }

        /// <summary>
        ///     本地数据文件夹
        /// </summary>
        [DataMember]
        public string DataFolder { get; set; }

        /// <summary>
        ///     本地日志文件夹
        /// </summary>
        [DataMember]
        public string LogFolder { get; set; }

        /// <summary>
        ///     本地配置文件夹
        /// </summary>
        [DataMember]
        public string ConfigFolder { get; set; }

        /// <summary>
        ///     应用所在的顶级目录
        /// </summary>
        [DataMember]
        public string RootPath { get; set; }

        /// <summary>
        ///     插件地址,如为空则与运行目录相同
        /// </summary>
        [DataMember]
        public string AddInPath { get; set; }

        /// <summary>
        /// 如果目标配置存在,则复制之
        /// </summary>
        /// <param name="option"></param>
        internal void CopyByHase(ZeroAppConfig option)
        {
            if (!string.IsNullOrWhiteSpace(option.AddInPath))
                AddInPath = option.AddInPath;
            if (!string.IsNullOrWhiteSpace(option.AddInPath))
                ConfigFolder = option.ConfigFolder;
            if (!string.IsNullOrWhiteSpace(option.LogFolder))
                LogFolder = option.LogFolder;
            if (!string.IsNullOrWhiteSpace(option.DataFolder))
                DataFolder = option.DataFolder;
            if (!string.IsNullOrWhiteSpace(option.ZeroAddress))
                ZeroAddress = option.ZeroAddress;
            if (option.ZeroMonitorPort > 0)
                ZeroMonitorPort = option.ZeroMonitorPort;
            if (option.ZeroManagePort > 0)
                ZeroManagePort = option.ZeroManagePort;
            if (!string.IsNullOrWhiteSpace(option.StationName))
                StationName = option.StationName;
            if (!string.IsNullOrWhiteSpace(option.ShortName))
                ShortName = option.ShortName;
            if (!string.IsNullOrWhiteSpace(option.ServiceName))
                ServiceName = option.ServiceName;
            if (!string.IsNullOrWhiteSpace(option.ServiceKey))
                ServiceKey = option.ServiceKey;
            if (option.StationIsolate != null)
                StationIsolate = option.StationIsolate;

            if (option.SpeedLimitModel > SpeedLimitType.None)
                SpeedLimitModel = option.SpeedLimitModel;
            if (option.TaskCpuMultiple > 0)
                TaskCpuMultiple = option.TaskCpuMultiple;
            if (option.MaxWait > 0)
                MaxWait = option.MaxWait;
            if (option.PoolSize > 0)
                PoolSize = option.PoolSize;
            if (option.CanRaiseEvent != null)
                CanRaiseEvent = option.CanRaiseEvent;

            if (option.ApiTimeout > 0)
                ApiTimeout = option.ApiTimeout;
            if (option.ApiTimeoutKill)
                ApiTimeoutKill = option.ApiTimeoutKill;

            //BridgeLocalAddress = option.BridgeLocalAddress;
            //BridgeCallAddress = option.BridgeCallAddress;
            //BridgeResultAddress = option.BridgeResultAddress;
        }

        /// <summary>
        /// 如果本配置内容为空则用目标配置补全
        /// </summary>
        /// <param name="option"></param>
        internal void CopyByEmpty(ZeroAppConfig option)
        {
            if (string.IsNullOrWhiteSpace(AddInPath))
                AddInPath = option.AddInPath;
            if (string.IsNullOrWhiteSpace(ConfigFolder))
                ConfigFolder = option.ConfigFolder;
            if (string.IsNullOrWhiteSpace(LogFolder))
                LogFolder = option.LogFolder;
            if (string.IsNullOrWhiteSpace(DataFolder))
                DataFolder = option.DataFolder;
            if (string.IsNullOrWhiteSpace(ZeroAddress))
                ZeroAddress = option.ZeroAddress;
            if (ZeroMonitorPort <= 0)
                ZeroMonitorPort = option.ZeroMonitorPort;

            if (ZeroManagePort <= 0)
                ZeroManagePort = option.ZeroManagePort;
            if (string.IsNullOrWhiteSpace(StationName))
                StationName = option.StationName;
            if (string.IsNullOrWhiteSpace(ShortName))
                ShortName = option.ShortName;
            if (string.IsNullOrWhiteSpace(ServiceName))
                ServiceName = option.ServiceName;
            if (string.IsNullOrWhiteSpace(ServiceKey))
                ServiceKey = option.ServiceKey;
            if (StationIsolate == null)
                StationIsolate = option.StationIsolate;

            if (SpeedLimitModel == SpeedLimitType.None)
                SpeedLimitModel = option.SpeedLimitModel;
            if (TaskCpuMultiple == 0)
                TaskCpuMultiple = option.TaskCpuMultiple;
            if (MaxWait == 0)
                MaxWait = option.MaxWait;

            if (PoolSize <= 0)
                PoolSize = option.PoolSize;

            if (CanRaiseEvent == null)
                CanRaiseEvent = option.CanRaiseEvent;
            if (ApiTimeout <= 0)
                ApiTimeout = option.ApiTimeout;
            if (!ApiTimeoutKill)
                ApiTimeoutKill = option.ApiTimeoutKill;

            //BridgeLocalAddress = option.BridgeLocalAddress;
            //BridgeCallAddress = option.BridgeCallAddress;
            //BridgeResultAddress = option.BridgeResultAddress;
        }
        /// <summary>
        ///     程序所在地址
        /// </summary>
        [IgnoreDataMember]
        public string BinPath { get; set; }

        /// <summary>
        ///     本机IP地址
        /// </summary>
        [IgnoreDataMember]
        public string LocalIpAddress { get; set; }

        /// <summary>
        ///     监测中心广播地址
        /// </summary>
        [IgnoreDataMember]
        public string ZeroMonitorAddress { get; set; }

        /// <summary>
        ///     监测中心管理地址
        /// </summary>
        [IgnoreDataMember]
        public string ZeroManageAddress { get; set; }

        /// <summary>
        ///     是否在Linux黑环境下
        /// </summary>
        [IgnoreDataMember]
        public bool IsLinux { get; set; }

        /// <summary>
        ///    连接池大小
        /// </summary>
        [IgnoreDataMember]
        public int PoolSize { get; set; }

        #region 桥接

        /*
        
        /// <summary>
        ///     ZeroCenter桥接地址
        /// </summary>
        [DataMember]
        public string BridgeResultAddress { get; set; }

        /// <summary>
        ///     ZeroCenter桥接地址
        /// </summary>
        [DataMember]
        public string BridgeCallAddress { get; set; }

        /// <summary>
        ///     ZeroCenter桥接地址
        /// </summary>
        [DataMember]
        public string BridgeLocalAddress { get; set; }

         * /

        #endregion

        #region 站点

        /// <summary>
        ///     发现的文档集合
        /// </summary>
        public Dictionary<string, StationDocument> Documents = new Dictionary<string, StationDocument>();

        /// <summary>
        ///     检查重名情况
        /// </summary>
        /// <param name="old"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool Check(StationConfig old, StationConfig config)
        {
            if (!CheckName(old, config.Name))
                return false;
            if (!CheckName(old, config.ShortName))
                return false;
            if (config.StationAlias == null || config.StationAlias.Count == 0)
                return true;
            foreach (var al in config.StationAlias)
                if (!CheckName(old, al))
                    return false;
            return true;
        }

        private bool CheckName(StationConfig config, string name)
        {
            //Console.WriteLine("lock (_configs)");
            lock (_configs)
            {
                if (_configs.Values.Where(p => p != config).Any(p => p.StationName == name))
                    return false;
                if (_configs.Values.Where(p => p != config).Any(p => p.ShortName == name))
                    return false;
                if (_configs.Values.Where(p => p != config && p.StationAlias != null)
                    .Any(p => p.StationAlias.Any(a => string.Equals(a, name))))
                    return false;
            }

            return true;
        }
        /// <summary>
        ///     快捷取站点配置
        /// </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        public StationConfig this[string station]
        {
            get
            {
                if (station == null)
                    return null;
                //Console.WriteLine("lock (_configs)");
                lock (_configs)
                {
                    _configMap.TryGetValue(station, out var config);
                    return config;
                }
            }
        }

        internal void Remove(StationConfig station)
        {
            //Console.WriteLine("lock (_configs)");
            lock (_configs)
            {
                _configs.Remove(station.Name);
                _configMap.Remove(station.Name);
                _configMap.Remove(station.ShortName);
                if (station.StationAlias == null)
                    return;
                foreach (var ali in station.StationAlias)
                    _configMap.Remove(ali);
            }
        }

        private void AddStation(StationConfig station)
        {
            //Console.WriteLine("lock (_configs)");
            lock (_configs)
            {
                if (_configs.TryGetValue(station.Name, out var config))
                    config.Copy(station);
                else
                    _configs.Add(station.Name, config = station);

                if (!_configMap.ContainsKey(station.Name))
                    _configMap.Add(station.Name, config);
                else
                    _configMap[station.Name] = config;
                if (string.IsNullOrWhiteSpace(station.ShortName) || station.ShortName == station.Name)
                    return;
                if (!_configMap.ContainsKey(station.ShortName))
                    _configMap.Add(station.ShortName, config);
                else
                    _configMap[station.ShortName] = config;

                //if (station.StationAlias == null)
                //    return;
                //foreach (var ali in station.StationAlias)
                //    if (!_configMap.ContainsKey(ali))
                //        _configMap.Add(ali, config);
                //    else
                //        _configMap[ali] = config;
            }
        }

        /// <summary>
        ///     站点集合
        /// </summary>
        private readonly Dictionary<string, StationConfig> _configs =
            new Dictionary<string, StationConfig>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     站点配置
        /// </summary>
        public IEnumerable<StationConfig> Stations => GetConfigs();

        /// <summary>
        ///     站点集合
        /// </summary>
        private readonly Dictionary<string, StationConfig> _configMap =
            new Dictionary<string, StationConfig>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     站点集合
        /// </summary>
        public StationConfig[] GetConfigs()
        {
            //Console.WriteLine("lock (_configs)");
            lock (_configs)
            {
                return _configs.Values.Distinct().ToArray();
            }
        }

        /// <summary>
        ///     站点集合
        /// </summary>
        public StationConfig[] GetConfigs(Func<StationConfig, bool> condition)
        {
            //Console.WriteLine("lock (_configs)");
            lock (_configs)
            {
                return _configs.Values.Where(condition).Distinct().ToArray();
            }
        }

        /// <summary>
        ///     遍历
        /// </summary>
        /// <param name="action"></param>
        public void Foreach(Action<StationConfig> action)
        {
            //Console.WriteLine("lock (_configs)");
            lock (_configs)
            {
                foreach (var config in _configs.Values.ToArray())
                    action(config);
            }
        }

        /// <summary>
        ///     试着取配置
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

            //Console.WriteLine("lock (_configs)");
            lock (_configs)
            {
                return _configs.TryGetValue(stationName, out stationConfig);
            }
        }

        /// <summary>
        ///     清除配置
        /// </summary>
        /// <returns></returns>
        internal void ClearConfig()
        {
            _configs.Clear();
            _configMap.Clear();
        }

        /// <summary>
        ///     试着取配置
        /// </summary>
        /// <param name="stationName"></param>
        /// <param name="json"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool UpdateConfig(string stationName, string json, out StationConfig config)
        {
            if (stationName == null || string.IsNullOrEmpty(json) || json[0] != '{')
            {
                ZeroTrace.WriteError("UpdateConfig", "argument error", stationName, json);
                config = null;
                return false;
            }

            try
            {
                config = JsonConvert.DeserializeObject<StationConfig>(json);
                AddStation(config);
                return true;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("UpdateConfig", e, stationName, json);
                config = null;
                return false;
            }
        }

        /// <summary>
        ///     刷新配置
        /// </summary>
        /// <param name="json"></param>
        public bool FlushConfigs(string json)
        {
            try
            {
                var configs = JsonConvert.DeserializeObject<List<StationConfig>>(json);
                foreach (var config in configs)
                    AddStation(config);
                ZeroApplication.RaiseEvent(ZeroNetEventType.ConfigUpdate);
                return true;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("LoadAllConfig", e, json);
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
            ZeroAppConfig baseConfig = null;
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
                    baseConfig = s.Child<ZeroAppConfig>("Global");
                    var so = s.Child<SocketOption>("socketOption");
                    if (so != null)
                        ZSocket.Option = so;
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

            var option = sec.Child<SocketOption>("socketOption");
            if (option != null)
            {
                ZSocket.Option = option;
            }

            Config = sec.Child<ZeroAppConfig>(AppName) ?? new ZeroAppConfig();
            if (string.IsNullOrWhiteSpace(Config.StationName))
                Config.StationName = AppName;

            Config.BinPath = curPath;
            Config.RootPath = rootPath;
            Config.IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            if (baseConfig != null)
            {
                Config.CopyByHase(baseConfig);
            }
            {
                var global = sec.Child<ZeroAppConfig>("Global");

                if (global == null)
                    if (WorkModel == ZeroWorkModel.Bridge)
                        global = new ZeroAppConfig();
                    else
                        throw new Exception("无法找到主配置节点,路径为Zero.Global,在zero.json或appsettings.json中设置");

                Config.CopyByEmpty(global);
            }
            if (Config.ApiTimeout <= 0)
                Config.ApiTimeout = 60;
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

                if (string.IsNullOrWhiteSpace(Config.ConfigFolder))
                    Config.ConfigFolder = IOHelper.CheckPath(rootPath, "config", AppName);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Config.DataFolder))
                    Config.DataFolder = IOHelper.CheckPath(rootPath, "datas");

                if (string.IsNullOrWhiteSpace(Config.LogFolder))
                    Config.LogFolder = IOHelper.CheckPath(rootPath, "logs");

                if (string.IsNullOrWhiteSpace(Config.ConfigFolder))
                    Config.ConfigFolder = IOHelper.CheckPath(rootPath, "config");
            }

            #endregion

            #region ZeroCenter

            if (WorkModel == ZeroWorkModel.Bridge)
                return;
            if (string.IsNullOrWhiteSpace(Config.ZeroAddress))
                Config.ZeroAddress = "127.0.0.1";
            if (Config.ZeroManagePort <= 1024 || Config.ZeroManagePort >= 65000)
                Config.ZeroManagePort = 8000;
            if (Config.ZeroMonitorPort <= 1024 || Config.ZeroMonitorPort >= 65000)
                Config.ZeroMonitorPort = 8001;
            if (Config.PoolSize > 4096 || Config.PoolSize < 10)
                Config.PoolSize = 100;
            Config.ZeroManageAddress = ZeroIdentityHelper.GetRequestAddress("SystemManage", Config.ZeroManagePort);
            Config.ZeroMonitorAddress = ZeroIdentityHelper.GetWorkerAddress("SystemMonitor", Config.ZeroMonitorPort);
            #endregion
        }

        private static string GetHostIps()
        {
            var ips = new StringBuilder();
            try
            {
                var first = true;
                string hostName = Dns.GetHostName();
                Console.WriteLine(hostName);
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
            option.Copy(Config);
            if (option.ApiTimeout <= 1)
                option.ApiTimeout = 60;

            if (option.SpeedLimitModel < SpeedLimitType.Single || option.SpeedLimitModel > SpeedLimitType.WaitCount)
                option.SpeedLimitModel = SpeedLimitType.None;

            if (option.TaskCpuMultiple <= 0)
                option.TaskCpuMultiple = 1;
            else if (option.TaskCpuMultiple > 128)
                option.TaskCpuMultiple = 128;

            if (option.MaxWait < 0xFF)
                option.MaxWait = 0xFF;
            else if (option.MaxWait > 0xFFFFF)
                option.MaxWait = 0xFFFFF;

            return option;
        }

        private static void ShowOptionInfo()
        {
            ZeroTrace.SystemLog("OS", RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" : "Windows");
            ZeroTrace.SystemLog("RunModel", Config.CanRaiseEvent == true ? "Service" : "Monitor", ConfigurationManager.Root["ASPNETCORE_ENVIRONMENT_"]);

            ZeroTrace.SystemLog("RootPath", Config.RootPath);
            ZeroTrace.SystemLog("LogPath", LogRecorder.LogPath);
            ZeroTrace.SystemLog("ZeroCenter", Config.ZeroAddress, Config.ZeroManagePort, Config.ZeroMonitorPort);
            ZeroTrace.SystemLog("Name", Config.StationName, GlobalContext.ServiceName, GlobalContext.ServiceRealName, Config.LocalIpAddress);

            ZeroTrace.SystemLog("PoolSize", Config.PoolSize);

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

            ZeroTrace.SystemLog("SpeedLimitModel", model);
        }
    }*/
}