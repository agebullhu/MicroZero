using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     Api结构的信息
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class TypeDocument : DocumentItem
    {
        /// <summary>
        ///     类型
        /// </summary>
        [DataMember, JsonProperty("class", NullValueHandling = NullValueHandling.Ignore)]
        public string ClassName;

        /// <summary>
        ///     类型
        /// </summary>
        [DataMember, JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string TypeName;

        /// <summary>
        ///     类型
        /// </summary>
        [DataMember, JsonProperty("object", NullValueHandling = NullValueHandling.Ignore)]
        public ObjectType ObjectType;

        /// <summary>
        ///     枚举
        /// </summary>
        [DataMember, JsonProperty("enum", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsEnum;
        
        /// <summary>
        ///     类型
        /// </summary>
        [DataMember, JsonProperty("jsonName", NullValueHandling = NullValueHandling.Ignore)]
        public string JsonName;

        /// <summary>
        ///     字段
        /// </summary>
        [DataMember, JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, TypeDocument> fields;

        /// <summary>
        /// 能否为空
        /// </summary>
        [DataMember, JsonProperty("canNull", NullValueHandling = NullValueHandling.Ignore)]
        public bool CanNull { get; set; }
        /// <summary>
        /// 正则校验(文本)
        /// </summary>
        [DataMember, JsonProperty("regex", NullValueHandling = NullValueHandling.Ignore)]
        public string Regex { get; set; }
        /// <summary>
        /// 包含的最小时间
        /// </summary>
        [DataMember, JsonProperty("minDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? MinDate { get; set; }
        /// <summary>
        /// 包含的最大时间
        /// </summary>
        [DataMember, JsonProperty("maxDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? MaxDate { get; set; }
        /// <summary>
        /// 最小(包含的数值或文本长度)
        /// </summary>
        [DataMember, JsonProperty("min", NullValueHandling = NullValueHandling.Ignore)]
        public long? Min { get; set; }
        /// <summary>
        /// 最大(包含的数值或文本长度)
        /// </summary>
        [DataMember, JsonProperty("max", NullValueHandling = NullValueHandling.Ignore)]
        public long? Max { get; set; }
        /// <summary>
        ///     字段
        /// </summary>
        public Dictionary<string, TypeDocument> Fields => fields ?? (fields = new Dictionary<string, TypeDocument>());
    }
}