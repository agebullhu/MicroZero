using System;
using System.Runtime.Serialization;
using Gboxt.Common.DataModel;
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
#pragma warning disable CS0649
        /// <summary>
        /// 实例名称
        /// </summary>
        private string _realName;
        /// <summary>
        /// Zmq标识
        /// </summary>
        private byte[] _identity;

        /// <summary>
        /// 实例名称
        /// </summary>
        [DataMember, JsonProperty("serviceKey")]
        private string _serviceKey;
        /// <summary>
        /// 站点名称，注意唯一性
        /// </summary>
        [DataMember, JsonProperty("shortName")]
        private string _shortName;
#pragma warning restore CS0649

        /// <summary>
        /// 服务名称
        /// </summary>
        [DataMember, JsonProperty("serviceName")]
        public string ServiceName { get; set; }

        /// <summary>
        /// 短名称
        /// </summary>
        public string ShortName => _shortName;

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
        /// 本地数据文件夹
        /// </summary>
        [DataMember, JsonProperty("dataFolder")]
        public string DataFolder { get; set; }

        /// <summary>
        /// 实例名称
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string ServiceKey => _serviceKey ?? (_serviceKey = RandomOperate.Generate(4));

        /// <summary>
        /// 实例名称
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string RealName => _realName ?? (_realName = ZeroIdentityHelper.CreateRealName());

        /// <summary>
        /// Zmq标识
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public byte[] Identity => _identity ?? (_identity = ZeroIdentityHelper.ToZeroIdentity());

    }
}