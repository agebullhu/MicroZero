using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Agebull.Common.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Yizuan.Service.Api;

namespace Yizuan.Service.Host
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

        /// <summary>
        /// 生成请求对象
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="method"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public HttpWebRequest CreateRequest(string apiName, string method, IEnumerable<KeyValuePair<string, StringValues>> form)
        {
            var auth = Bearer;

            var url = $"{Host?.TrimEnd('/') + "/"}{apiName?.TrimStart('/')}";

            LogRecorder.BeginStepMonitor($"内部API调用:{apiName}");
            LogRecorder.MonitorTrace($"Url:{url}");
            LogRecorder.MonitorTrace($"Auth:{auth}");

            var req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = method;
            //req.Headers.Add(HttpRequestHeader.UserAgent, "Yizuan.Service.WebApi.WebApiCaller");
            req.Headers.Add(HttpRequestHeader.Authorization, auth);

            if (method.ToUpper() == "POST")
            {
                var fm = FormatParams(form) ?? "";
                req.ContentType = "application/x-www-form-urlencoded";
                using (var rs = req.GetRequestStream())
                {
                    var formData = Encoding.UTF8.GetBytes(fm);
                    rs.Write(formData, 0, formData.Length);
                }
                LogRecorder.MonitorTrace("Form:" + fm);
            }
            return req;
        }

        static string ToString(object obj)
        {
            return obj == null ? "{}" : JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        ///     取返回值
        /// </summary>
        /// <param name="req"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public string GetResult(HttpWebRequest req, out WebExceptionStatus status)
        {
            string jsonResult= GetResponse(req,out status );
            if (string.IsNullOrWhiteSpace(jsonResult))
                return ToString(ApiResult.Error(ErrorCode.UnknowError));
            LogRecorder.MonitorTrace(jsonResult);
            LogRecorder.EndStepMonitor();
            return jsonResult;
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
                if (e.Status != WebExceptionStatus.ProtocolError)
                {
                    LogRecorder.EndStepMonitor();
                    return ResponseError(e);
                }
                var codes = e.Message.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (codes.Length == 3)
                {
                    int s;
                    if (int.TryParse(codes[1], out s))
                    {
                        switch (s)
                        {
                            case 404:
                                LogRecorder.Error($"内部服务器异常(找不到页面):{e.Response.ResponseUri}");
                                return ToString(ApiResult.Error(ErrorCode.NetworkError, "页面不存在", "页面不存在"));
                            case 503:
                                LogRecorder.Error($"内部服务器异常(拒绝访问):{e.Response.ResponseUri}");
                                return ToString(ApiResult.Error(ErrorCode.NetworkError, "拒绝访问", "页面不存在"));
                        }
                    }
                }
                return ReadResponse(e.Response);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                LogRecorder.EndStepMonitor();
                return ToString(ApiResult.Error(ErrorCode.UnknowError, "未知错误", e.Message));
            }
        }

        private static string ResponseError(WebException e)
        {
            switch (e.Status)
            {
                case WebExceptionStatus.CacheEntryNotFound:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "找不到指定的缓存项"));
                case WebExceptionStatus.ConnectFailure:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "在传输级别无法联系远程服务点"));
                case WebExceptionStatus.ConnectionClosed:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "过早关闭连接"));
                case WebExceptionStatus.KeepAliveFailure:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "指定保持活动状态的标头的请求的连接意外关闭"));
                case WebExceptionStatus.MessageLengthLimitExceeded:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError,
                        "已收到一条消息的发送请求时超出指定的限制或从服务器接收响应"));
                case WebExceptionStatus.NameResolutionFailure:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "名称解析程序服务或无法解析主机名"));
                case WebExceptionStatus.Pending:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "内部异步请求处于挂起状态"));
                case WebExceptionStatus.PipelineFailure:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "该请求是管线请求和连接被关闭之前收到响应"));
                case WebExceptionStatus.ProxyNameResolutionFailure:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "名称解析程序服务无法解析代理服务器主机名"));
                case WebExceptionStatus.ReceiveFailure:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "从远程服务器未收到完整的响应"));
                case WebExceptionStatus.RequestCanceled:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "请求已取消"));
                case WebExceptionStatus.RequestProhibitedByCachePolicy:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "缓存策略不允许该请求"));
                case WebExceptionStatus.RequestProhibitedByProxy:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "由该代理不允许此请求"));
                case WebExceptionStatus.SecureChannelFailure:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "使用 SSL 建立连接时出错"));
                case WebExceptionStatus.SendFailure:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "无法与远程服务器发送一个完整的请求"));
                case WebExceptionStatus.ServerProtocolViolation:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "服务器响应不是有效的 HTTP 响应"));
                case WebExceptionStatus.Timeout:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "请求的超时期限内未不收到任何响应"));
                case WebExceptionStatus.TrustFailure:
                    LogRecorder.EndStepMonitor();
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "无法验证服务器证书"));
                default:
                    LogRecorder.Error($"内部服务器异常(未知错误):{e.Response.ResponseUri}");
                    //case WebExceptionStatus.UnknownError:
                    return ToString(ApiResult.Error(ErrorCode.NetworkError, "内部服务器异常(未知错误)"));
            }
        }

        private static string ReadResponse(WebResponse response)
        {
            if (response.ContentLength == 0)
                return ToString(ApiResult.Error(ErrorCode.UnknowError, "服务器无返回值"));
            using (response)
            {
                var receivedStream = response.GetResponseStream();
                if (receivedStream != null)
                {
                    using (receivedStream)
                    {
                        var streamReader = new StreamReader(receivedStream);
                        return streamReader.ReadToEnd();
                    }
                }
                LogRecorder.EndStepMonitor();
                return ToString(ApiResult.Error(ErrorCode.UnknowError, "服务器无返回值"));
            }
        }

        #endregion
    }
}