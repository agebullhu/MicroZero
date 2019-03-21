using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Agebull.MicroZero.ApiDocuments;
using Newtonsoft.Json;

namespace Agebull.MicroZero
{

    /// <summary>
    ///     站点配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    [Serializable]
    public class StationConfig : AnnotationsConfig
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
        ///     是否基础站点
        /// </summary>
        [DataMember]
        [JsonProperty("is_base")]
        public bool IsBaseStation { get; set; }

        /// <summary>
        ///     站点类型
        /// </summary>
        [DataMember]
        [JsonProperty("station_type")]
        public ZeroStationType StationType { get; set; }


        /// <summary>
        ///     是否系统支撑站点
        /// </summary>
        /// <remarks></remarks>
        [IgnoreDataMember, JsonIgnore]
        public bool IsSystem => StationType == ZeroStationType.Dispatcher ||
                                StationType == ZeroStationType.Trace ||
                                StationType == ZeroStationType.Proxy ||
                                StationType == ZeroStationType.Plan;


        /// <summary>
        ///     是否接口站点
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public bool IsApi => StationType == ZeroStationType.Api ||
                             StationType == ZeroStationType.RouteApi ||
                             StationType == ZeroStationType.Vote ||
                             StationType == ZeroStationType.Queue;

        /// <summary>
        ///     是否广播站点
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public bool IsNotify => StationType == ZeroStationType.Notify ||
                                StationType == ZeroStationType.Queue ||
                                StationType == ZeroStationType.Dispatcher ||
                                StationType == ZeroStationType.Trace;

        /// <summary>
        ///     是否普通站点（区别于基础站点）
        /// </summary>
        /// <remarks></remarks>
        [IgnoreDataMember, JsonIgnore]
        public bool IsGeneral => StationType == ZeroStationType.Api ||
                                 StationType == ZeroStationType.RouteApi ||
                                 StationType == ZeroStationType.Vote ||
                                 StationType == ZeroStationType.Queue ||
                                 StationType == ZeroStationType.Notify;

        /// <summary>
        ///     站点简称
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
        [JsonProperty("type")] public string Type => StationType.ToString();

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
        public ZeroCenterState State
        {
            get;
            set;
        }

        /// <summary>
        ///     运行状态
        /// </summary>
        public bool ChangedState(ZeroCenterState value)
        {
            if (State == value)
                return false;
            if (State >= ZeroCenterState.Failed && value >= ZeroCenterState.Failed && State > value)
                return false;
            State = value;
            return true;
        }
        /*// <summary>
        ///     状态变更事件
        /// </summary>
        internal Action<ZeroCenterState, ZeroCenterState> OnStateChanged;*/

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
            IsBaseStation = src.IsBaseStation;
            StationName = src.StationName;
            StationAlias = src.StationAlias;
        }

        /// <summary>
        /// 构造
        /// </summary>
        public static string TypeName(ZeroStationType type)
        {
            switch (type)
            {
                case ZeroStationType.Api:
                    return  "api";
                case ZeroStationType.Vote:
                    return  "vote";
                case ZeroStationType.RouteApi:
                    return  "route_api";
                case ZeroStationType.Queue:
                    return "queue";
                case ZeroStationType.Notify:
                    return "notify";
                case ZeroStationType.Plan:
                case ZeroStationType.Proxy:
                case ZeroStationType.Trace:
                case ZeroStationType.Dispatcher:
                    return "sys";
                default:
                    return "err";
            }
        }
    }
}