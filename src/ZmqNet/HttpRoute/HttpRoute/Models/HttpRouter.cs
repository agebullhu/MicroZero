using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Agebull.Common.Base;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    ///     调用映射核心类
    /// </summary>
    internal class HttpRouter : ScopeBase
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

            ApiContext.Current.Request.RequestId = RandomOperate.Generate(8);
            ApiContext.Current.Request.Ip = HttpContext.Connection.RemoteIpAddress.ToString();
            ApiContext.Current.Request.Port = HttpContext.Connection.RemotePort.ToString();
            ApiContext.Current.Request.ServiceKey = ApiContext.MyServiceKey;
            ApiContext.Current.Request.ArgumentType = ArgumentType.Json;
            ApiContext.Current.Request.UserAgent = Request.Headers["User-Agent"];
        }


        /// <summary>清理资源</summary>
        protected override void OnDispose()
        {
        }

        /// <summary>
        /// 调用
        /// </summary>
        internal void Call()
        {
            if (!CheckCall())
                return;
            // 1 安全检查
            if (!SecurityCheck())
                return;
            // 2 初始化路由信息
            if (!InitializeContext())
                return;
            // 3 缓存快速处理
            if (RouteChahe.LoadCache(Data.Uri, Data.Bearer, out Data.CacheSetting, out Data.CacheKey, ref Data.ResultMessage))
            {
                //找到并返回缓存
                Data.Status = RouteStatus.Cache;
                return;
            }
            // 4 远程调用
            if (Data.RouteHost.Failed)
            {
                Data.IsSucceed = false;
                Data.ResultMessage = RouteRuntime.NoFindJson;
                return;
            }
            Data.ResultMessage = Data.RouteHost.ByZero ? CallZero() : CallHttp();
            // 5 结果检查
            Data.IsSucceed = CheckResult(Data);
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
                Data.ResultMessage = RouteRuntime.DenyAccessJson;
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

            var checker = new SecurityChecker
            {
                Request = Request,
                Bearer = Data.Bearer
            };
            if (checker.Check())
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
            Response.WriteAsync(Data.ResultMessage ?? RouteRuntime.RemoteEmptyErrorJson, Encoding.UTF8);
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
            try
            {
                var result = JsonConvert.DeserializeObject<ApiResult>(data.ResultMessage);
                if (result == null)
                {
                    RuntimeWaring.Waring(data.HostName, data.ApiName, "返回值非法(空内容)");
                    return false;
                }
                if (result.Status != null && !result.Result)
                {
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
                        default:
                            RuntimeWaring.Waring(data.HostName, data.ApiName, result.Status?.Message ?? "处理错误但无消息");
                            return false;
                    }
                }
            }
            catch
            {
                RuntimeWaring.Waring(data.HostName, data.ApiName, "返回值非法(不是Json格式)");
                return false;
            }
            return true;
        }

        #endregion
        #region 路由

        /// <summary>
        ///     初始化基本上下文
        /// </summary>
        private bool InitializeContext()
        {

            ApiContext.Current.Request.Bear = Data.Bearer;

            if (!AppConfig.Config.RouteMap.TryGetValue(Data.HostName, out Data.RouteHost))
                Data.RouteHost = HostConfig.DefaultHost;
            return true;
        }

        #endregion

        #region Http

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        private string CallHttp()
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
            var req = caller.CreateRequest(httpApi, Data.HttpMethod, Request, Data);

            LogRecorder.BeginStepMonitor("内部HTTP调用");
            LogRecorder.MonitorTrace($"Url:{req.RequestUri.PathAndQuery}");
            LogRecorder.MonitorTrace($"Auth:{caller.Bearer}");

            try
            {
                // 远程调用状态
                Data.ResultMessage = caller.GetResult(req, out var webStatus);
                LogRecorder.MonitorTrace(webStatus.ToString());
                if (webStatus != WebExceptionStatus.Success)
                    Data.Status = RouteStatus.RemoteError;
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                LogRecorder.MonitorTrace($"发生异常：{ex.Message}");
                Data.ResultMessage = RouteRuntime.NetworkErrorJson;
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
        private string CallZero()
        {
            var values = new Dictionary<string, string>();
            //参数解析
            foreach (var query in Request.Query.Keys)
                if (!values.ContainsKey(query))
                    values.Add(query, Request.Query[query]);

            if (Data.HttpMethod == "POST")
                if (Request.ContentLength > 0)
                    foreach (var form in Request.Form.Keys)
                        if (!values.ContainsKey(form))
                            values.Add(form, Request.Form[form]);

            LogRecorder.BeginStepMonitor("内部Zero调用");
            LogRecorder.MonitorTrace($"Station:{Data.HostName}");
            LogRecorder.MonitorTrace($"Command:{Data.ApiName}");

            // 远程调用状态
            try
            {
                Data.ResultMessage = ApiClient.Call(Data.HostName, Data.ApiName, JsonConvert.SerializeObject(values));
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                LogRecorder.MonitorTrace($"发生异常：{ex.Message}");
                Data.ResultMessage = RouteRuntime.NetworkErrorJson;
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
    }
}