using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.MicroZero.ZeroApis
{

    /// <summary>
    /// Api调用节点
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class ApiCallItem : ZeroNetMessage
    {
        /// <summary>
        /// 全局ID
        /// </summary>
        internal List<IApiHandler> Handlers { get; set; }

        /// <summary>
        /// 站点请求ID(队列使用)
        /// </summary>
        public string LocalId { get; set; }

        /// <summary>
        /// 请求者
        /// </summary>
        [JsonIgnore]
        public byte[] Caller => Head;

        /// <summary>
        /// 请求者
        /// </summary>
        [JsonProperty("Caller")]
        public string CallerName => Head == null || Head.Length == 0 ? "" : GetString(Head);

        /// <summary>
        /// 广播标题
        /// </summary>
        [JsonProperty("Title")]
        public string Title => Head == null || Head.Length == 0 ? "" : GetString(Head);

        /// <summary>
        /// 命令
        /// </summary>
        public string Command => CommandOrSubTitle;

        /// <summary>
        /// API名称
        /// </summary>
        public string ApiName => Command;

        /// <summary>
        /// 请求参数
        /// </summary>
        public string Argument { get; set; }

        /// <summary>
        /// 返回
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 执行状态
        /// </summary>
        public UserOperatorStateType Status { get; set; }

        /// <summary>
        /// 原样帧
        /// </summary>
        public Dictionary<byte, byte[]> Originals { get; } = new Dictionary<byte, byte[]>();


        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="callItem"></param>
        /// <returns></returns>
        public static bool Unpack(ZMessage messages, out ApiCallItem callItem)
        {
            if (messages.Count == 0)
            {
                callItem = null;
                return false;
            }

            byte[][] buffers;
            using (messages)
                buffers = messages.Select(p => p.ReadAll()).ToArray();
            try
            {
                if (!Unpack(false, buffers, out callItem, (item, type, bytes) =>
                {
                    switch (type)
                    {
                        case ZeroFrameType.LocalId:
                            item.LocalId = GetString(bytes);
                            break;
                        case ZeroFrameType.Argument:
                            item.Argument = GetString(bytes);
                            break;
                        case ZeroFrameType.Original1:
                        case ZeroFrameType.Original2:
                        case ZeroFrameType.Original3:
                        case ZeroFrameType.Original4:
                        case ZeroFrameType.Original5:
                        case ZeroFrameType.Original6:
                        case ZeroFrameType.Original7:
                        case ZeroFrameType.Original8:
                            if (!item.Originals.ContainsKey(type))
                                item.Originals.Add(type, bytes);
                            break;
                    }
                }))
                    return false;
                return callItem.ApiName != null;// && item.GlobalId != null;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, $"FrameSize{buffers.Length}");
                callItem = new ApiCallItem
                {
                    State = ZeroOperatorStateType.FrameInvalid
                };
                return false;
            }
        }
    }
}