using Newtonsoft.Json;

namespace Agebull.ZeroNet.ZeroApi
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
        [JsonProperty("success")]
        public bool Success { get; set; } = true;



        /// <summary>
        /// API执行状态（为空表示状态正常）
        /// </summary>
        [JsonIgnore]
        IApiStatusResult IApiResult.Status
        {
            get => Status;
            set => Status = value as ApiStatsResult;
        }

        /// <summary>
        /// API执行状态（为空表示状态正常）
        /// </summary>
        [JsonProperty("status")]
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
        public static ApiResult Error(int errCode, string message)
        {
            return new ApiResult
            {
                Success = errCode == ErrorCode.Success,
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
        public static ApiResult Error(int errCode, string message, string innerMessage)
        {
            return new ApiResult
            {
                Success = errCode == ErrorCode.Success,
                Status = new ApiStatsResult
                {
                    ErrorCode = errCode,
                    ClientMessage = message,
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
                Success = true
            };
        }

        /// <summary>
        ///     页面不存在的Json字符串
        /// </summary>
        public static readonly string NoFindJson = JsonConvert.SerializeObject(Error(ErrorCode.NoFind, "*页面不存在*"));

        /// <summary>
        ///     参数错误字符串
        /// </summary>
        public static readonly string ArgumentErrorJson = JsonConvert.SerializeObject(Error(ErrorCode.LogicalError, "参数错误"));

        /// <summary>
        ///     逻辑错误字符串
        /// </summary>
        public static readonly string LogicalErrorJson = JsonConvert.SerializeObject(Error(ErrorCode.LogicalError));

        /// <summary>
        ///     拒绝访问的Json字符串
        /// </summary>
        public static readonly string DenyAccessJson = JsonConvert.SerializeObject(Error(ErrorCode.DenyAccess));

        /// <summary>
        ///     服务器无返回值的字符串
        /// </summary>
        public static readonly string RemoteEmptyErrorJson = JsonConvert.SerializeObject(Error(ErrorCode.RemoteError, "*服务器无返回值*"));

        /// <summary>
        ///     服务器访问异常
        /// </summary>
        public static readonly string NetworkErrorJson = JsonConvert.SerializeObject(Error(ErrorCode.NetworkError));

        /// <summary>
        ///     服务器访问异常
        /// </summary>
        public static readonly string LocalErrorJson = JsonConvert.SerializeObject(Error(ErrorCode.LocalError));

        /// <summary>
        ///     服务器访问异常
        /// </summary>
        public static readonly string LocalExceptionJson = JsonConvert.SerializeObject(Error(ErrorCode.LocalException));
        /// <summary>
        ///     系统未就绪
        /// </summary>
        public static readonly string NoReadyJson = JsonConvert.SerializeObject(Error(ErrorCode.NoReady));

        /// <summary>
        /// 参数错误的Json文本
        /// </summary>
        /// <remarks>参数校验不通过</remarks>
        public static readonly string SucceesJson = JsonConvert.SerializeObject(Succees());

        /// <summary>
        /// 未知错误的Json文本
        /// </summary>
        public static readonly string UnknowErrorJson = JsonConvert.SerializeObject(Error(ErrorCode.LocalError, "未知错误"));

        /// <summary>
        /// 网络超时的Json文本
        /// </summary>
        /// <remarks>调用其它Api时时抛出未处理异常</remarks>
        public static readonly string TimeOutJson = JsonConvert.SerializeObject(Error(ErrorCode.NetworkError));

        /// <summary>
        /// 内部错误的Json文本
        /// </summary>
        /// <remarks>执行方法时抛出未处理异常</remarks>
        public static readonly string InnerErrorJson = JsonConvert.SerializeObject(Error(ErrorCode.LocalError));

        /// <summary>
        /// 服务不可用的Json文本
        /// </summary>
        public static readonly string UnavailableJson = JsonConvert.SerializeObject(Error(ErrorCode.Unavailable, "服务不可用"));
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
        [JsonProperty("data")]
        public TData ResultData { get; set; }

        /// <summary>
        /// 生成一个成功的标准返回
        /// </summary>
        /// <returns></returns>
        public static ApiResult<TData> Succees(TData data)
        {
            return new ApiResult<TData>
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
        public static ApiResult<TData> ErrorResult(int errCode)
        {
            return new ApiResult<TData>
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
        public static ApiResult<TData> ErrorResult(int errCode, string message)
        {
            return new ApiResult<TData>
            {
                Success = false,
                Status = new ApiStatsResult
                {
                    ErrorCode = errCode,
                    ClientMessage = message
                }
            };
        }
    }

}
