using System.Runtime.Serialization;
using Agebull.ZeroNet.PubSub;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 报警节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract]
    public class WaringItem : NetData, IPublishData
    {
        /// <summary>
        /// API对象
        /// </summary>
        [DataMember, JsonProperty("host")]
        public string Host { get; set; }
        /// <summary>
        /// API对象
        /// </summary>
        [DataMember, JsonProperty("api")]
        public string Api { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        [DataMember, JsonProperty("message")]
        public string Message { get; set; }

        string IPublishData.Title => "RuntimeWaring";
    }
}