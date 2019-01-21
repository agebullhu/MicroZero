using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Agebull.ZeroNet.Core;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    [JsonObject(MemberSerialization.OptIn)]
    [DataContract]
    [Serializable]
    public class StationCountItem
    {

        [DataMember]
        [JsonProperty("station_name")]
        public string Station { get; set; }


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
        [JsonProperty("workers")]
        public List<ZeroWorker> Workers { get; set; } = new List<ZeroWorker>();

        /// <summary>
        ///     状态
        /// </summary>
        [DataMember]
        [JsonProperty("worker_count")]
        public int WorkerCount => Workers == null ? 0 : Workers.Count;

        /// <summary>
        ///     请求入
        /// </summary>
        [DataMember]
        [JsonProperty("request_in")]
        public long RequestIn { get; set; }

        /// <summary>
        ///     请求出
        /// </summary>
        [DataMember]
        [JsonProperty("request_out")]
        public long RequestOut { get; set; }

        /// <summary>
        ///     请求错误
        /// </summary>
        [DataMember]
        [JsonProperty("request_err")]
        public long RequestErr { get; set; }

        /// <summary>
        ///     调用回
        /// </summary>
        [DataMember]
        [JsonProperty("worker_in")]
        public long WorkerIn { get; set; }

        /// <summary>
        ///     调用出
        /// </summary>
        [DataMember]
        [JsonProperty("worker_out")]
        public long WorkerOut { get; set; }

        /// <summary>
        ///     调用错
        /// </summary>
        [DataMember]
        [JsonProperty("worker_err")]
        public long WorkerErr { get; set; }

        /// <summary>
        ///     心跳数（即秒数）
        /// </summary>
        [DataMember]
        [JsonProperty] public int Count { get; set; }

        /// <summary>
        ///     总数
        /// </summary>
        [DataMember]
        [JsonProperty] public long TotalQps { get; set; }

        /// <summary>
        ///     总数
        /// </summary>
        [DataMember]
        [JsonProperty] public long TotalTps { get; set; }

        /// <summary>
        ///     平均
        /// </summary>
        [DataMember]
        [JsonProperty] public long AvgQps { get; set; }

        /// <summary>
        ///     平均
        /// </summary>
        [DataMember]
        [JsonProperty] public long AvgTps { get; set; }

        /// <summary>
        ///     最后
        /// </summary>
        [DataMember]
        [JsonProperty] public long LastTps { get; set; }

        /// <summary>
        ///     最后
        /// </summary>
        [DataMember]
        [JsonProperty] public long LastQps { get; set; }

        /// <summary>
        ///     最大
        /// </summary>
        [DataMember]
        [JsonProperty] public long MaxTps { get; set; }

        /// <summary>
        ///     最大
        /// </summary>
        [DataMember]
        [JsonProperty] public long MaxQps { get; set; }

        /// <summary>
        ///     最小
        /// </summary>
        [DataMember]
        [JsonProperty] public long MinTps { get; set; }

        /// <summary>
        ///     最小
        /// </summary>
        [DataMember]
        [JsonProperty] public long MinQps { get; set; }

        /// <summary>
        ///     设置计数值
        /// </summary>
        public void CheckValue(StationConfig station, StationCountItem src)
        {
            Workers = src.Workers;
            station.State = src.State;
            Station = station.StationName;
            if (Count == 0)
            {
                TotalQps = 0;
                LastQps = 0;
                AvgQps = 0;
                MaxQps = 0;
                MinQps = 0;
                TotalTps = 0;
                LastTps = 0;
                MaxTps = 0;
                MinTps = 0;
                AvgTps = 0;
            }
            else
            {
                if (src.RequestOut < RequestOut)
                {
                    LastQps = src.RequestOut;
                }
                else
                {
                    LastQps = src.RequestOut - RequestOut;
                }
                TotalQps += LastQps;
                if (LastQps > 0)
                {
                    LastQps = LastQps / 2;
                    if (MaxQps == 0 || MaxQps < LastQps)
                        MaxQps = LastQps;
                    if (MinQps == 0 || MinQps > LastQps)
                        MinQps = LastQps;
                    AvgQps = TotalQps / Count / 2;
                }
                if (src.WorkerIn < WorkerIn)
                {
                    LastTps = src.WorkerIn;
                }
                else
                {
                    LastTps = src.WorkerIn - WorkerIn;
                }
                TotalTps += LastTps;
                if (LastTps > 0)
                {
                    LastTps = LastTps / 2;
                    if (MaxTps == 0 || MaxTps < LastTps)
                        MaxTps = LastTps;
                    if (MinTps == 0 || MinTps > LastTps)
                        MinTps = LastTps;
                    AvgTps = TotalTps / Count / 2;
                }
            }

            Count += 1;
            RequestIn = src.RequestIn;
            RequestOut = src.RequestOut;
            RequestErr = src.RequestErr;
            WorkerIn = src.WorkerIn;
            WorkerOut = src.WorkerOut;
            WorkerErr = src.WorkerErr;
        }
    }
}