using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Agebull.MicroZero
{
    /// <summary>
    /// 工作对象
    /// </summary>
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class ZeroWorker
    {
        /// <summary>
        ///  实名
        /// </summary>
        [DataMember, JsonProperty("real_name")]
        public string RealName { get; set; }

        /// <summary>
        ///  上报的IP地址
        /// </summary>
        [DataMember, JsonProperty("ip_address")]
        public string IpAddress { get; set; }

        /// <summary>
        ///  上次心跳的时间
        /// </summary>
        [DataMember, JsonProperty("pre_time")] public string pre_time;

        /// <summary>
        ///  健康等级
        /// </summary>
        [DataMember, JsonProperty("level")]
        public int Level { get; set; }

        /// <summary>
        ///  状态 -1 已失联 0 正在准备中 1 已就绪 3 已退出
        /// </summary>
        [DataMember, JsonProperty("state")]
        public int State { get; set; }

        /// <summary>
        ///  状态
        /// </summary>
        [DataMember, JsonProperty("state_text")]
        public string StateText
        {
            get
            {
                switch (State)
                {
                    case 0:
                        return "Prepare";
                    case 1:
                        return "Ready";
                    case 2:
                        return "Left";
                    default:
                        return "Lost";
                }
            }
        }
    }
}