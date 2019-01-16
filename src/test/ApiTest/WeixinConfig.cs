using Newtonsoft.Json;
namespace ApiTest
{

    /// <summary>
    ///     系统配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class WeixinConfig
    {
        /// <summary>
        /// 公众号Token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 公众号消息加密Key
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string EncodingAESKey { get; set; }
        /// <summary>
        /// 公众号AppId
        /// </summary>
        public string WeixinAppId { get; set; }
        /// <summary>
        /// 公众号AppSecret
        /// </summary>
        public string WeixinAppSecret { get; set; }

    }
}