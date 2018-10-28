using System.Runtime.Serialization;
using Agebull.Common.OAuth;
using Newtonsoft.Json;

namespace Agebull.EntityModel.Designer.DataTemplate
{
    /// <summary>
    /// 测试使用的用户信息
    /// </summary>

    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public class UserInfo : NotificationObject
    {
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string _userName = "root";
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string _passWord = "root";

        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string _accessToken;
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string _refreshToken;
        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private string _deviceId;

        [DataMember, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public LoginUserInfo Customer { get; set; }

        public string UserName
        {
            get => _userName;
            set { _userName = value;
                RaisePropertyChanged(nameof(UserName));
            }
        }

        public string PassWord
        {
            get => _passWord;
            set { _passWord = value;
                RaisePropertyChanged(nameof(PassWord));
            }
        }


        public string AccessToken
        {
            get => _accessToken;
            set { _accessToken = value;
                RaisePropertyChanged(nameof(AccessToken));
            }
        }

        public string RefreshToken
        {
            get => _refreshToken;
            set { _refreshToken = value;
                RaisePropertyChanged(nameof(RefreshToken));
            }
        }

        public string DeviceId
        {
            get => _deviceId;
            set { _deviceId = value;
                RaisePropertyChanged(nameof(DeviceId));
            }
        }
        [JsonProperty]
        private string _contextJson;

        public string ContextJson
        {
            get => _contextJson;
            set
            {
                _contextJson = value;
                RaisePropertyChanged(nameof(ContextJson));
            }
        }
    }
}