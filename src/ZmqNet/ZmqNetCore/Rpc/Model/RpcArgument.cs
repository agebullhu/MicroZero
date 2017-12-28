
using System;
using Newtonsoft.Json;
using Yizuan.Service.Api;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    /// 命令调用参数
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class RpcArgument
    {
        /// <summary>
        /// 请求者标识（如果需要回发，则不能为空，因为这是回发接收者的全局标识）
        /// </summary>
        [JsonIgnore]
        public string ServiceKey => ApiContext.RequestContext.ServiceKey;

        /// <summary>
        /// 请求标识（不可为空）
        /// </summary>
        [JsonIgnore]
        public Guid RequestId => ApiContext.RequestContext.RequestId;

        /// <summary>
        /// 当前原始参数
        /// </summary>
        [JsonIgnore]
        public CommandArgument Command { get; set; }

        /// <summary>
        /// 命令参数标识，为0表示无参数，其实应使用RpcArgument
        /// </summary>
        [JsonProperty("ArgumentTypeId", NullValueHandling = NullValueHandling.Ignore)]
        public int ArgumentTypeId { get; set; }

    }

    /// <summary>
    /// 命令调用参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [JsonObject(MemberSerialization.OptIn)]
    public class RpcArgument<T> : RpcArgument
    {
        /// <summary>
        /// 参数
        /// </summary>
        [JsonProperty("Argument", NullValueHandling = NullValueHandling.Ignore)]
        public T Argument { get; set; }
    }
    
}
