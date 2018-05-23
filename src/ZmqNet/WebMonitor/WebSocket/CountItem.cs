using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
        public int Error { get; set; }

        /// <summary>
        /// 拒绝次数
        /// </summary>
        [DataMember, JsonProperty]
        public int Deny { get; set; }
        
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
        public void SetValue(double tm, RouteData data)
        {
            LastCall = data.Start;
            Count += 1;
            LastTime = tm;
            TotalTime += tm;
            AvgTime = TotalTime / Count;
            if (MaxTime < tm)
                MaxTime = tm;
            if (MinTime > tm)
                MinTime = tm;

            if (data.Status == RouteStatus.DenyAccess)
            {
                Deny += 1;
            }
            if (data.Status >= RouteStatus.LocalError)
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