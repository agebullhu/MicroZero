using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Base;
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
    internal class HttpRouter
    {

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
        /// 安全检查器
        /// </summary>
        public SecurityChecker SecurityChecker { get; }

        #endregion

        #region 流程

        /// <summary>
        ///     内部构架
        /// </summary>
        /// <param name="context"></param>
        internal HttpRouter(HttpContext context)
        {
            HttpContext = context;
            Request = context.Request;
            Response = context.Response;
            Data = new RouteData();
            Data.Prepare(context.Request);
            SecurityChecker = new SecurityChecker { Data = Data };
            ApiContext.Current.Request.Ip = HttpContext.Connection.RemoteIpAddress.ToString();
            ApiContext.Current.Request.Port = HttpContext.Connection.RemotePort.ToString();
            ApiContext.Current.Request.ArgumentType = ArgumentType.Json;
            ApiContext.Current.Request.UserAgent = Request.Headers["User-Agent"];
        }

        /// <summary>
        /// 调用
        /// </summary>
        internal void Call()
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
            if (RouteChahe.LoadCache(Data.Uri, Data.Bearer, out Data.CacheSetting, out Data.CacheKey, ref Data.ResultMessage))
            {
                //找到并返回缓存
                Data.Status = RouteStatus.Cache;
                return;
            }
            // 4 远程调用
            if (Data.RouteHost.ByZero)
            {
                if (!ZeroApplication.IsRun)
                {
                    Data.ResultMessage = ApiResult.NoReadyJson;
                }
                else
                {
                    Data.ResultMessage = CallZero().Result;
                }
            }
            else
                Data.ResultMessage = CallHttp().Result;
            // 5 结果检查
            //if (!CheckResult(Data))
            //    Data.Status = RouteStatus.RemoteError;
        }

        /// <summary>
        /// 检查调用内容
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
            if (String.IsNullOrWhiteSpace(authorization))
            {
                Data.Bearer = Request.Query["ClientKey"];
                return true;
            }

            var words = authorization.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length != 2 || !String.Equals(words[0], "Bearer", StringComparison.OrdinalIgnoreCase) || words[1].Equals("null") || words[1].Equals("undefined"))
                Data.Bearer = null;
            else
                Data.Bearer = words[1];

            ApiContext.Current.Request.Bear = Data.Bearer;
            SecurityChecker.Bearer = Data.Bearer;
            if (SecurityChecker.Check())
                return true;
            Data.Status = RouteStatus.DenyAccess;
            Data.ResultMessage = AppConfig.Config.Security.BlockHost;
            Response.Redirect(AppConfig.Config.Security.BlockHost, false);
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
            Response.WriteAsync(Data.ResultMessage ?? (Data.ResultMessage = ApiResult.RemoteEmptyErrorJson), Encoding.UTF8);
        }

        #endregion

        #region 检查返回值是否合理

        /// <summary>
        /// 检查返回值是否合理
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static bool CheckResult(RouteData data)
        {
            if (data.Status != RouteStatus.None || data.HostName == null)// "".Equals(data.HostName,StringComparison.OrdinalIgnoreCase))
                return false;
            if (string.IsNullOrWhiteSpace(data.ResultMessage))
            {
                RuntimeWaring.Waring(data.HostName, data.ApiName, "返回值非法(空内容)");
                return false;
            }

            var json = data.ResultMessage.Trim();
            switch (json[0])
            {
                case '{':
                case '[':
                    break;
                default:
                    RuntimeWaring.Waring(data.HostName, data.ApiName, "返回值非法(空内容)");
                    return true;
            }
            ApiResult result;
            try
            {
                result = JsonConvert.DeserializeObject<ApiResult>(data.ResultMessage);
                if (result == null)
                {
                    RuntimeWaring.Waring(data.HostName, data.ApiName, "返回值非法(空内容)");
                    return false;
                }
            }
            catch
            {
                RuntimeWaring.Waring(data.HostName, data.ApiName, "返回值非法(空内容)");
                return false;
            }
            if (result.Status == null || result.Success)
            {
                return true;
            }
            switch (result.Status.ErrorCode)
            {
                case ErrorCode.ReTry:
                case ErrorCode.DenyAccess:
                case ErrorCode.Ignore:
                case ErrorCode.ArgumentError:
                case ErrorCode.Auth_RefreshToken_Unknow:
                case ErrorCode.Auth_ServiceKey_Unknow:
                case ErrorCode.Auth_AccessToken_Unknow:
                case ErrorCode.Auth_User_Unknow:
                case ErrorCode.Auth_Device_Unknow:
                case ErrorCode.Auth_AccessToken_TimeOut:
                    return false;
            }

            RuntimeWaring.Waring(data.HostName, data.ApiName, result.Status?.ClientMessage ?? "处理错误但无消息");
            return false;
        }

        #endregion

        #region 路由

        /// <summary>
        ///     初始化基本上下文
        /// </summary>
        private bool FindHost()
        {
            if (!AppConfig.Config.RouteMap.TryGetValue(Data.HostName, out Data.RouteHost))
                Data.RouteHost = HostConfig.DefaultHost;
            return Data.RouteHost != null;
        }

        #endregion

        #region Http

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        private async Task<string> CallHttp()
        {
            // 当前请求调用的模型对应的主机名称
            string httpHost;

            // 当前请求调用的Api名称
            var httpApi = Data.RouteHost == HostConfig.DefaultHost ? Data.Uri.PathAndQuery : $"{Data.ApiName}{Data.Uri.Query}";

            // 查找主机
            if (Data.RouteHost.Hosts.Length == 1)
            {
                httpHost = Data.RouteHost.Hosts[0];
            }
            else lock (Data.RouteHost)
                {
                    //平均分配
                    httpHost = Data.RouteHost.Hosts[Data.RouteHost.Next];
                    if (++Data.RouteHost.Next >= Data.RouteHost.Hosts.Length)
                        Data.RouteHost.Next = 0;
                }
            // 远程调用
            var caller = new HttpApiCaller(httpHost)
            {
                Bearer = $"Bearer {ApiContext.RequestContext.Bear}"
            };
            caller.CreateRequest(httpApi, Data.HttpMethod, Request, Data);

            LogRecorder.BeginStepMonitor("内部HTTP调用");

            try
            {
                // 远程调用状态
                Data.ResultMessage = await caller.GetResult();
                LogRecorder.MonitorTrace(caller.Status.ToString());
                if (caller.Status != WebExceptionStatus.Success)
                    Data.Status = RouteStatus.RemoteError;
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                LogRecorder.MonitorTrace($"发生异常：{ex.Message}");
                Data.ResultMessage = ApiResult.NetworkErrorJson;
                Data.Status = RouteStatus.RemoteError;
            }
            finally
            {
                LogRecorder.MonitorTrace(Data.ResultMessage);
                LogRecorder.EndStepMonitor();
            }
            return Data.ResultMessage;
        }


        #endregion

        #region Zero

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        private async Task<string> CallZero()
        {
            string context;
            //参数解析
            if (Request.HasFormContentType)
            {
                var values = new Dictionary<string, string>();
                foreach (var form in Request.Form.Keys)
                    values.TryAdd(form, Request.Form[form]);
                context = JsonConvert.SerializeObject(values);
            }
            else if (Request.ContentLength > 0)
            {
                using (var texter = new StreamReader(Request.Body))
                {
                    context = texter.ReadToEnd();
                }
            }
            else
            {
                var values = new Dictionary<string, string>();
                foreach (var query in Request.Query.Keys)
                    values.TryAdd(query, Request.Query[query]);
                context = JsonConvert.SerializeObject(values);
            }

            Data.ResultMessage = await ApiClient.CallSync(Data.HostName, Data.ApiName, context);
            if (ApiContext.Current.LastError != ErrorCode.Success)
                Data.Status = RouteStatus.RemoteError;
            return Data.ResultMessage;
        }

        #endregion
    }
}