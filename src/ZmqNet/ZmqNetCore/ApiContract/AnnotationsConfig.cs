using System.Runtime.Serialization;
using Newtonsoft.Json;

// ReSharper disable All


namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     自注释配置对象
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class AnnotationsConfig
    {
        /// <summary>
        ///     名称
        /// </summary>
        [DataMember, JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        private string _name;

        /// <summary>
        ///     名称
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        ///     标题
        /// </summary>
        [DataMember, JsonProperty("caption", NullValueHandling = NullValueHandling.Ignore)]
        protected string _caption;

        /// <summary>
        ///     标题
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string Caption
        {
            get => _caption;
            set => _caption = value;
        }

        /// <summary>
        ///     说明
        /// </summary>
        [DataMember, JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        protected string _description;

        /// <summary>
        ///     说明
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string Description
        {
            get => _description;
            set => _description = value;
        }
        /// <summary>
        /// 显示文本
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name}({Caption})";
        }
    }
}
