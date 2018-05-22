using System;
using System.Runtime.Serialization;
using System.Text;
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
        /// <summary>
        /// 服务名称
        /// </summary>
        [DataMember, JsonProperty("serviceName")]
        public string ServiceName { get; set; }

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
        [DataMember, JsonProperty("realName")]
        private string _realName;
        /// <summary>
        /// 实例名称
        /// </summary>
        [DataMember, JsonProperty("serviceKey")]
        private string _serviceKey;

        /// <summary>
        /// 实例名称
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string ServiceKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_serviceKey))
                    return _serviceKey;
                return _serviceKey = RandomOperate.Generate(8);
            }
        }

        /// <summary>
        /// 实例名称
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string RealName
        {
            get
            {
                if (!string.IsNullOrEmpty(_realName))
                    return _realName;
                return _realName = $"{StationName}-{ServiceName}-{ServiceKey}";
            }
        }

        private byte[] _identity;

        /// <summary>
        /// 实例名称
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public byte[] Identity => _identity ?? (_identity = ToZeroIdentity());

        /// <summary>
        /// 是否本机
        /// </summary>
        /// <returns></returns>
        public bool IsLocalhost => string.IsNullOrWhiteSpace(ZeroAddress) ||
                                   ZeroAddress == "127.0.0.1" ||
                                   ZeroAddress == "::1" ||
                                   ZeroAddress.Equals("localhost", StringComparison.OrdinalIgnoreCase);
        /// <summary>
        /// 格式化地址
        /// </summary>
        /// <param name="station"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public string GetRemoteAddress(string station, int port)
        {
            return /*IsLocalhost
                ? $"ipc:///usr/zero/{station}.ipc"
                : */$"tcp://{ZeroApplication.Config.ZeroAddress}:{port}";
        }

        /// <summary>
        /// 格式化身份名称
        /// </summary>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public byte[] ToZeroIdentity(params string[] ranges)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('+');//IsLocalhost ? "-" : "+"
            sb.Append(RealName);
            foreach (var range in ranges)
            {
                sb.Append("-");
                sb.Append(range);
            }
            return sb.ToString().ToAsciiBytes();
        }
    }
}