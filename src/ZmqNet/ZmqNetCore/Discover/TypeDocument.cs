using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    /// 对象类型
    /// </summary>
    public enum ObjectType
    {
        /// <summary>
        /// 基本类型
        /// </summary>
        Base,
        /// <summary>
        /// 数组
        /// </summary>
        Array,
        /// <summary>
        /// 字典
        /// </summary>
        Dictionary, 
        /// <summary>
        /// 对象
        /// </summary>
        Object
    }
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
        ///     字段
        /// </summary>
        public Dictionary<string, TypeDocument> Fields => fields ?? (fields = new Dictionary<string, TypeDocument>());
    }
}