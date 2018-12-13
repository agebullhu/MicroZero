using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ZeroNet.Http.Route;

namespace ZeroNet.Http.Gateway
{
    /// <summary>
    ///     调用映射核心类
    /// </summary>
    internal class RouteApp
    {
        #region 内部命令处理

        /// <summary>
        ///     内部命令处理
        /// </summary>
        /// <param name="url"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static bool InnerCommand(string url, HttpResponse response)
        {
            //命令
            switch (url)
            {
                case "/":
                    response.WriteAsync("Wecome ZeroNet Http Router!", Encoding.UTF8);
                    return true;
                case "/_1_clear_1_":
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
                    response.WriteAsync(JsonConvert.SerializeObject(RouteOption.Option, Formatting.Indented),
                        Encoding.UTF8);
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

        #region 初始化

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize()
        {
            ZeroApplication.Initialize();
            RouteOption.CheckOption();
            RouteCache.InitCache();
            ZeroApplication.ZeroNetEvent += OnZeroNetEvent;
            ZeroApplication.Run();
        }

        /// <summary>
        ///     刷新
        /// </summary>
        public static void Flush()
        {
            RouteCache.Flush();
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
            return Task.Factory.StartNew(CallTask, context);
        }

        /// <summary>
        ///     调用
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private static void CallTask(object arg)
        {
            var context = (HttpContext)arg;
            var router = new Router(context);
            //跨域支持
            if (router.Data.HttpMethod == "OPTIONS")
            {
                HttpProtocol.Cros(context.Response);
                return;
            }

            IoHandler.OnBegin(router.Data);
            try
            {
                if (router.Data.Uri.LocalPath == "/publish")
                {
                    var suc = ZeroPublisher.Publish(context.Request.Form["Host"], context.Request.Form["Title"],
                        context.Request.Form["Sub"], (string)context.Request.Form["Arg"]);
                    context.Response.WriteAsync(suc ? ApiResult.SucceesJson : ApiResult.NetworkErrorJson,
                        Encoding.UTF8);
                    return;
                }

                //内容页转向
                if (router.Data.Uri.LocalPath.IndexOf(".", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    context.Response.Redirect(RouteOption.Option.SystemConfig.ContextHost +
                                              router.Data.Uri.LocalPath.Trim('/'));
                    return;
                }

                HttpProtocol.FormatResponse(context.Response);
                //命令
                if (InnerCommand(router.Data.Uri.LocalPath, context.Response))
                    return;

                //开始调用
                if (!router.SecurityChecker.PreCheck())
                {
                    router.Data.Status = ZeroOperatorStatus.DenyAccess;
                    context.Response.WriteAsync(ApiResult.DenyAccessJson, Encoding.UTF8);
                    return;
                }

                // 正常调用
                router.Call();
                // 写入返回
                router.WriteResult();
                // 缓存
                RouteCache.CacheResult(router.Data);
            }
            catch (Exception e)
            {
                OnError(router, e, context);
            }
            finally
            {
                IoHandler.OnEnd(router.Data);
            }
        }

        private static void OnError(Router router, Exception e, HttpContext context)
        {
            try
            {
                router.Data.Status = ZeroOperatorStatus.LocalException;
                ZeroTrace.WriteException("Route", e);
                IocHelper.Create<IRuntimeWaring>()?.Waring("Route", router.Data.Uri.LocalPath, e.Message);
                context.Response.WriteAsync(ApiResult.LocalErrorJson, Encoding.UTF8);
            }
            catch (Exception exception)
            {
                LogRecorder.Exception(exception);
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

        #region OnZeroNetEvent

        private static DateTime _preUpdate;

        private static void OnZeroNetEvent(object sender, ZeroNetEventArgument e)
        {
            switch (e.Event)
            {
                case ZeroNetEventType.AppRun:
                    OnZeroNetRuning();
                    return;
                case ZeroNetEventType.AppStop:
                    OnZeroNetClose();
                    return;
                case ZeroNetEventType.CenterStationDocument:
                    if (e.EventConfig.IsBaseStation)
                        return;
                    if (Router.RouteMap.TryGetValue(e.EventConfig.StationName, out var host))
                    {
                        UpdateApiItems(host as ZeroHost);
                    }
                    break;
                case ZeroNetEventType.CenterStationJoin:
                case ZeroNetEventType.CenterStationResume:
                case ZeroNetEventType.CenterStationUpdate:
                    if (e.EventConfig.IsBaseStation)
                        return;
                    StationJoin(e.EventConfig);
                    break;
                case ZeroNetEventType.CenterStationLeft:
                case ZeroNetEventType.CenterStationPause:
                case ZeroNetEventType.CenterStationClosing:
                case ZeroNetEventType.CenterStationRemove:
                    //case ZeroNetEventType.CenterClientLeft:
                    if (e.EventConfig.IsBaseStation)
                        return;
                    StationLeft(e.EventConfig);
                    break;
            }

            if ((DateTime.Now - _preUpdate).TotalMinutes > 5)
                OnZeroNetRuning();
        }

        private static readonly object lock_obj = new object();

        private static void OnZeroNetRuning()
        {
            lock (lock_obj)
            {
                _preUpdate = DateTime.Now;
                ZeroApplication.Config.Foreach(config =>
                {
                    if (!config.IsBaseStation)
                        StationJoin(config);
                });
            }
        }

        private static void OnZeroNetClose()
        {
            foreach (var host in Router.RouteMap.Where(p => p.Value.ByZero).ToArray())
            {
                host.Value.Failed = true;
                host.Value.ByZero = true;
            }
        }

        private static void StationLeft(StationConfig station)
        {
            if (Router.RouteMap.TryGetValue(station.StationName, out var host))
            {
                host.Failed = true;
                host.ByZero = true;
            }
        }


        private static void StationJoin(StationConfig station)
        {
            if (station.IsBaseStation || station.StationType == ZeroStationType.Notify || string.IsNullOrWhiteSpace(station.StationName))
                return;
            var host = SetZeroHost(station);
            if (Router.RouteMap.ContainsKey(station.ShortName))
                Router.RouteMap[station.ShortName] = host;
            else
                Router.RouteMap.Add(station.ShortName, host);
            if (station.StationAlias == null)
                return;
            foreach (var alia in station.StationAlias)
                if (Router.RouteMap.ContainsKey(alia))
                    Router.RouteMap[alia] = host;
                else
                    Router.RouteMap.Add(alia, host);
        }

        private static ZeroHost SetZeroHost(StationConfig station)
        {
            ZeroHost zeroHost;
            if (Router.RouteMap.TryGetValue(station.StationName, out var host))
            {
                zeroHost = host as ZeroHost;
                if (zeroHost == null) Router.RouteMap[station.StationName] = zeroHost = new ZeroHost();
            }
            else
            {
                lock (Router.RouteMap)
                {
                    Router.RouteMap.Add(station.StationName, zeroHost = new ZeroHost());
                }
            }

            zeroHost.ByZero = true;
            zeroHost.Station = station.StationName;
            zeroHost.Failed = false;

            UpdateApiItems(zeroHost);
            return zeroHost;
        }

        private static void UpdateApiItems(ZeroHost zeroHost)
        {
            if (!SystemManager.Instance.LoadDocument(zeroHost.Station, out var doc))
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