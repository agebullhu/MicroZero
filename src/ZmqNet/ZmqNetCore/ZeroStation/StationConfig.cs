using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     站点配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    [Serializable]
    public class StationConfig : AnnotationsConfig, IApiResultData
    {
        /// <summary>
        ///     站点名称
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public string StationName
        {
            get => Name;
            set => Name = value;
        }

        /// <summary>
        ///     站点名称
        /// </summary>
        [DataMember]
        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        /// <summary>
        ///     站点别名
        /// </summary>
        [DataMember]
        [JsonProperty("station_alias")]
        public List<string> StationAlias { get; set; }

        /// <summary>
        ///     站点类型
        /// </summary>
        [DataMember]
        [JsonProperty("station_type")]
        public int StationType { get; set; }

        /// <summary>
        ///     入站端口
        /// </summary>
        [DataMember]
        [JsonProperty("request_port")]
        public int RequestPort { get; set; }

        /// <summary>
        ///     入站地址
        /// </summary>
        [DataMember]
        [JsonProperty]
        public string RequestAddress => ZeroIdentityHelper.GetRequestAddress(StationName, RequestPort);

        /// <summary>
        ///     入站端口
        /// </summary>
        [DataMember]
        [JsonProperty("worker_out_port")]
        public int WorkerCallPort { get; set; }

        /// <summary>
        ///     入站端口
        /// </summary>
        [DataMember]
        [JsonProperty("worker_in_port")]
        public int WorkerResultPort { get; set; }

        /// <summary>
        ///     出站地址
        /// </summary>
        [DataMember]
        [JsonProperty]
        public string WorkerResultAddress => ZeroIdentityHelper.GetWorkerAddress(StationName, WorkerResultPort);

        /// <summary>
        ///     出站地址
        /// </summary>
        [DataMember]
        [JsonProperty]
        public string WorkerCallAddress => ZeroIdentityHelper.GetWorkerAddress(StationName, WorkerCallPort);

        /// <summary>
        ///     出站地址
        /// </summary>
        [DataMember]
        [JsonProperty]
        public string SubAddress => ZeroIdentityHelper.GetSubscriberAddress(StationName, WorkerCallPort);


        /// <summary>
        ///     运行状态
        /// </summary>
        [DataMember]
        [JsonProperty("station_state")]
        public ZeroCenterState State { get; set; }

        /// <summary>
        ///     状态
        /// </summary>
        [DataMember]
        [JsonProperty("workers")] public List<ZeroWorker> Workers { get; set; }

        /// <summary>
        ///     复制
        /// </summary>
        /// <param name="src"></param>
        public void Copy(StationConfig src)
        {
            StationName = src.StationName;
            StationAlias = src.StationAlias;
            StationType = src.StationType;
            RequestPort = src.RequestPort;
            WorkerCallPort = src.WorkerCallPort;
            WorkerResultPort = src.WorkerResultPort;
            State = src.State;
            Workers = src.Workers;
        }

    }
}