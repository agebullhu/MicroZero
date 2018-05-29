using System;
using System.Runtime.Serialization;
using Agebull.ZeroNet.PubSub;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 统计数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract]
    public class CountData : NetData, IPublishData
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        [DataMember, JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [DataMember, JsonProperty("start")]
        public DateTime Start { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [DataMember, JsonProperty("end")]
        public DateTime End { get; set; }

        /// <summary>
        /// 执行的全局标识
        /// </summary>
        [DataMember, JsonProperty("toId")]
        public String ToId { get; set; }

        /// <summary>
        /// 请求的全局标识
        /// </summary>
        [DataMember, JsonProperty("fromId")]
        public String FromId { get; set; }

        /// <summary>
        /// 请求地址
        /// </summary>
        [DataMember, JsonProperty("requester")]
        public string Requester { get; set; }

        /// <summary>
        ///     当前请求调用的主机名称
        /// </summary>
        [DataMember, JsonProperty("host")]
        public string HostName { get; set; }

        /// <summary>
        ///     当前请求调用的API名称
        /// </summary>
        [DataMember, JsonProperty("apiName")]
        public string ApiName { get; set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        [DataMember, JsonProperty("status")]
        public OperatorStatus Status { get; set; }
    }
}