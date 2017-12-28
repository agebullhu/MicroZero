using System;
using Newtonsoft.Json;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    /// RPC配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class RpcConfig
    {
        /// <summary>
        /// 请求地址
        /// </summary>
        [JsonProperty("requestUrl")]
        public string RequestUrl { get; set; }
        /// <summary>
        /// 回复地址
        /// </summary>
        [JsonProperty("answerUrl")]
        public string AnswerUrl { get; set; }

        /// <summary>
        /// 心跳地址
        /// </summary>
        [JsonProperty("heartbeatUrl")]
        public string HeartbeatUrl { get; set; }
       
        /// <summary>
        /// 通知地址
        /// </summary>
        [JsonProperty("notifyUrl")]
        public string NotifyUrl { get; set; }

        /// <summary>
        /// 本地服务在全网中的标识
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; }

    }
}