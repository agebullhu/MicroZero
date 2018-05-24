using System;
using System.Runtime.Serialization;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 性能计数器
    /// </summary>
    public class ApiCounter : Publisher<CountData>
    {
        private static readonly ApiCounter Instance = new ApiCounter
        {
            Name = "ApiCounter",
            StationName = "HealthCenter"
        };

        /// <summary>
        /// 开始计数
        /// </summary>
        /// <returns></returns>
        public static void OnBegin(RouteData data)
        {
            data.Start = DateTime.Now;
        }

        /// <summary>
        /// 开始计数
        /// </summary>
        /// <returns></returns>
        public static void End(RouteData data)
        {
            Instance.Publish(new CountData
            {
                Title = "ApiCounter",
                Machine = ApiContext.MyServiceName,
                User = ApiContext.Customer?.Account ?? "Unknow",
                RequestId = ApiContext.RequestContext.RequestId,
                Start = data.Start,
                End = DateTime.Now,
                HostName = data.HostName,
                ApiName = data.ApiName,
                IsSucceed = data.IsSucceed,
                Status = data.Status,
                Requester = $"Web://{ApiContext.RequestContext.Ip}:{ApiContext.RequestContext.Port}"
            });
        }
    }

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
        public bool IsSucceed { get; set; }


        /// <summary>
        /// 执行状态
        /// </summary>
        [DataMember, JsonProperty("status")]
        public RouteStatus Status;
    }
}