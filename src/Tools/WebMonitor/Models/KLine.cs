using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// K线数据
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract]
    public class KLine
    {
        /// <summary>
        /// 时间
        /// </summary>
        [DataMember, JsonProperty("x")]
        public long Time { get; set; }

        /// <summary>
        /// 开盘
        /// </summary>
        [DataMember, JsonProperty("q1")]
        public decimal Open { get; set; }

        /// <summary>
        /// 收盘
        /// </summary>
        [DataMember, JsonProperty("q3")]
        public decimal Close { get; set; }

        /// <summary>
        /// 最大
        /// </summary>
        [DataMember, JsonProperty("high")]
        public decimal Max { get; set; }

        /// <summary>
        /// 最小
        /// </summary>
        [DataMember, JsonProperty("low")]
        public decimal Min { get; set; }

        /// <summary>
        /// 平均
        /// </summary>
        [DataMember, JsonProperty/*("median")*/]
        public decimal Avg { get; set; }

        /// <summary>
        /// 计数
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// 计数
        /// </summary>
        public int Count { get; set; }

        public override string ToString()
        {
            return new DateTime(Time* 10000 + 621355968000000000).ToString();
        }
    }
}