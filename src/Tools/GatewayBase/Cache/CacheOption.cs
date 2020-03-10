using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MicroZero.Http.Gateway
{

    /// <summary>
    ///     缓存设置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CacheOption
    {
        /// <summary>
        ///     缓存API设置
        /// </summary>
        [JsonProperty("api")]
        public List<ApiCacheOption> Api { get; set; }


        /// <summary>
        ///     刷新触发API配置
        /// </summary>
        [JsonProperty("trigger")]
        public List<CacheFlushOption> Trigger { get; set; }

        /// <summary>
        ///     初始化
        /// </summary>
        public void Initialize()
        {
            Api?.ForEach(p => p.Initialize());
        }
    }


    /// <summary>
    ///     缓存设置
    /// </summary>
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiCacheOption
    {
        /// <summary>
        ///     API名称
        /// </summary>
        [DataMember]
        [JsonProperty]
        public string Api { get; set; }


        /// <summary>
        ///     用于校验的身份头
        /// </summary>
        [DataMember]
        [JsonProperty]
        public List<string> Keys { get; set; }

        /// <summary>
        ///     用于校验的身份头
        /// </summary>
        [DataMember]
        [JsonProperty]
        public bool Bear { get; set; }

        /// <summary>
        ///     缓存更新的秒数
        /// </summary>
        [DataMember]
        [JsonProperty]
        public int FlushSecond { get; set; }

        /// <summary>
        ///     缓存时仅使用名称（否则包含查询字符串）
        /// </summary>
        [DataMember]
        [JsonProperty]
        public bool OnlyName { get; set; }

        /// <summary>
        ///     发生网络错误时缓存
        /// </summary>
        [DataMember]
        [JsonProperty]
        public bool ByNetError { get; set; }

        /// <summary>
        ///     缓存特征
        /// </summary>
        [IgnoreDataMember]
        [JsonIgnore] public CacheFeature Feature { get; private set; }

        /// <summary>
        ///     初始化
        /// </summary>
        public void Initialize()
        {
            //默认5分钟
            if (FlushSecond <= 0)
                FlushSecond = 300;
            else if (FlushSecond > 3600)
                FlushSecond = 3600;
            if (Keys != null && Keys.Count > 0)
            {
                Feature = CacheFeature.Keys;
                return;
            }
            if (Bear)
                Feature |= CacheFeature.Bear;
            if (!OnlyName)
                Feature |= CacheFeature.QueryString;
            if (ByNetError)
                Feature |= CacheFeature.NetError;
        }
    }


    /// <summary>
    ///     缓存触发刷新设置
    /// </summary>
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class CacheFlushOption
    {
        /// <summary>
        ///     触发更新的API名称
        /// </summary>
        [DataMember]
        [JsonProperty]
        public string TriggerApi { get; set; }

        /// <summary>
        ///     需要缓存的API名称
        /// </summary>
        [DataMember]
        [JsonProperty]
        public string CacheApi { get; set; }

        /// <summary>
        ///  字段映射
        /// </summary>
        [DataMember]
        [JsonProperty]
        public Dictionary<string,string> Map { get; set; }

    }
}