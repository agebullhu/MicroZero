using Gboxt.Common.DataModel;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace Agebull.Common.OAuth
{
    /// <summary>
    /// 当前登录的用户信息
    /// </summary>
    public interface IUser : IStateData
    {
        /// <summary>
        /// 用户数字标识
        /// </summary>
        long UserId { get; set; }
    }

    /// <summary>
    /// 当前登录的用户信息
    /// </summary>
    public interface IPerson : IUser
    {
        /// <summary>
        /// 用户昵称
        /// </summary>
        string NickName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        /// <remarks>
        /// 头像
        /// </remarks>
        string AvatarUrl
        {
            get;
            set;
        }

    }

    /// <summary>
    /// 当前登录的用户信息
    /// </summary>
    public interface ILoginUserInfo : IPerson, IApiResultData
    {
        /// <summary>
        /// 当前用户登录到哪个系统（预先定义的系统标识）
        /// </summary>
        string LoginSystem { get; set; }

        /// <summary>
        /// 当前用户登录方式
        /// </summary>
        int LoginType { get; set; }

        /// <summary>
        /// 登录者的手机号
        /// </summary>
        string Phone { get; set; }

        /// <summary>
        /// 登录者的账号
        /// </summary>
        string Account { get; set; }

        /// <summary>
        /// 登录设备的标识
        /// </summary>
        string DeviceId { get; set; }

        /// <summary>
        /// 身份令牌
        /// </summary>
        string AccessToken { get; set; }

        /// <summary>
        /// 登录设备的操作系统
        /// </summary>
        string Os { get; set; }

        /// <summary>
        /// 登录设备的浏览器
        /// </summary>
        string Browser { get; set; }
    }

    /// <summary>
    /// 当前登录的用户信息
    /// </summary>
    [DataContract, Category("上下文")]
    [JsonObject(MemberSerialization.OptIn)]
    public class LoginUserInfo : ILoginUserInfo
    {
        /// <summary>
        /// 用户数字标识
        /// </summary>
        [JsonProperty("userId")]
        public long UserId { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        [JsonProperty("nickName")]
        public string NickName { get; set; }

        /// <summary>
        /// 登录者的账号
        /// </summary>
        [JsonProperty("account")]
        public string Account { get; set; }

        /// <summary>
        ///     数据状态
        /// </summary>
        [JsonIgnore]
        public DataStateType DataState { get; set; }

        /// <summary>
        ///     数据是否已冻结，如果是，则为只读数据
        /// </summary>
        /// <value>bool</value>
        [JsonIgnore]
        public bool IsFreeze { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        /// <remarks>
        /// 头像
        /// </remarks>
        [JsonProperty("avatarUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string AvatarUrl
        {
            get;
            set;
        }

        /// <summary>
        /// 当前用户登录到哪个系统（预先定义的系统标识）
        /// </summary>
        [JsonProperty("loginSystem", NullValueHandling = NullValueHandling.Ignore)]
        public string LoginSystem { get; set; }

        /// <summary>
        /// 当前用户登录方式
        /// </summary>
        [JsonProperty("loginType")]
        public int LoginType { get; set; }

        /// <summary>
        /// 登录者的手机号
        /// </summary>
        [JsonProperty("phone", NullValueHandling = NullValueHandling.Ignore)]
        public string Phone { get; set; }

        /// <summary>
        /// 登录设备的标识
        /// </summary>
        [JsonProperty("deviceId", NullValueHandling = NullValueHandling.Ignore)]
        public string DeviceId { get; set; }
        /// <summary>
        /// 身份令牌
        /// </summary>
        [JsonProperty("accessToken", NullValueHandling = NullValueHandling.Ignore)]
        public string AccessToken { get; set; }


        /// <summary>
        /// 登录设备的操作系统
        /// </summary>
        [JsonProperty("os", NullValueHandling = NullValueHandling.Ignore)]
        public string Os { get; set; }

        /// <summary>
        /// 登录设备的浏览器
        /// </summary>
        [JsonProperty("browser", NullValueHandling = NullValueHandling.Ignore)]
        public string Browser { get; set; }

        #region 预定义

        /// <summary>
        /// 匿名用户
        /// </summary>
        public static LoginUserInfo CreateAnymouse(string did, string br, string os)
        {
            return new LoginUserInfo
            {
                UserId = -2,
                Account = "anymouse",
                Browser = br,
                Os = os,
                DeviceId= did,
                DataState = DataStateType.Discard
            };
        }

        /// <summary>
        /// 系统用户
        /// </summary>
        public static LoginUserInfo System { get; } = new LoginUserInfo
        {
            UserId = -1,
            Account = "system",
            DeviceId = "***system***",
            Browser = "sys",
            Os = "sys",
            IsFreeze = true,
            LoginType = 0,
            DataState = DataStateType.Enable
        };

        #endregion

    }
}