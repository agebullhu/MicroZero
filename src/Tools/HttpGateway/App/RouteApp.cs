using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
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
        #region 初始化

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize()
        {
            ZeroApplication.Initialize();
            RouteOption.CheckOption();
            RouteChahe.InitCache();
            ZeroApplication.ZeroNetEvent += OnZeroNetEvent;
            ZeroApplication.Run();
        }

        /// <summary>
        ///     刷新
        /// </summary>
        public static void Flush()
        {
            RouteChahe.Flush();
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
            HttpContext context = (HttpContext)arg;
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
                    bool suc = ZeroPublisher.Publish(context.Request.Form["Host"], context.Request.Form["Title"], context.Request.Form["Sub"], (string)context.Request.Form["Arg"]);
                    
                    context.Response.WriteAsync(suc ? ApiResult.SucceesJson : ApiResult.NetworkErrorJson, Encoding.UTF8);

                    return;
                }
                //内容页转向
                if (router.Data.Uri.LocalPath.IndexOf(".", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    context.Response.Redirect(RouteOption.Option.SystemConfig.ContextHost +
                                              router.Data.Uri.LocalPath.Trim('/'));
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
                }
                else
                {
                    // 正常调用
                    router.Call();
                    // 写入返回
                    router.WriteResult();
                    // 缓存
                    RouteChahe.CacheResult(router.Data);
                }
            }
            catch (Exception e)
            {
                router.Data.Status = ZeroOperatorStatus.LocalException;
                ZeroTrace.WriteException("Route", e);
                IocHelper.Create<IRuntimeWaring>()?.Waring("Route", router.Data.Uri.LocalPath, e.Message);
                context.Response.WriteAsync(ApiResult.LocalErrorJson, Encoding.UTF8);
            }
            finally
            {
                IoHandler.OnEnd(router.Data);
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
                    RouteChahe.Flush();
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

        #region OnZeroNetEvent

        private static void OnZeroNetEvent(object sender, ZeroNetEventArgument e)
        {
            switch (e.Event)
            {
                case ZeroNetEventType.AppRun:
                    OnZeroNetRuning();
                    break;
                case ZeroNetEventType.AppStop:
                    OnZeroNetClose();
                    break;
                case ZeroNetEventType.CenterStationJoin:
                case ZeroNetEventType.CenterStationResume:
                case ZeroNetEventType.CenterStationUpdate:
                    StationJoin(e.EventConfig);
                    break;
                case ZeroNetEventType.CenterStationLeft:
                case ZeroNetEventType.CenterStationPause:
                case ZeroNetEventType.CenterStationClosing:
                case ZeroNetEventType.CenterStationRemove:
                    StationLeft(e.EventConfig);
                    break;
            }
        }

        private static void OnZeroNetRuning()
        {
            ZeroApplication.Config.Foreach(config =>
            {
                if (config.StationType == ZeroStationType.Api)
                    StationJoin(config);
            });
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
            SetZeroHost(station, station.StationName);
            SetZeroHost(station, station.ShortName);
            if (station.StationAlias == null)
                return;
            foreach (var alia in station.StationAlias)
                SetZeroHost(station, alia);
        }
        private static void SetZeroHost(StationConfig station, string name)
        {
            if (string.IsNullOrEmpty(name))
                return;
            name = name.Trim();
            ZeroHost zeroHost;
            if (Router.RouteMap.TryGetValue(name, out var host))
            {
                zeroHost = host as ZeroHost;
                if (zeroHost == null)
                {
                    Router.RouteMap[name] = zeroHost = new ZeroHost
                    {
                        ByZero = true,
                        Station = station.StationName,
                        Alias = name != station.StationName
                               ? null
                               : station.StationAlias == null
                                   ? new string[0]
                                   : station.StationAlias.ToArray()
                    };
                }
                else
                {
                    host.Failed = false;
                    host.ByZero = true;
                    zeroHost.Station = station.StationName;
                    if (name == station.StationName)
                    {
                        foreach (var alia in host.Alias)
                        {
                            Router.RouteMap.Remove(alia);
                        }
                        host.Alias = station.StationAlias == null ? new string[0] : station.StationAlias.ToArray();
                    }
                    else
                    {
                        host.Alias = null;
                    }
                }
            }
            else
            {
                lock (Router.RouteMap)
                {
                    Router.RouteMap.Add(name, zeroHost = new ZeroHost
                    {
                        ByZero = true,
                        Station = station.StationName,
                        Alias = name != station.StationName
                            ? null
                            : station.StationAlias == null
                                ? new string[0]
                                : station.StationAlias.ToArray()
                    });
                }
            }

            zeroHost.Apis = new System.Collections.Generic.Dictionary<string, ApiItem>(StringComparer.OrdinalIgnoreCase);
            if (SystemManager.Instance.LoadDocument(station.StationName, out var doc))
            {
                foreach (var api in doc.Aips.Values)
                {
                    zeroHost.Apis.Add(api.RouteName,new ApiItem
                    {
                        Name = api.RouteName,
                        Access = api.AccessOption
                    });
                }
            }
        }
        #endregion
    }
}