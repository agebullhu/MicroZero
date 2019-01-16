using System;
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
            7,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.TsonValue,
            ZeroFrameType.Context,
            ZeroFrameType.CallId,
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
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.TextContent,
            ZeroFrameType.Context,
            ZeroFrameType.CallId,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };
        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionData =
        {
            7,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.RequestId,
            ZeroFrameType.Publisher,
            ZeroFrameType.BinaryValue,
            ZeroFrameType.Context,
            ZeroFrameType.CallId,
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
            ZeroFrameType.RequestId,
            ZeroFrameType.SubTitle,
            ZeroFrameType.Publisher,
            ZeroFrameType.TsonValue,
            ZeroFrameType.Context,
            ZeroFrameType.CallId,
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
            ZeroFrameType.RequestId,
            ZeroFrameType.SubTitle,
            ZeroFrameType.Publisher,
            ZeroFrameType.TextContent,
            ZeroFrameType.Context,
            ZeroFrameType.CallId,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };
        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        public static byte[] PubDescriptionData2 =
        {
            8,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.RequestId,
            ZeroFrameType.SubTitle,
            ZeroFrameType.Publisher,
            ZeroFrameType.BinaryValue,
            ZeroFrameType.Context,
            ZeroFrameType.CallId,
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
            ZeroFrameType.Context,
            ZeroFrameType.CallId,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

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
            return subTitle == null
                ? Publish(socket, PubDescriptionData, title, content.ToZeroBytes())
                : Publish(socket, PubDescriptionData2, title, subTitle, content.ToZeroBytes());
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
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool Publish(this ZSocket socket, string title)
        {
            if (socket == null)
                return false;
            var result = Publish(socket, title, PubDescriptionEmpty,
                title.ToZeroBytes(),
                GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                ZeroApplication.Config.RealName.ToZeroBytes(),
                GlobalContext.Current.ToZeroBytes(),
                GlobalContext.RequestInfo.LocalGlobalId.ToZeroBytes(),
                GlobalContext.ServiceKey.ToZeroBytes());
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
            var result = Publish(socket, title, description,
                title.ToZeroBytes(),
                GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                subTitle.ToZeroBytes(),
                ZeroApplication.Config.RealName.ToZeroBytes(),
                content,
                GlobalContext.Current.ToZeroBytes(),
                GlobalContext.RequestInfo.LocalGlobalId.ToZeroBytes(),
                GlobalContext.ServiceKey.ToZeroBytes());
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
            var result = Publish(socket, title, description,
                title.ToZeroBytes(),
                GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                ZeroApplication.Config.RealName.ToZeroBytes(),
                content,
                GlobalContext.Current.ToZeroBytes(),
                GlobalContext.RequestInfo.LocalGlobalId.ToZeroBytes(),
                GlobalContext.ServiceKey.ToZeroBytes());
            return result.InteractiveSuccess && result.State == ZeroOperatorStateType.Ok;
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="title"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        public static ZeroResult Publish(ZSocket socket, string title, params byte[][] frames)
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
        public static ZeroResult PublishByOrg(this ZSocket socket, string title, string subTitle, string content)
        {
            return PublishByOrg(socket, title, subTitle, content.ToZeroBytes());
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static ZeroResult PublishByOrg<T>(this ZSocket socket, string title, string subTitle, T content)
            where T : class
        {
            return PublishByOrg(socket, title, subTitle, content.ToZeroBytes());
        }

        /// <summary>
        ///     发送广播
        /// </summary>
        /// <param name="title"></param>
        /// <param name="subTitle"></param>
        /// <param name="content"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        private static ZeroResult PublishByOrg(this ZSocket socket, string title, string subTitle, byte[] content)
        {
            if (socket == null)
                return new ZeroResult
                {
                    State = ZeroOperatorStateType.ArgumentInvalid,
                    ErrorMessage = "参数错误"
                };
            return subTitle == null
                ? Publish(socket, title, PubDescriptionJson,
                    title.ToZeroBytes(),
                    GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                    ZeroApplication.Config.RealName.ToZeroBytes(),
                    content,
                    GlobalContext.Current.ToZeroBytes(),
                    GlobalContext.RequestInfo.LocalGlobalId.ToZeroBytes(),
                    GlobalContext.ServiceKey.ToZeroBytes())
                : Publish(socket, title, PubDescriptionJson2,
                    title.ToZeroBytes(),
                    GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                    subTitle.ToZeroBytes(),
                    ZeroApplication.Config.RealName.ToZeroBytes(),
                    content,
                    GlobalContext.Current.ToZeroBytes(),
                    GlobalContext.RequestInfo.LocalGlobalId.ToZeroBytes(),
                    GlobalContext.ServiceKey.ToZeroBytes());
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
            return PublishByOrg(socket, title, subTitle, content.ToZeroBytes());
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
            return Publish(socket, title, DescriptionNoGlobalId,
                    subTitle.ToZeroBytes(),
                    content,
                    GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                    ZeroApplication.AppName.ToZeroBytes(),
                    ZeroApplication.AppName.ToZeroBytes(),
                    GlobalContext.ServiceKey.ToZeroBytes());
        }
    }
}