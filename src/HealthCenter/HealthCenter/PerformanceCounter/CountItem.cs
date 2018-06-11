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
    internal class CountItem
    {
        /// <summary>
        /// 设置计数值
        /// </summary>
        /// <param name="tm"></param>
        /// <param name="data"></param>
        public void SetValue(double tm, CountData data)
        {
            LastTime = tm;
            Count += 1;

            if (data.Status == OperatorStatus.DenyAccess)
            {
                Deny += 1;
            }
            else if (data.Status >= OperatorStatus.LocalError)
            {
                Error += 1;
            }
            else
            {
                //if (tm > AppConfig.Config.SystemConfig.WaringTime)
                //{
                //    TimeOut += 1;
                //}
                TotalTime += tm;
                AvgTime = TotalTime / Count;
                if (MaxTime < tm)
                    MaxTime = tm;
                if (MinTime > tm)
                    MinTime = tm;
            }
        }

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
        [DataMember, JsonProperty]
        public Dictionary<string, CountItem> Items { get; set; } = new Dictionary<string, CountItem>(StringComparer.OrdinalIgnoreCase);
    }
}