using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Agebull.ZeroNet.LogRecorder;

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
            LogRecorder.Initialize(new RemoteRecorder());
            AppConfig.Initialize(Path.Combine(Startup.Configuration["contentRoot"], "route_config.json"));
            StationProgram.Run();
            RouteChahe.Flush();
            RuntimeWaring.Flush();
            RouteCommand.ZeroFlush();
            //Datas = new List<RouteData>();

        }
        #endregion


        #region 基本调用

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
        /// POST调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task Call(HttpContext context)
        {
            return Task.Factory.StartNew(CallTask, context);
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
                HttpProtocol.FormatResponse(context.Response);
                //内容页转向
                if (uri.LocalPath.IndexOf(".", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    context.Response.Redirect(AppConfig.Config.SystemConfig.ContextHost + uri.LocalPath.Trim('/'));
                    return;
                }
                //跨域支持
                if (context.Request.Method.ToUpper() == "OPTIONS")
                {
                    HttpProtocol.Cros(context.Response);
                    return;
                }
                //命令
                if (RouteCommand.InnerCommand(uri.LocalPath, context.Response))
                    return;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                RuntimeWaring.Waring("Route", uri.LocalPath, e.Message);
                context.Response.WriteAsync(RouteRuntime.InnerError, Encoding.UTF8);
                return;
            }

            var router = new HttpRouter(context);
            
            HttpIoLog.OnBegin(router.Data);
            var counter = PerformanceCounter.OnBegin(router.Data);
            try
            {
                var checker = new SecurityChecker
                {
                    Request = context.Request
                };
                if (!checker.PreCheck())
                {
                    router.Data.Status = RouteStatus.DenyAccess;
                    context.Response.WriteAsync(RouteRuntime.Inner2Error, Encoding.UTF8);
                }
                else
                {
                    // 正常调用
                    router.Call();
                    LogRecorder.BeginStepMonitor("End");
                    // 写入返回
                    router.WriteResult();
                    // 缓存
                    RouteChahe.CacheResult(router.Data);
                }
            }
            catch (Exception e)
            {
                router.Data.Status = RouteStatus.LocalError;
                LogRecorder.Exception(e);
                RuntimeWaring.Waring("Route", uri.LocalPath, e.Message);
                context.Response.WriteAsync(RouteRuntime.InnerError, Encoding.UTF8);
            }
            finally
            {
                //计时
                counter.End(router.Data);
                HttpIoLog.OnEnd(router.Data);
            }
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