using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Agebull.Common.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Agebull.ZeroNet.ZeroApi;
using Microsoft.AspNetCore.Http;

namespace ZeroNet.Http.Route
{
    /// <summary>
    ///     内部服务调用代理
    /// </summary>
    internal class HttpApiCaller
    {
        #region 基本构造

        /// <summary>
        ///     构造
        /// </summary>
        public HttpApiCaller(string host)
        {
            Host = host;
        }

        /// <summary>
        ///     主机
        /// </summary>
        public string Host { get; }

        /// <summary>
        ///     ApiName
        /// </summary>
        public string ApiName { get; set; }

        /// <summary>
        ///     身份头
        /// </summary>
        public string Bearer { get; set; }

        #endregion

        #region 辅助方法

        /// <summary>
        ///     参数格式化
        /// </summary>
        /// <param name="httpParams"></param>
        /// <returns></returns>
        public static string FormatParams(IEnumerable<KeyValuePair<string, StringValues>> httpParams)
        {
            if (httpParams == null)
                return null;
            var first = true;
            var builder = new StringBuilder();
            foreach (var kvp in httpParams)
            {
                if (first)
                    first = false;
                else
                    builder.Append('&');
                builder.Append($"{kvp.Key}=");
                if (!string.IsNullOrEmpty(kvp.Value))
                    builder.Append($"{HttpUtility.UrlEncode(kvp.Value, Encoding.UTF8)}");
            }
            return builder.ToString();
        }

        private string _url;

        /// <summary>
        /// 生成请求对象
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="method"></param>
        /// <param name="destRequest"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public HttpWebRequest CreateRequest(string apiName, string method, HttpRequest destRequest, RouteData data)
        {
            ApiName = apiName;
            var auth = Bearer;

            _url = $"{Host?.TrimEnd('/') + "/"}{apiName?.TrimStart('/')}";


            var req = (HttpWebRequest)WebRequest.Create(_url);
            req.Timeout = AppConfig.Config.SystemConfig.HttpTimeOut;

            req.Method = method;
            req.Headers.Add(HttpRequestHeader.Authorization, auth);

            if (destRequest.ContentLength == null)
                return req;

            req.ContentType = destRequest.ContentType;
            if (destRequest.HasFormContentType)
            {
                req.ContentType = "application/x-www-form-urlencoded";
                var builder = new StringBuilder();
                bool first = true;
                foreach (var kvp in destRequest.Form)
                {
                    if (first)
                        first = false;
                    else
                        builder.Append('&');
                    builder.Append($"{kvp.Key}=");
                    if (!string.IsNullOrEmpty(kvp.Value))
                        builder.Append($"{HttpUtility.UrlEncode(kvp.Value, Encoding.UTF8)}");
                }
                var form = builder.ToString();
                using (var rs = req.GetRequestStream())
                {
                    var formData = Encoding.UTF8.GetBytes(form);
                    rs.Write(formData, 0, formData.Length);
                }
                LogRecorder.MonitorTrace($"Form:{form}");
            }
            else
            {
                req.ContentType = "application/json;charset=utf-8";
                if (string.IsNullOrWhiteSpace(data.Context))
                {
                    using (var texter = new StreamReader(destRequest.Body))
                    {
                        data.Context = texter.ReadToEnd();
                    }
                }
                if (!string.IsNullOrWhiteSpace(data.Context))
                {
                    var buffer = data.Context.ToUtf8Bytes();
                    using (var rs = req.GetRequestStream())
                    {
                        rs.Write(buffer, 0, buffer.Length);
                    }
                    LogRecorder.MonitorTrace($"Context:{data.Context}");
                }
            }

            return req;
        }
        /// <summary>
        /// 序列化到错误内容
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="message2"></param>
        /// <returns></returns>
        private string ToErrorString(int code, string message, string message2 = null)
        {
            LogRecorder.MonitorTrace($"调用异常：{message}");
            return JsonConvert.SerializeObject(ApiResult.Error(code, _url + message, message2));
        }

        /// <summary>
        ///     取返回值
        /// </summary>
        /// <param name="req"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public string GetResult(HttpWebRequest req, out WebExceptionStatus status)
        {
            var jsonResult = GetResponse(req, out status);
            return string.IsNullOrWhiteSpace(jsonResult) ? RouteRuntime.RemoteEmptyError : jsonResult;
        }

        /// <summary>
        ///     取返回值
        /// </summary>
        /// <param name="req"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private string GetResponse(HttpWebRequest req, out WebExceptionStatus status)
        {
            status = WebExceptionStatus.Success;
            try
            {
                return ReadResponse(req.GetResponse());
            }
            catch (WebException e)
            {
                status = e.Status;
                return e.Status == WebExceptionStatus.ProtocolError ? ProtocolError(e, ref status) : ResponseError(e);
            }
            catch (Exception e)
            {
                status = WebExceptionStatus.UnknownError;
                LogRecorder.Exception(e);
                return ToErrorString(ErrorCode.UnknowError, "未知错误", e.Message);
            }
        }
        /// <summary>
        /// 协议错误
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private string ProtocolError(WebException exception, ref WebExceptionStatus status)
        {
            try
            {
                var codes = exception.Message.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (codes.Length == 3)
                {
                    if (int.TryParse(codes[1], out var s))
                    {
                        switch (s)
                        {
                            case 404:
                                return ToErrorString(ErrorCode.NetworkError, "页面不存在", "页面不存在");
                            case 503:
                                return ToErrorString(ErrorCode.NetworkError, "拒绝访问", "页面不存在");
                                //default:
                                //    return ToErrorString(ErrorCode.NetworkError, $"错误{s}", exception.Message);
                        }
                    }
                }
                var msg = ReadResponse(exception.Response);
                RuntimeWaring.Waring(Host, ApiName, msg);
                return msg; //ToErrorString(ErrorCode.NetworkError, "未知错误", );
            }
            catch (Exception e)
            {
                status = WebExceptionStatus.UnknownError;
                LogRecorder.Exception(e);
                return ToErrorString(ErrorCode.NetworkError, "未知错误", e.Message);
            }
        }
        private string ResponseError(WebException e)
        {
            switch (e.Status)
            {
                case WebExceptionStatus.CacheEntryNotFound:
                    return ToErrorString(ErrorCode.NetworkError, "找不到指定的缓存项");
                case WebExceptionStatus.ConnectFailure:
                    return ToErrorString(ErrorCode.NetworkError, "在传输级别无法联系远程服务点");
                case WebExceptionStatus.ConnectionClosed:
                    return ToErrorString(ErrorCode.NetworkError, "过早关闭连接");
                case WebExceptionStatus.KeepAliveFailure:
                    return ToErrorString(ErrorCode.NetworkError, "指定保持活动状态的标头的请求的连接意外关闭");
                case WebExceptionStatus.MessageLengthLimitExceeded:
                    return ToErrorString(ErrorCode.NetworkError, "已收到一条消息的发送请求时超出指定的限制或从服务器接收响应");
                case WebExceptionStatus.NameResolutionFailure:
                    return ToErrorString(ErrorCode.NetworkError, "名称解析程序服务或无法解析主机名");
                case WebExceptionStatus.Pending:
                    return ToErrorString(ErrorCode.NetworkError, "内部异步请求处于挂起状态");
                case WebExceptionStatus.PipelineFailure:
                    return ToErrorString(ErrorCode.NetworkError, "该请求是管线请求和连接被关闭之前收到响应");
                case WebExceptionStatus.ProxyNameResolutionFailure:
                    return ToErrorString(ErrorCode.NetworkError, "名称解析程序服务无法解析代理服务器主机名");
                case WebExceptionStatus.ReceiveFailure:
                    return ToErrorString(ErrorCode.NetworkError, "从远程服务器未收到完整的响应");
                case WebExceptionStatus.RequestCanceled:
                    return ToErrorString(ErrorCode.NetworkError, "请求已取消");
                case WebExceptionStatus.RequestProhibitedByCachePolicy:
                    return ToErrorString(ErrorCode.NetworkError, "缓存策略不允许该请求");
                case WebExceptionStatus.RequestProhibitedByProxy:
                    return ToErrorString(ErrorCode.NetworkError, "由该代理不允许此请求");
                case WebExceptionStatus.SecureChannelFailure:
                    return ToErrorString(ErrorCode.NetworkError, "使用 SSL 建立连接时出错");
                case WebExceptionStatus.SendFailure:
                    return ToErrorString(ErrorCode.NetworkError, "无法与远程服务器发送一个完整的请求");
                case WebExceptionStatus.ServerProtocolViolation:
                    return ToErrorString(ErrorCode.NetworkError, "服务器响应不是有效的 HTTP 响应");
                case WebExceptionStatus.Timeout:
                    return ToErrorString(ErrorCode.NetworkError, "请求的超时期限内未不收到任何响应");
                case WebExceptionStatus.TrustFailure:
                    return ToErrorString(ErrorCode.NetworkError, "无法验证服务器证书");
                default:
                    return ToErrorString(ErrorCode.NetworkError, "内部服务器异常(未知错误)");
            }
        }

        private string ReadResponse(WebResponse response)
        {
            if (response.ContentLength == 0)
                return RouteRuntime.RemoteEmptyError;
            using (response)
            {
                var receivedStream = response.GetResponseStream();
                if (receivedStream == null)
                    return RouteRuntime.RemoteEmptyError;
                using (receivedStream)
                {
                    var streamReader = new StreamReader(receivedStream);
                    return streamReader.ReadToEnd();
                }
            }
        }

        #endregion
    }
}