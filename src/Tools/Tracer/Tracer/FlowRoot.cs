using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MicroZero.Http.Route
{
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class FlowRoot : FlowBase
    {
        [JsonProperty("r")]
        public string RequestId { get; set; }

        [JsonProperty("s")]
        public DateTime StartTime { get; set; }

        [JsonProperty("f")]
        public FlowStep First { get; set; }

        [JsonIgnore]
        public Dictionary<string, FlowStep> Items = new Dictionary<string, FlowStep>();
    }
}