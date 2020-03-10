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
        ///     是否测试环境
        /// </summary>
        [JsonProperty]
        public bool IsTest { get; set; }


        /// <summary>
        ///     关闭文件参数(防止异常流量攻击)
        /// </summary>
        [JsonProperty]
        public bool CloseFileArgument { get; set; }


        /// <summary>
        ///     返回的上下文类型
        /// </summary>
        [JsonProperty]
        public string ContentType { get; set; }

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
        /// 开启链路跟踪
        /// </summary>
        [JsonProperty]
        public bool EnableLinkTrace { get; set; }

        /// <summary>
        /// 开启API统计
        /// </summary>
        [JsonProperty]
        public bool EnableApiCollect { get; set; }
    }
}