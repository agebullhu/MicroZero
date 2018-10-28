using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Agebull.EntityModel.Designer.DataTemplate
{
    /// <summary>
    /// 登录返回数据
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class LoginResponse
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        /// <remarks>
        /// 访问令牌
        /// </remarks>
        [DataMember, JsonProperty("AccessToken", NullValueHandling = NullValueHandling.Ignore)]
        public string AccessToken
        {
            get;
            set;
        }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        /// <remarks>
        /// 刷新Token
        /// </remarks>
        [DataMember, JsonProperty("RefreshToken", NullValueHandling = NullValueHandling.Ignore)]
        public string RefreshToken
        {
            get;
            set;
        }

        /// <summary>
        /// 用户对象存储的用户信息
        /// </summary>
        /// <remarks>
        /// 用户对象存储的用户信息
        /// </remarks>
        [DataMember, JsonProperty("Profile", NullValueHandling = NullValueHandling.Ignore)]
        public PersonPublishInfo Profile
        {
            get;
            set;
        }

    }
}