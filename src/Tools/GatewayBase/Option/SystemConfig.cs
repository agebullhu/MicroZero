using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MicroZero.Http.Gateway
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
        ///     是否加入MicroZero
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
        ///     开启管理命令
        /// </summary>
        [JsonProperty]
        public bool EnableInnerCommand { get; set; }


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
        ///     是否启用内容页
        /// </summary>
        [JsonProperty]
        public bool EnableContext { get; set; }

        /// <summary>
        ///     内容页地址
        /// </summary>
        [JsonProperty]
        public string ContextAddr { get; set; }


        /// <summary>
        ///     是否包含顶级目录(即域名后第一个节是目录而不是站点名称)
        /// </summary>
        [JsonProperty]
        public bool IsAppFolder { get; set; }
        
    }
}