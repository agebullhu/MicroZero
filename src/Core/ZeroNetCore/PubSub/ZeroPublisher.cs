using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.PubSub
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
            if (socket.Socket == null)
            {
                return false;
            }
            using (socket)
            {
                return socket.Socket.Publish(new PublishItem
                {
                    Station = station,
                    Title = title,
                    SubTitle = sub,
                    RequestId = ApiContext.RequestInfo.RequestId,
                    Content = value ?? "{}"
                });
            }
        }
        
    }
}