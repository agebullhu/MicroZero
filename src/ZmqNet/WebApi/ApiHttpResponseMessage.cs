using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.ZeroApi.WebApi
{
    /// <summary>
    /// 基于HTTP协议的返回消息对象扩展
    /// </summary>
    public static class HttpResponseMessageExtend
    {

        /// <summary>
        /// 生成一个标准返回对象
        /// </summary>
        /// <param name="request">请求对象</param>
        /// <returns>HttpResponseMessage对象</returns>
        /// <param name="statusCode">HTTP状态码</param>
        public static ApiResponseMessage ToResponse(this HttpRequestMessage request, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ApiResponseMessage(statusCode)
            {
                RequestMessage = request
            };
        }

        /// <summary>
        /// 生成一个标准返回对象
        /// </summary>
        /// <param name="request">请求对象</param>
        /// <param name="result">返回内容</param>
        /// <returns>HttpResponseMessage对象</returns>
        /// <param name="statusCode">HTTP状态码</param>
        public static ApiResponseMessage<ApiValueResult<T>> ToResponse<T>(this HttpRequestMessage request, ApiValueResult<T> result, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            if (result == null)
            {
                return new ApiResponseMessage<ApiValueResult<T>>(statusCode)
                {
                    RequestMessage = request
                };
            }
            var response = new ApiResponseMessage<ApiValueResult<T>>(result, statusCode)
            {
                RequestMessage = request
            };
            return response;
        }
        
        /// <summary>
        /// 生成一个标准返回对象
        /// </summary>
        /// <param name="request">请求对象</param>
        /// <param name="result">返回内容</param>
        /// <param name="statusCode">HTTP状态码</param>
        /// <returns>HttpResponseMessage对象</returns>
        public static ApiResponseMessage ToResponse(this HttpRequestMessage request, ApiResult result, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ApiResponseMessage(statusCode, result)
            {
                RequestMessage = request
            };
        }
        /// <summary>
        /// 生成一个标准返回对象
        /// </summary>
        /// <param name="request">请求对象</param>
        /// <param name="result">返回内容</param>
        /// <param name="statusCode">HTTP状态码</param>
        /// <returns>HttpResponseMessage对象</returns>
        public static ApiResponseMessage ToResponse(this HttpRequestMessage request, IApiResult result, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ApiResponseMessage(statusCode, result)
            {
                RequestMessage = request
            };
        }

        /// <summary>
        /// 生成一个标准返回对象(没有返回值）
        /// </summary>
        /// <typeparam name="TResult">返回类型，基于ApiResult</typeparam>
        /// <param name="request">请求对象</param>
        /// <param name="result">返回内容</param>
        /// <param name="statusCode">HTTP状态码</param>
        /// <returns>HttpResponseMessage对象</returns>
        public static ApiResponseMessage<ApiResult<TResult>> ToResponse<TResult>(this HttpRequestMessage request, TResult result, HttpStatusCode statusCode = HttpStatusCode.OK)
            where TResult : IApiResultData
        {
            if (result == null)
            {
                return new ApiResponseMessage<ApiResult<TResult>>(statusCode)
                {
                    RequestMessage = request
                };
            }
            var response = new ApiResponseMessage<ApiResult<TResult>>(ApiResult<TResult>.Succees(result),statusCode)
            {
                RequestMessage = request
            };
            return response;
        }
        /// <summary>
        /// 生成一个标准返回对象(返回一般数据)
        /// </summary>
        /// <typeparam name="TResult">返回类型，基于ApiResult</typeparam>
        /// <param name="request">请求对象</param>
        /// <param name="statusCode">HTTP状态码</param>
        /// <param name="result">返回内容</param>
        /// <returns>HttpResponseMessage对象</returns>
        /// <returns></returns>
        public static ApiResponseMessage<ApiResult<TResult>> ToResponse<TResult>(this HttpRequestMessage request, ApiResult<TResult> result, HttpStatusCode statusCode = HttpStatusCode.OK)
            where TResult : IApiResultData
        {
            if (result == null)
            {
                return new ApiResponseMessage<ApiResult<TResult>>(statusCode)
                {
                    RequestMessage = request
                };
            }
            var response = new ApiResponseMessage<ApiResult<TResult>>(statusCode)
            {
                RequestMessage = request,
                Content = new StringContent(JsonConvert.SerializeObject(result))
            };
            return response;
        }

        /// <summary>
        /// 生成一个标准返回对象(返回一般数据)
        /// </summary>
        /// <typeparam name="TResult">返回类型，基于ApiResult</typeparam>
        /// <param name="request">请求对象</param>
        /// <param name="statusCode">HTTP状态码</param>
        /// <param name="result">返回内容</param>
        /// <returns>HttpResponseMessage对象</returns>
        /// <returns></returns>
        public static ApiResponseMessage<ApiResult<TResult>> ToResponse<TResult>(this HttpRequestMessage request, IApiResult<TResult> result, HttpStatusCode statusCode = HttpStatusCode.OK)
            where TResult : IApiResultData
        {
            if (result == null)
            {
                return new ApiResponseMessage<ApiResult<TResult>>(statusCode)
                {
                    RequestMessage = request
                };
            }
            var response = new ApiResponseMessage<ApiResult<TResult>>(statusCode)
            {
                RequestMessage = request,
                Content = new StringContent(JsonConvert.SerializeObject(result))
            };
            return response;
        }
        /// <summary>
        /// 生成一个标准返回对象（返回数组）
        /// </summary>
        /// <typeparam name="TResult">返回类型，基于ApiResult</typeparam>
        /// <param name="request">请求对象</param>
        /// <param name="statusCode">HTTP状态码</param>
        /// <param name="result">返回内容</param>
        /// <returns>HttpResponseMessage对象</returns>
        /// <returns></returns>
        public static ApiResponseMessage<ApiArrayResult<TResult>> ToResponse<TResult>(this HttpRequestMessage request, IList<TResult> result, HttpStatusCode statusCode = HttpStatusCode.OK)
            where TResult : IApiResultData
        {
            if (result == null)
            {
                return new ApiResponseMessage<ApiArrayResult<TResult>>(statusCode)
                {
                    RequestMessage = request
                };
            }
            var response = new ApiResponseMessage<ApiArrayResult<TResult>>(new ApiArrayResult<TResult> {ResultData = new ApiList<TResult>(result) }, statusCode)
            {
                RequestMessage = request
            };
            return response;
        }

        /// <summary>
        /// 生成一个标准返回对象（返回数组）
        /// </summary>
        /// <typeparam name="TResult">返回类型，基于ApiResult</typeparam>
        /// <param name="request">请求对象</param>
        /// <param name="statusCode">HTTP状态码</param>
        /// <param name="result">返回内容</param>
        /// <returns>HttpResponseMessage对象</returns>
        /// <returns></returns>
        public static ApiResponseMessage<ApiArrayResult<TResult>> ToResponse<TResult>(this HttpRequestMessage request, ApiArrayResult<TResult> result, HttpStatusCode statusCode = HttpStatusCode.OK)
            where TResult : IApiResultData
        {
            if (result == null)
            {
                return new ApiResponseMessage<ApiArrayResult<TResult>>(statusCode)
                {
                    RequestMessage = request
                };
            }
            var response = new ApiResponseMessage<ApiArrayResult<TResult>>(result, statusCode)
            {
                RequestMessage = request
            };
            return response;
        }

        /// <summary>
        /// 生成一个标准返回对象（返回分页）
        /// </summary>
        /// <typeparam name="TResult">返回类型，基于ApiResult</typeparam>
        /// <param name="request">请求对象</param>
        /// <param name="statusCode">HTTP状态码</param>
        /// <param name="result">返回内容</param>
        /// <returns>HttpResponseMessage对象</returns>
        /// <returns></returns>
        public static ApiResponseMessage<ApiPageResult<TResult>> ToResponse<TResult>(this HttpRequestMessage request, ApiPageResult<TResult> result, HttpStatusCode statusCode = HttpStatusCode.OK)
            where TResult : IApiResultData
        {
            if (result == null)
            {
                return new ApiResponseMessage<ApiPageResult<TResult>>(statusCode)
                {
                    RequestMessage = request
                };
            }
            var response = new ApiResponseMessage<ApiPageResult<TResult>>(result,statusCode)
            {
                RequestMessage = request
            };
            return response;
        }
    }

    /// <summary>
    /// API返回专用的ResponseMessage
    /// </summary>
    public class ApiResponseMessage : HttpResponseMessage
    {
        /// <summary>
        /// 默认的成功消息
        /// </summary>
        protected static readonly string SuccessMessage;

        static ApiResponseMessage()
        {
            SuccessMessage = JsonConvert.SerializeObject(ApiResult.Succees());
        }

        /// <summary>
        /// 默认构造（状态码为
        /// </summary>
        public ApiResponseMessage()
            : base(HttpStatusCode.OK)
        {
            Content = new StringContent(SuccessMessage);
        }

        /// <summary>
        /// 状态构造
        /// </summary>
        /// <param name="statusCode">状态</param>
        public ApiResponseMessage(HttpStatusCode statusCode)
            : base(statusCode)
        {
            Content = new StringContent(SuccessMessage);
        }

        /// <summary>
        /// 状态构造
        /// </summary>
        /// <param name="statusCode">状态</param>
        /// <param name="result"></param>
        public ApiResponseMessage(HttpStatusCode statusCode, ApiResult result)
            : base(statusCode)
        {
            Content = result != null ? new StringContent(JsonConvert.SerializeObject(result)) : new StringContent(SuccessMessage);
        }

        /// <summary>
        /// 状态构造
        /// </summary>
        /// <param name="statusCode">状态</param>
        /// <param name="result"></param>
        public ApiResponseMessage(HttpStatusCode statusCode, IApiResult result)
            : base(statusCode)
        {
            Content = result != null ? new StringContent(JsonConvert.SerializeObject(result)) : new StringContent(SuccessMessage);
        }
    }
    /// <summary>
    /// API返回专用的ResponseMessage
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class ApiResponseMessage<TResult> : ApiResponseMessage
        where TResult : IApiResult
    {
        /// <summary>
        /// 默认构造（状态码为
        /// </summary>
        public ApiResponseMessage()
            : base(HttpStatusCode.OK)
        {
            Content = new StringContent(SuccessMessage);
        }

        /// <summary>
        /// 状态构造
        /// </summary>
        /// <param name="statusCode">状态</param>
        public ApiResponseMessage(HttpStatusCode statusCode)
            : base(statusCode)
        {
            Content = new StringContent(SuccessMessage);
        }
        /// <summary>
        /// 状态构造
        /// </summary>
        /// <param name="result">数据</param>
        /// <param name="statusCode">状态</param>
        public ApiResponseMessage(TResult result, HttpStatusCode statusCode = HttpStatusCode.OK)
            : base(statusCode)
        {
            Content = result != null ? new StringContent(JsonConvert.SerializeObject(result)) : new StringContent(SuccessMessage);
        }

    }
}