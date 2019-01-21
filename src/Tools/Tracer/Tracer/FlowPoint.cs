using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Newtonsoft.Json;

namespace ZeroNet.Http.Route
{
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
        /// 数据方向
        /// </summary>
        public byte Tag { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public byte State { get; set; }

        [JsonProperty("tag")]
        public string TagString
        {
            get
            {

                switch (Tag)
                {
                    case 1:
                        return "Client request";
                    case 2:
                        return StationType == "PUB" ? "Publish" : "Issue to worker";
                    case 3:
                        return "Result to client";
                    case 4:
                        return "Result to proxy";
                    default:
                        return "?";
                }
            }
        }

        [JsonProperty("type")]
        public string Type
        {
            get
            {
                switch (Tag)
                {
                    case 1:
                        return "Request";
                    case 2:
                        return "Issue";
                    case 3:
                    case 4:
                        return "Result";
                    default:
                        return "?";
                }
            }
        }
        [JsonProperty("state")]
        public string StateString
        {
            get
            {
                if (StationType == "PUB")
                    return "Publish";
                switch (Tag)
                {
                    case 1:
                    case 2:
                        return ((ZeroByteCommand)State).ToString();
                    default:
                        return ((ZeroOperatorStateType)State).ToString();
                }
            }
        }


        public void GetInfomation(StringBuilder code, FlowPoint point)
        {
            code.AppendLine($"<h3>{point.TagString}</h3");
            code.AppendLine("<hr/>");
            code.AppendLine("<div style='frames'>");
            code.AppendLine($"<i>State:{point.StateString}</i><br/>");
            foreach (var item in point.Item.Frames)
            {
                code.AppendLine($"<label>{item.Key} : {ZeroFrameType.FrameName(item.Value.Type)}</label><br/>");
                string str = ZeroNetMessage.GetString(item.Value.Data);
                if (string.IsNullOrWhiteSpace(str))
                {
                    code.AppendLine("<div class=\'frameValue\'>&nbsp;</div>");
                    continue;
                }
                switch (item.Value.Type)
                {
                    case ZeroFrameType.Context:
                    case ZeroFrameType.Argument:
                    case ZeroFrameType.ResultText:
                    case ZeroFrameType.TextContent:
                        var obj = JsonConvert.DeserializeObject(str);
                        str = JsonConvert.SerializeObject(obj, Formatting.Indented);
                        code.AppendLine($"<div class='frameValue'>{HttpUtility.HtmlEncode(str)?.Replace("\n", "<br/>").Replace(" ", "&nbsp;")}</div>");
                        break;
                    default:
                        code.AppendLine($"<div class='frameValue'>{HttpUtility.HtmlEncode(str)}</div>");
                        break;
                }
            }
            code.AppendLine("</div>");
            code.AppendLine("<hr/>");
        }
    }
}