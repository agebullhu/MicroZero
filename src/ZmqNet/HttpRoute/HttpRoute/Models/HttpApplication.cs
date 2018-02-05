using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 调用映射核心类
    /// </summary>
    public class HttpApplication
    {
        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize()
        {
            // 日志支持
            //Agebull.Common.Logging.LogRecorder.GetRequestIdFunc = () => ApiContext.RequestContext.RequestId;.
            LogRecorder.Initialize();
            AppConfig.Initialize();
            StationProgram.Run();
            RouteRuntime.Flush();
            ZeroFlush();
            //Datas = new List<RouteData>();

        }
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Flush()
        {
            AppConfig.Initialize();
            RouteRuntime.Flush();
            ZeroFlush();
            //Datas = new List<RouteData>();

        }

        #endregion

        #region ZeroNet


        /// <summary>
        /// 初始化
        /// </summary>
        public static void ZeroFlush()
        {
            if (!AppConfig.Config.SystemConfig.FireZero)
                return;

            StationProgram.StationEvent -= StationProgram_StationEvent;

            foreach (var host in AppConfig.Config.RouteMap.Where(p => p.Value.ByZero).ToArray())
                AppConfig.Config.RouteMap.Remove(host.Key);

            foreach (var station in StationProgram.Configs.Values.Where(p => p.StationType == 2))
            {
                var host = new HostConfig
                {
                    ByZero = true
                };
                if (!AppConfig.Config.RouteMap.ContainsKey(station.StationName))
                    AppConfig.Config.RouteMap.Add(station.StationName, host);
                foreach (var name in station.StationAlias)
                    if (!AppConfig.Config.RouteMap.ContainsKey(name))
                        AppConfig.Config.RouteMap.Add(name, host);
            }
            StationProgram.StationEvent += StationProgram_StationEvent;
        }
        private static void StationProgram_StationEvent(object sender, StationProgram.StationEventArgument e)
        {
            switch (e.EventName)
            {
                case "system_start":
                    break;
                case "system_stop":
                    foreach (var host in AppConfig.Config.RouteMap.Where(p => p.Value.ByZero).ToArray())
                        AppConfig.Config.RouteMap.Remove(host.Key);
                    break;
                case "worker_heat":
                case "station_resume":
                case "station_install":
                case "station_join":
                    var route = new HostConfig
                    {
                        ByZero = true
                    };
                    if (!AppConfig.Config.RouteMap.ContainsKey(e.EventConfig.StationName))
                        AppConfig.Config.RouteMap.Add(e.EventConfig.StationName, route);
                    foreach (var name in e.EventConfig.StationAlias)
                        if (!AppConfig.Config.RouteMap.ContainsKey(name))
                            AppConfig.Config.RouteMap.Add(name, route);
                    break;
                case "station_left":
                    break;
                case "station_pause":
                case "station_closing":
                    AppConfig.Config.RouteMap.Remove(e.EventConfig.StationName);
                    foreach (var name in e.EventConfig.StationAlias)
                        AppConfig.Config.RouteMap.Remove(name);
                    break;
            }
        }

        #endregion

        #region 基本调用

        /// <summary>
        /// POST调用
        /// </summary>
        /// <param name="h"></param>
        /// <returns></returns>
        public static Task Call(HttpContext h)
        {
            return Task.Factory.StartNew(CallTask, h);
        }

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static void CallTask(object arg)
        {
            CallTask((HttpContext)arg);
        }

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static void CallTask(HttpContext context)
        {
            var uri = context.Request.GetUri();
            try
            {
                context.Response.ContentType = "text/plain; charset=utf-8";
                context.Response.Headers["Content-Type"] = "text/plain; charset=utf-8";
                //内容页转向
                if (uri.LocalPath.IndexOf(".", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    context.Response.Redirect(AppConfig.Config.SystemConfig.ContextHost + uri.LocalPath.Trim('/'));
                    return;
                }
                //跨域支持
                if (context.Request.Method.ToUpper() == "OPTIONS")
                {
                    Cros(context.Response);
                    return;
                }
                //命令
                switch (uri.LocalPath)
                {
                    case "/":
                        context.Response.WriteAsync("Wecome ZeroNet Http Router!", Encoding.UTF8);
                        return;
                    case "/_1_clear_1_":
                        Flush();
                        context.Response.WriteAsync(JsonConvert.SerializeObject(AppConfig.Config, Formatting.Indented), Encoding.UTF8);
                        return;
                    case "/_1_counter_1_/info":
                        context.Response.WriteAsync(JsonConvert.SerializeObject(RouteCounter.Station, Formatting.Indented), Encoding.UTF8);
                        return;
                    case "/_1_counter_1_/save":
                        RouteCounter.Save();
                        context.Response.WriteAsync(JsonConvert.SerializeObject(RouteCounter.Station, Formatting.Indented), Encoding.UTF8);
                        return;
                    case "/_1_config_1_":
                        context.Response.WriteAsync(JsonConvert.SerializeObject(AppConfig.Config, Formatting.Indented), Encoding.UTF8);
                        return;
                    case "/_1_warings_1_":
                        context.Response.WriteAsync(JsonConvert.SerializeObject(RouteRuntime.WaringsTime, Formatting.Indented), Encoding.UTF8);
                        return;
                    case "/_1_cache_1_":
                        context.Response.WriteAsync(JsonConvert.SerializeObject(RouteRuntime.Cache, Formatting.Indented), Encoding.UTF8);
                        return;
                }
                
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                RouteRuntime.RuntimeWaring("Route", uri.LocalPath, e.Message);
                context.Response.WriteAsync(RouteRuntime.InnerError, Encoding.UTF8);
                return;
            }

            var router = new HttpRouter(context);
            HttpIoLog.OnBegin(context.Request, router.Data);
            try
            {
                var counter = RouteCounter.Begin();
                // 正常调用
                router.Call();
                LogRecorder.BeginStepMonitor("End");
                // 写入返回
                router.WriteResult();
                // 缓存
                RouteRuntime.CacheResult(router.Data);
                //计时
                counter.End(router.Data);
            }
            catch (Exception e)
            {
                router.Data.Status = RouteStatus.LocalError;
                LogRecorder.Exception(e);
                RouteRuntime.RuntimeWaring("Route", uri.LocalPath, e.Message);
                context.Response.WriteAsync(RouteRuntime.InnerError, Encoding.UTF8);
            }
            finally
            {
                HttpIoLog.OnEnd(router.Data.Status, router.Data.ResultMessage);
            }
        }
        #endregion
        #region 跨域支持

        /// <summary>
        ///     跨域支持
        /// </summary>
        private static void Cros(HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Methods", new[] { "GET", "POST" });
            response.Headers.Add("Access-Control-Allow-Headers",
                new[] { "x-requested-with", "content-type", "authorization", "*" });
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        #endregion

        #region 调用后处理
        /*
        /// <summary>
        /// 数据
        /// </summary>
        internal static List<RouteData> Datas = new List<RouteData>();

        public static Mutex Mutex = new Mutex();

        /// <summary>
        /// 流程结束后的处理
        /// </summary>
        internal static void PushFlowExtend(RouteData data)
        {
            Datas.Add(data);
            Mutex.ReleaseMutex();
        }

        /// <summary>
        /// 流程结束后的处理
        /// </summary>
        internal static void FlowExtendTask()
        {
            while (true)
            {
                if (!Mutex.WaitOne(1000))
                {
                    continue;
                }
                var datas = Datas;
                Datas = new List<RouteData>();
                Mutex.ReleaseMutex();
                //最多处理后50行
                var index = 0;
                if (datas.Count > 50)
                {
                    index = datas.Count - 50;
                }
                for (; index < datas.Count; index++)
                {
                    RouteRuntime.CacheData(datas[index]);
                }
            }
        }*/

        #endregion
    }
}