using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.PubSub;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     广播扩展
    /// </summary>
    public static class ZeroPublishExtend
    {
        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionTson =
        {
            6,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.TsonValue,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionJson =
        {
            6,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.TextContent,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };
        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionData =
        {
            6,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.BinaryValue,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionTson2 =
        {
            7,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.RequestId,
            ZeroFrameType.SubTitle,
            ZeroFrameType.Publisher,
            ZeroFrameType.TsonValue,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionJson2 =
        {
            7,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.RequestId,
            ZeroFrameType.SubTitle,
            ZeroFrameType.Publisher,
            ZeroFrameType.TextContent,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };
        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionData2 =
        {
            7,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.RequestId,
            ZeroFrameType.SubTitle,
            ZeroFrameType.Publisher,
            ZeroFrameType.BinaryValue,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static readonly byte[] PubDescriptionEmpty =
        {
            5,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };


        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish<T>(this ZSocket socket, T content)
            where T : class, IPublishData
        {
            return content != null && Publish(socket, PubDescriptionJson, content.Title, content.ToZeroBytes());
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish<T>(this ZSocket socket, string title, T content) where T : class
        {
            return content == null
                ? Publish(socket, title)
                : Publish(socket, PubDescriptionJson, title, content.ToZeroBytes());
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish<T>(this ZSocket socket, string title, string subTitle, T content)
            where T : class
        {
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(T));
            byte[] buffer;
            using (MemoryStream ms = new MemoryStream())
            {
                js.WriteObject(ms, content);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                buffer = ms.ToArray();
            }
            return subTitle == null
                ? Publish(socket, PubDescriptionData, title, buffer)
                : Publish(socket, PubDescriptionData2, title, subTitle, buffer);
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="item"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish(this ZSocket socket, PublishItem item)
        {
            if (item.Tson != null)
            {
                return item.SubTitle == null
                    ? Publish(socket, PubDescriptionTson, item.Title, item.Tson)
                    : Publish(socket, PubDescriptionTson2, item.Title, item.SubTitle, item.Tson);
            }
            if (item.Buffer != null)
            {
                return item.SubTitle == null
                    ? Publish(socket, PubDescriptionData, item.Title, item.Buffer)
                    : Publish(socket, PubDescriptionData2, item.Title, item.SubTitle, item.Buffer);
            }
            return item.SubTitle == null
                ? Publish(socket, PubDescriptionJson, item.Title, item.Content.ToZeroBytes())
                : Publish(socket, PubDescriptionJson2, item.Title, item.SubTitle, item.Content.ToZeroBytes());
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish(this ZSocket socket, string title, string subTitle, string content)
        {
            return Publish(socket, PubDescriptionJson2, title, subTitle, content.ToZeroBytes());
        }


        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish(this ZSocket socket, string title)
        {
            if (socket == null)
                return false;
            try
            {
                if (!socket.SendTo(PubDescriptionEmpty,
                    title.ToZeroBytes(),
                    GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                    ZeroApplication.Config.RealName.ToZeroBytes(),
                    GlobalContext.Current.ToZeroBytes(),
                    GlobalContext.ServiceKey.ToZeroBytes()))
                {
                    ZeroTrace.WriteError("Pub", socket.LastError.Text, socket.Endpoint, title);
                    return false;
                }
                var result = socket.ReceiveString();
                return result.InteractiveSuccess && result.State == ZeroOperatorStateType.Ok;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Pub", e, socket.Endpoint, $"Socket Ptr:{socket.SocketPtr}");
                return false;
            }
        }
        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish(this ZSocket socket, string title, string subTitle, byte[] content)
        {
            return Publish(socket, PubDescriptionJson2, title, subTitle, content);
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="description"></param>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish(this ZSocket socket, byte[] description, string title, string subTitle, byte[] content)
        {
            if (socket == null)
                return false;
            try
            {
                if (!socket.SendTo(description,
                    title.ToZeroBytes(),
                    GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                    subTitle.ToZeroBytes(),
                    ZeroApplication.Config.RealName.ToZeroBytes(),
                    content,
                    GlobalContext.Current.ToZeroBytes(),
                    GlobalContext.ServiceKey.ToZeroBytes()))
                {
                    ZeroTrace.WriteError("Pub", socket.LastError.Text, socket.Endpoint, title, subTitle);
                    return false;
                }
                var result = socket.ReceiveString();
                return result.InteractiveSuccess && result.State == ZeroOperatorStateType.Ok;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Pub", e, socket.Endpoint, $"Socket Ptr:{socket.SocketPtr}");
                return false;
            }
        }


        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="description"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish(this ZSocket socket, byte[] description, string title, byte[] content)
        {
            if (socket == null)
                return false;
            try
            {
                if (!socket.SendTo(description,
                    title.ToZeroBytes(),
                    GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                    ZeroApplication.Config.RealName.ToZeroBytes(),
                    content,
                    GlobalContext.Current.ToZeroBytes(),
                    GlobalContext.ServiceKey.ToZeroBytes()))
                {
                    ZeroTrace.WriteError("Pub", socket.LastError.Text, socket.Endpoint, title);
                    return false;
                }
                var result = socket.ReceiveString();
                return result.InteractiveSuccess && result.State == ZeroOperatorStateType.Ok;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Pub", e, socket.Endpoint, $"Socket Ptr:{socket.SocketPtr}");
                return false;
            }
        }

        /// <summary>
        ///     广播消息解包
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="item"></param>
        /// <param name="showError"></param>
        /// <returns></returns>
        public static bool Unpack(this ZMessage msg, out PublishItem item, bool showError = true)
        {
            if (msg == null)
            {
                item = null;
                return false;
            }

            byte[][] msgs;
            using (msg)
                msgs = msg.Select(p => p.Read()).ToArray();
            try
            {
                if (msgs.Length < 3)
                {
                    item = null;
                    return false;
                }
                var description = msgs[1];
                if (description.Length < 2)
                {
                    item = null;
                    return false;
                }

                int end = description[0] + 2;
                if (end != msgs.Length)
                {
                    item = null;
                    return false;
                }

                item = new PublishItem
                {
                    Title = Encoding.UTF8.GetString(msgs[0]),
                    State = (ZeroOperatorStateType)description[1],
                    ZeroEvent = (ZeroNetEventType)description[1]
                };

                for (int idx = 2; idx < end; idx++)
                {
                    var bytes = msgs[idx];
                    if (bytes.Length == 0)
                        continue;
                    switch (description[idx])
                    {
                        case ZeroFrameType.SubTitle:
                            item.SubTitle = Encoding.UTF8.GetString(bytes);
                            break;
                        case ZeroFrameType.Context:
                            item.ContextJson = Encoding.UTF8.GetString(bytes);
                            break;
                        case ZeroFrameType.GlobalId:
                            item.GlobalId = Encoding.UTF8.GetString(bytes);
                            break;
                        case ZeroFrameType.CallId:
                            item.CallId = Encoding.UTF8.GetString(bytes);
                            break;
                        case ZeroFrameType.Station:
                            item.Station = Encoding.UTF8.GetString(bytes);
                            break;
                        case ZeroFrameType.Publisher:
                            item.Publisher = Encoding.UTF8.GetString(bytes);
                            break;
                        case ZeroFrameType.TextContent:
                            if (item.Content == null)
                                item.Content = Encoding.UTF8.GetString(bytes);
                            else
                                item.Values.Add(Encoding.UTF8.GetString(bytes));
                            break;
                        case ZeroFrameType.BinaryValue:
                            item.Buffer = bytes;
                            break;
                        case ZeroFrameType.TsonValue:
                            item.Tson = bytes;
                            break;
                        default:
                            item.Values.Add(Encoding.UTF8.GetString(bytes));
                            break;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Unpack", e);
                item = null;
                return false;
            }
        }
    }
}