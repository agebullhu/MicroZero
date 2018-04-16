using Newtonsoft.Json;

namespace Agebull.ZeroNet.PubSub
{
    /// <summary>
    ///     广播节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PublishItem
    {
        /// <summary>
        /// 请求ID
        /// </summary>
        [JsonProperty]
        public string RequetId { get; set; }

        /// <summary>
        ///     分类（即站点）
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
    }
}