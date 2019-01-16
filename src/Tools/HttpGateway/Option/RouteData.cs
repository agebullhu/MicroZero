using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ZeroNet.Http.Gateway
{
    /// <summary>
    ///     路由数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    public class RouteData
    {

        /// <summary>
        ///     当前请求调用的API名称
        /// </summary>
        [DataMember] [JsonProperty("apiName")] public string ApiName;

        /// <summary>
        ///     当前请求调用的API配置
        /// </summary>
        public ApiItem ApiItem;
        
        /// <summary>
        ///     Http Header中的Authorization信息
        /// </summary>
        [DataMember] [JsonProperty("token")] public string Token { get; set; }

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
        ///     请求的内容
        /// </summary>
        [DataMember] [JsonProperty("context")] public string HttpContext;

        /// <summary>
        ///     请求的表单
        /// </summary>
        [DataMember] [JsonProperty("form")] public string HttpForm;

        /*
        /// <summary>
        ///     请求的参数
        /// </summary>
        [DataMember]
        [JsonProperty("queryString")] public string QueryString;
        */

        /// <summary>
        ///     请求的表单
        /// </summary>
        [DataMember]
        [JsonProperty("headers")]
        public Dictionary<string, List<string>> Headers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     当前请求调用的主机名称
        /// </summary>
        [DataMember] [JsonProperty("host")] public string HostName;

        /// <summary>
        ///     HTTP method
        /// </summary>
        [DataMember] [JsonProperty("method")] public string HttpMethod;

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
        ///     路由主机信息
        /// </summary>
        public RouteHost RouteHost;

        /// <summary>
        ///     结果状态
        /// </summary>
        public ZeroOperatorStateType ZeroState { get; set; }

        /// <summary>
        ///     执行状态
        /// </summary>
        [DataMember] [JsonProperty("status")] public UserOperatorStateType UserState;

        /// <summary>
        ///     请求地址
        /// </summary>
        [DataMember] [JsonProperty("uri")] public Uri Uri;

        /// <summary>
        ///     开始时间
        /// </summary>
        [IgnoreDataMember]
        [JsonIgnore] public string Title { get; set; }

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
        ///     准备
        /// </summary>
        /// <param name="context"></param>
        public void Prepare(HttpContext context)
        {
            var request = context.Request;
            string userAgent = null;
            foreach (var head in request.Headers)
            {
                var key = head.Key.ToUpper();
                var vl = head.Value.ToList();
                Headers.Add(key, vl);
                switch (key)
                {
                    case "AUTHORIZATION":
                        Token = vl.LinkToString();
                        var words = Token.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
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
                }
            }
            if (string.IsNullOrWhiteSpace(Token))
            {
                Token = context.Request.Query["token"];
            }
            GlobalContext.SetRequestContext(new RequestInfo
            {
                RequestId = $"{ZeroApplication.AppName}-{context.Connection.Id}-{RandomOperate.Generate(6)}",
                UserAgent = userAgent,
                Token = Token,
                RequestType = RequestType.Http,
                ArgumentType = ArgumentType.Json,
                Ip = context.Connection.RemoteIpAddress?.ToString(),
                Port = context.Connection.RemotePort.ToString(),
            });
            Console.WriteLine(GlobalContext.Current.Request.RequestId);
        }
    }
}