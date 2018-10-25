using System;
using Newtonsoft.Json;

namespace Agebull.Common.WebApi
{
    /// <summary>
    ///     实体信息
    /// </summary>
    [JsonObject]
    public class EntityInfo
    {
        /// <summary>
        /// 类型标识
        /// </summary>
        [JsonProperty("entityType")] public int EntityType { get; set; }
        /// <summary>
        /// 页面标识
        /// </summary>
        [JsonProperty("pageId")] public long PageId { get; set; }
    }

    public class HttpPostAttribute : Attribute
    {

    }
    public class HttpGetAttribute : Attribute
    {

    }
}