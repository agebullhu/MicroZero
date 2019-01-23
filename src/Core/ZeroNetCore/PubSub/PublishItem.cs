using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Agebull.ZeroNet.Core;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.ZeroNet.PubSub
{
    /// <summary>
    ///     广播节点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PublishItem : ZeroNetMessage
    {

        /// <summary>
        /// 是否非正常值
        /// </summary>
        [JsonProperty]
        public bool IsBadValue { get; set; }


        /// <summary>
        /// 带外事件
        /// </summary>
        [JsonIgnore]
        public ZeroNetEventType ZeroEvent => (ZeroNetEventType)InnerCommand;

        /// <summary>
        ///  发布者的Identity(可能已消失)
        /// </summary>
        [JsonProperty]
        public string Publisher => Requester;

        /// <summary>
        ///     主标题
        /// </summary>
        [JsonProperty]
        public string Title { get; set; }

        /// <summary>
        ///     副标题
        /// </summary>
        [JsonProperty]
        public string SubTitle { get; set; }

        /// <summary>
        ///     内容
        /// </summary>
        [JsonProperty]
        public byte[] Buffer { get; set; }

        /// <summary>
        ///     内容
        /// </summary>
        [JsonProperty]
        public byte[] Status { get; set; }

        /// <summary>
        ///     内容
        /// </summary>
        [JsonProperty]
        public byte[] Tson { get; set; }

        /// <summary>
        ///     广播消息解包
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="publishItem"></param>
        /// <returns></returns>
        public static bool Unpack(ZMessage msg, out PublishItem publishItem)
        {
            return Unpack(msg, out publishItem, UnpackAction);
        }

        /// <summary>
        ///     广播消息解包
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="publishItem"></param>
        /// <returns></returns>
        public static bool Unpack2<TPublishItem>(ZMessage msg, out TPublishItem publishItem)
            where TPublishItem : PublishItem, new()
        {
            if (msg == null)
            {
                publishItem = null;
                return false;
            }

            byte[][] msgs;
            using (msg)
            {
                msgs = msg.Select(p => p.ReadAll()).ToArray();
            }

            for (var index = 2; index < msgs.Length; index++)
            {
                var b = msgs[1][index];
                Debug.WriteLine($"{index}:{(int)b}|{ZeroFrameType.FrameName(b)}\r\n{GetString(msgs[index])}");
            }

            try
            {
                return Unpack(false, msgs, out publishItem, UnpackAction);
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Unpack", e);
                publishItem = null;
                return false;
            }
        }

        /// <summary>
        ///     广播消息解包
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="publishItem"></param>
        /// <returns></returns>
        public static bool Unpack<TPublishItem>(ZMessage msg, out TPublishItem publishItem)
            where TPublishItem : PublishItem, new()
        {
            return Unpack(msg, out publishItem, UnpackAction);
        }

        /// <summary>
        ///     广播消息解包
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="publishItem"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        protected static bool Unpack<TPublishItem>(ZMessage msg, out TPublishItem publishItem, Action<TPublishItem, byte, byte[]> action)
            where TPublishItem : PublishItem, new()
        {
            if (msg == null)
            {
                publishItem = null;
                return false;
            }

            byte[][] msgs;
            using (msg)
            {
                msgs = msg.Select(p => p.ReadAll()).ToArray();
            }
            try
            {
                return Unpack(false, msgs, out publishItem, action);
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Unpack", e);
                publishItem = null;
                return false;
            }
        }

        static void UnpackAction(PublishItem item, byte type, byte[] bytes)
        {
            switch (type)
            {
                case ZeroFrameType.PubTitle:
                    item.Title = GetString(bytes);
                    break;
                case ZeroFrameType.SubTitle:
                    item.SubTitle = GetString(bytes);
                    break; 
                case ZeroFrameType.TextContent:
                    item.Content = GetString(bytes);
                    break;
                case ZeroFrameType.BinaryContent:
                    item.Buffer = bytes;
                    break;
                case ZeroFrameType.TsonContent:
                    item.Tson = bytes;
                    break;
            }
        }
    }
}