using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ZmqNet.Rpc.Core.ZeroNet
{
    /// <summary>
    /// 站点配置
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class StationConfig
    {
        /// <summary>
        /// 站点名称
        /// </summary>
        [DataMember, JsonProperty("station_name", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string StationName { get; set; }
        /// <summary>
        /// 站点类型
        /// </summary>
        [DataMember, JsonProperty("station_type", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int StationType { get; set; }
        /// <summary>
        /// 入站端口
        /// </summary>
        [DataMember, JsonProperty("out_port", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int OutPort { get; set; }

        /// <summary>
        /// 入站地址
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string OutAddress => $"tcp://{StationProgram.Config.ZeroAddress}:{OutPort}";

        /// <summary>
        /// 入站端口
        /// </summary>
        [DataMember, JsonProperty("inner_port", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int InnerPort { get; set; }
        /// <summary>
        /// 出站地址
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string InnerAddress=> $"tcp://{StationProgram.Config.ZeroAddress}:{InnerPort}";
        /// <summary>
        /// 心跳端口
        /// </summary>
        [DataMember, JsonProperty("heart_port", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int HeartPort { get; set; }

        /// <summary>
        /// 心跳地址
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string HeartAddress => $"tcp://{StationProgram.Config.ZeroAddress}:{HeartPort}";

        /// <summary>
        /// 状态
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public StationState State { get; set; }
    }
}
