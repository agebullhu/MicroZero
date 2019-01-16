using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;
using WebMonitor;

namespace ZeroNet.Http.Route
{
    /// <summary>
    /// API调用跟踪器
    /// </summary>
    public class FlowTracer : SubStation<CountData, PublishItem>
    {
        /// <summary>
        /// 构造路由计数器
        /// </summary>
        public FlowTracer()
        {
            Name = "FlowTracer";
            StationName = "TraceDispatcher";
            Subscribe = "";
            IsRealModel = true;
        }

        void SendToWebSocket(FlowRoot root)
        {
            var json = JsonConvert.SerializeObject(root);
            WebSocketNotify.Publish("trace", "flow", json);
            //Console.WriteLine(json);
        }

        private readonly Dictionary<string, FlowRoot> _flows = new Dictionary<string, FlowRoot>();


        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override void Handle(PublishItem args)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(args.RequestId))
                    return;
                if (string.IsNullOrWhiteSpace(args.CallId))
                    args.CallId = "0";
                Console.WriteLine($"{args.RequestId}({args.CallId}>{args.GlobalId}) : {args.Tag}/{args.State} : {args.Station}/{args.CommandOrSubTitle}");
                if (!_flows.TryGetValue(args.RequestId, out var root))
                {
                    _flows.Add(args.RequestId, root = new FlowRoot
                    {
                        RequestId = args.RequestId,
                        Start = new FlowStep
                        {
                            Item = args,
                            GlobalId = args.GlobalId,
                            CallId = args.CallId,
                            Station = args.Requester,
                            Command = args.CommandOrSubTitle,
                            tag = args.Tag,
                            state = (byte)args.State
                        }
                    });
                    //root.Items.Add(args.GlobalId, root.Start);
                    //root.Start.States.Add(new FlowPoint
                    //{
                    //    Item = args,
                    //    tag = args.Tag,
                    //    state = (byte)args.State
                    //});
                    Handle(args, root);
                }
                else
                {
                    Handle(args, root);
                    SendToWebSocket(root);
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("FlowTracer", e, args.Content);
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        void Handle(PublishItem args, FlowRoot root)
        {
            if (root.Items.TryGetValue(args.GlobalId, out var step))
            {
                // 1 ? 之前找不到父级时已被加入
                FlowStep par;
                if (args.CallId == "0")
                {
                    par = root.Start;
                }
                else if (!root.Items.TryGetValue(args.CallId, out par))
                {
                    root.Items.Add(args.CallId, par = new FlowStep
                    {
                        GlobalId = args.CallId
                    }); //2 ? 上级置疑：后续引发1?的判断
                    par.Child.Add(step);
                    step.Parent = par;
                }
                if (step == root.Start && step.Station != args.Requester)
                {
                    par.Child.Add(step = new FlowStep
                    {
                        Item = args,
                        GlobalId = args.GlobalId,
                        CallId = args.CallId,
                        Station = args.Requester,
                        Command = args.CommandOrSubTitle,
                        tag = args.Tag,
                        state = (byte)args.State,
                        Parent = par
                    });
                    par.Child.Add(step);
                    root.Items[args.GlobalId] = step;
                }
                else
                {
                    step.Item = args;
                    step.CallId = args.CallId;
                    step.Station = args.Station;
                    if (string.IsNullOrWhiteSpace(step.Command))
                        step.Command = args.CommandOrSubTitle;
                    if (step.tag < args.Tag)
                        step.tag = args.Tag;
                    step.state = (byte)args.State;
                }
            }
            else
            {
                root.Items.Add(args.GlobalId, step = new FlowStep
                {
                    Item = args,
                    GlobalId = args.GlobalId,
                    CallId = args.CallId,
                    Station = args.Station,
                    Command = args.CommandOrSubTitle,
                    tag = args.Tag,
                    state = (byte)args.State
                });
                FlowStep par;
                if (args.CallId == "0")
                {
                    par = root.Start;
                }
                else if (!root.Items.TryGetValue(args.CallId, out par))
                {
                    root.Items.Add(args.CallId, par = new FlowStep
                    {
                        GlobalId = args.CallId
                    }); //2 ? 上级置疑：后续引发1?的判断
                }
                step.Parent = par;
                par.Child.Add(step);
            }
            step.States.Add(new FlowPoint
            {
                Item = args,
                tag = args.Tag,
                state = (byte)args.State
            });
        }
    }
    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class FlowBase
    {

    }

    [JsonObject(MemberSerialization.OptIn), DataContract, Serializable]
    public class FlowPoint : FlowBase
    {
        private PublishItem _item;

        [JsonIgnore]
        public PublishItem Item
        {
            get => _item;
            set
            {
                _item = value;
                if (_item != null)
                {
                    StationType = ZeroNetMessage.GetString(Item.Frames.Values.FirstOrDefault(p => p.Type == ZeroFrameType.StationType)?.Data);
                }
            }
        }
        /// <summary>
        /// 帧数据类型
        /// </summary>
        public string StationType { get; set; }


        /// <summary>
        /// 帧数据类型
        /// </summary>
        public byte tag { get; set; }

        /// <summary>
        /// 帧数据类型
        /// </summary>
        public byte state { get; set; }

        [JsonProperty("tag")]
        public string Tag
        {
            get
            {

                switch (tag)
                {
                    case 1:
                        return "Client request";
                    case 2:
                        return StationType == "PUB" ? "Publish" : "Issue to worker";
                    case 3:
                        return "Result to client";
                    case 4:
                        return "Result to proxy";
                }
                return "?";
            }
        }

        [JsonProperty("state")]
        public string State
        {
            get
            {
                if (StationType == "PUB")
                    return "Publish";
                switch (tag)
                {
                    case 1:
                    case 2:
                        return ((ZeroByteCommand)state).ToString();
                }
                return ((ZeroOperatorStateType)state).ToString();
            }
        }
    }

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


        public void GetInfomation(StringBuilder code, FlowPoint point)
        {
            code.AppendLine("<hr/>");
            code.AppendLine($"<h3>{point.Tag}</h3");
            code.AppendLine("<hr/>");
            code.AppendLine("<div style='frames'>");
            code.AppendLine($"<i>State:{point.State}</i><br/>");
            foreach (var item in point.Item.Frames)
            {
                code.AppendLine($"<label>{item.Key} : {ZeroFrameType.FrameName(item.Value.Type)}</label><br/>");
                string str = ZeroNetMessage.GetString(item.Value.Data);
                if (string.IsNullOrWhiteSpace(str))
                {
                    code.AppendLine($"<div class='frameValue'></div>");
                    continue;
                }
                if (item.Value.Type == ZeroFrameType.Context || item.Value.Type == ZeroFrameType.TextContent)
                {
                    var obj = JsonConvert.DeserializeObject(str);
                    str = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    code.AppendLine($"<div class='frameValue'>{HttpUtility.HtmlEncode(str)?.Replace("\n", "<br/>").Replace(" ", "&nbsp;")}</div>");
                }
                else
                    code.AppendLine($"<div class='frameValue'>{HttpUtility.HtmlEncode(str)}</div>");
            }
            code.AppendLine("</div>");
        }
    }
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