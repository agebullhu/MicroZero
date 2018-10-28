using System.Windows;
using Agebull.Common.OAuth;
using Agebull.Common.Rpc;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;

namespace Agebull.EntityModel.Designer.DataTemplate
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ApiTestItem : NotificationObject
    {

        public Visibility Visibility { get; set; }

        [JsonProperty]
        private string _station;
        [JsonProperty]
        private string _resultJson;
        [JsonProperty]
        private string _requestId;
        [JsonProperty]
        private string _globalId;
        [JsonProperty]
        private string _api;
        [JsonProperty]
        private string _arguments;

        public string Station
        {
            get => _station;
            set
            {
                _station = value;
                RaisePropertyChanged(nameof(Station));
            }
        }

        public string RequestId
        {
            get => _requestId;
            set { _requestId = value;
                RaisePropertyChanged(nameof(RequestId));
            }
        }

        public string GlobalId
        {
            get => _globalId;
            set { _globalId = value;
                RaisePropertyChanged(nameof(GlobalId));
            }
        }

        public string Api
        {
            get => _api;
            set { _api = value;
                RaisePropertyChanged(nameof(Api));
            }
        }

        public LoginUserInfo Customer { get; set; }
        public RequestInfo Request { get; set; }

        public string Arguments
        {
            get => _arguments;
            set { _arguments = value;
                RaisePropertyChanged(nameof(Arguments));
            }
        }

        public ApiResult Result { get; set; }

        public string ResultJson
        {
            get => _resultJson;
            set
            {
                _resultJson = value;
                RaisePropertyChanged(nameof(ResultJson));
            }
        }

    }
}
