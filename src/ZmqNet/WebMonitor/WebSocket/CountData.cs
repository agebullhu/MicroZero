using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{

    /// <summary>
    /// 路由数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract]
    public class CountData : NetData, IPublishData
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
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
        /// 请求地址
        /// </summary>
        [DataMember, JsonProperty("requester")]
        public string Requester;

        /// <summary>
        ///     当前请求调用的API名称
        /// </summary>
        [DataMember, JsonProperty("apiName")]
        public string ApiName;

        /// <summary>
        /// 是否正常
        /// </summary>
        [DataMember, JsonProperty("succeed")]
        public bool IsSucceed { get; set; }

    }
}