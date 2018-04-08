using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 路由主机
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class HostConfig
    {
        /// <summary>
        /// 默认主机
        /// </summary>
        public static HostConfig DefaultHost;

        /// <summary>
        /// 使用ZeroNet通讯吗
        /// </summary>
        [DataMember, JsonProperty("zero", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool ByZero { get; set; }

        /// <summary>
        /// 下一次命中的主机
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public int Next { get; set; }

        /// <summary>
        /// 主机列表
        /// </summary>
        [DataMember, JsonProperty("hosts", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Hosts { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        [DataMember, JsonProperty("alias", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Alias { get; set; }

    }
}