using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Agebull.Common.Context;
using Agebull.Common.Logging;

using Agebull.MicroZero;
using Agebull.MicroZero.ZeroApis;
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
        [DataMember] [JsonProperty("apiHost")] public string ApiHost { get; internal set; }

        /// <summary>
        ///     当前请求调用的API名称
        /// </summary>
        [DataMember] [JsonProperty("apiName")] public string ApiName { get; internal set; }

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
        /// 取参数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key] => Arguments.TryGetValue(key, out var vl) ? vl : null;

        /// <summary>
        ///     返回二进制值
        /// </summary>
        [IgnoreDataMember] [JsonIgnore] public Dictionary<string, byte[]> Files;

        /// <summary>
        ///     执行HTTP重写向吗
        /// </summary>
        [DataMember]
        [JsonProperty("redirect")]
        public bool Redirect;

        /// <summary>
        ///     返回文本值
        /// </summary>
        [DataMember] [JsonProperty("result")] public string ResultMessage;

        /// <summary>
        ///     返回二进制值
        /// </summary>
        [IgnoreDataMember] [JsonIgnore] public byte[] ResultBinary;

        /// <summary>
        ///     返回值是否文件
        /// </summary>
        [DataMember] [JsonProperty("binary")] public bool IsFile;

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
                Ip = request.Headers["X-Forwarded-For"].FirstOrDefault() ?? request.Headers["X-Real-IP"].FirstOrDefault() ?? context.Connection.RemoteIpAddress?.ToString(),
                Port = request.Headers["X-Real-Port"].FirstOrDefault() ?? context.Connection.RemotePort.ToString(),
            });
            return ok && Read(context);
        }

        private string CheckHeaders(HttpContext context, HttpRequest request)
        {
            Token = request.Headers["AUTHORIZATION"].LastOrDefault()?
                .Trim()
                .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Last()
                ?? context.Request.Query["token"];
            if (string.IsNullOrWhiteSpace(Token) || Token.Equals("null") || Token.Equals("undefined") || Token.Equals("Bearer"))
            {
                Token = null;
            }
            return null;
            //string userAgent = null;
            //foreach (var head in request.Headers)
            //{
            //    var key = head.Key.ToUpper();
            //    switch (key)
            //    {
            //        case "AUTHORIZATION":
            //            var token = head.Value.LinkToString();
            //            var words = token.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            //            if (words.Length != 2 ||
            //                !string.Equals(words[0], "Bearer", StringComparison.OrdinalIgnoreCase) ||
            //                words[1].Equals("null") ||
            //                words[1].Equals("undefined"))
            //                Token = null;
            //            else
            //                Token = words[1];
            //            break;
            //        //case "USER-AGENT":
            //        //    userAgent = head.Value.LinkToString("|");
            //        //    break;
            //        default:
            //            Headers.Add(key, head.Value.ToList());
            //            break;
            //    }
            //}
            //if (string.IsNullOrWhiteSpace(Token))
            //{
            //    Token = context.Request.Query["token"];
            //}
            //return userAgent;
        }

        /// <summary>
        ///     检查调用内容
        /// </summary>
        /// <returns></returns>
        private bool CheckCall()
        {
            var words = Uri.LocalPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1)
            {
                UserState = UserOperatorStateType.FormalError;
                ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                ResultMessage = ApiResultIoc.ArgumentErrorJson;
                return false;
            }

            if (!RouteOption.Option.HostPath.TryGetValue(Uri.Host, out var idx))
            {
                idx = 0;
            }
            if (words.Length <= idx + 1)
            {
                UserState = UserOperatorStateType.FormalError;
                ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                ResultMessage = ApiResultIoc.ArgumentErrorJson;
                return false;
            }
            ApiHost = words[idx];
            ApiName = string.Join('/', words.Skip(idx + 1));
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
                    var files = request.Form?.Files;
                    if (files != null && files.Count > 0)
                    {
                        Files = new Dictionary<string, byte[]>();
                        foreach (var file in files)
                        {
                            if (Files.ContainsKey(file.Name))
                            {
                                UserState = UserOperatorStateType.FormalError;
                                ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                                ResultMessage = ApiResultIoc.ArgumentErrorJson;
                                return false;
                            }

                            var bytes = new byte[file.Length];
                            using (var stream = file.OpenReadStream())
                            {
                                stream.Read(bytes, 0, (int)file.Length);
                            }
                            Files.Add(file.Name, bytes);
                        }
                    }

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
                LogRecorderX.Exception(e, "读取远程参数");
                UserState = UserOperatorStateType.FormalError;
                ZeroState = ZeroOperatorStateType.ArgumentInvalid;
                ResultMessage = ApiResultIoc.ArgumentErrorJson;
                return false;
            }
        }
    }
}