using System;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 内部服务调用上下文（跨系统边界传递）
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class RequestContext
    {
        /// <summary>
        /// 修改值
        /// </summary>
        /// <param name="globalId"></param>
        /// <param name="serviceKey"></param>
        /// <param name="requestId"></param>
        public void SetValue(string globalId, string serviceKey, string requestId)
        {
            CallGlobalId = globalId;
            _serviceKey = serviceKey;
            _requestId = requestId;
        }
        /// <summary>
        /// 构造
        /// </summary>
        public RequestContext() : this(ApiContext.MyServiceKey,Guid.NewGuid().ToString())
        {
        }


        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="globalId"></param>
        /// <param name="serviceKey"></param>
        /// <param name="requestId"></param>
        public RequestContext(string globalId, string serviceKey, string requestId) : this(serviceKey, requestId)
        {
            CallGlobalId = globalId;
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="serviceKey"></param>
        /// <param name="requestId"></param>
        public RequestContext(string serviceKey, string requestId)
        {
            _serviceKey = serviceKey;
            _requestId = requestId;
            Bear = "<error>";
        }

        /// <summary>
        /// 当前的全局标识(本地)
        /// </summary>
        [JsonProperty("lgi", DefaultValueHandling = DefaultValueHandling.Ignore,NullValueHandling = NullValueHandling.Ignore)]
        public string LocalGlobalId;

        /// <summary>
        /// 请求的全局标识(传递)
        /// </summary>
        [JsonProperty("cgi", DefaultValueHandling = DefaultValueHandling.Ignore,NullValueHandling = NullValueHandling.Ignore)]
        public String CallGlobalId { get; set; }

        /// <summary>
        /// 请求服务身份
        /// </summary>
        [JsonProperty("sk", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        private string _serviceKey;

        /// <summary>
        /// 请求服务身份
        /// </summary>
        [JsonIgnore]
        public string ServiceKey => _serviceKey;

        /// <summary>
        /// 全局请求标识（源头为用户请求）
        /// </summary>
        [JsonProperty("ri", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        private string _requestId;

        /// <summary>
        /// 全局请求标识（源头为用户请求）
        /// </summary>
        [JsonIgnore]
        public string RequestId => _requestId;

        /// <summary>
        /// 当前请求的Http Authorizaition
        /// </summary>
        [JsonProperty("ha", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Bear { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        [JsonProperty("at", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public ArgumentType ArgumentType { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        [JsonProperty("rt", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public RequestType RequestType { get; set; }

        /// <summary>
        /// 请求IP
        /// </summary>
        [JsonProperty("ip", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Ip { get; set; }

        /// <summary>
        /// 请求端口号
        /// </summary>
        [JsonProperty("pt", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Port { get; set; }

        /// <summary>
        /// HTTP的UserAgent
        /// </summary>
        [JsonProperty("ua", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string UserAgent { get; set; }

    }

    /// <summary>
    /// 参数类型
    /// </summary>
    public enum ArgumentType
    {
        /// <summary>
        /// HTTP的Form格式
        /// </summary>
        HttpForm,
        /// <summary>
        /// Json格式
        /// </summary>
        Json,
        /// <summary>
        /// Tson格式（二进制）
        /// </summary>
        Tson,
        /// <summary>
        /// 自定义
        /// </summary>
        Custom
    }
    /// <summary>
    /// 参数类型
    /// </summary>
    public enum RequestType
    {
        /// <summary>
        /// 无
        /// </summary>
        None,
        /// <summary>
        /// HTTP请求
        /// </summary>
        Http,
        /// <summary>
        /// ZeroNet请求
        /// </summary>
        Zero
    }
}