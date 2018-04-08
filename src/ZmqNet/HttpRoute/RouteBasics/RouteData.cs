using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 路由数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract]
    public class RouteData
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        [DataMember, JsonProperty("start")]
        public DateTime Start { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [DataMember, JsonProperty("end")]
        public DateTime End { get; set; }

        /// <summary>
        /// 请求地址
        /// </summary>
        [DataMember, JsonProperty("uri")]
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
        [DataMember, JsonProperty("status")]
        public RouteStatus Status;
        /// <summary>
        /// 执行HTTP重写向吗
        /// </summary>
        [DataMember, JsonProperty("redirect")]
        public bool Redirect;
        /// <summary>
        ///     返回值
        /// </summary>
        [DataMember, JsonProperty("result")]
        public string ResultMessage;

        /// <summary>
        ///     Http Header中的Authorization信息
        /// </summary>
        [DataMember, JsonProperty("bear")]
        public string Bearer;

        /// <summary>
        ///     当前请求调用的主机名称
        /// </summary>
        [DataMember, JsonProperty("host")]
        public string HostName;

        /// <summary>
        ///     当前请求调用的API名称
        /// </summary>
        [DataMember, JsonProperty("apiName")]
        public string ApiName;

        /// <summary>
        ///     请求的内容
        /// </summary>
        [DataMember, JsonProperty("context")]
        public string Context;

        /// <summary>
        ///     请求的参数
        /// </summary>
        [DataMember, JsonProperty("queryString")]
        public string QueryString;

        /// <summary>
        ///     请求的表单
        /// </summary>
        [DataMember, JsonProperty("headers")]
        public Dictionary< string,List<string>> Headers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     请求的表单
        /// </summary>
        [DataMember, JsonProperty("form")]
        public string Form;
        
        /// <summary>
        ///     HTTP method
        /// </summary>
        [DataMember, JsonProperty("method")]
        public string HttpMethod;
        /// <summary>
        ///     路由主机信息
        /// </summary>
        public HostConfig RouteHost;

        /// <summary>
        /// 是否正常
        /// </summary>
        [DataMember, JsonProperty("succeed")]
        public bool IsSucceed { get; set; }

        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="request"></param>
        public void Prepare(HttpRequest request)
        {
            Uri = request.GetUri();
            foreach (var head in request.Headers)
            {
                Headers.Add(head.Key, head.Value.ToList());
            }
            HttpMethod = request.Method.ToUpper();
            if (request.QueryString.HasValue)
            {
                Form = request.QueryString.ToString();
            }
            if (request.HasFormContentType)
            {
                Form = request.Form.LinkToString(p => $"{p.Key}={p.Value}", "&");
            }
            else if (request.ContentLength != null)
            {
                using (var texter = new StreamReader(request.Body))
                {
                    Context = texter.ReadToEnd();
                }
            }
        }
    }
}