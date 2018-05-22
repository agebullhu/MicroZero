using System;
using System.Collections.Generic;
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
    public class RouteData : NetData,IPublishData
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        [IgnoreDataMember, JsonIgnore ]
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
        [DataMember, JsonProperty("uri")]
        public Uri Uri;


        /// <summary>
        /// 执行状态
        /// </summary>
        [DataMember, JsonProperty("status")]
        public RouteStatus Status;
        /// <summary>
        /// 执行HTTP重写向吗
        /// </summary>
        [DataMember, JsonProperty("redirect")]
        public bool Redirect;
        /// <summary>
        ///     返回值
        /// </summary>
        [DataMember, JsonProperty("result")]
        public string ResultMessage;

        /// <summary>
        ///     Http Header中的Authorization信息
        /// </summary>
        [DataMember, JsonProperty("bear")]
        public string Bearer;

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
        ///     请求的内容
        /// </summary>
        [DataMember, JsonProperty("context")]
        public string Context;

        /// <summary>
        ///     请求的参数
        /// </summary>
        [DataMember, JsonProperty("queryString")]
        public string QueryString;

        /// <summary>
        ///     请求的表单
        /// </summary>
        [DataMember, JsonProperty("headers")]
        public Dictionary< string,List<string>> Headers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     请求的表单
        /// </summary>
        [DataMember, JsonProperty("form")]
        public string Form;
        
        /// <summary>
        ///     HTTP method
        /// </summary>
        [DataMember, JsonProperty("method")]
        public string HttpMethod;

        /// <summary>
        /// 是否正常
        /// </summary>
        [DataMember, JsonProperty("succeed")]
        public bool IsSucceed { get; set; }


    }
}