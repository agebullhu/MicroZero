using System.Collections.Generic;
using Agebull.Common.Logging;
using Agebull.MicroZero;

namespace ZeroMQ
{

    /// <summary>
    ///     Sends and receives messages, single frames and byte frames across ZeroMQ.
    /// </summary>
    public sealed class ZSocketEx : ZSocket
    {
        /// <summary>
        /// 服务令牌字节内容
        /// </summary>
        public byte[] ServiceKey { get; set; }

        /// <summary>
        ///     构造
        /// </summary>
        /// <returns>
        ///     实例
        /// </returns>
        public ZSocketEx(ZContext context, ZSocketType socketType, out ZError error) : base(context, socketType, out error)
        {
        }

        /// <summary>
        /// 构建套接字
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="serviceKey">服务令牌</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static ZSocketEx CreateServiceSocket(string address, byte[] serviceKey, ZSocketType type)
        {
            if (!ZContext.IsAlive)
                return null;
            var socket = new ZSocketEx(ZContext.Current, type, out var error);
            if (error != null)
            {
                LogRecorder.Error($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
                return null;
            }
            ConfigSocket(socket, null, true, false);
            socket.ServiceKey = serviceKey;
            if (socket.Bind(address, out error))
                return socket;
            LogRecorder.Error($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
            socket.Dispose();
            return null;
        }

        /// <summary>
        /// 构建套接字
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="serviceKey">服务令牌</param>
        /// <param name="type">类型</param>
        /// <param name="identity">身份标签</param>
        /// <param name="longLink">是否保持长连接</param>
        /// <returns></returns>
        public static ZSocketEx CreateClientSocket(string address, byte[] serviceKey, ZSocketType type, byte[] identity, bool longLink)
        {
            return CreateClientSocketInner(address, serviceKey, type, identity, longLink);
        }

        /// <summary>
        /// 构建套接字
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="type">类型</param>
        /// <param name="serviceKey">服务令牌</param>
        /// <param name="identity">身份标签</param>
        /// <returns></returns>
        public static ZSocketEx CreatePoolSocket(string address, byte[] serviceKey, ZSocketType type, byte[] identity)
        {
            return CreateClientSocketInner(address, serviceKey, type, identity, false);
        }

        /// <summary>
        /// 构建长连接套接字
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="type">类型</param>
        /// <param name="serviceKey">服务令牌</param>
        /// <param name="identity">身份标签</param>
        /// <returns></returns>
        public static ZSocketEx CreateLongLink(string address, byte[] serviceKey, ZSocketType type, byte[] identity)
        {
            if (!ZContext.IsAlive)
                return null;
            var socket = new ZSocketEx(ZContext.Current, type, out var error);
            if (error != null)
            {
                LogRecorder.Error($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
                return null;
            }
            ConfigSocket(socket, identity, false, true);
            socket.ServiceKey = serviceKey;
            if (socket.Connect(address, out error))
                return socket;
            LogRecorder.Error($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
            socket.Dispose();
            return socket;
        }

        /// <summary>
        /// 构建一次性使用的套接字
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="identity">身份标签</param>
        /// <param name="serviceKey">服务令牌</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static ZSocketEx CreateOnceSocket(string address, byte[] serviceKey, byte[] identity, ZSocketType type = ZSocketType.DEALER)
        {
            return CreateClientSocketInner(address, serviceKey, type, identity, false);
        }

        /// <summary>
        /// 构建套接字
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="subscribe">订阅内容</param>
        /// <param name="serviceKey">服务令牌</param>
        /// <param name="identity">身份标签</param>
        /// <returns></returns>
        public static ZSocketEx CreateSubSocket(string address, byte[] serviceKey, byte[] identity, string subscribe)
        {
            var socket = CreateClientSocketInner(address, serviceKey, ZSocketType.SUB, identity, true);
            if (string.IsNullOrEmpty(subscribe))
                socket.SubscribeAll();
            else
                socket.Subscribe(subscribe);
            return socket;
        }

        /// <summary>
        /// 构建套接字
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="subscribes">订阅内容</param>
        /// <param name="serviceKey">服务令牌</param>
        /// <param name="identity">身份标签</param>
        /// <returns></returns>
        public static ZSocketEx CreateSubSocket(string address, byte[] serviceKey, byte[] identity, ICollection<string> subscribes)
        {
            var socket = CreateClientSocketInner(address, serviceKey, ZSocketType.SUB, identity, true);
            if (subscribes == null || subscribes.Count == 0)
                socket.SubscribeAll();
            else
                foreach (var subscribe in subscribes)
                    socket.Subscribe(subscribe);
            return socket;
        }

        /// <summary>
        /// 构建套接字
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="type">套接字类型</param>
        /// <param name="serviceKey">服务令牌</param>
        /// <param name="identity">身份标签</param>
        /// <param name="longLink">是否保持长连接</param>
        /// <returns></returns>
        private static ZSocketEx CreateClientSocketInner(string address, byte[] serviceKey, ZSocketType type, byte[] identity, bool longLink)
        {
            if (!ZContext.IsAlive)
                return null;
            var socket = new ZSocketEx(ZContext.Current, type, out var error);
            if (error != null)
            {
                LogRecorder.Error($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
                return null;
            }

            socket.ServiceKey = serviceKey;
            ConfigSocket(socket, identity, false, longLink);

            if (socket.Connect(address, out error))
                return socket;
            LogRecorder.Error($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
            socket.Dispose();
            return null;
        }

        /// <summary>
        /// 配置套接字
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="identity"></param>
        /// <param name="service"></param>
        /// <param name="longLink"></param>
        /// <returns></returns>
        private static void ConfigSocket(ZSocketEx socket, byte[] identity, bool service, bool longLink)
        {
            if (identity != null)
                socket.SetOption(ZSocketOption.IDENTITY, identity);
            if (Option.Linger > 0)
                socket.SetOption(ZSocketOption.LINGER, Option.Linger);
            if (Option.RecvTimeout > 0)
                socket.SetOption(ZSocketOption.RCVTIMEO, Option.RecvTimeout);
            if (Option.SendTimeout > 0)
                socket.SetOption(ZSocketOption.SNDTIMEO, Option.SendTimeout);
            if (service)
            {
                if (Option.Backlog > 0)
                    socket.SetOption(ZSocketOption.BACKLOG, Option.Backlog);
            }
            else
            {
                if (Option.ConnectTimeout > 0)
                    socket.SetOption(ZSocketOption.CONNECT_TIMEOUT, Option.ConnectTimeout);
                if (Option.ReconnectIvl > 0)
                    socket.SetOption(ZSocketOption.RECONNECT_IVL, Option.ReconnectIvl);
                if (Option.ReconnectIvlMax > 0)
                    socket.SetOption(ZSocketOption.RECONNECT_IVL_MAX, Option.ReconnectIvlMax);
            }
            if (!longLink)
                return;

            //if (Option.HeartbeatIvl > 0)
            //{
            //    socket.SetOption(ZSocketOption.HEARTBEAT_IVL, Option.HeartbeatIvl);
            //    socket.SetOption(ZSocketOption.HEARTBEAT_TIMEOUT, Option.HeartbeatTimeout);
            //    socket.SetOption(ZSocketOption.HEARTBEAT_TTL, Option.HeartbeatTtl);
            //}
            if (Option.TcpKeepalive > 0)
            {
                socket.SetOption(ZSocketOption.TCP_KEEPALIVE, 1);
                socket.SetOption(ZSocketOption.TCP_KEEPALIVE_IDLE, Option.TcpKeepaliveIdle);
                socket.SetOption(ZSocketOption.TCP_KEEPALIVE_INTVL, Option.TcpKeepaliveIntvl);
            }
        }


        #region Extend

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns>1 发送成功 0 发送失败 -1部分发送</returns>
        public bool SendByServiceKey(ZMessage message)
        {
            if (message == null || message.Count == 0)
                return false;
            using (message)
            {
                _error = null; 
                foreach (var frame in message)
                {
                    if (!SendFrame(frame, FlagsSndmore, out _error))
                        return false;
                }
                return SendFrame(new ZFrame(ServiceKey), FlagsDontwait, out _error);
            }
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="array">消息</param>
        /// <returns>是否发送成功</returns>
        public bool SendByServiceKey(byte[] desc, params string[] array)
        {
            _error = null;
            if (!SendFrame(new ZFrame(desc), FlagsSndmore,out _error))
                return false;
            foreach (var data in array)
            {
                if (!SendFrame(new ZFrame(data.ToZeroBytes()), FlagsSndmore, out _error))
                    return false;
            }
            return SendFrame(new ZFrame(ServiceKey), FlagsDontwait, out _error);
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="array">消息</param>
        /// <returns>是否发送成功</returns>
        public bool SendBy(byte[] desc, params byte[][] array)
        {
            _error = null;
            if (!SendFrame(new ZFrame(desc), FlagsSndmore,out _error))
                return false;
            foreach (var data in array)
            {
                if (!SendFrame(new ZFrame(data), FlagsSndmore, out _error))
                    return false;
            }
            return SendFrame(new ZFrame(ServiceKey), FlagsDontwait, out _error);
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="array">消息</param>
        /// <returns>是否发送成功</returns>
        public bool SendByServiceKey(byte[] desc, params byte[][] array)
        {
            _error = null;
            if (!SendFrame(new ZFrame(desc), FlagsSndmore, out _error))
                return false;
            foreach (var data in array)
            {
                if (!SendFrame(new ZFrame(data), FlagsSndmore, out _error))
                    return false;
            }
            return SendFrame(new ZFrame(ServiceKey), FlagsDontwait, out _error);
        }
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="strs">消息</param>
        /// <returns>是否发送成功</returns>
        public bool SendByServiceKey(params string[] strs)
        {
            if (strs == null || strs.Length == 0)
                return false;
            _error = null;
            foreach (var str in strs)
            {
                if (!SendFrame(new ZFrame(str.ToZeroBytes()), FlagsSndmore, out _error))
                    return false;
            }
            return SendFrame(new ZFrame(ServiceKey), FlagsDontwait, out _error);
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="array">消息</param>
        /// <returns>是否发送成功</returns>
        public bool SendByServiceKey(params byte[][] array)
        {
            if (array == null || array.Length == 0)
                return false;
            _error = null;
            foreach (var data in array)
            {
                if (!SendFrame(new ZFrame(data), FlagsSndmore, out _error))
                    return false;
            }
            return SendFrame(new ZFrame(ServiceKey), FlagsDontwait, out _error);
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="array">消息</param>
        /// <param name="extend"></param>
        /// <returns>是否发送成功</returns>
        public bool SendByServiceKey(byte[][] array, params byte[][] extend)
        {
            _error = null;
            foreach (var data in array)
            {
                if (!SendFrame(new ZFrame(data), FlagsSndmore, out _error))
                    return false;
            }
            foreach (var data in extend)
            {
                if (!SendFrame(new ZFrame(data), FlagsSndmore, out _error))
                    return false;
            }
            return SendFrame(new ZFrame(ServiceKey), FlagsSndmore, out _error);
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public ZMessage Recv(int flags = FlagsNone)
        {
            var message = new ZMessage();
            do
            {
                if (!RecvFrame(out var frame, flags))
                {
                    break;
                }
                message.Add(frame);
            }
            while (ReceiveMore);
            return message;
        }

        /*
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public bool Recv(out ZMessage message, int flags = FlagsNone)
        {
            message = new ZMessage();
            do
            {
                Error = null;
                if (!RecvFrame(out var frame, flags))
                {
                    break;
                }
                message.Add(frame);
            } while (ReceiveMore);

            return message.Count > 0;
        }
        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="array">消息</param>
        /// <returns>是否发送成功</returns>
        public bool SendTo(params ZFrame[] array)
        {
            if (array == null || array.Length == 0)
                return false;
            _error = null;
            foreach (var frame in array)
            {
                if (!SendFrame(frame, FlagsSndmore))
                    return false;
            }
            using (var f = new ZFrame(ServiceKey))
                return SendFrame(f, FlagsDontwait);
        }


        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="des"></param>
        /// <param name="array">消息</param>
        /// <returns>是否发送成功</returns>
        public bool SendTo(byte[] des, params string[] array)
        {
            using (var f = new ZFrame(des))
                if (!SendFrame(f, FlagsSndmore))
                    return false;
            if (array == null || array.Length == 0)
            {
                return true;
            }
            _error = null;
            foreach (var data in array)
            {
                using (var frame = new ZFrame(data))
                    if (!SendFrame(frame, FlagsSndmore))
                        return false;
            }
            using (var f = new ZFrame(ServiceKey))
                return SendFrame(f, FlagsDontwait);
        }*/
        #endregion

    }
}