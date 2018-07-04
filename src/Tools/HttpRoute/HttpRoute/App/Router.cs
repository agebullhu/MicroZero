using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
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
            return Data.RouteHost != null;
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


        /// <summary>
        ///     检查返回值是否合理
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static bool CheckResult(RouteData data)
        {
            if (data.Status != RouteStatus.None || data.HostName == null
            ) // "".Equals(data.HostName,StringComparison.OrdinalIgnoreCase))
                return false;
            if (string.IsNullOrWhiteSpace(data.ResultMessage))
            {
                IocHelper.Create<IRuntimeWaring>()?.Waring(data.HostName, data.ApiName, "返回值非法(空内容)");
                return false;
            }

            var json = data.ResultMessage.Trim();
            switch (json[0])
            {
                case '{':
                case '[':
                    break;
                default:
                    IocHelper.Create<IRuntimeWaring>()?.Waring(data.HostName, data.ApiName, "返回值非法(空内容)");
                    return true;
            }

            ApiResult result;
            try
            {
                result = JsonConvert.DeserializeObject<ApiResult>(data.ResultMessage);
                if (result == null)
                {
                    IocHelper.Create<IRuntimeWaring>()?.Waring(data.HostName, data.ApiName, "返回值非法(空内容)");
                    return false;
                }
            }
            catch
            {
                IocHelper.Create<IRuntimeWaring>()?.Waring(data.HostName, data.ApiName, "返回值非法(空内容)");
                return false;
            }

            if (result.Status == null || result.Success) return true;
            switch (result.Status.ErrorCode)
            {
                case ErrorCode.ReTry:
                case ErrorCode.DenyAccess:
                case ErrorCode.Ignore:
                case ErrorCode.LogicalError:
                case ErrorCode.Auth_RefreshToken_Unknow:
                case ErrorCode.Auth_ServiceKey_Unknow:
                case ErrorCode.Auth_AccessToken_Unknow:
                case ErrorCode.Auth_User_Unknow:
                case ErrorCode.Auth_Device_Unknow:
                case ErrorCode.Auth_AccessToken_TimeOut:
                    return false;
            }

            IocHelper.Create<IRuntimeWaring>()
                ?.Waring(data.HostName, data.ApiName, result.Status?.ClientMessage ?? "处理错误但无消息");
            return false;
        }

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
            Data.Prepare(context.Request);
            SecurityChecker = new SecurityChecker {Data = Data};
            ApiContext.Current.Request.Ip = HttpContext.Connection.RemoteIpAddress.ToString();
            ApiContext.Current.Request.Port = HttpContext.Connection.RemotePort.ToString();
            ApiContext.Current.Request.ArgumentType = ArgumentType.Json;
            ApiContext.Current.Request.UserAgent = Request.Headers["User-Agent"];
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
                Data.Status = RouteStatus.FormalError;
                Data.ResultMessage = ApiResult.NoFindJson;
                return;
            }

            // 2 安全检查
            if (!SecurityCheck())
                return;
            // 3 缓存快速处理
            if (RouteChahe.LoadCache(Data.Uri, Data.Bearer, out Data.CacheSetting, out Data.CacheKey,
                ref Data.ResultMessage))
            {
                //找到并返回缓存
                Data.Status = RouteStatus.Cache;
                return;
            }

            // 4 远程调用
            if (!Data.RouteHost.ByZero)
                Data.ResultMessage = await CallHttp();
            else if (ZeroApplication.ZerCenterIsRun)
                Data.ResultMessage = CallZero();
            else
            {
                Data.Status = RouteStatus.LocalError;
                Data.ResultMessage = ApiResult.NoReadyJson;
                return;
            }
            // 5 结果检查
            if (!CheckResult(Data))
                Data.Status = RouteStatus.RemoteError;
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
                Data.Status = RouteStatus.FormalError;
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
        private bool SecurityCheck()
        {
            string authorization = Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authorization))
            {
                Data.Bearer = Request.Query["ClientKey"];
                return true;
            }

            var words = authorization.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length != 2 || !string.Equals(words[0], "Bearer", StringComparison.OrdinalIgnoreCase) ||
                words[1].Equals("null") || words[1].Equals("undefined"))
                Data.Bearer = null;
            else
                Data.Bearer = words[1];

            ApiContext.Current.Request.Bear = Data.Bearer;
            SecurityChecker.Bearer = Data.Bearer;
            if (SecurityChecker.Check())
                return true;
            Data.Status = RouteStatus.DenyAccess;
            Data.ResultMessage = RouteOption.Option.Security.BlockHost;
            Response.Redirect(RouteOption.Option.Security.BlockHost, false);
            Data.Redirect = true;
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
            Response.WriteAsync(Data.ResultMessage ?? (Data.ResultMessage = ApiResult.RemoteEmptyErrorJson),
                Encoding.UTF8);
        }

        #endregion
    }
}