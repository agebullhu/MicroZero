using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yizuan.Service.Api
{
    /// <summary>
    /// API返回基类
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiResult : IApiResult
    {
        /// <summary>
        /// 成功或失败标记
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Result { get; set; }

        /// <summary>
        /// API执行状态（为空表示状态正常）
        /// </summary>
        [JsonIgnore]
        IApiStatusResult IApiResult.Status => Status;

        /// <summary>
        /// API执行状态（为空表示状态正常）
        /// </summary>
        [JsonProperty("ResponseStatus", NullValueHandling = NullValueHandling.Ignore)]
        public ApiStatsResult Status { get; set; }

        /// <summary>
        /// 生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        public static ApiResult Error(int errCode)
        {
            return new ApiResult
            {
                Result = false,
                Status = new ApiStatsResult
                {
                    ErrorCode = errCode,
                    Message = ErrorCode.GetMessage(errCode)
                }
            };
        }

        /// <summary>
        /// 生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult Error(int errCode, string message)
        {
            return new ApiResult
            {
                Result = false,
                Status = new ApiStatsResult
                {
                    ErrorCode = errCode,
                    Message = message
                }
            };
        }
        /// <summary>
        /// 生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message"></param>
        /// <param name="innerMessage"></param>
        /// <returns></returns>
        public static ApiResult Error(int errCode, string message, string innerMessage)
        {
            return new ApiResult
            {
                Result = false,
                Status = new ApiStatsResult
                {
                    ErrorCode = errCode,
                    Message = message,
                    InnerMessage = innerMessage
                }
            };
        }
        /// <summary>
        /// 生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static ApiResult Succees()
        {
            return new ApiResult
            {
                Result = true
            };
        }
    }


    /// <summary>
    /// API返回数据泛型类
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiResult<TData> : ApiResult, IApiResult<TData>
        where TData : IApiResultData
    {
        /// <summary>
        /// 返回值
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TData ResultData { get; set; }

        /// <summary>
        /// 生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static ApiResult<TData> Succees(TData data)
        {
            return new ApiResult<TData>
            {
                Result = true,
                ResultData = data
            };
        }
        /// <summary>
        /// 生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        public static ApiResult<TData> ErrorResult(int errCode)
        {
            return new ApiResult<TData>
            {
                Result = false,
                Status = new ApiStatsResult
                {
                    ErrorCode = errCode,
                    Message = ErrorCode.GetMessage(errCode)
                }
            };
        }
        /// <summary>
        /// 生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiResult<TData> ErrorResult(int errCode, string message)
        {
            return new ApiResult<TData>
            {
                Result = false,
                Status = new ApiStatsResult
                {
                    ErrorCode = errCode,
                    Message = message
                }
            };
        }
    }

}
