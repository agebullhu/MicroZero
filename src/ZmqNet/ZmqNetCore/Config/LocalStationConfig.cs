using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Agebull.Common.Logging;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 本地站点配置
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class LocalStationConfig
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        [DataMember, JsonProperty("serviceName")]
        public string ServiceName { get; set; }

        /// <summary>
        /// 短名称
        /// </summary>
        [DataMember, JsonProperty("shortName")]
        public string ShortName { get; set; }

        /// <summary>
        /// 站点名称，注意唯一性
        /// </summary>
        [DataMember, JsonProperty("stationName")]
        public string StationName { get; set; }

        /// <summary>
        /// ZeroNet消息中心主机IP地址
        /// </summary>
        [DataMember, JsonProperty("zeroAddr")]
        public string ZeroAddress { get; set; }

        /// <summary>
        /// ZeroNet消息中心监测站端口号
        /// </summary>
        [DataMember, JsonProperty("monitorPort")]
        public int ZeroMonitorPort { get; set; }

        /// <summary>
        /// ZeroNet消息中心管理站端口号
        /// </summary>
        [DataMember, JsonProperty("managePort")]
        public int ZeroManagePort { get; set; }

        /// <summary>
        /// 本地日志文件夹
        /// </summary>
        [DataMember, JsonProperty("logFolder")]
        public string LogFolder { get; set; }

        /// <summary>
        /// 本地数据文件夹
        /// </summary>
        [DataMember, JsonProperty("dataFolder")]
        public string DataFolder { get; set; }

        /// <summary>
        /// 实例名称
        /// </summary>
        [DataMember, JsonProperty("serviceKey")]
        public string ServiceKey { get; set; }

        /// <summary>
        /// 本机IP地址
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string LocalIpAddress { get; set; }

        /// <summary>
        /// 实例名称
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string RealName { get; set; }

        /// <summary>
        /// Zmq标识
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public byte[] Identity { get; set; }

        /// <summary>
        ///     监测中心广播地址
        /// </summary>
        public string ZeroMonitorAddress { get; set; }

        /// <summary>
        ///     监测中心管理地址
        /// </summary>
        public string ZeroManageAddress { get; set; }

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
        public StationConfig[] Getonfigs()
        {
            lock (_configs)
                return _configs.Values.ToArray();
        }
        /// <summary>
        ///     站点集合
        /// </summary>
        public StationConfig[] Getonfigs(Func<StationConfig, bool> condition)
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
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                ZeroTrace.WriteError("读取站点配置", "Exception", json, e);
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
        public bool UpdateConfig(string stationName,string json, out StationConfig stationConfig)
        {
            try
            {
                stationConfig=this[stationName] = JsonConvert.DeserializeObject<StationConfig>(json);
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                ZeroTrace.WriteError("UpdateConfig", "Exception", stationName, json, e);
                stationConfig = null;
                return false;
            }

        }
        #endregion
    }
}