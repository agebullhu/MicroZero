using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 表示网络数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public interface INetData
    {
        /// <summary>
        /// 机器
        /// </summary>
        [DataMember, JsonProperty("machine")]
        String Machine { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        [DataMember, JsonProperty("user")]
        String User { get; set; }

        /// <summary>
        /// 请求ID
        /// </summary>
        [DataMember, JsonProperty("requestId")]
        String RequestId { get; set; }
    }
}