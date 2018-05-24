using System;
using System.Runtime.Serialization;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{

    /// <summary>
    /// 一次路由执行状态
    /// </summary>
    public enum RouteStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        None,
        /// <summary>
        /// 缓存
        /// </summary>
        Cache,
        /// <summary>
        /// Http的OPTION协商
        /// </summary>
        HttpOptions,
        /// <summary>
        /// 非法格式
        /// </summary>
        FormalError,
        /// <summary>
        /// 本地错误
        /// </summary>
        LocalError,
        /// <summary>
        /// 远程错误
        /// </summary>
        RemoteError,
        /// <summary>
        /// 非法请求
        /// </summary>
        DenyAccess
    }
    /// <summary>
    /// 路由数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract]
    internal class CountData : NetData
    {

        /// <summary>
        /// 开始时间
        /// </summary>
        [DataMember, JsonProperty("start")]
        public DateTime Start ;

        /// <summary>
        /// 结束时间
        /// </summary>
        [DataMember, JsonProperty("end")]
        public DateTime End ;

        /// <summary>
        /// 请求地址
        /// </summary>
        [DataMember, JsonProperty("requester")]
        public string Requester;

        /// <summary>
        ///     当前请求调用的主机名称
        /// </summary>
        [DataMember, JsonProperty("host")]
        public string HostName;

        /// <summary>
        ///     当前请求调用的API名称
        /// </summary>
        [DataMember, JsonProperty("apiName")]
        public string ApiName;

        /// <summary>
        /// 是否正常
        /// </summary>
        [DataMember, JsonProperty("succeed")]
        public bool IsSucceed ;


        /// <summary>
        /// 执行状态
        /// </summary>
        [DataMember, JsonProperty("status")]
        public RouteStatus Status;
    }
}