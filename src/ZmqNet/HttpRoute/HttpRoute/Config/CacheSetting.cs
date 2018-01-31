using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 缓存特征
    /// </summary>
    [Flags]
    internal enum CacheFeature : uint
    {
        /// <summary>
        /// 无
        /// </summary>
        None,

        /// <summary>
        /// 身份头相同
        /// </summary>
        Bear = 0x1,

        /// <summary>
        /// 发生网络错误时
        /// </summary>
        NetError = 0x2,

        /// <summary>
        /// 参数相同
        /// </summary>
        QueryString = 0x4,

    }
    /// <summary>
    /// 缓存设置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract]
    internal class CacheSetting
    {
        /// <summary>
        /// API名称
        /// </summary>
        [DataMember, JsonProperty]
        public string Api { get; set; }

        /// <summary>
        /// 用于校验的身份头
        /// </summary>
        [DataMember, JsonProperty]
        public string Bear { get; set; }

        /// <summary>
        /// 缓存的秒数
        /// </summary>
        [DataMember, JsonProperty]
        public int FlushSecond { get; set; }

        /// <summary>
        /// 缓存时仅使用名称（否则包含查询字符串）
        /// </summary>
        [DataMember, JsonProperty]
        public bool OnlyName { get; set; }

        /// <summary>
        /// 发生网络错误时缓存
        /// </summary>
        [DataMember, JsonProperty]
        public bool ByNetError { get; set; }

        /// <summary>
        /// 缓存特征
        /// </summary>
        [IgnoreDataMember , JsonIgnore]
        public CacheFeature Feature { get; private set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            //默认5分钟
            if (FlushSecond <= 0)
                FlushSecond = 300;
            else if (FlushSecond > 3600)
                FlushSecond = 3600;
            if (!string.IsNullOrWhiteSpace(Bear))
                Feature |= CacheFeature.Bear;
            if(!OnlyName)
                Feature |= CacheFeature.QueryString;
            if(ByNetError)
                Feature |= CacheFeature.NetError;
        }
    }
    /// <summary>
    /// 缓存数据
    /// </summary>
    internal class CacheData
    {
        /// <summary>
        /// 下次更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 缓存内容
        /// </summary>
        public string Content { get; set; }
    }
}