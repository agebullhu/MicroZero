using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MicroZero.Http.Route
{
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class FlowRoot : FlowBase
    {
        [JsonProperty("rid")]
        public string RequestId { get; set; }

        [JsonProperty("start")]
        public FlowStep Start { get; set; }

        [JsonIgnore]
        public Dictionary<string, FlowStep> Items = new Dictionary<string, FlowStep>();
    }
}