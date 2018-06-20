using System.Collections.Generic;
using Agebull.ZeroNet.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Agebull.ZeroNet.PubSub
{
    /// <summary>
    ///     广播节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PublishItem
    {

        /// <summary>
        /// 是否非正常值
        /// </summary>
        [JsonProperty]
        public bool IsBadValue { get; set; }


        /// <summary>
        /// 带外事件
        /// </summary>
        [JsonIgnore]
        public ZeroNetEventType ZeroEvent { get; set; }


        /// <summary>
        /// 带外事件
        /// </summary>
        [JsonIgnore]
        public ZeroOperatorStateType State { get; set; }

        /// <summary>
        /// 全局ID
        /// </summary>
        [JsonProperty]
        public long GlobalId { get; set; }

        /// <summary>
        /// 请求ID
        /// </summary>
        [JsonProperty]
        public string RequestId { get; set; }

        /// <summary>
        ///  发布者的Identity(可能已消失)
        /// </summary>
        [JsonProperty]
        public string Publisher { get; set; }


        /// <summary>
        ///  站点
        /// </summary>
        [JsonProperty]
        public string Station { get; set; }

        /// <summary>
        ///     主标题
        /// </summary>
        [JsonProperty]
        public string Title { get; set; }

        /// <summary>
        ///     副标题
        /// </summary>
        [JsonProperty]
        public string SubTitle { get; set; }

        /// <summary>
        ///     内容
        /// </summary>
        [JsonProperty]
        public string Content { get; set; }

        /// <summary>
        ///     内容
        /// </summary>
        [JsonProperty("Values")] private List<string> _values;

        /// <summary>
        ///     内容
        /// </summary>
        [JsonIgnore]
        public List<string> Values => _values ?? (_values = new List<string>());
    }
}