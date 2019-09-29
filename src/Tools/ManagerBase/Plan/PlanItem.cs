using System.Linq;
using Agebull.MicroZero;
using Agebull.MicroZero.PubSub;
using Newtonsoft.Json;
using ZeroMQ;

namespace MicroZero.Http.Route
{
    /// <summary>
    ///     计划广播节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PlanItem : PublishItem
    {
        /// <summary>
        /// 逻辑操作状态
        /// </summary>
        public ZeroOperatorStateType State
        {
            get => (ZeroOperatorStateType)ZeroState;
            set => ZeroState = (byte)value;
        }

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
        /// <param name="msg"></param>
        /// <param name="planItem"></param>
        /// <returns></returns>
        public static bool UnpackResult(ZMessage msg, out PlanItem planItem)
        {
            if (msg == null)
            {
                planItem = null;
                return false;
            }

            byte[][] msgs;
            using (msg)
            {
                msgs = msg.Select(p => p.ReadAll()).ToArray();
            }
            return Unpack(true, msgs, out planItem, null);
        }

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


        static bool UnpackAction(PlanItem item, byte type, byte[] bytes)
        {
            switch (type)
            {
                case ZeroFrameType.SubTitle:
                    item.SubTitle = GetString(bytes);
                    return true;
                case ZeroFrameType.Plan:
                    //BUG
                    var json = GetString(bytes);
                    item.Plan = JsonConvert.DeserializeObject<ZeroPlan>(json);
                    return true;
                case ZeroFrameType.TextContent:
                    item.Content = GetString(bytes);
                    return true;
                case ZeroFrameType.ResultText:
                    item.CallResultJson = GetString(bytes);
                    return true;
                case ZeroFrameType.Status:
                    item.Status = bytes;
                    return true;
                case ZeroFrameType.BinaryContent:
                    item.Buffer = bytes;
                    return true;
                case ZeroFrameType.TsonContent:
                    item.Tson = bytes;
                    return true;
            }
            return false;
        }
    }
}