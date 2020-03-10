using System;
using Agebull.Common.Context;
using Agebull.MicroZero.ZeroApis;
using ZeroMQ;

namespace Agebull.MicroZero.PubSub
{
    /// <summary>
    ///     消息发布
    /// </summary>
    public static class ZeroPublisher
    {
        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete]
        public static bool DoPublish(string station, string title, string value)
        {
            return Publish(station, title, null, value);
        }

        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete]
        public static bool DoPublish<T>(string station, string title, T value)
            where T : class
        {
            return Publish(station, title, null,value == default(T) ? "{}" : JsonHelper.SerializeObject(value));
        }

        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="sub"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete]
        public static bool DoPublish(string station, string title, string sub, string value)
        {
            return Publish(station, title, sub, value);
        }

        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="sub"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete]
        public static bool DoPublish<T>(string station, string title, string sub, T value)
            where T : class
        {
            return Publish(station, title, sub, value == default(T) ? "{}" : JsonHelper.SerializeObject(value));
        }


        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="sub"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Publish<T>(string station, string title, string sub, T value)
            where T : class
        {
            return Publish(station, title, sub, value == default(T) ? "{}" : JsonHelper.SerializeObject(value));
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public static bool Publish<T>(string station, string title, T content) where T : class
        {
            return content == null
                ? Publish(station, title)
                : PublishInner(station, PubDescriptionJson, title, content.ToZeroBytes());
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="item"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public static bool Publish(string station, PublishItem item)
        {
            if (item.Tson != null)
            {
                return PublishInner(station, PubDescriptionTson, item.Title, item.SubTitle, item.Tson);
            }
            if (item.Buffer != null)
            {
                return PublishInner(station, PubDescriptionBin, item.Title, item.SubTitle, item.Buffer);
            }
            return PublishInner(station, PubDescriptionJson, item.Title, item.SubTitle, item.Content.ToZeroBytes());
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public static bool Publish(string station, string title)
        {
            return PublishInner(station, PubDescriptionJson, title, null, null);
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public static bool Publish(string station, string title,string content)
        {
            return PublishInner(station, PubDescriptionJson, title, null, content.ToZeroBytes());
        }


        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <param name="content"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public static bool Publish(string station, string title, string subTitle, string content)
        {
            return PublishInner(station, PubDescriptionJson, title, subTitle, content.ToZeroBytes());
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="description"></param>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <param name="content"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public static bool Publish(string station, byte[] description, string title, string subTitle, byte[] content)
        {
            var result = PublishInner(station, title, description, title.ToZeroBytes(), subTitle.ToZeroBytes(), content);
            return result.InteractiveSuccess && result.State == ZeroOperatorStateType.Ok;
        }


        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="sub"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ZeroResult Send<T>(string station, string title, string sub, T value)
            where T : class
        {
            return PublishInner(station, title, PubDescriptionJson, title.ToZeroBytes(), sub.ToZeroBytes(), value.ToZeroBytes());
        }
        #region Frames

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="description"></param>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <param name="content"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        private static bool PublishInner(string station, byte[] description, string title, string subTitle, byte[] content)
        {
            var result = PublishInner(station, title, description, title.ToZeroBytes(), subTitle.ToZeroBytes(), content);
            return result.InteractiveSuccess && result.State == ZeroOperatorStateType.Ok;
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="description"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        private static bool PublishInner(string station, byte[] description, string title, byte[] content)
        {
            var result = PublishInner(station, title, description, title.ToZeroBytes(), content);
            return result.InteractiveSuccess && result.State == ZeroOperatorStateType.Ok;
        }


        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static ZeroResult Publish(string station, params byte[][] frames)
        {
            try
            {
                var socket = ApiProxy.GetSocket(station);
                if (socket == null)
                {
                    return new ZeroResult
                    {
                        State = ZeroOperatorStateType.NetError,
                        ErrorMessage = "站点不存在"
                    };
                }
                ZError err;
                using (socket)
                {
                    using (var message = new ZMessage(frames))
                    {
                        message.Insert(0, new ZFrame(station.ToZeroBytes()));
                        if (socket.Send(message, out err))
                            return socket.ReceiveString();
                    }
                }
                ZeroTrace.WriteError("Pub", socket.Endpoint, socket.LastError?.Text);
                return new ZeroResult
                {
                    State = ZeroOperatorStateType.NetError,
                    ErrorMessage = err.Text
                };
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Pub", e, station);
                return new ZeroResult
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e,
                    ErrorMessage = e.Message
                };
            }
        }


        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        private static ZeroResult PublishInner(string station, string title, params byte[][] frames)
        {
            try
            {
                var socket = ApiProxy.GetSocket(station);
                if (socket == null)
                {
                    return new ZeroResult
                    {
                        State = ZeroOperatorStateType.NetError,
                        ErrorMessage = "站点不存在"
                    };
                }

                using (socket)
                {
                    using (var message = new ZMessage(frames)
                    {
                        new ZFrame(GlobalContext.CurrentNoLazy?.ToZeroBytes()),
                        new ZFrame(GlobalContext.CurrentNoLazy?.Request.LocalGlobalId.ToZeroBytes()),
                        new ZFrame(GlobalContext.CurrentNoLazy?.Request.RequestId.ToZeroBytes()),
                        new ZFrame(socket.Identity)
                    })
                    {
                        //message.Insert(0, new ZFrame(title.ToZeroBytes()));
                        message.Insert(0, new ZFrame(station.ToZeroBytes()));
                        if (socket.SendByServiceKey(message))
                            return socket.ReceiveString();
                    }
                }
                ZeroTrace.WriteError("Pub", title, socket.Endpoint, socket.LastError?.Text);
                return new ZeroResult
                {
                    State = ZeroOperatorStateType.NetError,
                    ErrorMessage = socket.LastError?.Text
                };
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Pub", e, station, title);
                return new ZeroResult
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e,
                    ErrorMessage = e.Message
                };
            }
        }

        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionTson =
        {
            8,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.SubTitle,
            ZeroFrameType.TsonContent,
            ZeroFrameType.Context,
            ZeroFrameType.CallId,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionJson =
        {
            8,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.SubTitle,
            ZeroFrameType.TextContent,
            ZeroFrameType.Context,
            ZeroFrameType.CallId,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };
        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionBin =
        {
            8,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.SubTitle,
            ZeroFrameType.BinaryContent,
            ZeroFrameType.Context,
            ZeroFrameType.CallId,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        #endregion
    }
}
/*


        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static bool SendPublish(string station, params byte[][] frames)
        {
            return socket.SendByExtend(frames,
                GlobalContext.CurrentNoLazy?.Request.RequestId.ToZeroBytes(),
                GlobalContext.ServiceRealName.ToZeroBytes(),
                GlobalContext.CurrentNoLazy?.Request.LocalGlobalId.ToZeroBytes(),
                GlobalContext.CurrentNoLazy.ToZeroBytes(),
                ZeroCommandExtend.ServiceKeyBytes);
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static ZeroResult Publish(string station, string title, params byte[][] frames)
        {
            try
            {
                if (socket.SendTo(frames))
                    return socket.ReceiveString();
                ZeroTrace.WriteError("Pub", title, socket.LastError.Text, socket.Endpoint);
                return new ZeroResult
                {
                    State = ZeroOperatorStateType.NetError,
                    ErrorMessage = socket.LastError.Text
                };
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Pub", e, title, socket.Endpoint, $"Socket Ptr:{socket.SocketPtr}");
                return new ZeroResult
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e,
                    ErrorMessage = e.Message
                };
            }
        }
 */
