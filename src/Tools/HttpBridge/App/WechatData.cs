using System;
using System.Collections.Generic;
using Agebull.MicroZero.ZeroApis;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     路由数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class WechatData
    {
        /// <summary>
        ///     请求的内容
        /// </summary>
        [JsonProperty("context")] public string Context;

        /// <summary>
        /// 参数
        /// </summary>
        [JsonProperty("arguments")]
        public Dictionary<string, string> Arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     请求的表单
        /// </summary>
        public Dictionary<string, List<string>> Headers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     HTTP method
        /// </summary>
        public string HttpMethod;

        /// <summary>
        ///     执行HTTP重写向吗
        /// </summary>
        public bool Redirect;

        /// <summary>
        ///     返回值
        /// </summary>
        public string ResultMessage = "";

        /// <summary>
        ///     执行状态
        /// </summary>
        public UserOperatorStateType Status;

        /// <summary>
        ///     请求地址
        /// </summary>
        public Uri Uri;

        /// <summary>
        ///     开始时间
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        ///     结束时间
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        ///     是否正常
        /// </summary>
        public bool IsSucceed => Status == UserOperatorStateType.Success;

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
            HttpMethod = request.Method.ToUpper();
        }
    }
}