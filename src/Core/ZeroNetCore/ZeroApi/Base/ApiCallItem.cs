using System;
using System.Collections.Generic;
using System.Linq;
using Agebull.EntityModel.Common;
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
        /// 请求参数
        /// </summary>
        public string Extend { get; set; }

        /// <summary>
        ///     文件
        /// </summary>
        public List<NameValue<string, byte[]>> Files = new List<NameValue<string, byte[]>>();

        /// <summary>
        /// 返回
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 说明帧结束标识
        /// </summary>
        public byte EndTag { get; set; }

        /// <summary>
        /// 扩展的二进制
        /// </summary>
        public byte[] Binary { get; set; }

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

                if (!Unpack(false, buffers, out callItem, OnFrameRead))
                    return false;
                return callItem.ApiName != null;// && item.GlobalId != null;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Unpack", e, $"FrameSize:{buffers.Length}");
                callItem = new ApiCallItem
                {
                    ZeroState = (byte)ZeroOperatorStateType.FrameInvalid
                };
                return false;
            }
        }

        static bool OnFrameRead(ApiCallItem item, byte type, byte[] bytes)
        {
            switch (type)
            {
                case ZeroFrameType.LocalId:
                    item.LocalId = GetString(bytes);
                    return true;
                case ZeroFrameType.Argument:
                    item.Argument = GetString(bytes);
                    return true;
                case ZeroFrameType.TextContent:
                    item.Extend = GetString(bytes);
                    return true;
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
                    return true;
                //ExtendText与BinaryContent成对出现
                case ZeroFrameType.ExtendText:
                    item.Files.Add(new NameValue<string, byte[]>
                    {
                        Name = GetString(bytes)
                    });
                    return true;
                case ZeroFrameType.BinaryContent:
                    if (item.Files.Count > 0)
                        item.Files.Last().Value = bytes;
                    else
                        item.ZeroState = (byte) ZeroOperatorStateType.FrameInvalid;
                    return true;
            }

            return false;
        }
    }
}