using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Agebull.MicroZero;
using Newtonsoft.Json;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     路由主机
    /// </summary>
    public class RouteHost
    {
        /// <summary>
        /// 总等待数
        /// </summary>
        [IgnoreDataMember]
        public static int WaitCount;

        /// <summary>
        ///     使用MicroZero通讯吗
        /// </summary>
        public bool ByZero { get; set; }
        
        /// <summary>
        ///     是否异常
        /// </summary>
        [IgnoreDataMember]
        [JsonIgnore] public bool Failed { get; set; }

        /// <summary>
        ///     说明
        /// </summary>
        [IgnoreDataMember]
        [JsonIgnore] public string Description { get; set; }
        
        /// <summary>
        ///     别名
        /// </summary>
        public string[] Alias { get; set; }
    }
    /// <summary>
    ///     路由主机
    /// </summary>
    public class ZeroHost : RouteHost
    {
        /// <summary>
        ///     站点
        /// </summary>
        [IgnoreDataMember]
        public string Station { get; set; }

        /// <summary>
        ///     站点
        /// </summary>
        [IgnoreDataMember]
        public StationConfig Config { get; set; }

        /// <summary>
        ///     适用于的App的名称组合，全适合则为null，部分适用为用逗号分隔的App名称
        /// </summary>
        [IgnoreDataMember]
        public string AppName { get; set; }

        /// <summary>
        ///  Api
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, ApiItem> Apis { get; set; }
    }

    /// <summary>
    ///     路由主机
    /// </summary>
    public class HttpHost : RouteHost
    {
        /// <summary>
        ///     默认主机
        /// </summary>
        public static HttpHost DefaultHost;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="config"></param>
        public HttpHost(HostConfig config)
        {
            ByZero = false;
            Hosts = config.Hosts;
            Alias = config.Alias;
            Alias = config.Alias;
        }

        /// <summary>
        ///     下一次命中的主机
        /// </summary>
        [JsonIgnore] public int Next { get; set; }

        /// <summary>
        ///     主机列表
        /// </summary>
        public string[] Hosts { get; set; }
    }

    /// <summary>
    ///     路由主机
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    [Serializable]
    public class HostConfig
    {
        /// <summary>
        ///     使用MicroZero通讯
        /// </summary>
        [DataMember]
        [JsonProperty("zero", NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Zero { get; set; }

        /// <summary>
        ///     主机列表
        /// </summary>
        [DataMember]
        [JsonProperty("hosts", NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Hosts { get; set; }

        /// <summary>
        ///     别名
        /// </summary>
        [DataMember]
        [JsonProperty("alias", NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Alias { get; set; }

    }
}