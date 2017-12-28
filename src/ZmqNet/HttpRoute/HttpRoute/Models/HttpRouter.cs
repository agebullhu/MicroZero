using System;
using System.Net;
using System.Text;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Yizuan.Service.Host;
using Agebull.Common;
using Yizuan.Service.Api;
using Agebull.Common.Logging;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv.Internal;

namespace ExternalStation.Models
{
    /// <summary>
    /// 调用映射核心类
    /// </summary>
    public class HttpRouter
    {
        #region 变量

        /// <summary>
        /// Http上下文
        /// </summary>
        public HttpContext HttpContext { get; }
        /// <summary>
        /// Http请求
        /// </summary>
        public HttpRequest Request { get; }
        /// <summary>
        /// Http返回
        /// </summary>
        public HttpResponse Response { get; }


        /// <summary>
        /// Http Header中的Authorization信息
        /// </summary>
        string _bear;

        /// <summary>
        /// 当前路径
        /// </summary>
        private Uri _callUri;

        /// <summary>
        /// 当前请求调用的模型对应的主机名称
        /// </summary>
        private string _host;

        /// <summary>
        /// 当前请求调用的模型名称
        /// </summary>
        private string _model;

        /// <summary>
        /// HTTP method
        /// </summary>
        private string _httpMethod;

        /// <summary>
        /// 当前请求调用的Api名称
        /// </summary>
        private string _apiAndQuery;

        /// <summary>
        /// 已失败
        /// </summary>
        private bool _isFailed;

        /// <summary>
        /// 返回值
        /// </summary>
        private string _resultMessage;


        #endregion

        #region 默认返回内容

        /// <summary>
        /// 拒绝访问的Json字符串
        /// </summary>
        private static readonly string DenyAccess = JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.DenyAccess));

        /// <summary>
        /// 拒绝访问的Json字符串
        /// </summary>
        private static readonly string DeviceUnknow = JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.Auth_Device_Unknow));

        /// <summary>
        /// 拒绝访问的Json字符串
        /// </summary>
        private static readonly string TokenUnknow = JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.Auth_AccessToken_Unknow));

        /// <summary>
        /// 操作成功的字符串
        /// </summary>
        private static readonly string DefaultSuccees = JsonConvert.SerializeObject(ApiResult.Succees());


        #endregion


        /// <summary>
        /// 内部构架
        /// </summary>
        /// <param name="context"></param>
        private HttpRouter(HttpContext context)
        {
            HttpContext = context;
            Request = context.Request;
            Response = context.Response;
        }

        /// <summary>
        /// 执行路由操作
        /// </summary>
        public static void Todo(HttpContext context)
        {
            HttpRouter router = new HttpRouter(context);
            router.DoRoute();
        }
        /// <summary>
        /// 执行路由操作
        /// </summary>
        private void DoRoute()
        {
            _httpMethod = Request.Method.ToUpper();
            //跨域支持
            if (_httpMethod == "OPTIONS")
            {
                Cros();
                return;
            }
            //以下为正确流程
            HttpIoLog.OnBegin(Request);
            Call();
            WriteResult();
            HttpIoLog.OnEnd(_resultMessage ?? DenyAccess);
        }

        /// <summary>
        /// 写入返回
        /// </summary>
        void WriteResult()
        {
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.ContentType = "text/plain";
            Response.WriteAsync(_resultMessage ?? DenyAccess, Encoding.UTF8);
            //task.Wait();
        }

        void Call()
        {
            // 1 初始化基本信息
            InitializeBaseContext();
            if (_isFailed)
                return;
            // 2 缓存快速处理
            if (CheckCache())
                return;
            //3 安全检查
            SecurityCheck();
            if (_isFailed)
                return;
            //3 初始化路由信息
            InitializeRoute();
            if (_isFailed)
                return;
            //4 查找远程机器
            FindHost();
            if (_isFailed)
                return;
            //5 远程调用
            _resultMessage = CallRemote();
            //缓存
            CacheData();
        }

        /// <summary>
        /// 初始化基本上下文
        /// </summary>
        void InitializeBaseContext()
        {
            _callUri = Request.GetUri();

            string authorization = Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authorization) ||
                string.Equals(authorization, "Bear null", StringComparison.OrdinalIgnoreCase))
            {
                _bear = null;
            }
            else
            {
                var aa = authorization.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (aa.Length != 2 || !string.Equals(aa[0], "Bear", StringComparison.OrdinalIgnoreCase))
                    _bear = null;
                else
                    _bear = aa[1];
            }
        }
        /// <summary>
        /// 安全检查
        /// </summary>
        void SecurityCheck()
        {
            SecurityChecker checker = new SecurityChecker
            {
                Request = Request,
                Bear = _bear
            };
            if (checker.Check())
                return;
            _isFailed = true;
            switch (checker.Status)
            {
                case ErrorCode.Auth_Device_Unknow:
                    _resultMessage = DeviceUnknow;
                    break;
                case ErrorCode.Auth_AccessToken_Unknow:
                    _resultMessage = TokenUnknow;
                    break;
            }
        }

        /// <summary>
        /// 初始化路由信息
        /// </summary>
        private void InitializeRoute()
        {
            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            foreach (var ch in _callUri.PathAndQuery)
            {
                switch (ch)
                {
                    case '/':
                        if (!isFirst)
                        {
                            sb.Append(ch);
                        }
                        else if (sb.Length > 0)
                        {
                            isFirst = false;
                            _model = sb.ToString();
                            sb.Clear();
                        }
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            if (_model == null)
            {
                _isFailed = true;
                _resultMessage = DenyAccess;
            }
            if (sb.Length > 0)
                _apiAndQuery = sb.ToString();
            else
            {
                _isFailed = true;
                _resultMessage = DenyAccess;
            }
        }

        /// <summary>
        /// 查找主机
        /// </summary>
        private void FindHost()
        {
            RouteHost hostData;
            if (!RouteData.HostMap.TryGetValue(_model, out hostData) || hostData.Hosts.Length == 0)
            {
                lock (RouteData.DefaultHostData)//平均分配
                {
                    _host = RouteData.DefaultHostData.Hosts[RouteData.DefaultHostData.Next] + _model;
                    if (++RouteData.DefaultHostData.Next >= RouteData.DefaultHostData.Hosts.Length)
                    {
                        RouteData.DefaultHostData.Next = 0;
                    }
                }
            }
            else if (hostData.Hosts.Length == 1)
            {
                _host = hostData.Hosts[0];
            }
            else
            {
                lock (hostData)//平均分配
                {
                    _host = hostData.Hosts[hostData.Next];
                    if (++hostData.Next >= hostData.Hosts.Length)
                    {
                        hostData.Next = 0;
                    }
                }
            }
        }


        #region 缓存

        /// <summary>
        /// 当前适用的缓存设置对象
        /// </summary>
        CacheSetting _cacheSetting;

        /// <summary>
        /// 缓存键
        /// </summary>
        string _cacheKey;

        CacheData cacheData;

        /// <summary>
        /// 检查缓存
        /// </summary>
        /// <returns>取到缓存，可以直接返回</returns>
        private bool CheckCache()
        {
            if (!RouteData.CacheMap.TryGetValue(_callUri.LocalPath, out _cacheSetting))
                return false;

            if (_cacheSetting.Feature.HasFlag(CacheFeature.Bear) &&
                _bear.Substring(0, _cacheSetting.Bear.Length) != _cacheSetting.Bear)
            {
                _cacheSetting = null;
                return false;
            }

            _cacheKey = _cacheSetting.OnlyName ? _callUri.LocalPath : _callUri.PathAndQuery;
            lock (_cacheSetting)
            {
                if (!RouteData.Cache.TryGetValue(_cacheKey, out cacheData))
                    return false;
            }
            if (cacheData.UpdateTime <= DateTime.Now)
                return false;
            _resultMessage = cacheData.Content;
            return true;
        }
        /// <summary>
        /// 缓存数据
        /// </summary>
        private void CacheData()
        {
            if (_cacheSetting == null)
                return;
            if (cacheData == null)
                cacheData = new CacheData
                {
                    Content = _resultMessage
                };
            if (_cacheSetting.Feature.HasFlag(CacheFeature.NetError) && WebStatus != WebExceptionStatus.Success)
            {
                cacheData.UpdateTime = DateTime.Now.AddSeconds(30);
            }
            else
            {
                cacheData.UpdateTime = DateTime.Now.AddSeconds(_cacheSetting.FlushSecond);
            }

            lock (RouteData.Cache)
            {
                if (!RouteData.Cache.ContainsKey(_cacheKey))
                    RouteData.Cache.Add(_cacheKey, cacheData);
                else
                    RouteData.Cache[_cacheKey] = cacheData;
            }
        }

        #endregion
        #region 远程调用
        /// <summary>
        /// 远程调用状态
        /// </summary>
        WebExceptionStatus WebStatus;
        /// <summary>
        /// 远程调用
        /// </summary>
        /// <returns></returns>
        private string CallRemote()
        {
            var callContext = new InternalCallContext
            {
                ServiceKey = RouteData.ServiceKey,
                RequestId = Guid.NewGuid(),
                Bear = _bear
            };
            ApiContext.SetRequestContext(callContext);
            ApiContext.Current.Cache();
            var caller = new ApiCaller(_host)
            {
                Bearer = "Bear " + ApiContext.RequestContext.Bear
            };
            var req = caller.CreateRequest(_apiAndQuery, _httpMethod, _httpMethod == "POST" && Request.ContentLength > 0 ? Request.Form : null);

            return caller.GetResult(req, out WebStatus);
        }
        #endregion
        #region 跨域支持

        /// <summary>
        /// 跨域支持
        /// </summary>
        void Cros()
        {
            Response.Headers.Add("Access-Control-Allow-Methods", new[] { "GET", "POST" });
            Response.Headers.Add("Access-Control-Allow-Headers", new[] { "x-requested-with", "content-type", "authorization", "*" });
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        #endregion
    }
}