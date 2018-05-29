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
            ClientMessage = messgae;
        }
        /// <summary>
        /// 错误码
        /// </summary>
        [JsonProperty("errorCode", NullValueHandling = NullValueHandling.Ignore)]
        public int ErrorCode { get; set; }

        /// <summary>
        /// 对应HTTP错误码（参考）
        /// </summary>
        [JsonProperty("httpCode", NullValueHandling = NullValueHandling.Ignore)]
        public string HttpCode { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        [JsonProperty("msg", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientMessage { get; set; }
        /// <summary>
        /// 内部提示信息
        /// </summary>
        [JsonProperty("info", NullValueHandling = NullValueHandling.Ignore)]
        public string InnerMessage { get; set; }
    }
}