using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{

    /// <summary>
    /// 路由计数器节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract]
    public class CountItem
    {
        /// <summary>
        /// 时间
        /// </summary>
        [DataMember, JsonProperty("time")]
        public long Time { get; set; }

        /// <summary>
        /// 最后时间
        /// </summary>
        [DataMember, JsonProperty("LastCall")]
        public DateTime Last { get; set; }
        /// <summary>
        /// 最后时间
        /// </summary>
        [IgnoreDataMember, JsonIgnore ]
        public long LastCall { get; set; }

        [DataMember, JsonProperty("id")]
        public string Id { get; set; }

        [DataMember, JsonProperty("label")]
        public string Label { get; set; }

        /// <summary>
        /// 最长时间
        /// </summary>
        [DataMember, JsonProperty]
        public long MaxTime { get; set; }

        /// <summary>
        /// 最短时间
        /// </summary>
        [DataMember, JsonProperty]
        public long MinTime { get; set; }
        /// <summary>
        /// 总时间
        /// </summary>
        [DataMember, JsonProperty]
        public long TotalTime { get; set; }
        /// <summary>
        /// 平均时间
        /// </summary>
        [DataMember, JsonProperty]
        public long AvgTime { get; set; }

        /// <summary>
        /// 平均时间
        /// </summary>
        [DataMember, JsonProperty]
        public long Ideal { get; set; }

        /// <summary>
        /// 最后时间
        /// </summary>
        [DataMember, JsonProperty]
        public long LastTime { get; set; }

        /// <summary>
        /// 总次数
        /// </summary>
        [DataMember, JsonProperty]
        public int Count { get; set; }

        /// <summary>
        /// 单元次数
        /// </summary>
        [DataMember, JsonProperty]
        public int UnitCount { get; set; }

        /// <summary>
        /// 错误次数
        /// </summary>
        [DataMember, JsonProperty]
        public int TimeOut { get; set; }


        /// <summary>
        /// 错误次数
        /// </summary>
        [DataMember, JsonProperty]
        public int Bug { get; set; }

        /// <summary>
        /// 错误次数
        /// </summary>
        [DataMember, JsonProperty]
        public int Error { get; set; }

        /// <summary>
        /// 拒绝次数
        /// </summary>
        [DataMember, JsonProperty]
        public int Deny { get; set; }

        /// <summary>
        /// 错误调用次数
        /// </summary>
        [DataMember, JsonProperty]
        public int FormalError { get; set; }

        /// <summary>
        /// 子级
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        public Dictionary<string, CountItem> Items { get; set; } = new Dictionary<string, CountItem>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// 子级
        /// </summary>
        [DataMember, JsonProperty("children")]
        public List<CountItem> Children { get; set; } = new List<CountItem>();


        /// <summary>
        /// 重置单元计数
        /// </summary>
        internal void Start()
        {
            UnitCount = 0;
            if (Children == null)
                return;
            foreach (var child in Children)
                child.Start();
        }
        /// <summary>
        /// 重置单元计数
        /// </summary>
        internal void End()
        {
            if (Count > 0 && TotalTime > 0)
            {
                AvgTime = TotalTime / Count;
                Ideal = Count * 1000 / TotalTime;
            }
            else
            {
                AvgTime = 0;
                Ideal = 0;
            }
            Time = (LastCall - 621355968000000000) / 10000;
            Time -= Time % 1000;
            Last = new DateTime(LastCall);
            foreach (var child in Children)
                child.End();
        }

        /// <summary>
        /// 设置计数值
        /// </summary>
        /// <param name="tm"></param>
        /// <param name="data"></param>
        internal void SetValue(long tm, CountData data)
        {
            LastCall = data.End;
            Count += 1;
            UnitCount += 1;
            LastTime = tm;
            TotalTime += tm;
            if (MaxTime==0 || MaxTime < tm)
                MaxTime = tm;
            if (MinTime == 0 || MinTime > tm)
                MinTime = tm;
            switch (data.Status)
            {
                case OperatorStatus.Success:
                    break;
                case OperatorStatus.Unavailable:
                case OperatorStatus.NotFind:
                case OperatorStatus.DenyAccess:
                    Deny += 1;
                    break;
                case OperatorStatus.FormalError:
                    FormalError += 1;
                    break;
                case OperatorStatus.LocalException:
                case OperatorStatus.LogicalError:
                    Bug += 1;
                    break;
                default:
                    Error += 1;
                    break;
            }

            if (tm > 20000000)
            {
                TimeOut += 1;
            }
        }
    }
}