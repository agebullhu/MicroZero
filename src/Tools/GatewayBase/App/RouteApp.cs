using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.Common.Logging;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroManagemant;
using Agebull.MicroZero.PubSub;
using Agebull.MicroZero.ZeroApis;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     调用映射核心类
    /// </summary>
    public class RouteApp
    {
        #region 初始化

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize()
        {
            ZeroApplication.Initialize();
            RouteCache.InitCache();
            ZeroApplication.ZeroNetEvent += async (sender, e) => await OnZeroNetEvent(sender, e);
            ZeroApplication.Run();
        }

        /// <summary>
        ///     刷新
        /// </summary>
        public static void Flush()
        {
            RouteCache.Flush();
        }

        /// <summary>
        /// 配置HTTP
        /// </summary>
        /// <param name="options"></param>
        public static void Options(KestrelServerOptions options)
        {
            options.AddServerHeader = true;
            //将此选项设置为 null 表示不应强制执行最低数据速率。
            options.Limits.MinResponseDataRate = null;
            var httpOptions = ConfigurationManager.Root.GetSection("http").Get<HttpOption[]>();
            foreach (var option in httpOptions)
            {
                if (option.IsHttps)
                {
                    var filename = option.CerFile[0] == '/'
                        ? option.CerFile
                        : Path.Combine(Environment.CurrentDirectory, option.CerFile);
                    var certificate = new X509Certificate2(filename, option.CerPwd);
                    options.Listen(IPAddress.Any, option.Port, listenOptions => { listenOptions.UseHttps(certificate); });
                }
                else
                {
                    options.Listen(IPAddress.Any, option.Port);
                }
            }
        }
        #endregion

        #region 基本调用

        /// <summary>
        ///     POST调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task Call(HttpContext context)
        {
            //跨域支持
            if (string.Equals(context.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                return Task.Factory.StartNew(() => HttpProtocol.CrosOption(context.Response));
            }
            else
            {
                /*
                在ASP.Net Core的机制中，当接收到http的头为 application/x-www-form-urlencoded 或者 multipart/form-data 时，
                netcore会通过 FormReader 预先解析 Request.Body 的 Form 的内容，经过 Reader 读取后 Request.Body 就会变 null，
                这样我们在代码中需要再次使用 Request.Body 时就会报空异常。
                */
                //context.Request.EnableRewind();

                return CallTask(context);
            }
        }

        /// <summary>
        /// 修改缺省的路由对象
        /// </summary>
        public static Func<IRouter> CreateDefaultRouter { get; set; } = () => new Router();

        /// <summary>
        /// 特殊路径使用的路由器
        /// </summary>
        public static Dictionary<string, Func<IRouter>> Extends = new Dictionary<string, Func<IRouter>>();

        /// <summary>
        ///     调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static async Task CallTask(HttpContext context)
        {
            HttpProtocol.CrosCall(context.Response);
            var uri = context.Request.GetUri();
            if (uri.AbsolutePath == "/")
            {
                //response.Redirect("/index.html");
                await context.Response.WriteAsync("Wecome MicroZero!", Encoding.UTF8);
                return;
            }

            HttpProtocol.FormatResponse(context.Request, context.Response);
            AshxMapConfig map = null;
            if (RouteOption.Option.SystemConfig.EnableContext)
            {
                var folders = uri.AbsolutePath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                if (folders.Length == 0)
                {
                    //response.Redirect("/index.html");
                    await context.Response.WriteAsync("Wecome MicroZero!", Encoding.UTF8);
                    return;
                }
                var ext = Path.GetExtension(folders[folders.Length - 1]);
                if (!string.IsNullOrWhiteSpace(ext))
                {
                    if (!RouteOption.Option.UrlMap.TryGetValue(ext, out map))
                    {
                        var cr = new ContentRouter(context);
                        await cr.WriteContent(folders, ext);
                        return;
                    }
                }
            }

            if (RouteOption.Option.SystemConfig.EnableInnerCommand && InnerCommand(uri.AbsolutePath, context.Request, context.Response))
            {
                LogRecorderX.MonitorTrace("InnerCommand");
                return;
            }
            //命令
            using (MonitorScope.CreateScope(uri.AbsolutePath))
            {
                var router = Extends.TryGetValue(uri.AbsolutePath, out var creater) ? creater() : CreateDefaultRouter();
                try
                {
                    //开始调用
                    if (await router.Prepare(context))
                    {
                        if (map != null)
                            router.CheckMap(map);
                        // 正常调用
                        await router.Call();
                    }
                    // 写入返回
                    await router.WriteResult();
                }
                catch (Exception e)
                {
                    await router.OnError(e, context);
                }
                finally
                {
                    router.End();
                }
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
                    ApiResult.CacheData(datas[index]);
                }
            }
        }*/

        #endregion

        #region 内部命令处理

        /// <summary>
        ///     内部命令处理
        /// </summary>
        /// <param name="url"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static bool InnerCommand(string url, HttpRequest request, HttpResponse response)
        {
            //命令
            switch (url)
            {
                case "/_1_clear_1_":
                    HttpProtocol.FormatResponse(request, response);
                    RouteCache.Flush();
                    response.WriteAsync(JsonConvert.SerializeObject(RouteOption.Option, Formatting.Indented),
                        Encoding.UTF8);
                    return true;
                //case "/_1_counter_1_/info":
                //    response.WriteAsync(JsonConvert.SerializeObject(RouteCounter.Station, Formatting.Indented), Encoding.UTF8);
                //    return true;
                //case "/_1_counter_1_/save":
                //    RouteCounter.Save();
                //    response.WriteAsync(JsonConvert.SerializeObject(RouteCounter.Station, Formatting.Indented), Encoding.UTF8);
                //    return true;
                case "/_1_config_1_":
                    HttpProtocol.FormatResponse(request, response);
                    response.WriteAsync(JsonConvert.SerializeObject(RouteOption.Option, Formatting.Indented),
                        Encoding.UTF8);
                    return true;
                case "/publish":
                    HttpProtocol.FormatResponse(request, response);
                    var suc = ZeroPublisher.Publish(request.Form["Host"], request.Form["Title"], request.Form["Sub"], request.Form["Arg"]);
                    response.WriteAsync(suc ? ApiResultIoc.SucceesJson : ApiResultIoc.NetworkErrorJson, Encoding.UTF8);
                    return true;
                    //case "/_1_warings_1_":
                    //    response.WriteAsync(JsonConvert.SerializeObject(RuntimeWaring.WaringsTime, Formatting.Indented), Encoding.UTF8);
                    //    return true;
                    //case "/_1_cache_1_":
                    //    response.WriteAsync(JsonConvert.SerializeObject(RouteChahe.Cache, Formatting.Indented), Encoding.UTF8);
                    //    return true;
            }

            return false;
        }

        #endregion

        #region OnZeroNetEvent

        private static async Task OnZeroNetEvent(object sender, ZeroNetEventArgument e)
        {
            switch (e.Event)
            {
                case ZeroNetEventType.AppRun:
                case ZeroNetEventType.ConfigUpdate:
                    await OnZeroNetRuning();
                    return;
                case ZeroNetEventType.AppStop:
                    OnZeroNetClose();
                    return;
                case ZeroNetEventType.CenterStationDocument:
                    if (!e.EventConfig.IsApi)
                        return;
                    if (RouteOption.RouteMap.TryGetValue(e.EventConfig.StationName, out var host))
                    {
                        await UpdateApiItems(host as ZeroHost);
                    }
                    break;
                case ZeroNetEventType.CenterStationJoin:
                case ZeroNetEventType.CenterStationInstall:
                case ZeroNetEventType.CenterStationResume:
                case ZeroNetEventType.CenterStationUpdate:
                    if (!e.EventConfig.IsApi)
                        return;
                    await StationJoin(e.EventConfig);
                    break;
                case ZeroNetEventType.CenterStationLeft:
                case ZeroNetEventType.CenterStationPause:
                case ZeroNetEventType.CenterStationClosing:
                case ZeroNetEventType.CenterStationRemove:
                case ZeroNetEventType.CenterStationStop:
                    if (!e.EventConfig.IsApi)
                        return;
                    StationLeft(e.EventConfig);
                    break;
            }

            //if (!((DateTime.Now - _preUpdate).TotalMinutes > 5))
            //    return;
            //LogRecorderX.SystemLog($"Reload Document by {e.Event}.");
            //OnZeroNetRuning();
        }

        private static async Task OnZeroNetRuning()
        {
            //Console.WriteLine("lock (_configs)");
            var cfgs = ZeroApplication.Config.GetConfigs(p => p.IsApi && !string.IsNullOrWhiteSpace(p.StationName));
            foreach (var config in cfgs)
                await StationJoin(config);
        }

        private static void OnZeroNetClose()
        {
            foreach (var host in RouteOption.RouteMap.Where(p => p.Value.ByZero).ToArray())
            {
                host.Value.Failed = true;
                host.Value.Description = "OnZeroNetClose";
            }
        }

        private static void StationLeft(StationConfig station)
        {
            if (RouteOption.RouteMap.TryGetValue(station.StationName, out var host))
            {
                host.Failed = true;
                host.Description = "StationLeft";
            }
        }


        private static async Task StationJoin(StationConfig station)
        {
            ZeroHost zeroHost;
            if (RouteOption.RouteMap.TryGetValue(station.StationName, out var h))
            {
                zeroHost = h as ZeroHost;
                if (zeroHost == null) RouteOption.RouteMap[station.StationName] = zeroHost = new ZeroHost();
            }
            else
            {
                RouteOption.RouteMap.TryAdd(station.StationName, zeroHost = new ZeroHost());
            }
            zeroHost.Description = null;
            zeroHost.ByZero = true;
            zeroHost.Failed = false;
            zeroHost.Station = station.StationName;

            await UpdateApiItems(zeroHost);

            if (!string.IsNullOrWhiteSpace(station.ShortName))
            {
                if (RouteOption.RouteMap.ContainsKey(station.ShortName))
                    RouteOption.RouteMap[station.ShortName] = zeroHost;
                else
                    RouteOption.RouteMap.TryAdd(station.ShortName, zeroHost);
            }
            if (station.StationAlias == null)
                return;
            foreach (var alia in station.StationAlias)
                if (RouteOption.RouteMap.ContainsKey(alia))
                    RouteOption.RouteMap[alia] = zeroHost;
                else
                    RouteOption.RouteMap.TryAdd(alia, zeroHost);
        }

        private static async Task UpdateApiItems(ZeroHost zeroHost)
        {
            var doc = await SystemManager.Instance.LoadDocument(zeroHost.Station);
            if (doc == null)
            {
                zeroHost.Apis = null;
                return;
            }

            if (zeroHost.Apis == null)
                zeroHost.Apis = new Dictionary<string, ApiItem>(StringComparer.OrdinalIgnoreCase);
            foreach (var api in doc.Aips.Values)
                if (zeroHost.Apis.TryGetValue(api.RouteName, out var item))
                {
                    item.App = zeroHost.AppName;
                    item.Access = api.AccessOption;
                }
                else
                {
                    zeroHost.Apis.Add(api.RouteName, new ApiItem
                    {
                        Name = api.RouteName,
                        App = zeroHost.AppName,
                        Access = api.AccessOption
                    });
                }
        }

        #endregion
    }
}