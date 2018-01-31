using Newtonsoft.Json;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// API状态返回接口实现
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiStatsResult : IApiStatusResult
    {
        /// <summary>
        /// 默认构造
        /// </summary>
        public ApiStatsResult()
        {

        }
        /// <summary>
        /// 默认构造
        /// </summary>
        public ApiStatsResult(int code, string messgae)
        {
            ErrorCode = code;
            Message = messgae;
        }
        /// <summary>
        /// 错误码
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int ErrorCode { get; set; }

        /// <summary>
        /// 对应HTTP错误码（参考）
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string HttpCode { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
        /// <summary>
        /// 内部提示信息
        /// </summary>
#if DEBUG
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#else
        [JsonIgnore]
#endif
        public string InnerMessage { get; set; }
    }
}