using System;
using System.Runtime.Serialization;
using Agebull.Common.Logging;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 系统配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class SystemConfig
    {
        /// <summary>
        /// 是否加入ZeroNet
        /// </summary>
        [JsonProperty]
        public bool FireZero { get; set; }
        /// <summary>
        /// 当前服务器Key
        /// </summary>
        [JsonProperty]
        public string ServiceKey { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        [JsonProperty]
        public int HttpTimeOut { get; set; }
        /// <summary>
        /// 触发警告的执行时间
        /// </summary>
        [JsonProperty]
        public int WaringTime { get; set; }

        /// <summary>
        /// 内容页地址
        /// </summary>
        [JsonProperty]
        public string ContextHost { get; set; }

        /// <summary>
        /// 记录跟踪日志
        /// </summary>
        [JsonProperty]
        public bool LogMonitor { get => LogRecorder.LogMonitor; set => LogRecorder.LogMonitor = value; }

        /// <summary>
        /// 记录跟踪日志
        /// </summary>
        [JsonProperty]
        public string LogPath { get => TxtRecorder.LogPath; set => TxtRecorder.LogPath = value; }

    }
}