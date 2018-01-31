using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Agebull.Common.Base;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 路由数据
    /// </summary>
    internal class RouteData
    {
        /// <summary>
        /// 请求地址
        /// </summary>
        public Uri Uri;
        /// <summary>
        ///     当前适用的缓存设置对象
        /// </summary>
        public CacheSetting CacheSetting;
        /// <summary>
        ///     缓存键
        /// </summary>
        public string CacheKey;
        /// <summary>
        /// 执行状态
        /// </summary>
        public RouteStatus Status;
        /// <summary>
        ///     返回值
        /// </summary>
        public string ResultMessage;
        
        /// <summary>
        ///     Http Header中的Authorization信息
        /// </summary>
        public string Bearer;

        /// <summary>
        ///     当前请求调用的主机名称
        /// </summary>
        public string HostName;

        /// <summary>
        ///     当前请求调用的API名称
        /// </summary>
        public string ApiName;

        /// <summary>
        ///     HTTP method
        /// </summary>
        public string HttpMethod;
        /// <summary>
        ///     路由主机信息
        /// </summary>
        public HostConfig RouteHost;


    }
    /// <inheritdoc />
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
        private HttpRouter(HttpContext context)
        {
            Data=new RouteData();
            HttpContext = context;
            Request = context.Request;
            Response = context.Response;
            HttpIoLog.OnBegin(Request);
        }

        /// <inheritdoc />
        /// <summary>清理资源</summary>
        protected override void OnDispose()
        {
            HttpIoLog.OnEnd(Data.Status, Data.ResultMessage);
        }

        /// <summary>
        ///     执行路由操作
        /// </summary>
        public static void Todo(HttpContext context)
        {
            using (var router = new HttpRouter(context))
            {
                // 正常调用
                router.Call();
                // 写入返回
                router.WriteResult();
            }
        }

        /// <summary>
        /// 调用
        /// </summary>
        private void Call()
        {
            Data.Uri = Request.GetUri();
            Data.HttpMethod = Request.Method.ToUpper();

            // 1 初始化基本信息
            if (!InitializeContext())
                return;
            // 2 缓存快速处理
            if (RouteRuntime.CheckCache(Data.Uri, Data.Bearer, out Data.CacheSetting, out Data.CacheKey, ref Data.ResultMessage))
            {
                //找到并返回缓存
                Data.Status = RouteStatus.Cache;
                return;
            }

            //3 安全检查
            if (!SecurityCheck()) return;
            //3 初始化路由信息
            if (!InitializeRoute())
                return;
            //4 查找远程机器
            Data.ResultMessage = Data.RouteHost.ByZero ? CallZero() : CallHttp();
            //5 远程调用
            //缓存
            RouteRuntime.CacheData(Data);
        }

        /// <summary>
        ///     初始化基本上下文
        /// </summary>
        private bool InitializeContext()
        {
            string authorization = Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authorization))
            {
                Data.Bearer = Request.Query["ClientKey"];
            }
            else
            {
                var aa = authorization.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (aa.Length != 2 || !string.Equals(aa[0], "Bearer", StringComparison.OrdinalIgnoreCase) ||
                    aa[1].Equals("null"))
                    Data.Bearer = null;
                else
                    Data.Bearer = aa[1];
            }
            ApiContext.Current.Request.Bear = Data.Bearer;
            ApiContext.Current.Request.RequestId = RandomOperate.Generate(8);
            ApiContext.Current.Request.Ip = HttpContext.Connection.RemoteIpAddress.ToString();
            ApiContext.Current.Request.Port = HttpContext.Connection.RemotePort.ToString();
            ApiContext.Current.Request.ServiceKey = ApiContext.MyServiceKey;
            ApiContext.Current.Request.ArgumentType = ArgumentType.Json;
            ApiContext.Current.Request.UserAgent = Request.Headers["User-Agent"];

            return true;
        }

        /// <summary>
        ///     安全检查
        /// </summary>
        private bool SecurityCheck()
        {
            var checker = new SecurityChecker
            {
                Request = Request,
                Bearer = Data.Bearer
            };
            if (checker.Check())
                return true;
            Data.Status = RouteStatus.DenyAccess;
            Data.ResultMessage = AppConfig.Config.SystemConfig.BlockHost;
            Response.Redirect(AppConfig.Config.SystemConfig.BlockHost, false);
            return false;
        }


        /// <summary>
        ///     写入返回
        /// </summary>
        private void WriteResult()
        {
            if (Data.Status >= RouteStatus.HttpRedirect)
                return;
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.ContentType = "text/plain; charset=utf-8";
            if (Data.ResultMessage != null)
                Response.WriteAsync(Data.ResultMessage, Encoding.UTF8);
        }

        #endregion

        #region 路由

        /// <summary>
        ///     初始化路由信息
        /// </summary>
        private bool InitializeRoute()
        {
            var words = Data.Uri.LocalPath.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
            Data.HostName = words[0];
            if (words.Length <= 1)
            {
                Data.Status = RouteStatus.FormalError;
                Data.ResultMessage = RouteRuntime.DenyAccess;
                return false;
            }

            Data.ApiName = words[1];
            if (!AppConfig.Config.RouteMap.TryGetValue(Data.HostName, out Data.RouteHost))
                Data.RouteHost = HostConfig.DefaultHost;
            return true;
            //StringBuilder sb = new StringBuilder();
            //int step = 0;
            //foreach (var ch in Data.Uri.PathAndQuery)
            //{
            //    switch (ch)
            //    {
            //        case '/':
            //            if (step == 0)
            //            {
            //                sb.Append(ch);
            //            }
            //            else if (sb.Length > 0)
            //            {
            //                isFirst = false;
            //                _model = sb.ToString();
            //                sb.Clear();
            //            }
            //            break;
            //        case '?':
            //            _api = sb.ToString();
            //            break;
            //        default:
            //            sb.Append(ch);
            //            break;
            //    }
            //}
            //if (_model == null)
            //{
            //    status != 0 = true;
            //    Data.ResultMessage = DenyAccess;
            //}
            //if (sb.Length > 0)
            //    _apiAndQuery = sb.ToString();
            //else
            //{
            //    status != 0 = true;
            //    Data.ResultMessage = DenyAccess;
            //}
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
            string httpApi = Data.RouteHost == HostConfig.DefaultHost ? Data.Uri.PathAndQuery : $"{Data.ApiName}{Data.Uri.Query}";

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
            var req = caller.CreateRequest(httpApi, Data.HttpMethod, Request);

            LogRecorder.BeginStepMonitor("内部HTTP调用");
            LogRecorder.MonitorTrace($"Url:{req.RequestUri.PathAndQuery}");
            LogRecorder.MonitorTrace($"Auth:{caller.Bearer}");

            try
            {
                // 远程调用状态
                Data.ResultMessage = caller.GetResult(req, out var _webStatus);
                LogRecorder.MonitorTrace(_webStatus.ToString());
                if (_webStatus != WebExceptionStatus.Success)
                    Data.Status = RouteStatus.RemoteError;
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                LogRecorder.MonitorTrace($"发生异常：{ex.Message}");
                Data.ResultMessage = RouteRuntime.NetworkError;
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
                Data.ResultMessage = StationProgram.Call(Data.HostName, Data.ApiName, JsonConvert.SerializeObject(values));
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                LogRecorder.MonitorTrace($"发生异常：{ex.Message}");
                Data.ResultMessage = RouteRuntime.NetworkError;
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