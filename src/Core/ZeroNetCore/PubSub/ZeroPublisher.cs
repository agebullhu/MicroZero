using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.PubSub
{
    /// <summary>
    ///     消息发布
    /// </summary>
    internal class ZPublisher : IZeroPublisher
    {
        bool IZeroPublisher.Publish(string station, string title, string sub, string arg)
        {
            return ZeroPublisher.Publish(station, title, sub, arg);
        }
    }

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
        /// <param name="sub"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Publish(string station, string title, string sub, string value)
        {
            return DoPublish(station, title, sub, value);
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
            return DoPublish(station, title, sub, value == default(T) ? "{}" : JsonConvert.SerializeObject(value));
        }
        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="sub"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool DoPublish(string station, string title, string sub, string value)
        {
            var socket = ZeroConnectionPool.GetSocket(station, RandomOperate.Generate(8));
            if (socket?.Socket == null)
            {
                return false;
            }
            using (socket)
            {
                return socket.Socket.Publish(title, sub, value);
            }
        }

        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="sub"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ZeroResult PublishByOrg<T>(string station, string title, string sub, T value)
            where T : class
        {
            var socket = ZeroConnectionPool.GetSocket(station, RandomOperate.Generate(8));
            if (socket?.Socket == null)
            {
                return new ZeroResult
                {
                    State = ZeroOperatorStateType.LocalNoReady,
                    ErrorMessage = "网络未准备好"
                };
            }
            using (socket)
            {
                return socket.Socket.PublishByOrg(title, sub, value);
            }
        }

        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="sub"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ZeroResult PublishByOrg(string station, string title, string sub, string value)
        {
            var socket = ZeroConnectionPool.GetSocket(station, RandomOperate.Generate(8));
            if (socket?.Socket == null)
            {
                return new ZeroResult
                {
                    State = ZeroOperatorStateType.LocalNoReady,
                    ErrorMessage = "网络未准备好"
                };
            }
            using (socket)
            {
                return socket.Socket.PublishByOrg(title, sub, value);
            }
        }
    }
}