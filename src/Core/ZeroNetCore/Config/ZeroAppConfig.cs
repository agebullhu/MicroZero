using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Agebull.MicroZero.ApiDocuments;

using Newtonsoft.Json;

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
        ///   服务小心组，第一个为主
        /// </summary>
        [DataMember]
        public List<ZeroItem> ZeroGroup { get; set; }


        /// <summary>
        ///   主服务中心
        /// </summary>
        [DataMember]
        public ZeroItem Master { get; set; }

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
            if (option.TaskCpuMultiple > 0)
                TaskCpuMultiple = option.TaskCpuMultiple;
            if (option.MaxWait > 0)
                MaxWait = option.MaxWait;
            if (option.CanRaiseEvent != null)
                CanRaiseEvent = option.CanRaiseEvent;

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
            if (TaskCpuMultiple == 0)
                TaskCpuMultiple = option.TaskCpuMultiple;
            if (MaxWait == 0)
                MaxWait = option.MaxWait;

            if (CanRaiseEvent == null)
                CanRaiseEvent = option.CanRaiseEvent;

            if (ZeroGroup == null)
                ZeroGroup = option.ZeroGroup;
            /*

            if (PoolSize <= 0)
                PoolSize = option.PoolSize;
            if (string.IsNullOrWhiteSpace(ZeroAddress))
                ZeroAddress = option.ZeroAddress;
            if (ZeroMonitorPort <= 0)
                ZeroMonitorPort = option.ZeroMonitorPort;
            
            if (ZeroManagePort <= 0)
                ZeroManagePort = option.ZeroManagePort;
            if (string.IsNullOrWhiteSpace(ServiceKey))
                ServiceKey = option.ServiceKey;
            BridgeLocalAddress = option.BridgeLocalAddress;
            BridgeCallAddress = option.BridgeCallAddress;
            BridgeResultAddress = option.BridgeResultAddress;*/
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
        ///     是否在Linux黑环境下
        /// </summary>
        [IgnoreDataMember]
        public bool IsLinux { get; set; }

        #region 桥接

        /*
        
        /// <summary>
        ///    连接池大小
        /// </summary>
        [IgnoreDataMember]
        public int PoolSize { get; set; }

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

         */

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
                foreach (var ali in station.StationAlias) _configMap.Remove(ali);
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
        /// <param name="item"></param>
        /// <param name="stationName"></param>
        /// <param name="json"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool UpdateConfig(ZeroItem item, string stationName, string json, out StationConfig config)
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
                config.Group = item.Name;
                config.Address = item.Address;
                config.ServiceKey = item.ServiceKey.ToZeroBytes();
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
        /// <param name="item">服务中心组</param>
        /// <param name="json"></param>
        public bool FlushConfigs(ZeroItem item, string json)
        {
            try
            {
                lock (_configs)
                {
                    if (item == null)
                        return false;
                    var configs = JsonConvert.DeserializeObject<List<StationConfig>>(json);
                    foreach (var config in configs)
                    {
                        if (item != Master && (config.IsBaseStation || config.IsSystem))
                            continue;

                        //Console.WriteLine("lock (_configs)");

                        if (_configs.TryGetValue(config.Name, out var old))
                        {
                            if (old.Name != item.Name)
                                continue;
                        }

                        config.Group = item.Name;
                        config.Address = item.Address;
                        config.ServiceKey = item.ServiceKey.ToZeroBytes();
                        AddStation(config);
                    }
                }
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
}