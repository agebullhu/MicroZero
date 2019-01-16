using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using ZeroNet.Http.Route;

namespace ZeroNet.Http.Gateway
{
    /// <summary>
    ///     调用映射核心类
    /// </summary>
    internal partial class Router : IRouter
    {
        #region 路由

        /// <summary>
        ///     查找站点
        /// </summary>
        private bool FindHost()
        {
            if (!RouteMap.TryGetValue(Data.HostName, out Data.RouteHost) || Data.RouteHost == null)
            {
                LogRecorder.MonitorTrace($"{Data.HostName} no find");
                return false; //Data.RouteHost = HttpHost.DefaultHost;
            }
            if (!RouteOption.Option.SystemConfig.CheckApiItem || !(Data.RouteHost is ZeroHost host))
                return true;
            if (host.Apis == null || !host.Apis.TryGetValue(Data.ApiName, out Data.ApiItem))
            {
                LogRecorder.MonitorTrace($"{Data.ApiName} no find");
                return false; //Data.RouteHost = HttpHost.DefaultHost;
            }
            return Data.ApiItem.Access == ApiAccessOption.None || Data.ApiItem.Access.HasFlag(ApiAccessOption.Public);
        }

        /// <summary>
        ///     缓存图
        /// </summary>
        public static Dictionary<string, RouteHost> RouteMap { get; internal set; }

        #endregion

        #region 变量

        /// <summary>
        ///     Http上下文
        /// </summary>
        public HttpContext HttpContext { get; set; }

        /// <summary>
        ///     Http请求
        /// </summary>
        public HttpRequest Request { get; set; }

        /// <summary>
        ///     Http返回
        /// </summary>
        public HttpResponse Response { get; set; }

        /// <summary>
        ///     Http返回
        /// </summary>
        public RouteData Data { get; set; }

        /// <summary>
        ///     安全检查器
        /// </summary>
        public SecurityChecker SecurityChecker { get; set; }

        #endregion

        #region 流程

        /// <summary>
        ///     调用
        /// </summary>
        /// <param name="context"></param>
        bool IRouter.Prepare(HttpContext context)
        {
            LogRecorder.MonitorTrace("ApiRouter");
            HttpContext = context;
            Request = context.Request;
            Response = context.Response;
            Data = new RouteData
            {
                Uri = Request.GetUri(),
                HttpMethod = Request.Method.ToUpper()
            };
            SecurityChecker = new SecurityChecker { Data = Data };
            Data.Prepare(HttpContext);

            IoHandler.OnBegin(Data);
            if (SecurityChecker.PreCheck())
                return true;
            Data.UserState = UserOperatorStateType.DenyAccess;
            Data.ZeroState = ZeroOperatorStateType.DenyAccess;
            Data.ResultMessage = ApiResult.DenyAccessJson;
            return false;
        }

        /// <summary>
        ///     调用
        /// </summary>
        async void IRouter.Call()
        {
            if (!CheckCall())
                return;
            if (Data.HostName.Equals("Zero", StringComparison.OrdinalIgnoreCase))
            {
                var manager = new ZeroManager();
                manager.Command(Data);
                return;
            }

            // 1 初始化路由信息
            if (!FindHost() || Data.RouteHost.Failed)
            {
                Data.UserState = UserOperatorStateType.NotFind;
                Data.ZeroState = ZeroOperatorStateType.NotFind;
                Data.ResultMessage = ApiResult.NoFindJson;
                return;
            }

            // 2 安全检查
            if (!TokenCheck())
            {
                Data.UserState = UserOperatorStateType.DenyAccess;
                Data.ZeroState = ZeroOperatorStateType.DenyAccess;
                if (Data.ResultMessage == null)
                    Data.ResultMessage = ApiResult.DenyAccessJson;
                return;
            }
            // 3 缓存快速处理
            if (RouteCache.LoadCache(Data.Uri, Data.Token, out Data.CacheSetting, out Data.CacheKey, ref Data.ResultMessage))
            {
                //找到并返回缓存
                Data.UserState = UserOperatorStateType.Success;
                Data.ZeroState = ZeroOperatorStateType.Ok;
                return;
            }

            // 4 远程调用
            if (!Data.RouteHost.ByZero)
                Data.ResultMessage = await CallHttp();
            else if (ZeroApplication.ZerCenterIsRun)
                Data.ResultMessage = CallZero();
            else
            {
                Data.UserState = UserOperatorStateType.LocalError;
                Data.ZeroState = ZeroOperatorStateType.NotFind;
                Data.ResultMessage = ApiResult.NoReadyJson;
                return;
            }
            // 5 结果检查
            SecurityChecker.CheckResult(Data);
        }

        /// <summary>
        ///     检查调用内容
        /// </summary>
        /// <returns></returns>
        private bool CheckCall()
        {
            if (string.IsNullOrWhiteSpace(RouteOption.Option.SystemConfig.SiteFolder))
            {
                var words = Data.Uri.LocalPath.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length <= 1)
                {
                    Data.UserState = UserOperatorStateType.FormalError;
                    Data.ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                    Data.ResultMessage = ApiResult.ArgumentErrorJson;
                    return false;
                }

                Data.HostName = words[0];
                Data.ApiName = words[1];
            }
            else
            {
                //var path = Data.Uri.LocalPath.Replace(RouteOption.Option.SystemConfig.SiteFolder, "",StringComparison.OrdinalIgnoreCase);

                //LogRecorder.MonitorTrace($"{RouteOption.Option.SystemConfig.SiteFolder}:{Data.Uri.LocalPath}");
                var words = Data.Uri.LocalPath.Split('/', 3, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length <= 1)
                {
                    Data.UserState = UserOperatorStateType.FormalError;
                    Data.ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                    Data.ResultMessage = ApiResult.ArgumentErrorJson;
                    return false;
                }

                Data.HostName = words[1];
                Data.ApiName = words[2];
            }
            LogRecorder.MonitorTrace($"{Data.HostName}:{Data.ApiName}");
            return true;
        }

        /// <summary>
        ///     安全检查
        /// </summary>
        private bool TokenCheck()
        {
            
            SecurityChecker.Data = Data;
            //if (SecurityChecker.CheckToken())
            //    return true;
            //Data.ResultMessage = RouteOption.Option.Security.BlockHost;
            //Response.Redirect(RouteOption.Option.Security.BlockHost, false);
            //Data.Redirect = true;
            return SecurityChecker.CheckToken();
        }


        /// <summary>
        ///     写入返回
        /// </summary>
        void IRouter.WriteResult()
        {
            //if (Data.Redirect)
            //    return;
            //// 缓存
            //RouteCache.CacheResult(Data);
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.WriteAsync(Data.ResultMessage ?? (Data.ResultMessage = ApiResult.RemoteEmptyErrorJson), Encoding.UTF8);
        }

        /// <summary>
        ///     结束
        /// </summary>
        void IRouter.End()
        {
            IoHandler.OnEnd(Data);
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="e"></param>
        /// <param name="context"></param>
        void IRouter.OnError(Exception e, HttpContext context)
        {
            try
            {
                LogRecorder.MonitorTrace(e.Message);
                Data.UserState = UserOperatorStateType.LocalException;
                Data.ZeroState = ZeroOperatorStateType.LocalException;
                ZeroTrace.WriteException("Route", e);
                IocHelper.Create<IRuntimeWaring>()?.Waring("Route", Data.Uri.LocalPath, e.Message);
                context.Response.WriteAsync(ApiResult.LocalErrorJson, Encoding.UTF8);
            }
            catch (Exception exception)
            {
                LogRecorder.Exception(exception);
            }
        }

        #endregion
    }
}