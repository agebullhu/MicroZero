using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Agebull.Common.Context;
using Agebull.Common.Logging;

using Agebull.MicroZero;
using Agebull.MicroZero.ZeroApi;
using Agebull.EntityModel.Common;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     路由数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    public class RouteData
    {
        /// <summary>
        ///     请求地址
        /// </summary>
        [DataMember] [JsonProperty("uri")] public Uri Uri { get; private set; }
        /// <summary>
        ///     当前请求调用的主机名称
        /// </summary>
        [DataMember] [JsonProperty("host")] public string HostName { get; private set; }

        /// <summary>
        ///     当前请求调用的API名称
        /// </summary>
        [DataMember] [JsonProperty("apiName")] public string ApiName { get; private set; }

        /// <summary>
        ///     Http Header中的Authorization信息
        /// </summary>
        [DataMember] [JsonProperty("token")] public string Token { get; set; }

        /// <summary>
        ///     HTTP method
        /// </summary>
        [DataMember] [JsonProperty("method")] public string HttpMethod { get; private set; }

        /// <summary>
        ///     请求的内容
        /// </summary>
        [DataMember] [JsonProperty("context")] public string HttpContext { get; private set; }

        /// <summary>
        ///     请求的表单
        /// </summary>
        [DataMember]
        [JsonProperty("headers")]
        public Dictionary<string, List<string>> Headers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     请求的表单
        /// </summary>
        [DataMember]
        [JsonProperty("arguments")]
        public Dictionary<string, string> Arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        ///     执行HTTP重写向吗
        /// </summary>
        [DataMember]
        [JsonProperty("redirect")]
        public bool Redirect;

        /// <summary>
        ///     返回值
        /// </summary>
        [DataMember] [JsonProperty("result")] public string ResultMessage;

        /// <summary>
        ///     开始时间
        /// </summary>
        [DataMember]
        [JsonProperty("start")] public DateTime Start { get; set; }

        /// <summary>
        ///     结束时间
        /// </summary>
        [DataMember]
        [JsonProperty("end")] public DateTime End { get; set; }

        /// <summary>
        ///     是否正常
        /// </summary>
        [DataMember]
        [JsonProperty("succeed")] public bool IsSucceed => UserState == UserOperatorStateType.Success;

        /// <summary>
        ///     执行状态
        /// </summary>
        [DataMember] [JsonProperty("status")] public UserOperatorStateType UserState;

        /// <summary>
        ///     当前请求调用的API配置
        /// </summary>
        public ApiItem ApiItem;

        /// <summary>
        ///     缓存键
        /// </summary>
        public string CacheKey;

        /// <summary>
        ///     当前适用的缓存设置对象
        /// </summary>
        public CacheOption CacheSetting;

        /// <summary>
        ///     上下文的JSON内容(透传)
        /// </summary>
        public string GlobalContextJson;

        /// <summary>
        ///     路由主机信息
        /// </summary>
        public RouteHost RouteHost;

        /// <summary>
        ///     结果状态
        /// </summary>
        public ZeroOperatorStateType ZeroState { get; set; }


        /// <summary>
        ///     准备
        /// </summary>
        /// <param name="context"></param>
        public bool Prepare(HttpContext context)
        {
            var request = context.Request;
            Uri = request.GetUri();
            HttpMethod = request.Method.ToUpper();

            var userAgent = CheckHeaders(context, request);
            var ok = CheckCall();
            GlobalContext.SetRequestContext(new RequestInfo
            {
                RequestId = $"{Token}-{RandomOperate.Generate(6)}",
                UserAgent = userAgent,
                Token = Token,
                RequestType = RequestType.Http,
                ArgumentType = ArgumentType.Json,
                Ip = context.Connection.RemoteIpAddress?.ToString(),
                Port = context.Connection.RemotePort.ToString(),
            });
            return ok && Read(context);
        }

        private string CheckHeaders(HttpContext context, HttpRequest request)
        {
            string userAgent = null;
            foreach (var head in request.Headers)
            {
                var key = head.Key.ToUpper();
                switch (key)
                {
                    case "AUTHORIZATION":
                        var token = head.Value.LinkToString();
                        var words = token.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
                        if (words.Length != 2 ||
                            !string.Equals(words[0], "Bearer", StringComparison.OrdinalIgnoreCase) ||
                            words[1].Equals("null") ||
                            words[1].Equals("undefined"))
                            Token = null;
                        else
                            Token = words[1];
                        break;
                    case "USER-AGENT":
                        userAgent = head.Value.LinkToString("|");
                        break;
                    default:
                        Headers.Add(key, head.Value.ToList());
                        break;
                }
            }
            if (string.IsNullOrWhiteSpace(Token))
            {
                Token = context.Request.Query["token"];
            }
            return userAgent;
        }

        /// <summary>
        ///     检查调用内容
        /// </summary>
        /// <returns></returns>
        private bool CheckCall()
        {
            if (RouteOption.Option.SystemConfig.IsAppFolder)
            {
                var words = Uri.LocalPath.Split('/', 3, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length <= 1)
                {
                    UserState = UserOperatorStateType.FormalError;
                    ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                    ResultMessage = ApiResultIoc.ArgumentErrorJson;
                    return false;
                }

                HostName = words[1];
                ApiName = words[2];
            }
            else
            {
                var words = Uri.LocalPath.Split('/', 2, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length <= 1)
                {
                    UserState = UserOperatorStateType.FormalError;
                    ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                    ResultMessage = ApiResultIoc.ArgumentErrorJson;
                    return false;
                }

                HostName = words[0];
                ApiName = words[1];
            }
            return true;
        }

        private bool Read(HttpContext context)
        {
            var request = context.Request;
            try
            {
                if (request.QueryString.HasValue)
                {
                    foreach (var key in request.Query.Keys)
                        Arguments.TryAdd(key, request.Query[key]);
                }
                if (request.HasFormContentType)
                {
                    foreach (var key in request.Form.Keys)
                        Arguments.TryAdd(key, request.Form[key]);
                }
                if (request.ContentLength == null)
                    return true;
                using (var texter = new StreamReader(request.Body))
                {
                    HttpContext = texter.ReadToEnd();
                    if (string.IsNullOrEmpty(HttpContext))
                        HttpContext = null;
                    texter.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "读取远程参数");
                UserState = UserOperatorStateType.FormalError;
                ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                ResultMessage = ApiResultIoc.ArgumentErrorJson;
                return false;
            }
        }
    }
}