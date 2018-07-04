using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     Api方法的信息
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class ApiDocument : DocumentItem
    {
        /// <summary>
        ///     Api路由名称
        /// </summary>
        [DataMember, JsonProperty("route", NullValueHandling = NullValueHandling.Ignore)]
        public string RouteName;

        /// <summary>
        ///     访问设置
        /// </summary>
        [DataMember, JsonProperty("access", NullValueHandling = NullValueHandling.Ignore)]
        public ApiAccessOption AccessOption;

        /// <summary>
        ///     参数说明
        /// </summary>
        [DataMember, JsonProperty("argument", NullValueHandling = NullValueHandling.Ignore)]
        public TypeDocument ArgumentInfo;

        /// <summary>
        ///     返回值说明
        /// </summary>
        [DataMember, JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public TypeDocument ResultInfo;
    }
}