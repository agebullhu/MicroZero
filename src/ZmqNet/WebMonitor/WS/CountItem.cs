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
        [DataMember, JsonProperty("id")]
        public string Id { get; set; }

        [DataMember, JsonProperty("label")]
        public string Label { get; set; }

        /// <summary>
        /// 最长时间
        /// </summary>
        [DataMember, JsonProperty]
        public double MaxTime { get; set; } = double.MinValue;

        /// <summary>
        /// 最短时间
        /// </summary>
        [DataMember, JsonProperty]
        public double MinTime { get; set; } = Double.MaxValue;
        /// <summary>
        /// 总时间
        /// </summary>
        [DataMember, JsonProperty]
        public double TotalTime { get; set; }
        /// <summary>
        /// 平均时间
        /// </summary>
        [DataMember, JsonProperty]
        public double AvgTime { get; set; }

        /// <summary>
        /// 平均时间
        /// </summary>
        [DataMember, JsonProperty]
        public double Qps { get; set; }

        /// <summary>
        /// 平均时间
        /// </summary>
        [DataMember, JsonProperty]
        public double Tps { get; set; }

        /// <summary>
        /// 最后时间
        /// </summary>
        [DataMember, JsonProperty]
        public double LastTime { get; set; }

        /// <summary>
        /// 最后时间
        /// </summary>
        [DataMember, JsonProperty]
        public DateTime LastCall { get; set; }
        /// <summary>
        /// 总次数
        /// </summary>
        [DataMember, JsonProperty]
        public int Count { get; set; }

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
        public Dictionary<string, CountItem> Items { get; set; }


        /// <summary>
        /// 子级
        /// </summary>
        [DataMember, JsonProperty("children")]
        public List<CountItem> Children { get; set; }


        /// <summary>
        /// 设置计数值
        /// </summary>
        /// <param name="tm"></param>
        /// <param name="data"></param>
        internal void SetValue(double tm, CountData data)
        {
            LastCall = data.Start;
            Count += 1;
            LastTime = tm;
            TotalTime += tm;
            if (MaxTime < tm)
                MaxTime = tm;
            if (MinTime > tm)
                MinTime = tm;
            if (Count > 2)
            {
                AvgTime = (TotalTime - MaxTime - MinTime) / (Count - 2);
                Qps = (Count - 2) / (TotalTime - MaxTime - MinTime) * 1000;
                Tps = (Count - 2) / (TotalTime - MaxTime - MinTime) * 1000;
            }
            switch (data.Status)
            {
                case OperatorStatus.Success:
                    break;
                case OperatorStatus.DenyAccess:
                    Deny += 1;
                    break;
                case OperatorStatus.FormalError:
                    FormalError += 1;
                    break;
                case OperatorStatus.LocalError:
                    Bug += 1;
                    break;
                case OperatorStatus.RemoteError:
                    Error += 1;
                    break;
            }

            if (data.Status >= OperatorStatus.LocalError)
            {
                Error += 1;
            }
            if (tm > 2000)
            {
                TimeOut += 1;
            }

            //Label = $"{Id} Count:[{Count}] Deny:[{Deny}] Error:[{Error}] TotalTime:[{TotalTime:F2}] AvgTime:[{AvgTime:F2}] MaxTime:[{MaxTime:F2}] MinTime:[{MinTime:F2}]";
        }

    }
}