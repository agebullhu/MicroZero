using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

// ReSharper disable All


namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 表示网络数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract]
    public class NetData : INetData
    {
        /// <summary>
        /// 机器
        /// </summary>
        [DataMember, JsonProperty("machine")]
        public String Machine { get; set; }

        /// <summary>
        /// 站点
        /// </summary>
        [DataMember, JsonProperty("station")]
        public String Station { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        [DataMember, JsonProperty("user")]
        public String User { get; set; }

        /// <summary>
        /// 请求ID
        /// </summary>
        [DataMember, JsonProperty("requestId")]
        public String RequestId { get; set; }

    }
}
