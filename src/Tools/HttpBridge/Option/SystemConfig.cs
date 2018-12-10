using System;
using System.Runtime.Serialization;
using Agebull.Common.Logging;
using Newtonsoft.Json;

namespace ZeroNet.Http.Gateway
{
    /// <summary>
    ///     系统配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    [Serializable]
    public class SystemConfig
    {
        /// <summary>
        /// 公众号Token
        /// </summary>
        public virtual string Token { get; set; }
        /// <summary>
        /// 公众号消息加密Key
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public virtual string EncodingAESKey { get; set; }
        /// <summary>
        /// 公众号AppId
        /// </summary>
        public virtual string WeixinAppId { get; set; }
        /// <summary>
        /// 公众号AppSecret
        /// </summary>
        public virtual string WeixinAppSecret { get; set; }


    }
}