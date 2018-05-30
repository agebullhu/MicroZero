using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 站点配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class StationConfig : SimpleConfig, IApiResultData
    {
        /// <summary>
        /// 站点名称
        /// </summary>
        [DataMember, JsonProperty("station_name")]
        public string StationName { get => _name; set => _name = value; }
        /// <summary>
        /// 站点名称
        /// </summary>
        [DataMember, JsonProperty("short_name")]
        public string ShortName { get; set; }
        /// <summary>
        /// 站点别名
        /// </summary>
        [DataMember, JsonProperty("station_alias")]
        public List<string> StationAlias { get; set; }
        /// <summary>
        /// 站点类型
        /// </summary>
        [DataMember, JsonProperty("station_type")]
        public int StationType { get; set; }
        /// <summary>
        /// 入站端口
        /// </summary>
        [DataMember, JsonProperty("request_port")]
        public int RequestPort { get; set; }

        /// <summary>
        /// 入站地址
        /// </summary>
        [DataMember, JsonProperty]
        public string RequestAddress => ZeroIdentityHelper.GetRequestAddress(StationName, RequestPort);

        /// <summary>
        /// 入站端口
        /// </summary>
        [DataMember, JsonProperty("worker_port")]
        public int WorkerPort { get; set; }

        /// <summary>
        /// 出站地址
        /// </summary>
        [DataMember, JsonProperty]
        public string WorkerAddress => ZeroIdentityHelper.GetWorkerAddress(StationName, WorkerPort);

        /// <summary>
        /// 请求入
        /// </summary>
        [DataMember, JsonProperty("request_in")]
        public long RequestIn { get; set; }
        /// <summary>
        /// 请求出
        /// </summary>
        [DataMember, JsonProperty("request_out")]
        public long RequestOut { get; set; }
        /// <summary>
        /// 请求错误
        /// </summary>
        [DataMember, JsonProperty("request_err")]
        public long RequestErr { get; set; }

        /// <summary>
        /// 调用回
        /// </summary>
        [DataMember, JsonProperty("worker_in")]
        public long WorkerIn { get; set; }
        /// <summary>
        /// 调用出
        /// </summary>
        [DataMember, JsonProperty("worker_out")]
        public long WorkerOut { get; set; }
        /// <summary>
        /// 调用错
        /// </summary>
        [DataMember, JsonProperty("worker_err")]
        public long WorkerErr { get; set; }

        /// <summary>
        ///     运行状态
        /// </summary>
        [DataMember, JsonProperty("station_state")]
        public ZeroCenterState State { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [DataMember, JsonProperty("state")]
        public string _ => State.ToString();

        /// <summary>
        /// 状态
        /// </summary>
        [DataMember, JsonProperty("workers")]
        public List<string> Workers { get; set; }

        /// <summary>
        /// 站点类型
        /// </summary>
        [DataMember, JsonProperty]
        public string TypeName
        {
            get
            {
                switch (StationType)
                {
                    default:
                        return "Error";
                    case ZeroStation.StationTypeApi:
                        return "API";
                    case ZeroStation.StationTypeDispatcher:
                        return "Dispatcher";
                    case ZeroStation.StationTypeMonitor:
                        return "Monitor";
                    case ZeroStation.StationTypePublish:
                        return "Publish";
                    case ZeroStation.StationTypeVote:
                        return "Vote";
                }
            }
        }
        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="src"></param>
        public void Copy(StationConfig src)
        {
            StationName = src.StationName;
            StationAlias = src.StationAlias;
            StationType = src.StationType;
            RequestPort = src.RequestPort;
            WorkerPort = src.WorkerPort;
            State = src.State;
            RequestIn = src.RequestIn;
            RequestOut = src.RequestOut;
            RequestErr = src.RequestErr;
            WorkerIn = src.WorkerIn;
            WorkerOut = src.WorkerOut;
            WorkerErr = src.WorkerErr;
            Workers = src.Workers;
        }

    }
}
