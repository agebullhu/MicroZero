using System;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 性能计数器
    /// </summary>
    public class ApiCounter : Publisher<RouteData>
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
            data.Title = "ApiCounter";
            data.Machine = ApiContext.MyServiceName;
            data.User = ApiContext.Customer?.Account ?? "Unknow";
            data.RequestId = ApiContext.RequestContext.RequestId;
            data.End = DateTime.Now;
            Instance.Publish(data);
        }
    }
}