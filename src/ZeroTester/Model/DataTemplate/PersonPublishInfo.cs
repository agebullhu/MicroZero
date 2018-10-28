using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Agebull.EntityModel.Designer.DataTemplate
{
    /// <summary>
    /// 用户的公开信息
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class PersonPublishInfo
    {
        /// <summary>
        /// 头像
        /// </summary>
        /// <remarks>
        /// 头像
        /// </remarks>
        [DataMember, JsonProperty("AvatarUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string AvatarUrl { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        /// <remarks>
        /// 昵称
        /// </remarks>
        [DataMember, JsonProperty("NickName", NullValueHandling = NullValueHandling.Ignore)]
        public string NickName { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        /// <remarks>
        /// 所在县Id
        /// </remarks>
        [DataMember, JsonProperty("PhoneNumber", NullValueHandling = NullValueHandling.Ignore)]
        public string PhoneNumber { get; set; }
    }
}