using System;
using System.Collections.Generic;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 运行时警告
    /// </summary>
    public class RuntimeWaring
    {
        /// <summary>
        ///     刷新
        /// </summary>
        public static void Flush()
        {
            Publisher.Publish("PerformanceCounter", "RuntimeWaring", "*Flush");
        }
        
        /// <summary>
        /// 运行时警告
        /// </summary>
        /// <param name="host"></param>
        /// <param name="api"></param>
        /// <param name="message"></param>
        public static void Waring(string host, string api, string message)
        {
            if (AppConfig.Config.SmsConfig == null || AppConfig.Config.SmsConfig.CycleHours <= 0)
                return;
            Publisher.Publish("PerformanceCounter", "RuntimeWaring", JsonConvert.SerializeObject(
            new{
                host,
                api,
                message
            }));
        }
        
    }

}