using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Yizuan.Service.Api;

namespace ExternalStation.Models
{
    /// <summary>
    /// 调用映射核心类
    /// </summary>
    public class HttpApplication
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize()
        {
            // 日志支持
            Agebull.Common.Logging.LogRecorder.GetRequestIdFunc = () => ApiContext.RequestContext.RequestId;
            Agebull.Common.Logging.LogRecorder.Initialize();
            RouteData.Flush();
        }
        /// <summary>
        /// POST调用
        /// </summary>
        /// <param name="h"></param>
        /// <returns></returns>
        public static Task Call(HttpContext h)
        {
            return Task.Factory.StartNew(Call, h);
        }

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static void Call(object arg)
        {
            var start = DateTime.Now;
            HttpRouter.Todo((HttpContext)arg);
            var sm = DateTime.Now - start;
            //if (sm.TotalMilliseconds > 200)
            //    LogRecorder.Warning($"执行时间异常({sm.TotalMilliseconds}):{url.LocalPath}");
            //else
                Console.WriteLine($"{((HttpContext)arg).Connection.LocalIpAddress}:{sm.TotalMilliseconds}");
        }
    }
}