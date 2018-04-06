using System;
using Agebull.ZeroNet.Core;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 路由计数器
    /// </summary>
    public class RouteCounter
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime Start { get; set; }
        
        /// <summary>
        /// 开始计数
        /// </summary>
        /// <returns></returns>
        public static RouteCounter OnBegin(RouteData data)
        {
            data.Start = DateTime.Now;
            return new RouteCounter();
        }

        /// <summary>
        /// 开始计数
        /// </summary>
        /// <returns></returns>
        public void End(RouteData data)
        {
            data.End = DateTime.Now;
            Publisher.Publish("PerformanceCounter", "RouteCounter", JsonConvert.SerializeObject(data));
        }


        /// <summary>
        /// 保存为性能日志
        /// </summary>
        public static void Save()
        {
            Publisher.Publish("PerformanceCounter", "RouteCounter", "*Save");
        }
    }
}