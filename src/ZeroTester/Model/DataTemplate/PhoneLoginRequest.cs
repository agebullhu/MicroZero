using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Agebull.EntityModel.Designer.DataTemplate
{
    /// <summary>
    /// 基于手机的账号登录参数
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class PhoneLoginRequest
    {

        #region 属性

        /// <summary>
        /// 手机号
        /// </summary>
        /// <remarks>
        /// 旧版本编号
        /// </remarks>
        [DataMember, JsonProperty("MobilePhone", NullValueHandling = NullValueHandling.Ignore)]
        public string MobilePhone
        {
            get;
            set;
        }

        /// <summary>
        /// 密码
        /// </summary>
        /// <remarks>
        /// 密码
        /// </remarks>
        [DataMember, JsonProperty("UserPassword", NullValueHandling = NullValueHandling.Ignore)]
        public string UserPassword
        {
            get;
            set;
        }

        /// <summary>
        /// 图片验证码
        /// </summary>
        /// <remarks>
        /// 图片验证码（4位数字+英文）
        /// </remarks>
        [DataMember, JsonProperty("VerificationCode", NullValueHandling = NullValueHandling.Ignore)]
        public string VerificationCode
        {
            get;
            set;
        }
        #endregion

    }
}