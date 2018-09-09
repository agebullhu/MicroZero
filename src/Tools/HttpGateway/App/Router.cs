using System;
using System.Collections.Generic;
using System.Text;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Microsoft.AspNetCore.Http;

namespace ZeroNet.Http.Gateway
{
    /// <summary>
    ///     调用映射核心类
    /// </summary>
    internal partial class Router
    {
        #region 路由

        /// <summary>
        ///     查找站点
        /// </summary>
        private bool FindHost()
        {
            if (!RouteMap.TryGetValue(Data.HostName, out Data.RouteHost))
                Data.RouteHost = HttpHost.DefaultHost;
            if (Data.RouteHost == null)
                return false;
            if (!(Data.RouteHost is ZeroHost host))
                return true;
            if (!host.Apis.TryGetValue(Data.ApiName, out Data.ApiItem))
                return false;
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
        public HttpContext HttpContext { get; }

        /// <summary>
        ///     Http请求
        /// </summary>
        public HttpRequest Request { get; }

        /// <summary>
        ///     Http返回
        /// </summary>
        public HttpResponse Response { get; }

        /// <summary>
        ///     Http返回
        /// </summary>
        public RouteData Data { get; }

        /// <summary>
        ///     安全检查器
        /// </summary>
        public SecurityChecker SecurityChecker { get; }

        #endregion

        #region 流程

        /// <summary>
        ///     内部构架
        /// </summary>
        /// <param name="context"></param>
        internal Router(HttpContext context)
        {
            HttpContext = context;
            Request = context.Request;
            Response = context.Response;
            Data = new RouteData();
            Data.Prepare(context);
            SecurityChecker = new SecurityChecker { Data = Data };
        }

        /// <summary>
        ///     调用
        /// </summary>
        internal async void Call()
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
                Data.Status = ZeroOperatorStatus.FormalError;
                Data.ResultMessage = ApiResult.NoFindJson;
                return;
            }

            // 2 安全检查
            if (!TokenCheck())
            {
                Data.Status = ZeroOperatorStatus.DenyAccess;
                return;
            }
            // 3 缓存快速处理
            if (RouteChahe.LoadCache(Data.Uri, Data.Token, out Data.CacheSetting, out Data.CacheKey, ref Data.ResultMessage))
            {
                //找到并返回缓存
                Data.Status = ZeroOperatorStatus.Success;
                return;
            }

            // 4 远程调用
            if (!Data.RouteHost.ByZero)
                Data.ResultMessage = await CallHttp();
            else if (ZeroApplication.ZerCenterIsRun)
                Data.ResultMessage = CallZero();
            else
            {
                Data.Status = ZeroOperatorStatus.LocalError;
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
            var words = Data.Uri.LocalPath.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1)
            {
                Data.Status = ZeroOperatorStatus.FormalError;
                Data.ResultMessage = ApiResult.DenyAccessJson;
                return false;
            }

            Data.HostName = words[0];
            Data.ApiName = words[1];
            return true;
        }

        /// <summary>
        ///     安全检查
        /// </summary>
        private bool TokenCheck()
        {
            if (string.IsNullOrWhiteSpace(Data.Token))
            {
                Data.Token = Request.Query["token"];
            }
            else
            {
                var words = Data.Token.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length != 2 ||
                    !string.Equals(words[0], "Bearer", StringComparison.OrdinalIgnoreCase) ||
                    words[1].Equals("null") ||
                    words[1].Equals("undefined"))
                    Data.Token = null;
                else
                    Data.Token = words[1];
            }
            GlobalContext.RequestInfo.Token = Data.Token;
            SecurityChecker.Data = Data;
            if (SecurityChecker.CheckToken())
                return true;
            //Data.ResultMessage = RouteOption.Option.Security.BlockHost;
            //Response.Redirect(RouteOption.Option.Security.BlockHost, false);
            //Data.Redirect = true;
            return false;
        }


        /// <summary>
        ///     写入返回
        /// </summary>
        internal void WriteResult()
        {
            if (Data.Redirect)
                return;
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.WriteAsync(Data.ResultMessage ?? (Data.ResultMessage = ApiResult.RemoteEmptyErrorJson), Encoding.UTF8);
        }

        #endregion
    }
}