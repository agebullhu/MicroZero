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
        /// <remarks>
        /// 参见 ErrorCode 说明
        /// </remarks>
        /// <example>-1</example>
        [JsonProperty("code")]
        public int ErrorCode { get; set; }

        /// <summary>
        /// 对应HTTP错误码（参考）
        /// </summary>
        /// <example>404</example>
        [JsonProperty("http")]
        public string HttpCode { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        /// <example>你的数据不正确</example>
        [JsonProperty("msg")]
        public string ClientMessage { get; set; }
        /// <summary>
        /// 内部提示信息
        /// </summary>
        public string InnerMessage { get; set; }
    }
}