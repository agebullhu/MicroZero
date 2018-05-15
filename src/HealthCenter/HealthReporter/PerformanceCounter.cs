using System;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 性能计数器
    /// </summary>
    public class PerformanceCounter
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime Start { get; set; }
        
        /// <summary>
        /// 开始计数
        /// </summary>
        /// <returns></returns>
        public static PerformanceCounter OnBegin(RouteData data)
        {
            data.Machine = ApiContext.MyServiceName;
            data.User = ApiContext.Customer?.Account ?? "Unknow";
            data.RequestId = ApiContext.RequestContext.RequestId;
            data.Start = DateTime.Now;
            return new PerformanceCounter();
        }

        /// <summary>
        /// 开始计数
        /// </summary>
        /// <returns></returns>
        public void End(RouteData data)
        {
            data.End = DateTime.Now;
            ZeroPublisher.Publish("HealthCenter", "PerformanceCounter", "Counter", JsonConvert.SerializeObject(data));
        }


        /// <summary>
        /// 保存为性能日志
        /// </summary>
        public static void Save()
        {
            ZeroPublisher.Publish("HealthCenter", "PerformanceCounter", "Save",null);
        }
    }
}