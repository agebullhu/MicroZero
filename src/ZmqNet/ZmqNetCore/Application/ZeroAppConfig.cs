using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{

    /// <summary>
    /// 本地站点配置
    /// </summary>
    [Serializable]
    public class ZeroAppConfig
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        [DataMember]
        public string ServiceName { get; set; }

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
        /// 实例名称
        /// </summary>
        [DataMember]
        public string ServiceKey { get; set; }

        /// <summary>
        /// 限速模式（0 单线程 1 按线程数限制 2 按等待数限制）
        /// </summary>
        [DataMember]
        public SpeedLimitType SpeedLimitModel { get; set; }

        /// <summary>
        /// 最大等待数
        /// </summary>
        [DataMember]
        public int MaxWait { get; set; }


        /// <summary>
        /// 最大Task与Cpu核心数的倍数关系
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
                    this[config.StationName] = config;
                }
                ZeroTrace.WriteInfo(ZeroApplication.AppName, "LoadAllConfig", json);
                return true;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(ZeroApplication.AppName,e, "LoadAllConfig", json);
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
        /// 遍历
        /// </summary>
        /// <param name="action"></param>
        public void ParallelForeach(Action<StationConfig> action)
        {
            lock (_configs)
            {
                Parallel.ForEach(_configs.Values, action);
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
        public bool UpdateConfig(string stationName,string json, out StationConfig stationConfig)
        {
            if (stationName == null || string.IsNullOrEmpty(json) || json[0] != '{')
            {
                stationConfig = null;
                return false;
            }
            try
            {
                stationConfig=this[stationName] = JsonConvert.DeserializeObject<StationConfig>(json);
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
}