using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Agebull.MicroZero;
using Newtonsoft.Json;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     安全相关的配置
    /// </summary>
    public class SecurityConfig
    {
        /// <summary>
        ///     需要检查的Api
        /// </summary>
        [JsonProperty("checkApis")] public Dictionary<string, ApiItem> checkApis;

        /// <summary>
        ///     禁止的Http头信息
        /// </summary>
        [JsonProperty("denyHttpHeaders")] public List<DenyItem> denyHttpHeaders;

        /// <summary>
        ///     启用验签
        /// </summary>
        [JsonProperty("fireSign")]
        public bool FireSign { get; set; }

        /// <summary>
        ///     启用验签
        /// </summary>
        [JsonProperty("auth2")]
        public bool Auth2 { get; set; }
        
        /// <summary>
        ///     黑洞地址
        /// </summary>
        [JsonProperty("blockAddress")]
        public string BlockHost { get; set; }

        /// <summary>
        ///     是否检查Auth头
        /// </summary>
        [JsonProperty("fireBearer")]
        public bool CheckBearer { get; set; }

        /// <summary>
        ///     是否检查Auth头
        /// </summary>
        [JsonProperty("authStation")]
        public string AuthStation { get; set; }

        /// <summary>
        ///     DeviceId检查API
        /// </summary>
        [JsonProperty("didApi")]
        public string DeviceIdCheckApi { get; set; }
        /// <summary>
        ///     AT检查API
        /// </summary>
        [JsonProperty("atApi")]
        public string AccessTokenCheckApi { get; set; }
        /// <summary>
        ///     AT检查API
        /// </summary>
        [JsonProperty("tokenApi")]
        public string TokenCheckApi { get; set; }

        /// <summary>
        ///     黑名单令牌
        /// </summary>
        [JsonProperty("denyTokens")]
        public List<string> denyTokens { get; set; }

        /// <summary>
        ///     黑名单令牌
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> DenyTokens { get; set; }

        /// <summary>
        ///     禁止的Http头信息
        /// </summary>
        [JsonProperty]
        public List<DenyItem> DenyHttpHeaders => denyHttpHeaders ?? (denyHttpHeaders = new List<DenyItem>());

    }


    /// <summary>
    ///     阻止类型
    /// </summary>
    public enum DenyType
    {
        /// <summary>
        ///     不阻止
        /// </summary>
        None,

        /// <summary>
        ///     有此内容
        /// </summary>
        Hase,

        /// <summary>
        ///     没有此内容
        /// </summary>
        NonHase,

        /// <summary>
        ///     达到数组数量
        /// </summary>
        Count,

        /// <summary>
        ///     内容等于
        /// </summary>
        Equals,

        /// <summary>
        ///     内容包含
        /// </summary>
        Like,

        /// <summary>
        ///     正则匹配
        /// </summary>
        Regex
    }

    /// <summary>
    ///     Api节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    [Serializable]
    public class ApiItem
    {
        /// <summary>
        ///     头
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }

        /// <summary>
        ///     是否允许不携带OAuth2.0的令牌
        /// </summary>
        public bool NoBearer => Access < ApiAccessOption.Anymouse;

        /// <summary>
        ///     需要登录
        /// </summary>
        public bool NeedLogin => (((int)Access) & 0xFFF0) > 0;

        /// <summary>
        /// 访问权限
        /// </summary>
        [JsonProperty]
        public ApiAccessOption Access { get; set; }

        /// <summary>
        ///     操作系统
        /// </summary>
        [JsonProperty]
        public string Os { get; set; }

        /// <summary>
        ///     浏览器
        /// </summary>
        [JsonProperty]
        public string App { get; set; }
    }

    /// <summary>
    ///     阻止节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    [Serializable]
    public class DenyItem
    {
        /// <summary>
        ///     头
        /// </summary>
        [JsonProperty]
        public string Head { get; set; }

        /// <summary>
        ///     内容
        /// </summary>
        [JsonProperty]
        public string Value { get; set; }

        /// <summary>
        ///     类型
        /// </summary>
        [JsonProperty]
        public DenyType DenyType { get; set; }
    }
}