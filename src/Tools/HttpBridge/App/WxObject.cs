using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ZeroNet.Http.Gateway
{
    /// <summary>
    /// 微信用户信息
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public partial class WxUser
    {

        /// <summary>
        /// 用户的唯一标识
        /// </summary>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("openid", NullValueHandling = NullValueHandling.Ignore)]
        public string Openid
        {
            get;
            set;
        }
        /// <summary>
        /// 用户昵称
        /// </summary>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("nickname", NullValueHandling = NullValueHandling.Ignore)]
        public string Nickname
        {
            get;
            set;
        }
        /// <summary>
        /// 用户的性别
        /// </summary>
        /// <remarks>
        /// 值为1时是男性,值为2时是女性,值为0时是未知
        /// </remarks>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("sex", NullValueHandling = NullValueHandling.Ignore)]
        public string Sex
        {
            get;
            set;
        }
        /// <summary>
        /// 用户个人资料填写的省份
        /// </summary>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("province", NullValueHandling = NullValueHandling.Ignore)]
        public string Province
        {
            get;
            set;
        }
        /// <summary>
        /// 普通用户个人资料填写的城市
        /// </summary>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
        public string City
        {
            get;
            set;
        }
        /// <summary>
        /// 国家
        /// </summary>
        /// <remarks>
        /// 如中国为CN
        /// </remarks>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
        public string Country
        {
            get;
            set;
        }
        /// <summary>
        /// 用户头像
        /// </summary>
        /// <remarks>
        /// 最后一个数值代表正方形头像大小（有0、46、64、96、132数值可选,0代表640*640正方形头像）,用户没有头像时该项为空
        /// </remarks>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("headimgurl", NullValueHandling = NullValueHandling.Ignore)]
        public string Headimgurl
        {
            get;
            set;
        }
        /// <summary>
        /// 用户特权信息
        /// </summary>
        /// <remarks>
        /// json,数组,如微信沃卡用户为（chinaunicom）
        /// </remarks>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("privilege", NullValueHandling = NullValueHandling.Ignore)]
        public string Privilege
        {
            get;
            set;
        }
    }


    /// <summary>
    /// 微信Auth信息
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public partial class WxAuth
    {

        /// <summary>
        /// 网页授权接口调用凭证
        /// </summary>
        /// <remarks>
        /// 注意：此access_token与基础支持的access_token不同
        /// </remarks>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("access_token", NullValueHandling = NullValueHandling.Ignore)]
        public string AccessToken
        {
            get;
            set;
        }
        /// <summary>
        /// access_token接口调用凭证超时时间
        /// </summary>
        /// <remarks>
        /// 单位（秒）
        /// </remarks>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("expires_in", NullValueHandling = NullValueHandling.Ignore)]
        public string ExpiresIn
        {
            get;
            set;
        }
        /// <summary>
        /// 用户刷新access_token
        /// </summary>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("refresh_token", NullValueHandling = NullValueHandling.Ignore)]
        public string RefreshToken
        {
            get;
            set;
        }
        /// <summary>
        /// 用户唯一标识
        /// </summary>
        /// <remarks>
        /// 请注意,在未关注公众号时,用户访问公众号的网页,也会产生一个用户和公众号唯一的OpenID
        /// </remarks>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("openid", NullValueHandling = NullValueHandling.Ignore)]
        public string Openid
        {
            get;
            set;
        }
        /// <summary>
        /// 用户授权的作用域
        /// </summary>
        /// <remarks>
        /// 使用逗号（,）分隔
        /// </remarks>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("scope", NullValueHandling = NullValueHandling.Ignore)]
        public string Scope
        {
            get;
            set;
        }
    }


    /// <summary>
    /// 登陆结果
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public partial class LoginResponse
    {
        /// <summary>
        /// 访问令牌
        /// </summary>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("AccessToken", NullValueHandling = NullValueHandling.Ignore), DisplayName(@"访问令牌")]
        public string AccessToken
        {
            get;
            set;
        }


        /// <summary>
        /// 刷新令牌
        /// </summary>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("RefreshToken", NullValueHandling = NullValueHandling.Ignore), DisplayName(@"刷新令牌")]
        public string RefreshToken
        {
            get;
            set;
        }


        /// <summary>
        /// 用户对象存储的用户信息
        /// </summary>
        [DataMember, JsonProperty("Profile", NullValueHandling = NullValueHandling.Ignore), DisplayName(@"用户对象存储的用户信息")]
        public PersonPublishInfo Profile
        {
            get;
            set;
        }


        /// <summary>
        /// 用户的微信at
        /// </summary>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("WechatAccessToken", NullValueHandling = NullValueHandling.Ignore), DisplayName(@"用户的微信at")]
        public string WechatAccessToken
        {
            get;
            set;
        }


        /// <summary>
        /// 用户的微信at过期时间
        /// </summary>
        [DataMember, JsonProperty("WechatAccessTokenExpires", NullValueHandling = NullValueHandling.Ignore), DisplayName(@"用户的微信at过期时间")]
        public DateTime WechatAccessTokenExpires
        {
            get;
            set;
        }


        /// <summary>
        /// 用户的微信rt
        /// </summary>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("WechatRefreshToken", NullValueHandling = NullValueHandling.Ignore), DisplayName(@"用户的微信rt")]
        public string WechatRefreshToken
        {
            get;
            set;
        }


        /// <summary>
        /// 微信公共号AppId
        /// </summary>
        /// <value>
        /// 可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataMember, JsonProperty("WechatAppId", NullValueHandling = NullValueHandling.Ignore), DisplayName(@"微信公共号AppId")]
        public string WechatAppId
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 用户的公开信息
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public partial class PersonPublishInfo
    {
        /// <summary>
        /// 头像
        /// </summary>
        /// <remarks>
        /// 头像
        /// </remarks>
        [DataMember, JsonProperty("AvatarUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string AvatarUrl
        {
            get;
            set;
        }

        /// <summary>
        /// 昵称
        /// </summary>
        /// <remarks>
        /// 昵称
        /// </remarks>
        [DataMember, JsonProperty("NickName", NullValueHandling = NullValueHandling.Ignore)]
        public string NickName
        {
            get;
            set;
        }

        /// <summary>
        /// 手机号码
        /// </summary>
        /// <remarks>
        /// 所在县Id
        /// </remarks>
        [DataMember, JsonProperty("PhoneNumber", NullValueHandling = NullValueHandling.Ignore)]
        public string PhoneNumber
        {
            get;
            set;
        }
    }
}