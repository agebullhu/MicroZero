using Newtonsoft.Json;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// API返回数据泛型类
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiValueResult : ApiResult
    {
        /// <summary>
        /// 返回值
        /// </summary>
        [JsonProperty("data")]
        public string ResultData { get; set; }

        /// <summary>
        /// 生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static ApiValueResult Succees(string data)
        {
            return new ApiValueResult
            {
                Success = true,
                ResultData = data
            };
        }
        /// <summary>
        /// 生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        public static ApiValueResult ErrorResult(int errCode)
        {
            return new ApiValueResult
            {
                Success = false,
                Status = new ApiStatsResult
                {
                    ErrorCode = errCode,
                    ClientMessage = ErrorCode.GetMessage(errCode)
                }
            };
        }
        /// <summary>
        /// 生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiValueResult ErrorResult(int errCode, string message)
        {
            return new ApiValueResult
            {
                Success = false,
                Status = new ApiStatsResult
                {
                    ErrorCode = errCode,
                    ClientMessage = message
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
        public static ApiValueResult ErrorResult(int errCode, string message, string innerMessage)
        {
            return new ApiValueResult
            {
                Success = false,
                Status = new ApiStatsResult
                {
                    ErrorCode = errCode,
                    ClientMessage = message,
                    InnerMessage = innerMessage
                }
            };
        }
    }


    /// <summary>
    /// API返回单数据数据泛型类
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiValueResult<TData> : ApiResult
    {
        /// <summary>
        /// 返回值
        /// </summary>
        [JsonProperty("data")]
        public TData ResultData { get; set; }

        /// <summary>
        /// 生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static ApiValueResult<TData> Succees(TData data)
        {
            return new ApiValueResult<TData>
            {
                Success = true,
                ResultData = data
            };
        }
        /// <summary>
        /// 生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <returns></returns>
        public static ApiValueResult<TData> ErrorResult(int errCode)
        {
            return new ApiValueResult<TData>
            {
                Success = false,
                Status = new ApiStatsResult
                {
                    ErrorCode = errCode,
                    ClientMessage = ErrorCode.GetMessage(errCode)
                }
            };
        }
        /// <summary>
        /// 生成一个包含错误码的标准返回
        /// </summary>
        /// <param name="errCode">错误码</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ApiValueResult<TData> ErrorResult(int errCode, string message)
        {
            return new ApiValueResult<TData>
            {
                Success = false,
                Status = new ApiStatsResult
                {
                    ErrorCode = errCode,
                    ClientMessage = message
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
        public static ApiValueResult<TData> ErrorResult(int errCode, string message, string innerMessage)
        {
            return new ApiValueResult<TData>
            {
                Success = false,
                Status = new ApiStatsResult
                {
                    ErrorCode = errCode,
                    ClientMessage = message,
                    InnerMessage = innerMessage
                }
            };
        }
    }
}