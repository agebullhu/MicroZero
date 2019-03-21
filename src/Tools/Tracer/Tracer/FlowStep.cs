using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace MicroZero.Http.Route
{
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class FlowStep : FlowPoint
    {
        [JsonProperty("station")]
        public string Station { get; set; }

        [JsonProperty("cmd")]
        public string Command { get; set; }

        [JsonProperty("gid")]
        public string GlobalId { get; set; }

        [JsonProperty("cid")]
        public string CallId { get; set; }

        [JsonIgnore]
        public FlowBase Parent { get; set; }

        [JsonIgnore]
        public List<FlowPoint> States = new List<FlowPoint>();


        [JsonProperty("child")]
        public List<FlowStep> Child = new List<FlowStep>();

        [JsonProperty("info")]
        public string Infomation
        {
            get
            {
                StringBuilder code = new StringBuilder();
                //GetInfomation(code, this);
                foreach (var item in States)
                    GetInfomation(code, item);
                return code.ToString();
            }
        }

        [JsonProperty("steps")]
        public Dictionary<string, string> Steps
        {
            get
            {
                Dictionary<string, string> steps = new Dictionary<string, string>();

                //GetInfomation(code, this);
                foreach (var type in States.GroupBy(p => p.Type))
                {
                    StringBuilder code = new StringBuilder();
                    foreach (var item in type)
                        GetInfomation(code, item);
                    steps.Add(type.Key, code.ToString());
                }
                return steps;
            }
        }
    }
}