using System.Runtime.Serialization;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// 报警节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract]
    public class WaringItem :NetData, IPublishData
    {
        /// <summary>
        /// API对象
        /// </summary>
        [DataMember, JsonProperty("host")]
        public string Host{ get; set; }
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

    /// <summary>
    /// 运行时警告
    /// </summary>
    public class RuntimeWaring : SignlePublisher<WaringItem>
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
                Machine = ZeroApplication.Config.StationName,
                User = ApiContext.Customer?.Account ?? "Unknow",
                RequestId = ApiContext.RequestContext.RequestId,
                Host = host,
                Api = api,
                Message = message
            });
        }
    }
}