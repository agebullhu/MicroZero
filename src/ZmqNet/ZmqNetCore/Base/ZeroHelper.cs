using System;
using System.Linq;
using System.Text;
using Agebull.Common.Logging;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     Zmq帮助类
    /// </summary>
    public static class ZeroHelper
    {

        #region 调用支持

        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="desicription"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ZeroResultData QuietSend(this ZSocket socket, byte[] desicription, params string[] args)
        {
            var message = new ZMessage();
            var frame = new ZFrame(desicription);
            message.Add(frame);
            if (args != null)
            {
                foreach (var arg in args)
                {
                    message.Add(new ZFrame((arg ?? "").ToUtf8Bytes()));
                }
            }
            using (message)
            {
                try
                {
                    if (!socket.SendTo(message))
                    {
                        return new ZeroResultData
                        {
                            State = ZeroOperatorStateType.LocalRecvError,
                            ZmqError = socket.LastError
                        };
                    }
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    return new ZeroResultData
                    {
                        State = ZeroOperatorStateType.LocalException,
                        Exception = e,
                        ZmqError = socket.LastError
                    };
                }
            }

            return new ZeroResultData
            {
                State = ZeroOperatorStateType.Ok,
                InteractiveSuccess = true
            };
        }
        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="desicription"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ZeroResultData SendTo(this ZSocket socket, byte[] desicription, params string[] args)
        {
            using (var frames = new ZMessage
            {
                new ZFrame(desicription)
            })
            {
                if (args != null)
                {
                    foreach (var arg in args)
                    {
                        frames.Add(new ZFrame((arg ?? "").ToUtf8Bytes()));
                    }
                }

                try
                {
                    if (!socket.SendTo(frames))
                    {
#if DEBUG
                        ZeroTrace.WriteError("SendTo", /*error.Text,*/ socket.Connects.LinkToString(','),
                            $"Socket Ptr:{socket.SocketPtr}");
#endif
                        return new ZeroResultData
                        {
                            State = ZeroOperatorStateType.LocalRecvError,
                            ZmqError = socket.LastError
                        };
                    }
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException("SendTo", e, socket.Connects.LinkToString(','),
                        $"Socket Ptr:{socket.SocketPtr}");
                    return new ZeroResultData
                    {
                        State = ZeroOperatorStateType.LocalException,
                        Exception = e,
                        ZmqError = socket.LastError
                    };
                }
            }

            return new ZeroResultData
            {
                State = ZeroOperatorStateType.Ok,
                InteractiveSuccess = true
            };
        }

        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="desicription"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ZeroResultData Call(this ZSocket socket, byte[] desicription, params string[] args)
        {
            var result = SendTo(socket, desicription, args);
            return !result.InteractiveSuccess ? result : socket.ReceiveString();
        }

        #endregion

        #region 广播支持

        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static readonly byte[] PubDescription =
        {
            5,
            ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.SubTitle,
            ZeroFrameType.Content,
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
            return Publish(socket, content.Title, null, JsonConvert.SerializeObject(content));
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
            return Publish(socket, title, subTitle, JsonConvert.SerializeObject(content));
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
            return Publish(socket, title, null, JsonConvert.SerializeObject(content));
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="item"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish(this ZSocket socket, PublishItem item)
        {
            return Publish(socket, item.Title, item.SubTitle, item.Content);
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
            if (socket == null)
                return false;
            try
            {
                using (var frames = new ZMessage
                {
                    new ZFrame(PubDescription),
                    new ZFrame((title ?? "").ToUtf8Bytes()),
                    new ZFrame(ApiContext.RequestContext.RequestId.ToUtf8Bytes()),
                    new ZFrame(ZeroApplication.Config.RealName.ToUtf8Bytes()),
                    new ZFrame((subTitle ?? "").ToUtf8Bytes()),
                    new ZFrame((content ?? "").ToUtf8Bytes())
                })
                {
                    if (!socket.SendTo(frames))
                    {
                        ZeroTrace.WriteError("Pub", socket.LastError.Text, socket.Connects.LinkToString(','), title, subTitle);
                        return false;
                    }
                }

                var result = socket.ReceiveString();
                return result.InteractiveSuccess && result.State == ZeroOperatorStateType.Ok;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Pub", e, socket.Connects.LinkToString(','), $"Socket Ptr:{socket.SocketPtr}");
                return false;
            }
        }

        /// <summary>
        ///     接收广播
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <param name="showError"></param>
        /// <returns></returns>
        public static bool Subscribe(this ZSocket socket, out PublishItem item, bool showError = true)
        {
            ZMessage messages;
            try
            {
                if (!socket.Recv(out messages))
                {
                    if (socket.LastError.Number != 11 && showError)
                        ZeroTrace.WriteError("Sub", socket.LastError.Text, socket.Connects.LinkToString(','));
                    item = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Sub", e, socket.Connects.LinkToString(','));
                item = null;
                return false;
            }

            return Unpack(messages, out item, showError);
        }

        /// <summary>
        ///     广播消息解包
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="item"></param>
        /// <param name="showError"></param>
        /// <returns></returns>
        public static bool Unpack(this ZMessage messages, out PublishItem item, bool showError = true)
        {
            if (messages == null)
            {
                item = null;
                return false;
            }
            try
            {
                if (messages.Count < 3)
                {
                    item = null;
                    return false;
                }
                var description = messages[1].Read();
                if (description.Length < 2)
                {
                    item = null;
                    return false;
                }

                int end = description[0] + 2;
                if (end != messages.Count)
                {
                    item = null;
                    return false;
                }

                item = new PublishItem
                {
                    Title = messages[0].ReadString(),
                    State = (ZeroOperatorStateType)description[1],
                    ZeroEvent = (ZeroNetEventType)description[1]
                };

                for (int idx = 2; idx < end; idx++)
                {
                    var bytes = messages[idx].Read();
                    if (bytes.Length == 0)
                        continue;
                    var val = Encoding.UTF8.GetString(bytes);
                    switch (description[idx])
                    {
                        case ZeroFrameType.SubTitle:
                            item.SubTitle = val;
                            break;
                        case ZeroFrameType.Station:
                            item.Station = val;
                            break;
                        case ZeroFrameType.Publisher:
                            item.Publisher = val;
                            break;
                        case ZeroFrameType.Content:
                            if (item.Content == null)
                                item.Content = val;
                            else
                                item.Values.Add(val);
                            break;
                        default:
                            item.Values.Add(val);
                            break;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Sub", e);
                item = null;
                return false;
            }
            finally
            {
                messages.Dispose();
            }
        }
        #endregion

        #region 接收支持

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="showError">是否显示错误</param>
        /// <returns></returns>
        public static ZeroResultData ReceiveString(this ZSocket socket, bool showError = false)
        {

            ZMessage messages;
            try
            {
                if (!socket.Recv(out messages))
                {
                    if (showError)
                        ZeroTrace.WriteError("Receive", socket.Connects.LinkToString(','), socket.LastError.Text);
                    return new ZeroResultData
                    {
                        State = ZeroOperatorStateType.LocalRecvError,
                        ZmqError = socket.LastError
                    };
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Connects.LinkToString(','));
                return new ZeroResultData
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }
            return Unpack(messages, showError);
        }
        /// <summary>
        /// 命令解包
        /// </summary>
        /// <param name="showError"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static ZeroResultData Unpack(this ZMessage messages, bool showError = false)
        {
            try
            {
                var description = messages[0].Read();
                if (description.Length == 0)
                {
                    if (showError)
                        ZeroTrace.WriteError("Unpack", "LaoutError",
                            description.LinkToString(p => p.ToString("X2"), ""));
                    return new ZeroResultData
                    {
                        State = ZeroOperatorStateType.Invalid,
                        Message = "网络格式错误"
                    };
                }

                int end = description[0] + 1;
                if (end != messages.Count)
                {
                    if (showError)
                        ZeroTrace.WriteError("Unpack", "LaoutError",
                            $"FrameSize{messages.Count}", description.LinkToString(p => p.ToString("X2"), ""));
                    return new ZeroResultData
                    {
                        State = ZeroOperatorStateType.Invalid,
                        Message = "网络格式错误"
                    };
                }

                var result = new ZeroResultData
                {
                    InteractiveSuccess = true,
                    State = (ZeroOperatorStateType)description[1]
                };
                for (int idx = 1; idx < end; idx++)
                {
                    result.Add(description[idx + 1], Encoding.UTF8.GetString(messages[idx].Read()));
                }

                return result;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Unpack", e);
                return new ZeroResultData
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }
            finally
            {
                messages.Dispose();
            }
        }


        /// <summary>
        ///     接收字节
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static ZeroResultData<byte[]> Receive(this ZSocket socket)
        {
            ZMessage messages;
            try
            {
                if (!socket.Recv(out messages))
                {
                    return new ZeroResultData<byte[]>
                    {
                        State = ZeroOperatorStateType.LocalRecvError,
                        ZmqError = socket.LastError
                    };
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Connects.LinkToString(','), "Exception");
                return new ZeroResultData<byte[]>
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }

            try
            {
                var description = messages[0].Read();
                if (description.Length < 2)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", socket.Connects.LinkToString(','), description.LinkToString(p => p.ToString("X2"), ""));
                    return new ZeroResultData<byte[]>
                    {
                        State = ZeroOperatorStateType.Invalid,
                        Message = "网络格式错误"
                    };
                }

                int end = description[0] + 1;
                if (end != messages.Count)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", socket.Connects.LinkToString(','), $"FrameSize{messages.Count}", description.LinkToString(p => p.ToString("X2"), ""));
                    return new ZeroResultData<byte[]>
                    {
                        State = ZeroOperatorStateType.Invalid,
                        Message = "网络格式错误"
                    };
                }

                var result = new ZeroResultData<byte[]>
                {
                    InteractiveSuccess = true,
                    State = (ZeroOperatorStateType)description[1]
                };
                for (int idx = 1; idx < end; idx++)
                {
                    result.Add(description[idx + 1], messages[idx].Read());
                }

                return result;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Connects.LinkToString(','), $"FrameSize{messages.Count},Socket Ptr:{ socket.SocketPtr}.");
                return new ZeroResultData<byte[]>
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }
            finally
            {
                messages.Dispose();
            }
        }

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static ZeroResultData<byte[]> ReceiveUnknow(this ZSocket socket)
        {
            //tryCnt = 0;
            ZMessage messages;
            try
            {
                if (!socket.Recv(out messages))
                {
                    ZeroTrace.WriteError("Receive", socket.Connects.LinkToString(','), socket.LastError.Text);
                    return new ZeroResultData<byte[]>
                    {
                        State = ZeroOperatorStateType.LocalRecvError,
                        ZmqError = socket.LastError
                    };
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Connects.LinkToString(','));
                return new ZeroResultData<byte[]>
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }

            try
            {
                var result = new ZeroResultData<byte[]>
                {
                    InteractiveSuccess = true,
                    State = ZeroOperatorStateType.Ok
                };
                foreach (var frame in messages)
                {
                    result.Add(ZeroFrameType.BinaryValue, frame.Read());
                }

                return result;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Connects.LinkToString(','), $"FrameSize{messages.Count}, Socket Ptr:{ socket.SocketPtr}.");
                return new ZeroResultData<byte[]>
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }
            finally
            {
                messages.Dispose();
            }
        }

        #endregion

    }
}