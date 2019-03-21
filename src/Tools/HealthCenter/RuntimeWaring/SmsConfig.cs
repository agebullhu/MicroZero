using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MicroZero.Http.Route
{
    /// <summary>
    ///     运维短信配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class SmsConfig
    {

        /// <summary>
        ///     每个重置周期小时数
        /// </summary>
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int CycleHours;

        /// <summary>
        ///     每个重置周期最多发送次数
        /// </summary>
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int CycleSendCount;

        /// <summary>
        ///     接收的电话号码
        /// </summary>
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Phones;

        /// <summary>
        ///     阿里的accessKeyId
        /// </summary>
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AliAccessKeyId;

        /// <summary>
        ///     阿里的accessKeySecret
        /// </summary>
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AliAccessKeySecret;

        /// <summary>
        ///     短信API产品域名
        /// </summary>
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AliDomain;

        /// <summary>
        ///     节点
        /// </summary>
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AliEndPointName;

        /// <summary>
        ///     短信API产品名称
        /// </summary>
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AliProduct;

        /// <summary>
        ///     区域
        /// </summary>
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AliRegionId;

        /// <summary>
        ///     签名
        /// </summary>
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AliSignName;

        /// <summary>
        ///     模板名称
        /// </summary>
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AliTemplateCode;
    }
}