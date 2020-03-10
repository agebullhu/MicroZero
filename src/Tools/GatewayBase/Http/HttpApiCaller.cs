using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Agebull.Common.Context;
using Agebull.Common.Logging;

using Agebull.MicroZero;
using Agebull.MicroZero.ZeroApis;
using Agebull.EntityModel.Common;
using Microsoft.AspNetCore.Http;

namespace MicroZero.Http.Gateway
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
        ///     远程地址
        /// </summary>
        internal string RemoteUrl;

        /// <summary>
        /// 远程请求对象
        /// </summary>
        internal HttpWebRequest RemoteRequest;

        /// <summary>
        /// 执行状态
        /// </summary>
        public UserOperatorStateType UserState { get; set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        public ZeroOperatorStateType ZeroState { get; set; }

        #endregion

        #region 辅助方法

        /// <summary>
        ///     生成请求对象
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="method"></param>
        /// <param name="localRequest"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public void CreateRequest(string apiName, string method, HttpRequest localRequest, RouteData data)
        {
            ApiName = apiName;
            var url = new StringBuilder();
            url.Append($"{Host?.TrimEnd('/') + "/"}{apiName?.TrimStart('/')}");

            if (localRequest.QueryString.HasValue)
            {
                url.Append('?');
                url.Append(data.Uri.Query);
            }
            RemoteUrl = url.ToString();
            RemoteRequest = (HttpWebRequest)WebRequest.Create(RemoteUrl);
            RemoteRequest.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {data.Token}");
            RemoteRequest.Timeout = GatewayOption.Option.SystemConfig.HttpTimeOut;
            RemoteRequest.Method = method;
            RemoteRequest.KeepAlive = true;

            if (localRequest.HasFormContentType)
            {
                RemoteRequest.ContentType = "application/x-www-form-urlencoded";
                var builder = new StringBuilder();
                builder.Append($"_api_context_={HttpUtility.UrlEncode(JsonHelper.SerializeObject(GlobalContext.Current), Encoding.UTF8)}");
                foreach (var kvp in data.Arguments)
                {
                    builder.Append('&');
                    builder.Append($"{kvp.Key}=");
                    if (!string.IsNullOrEmpty(kvp.Value))
                        builder.Append($"{HttpUtility.UrlEncode(kvp.Value, Encoding.UTF8)}");
                }

                using (var rs = RemoteRequest.GetRequestStream())
                {
                    var formData = Encoding.UTF8.GetBytes(builder.ToString());
                    rs.Write(formData, 0, formData.Length);
                }
            }
            else if (localRequest.ContentLength != null)
            {
                if (string.IsNullOrWhiteSpace(data.HttpContent))
                    return;
                RemoteRequest.ContentType = "application/json;charset=utf-8";
                var buffer = data.HttpContent.ToUtf8Bytes();
                using (var rs = RemoteRequest.GetRequestStream())
                {
                    rs.Write(buffer, 0, buffer.Length);
                }
            }
            else
            {
                RemoteRequest.ContentType = localRequest.ContentType;
            }
        }

        /// <summary>
        ///     取返回值
        /// </summary>
        /// <returns></returns>
        public async Task<string> Call()
        {
            string jsonResult;
            UserState = UserOperatorStateType.Success;
            ZeroState = ZeroOperatorStateType.Ok;
            try
            {
                var resp = await RemoteRequest.GetResponseAsync();
                jsonResult = await ReadResponse(resp);
            }
            catch (WebException e)
            {
                LogRecorder.Exception(e);
                jsonResult = e.Status == WebExceptionStatus.ProtocolError
                    ? await ProtocolError(e)
                    : ResponseError(e);
            }
            catch (Exception e)
            {
                UserState = UserOperatorStateType.LocalException;
                ZeroState = ZeroOperatorStateType.LocalException;
                LogRecorder.Exception(e);
                jsonResult = ToErrorString(ErrorCode.LocalException, "未知错误", e.Message);
            }
            return string.IsNullOrWhiteSpace(jsonResult) ? ApiResultIoc.RemoteEmptyErrorJson : jsonResult;
        }

        /// <summary>
        ///     协议错误
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private async Task<string> ProtocolError(WebException exception)
        {
            try
            {
                var codes = exception.Message.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (codes.Length == 3)
                    if (int.TryParse(codes[1], out var s))
                        switch (s)
                        {
                            case 404:
                                UserState = UserOperatorStateType.NotFind;
                                ZeroState = ZeroOperatorStateType.NotFind;
                                return ToErrorString(ErrorCode.NetworkError, "页面不存在", "页面不存在");
                            case 503:
                                UserState = UserOperatorStateType.Unavailable;
                                ZeroState = ZeroOperatorStateType.Unavailable;
                                return ToErrorString(ErrorCode.NetworkError, "拒绝访问", "页面不存在");
                        }

                var msg = await ReadResponse(exception.Response);
                LogRecorder.Error($"Call {Host}/{ApiName} Error:{msg}");
                return msg; //ToErrorString(ErrorCode.NetworkError, "未知错误", );
            }
            catch (Exception e)
            {
                UserState = UserOperatorStateType.LocalException;
                ZeroState = ZeroOperatorStateType.LocalException;
                LogRecorder.Exception(e);
                return ToErrorString(ErrorCode.NetworkError, "未知错误", e.Message);
            }
            finally
            {
                exception.Response?.Close();
            }
        }

        private string ResponseError(WebException e)
        {
            e.Response?.Close();
            switch (e.Status)
            {
                case WebExceptionStatus.CacheEntryNotFound:
                    UserState = UserOperatorStateType.NotFind;
                    ZeroState = ZeroOperatorStateType.NotFind;
                    return ToErrorString(ErrorCode.NetworkError, "找不到指定的缓存项");
                case WebExceptionStatus.ConnectFailure:
                    UserState = UserOperatorStateType.NetWorkError;
                    ZeroState = ZeroOperatorStateType.NetError;
                    return ToErrorString(ErrorCode.NetworkError, "在传输级别无法联系远程服务点");
                case WebExceptionStatus.ConnectionClosed:
                    UserState = UserOperatorStateType.NetWorkError;
                    ZeroState = ZeroOperatorStateType.NetError;
                    return ToErrorString(ErrorCode.NetworkError, "过早关闭连接");
                case WebExceptionStatus.KeepAliveFailure:
                    UserState = UserOperatorStateType.NetWorkError;
                    ZeroState = ZeroOperatorStateType.NetError;
                    return ToErrorString(ErrorCode.NetworkError, "指定保持活动状态的标头的请求的连接意外关闭");
                case WebExceptionStatus.MessageLengthLimitExceeded:
                    UserState = UserOperatorStateType.NetWorkError;
                    ZeroState = ZeroOperatorStateType.NetError;
                    return ToErrorString(ErrorCode.NetworkError, "已收到一条消息的发送请求时超出指定的限制或从服务器接收响应");
                case WebExceptionStatus.NameResolutionFailure:
                    UserState = UserOperatorStateType.NotFind;
                    ZeroState = ZeroOperatorStateType.NotFind;
                    return ToErrorString(ErrorCode.NetworkError, "名称解析程序服务或无法解析主机名");
                case WebExceptionStatus.Pending:
                    UserState = UserOperatorStateType.NetWorkError;
                    ZeroState = ZeroOperatorStateType.NetError;
                    return ToErrorString(ErrorCode.NetworkError, "内部异步请求处于挂起状态");
                case WebExceptionStatus.PipelineFailure:
                    UserState = UserOperatorStateType.NetWorkError;
                    ZeroState = ZeroOperatorStateType.NetError;
                    return ToErrorString(ErrorCode.NetworkError, "该请求是管线请求和连接被关闭之前收到响应");
                case WebExceptionStatus.ProxyNameResolutionFailure:
                    UserState = UserOperatorStateType.NotFind;
                    ZeroState = ZeroOperatorStateType.NotFind;
                    return ToErrorString(ErrorCode.NetworkError, "名称解析程序服务无法解析代理服务器主机名");
                case WebExceptionStatus.ReceiveFailure:
                    UserState = UserOperatorStateType.NetWorkError;
                    ZeroState = ZeroOperatorStateType.NetError;
                    return ToErrorString(ErrorCode.NetworkError, "从远程服务器未收到完整的响应");
                case WebExceptionStatus.RequestCanceled:
                    UserState = UserOperatorStateType.Unavailable;
                    ZeroState = ZeroOperatorStateType.Unavailable;
                    return ToErrorString(ErrorCode.NetworkError, "请求已取消");
                case WebExceptionStatus.RequestProhibitedByCachePolicy:
                    UserState = UserOperatorStateType.NetWorkError;
                    ZeroState = ZeroOperatorStateType.NetError;
                    return ToErrorString(ErrorCode.NetworkError, "缓存策略不允许该请求");
                case WebExceptionStatus.RequestProhibitedByProxy:
                    UserState = UserOperatorStateType.NetWorkError;
                    ZeroState = ZeroOperatorStateType.NetError;
                    return ToErrorString(ErrorCode.NetworkError, "由该代理不允许此请求");
                case WebExceptionStatus.SecureChannelFailure:
                    UserState = UserOperatorStateType.NetWorkError;
                    ZeroState = ZeroOperatorStateType.NetError;
                    return ToErrorString(ErrorCode.NetworkError, "使用 SSL 建立连接时出错");
                case WebExceptionStatus.SendFailure:
                    UserState = UserOperatorStateType.NetWorkError;
                    ZeroState = ZeroOperatorStateType.NetError;
                    return ToErrorString(ErrorCode.NetworkError, "无法与远程服务器发送一个完整的请求");
                case WebExceptionStatus.ServerProtocolViolation:
                    UserState = UserOperatorStateType.NetWorkError;
                    ZeroState = ZeroOperatorStateType.NetError;
                    return ToErrorString(ErrorCode.NetworkError, "服务器响应不是有效的 HTTP 响应");
                case WebExceptionStatus.Timeout:
                    UserState = UserOperatorStateType.NetWorkError;
                    ZeroState = ZeroOperatorStateType.NetError;
                    return ToErrorString(ErrorCode.NetworkError, "请求的超时期限内未不收到任何响应");
                case WebExceptionStatus.TrustFailure:
                    UserState = UserOperatorStateType.NetWorkError;
                    ZeroState = ZeroOperatorStateType.NetError;
                    return ToErrorString(ErrorCode.NetworkError, "无法验证服务器证书");
                default:
                    UserState = UserOperatorStateType.RemoteError;
                    ZeroState = ZeroOperatorStateType.NetError;
                    return ToErrorString(ErrorCode.NetworkError, "内部服务器异常(未知错误)");
            }
        }

        /// <summary>
        ///     读取返回消息
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private static async Task<string> ReadResponse(WebResponse response)
        {
            Memory<char> memory = new Memory<char>();
            //int len;
            using (response)
            {
                if (response.ContentLength == 0)
                {
                    return ApiResultIoc.RemoteEmptyErrorJson;
                }

                var receivedStream = response.GetResponseStream();
                if (receivedStream == null)
                    return ApiResultIoc.RemoteEmptyErrorJson;

                using (receivedStream)
                {
                    using (var streamReader = new StreamReader(receivedStream))
                    {
                        /*len = */await streamReader.ReadAsync(memory);
                        streamReader.Close();
                    }
                    receivedStream.Close();
                }
                response.Close();
            }
            return memory.ToNullString();
        }

        /// <summary>
        ///     序列化到错误内容
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="message2"></param>
        /// <returns></returns>
        private string ToErrorString(int code, string message, string message2 = null)
        {
            LogRecorder.MonitorTrace(() => $"调用异常：{message}.{message2}");
            var result = ApiResultIoc.Ioc.Error(code, RemoteUrl + message, message2);
            result.Status.Point = "web api gateway";
            return JsonHelper.SerializeObject(result);
        }
        #endregion
    }
}