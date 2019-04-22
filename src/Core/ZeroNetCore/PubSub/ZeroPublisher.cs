using System;
using Agebull.Common.Context;
using Agebull.EntityModel.Common;
using Newtonsoft.Json;
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
        public static bool DoPublish(string station, string title, string value)
        {
            return DoPublishInner(station, title, null, value);
        }
        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DoPublish<T>(string station, string title, T value)
            where T : class
        {
            return DoPublishInner(station, title, null, 
                value == default(T) ? "{}" : JsonHelper.SerializeObject(value));
        }

        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="sub"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DoPublish(string station, string title, string sub, string value)
        {
            return DoPublishInner(station, title, sub, value);
        }
        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="sub"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DoPublish<T>(string station, string title, string sub, T value)
            where T : class
        {
            return DoPublishInner(station, title, sub, 
                value == default(T) ? "{}" : JsonHelper.SerializeObject(value));
        }

        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="sub"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool DoPublishInner(string station, string title, string sub, string value)
        {
            var socket = ZeroConnectionPool.GetSocket(station, RandomOperate.Generate(8));
            if (socket?.Socket == null)
            {
                return false;
            }
            using (socket)
            {
                return Publish(socket.Socket, title, sub, value);
            }
        }

        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionTson =
        {
            7,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.TsonContent,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.CallId,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionJson =
        {
            7,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.TextContent,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.CallId,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };
        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionBin =
        {
            7,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.BinaryContent,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.CallId,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionTson2 =
        {
            8,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.SubTitle,
            ZeroFrameType.TsonContent,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.CallId,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionJson2 =
        {
            8,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.SubTitle,
            ZeroFrameType.TextContent,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.CallId,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };
        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionBin2 =
        {
            8,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.SubTitle,
            ZeroFrameType.BinaryContent,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.CallId,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static readonly byte[] PubDescriptionEmpty =
        {
            6,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.CallId,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

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
            return subTitle == null
                ? Publish(socket, PubDescriptionJson, title, content.ToZeroBytes())
                : Publish(socket, PubDescriptionJson2, title, subTitle, content.ToZeroBytes());
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
                    ? Publish(socket, PubDescriptionBin, item.Title, item.Buffer)
                    : Publish(socket, PubDescriptionBin2, item.Title, item.SubTitle, item.Buffer);
            }
            return item.SubTitle == null
                ? Publish(socket, PubDescriptionJson, item.Title, item.Content.ToZeroBytes())
                : Publish(socket, PubDescriptionJson2, item.Title, item.SubTitle, item.Content.ToZeroBytes());
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
            var result = PublishInner(socket, title, PubDescriptionEmpty, title.ToZeroBytes());
            return result.InteractiveSuccess && result.State == ZeroOperatorStateType.Ok;
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
            var result = PublishInner(socket, title, description, title.ToZeroBytes(), subTitle.ToZeroBytes(), content);
            return result.InteractiveSuccess && result.State == ZeroOperatorStateType.Ok;
        }


        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="description"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        private static bool Publish(ZSocket socket, byte[] description, string title, byte[] content)
        {
            if (socket == null)
                return false;
            var result = PublishInner(socket, title, description, title.ToZeroBytes(), content);
            return result.InteractiveSuccess && result.State == ZeroOperatorStateType.Ok;
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="title"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        private static ZeroResult PublishInner(ZSocket socket, string title, params byte[][] frames)
        {
            try
            {
                if (socket.SendByExtend(frames,
                    GlobalContext.CurrentNoLazy?.Request.RequestId.ToZeroBytes(),
                    GlobalContext.ServiceRealName.ToZeroBytes(),
                    GlobalContext.CurrentNoLazy?.Request.LocalGlobalId.ToZeroBytes(),
                    GlobalContext.CurrentNoLazy.ToZeroBytes(),
                    ZeroCommandExtend.ServiceKeyBytes))
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

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static bool SendPublish(this ZSocket socket, params byte[][] frames)
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
        /// <param name="socket"></param>
        /// <param name="title"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static ZeroResult Publish(this ZSocket socket, string title, params byte[][] frames)
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


        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static ZeroResult PublishNoGlobalId(this ZSocket socket, string title, string subTitle, string content)
        {
            return PublishNoGlobalId(socket, title, subTitle, content.ToZeroBytes());
        }
        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static ZeroResult PublishNoGlobalId<T>(this ZSocket socket, string title, string subTitle, T content)
            where T : class
        {
            return PublishNoGlobalId(socket, title, subTitle, content.ToZeroBytes());
        }

        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        private static readonly byte[] DescriptionNoGlobalId =
        {
            6,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.SubTitle,
            ZeroFrameType.TextContent,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.Station,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };
        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        private static ZeroResult PublishNoGlobalId(this ZSocket socket, string title, string subTitle, byte[] content)
        {
            if (socket == null)
                return new ZeroResult
                {
                    State = ZeroOperatorStateType.ArgumentInvalid,
                    ErrorMessage = "参数错误"
                };
            return PublishInner(socket, title, DescriptionNoGlobalId,
                    subTitle.ToZeroBytes(),
                    content,
                    GlobalContext.CurrentNoLazy?.Request.RequestId.ToZeroBytes(),
                    ZeroCommandExtend.AppNameBytes,
                    ZeroCommandExtend.AppNameBytes,
                    ZeroCommandExtend.ServiceKeyBytes);
        }
    }
}
