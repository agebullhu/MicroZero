using System;
using System.Linq;
using System.Text;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Newtonsoft.Json;
using ZeroMQ;

namespace ZeroNet.Http.Route
{
    /// <summary>
    ///     计划广播节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PlanItem : PublishItem
    {
        /// <summary>
        /// 调用返回值
        /// </summary>
        public string CallResultJson { get; set; }

        /// <summary>
        /// 计划
        /// </summary>
        public ZeroPlan Plan { get; set; }


        /// <summary>
        ///     广播消息解包
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="planItem"></param>
        /// <returns></returns>
        public static bool Unpack(ZMessage frames, out PlanItem planItem)
        {
            return Unpack(frames, out planItem, UnpackAction);
        }


        static void UnpackAction(PlanItem item, byte type, byte[] bytes)
        {
            switch (type)
            {
                case ZeroFrameType.SubTitle:
                    item.SubTitle = GetString(bytes);
                    break;
                case ZeroFrameType.Context:
                    var json = GetString(bytes);
                    item.Plan = JsonConvert.DeserializeObject<ZeroPlan>(json);
                    break;
                case ZeroFrameType.TextContent:
                    item.Content = GetString(bytes);
                    break;
                case ZeroFrameType.JsonValue:
                    item.CallResultJson = GetString(bytes);
                    break;
                case ZeroFrameType.Status:
                    item.Status = bytes;
                    break;
                case ZeroFrameType.BinaryValue:
                    item.Buffer = bytes;
                    break;
                case ZeroFrameType.TsonValue:
                    item.Tson = bytes;
                    break;
            }
        }
    }
}