using System.Collections.Generic;
using Newtonsoft.Json;

namespace MicroZero.Http.Gateway
{
    /// <summary>
    /// ASH的URL与接口映射
    /// </summary>
    public class AshxMapConfig
    {
        /// <summary>
        /// action参数名称
        /// </summary>
        [JsonProperty("action")]
        public string ActionName { get; set; }

        /// <summary>
        /// 模块集合
        /// </summary>
        [JsonProperty("models")]
        public List<MapModelConfig> Models { get; set; }
    }
}