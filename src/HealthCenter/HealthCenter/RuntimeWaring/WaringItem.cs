using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 报警节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract]
    public class WaringItem
    {
        /// <summary>
        /// API对象
        /// </summary>
        [DataMember, JsonProperty("host")] public string Host;
        [DataMember, JsonProperty("api")]
        public string Api;

        /// <summary>
        /// 消息
        /// </summary>
        [DataMember, JsonProperty("message")]
        public string Message;
    }
}