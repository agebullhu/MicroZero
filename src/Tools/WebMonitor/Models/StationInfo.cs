using Agebull.ZeroNet.Core;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace WebMonitor.Models
{
    /// <summary>
    ///     站点配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    [Serializable]
    public class StationInfo
    {
        /// <summary>
        ///     站点名称
        /// </summary>
        [DataMember, JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        [DataMember, JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        ///     站点名称
        /// </summary>
        [DataMember, JsonProperty("short_name")]
        public string ShortName { get; set; }

        /// <summary>
        ///     站点别名
        /// </summary>
        [DataMember, JsonProperty("alias")]
        public string Alias { get; set; }

        /// <summary>
        ///     入站地址
        /// </summary>
        [DataMember, JsonProperty("clientCallAddress")]
        public string RequestAddress { get; set; }

        /// <summary>
        ///     出站地址
        /// </summary>
        [DataMember, JsonProperty("workerResultAddress")]
        public string WorkerResultAddress { get; set; }

        /// <summary>
        ///     出站地址
        /// </summary>
        [DataMember, JsonProperty("workerCallAddress")]
        public string WorkerCallAddress { get; set; }

        /// <summary>
        ///     状态
        /// </summary>
        [DataMember, JsonProperty("state")]
        public string State { get; set; }

        /// <summary>
        ///     站点类型
        /// </summary>
        [DataMember, JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        ///     构造
        /// </summary>
        public StationInfo()
        {
        }

        /// <summary>
        ///     复制
        /// </summary>
        /// <param name="src"></param>
        public StationInfo(StationConfig src)
        {
            Name = src.StationName;
            Description = src.Description;
            ShortName = src.ShortName;
            Alias = src.StationAlias.LinkToString(',');

            switch (src.StationType)
            {
                default:
                    Type = "Error"; break;
                case ZeroStation.StationTypeApi:
                    Type = "API"; break;
                case ZeroStation.StationTypeDispatcher:
                    Type = "Dispatcher"; break;
                case ZeroStation.StationTypePublish:
                    Type = "Pub"; break;
                case ZeroStation.StationTypeVote:
                    Type = "Vote"; break;
                case ZeroStation.StationTypePlanDispatcher:
                    Type = "Plan"; break;
            }
            RequestAddress = src.RequestAddress;
            WorkerCallAddress = src.WorkerCallAddress;
            WorkerResultAddress = src.WorkerResultAddress;
            State = src.State.ToString();
        }

    }
}