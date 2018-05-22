using System.Runtime.Serialization;
using Agebull.ZeroNet.PubSub;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 报警节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract]
    public class WaringItem : IPublishData
    {
        /// <summary>
        /// API对象
        /// </summary>
        [DataMember, JsonProperty("host")]
        public string Host;
        [DataMember, JsonProperty("api")]
        public string Api;

        /// <summary>
        /// 消息
        /// </summary>
        [DataMember, JsonProperty("message")]
        public string Message;

        string IPublishData.Title => "RuntimeWaring";
    }
    /// <summary>
    /// 运行时警告
    /// </summary>
    public class RuntimeWaring : Publisher<WaringItem>
    {
        private static readonly RuntimeWaring Instance = new RuntimeWaring
        {
            Name = "RuntimeWaring",
            StationName = "HealthCenter"
        };

        /// <summary>
        /// 运行时警告
        /// </summary>
        /// <param name="host"></param>
        /// <param name="api"></param>
        /// <param name="message"></param>
        public static void Waring(string host, string api, string message)
        {
            Instance.Publish(new WaringItem
            {
                Host = host,
                Api = api,
                Message = message
            });
        }
    }

}