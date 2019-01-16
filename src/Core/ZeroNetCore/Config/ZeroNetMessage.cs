using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 一次传输的消息
    /// </summary>
    public class ZeroNetMessage
    {
        /// <summary>
        /// 逻辑操作状态
        /// </summary>
        public ZeroOperatorStateType State { get; set; }

        /// <summary>
        /// 帧数据类型
        /// </summary>
        public byte Tag { get; set; }

        /// <summary>
        ///     头帧
        /// </summary>
        [JsonProperty]
        public byte[] Head { get; set; }

        /// <summary>
        /// 内部简化命令
        /// </summary>
        [JsonIgnore]
        public ZeroByteCommand InnerCommand { get; set; }

        /// <summary>
        /// 请求还是返回
        /// </summary>
        [JsonIgnore]
        protected bool RequestOrResult { get; set; }

        /// <summary>
        /// 全局ID(本次)
        /// </summary>
        [JsonProperty]
        public string GlobalId { get; set; }

        /// <summary>
        /// 全局ID(调用方)
        /// </summary>
        [JsonProperty]
        public string CallId { get; set; }

        /// <summary>
        /// 请求ID
        /// </summary>
        [JsonProperty]
        public string RequestId { get; set; }

        /// <summary>
        ///  原始上下文的JSO内容
        /// </summary>
        public string ContextJson { get; set; }

        /// <summary>
        ///  调用的命令或广播子标题
        /// </summary>
        public string CommandOrSubTitle { get; set; }

        /// <summary>
        ///  请求者
        /// </summary>
        [JsonProperty]
        public string Requester { get; set; }


        /// <summary>
        ///  站点
        /// </summary>
        [JsonProperty]
        public string Station { get; set; }

        private byte[][] messages;

        /// <summary>
        /// 帧内容
        /// </summary>
        [JsonIgnore]
        public byte[][] Messages => messages;

        /// <summary>
        ///     内容
        /// </summary>
        [JsonIgnore]
        public IEnumerable<string> Values => _frames?.Values.Select(p => Encoding.UTF8.GetString(p.Data));

        /// <summary>
        ///     内容
        /// </summary>
        [JsonIgnore] internal Dictionary<int, ZeroFrameItem> _frames;

        /// <summary>
        /// 帧内容
        /// </summary>
        [JsonIgnore]
        public Dictionary<int, ZeroFrameItem> Frames
        {
            set => _frames = value;
            get => _frames ?? (_frames = new Dictionary<int, ZeroFrameItem>());
        }

        /// <summary>
        /// 加入数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void Add(byte type, byte[] value)
        {
            if (Frames.Count == 0)
            {
                Frames.Add(2, new ZeroFrameItem
                {
                    Type = type,
                    Data = value
                });
            }
            Frames.Add(Frames.Keys.Max() + 3, new ZeroFrameItem
            {
                Type = type,
                Data = value
            });
        }

        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">返回值</param>
        /// <returns>是否存在</returns>
        public bool TryGetValue(byte name, out byte[] value)
        {
            if (_frames?.Count == 0)
            {
                value = null;
                return false;
            }
            var vl = _frames.Values.FirstOrDefault(p => p.Type == name);
            value = vl.Data;
            return vl != null;
        }

        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="type">名称</param>
        /// <returns>存在返回值，不存在返回空对象</returns>
        public byte[] GetValue<TValue>(byte type)
        {
            if (_frames == null || _frames.Count == 0)
                return null;
            var vl = _frames.Values.FirstOrDefault(p => p.Type == type);
            return vl == null ? null : vl.Data;
        }

        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">返回值</param>
        /// <param name="parse"></param>
        /// <returns>是否存在</returns>
        public bool TryGetValue<TValue>(byte name, out TValue value, Func<byte[], TValue> parse)
        {
            if (_frames?.Count == 0)
            {
                value = default(TValue);
                return false;
            }
            var vl = _frames.Values.FirstOrDefault(p => p.Type == name);
            value = vl == null ? default(TValue) : parse(vl.Data);
            return vl != null;
        }

        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="parse"></param>
        /// <returns>存在返回值，不存在返回空对象</returns>
        public TValue GetValue<TValue>(byte name, Func<byte[], TValue> parse)
        {
            if (_frames == null || _frames.Count == 0)
                return default(TValue);
            var vl = _frames.Values.FirstOrDefault(p => p.Type == name);
            return vl == null ? default(TValue) : parse(vl.Data);
        }

        /// <summary>
        /// 显示到文本
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            if (_frames == null || Frames.Count <= 0) return text.ToString();
            foreach (var data in Frames.Values)
            {
                text.Append($" , [{ZeroFrameType.FrameName(data.Type)}] {data.Data}");
            }
            return text.ToString();
        }

        /// <summary>
        /// 解包
        /// </summary>
        /// <param name="isResult"></param>
        /// <param name="messages"></param>
        /// <param name="message"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static bool Unpack<TZeroMessage>(bool isResult, byte[][] messages, out TZeroMessage message, Action<TZeroMessage, byte, byte[]> action) where TZeroMessage : ZeroNetMessage, new()
        {
            message = new TZeroMessage
            {
                messages = messages
            };
            int move = isResult ? 1 : 0;
            byte[] description;
            if (isResult)
            {
                if (messages.Length == 0)
                {
                    message.State = ZeroOperatorStateType.FrameInvalid;
                    return false;
                }
                description = messages[0];
            }
            else
            {
                if (messages.Length < 2)
                {
                    message.State = ZeroOperatorStateType.FrameInvalid;
                    return false;
                }
                description = messages[1];
                message.Head = messages[0];
            }
            if (description.Length < 2)
            {
                message.State = ZeroOperatorStateType.FrameInvalid;
                return false;
            }
            int end = description[0] + 2;
            message.InnerCommand = (ZeroByteCommand)description[1];
            message.State = (ZeroOperatorStateType)description[1];
            int size = description[0] + 2;
            if (size < description.Length)
                message.Tag = description[size];
            message._frames = new Dictionary<int, ZeroFrameItem>();

            for (int idx = 2; idx < end && messages.Length > idx - move; idx++)
            {
                var bytes = messages[idx - move];
                message._frames.Add(idx - 2, new ZeroFrameItem
                {
                    Type = description[idx],
                    Data = bytes
                });
                if (bytes.Length == 0)
                    continue;
                switch (description[idx])
                {
                    case ZeroFrameType.GlobalId:
                        message.GlobalId = GetString(bytes);
                        break;
                    case ZeroFrameType.CallId:
                        message.CallId = GetString(bytes);
                        break;
                    case ZeroFrameType.RequestId:
                        message.RequestId = GetString(bytes);
                        break;
                    case ZeroFrameType.Requester:
                        message.Requester = GetString(bytes);
                        break;
                    case ZeroFrameType.Context:
                        message.ContextJson = GetString(bytes);
                        break;
                    case ZeroFrameType.Station:
                        message.Station = GetString(bytes);
                        break;
                    case ZeroFrameType.Command:
                        message.CommandOrSubTitle = GetString(bytes);
                        break;
                }
                action?.Invoke(message, description[idx], messages[idx]);
            }

            return true;
        }
        /// <summary>
        /// 取文本
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string GetString(byte[] bytes)
        {
            return bytes==null || bytes.Length == 0 ? null : Encoding.UTF8.GetString(bytes).Trim('\0');
        }
        /// <summary>
        /// 取文本
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        protected static long GetLong(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0 || !long.TryParse(Encoding.ASCII.GetString(bytes).Trim('\0'), out var l))
                return 0;
            return l;
        }
    }

    /// <summary>
    /// 帧节点
    /// </summary>
    public class ZeroFrameItem
    {
        /// <summary>
        /// 帧数据类型
        /// </summary>
        public byte Type { get; set; }
        /// <summary>
        /// 帧数据
        /// </summary>
        public byte[] Data { get; set; }
    }
}