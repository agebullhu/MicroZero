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
        ///     Http Header中的Authorization信息
        /// </summary>
        [DataMember] [JsonProperty("token")] public string Token { get; set; }

        /// <summary>
        ///     缓存键
        /// </summary>
        public string CacheKey;

        /// <summary>
        ///     请求的内容
        /// </summary>
        [DataMember] [JsonProperty("context")] public string Context;

        /// <summary>
        ///     请求的表单
        /// </summary>
        [DataMember] [JsonProperty("form")] public string Form;

        /// <summary>
        /// 参数
        /// </summary>
        public Dictionary<string, string> Arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
        ///     执行状态
        /// </summary>
        [DataMember] [JsonProperty("status")] public ZeroOperatorStatus Status;

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
        [JsonProperty("succeed")] public bool IsSucceed => Status == ZeroOperatorStatus.Success;

        /// <summary>
        ///     取参数值
        /// </summary>
        /// <param name="key"></param>
        public string this[string key] => Arguments.TryGetValue(key, out var value) ? value : null;

        /// <summary>
        ///     准备
        /// </summary>
        /// <param name="context"></param>
        public void Prepare(HttpContext context)
        {
            var request = context.Request;
            Uri = request.GetUri();
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
                        break;
                    case "USER-AGENT":
                        userAgent = head.Value.LinkToString("|");
                        break;
                }
            }
            HttpMethod = request.Method.ToUpper();


            GlobalContext.SetRequestContext(new RequestInfo
            {
                RequestId = RandomOperate.Generate(8),
                UserAgent = userAgent,
                Token = Token,
                RequestType = RequestType.Http,
                ArgumentType = ArgumentType.Json,
                Ip = context.Connection.RemoteIpAddress?.ToString(),
                Port = context.Connection.RemotePort.ToString(),
            });
        }
    }
}