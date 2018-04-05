using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Agebull.Common.Logging;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 系统配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    internal class SystemConfig
    {
        /// <summary>
        /// 是否加入ZeroNet
        /// </summary>
        [JsonProperty]
        internal bool FireZero { get; set; }
        /// <summary>
        /// 当前服务器Key
        /// </summary>
        [JsonProperty]
        internal string ServiceKey { get; set; }

        /// <summary>
        /// 黑洞地址
        /// </summary>
        [JsonProperty]
        internal string BlockHost { get; set; }
        /// <summary>
        /// 超时时间
        /// </summary>
        [JsonProperty]
        internal int HttpTimeOut { get; set; }
        /// <summary>
        /// 触发警告的执行时间
        /// </summary>
        [JsonProperty]
        internal int WaringTime { get; set; }

        /// <summary>
        /// 内容页地址
        /// </summary>
        [JsonProperty]
        internal string ContextHost { get; set; }

        /// <summary>
        /// 记录跟踪日志
        /// </summary>
        [JsonProperty]
        internal bool LogMonitor { get; set; }

        /// <summary>
        /// 记录跟踪日志
        /// </summary>
        [JsonProperty]
        internal string LogPath { get => TxtRecorder.LogPath; set => TxtRecorder.LogPath = value; }

        /// <summary>
        /// 是否检查Auth头
        /// </summary>
        [JsonProperty]
        internal bool CheckBearerCache { get; set; }

        /// <summary>
        /// 黑名单令牌
        /// </summary>
        [JsonProperty]
        internal Dictionary<string, string> DenyTokens { get; set; }

        /// <summary>
        /// 禁止的Http头信息
        /// </summary>
        [JsonProperty]
        internal List<DenyItem> DenyHttpHeaders { get; set; }

        /// <summary>
        /// 需要检查的Api
        /// </summary>
        [JsonProperty]
        internal Dictionary<string, ApiItem> CheckApis { get; set; }
    }

    /// <summary>
    /// Api节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class ApiItem
    {
        /// <summary>
        /// 头
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }
        /// <summary>
        /// 操作系统
        /// </summary>
        [JsonProperty]
        public string Os { get; set; }
        /// <summary>
        /// 浏览器
        /// </summary>
        [JsonProperty]
        public string Browser { get; set; }
    }

    /// <summary>
    /// 阻止节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class DenyItem
    {
        /// <summary>
        /// 头
        /// </summary>
        [JsonProperty]
        public string Head { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        [JsonProperty]
        public string Value { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        [JsonProperty]
        public DenyType DenyType { get; set; }
    }
    
    /// <summary>
    /// 阻止类型
    /// </summary>
    public enum DenyType
    {
        /// <summary>
        /// 不阻止
        /// </summary>
        None,
        /// <summary>
        /// 有此内容
        /// </summary>
        Hase,
        /// <summary>
        /// 没有此内容
        /// </summary>
        NonHase,
        /// <summary>
        /// 达到数组数量
        /// </summary>
        Count,
        /// <summary>
        /// 内容等于
        /// </summary>
        Equals,
        /// <summary>
        /// 内容包含
        /// </summary>
        Like,
        /// <summary>
        /// 正则匹配
        /// </summary>
        Regex
    }
}