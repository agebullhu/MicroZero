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
    public class ZeroAppConfigRuntime : ZeroAppConfig
    {
        /// <summary>
        ///   主服务中心
        /// </summary>
        [IgnoreDataMember]
        public ZeroItem Master { get; set; }

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
                if (station.StationAlias != null)
                    foreach (var ali in station.StationAlias)
                        _configMap.Remove(ali);
            }
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationRemove,station.Group,null,station, true);
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
                if (!string.IsNullOrWhiteSpace(station.ShortName) && station.ShortName != station.Name)
                {
                    if (!_configMap.ContainsKey(station.ShortName))
                        _configMap.Add(station.ShortName, config);
                    else
                        _configMap[station.ShortName] = config;
                }
                //if (station.StationAlias == null)
                //    return;
                //foreach (var ali in station.StationAlias)
                //    if (!_configMap.ContainsKey(ali))
                //        _configMap.Add(ali, config);
                //    else
                //        _configMap[ali] = config;
            }
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationUpdate, station.Group, null, station, true);
        }

        /// <summary>
        ///     站点集合
        /// </summary>
        private readonly Dictionary<string, StationConfig> _configs = new Dictionary<string, StationConfig>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     站点配置
        /// </summary>
        public IEnumerable<StationConfig> Stations => GetConfigs();

        /// <summary>
        ///     站点集合
        /// </summary>
        private readonly Dictionary<string, StationConfig> _configMap = new Dictionary<string, StationConfig>(StringComparer.OrdinalIgnoreCase);

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