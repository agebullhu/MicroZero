using System.Collections.Generic;
using System.Runtime.Serialization;
using Agebull.EntityModel.Common;
using Newtonsoft.Json;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    /// URL与接口映射
    /// </summary>
    public class MapModelConfig
    {
        /// <summary>
        /// 站点映射
        /// </summary>
        [JsonProperty("station")]
        public string Station { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        [JsonProperty("paths")]
        public List<MapItem> Paths { get; set; }

    }
    /// <summary>名称内容对象</summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class MapItem
    {
        /// <summary>名称</summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>值</summary>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}