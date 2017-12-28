using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Agebull.Common.Logging;
using Newtonsoft.Json;

namespace Yizuan.Service.Api.WebApi
{
    /// <summary>
    ///     内部服务调用代理
    /// </summary>
    public class WebApiCaller
    {
        #region 基本构造

        /// <summary>
        ///     构造
        /// </summary>
        public WebApiCaller()
        {
            ApiContext.TryCheckByAnymouse();
        }

        /// <summary>
        ///     构造
        /// </summary>
        public WebApiCaller(string host)
        {
            Host = host;
            ApiContext.TryCheckByAnymouse();
        }

        /// <summary>
        ///     构造
        /// </summary>
        public WebApiCaller(string host, string beare)
        {
            Host = host;
            _beare = beare;
            ApiContext.TryCheckByAnymouse();
        }

        /// <summary>
        ///     主机
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        ///     身份头
        /// </summary>
        private readonly string _beare;

        /// <summary>
        ///     身份头
        /// </summary>
        public string Bearer => _beare ?? $"${ApiContext.RequestContext.RequestId}";

        #endregion

        #region 辅助方法

        /// <summary>
        ///     参数格式化
        /// </summary>
        /// <param name="httpParams"></param>
        /// <returns></returns>
        public static string FormatParams(Dictionary<string, string> httpParams)
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
        /// <param name="arg"></param>
        /// <returns></returns>
        HttpWebRequest CreateRequest(string apiName, string method, string arg)
        {
            var auth = $"Bearer {Bearer}";

            var url = $"{Host?.TrimEnd('/') + "/"}{apiName?.TrimStart('/')}";
            if (method == "GET" && !string.IsNullOrWhiteSpace(arg))
                url = $"{url}?{arg}";

            LogRecorder.BeginStepMonitor($"内部API调用:{apiName}");
            LogRecorder.MonitorTrace($"Url:{url}");
            LogRecorder.MonitorTrace($"Auth:{auth}");

            var req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = method;
            //req.Headers.Add(HttpRequestHeader.UserAgent, "Yizuan.Service.WebApi.WebApiCaller");
            req.Headers.Add(HttpRequestHeader.Authorization, auth);

            if (method == "POST" )
            {
                if (arg == null)
                    arg = "";
                req.ContentType = "application/x-www-form-urlencoded";
                using (var rs = req.GetRequestStream())
                {
                    var formData = Encoding.UTF8.GetBytes(arg);
                    rs.Write(formData, 0, formData.Length);
                }
                LogRecorder.MonitorTrace("Form:" + arg);
            }
            return req;
        }
        #endregion

        #region 强类型取得

        /// <summary>
        ///     通过Get调用
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="apiName"></param>
        /// <returns></returns>
        public ApiResult<TResult> Get<TResult>(string apiName)
            where TResult : IApiResultData
        {
            return Get<TResult>(apiName, "");
        }

        /// <summary>
        ///     通过Get调用
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="apiName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public ApiResult<TResult> Get<TResult>(string apiName, Dictionary<string, string> arguments)
            where TResult : IApiResultData
        {
            return Get<TResult>(apiName, FormatParams(arguments));
        }

        /// <summary>
        ///     通过Get调用
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="apiName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public ApiResult<TResult> Get<TResult>(string apiName, IApiArgument arguments)
            where TResult : IApiResultData
        {
            return Get<TResult>(apiName, arguments?.ToFormString());
        }

        /// <summary>
        ///     通过Get调用
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="apiName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public ApiResult<TResult> Get<TResult>(string apiName, string arguments)
            where TResult : IApiResultData
        {
            var req = CreateRequest(apiName, "GET", arguments);
            return GetResult<TResult>(req);
        }

        /// <summary>
        ///     通过Post调用
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="apiName"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public ApiResult<TResult> Post<TResult>(string apiName, IApiArgument argument)
            where TResult : IApiResultData
        {
            return Post<TResult>(apiName, argument?.ToFormString());
        }

        /// <summary>
        ///     通过Post调用
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="apiName"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public ApiResult<TResult> Post<TResult>(string apiName, Dictionary<string, string> argument)
            where TResult : IApiResultData
        {
            return Post<TResult>(apiName, FormatParams(argument));
        }

        /// <summary>
        ///     通过Post调用
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="apiName"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public ApiResult<TResult> Post<TResult>(string apiName, string form)
            where TResult : IApiResultData
        {
            var req = CreateRequest(apiName, "POST", form);
            LogRecorder.MonitorTrace("Form:" + form);
            try
            {
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                LogRecorder.EndStepMonitor();
                return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError);
            }

            return GetResult<TResult>(req);
        }

        /// <summary>
        ///     取返回值
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="req"></param>
        /// <returns></returns>
        public ApiResult<TResult> GetResult<TResult>(HttpWebRequest req)
            where TResult : IApiResultData
        {
            string jsonResult;
            try
            {
                using (var response = req.GetResponse())
                {
                    var receivedStream = response.GetResponseStream();
                    if (receivedStream != null)
                    {
                        var streamReader = new StreamReader(receivedStream);
                        jsonResult = streamReader.ReadToEnd();
                        receivedStream.Dispose();
                    }
                    else
                    {
                        LogRecorder.EndStepMonitor();
                        return ApiResult<TResult>.ErrorResult(ErrorCode.UnknowError, "服务器无返回值");
                    }
                    response.Close();
                }
            }
            catch (WebException e)
            {
                if (e.Status != WebExceptionStatus.ProtocolError)
                {
                    LogRecorder.EndStepMonitor();
                    switch (e.Status)
                    {
                        case WebExceptionStatus.CacheEntryNotFound:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "找不到指定的缓存项");
                        case WebExceptionStatus.ConnectFailure:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "在传输级别无法联系远程服务点");
                        case WebExceptionStatus.ConnectionClosed:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "过早关闭连接");
                        case WebExceptionStatus.KeepAliveFailure:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "指定保持活动状态的标头的请求的连接意外关闭");
                        case WebExceptionStatus.MessageLengthLimitExceeded:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError,
                                "已收到一条消息的发送请求时超出指定的限制或从服务器接收响应");
                        case WebExceptionStatus.NameResolutionFailure:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "名称解析程序服务或无法解析主机名");
                        case WebExceptionStatus.Pending:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "内部异步请求处于挂起状态");
                        case WebExceptionStatus.PipelineFailure:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "该请求是管线请求和连接被关闭之前收到响应");
                        case WebExceptionStatus.ProxyNameResolutionFailure:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "名称解析程序服务无法解析代理服务器主机名");
                        case WebExceptionStatus.ReceiveFailure:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "从远程服务器未收到完整的响应");
                        case WebExceptionStatus.RequestCanceled:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "请求已取消");
                        case WebExceptionStatus.RequestProhibitedByCachePolicy:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "缓存策略不允许该请求");
                        case WebExceptionStatus.RequestProhibitedByProxy:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "由该代理不允许此请求");
                        case WebExceptionStatus.SecureChannelFailure:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "使用 SSL 建立连接时出错");
                        case WebExceptionStatus.SendFailure:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "无法与远程服务器发送一个完整的请求");
                        case WebExceptionStatus.ServerProtocolViolation:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "服务器响应不是有效的 HTTP 响应");
                        case WebExceptionStatus.Timeout:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "请求的超时期限内未不收到任何响应");
                        case WebExceptionStatus.TrustFailure:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "无法验证服务器证书");
                        default:
                            //case WebExceptionStatus.UnknownError:
                            return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError, "发生未知类型的异常");
                    }
                }
                using (var response = e.Response)
                {
                    var receivedStream = response.GetResponseStream();
                    if (receivedStream != null)
                    {
                        var streamReader = new StreamReader(receivedStream);
                        jsonResult = streamReader.ReadToEnd();
                        receivedStream.Dispose();
                    }
                    else
                    {
                        LogRecorder.EndStepMonitor();
                        return ApiResult<TResult>.ErrorResult(ErrorCode.UnknowError, "服务器无返回值");
                    }
                    response.Close();
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                LogRecorder.EndStepMonitor();
                return ApiResult<TResult>.ErrorResult(ErrorCode.NetworkError);
            }
            LogRecorder.MonitorTrace(jsonResult);
            try
            {
                if (string.IsNullOrWhiteSpace(jsonResult))
                    return ApiResult<TResult>.ErrorResult(ErrorCode.UnknowError);
                return JsonConvert.DeserializeObject<ApiResult<TResult>>(jsonResult);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return ApiResult<TResult>.ErrorResult(ErrorCode.UnknowError);
            }
            finally
            {
                LogRecorder.EndStepMonitor();
            }
        }

        #endregion

        #region 无类型取得

        /// <summary>
        ///     通过Get调用
        /// </summary>
        /// <param name="apiName"></param>
        /// <returns></returns>
        public ApiValueResult<string> Get(string apiName)
        {
            return Get(apiName, "");
        }

        /// <summary>
        ///     通过Get调用
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public ApiValueResult<string> Get(string apiName, Dictionary<string, string> arguments)
        {
            return Get(apiName, FormatParams(arguments));
        }

        /// <summary>
        ///     通过Get调用
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public ApiValueResult<string> Get(string apiName, IApiArgument arguments)
        {
            return Get(apiName, arguments?.ToFormString());
        }

        /// <summary>
        ///     通过Get调用
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public ApiValueResult<string> Get(string apiName, string arguments)
        {
            var req = CreateRequest(apiName, "GET", arguments);
            return GetResult(req);
        }

        /// <summary>
        ///     通过Post调用
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public ApiValueResult<string> Post(string apiName, IApiArgument argument)
        {
            return Post(apiName, argument?.ToFormString());
        }

        /// <summary>
        ///     通过Post调用
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public ApiValueResult<string> Post(string apiName, Dictionary<string, string> argument)
        {
            return Post(apiName, FormatParams(argument));
        }

        /// <summary>
        ///     通过Post调用
        /// </summary>
        /// <param name="apiName"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public ApiValueResult<string> Post(string apiName, string form)
        {
            var req = CreateRequest(apiName, "POST",form);
            return GetResult(req);
        }


        /// <summary>
        ///     取返回值
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public ApiValueResult<string> GetResult(HttpWebRequest req)
        {
            string jsonResult;
            try
            {
                using (var response = req.GetResponse())
                {
                    var receivedStream = response.GetResponseStream();
                    if (receivedStream != null)
                    {
                        var streamReader = new StreamReader(receivedStream);
                        jsonResult = streamReader.ReadToEnd();
                        receivedStream.Dispose();
                    }
                    else
                    {
                        LogRecorder.EndStepMonitor();
                        return ApiValueResult<string>.ErrorResult(ErrorCode.UnknowError, "服务器无返回值");
                    }
                    response.Close();
                }
            }
            catch (WebException e)
            {
                if (e.Status != WebExceptionStatus.ProtocolError)
                {
                    LogRecorder.EndStepMonitor();
                    switch (e.Status)
                    {
                        case WebExceptionStatus.CacheEntryNotFound:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "找不到指定的缓存项");
                        case WebExceptionStatus.ConnectFailure:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "在传输级别无法联系远程服务点");
                        case WebExceptionStatus.ConnectionClosed:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "过早关闭连接");
                        case WebExceptionStatus.KeepAliveFailure:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "指定保持活动状态的标头的请求的连接意外关闭");
                        case WebExceptionStatus.MessageLengthLimitExceeded:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError,
                                "已收到一条消息的发送请求时超出指定的限制或从服务器接收响应");
                        case WebExceptionStatus.NameResolutionFailure:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "名称解析程序服务或无法解析主机名");
                        case WebExceptionStatus.Pending:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "内部异步请求处于挂起状态");
                        case WebExceptionStatus.PipelineFailure:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "该请求是管线请求和连接被关闭之前收到响应");
                        case WebExceptionStatus.ProxyNameResolutionFailure:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "名称解析程序服务无法解析代理服务器主机名");
                        case WebExceptionStatus.ReceiveFailure:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "从远程服务器未收到完整的响应");
                        case WebExceptionStatus.RequestCanceled:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "请求已取消");
                        case WebExceptionStatus.RequestProhibitedByCachePolicy:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "缓存策略不允许该请求");
                        case WebExceptionStatus.RequestProhibitedByProxy:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "由该代理不允许此请求");
                        case WebExceptionStatus.SecureChannelFailure:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "使用 SSL 建立连接时出错");
                        case WebExceptionStatus.SendFailure:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "无法与远程服务器发送一个完整的请求");
                        case WebExceptionStatus.ServerProtocolViolation:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "服务器响应不是有效的 HTTP 响应");
                        case WebExceptionStatus.Timeout:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "请求的超时期限内未不收到任何响应");
                        case WebExceptionStatus.TrustFailure:
                            LogRecorder.EndStepMonitor();
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "无法验证服务器证书");
                        default:
                            //case WebExceptionStatus.UnknownError:
                            return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "发生未知类型的异常");
                    }
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
                                return ApiValueResult<string>.ErrorResult(ErrorCode.NetworkError, "服务器内部错误", "页面不存在");
                        }
                    }
                }
                using (var response = e.Response)
                {
                    var receivedStream = response.GetResponseStream();
                    if (receivedStream != null)
                    {
                        var streamReader = new StreamReader(receivedStream);
                        jsonResult = streamReader.ReadToEnd();
                        receivedStream.Dispose();
                    }
                    else
                    {
                        LogRecorder.EndStepMonitor();
                        return ApiValueResult<string>.ErrorResult(ErrorCode.UnknowError, "服务器无返回值");
                    }
                    response.Close();
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                LogRecorder.EndStepMonitor();
                return ApiValueResult<string>.ErrorResult(ErrorCode.UnknowError, "未知错误", e.Message);
            }
            LogRecorder.MonitorTrace(jsonResult);
            try
            {
                if (string.IsNullOrWhiteSpace(jsonResult))
                    return ApiValueResult<string>.ErrorResult(ErrorCode.UnknowError);
                var baseResult = JsonConvert.DeserializeObject<ApiResult>(jsonResult);
                return !baseResult.Result
                    ? ApiValueResult<string>.ErrorResult(baseResult.Status.ErrorCode, baseResult.Status.Message)
                    : ApiValueResult<string>.Succees(
                        ReadResultData(jsonResult, nameof(ApiValueResult<string>.ResultData)));
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return ApiValueResult<string>.ErrorResult(ErrorCode.UnknowError, "未知错误", e.Message);
            }
            finally
            {
                LogRecorder.EndStepMonitor();
            }
        }


        private static string ReadResultData(string json, string property)
        {
            if (json == null || json.Trim()[0] != '{')
                return json;
            var code = new StringBuilder();
            using (var textReader = new StringReader(json))
            {
                var reader = new JsonTextReader(textReader);
                var isResultData = false;
                var levle = 0;
                while (reader.Read())
                {
                    if (!isResultData && reader.TokenType == JsonToken.PropertyName)
                    {
                        var name = reader.Value.ToString();
                        if (name == property)
                            isResultData = true;
                        continue;
                    }
                    if (!isResultData)
                        continue;
                    switch (reader.TokenType)
                    {
                        case JsonToken.StartArray:
                            code.Append('[');
                            continue;
                        case JsonToken.StartObject:
                            code.Append('{');
                            levle++;
                            continue;
                        case JsonToken.PropertyName:
                            code.Append($"\"{reader.Value}\"=");
                            continue;
                        case JsonToken.Bytes:
                            code.Append($"\"{reader.Value}\"");
                            break;
                        case JsonToken.Date:
                        case JsonToken.String:
                            code.Append($"\"{reader.Value}\"");
                            break;
                        case JsonToken.Integer:
                        case JsonToken.Float:
                        case JsonToken.Boolean:
                            code.Append("null");
                            break;
                        case JsonToken.Null:
                            code.Append("null");
                            break;
                        case JsonToken.EndObject:
                            if (code.Length > 0 && code[code.Length - 1] == ',')
                                code[code.Length - 1] = '}';
                            else
                                code.Append('}');
                            levle--;
                            break;
                        case JsonToken.EndArray:
                            if (code.Length > 0 && code[code.Length - 1] == ',')
                                code[code.Length - 1] = ']';
                            else
                                code.Append(']');
                            break;
                        case JsonToken.Raw:
                            code.Append(reader.Value);
                            break;
                        case JsonToken.Undefined:
                            break;
                        case JsonToken.StartConstructor:
                            break;
                        case JsonToken.None:
                            break;
                        case JsonToken.EndConstructor:
                            break;
                        case JsonToken.Comment:
                            break;
                    }
                    if (levle == 0)
                        break;
                    code.Append(',');
                }
            }
            if (code.Length > 0 && code[code.Length - 1] == ',')
                code[code.Length - 1] = ' ';
            return code.ToString().Trim('\'', '\"');
        }

        #endregion
    }
}