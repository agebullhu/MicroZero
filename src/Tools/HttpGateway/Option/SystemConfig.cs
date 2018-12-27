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
        ///     是否加入ZeroNet
        /// </summary>
        [JsonProperty]
        public bool FireZero { get; set; }

        /// <summary>
        ///     是否检查返回值
        /// </summary>
        [JsonProperty]
        public bool CheckResult { get; set; }

        /// <summary>
        ///     是否开启API预检
        /// </summary>
        /// <remarks>
        ///     即检查API是否存在、是否需要令牌、是否可公开访问
        /// </remarks>
        [JsonProperty]
        public bool CheckApiItem { get; set; }


        /// <summary>
        ///     超时时间
        /// </summary>
        [JsonProperty]
        public int HttpTimeOut { get; set; }

        /// <summary>
        ///     触发警告的执行时间
        /// </summary>
        [JsonProperty]
        public int WaringTime { get; set; }

        /// <summary>
        ///     内容页地址
        /// </summary>
        [JsonProperty]
        public string ContextHost { get; set; }

        /// <summary>
        ///     内容页地址
        /// </summary>
        [JsonProperty]
        public string ContextAddr { get; set; }


        /// <summary>
        ///     内容页地址
        /// </summary>
        [JsonProperty]
        public string SiteFolder { get; set; }
        
    }
}