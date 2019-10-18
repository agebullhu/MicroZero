using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Agebull.Common.Context;
using Agebull.Common.Logging;

using Agebull.MicroZero;
using Agebull.EntityModel.Common;
using ZeroMQ.lib;

namespace ZeroMQ
{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    /// <summary>
    ///     Sends and receives messages, single frames and byte frames across ZeroMQ.
    /// </summary>
    public sealed class ZSocket : MemoryCheck
    {
        #region Const


#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        // From options.hpp: unsigned char identity [256];
        private const int MaxBinaryOptionSize = 256;

        public const int BinaryKeySize = 32;

        public const int FlagsNone = 0;
        public const int FlagsDontwait = 1;
        public const int FlagsSndmore = 2;


        #endregion

        #region Field
#if UNMANAGE_MONEY_CHECK
        protected override string TypeName => nameof(ZSocket);
#endif

        public ZContext Context { get; private set; }

        public IntPtr SocketPtr { get; private set; }

        /// <summary>
        ///     Gets the <see cref="ZeroMQ.ZSocketType" /> value for the current socket.
        /// </summary>
        public ZSocketType SocketType { get; private set; }


        #endregion

        #region Create

        /// <summary>
        ///     构造
        /// </summary>
        /// <returns>
        ///     实例
        /// </returns>
        private ZSocket(ZContext context, ZSocketType socketType, out ZError error)
        {
            Context = context;
            SocketType = socketType;

            Initialize(out error);
        }


        /// <summary>
        ///     构造
        /// </summary>
        /// <returns>
        ///     实例
        /// </returns>
        public static ZSocket Create(ZContext context, ZSocketType socketType)
        {
            return new ZSocket(context, socketType, out _);
        }


        /// <summary>
        ///     构造
        /// </summary>
        /// <returns>
        ///     实例
        /// </returns>
        public static ZSocket Create(ZSocketType socketType)
        {
            return new ZSocket(ZContext.Current, socketType, out _);
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <returns>
        ///     实例
        /// </returns>
        public static ZSocket Create(ZSocketType socketType, out ZError error)
        {
            return new ZSocket(ZContext.Current, socketType, out error);
        }

        /// <summary>
        ///     构造
        /// </summary>
        /// <returns>
        ///     实例
        /// </returns>
        public static ZSocket Create(ZContext context, ZSocketType socketType, out ZError error)
        {
            return new ZSocket(context, socketType, out error);
        }

        /// <summary>
        /// 构建套接字
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static ZSocket CreateServiceSocket(string address, ZSocketType type)
        {
            if (!ZContext.IsAlive)
                return null;
            var socket = Create(type, out var error);
            if (error != null)
            {
                LogRecorderX.Error($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
                return null;
            }
            ConfigSocket(socket, null, true, false);
            if (socket.Bind(address, out error))
                return socket;
            LogRecorderX.SystemLog($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
            socket.Dispose();
            return null;
        }

        /// <summary>
        /// 构建套接字
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="type">类型</param>
        /// <param name="identity">身份标签</param>
        /// <param name="longLink">是否保持长连接</param>
        /// <returns></returns>
        public static ZSocket CreateClientSocket(string address, ZSocketType type, byte[] identity, bool longLink)
        {
            return CreateClientSocketInner(address, type, identity, longLink);
        }

        /// <summary>
        /// 构建套接字
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="type">类型</param>
        /// <param name="identity">身份标签</param>
        /// <returns></returns>
        public static ZSocket CreatePoolSocket(string address, ZSocketType type, byte[] identity)
        {
            return CreateClientSocketInner(address, type, identity, false);
        }

        /// <summary>
        /// 构建长连接套接字
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="type">类型</param>
        /// <param name="identity">身份标签</param>
        /// <returns></returns>
        public static ZSocket CreateLongLink(string address, ZSocketType type, byte[] identity)
        {
            if (!ZContext.IsAlive)
                return null;
            var socket = Create(type, out var error);
            if (error != null)
            {
                LogRecorderX.Error($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
                return null;
            }
            ConfigSocket(socket, identity, false, true);
            if (socket.Connect(address, out error))
                return socket;
            LogRecorderX.Error($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
            socket.Dispose();
            return socket;
        }

        /// <summary>
        /// 构建一次性使用的套接字
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="identity">身份标签</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static ZSocket CreateOnceSocket(string address, byte[] identity, ZSocketType type = ZSocketType.DEALER)
        {
            return CreateClientSocketInner(address, type, identity, false);
        }

        /// <summary>
        /// 构建套接字
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="subscribe">订阅内容</param>
        /// <param name="identity">身份标签</param>
        /// <returns></returns>
        public static ZSocket CreateSubSocket(string address, byte[] identity, string subscribe)
        {
            var socket = CreateClientSocketInner(address, ZSocketType.SUB, identity, true);
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
        /// <param name="identity">身份标签</param>
        /// <returns></returns>
        public static ZSocket CreateSubSocket(string address, byte[] identity, ICollection<string> subscribes)
        {
            var socket = CreateClientSocketInner(address, ZSocketType.SUB, identity, true);
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
        /// <param name="identity">身份标签</param>
        /// <param name="longLink">是否保持长连接</param>
        /// <returns></returns>
        private static ZSocket CreateClientSocketInner(string address, ZSocketType type, byte[] identity, bool longLink)
        {
            if (!ZContext.IsAlive)
                return null;
            var socket = Create(type, out var error);
            if (error != null)
            {
                LogRecorderX.Error($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
                return null;
            }

            ConfigSocket(socket, identity, false, longLink);

            if (socket.Connect(address, out error))
                return socket;
            LogRecorderX.Error($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
            socket.Dispose();
            return null;
        }
        #endregion

        #region Close

        protected override void DoDispose()
        {
            Close(out _error);
        }

        /// <summary>
        ///     Close the current socket.
        /// </summary>
        public void Close()
        {
            if (!Close(out _error)) throw new ZException(_error);
        }

        /// <summary>
        ///     Close the current socket.
        /// </summary>
        public bool TryClose()
        {
            return Close(out _);
        }

        /// <summary>
        ///     Close the current socket.
        /// </summary>
        public bool Close(out ZError error)
        {
            return CloseInner(out error);
        }
        #endregion

        #region Bind

        /// <summary>
        ///     Bind the specified endpoint.
        /// </summary>
        /// <param name="endpoint">
        ///     A string consisting of a transport and an address, formatted as
        ///     <c><em>transport</em>://<em>address</em></c>.
        /// </param>
        public void Bind(string endpoint)
        {
            if (!Bind(endpoint, out _error)) throw new ZException(_error);
        }

        /// <summary>
        ///     Bind the specified endpoint.
        /// </summary>
        /// <param name="endpoint">
        ///     A string consisting of a transport and an address, formatted as
        ///     <c><em>transport</em>://<em>address</em></c>.
        /// </param>
        /// <param name="error"></param>
        public bool Bind(string endpoint, out ZError error)
        {
            return BindInner(endpoint, out error);
        }

        /// <summary>
        ///     Unbind the specified endpoint.
        /// </summary>
        /// <param name="endpoint">
        ///     A string consisting of a transport and an address, formatted as
        ///     <c><em>transport</em>://<em>address</em></c>.
        /// </param>
        public void Unbind(string endpoint)
        {
            if (!Unbind(endpoint, out _error)) throw new ZException(_error);
        }

        /// <summary>
        ///     Unbind the specified endpoint.
        /// </summary>
        /// <param name="endpoint">
        ///     A string consisting of a transport and an address, formatted as
        ///     <c><em>transport</em>://<em>address</em></c>.
        /// </param>
        /// <param name="error"></param>
        public bool Unbind(string endpoint, out ZError error)
        {
            return UnbindInner(endpoint, out error);
        }

        /// <summary>
        ///     Connect the specified endpoint.
        /// </summary>
        /// <param name="endpoint">
        ///     A string consisting of a transport and an address, formatted as
        ///     <c><em>transport</em>://<em>address</em></c>.
        /// </param>
        public void Connect(string endpoint)
        {
            if (!Connect(endpoint, out _error)) throw new ZException(_error);
        }

        /// <summary>
        ///     Connect the specified endpoint.
        /// </summary>
        /// <param name="endpoint">
        ///     A string consisting of a transport and an address, formatted as
        ///     <c><em>transport</em>://<em>address</em></c>.
        /// </param>
        /// <param name="error"></param>
        [SecurityCritical]//捕获c++异常
        [HandleProcessCorruptedStateExceptions]//捕获c++异常
        public bool Connect(string endpoint, out ZError error)
        {
            return ConnectInner(endpoint, out error);
        }

        /// <summary>
        ///     Disconnect the specified endpoint.
        /// </summary>
        public void Disconnect(string endpoint)
        {
            if (!Disconnect(endpoint, out _error))
                throw new ZException(_error);
        }

        /// <summary>
        ///     Disconnect the specified endpoint.
        /// </summary>
        /// <param name="endpoint">
        ///     A string consisting of a transport and an address, formatted as
        ///     <c><em>transport</em>://<em>address</em></c>.
        /// </param>
        /// <param name="error"></param>
        [SecurityCritical]//捕获c++异常
        [HandleProcessCorruptedStateExceptions]//捕获c++异常
        public bool Disconnect(string endpoint, out ZError error)
        {
            return DisconnectInner(endpoint, out error);
        }


        #endregion

        #region Receive

        /// <summary>
        ///     Receives HARD bytes into a new byte[n]. Please don't use ReceiveBytes, use instead ReceiveFrame.
        /// </summary>
        public int ReceiveBytes(byte[] buffer, int offset, int count)
        {
            int length;
            if (-1 == (length = ReceiveBytes(buffer, offset, count, ZSocketFlags.None, out _error)))
                throw new ZException(_error);
            return length;
        }


        public ZMessage ReceiveMessage()
        {
            return ReceiveMessage(ZSocketFlags.None);
        }

        public ZMessage ReceiveMessage(out ZError error)
        {
            return ReceiveMessage(ZSocketFlags.None, out error);
        }

        public ZMessage ReceiveMessage(ZSocketFlags flags)
        {
            var message = ReceiveMessage(flags, out _error);
            if (!Equals(_error, ZError.None)) throw new ZException(_error);
            return message;
        }

        public ZMessage ReceiveMessage(ZSocketFlags flags, out ZError error)
        {
            ZMessage message = null;
            ReceiveMessage(ref message, flags, out error);
            return message;
        }

        public bool ReceiveMessage(ref ZMessage message, out ZError error)
        {
            return ReceiveMessage(ref message, ZSocketFlags.None, out error);
        }

        public bool ReceiveMessage(ref ZMessage message, ZSocketFlags flags, out ZError error)
        {
            //EnsureNotDisposed();

            var count = int.MaxValue;
            return ReceiveFrames(ref count, ref message, flags, out error);
        }

        public ZFrame ReceiveFrame()
        {
            var frame = ReceiveFrame(out _error);
            if (!Equals(_error, ZError.None)) throw new ZException(_error);
            return frame;
        }

        public ZFrame ReceiveFrame(out ZError error)
        {
            return ReceiveFrame(ZSocketFlags.None, out error);
        }

        public ZFrame ReceiveFrame(ZSocketFlags flags, out ZError error)
        {
            var frames = ReceiveFrames(1, flags & ~ZSocketFlags.More, out error);
            if (frames != null)
                foreach (var frame in frames)
                    return frame;
            return null;
        }

        public IEnumerable<ZFrame> ReceiveFrames(int framesToReceive)
        {
            return ReceiveFrames(framesToReceive, ZSocketFlags.None);
        }

        public IEnumerable<ZFrame> ReceiveFrames(int framesToReceive, ZSocketFlags flags)
        {
            var frames = ReceiveFrames(framesToReceive, flags, out _error);
            if (!Equals(_error, ZError.None)) throw new ZException(_error);
            return frames;
        }

        public IEnumerable<ZFrame> ReceiveFrames(int framesToReceive, out ZError error)
        {
            return ReceiveFrames(framesToReceive, ZSocketFlags.None, out error);
        }

        public IEnumerable<ZFrame> ReceiveFrames(int framesToReceive, ZSocketFlags flags, out ZError error)
        {
            List<ZFrame> frames = null;
            while (!ReceiveFrames(ref framesToReceive, ref frames, flags, out error))
            {
                if (error.IsError(ZError.Code.EAGAIN) && (flags & ZSocketFlags.DontWait) == ZSocketFlags.DontWait)
                    break;
                return null;
            }

            return frames;
        }

        public bool ReceiveFrames<ListT>(ref int framesToReceive, ref ListT frames, ZSocketFlags flags,
            out ZError error)
            where ListT : IList<ZFrame>, new()
        {
            //EnsureNotDisposed();

            error = _error = null;
            flags |= ZSocketFlags.More;

            do
            {
                var frame = ZFrame.CreateEmpty();

                if (framesToReceive == 1)
                    flags &= ~ZSocketFlags.More;

                while (-1 == zmq.msg_recv(frame.Ptr, SocketPtr, (int)flags))
                {
                    error = _error = ZError.GetLastErr();

                    if (error.IsError(ZError.Code.EINTR))
                    {
                        error = _error = null;
                        continue;
                    }

                    frame.Close();
                    return false;
                }

                if (frames == null)
                    frames = new ListT();
                frames.Add(frame);
            } while (--framesToReceive > 0 && ReceiveMore);

            return true;
        }

        #endregion

        #region Send

        /// <summary>
        ///     Sends HARD bytes from a byte[n]. Please don't use SendBytes, use instead SendFrame.
        /// </summary>
        public bool Send(byte[] buffer, int offset, int count)
        {
            return SendBytes(buffer, offset, count);
        } // just Send*

        /// <summary>
        ///     Sends HARD bytes from a byte[n]. Please don't use SendBytes, use instead SendFrame.
        /// </summary>
        public bool Send(byte[] buffer, int offset, int count, ZSocketFlags flags, out ZError error)
        {
            return SendBytes(buffer, offset, count, flags, out error);
        }


        /// <summary>
        ///     Sends HARD bytes from a byte[n]. Please don't use SendBytes, use instead SendFrame.
        /// </summary>
        public bool SendBytes(byte[] buffer, int offset, int count)
        {
            if (!SendBytes(buffer, offset, count, ZSocketFlags.None, out _error)) throw new ZException(_error);
            return true;
        }

        /// <summary>
        ///     Sends HARD bytes from a byte[n]. Please don't use SendBytes, use instead SendFrame.
        /// </summary>
        public bool SendBytes(byte[] buffer, int offset, int count, ZSocketFlags flags, out ZError error)
        {
            return SendBytesInner(buffer, offset, count, ZSocketFlags.None, out error);
        }
        // just Send*
        public void Send(ZMessage msg)
        {
            SendMessage(msg);
        } // just Send*

        public bool Send(ZMessage msg, out ZError error)
        {
            return SendMessage(msg, out error);
        } // just Send*

        public void Send(ZMessage msg, ZSocketFlags flags)
        {
            SendMessage(msg, flags);
        } // just Send*

        public bool Send(ZMessage msg, ZSocketFlags flags, out ZError error)
        {
            return SendMessage(msg, flags, out error);
        } // just Send*

        public void Send(IEnumerable<ZFrame> frames)
        {
            SendFrames(frames);
        } // just Send*

        public bool Send(IEnumerable<ZFrame> frames, out ZError error)
        {
            return SendFrames(frames, out error);
        } // just Send*

        public void Send(IEnumerable<ZFrame> frames, ZSocketFlags flags)
        {
            SendFrames(frames, flags);
        } // just Send*

        public bool Send(IEnumerable<ZFrame> frames, ZSocketFlags flags, out ZError error)
        {
            return SendFrames(frames, flags, out error);
        } // just Send*

        public bool Send(IEnumerable<ZFrame> frames, ref int sent, ZSocketFlags flags, out ZError error)
        {
            return SendFrames(frames, ref sent, flags, out error);
        } // just Send*

        public void Send(ZFrame frame)
        {
            SendFrame(frame);
        } // just Send*

        public bool Send(ZFrame msg, out ZError error)
        {
            return SendFrame(msg, out error);
        } // just Send*

        public void SendMore(ZFrame frame)
        {
            SendFrameMore(frame);
        } // just Send*

        public bool SendMore(ZFrame msg, out ZError error)
        {
            return SendFrameMore(msg, out error);
        } // just Send*

        public void SendMore(ZFrame frame, ZSocketFlags flags)
        {
            SendFrameMore(frame, flags);
        } // just Send*

        public bool SendMore(ZFrame msg, ZSocketFlags flags, out ZError error)
        {
            return SendFrameMore(msg, flags, out error);
        } // just Send*

        public void Send(ZFrame frame, ZSocketFlags flags)
        {
            SendFrame(frame, flags);
        } // just Send*

        public bool Send(ZFrame frame, ZSocketFlags flags, out ZError error)
        {
            return SendFrame(frame, flags, out error);
        } // just Send*

        public void SendMessage(ZMessage msg)
        {
            SendMessage(msg, ZSocketFlags.DontWait);
        }

        public bool SendMessage(ZMessage msg, out ZError error)
        {
            return SendMessage(msg, ZSocketFlags.DontWait, out error);
        }

        public void SendMessage(ZMessage msg, ZSocketFlags flags)
        {
            if (!SendMessage(msg, flags, out _error)) throw new ZException(_error);
        }

        public bool SendMessage(ZMessage msg, ZSocketFlags flags, out ZError error)
        {
            return SendFrames(msg, flags, out error);
        }

        public void SendFrames(IEnumerable<ZFrame> frames)
        {
            SendFrames(frames, ZSocketFlags.None);
        }

        public bool SendFrames(IEnumerable<ZFrame> frames, out ZError error)
        {
            return SendFrames(frames, ZSocketFlags.DontWait, out error);
        }

        public void SendFrames(IEnumerable<ZFrame> frames, ZSocketFlags flags)
        {
            var sent = 0;
            if (!SendFrames(frames, ref sent, flags, out _error)) throw new ZException(_error);
        }

        public bool SendFrames(IEnumerable<ZFrame> frames, ZSocketFlags flags, out ZError error)
        {
            var sent = 0;
            if (!SendFrames(frames, ref sent, flags, out error)) return false;
            return true;
        }

        public bool SendFrames(IEnumerable<ZFrame> frames, ref int sent, ZSocketFlags flags, out ZError error)
        {
            //EnsureNotDisposed();

            error = _error = ZError.None;

            var more = (flags & ZSocketFlags.More) == ZSocketFlags.More;
            flags = flags | ZSocketFlags.More;

            var framesIsList = frames is IList<ZFrame> list && !list.IsReadOnly;
            var array = frames.ToArray();

            for (int i = 0, l = array.Length; i < l; ++i)
            {
                var frame = array[i];

                if (i == l - 1 && !more)
                    flags = flags & ~ZSocketFlags.More;

                if (!SendFrame(frame, flags, out error))
                    return false;

                if (framesIsList)
                {
                    ((IList<ZFrame>)frames).Remove(frame);
                    frame.Close();
                }

                ++sent;
            }

            return true;
        }

        public void SendFrame(ZFrame frame)
        {
            SendFrame(frame, ZSocketFlags.None);
        }

        public bool SendFrame(ZFrame msg, out ZError error)
        {
            return SendFrame(msg, ZSocketFlags.None, out error);
        }

        public void SendFrameMore(ZFrame frame)
        {
            SendFrame(frame, ZSocketFlags.More);
        }

        public bool SendFrameMore(ZFrame msg, out ZError error)
        {
            return SendFrame(msg, ZSocketFlags.More, out error);
        }

        public void SendFrameMore(ZFrame frame, ZSocketFlags flags)
        {
            SendFrame(frame, flags | ZSocketFlags.More);
        }

        public bool SendFrameMore(ZFrame msg, ZSocketFlags flags, out ZError error)
        {
            return SendFrame(msg, flags | ZSocketFlags.More, out error);
        }

        public void SendFrame(ZFrame frame, ZSocketFlags flags)
        {
            if (!SendFrame(frame, flags, out _error)) throw new ZException(_error);
        }

        public bool SendFrame(ZFrame frame, ZSocketFlags flags, out ZError error)
        {
            //EnsureNotDisposed();

            if (frame.IsDismissed) throw new ObjectDisposedException("frame");

            error = _error = null;

            while (-1 == zmq.msg_send(frame.Ptr, SocketPtr, (int)flags))
            {
                error = _error = ZError.GetLastErr();

                if (!error.IsError(ZError.Code.EINTR))
                    return false;
                error = _error = null;
            }

            // Tell IDisposable to not unallocate zmq_msg
            frame.Close();
            return true;
        }

        /// <summary>
        /// 转发
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool Forward(ZSocket socket, out ZMessage message, out ZError error)
        {
            //EnsureNotDisposed();

            error = _error = null;
            message = null; // message is always null

            using (var msg = ZFrame.CreateEmpty())
            {
                bool more;
                do
                {
                    while (-1 == zmq.msg_recv(msg.Ptr, SocketPtr, (int)ZSocketFlags.None))
                    {
                        error = _error = ZError.GetLastErr();
                        if (!error.IsError(ZError.Code.EINTR))
                            return false;
                        error = _error = null;
                    }

                    // will have to receive more?
                    more = ReceiveMore;

                    // sending scope
                    while (-1 != zmq.msg_send(msg.Ptr, socket.SocketPtr,
                               more ? (int)ZSocketFlags.More : (int)ZSocketFlags.None))
                    {
                        error = _error = ZError.GetLastErr();

                        if (!error.IsError(ZError.Code.EINTR))
                            return false;
                        error = _error = null;
                    }

                    // msg.Dismiss
                } while (more);
            } // using (msg) -> Dispose

            return true;
        }

        #endregion

        #region Extend

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns>1 发送成功 0 发送失败 -1部分发送</returns>
        public bool SendTo(ZMessage message)
        {
            if (message == null || message.Count == 0)
                return false;
            _error = null;
            var i = 0;
            for (; i < message.Count - 1; ++i)
            {
                if (!SendFrame(message[i], FlagsSndmore))
                    return false;
            }
            return SendFrame(message[i], FlagsDontwait);
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
            var i = 0;
            for (; i < array.Length - 1; ++i)
            {
                using (var f = new ZFrame(array[i]))
                    if (!SendFrame(f, FlagsSndmore))
                        return false;
            }
            using (var f = new ZFrame(array[i]))
                return SendFrame(f, FlagsDontwait);
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="array">消息</param>
        /// <returns>是否发送成功</returns>
        public bool SendTo(params byte[][] array)
        {
            if (array == null || array.Length == 0)
                return false;
            _error = null;
            var i = 0;
            for (; i < array.Length - 1; ++i)
            {
                using (var f = new ZFrame(array[i]))
                    if (!SendFrame(f, FlagsSndmore))
                        return false;
            }
            using (var f = new ZFrame(array[i]))
                return SendFrame(f, FlagsDontwait);
        }


        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="array">消息</param>
        /// <param name="extend"></param>
        /// <returns>是否发送成功</returns>
        public bool SendTo(byte[][] array, params byte[][] extend)
        {
            _error = null;
            int i;
            for (i = 0; i < array.Length; ++i)
            {
                using (var f = new ZFrame(array[i]))
                    if (!SendFrame(f, FlagsSndmore))
                        return false;
            }
            for (i = 0; i < extend.Length - 1; ++i)
            {
                using (var f = new ZFrame(extend[i]))
                    if (!SendFrame(f, FlagsSndmore))
                        return false;
            }
            using (var f = new ZFrame(extend[i]))
                return SendFrame(f, FlagsDontwait);
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
            var i = 0;
            for (; i < array.Length - 1; ++i)
            {
                if (!SendFrame(array[i], FlagsSndmore))
                    return false;
            }
            return SendFrame(array[i], FlagsDontwait);
        }

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
                _error = null;
                if (!RecvFrame(out var frame, flags))
                {
                    break;
                }
                message.Add(frame);
            } while (ReceiveMore);

            return message.Count > 0;
        }

        #endregion

        #region Subscribe

        /// <summary>
        ///     Subscribe to all messages.
        /// </summary>
        /// <remarks>
        ///     Only applies to <see cref="ZeroMQ.ZSocketType.SUB" /> and <see cref="ZeroMQ.ZSocketType.XSUB" /> sockets.
        /// </remarks>
        public void SubscribeAll()
        {
            Subscribe(new byte[0]);
        }

        /// <summary>
        ///     Subscribe to messages that begin with a specified prefix.
        /// </summary>
        /// <remarks>
        ///     Only applies to <see cref="ZeroMQ.ZSocketType.SUB" /> and <see cref="ZeroMQ.ZSocketType.XSUB" /> sockets.
        /// </remarks>
        /// <param name="prefix">Prefix for subscribed messages.</param>
        public void Subscribe(byte[] prefix)
        {
            SetOption(ZSocketOption.SUBSCRIBE, prefix);
        }

        /// <summary>
        ///     Subscribe to messages that begin with a specified prefix.
        /// </summary>
        /// <remarks>
        ///     Only applies to <see cref="ZeroMQ.ZSocketType.SUB" /> and <see cref="ZeroMQ.ZSocketType.XSUB" /> sockets.
        /// </remarks>
        /// <param name="prefix">Prefix for subscribed messages.</param>
        public void Subscribe(string prefix)
        {
            SetOption(ZSocketOption.SUBSCRIBE, ZContext.Encoding.GetBytes(prefix ?? ""));
        }

        /// <summary>
        ///     Unsubscribe from all messages.
        /// </summary>
        /// <remarks>
        ///     Only applies to <see cref="ZeroMQ.ZSocketType.SUB" /> and <see cref="ZeroMQ.ZSocketType.XSUB" /> sockets.
        /// </remarks>
        public void UnsubscribeAll()
        {
            Unsubscribe(new byte[0]);
        }

        /// <summary>
        ///     Unsubscribe from messages that begin with a specified prefix.
        /// </summary>
        /// <remarks>
        ///     Only applies to <see cref="ZeroMQ.ZSocketType.SUB" /> and <see cref="ZeroMQ.ZSocketType.XSUB" /> sockets.
        /// </remarks>
        /// <param name="prefix">Prefix for subscribed messages.</param>
        public void Unsubscribe(byte[] prefix)
        {
            SetOption(ZSocketOption.UNSUBSCRIBE, prefix);
        }

        /// <summary>
        ///     Unsubscribe from messages that begin with a specified prefix.
        /// </summary>
        /// <remarks>
        ///     Only applies to <see cref="ZeroMQ.ZSocketType.SUB" /> and <see cref="ZeroMQ.ZSocketType.XSUB" /> sockets.
        /// </remarks>
        /// <param name="prefix">Prefix for subscribed messages.</param>
        public void Unsubscribe(string prefix)
        {
            SetOption(ZSocketOption.UNSUBSCRIBE, ZContext.Encoding.GetBytes(prefix));
        }


        #endregion

        #region Option

        /// <summary>
        ///     Gets a value indicating whether the multi-part message currently being read has more message parts to follow.
        /// </summary>
        public bool ReceiveMore => GetOptionInt32(ZSocketOption.RCVMORE) == 1;


        /// <summary>
        ///     已绑定地址
        /// </summary>
        public string Endpoint { get; private set; }

        /// <summary>
        ///     已绑定地址
        /// </summary>
        public string LastEndpoint => GetOptionString(ZSocketOption.LAST_ENDPOINT);

        /// <summary>
        ///     Add a filter that will be applied for each new TCP transport connection on a listening socket.
        ///     Example: "127.0.0.1", "mail.ru/24", "::1", "::1/128", "3ffe:1::", "3ffe:1::/56"
        /// </summary>
        /// <seealso cref="ClearTcpAcceptFilter" />
        /// <remarks>
        ///     If no filters are applied, then TCP transport allows connections from any IP.
        ///     If at least one filter is applied then new connection source IP should be matched.
        /// </remarks>
        /// <param name="filter">IPV6 or IPV4 CIDR filter.</param>
        public void AddTcpAcceptFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) throw new ArgumentNullException(nameof(filter));

            SetOption(ZSocketOption.TCP_ACCEPT_FILTER, filter);
        }

        /// <summary>
        ///     Reset all TCP filters assigned by <see cref="AddTcpAcceptFilter" />
        ///     and allow TCP transport to accept connections from any IP.
        /// </summary>
        public void ClearTcpAcceptFilter()
        {
            SetOption(ZSocketOption.TCP_ACCEPT_FILTER, (string)null);
        }


        /// <summary>
        /// 配置
        /// </summary>
        public static SocketOption Option = new SocketOption();

        /// <summary>
        /// 配置套接字
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="identity"></param>
        /// <param name="service"></param>
        /// <param name="longLink"></param>
        /// <returns></returns>
        private static void ConfigSocket(ZSocket socket, byte[] identity, bool service, bool longLink)
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


        /// <summary>
        ///     Gets or sets the I/O thread affinity for newly created connections on this socket.
        /// </summary>
        public ulong Affinity
        {
            get => GetOptionUInt64(ZSocketOption.AFFINITY);
            set => SetOption(ZSocketOption.AFFINITY, value);
        }

        /// <summary>
        ///     Gets or sets the maximum length of the queue of outstanding peer connections. (Default = 100 connections).
        /// </summary>
        public int Backlog
        {
            get => GetOptionInt32(ZSocketOption.BACKLOG);
            set => SetOption(ZSocketOption.BACKLOG, value);
        }

        public byte[] ConnectRoutingId
        {
            get => GetOptionBytes(ZSocketOption.CONNECT_ROUTING_ID);
            set => SetOption(ZSocketOption.CONNECT_ROUTING_ID, value);
        }

        public bool Conflate
        {
            get => GetOptionInt32(ZSocketOption.CONFLATE) == 1;
            set => SetOption(ZSocketOption.CONFLATE, value ? 1 : 0);
        }

        public byte[] CurvePublicKey
        {
            get => GetOptionBytes(ZSocketOption.CURVE_PUBLICKEY, BinaryKeySize);
            set => SetOption(ZSocketOption.CURVE_PUBLICKEY, value);
        }

        public byte[] CurveSecretKey
        {
            get => GetOptionBytes(ZSocketOption.CURVE_SECRETKEY, BinaryKeySize);
            set => SetOption(ZSocketOption.CURVE_SECRETKEY, value);
        }

        public bool CurveServer
        {
            get => GetOptionInt32(ZSocketOption.CURVE_SERVER) == 1;
            set => SetOption(ZSocketOption.CURVE_SERVER, value ? 1 : 0);
        }

        public byte[] CurveServerKey
        {
            get => GetOptionBytes(ZSocketOption.CURVE_SERVERKEY, BinaryKeySize);
            set => SetOption(ZSocketOption.CURVE_SERVERKEY, value);
        }

        public bool GSSAPIPlainText
        {
            get => GetOptionInt32(ZSocketOption.GSSAPI_PLAINTEXT) == 1;
            set => SetOption(ZSocketOption.GSSAPI_PLAINTEXT, value ? 1 : 0);
        }

        public string GSSAPIPrincipal
        {
            get => GetOptionString(ZSocketOption.GSSAPI_PRINCIPAL);
            set => SetOption(ZSocketOption.GSSAPI_PRINCIPAL, value);
        }

        public bool GSSAPIServer
        {
            get => GetOptionInt32(ZSocketOption.GSSAPI_SERVER) == 1;
            set => SetOption(ZSocketOption.GSSAPI_SERVER, value ? 1 : 0);
        }

        public string GSSAPIServicePrincipal
        {
            get => GetOptionString(ZSocketOption.GSSAPI_SERVICE_PRINCIPAL);
            set => SetOption(ZSocketOption.GSSAPI_SERVICE_PRINCIPAL, value);
        }

        public int HandshakeInterval
        {
            get => GetOptionInt32(ZSocketOption.HANDSHAKE_IVL);
            set => SetOption(ZSocketOption.HANDSHAKE_IVL, value);
        }

        /// <summary>
        ///     Gets or sets the Identity.
        /// </summary>
        /// <value>Identity as byte[]</value>
        public byte[] Identity
        {
            get => GetOptionBytes(ZSocketOption.IDENTITY);
            set => SetOption(ZSocketOption.IDENTITY, value);
        }

        /// <summary>
        ///     Gets or sets the Identity.
        ///     Note: The string contains chars like \0 (null terminator,
        ///     which are NOT printed (in LogRecorderX.Debug)!
        /// </summary>
        /// <value>Identity as string</value>
        public string IdentityString
        {
            get => ZContext.Encoding.GetString(Identity);
            set => Identity = ZContext.Encoding.GetBytes(value);
        }

        public bool Immediate
        {
            get => GetOptionInt32(ZSocketOption.IMMEDIATE) == 1;
            set => SetOption(ZSocketOption.IMMEDIATE, value ? 1 : 0);
        }

        public bool IPv6
        {
            get => GetOptionInt32(ZSocketOption.IPV6) == 1;
            set => SetOption(ZSocketOption.IPV6, value ? 1 : 0);
        }

        /// <summary>
        ///     Gets or sets the linger period for socket shutdown. (Default = <see cref="TimeSpan.MaxValue" />, infinite).
        /// </summary>
        public TimeSpan Linger
        {
            get => TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.LINGER));
            set => SetOption(ZSocketOption.LINGER, (int)value.TotalMilliseconds);
        }

        /// <summary>
        ///     Gets or sets the maximum size for inbound messages (bytes). (Default = -1, no limit).
        /// </summary>
        public long MaxMessageSize
        {
            get => GetOptionInt64(ZSocketOption.MAXMSGSIZE);
            set => SetOption(ZSocketOption.MAXMSGSIZE, value);
        }

        /// <summary>
        ///     Gets or sets the time-to-live field in every multicast packet sent from this socket (network hops). (Default = 1
        ///     hop).
        /// </summary>
        public int MulticastHops
        {
            get => GetOptionInt32(ZSocketOption.MULTICAST_HOPS);
            set => SetOption(ZSocketOption.MULTICAST_HOPS, value);
        }

        public string PlainPassword
        {
            get => GetOptionString(ZSocketOption.PLAIN_PASSWORD);
            set => SetOption(ZSocketOption.PLAIN_PASSWORD, value);
        }

        public bool PlainServer
        {
            get => GetOptionInt32(ZSocketOption.PLAIN_SERVER) == 1;
            set => SetOption(ZSocketOption.PLAIN_SERVER, value ? 1 : 0);
        }

        public string PlainUserName
        {
            get => GetOptionString(ZSocketOption.PLAIN_USERNAME);
            set => SetOption(ZSocketOption.PLAIN_USERNAME, value);
        }

        public bool ProbeRouter
        {
            get => GetOptionInt32(ZSocketOption.PROBE_ROUTER) == 1;
            set => SetOption(ZSocketOption.PROBE_ROUTER, value ? 1 : 0);
        }

        /// <summary>
        ///     Gets or sets the maximum send or receive data rate for multicast transports (kbps). (Default = 100 kbps).
        /// </summary>
        public int MulticastRate
        {
            get => GetOptionInt32(ZSocketOption.RATE);
            set => SetOption(ZSocketOption.RATE, value);
        }

        /// <summary>
        ///     Gets or sets the underlying kernel receive buffer size for the current socket (bytes). (Default = 0, OS default).
        /// </summary>
        public int ReceiveBufferSize
        {
            get => GetOptionInt32(ZSocketOption.RCVBUF);
            set => SetOption(ZSocketOption.RCVBUF, value);
        }

        /// <summary>
        ///     Gets or sets the high water mark for inbound messages (number of messages). (Default = 0, no limit).
        /// </summary>
        public int ReceiveHighWatermark
        {
            get => GetOptionInt32(ZSocketOption.RCVHWM);
            set => SetOption(ZSocketOption.RCVHWM, value);
        }

        /// <summary>
        ///     Gets or sets the timeout for receive operations. (Default = <see cref="TimeSpan.MaxValue" />, infinite).
        /// </summary>
        public TimeSpan ReceiveTimeout
        {
            get => TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.RCVTIMEO));
            set => SetOption(ZSocketOption.RCVTIMEO, (int)value.TotalMilliseconds);
        }

        /// <summary>
        ///     Gets or sets the initial reconnection interval. (Default = 100 milliseconds).
        /// </summary>
        public TimeSpan ReconnectInterval
        {
            get => TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.RECONNECT_IVL));
            set => SetOption(ZSocketOption.RECONNECT_IVL, (int)value.TotalMilliseconds);
        }

        /// <summary>
        ///     Gets or sets the maximum reconnection interval. (Default = 0, only use <see cref="ReconnectInterval" />).
        /// </summary>
        public TimeSpan ReconnectIntervalMax
        {
            get => TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.RECONNECT_IVL_MAX));
            set => SetOption(ZSocketOption.RECONNECT_IVL_MAX, (int)value.TotalMilliseconds);
        }

        /// <summary>
        ///     Gets or sets the recovery interval for multicast transports. (Default = 10 seconds).
        /// </summary>
        public TimeSpan MulticastRecoveryInterval
        {
            get => TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.RECOVERY_IVL));
            set => SetOption(ZSocketOption.RECOVERY_IVL, (int)value.TotalMilliseconds);
        }

        public bool RequestCorrelate
        {
            get => GetOptionInt32(ZSocketOption.REQ_CORRELATE) == 1;
            set => SetOption(ZSocketOption.REQ_CORRELATE, value ? 1 : 0);
        }

        public bool RequestRelaxed
        {
            get => GetOptionInt32(ZSocketOption.REQ_RELAXED) == 1;
            set => SetOption(ZSocketOption.REQ_RELAXED, value ? 1 : 0);
        }

        public bool RouterHandover
        {
            get => GetOptionInt32(ZSocketOption.ROUTER_HANDOVER) == 1;
            set => SetOption(ZSocketOption.ROUTER_HANDOVER, value ? 1 : 0);
        }

        public RouterMandatory RouterMandatory
        {
            get => (RouterMandatory)GetOptionInt32(ZSocketOption.ROUTER_MANDATORY);
            set => SetOption(ZSocketOption.ROUTER_MANDATORY, (int)value);
        }

        public bool RouterRaw
        {
            get => GetOptionInt32(ZSocketOption.ROUTER_RAW) == 1;
            set => SetOption(ZSocketOption.ROUTER_RAW, value ? 1 : 0);
        }

        /// <summary>
        ///     Gets or sets the underlying kernel transmit buffer size for the current socket (bytes). (Default = 0, OS default).
        /// </summary>
        public int SendBufferSize
        {
            get => GetOptionInt32(ZSocketOption.SNDBUF);
            set => SetOption(ZSocketOption.SNDBUF, value);
        }

        /// <summary>
        ///     Gets or sets the high water mark for outbound messages (number of messages). (Default = 0, no limit).
        /// </summary>
        public int SendHighWatermark
        {
            get => GetOptionInt32(ZSocketOption.SNDHWM);
            set => SetOption(ZSocketOption.SNDHWM, value);
        }

        /// <summary>
        ///     Gets or sets the timeout for send operations. (Default = <see cref="TimeSpan.MaxValue" />, infinite).
        /// </summary>
        public TimeSpan SendTimeout
        {
            get => TimeSpan.FromMilliseconds(GetOptionInt32(ZSocketOption.SNDTIMEO));
            set => SetOption(ZSocketOption.SNDTIMEO, (int)value.TotalMilliseconds);
        }

        /// <summary>
        ///     Gets or sets the override value for the SO_KEEPALIVE TCP socket option. (where supported by OS). (Default = -1, OS
        ///     default).
        /// </summary>
        public TcpKeepaliveBehaviour TcpKeepAlive
        {
            get => (TcpKeepaliveBehaviour)GetOptionInt32(ZSocketOption.TCP_KEEPALIVE);
            set => SetOption(ZSocketOption.TCP_KEEPALIVE, (int)value);
        }

        /// <summary>
        ///     Gets or sets the override value for the 'TCP_KEEPCNT' socket option (where supported by OS). (Default = -1, OS
        ///     default).
        ///     The default value of '-1' means to skip any overrides and leave it to OS default.
        /// </summary>
        public int TcpKeepAliveCount
        {
            get => GetOptionInt32(ZSocketOption.TCP_KEEPALIVE_CNT);
            set => SetOption(ZSocketOption.TCP_KEEPALIVE_CNT, value);
        }

        /// <summary>
        ///     Gets or sets the override value for the TCP_KEEPCNT (or TCP_KEEPALIVE on some OS). (Default = -1, OS default).
        /// </summary>
        public int TcpKeepAliveIdle
        {
            get => GetOptionInt32(ZSocketOption.TCP_KEEPALIVE_IDLE);
            set => SetOption(ZSocketOption.TCP_KEEPALIVE_IDLE, value);
        }

        /// <summary>
        ///     Gets or sets the override value for the TCP_KEEPINTVL socket option (where supported by OS). (Default = -1, OS
        ///     default).
        /// </summary>
        public int TcpKeepAliveInterval
        {
            get => GetOptionInt32(ZSocketOption.TCP_KEEPALIVE_INTVL);
            set => SetOption(ZSocketOption.TCP_KEEPALIVE_INTVL, value);
        }

        public int TypeOfService
        {
            get => GetOptionInt32(ZSocketOption.TOS);
            set => SetOption(ZSocketOption.TOS, value);
        }

        public bool XPubVerbose
        {
            get => GetOptionInt32(ZSocketOption.XPUB_VERBOSE) == 1;
            set => SetOption(ZSocketOption.XPUB_VERBOSE, value ? 1 : 0);
        }

        public string ZAPDomain
        {
            get => GetOptionString(ZSocketOption.ZAP_DOMAIN);
            set => SetOption(ZSocketOption.ZAP_DOMAIN, value);
        }

        public bool IPv4Only
        {
            get => GetOptionInt32(ZSocketOption.IPV4_ONLY) == 1;
            set => SetOption(ZSocketOption.IPV4_ONLY, value ? 1 : 0);
        }

        public bool GetOption(ZSocketOption option, out byte[] value, int size)
        {
            value = null;

            var optionLength = size;
            using (var optionValue = DispoIntPtr.Alloc(optionLength))
            {
                if (!GetOptionInner(option, optionValue, ref optionLength, out _))
                    return false;
                value = new byte[optionLength];
                Marshal.Copy(optionValue, value, 0, optionLength);
                return true;

            }
        }

        public byte[] GetOptionBytes(ZSocketOption option, int size = MaxBinaryOptionSize)
        {
            if (GetOption(option, out var result, size)) return result;
            return null;
        }

        public bool GetOption(ZSocketOption option, out string value)
        {
            value = null;

            var optionLength = MaxBinaryOptionSize;
            using (var optionValue = DispoIntPtr.Alloc(optionLength))
            {
                if (GetOptionInner(option, optionValue, ref optionLength, out _))
                {
                    value = Marshal.PtrToStringAnsi(optionValue, optionLength);
                    return true;
                }

                return false;
            }
        }

        public string GetOptionString(ZSocketOption option)
        {
            if (GetOption(option, out string result)) return result;
            return null;
        }

        public bool GetOption(ZSocketOption option, out int value)
        {
            value = 0;

            var optionLength = Marshal.SizeOf(typeof(int));
            using (var optionValue = DispoIntPtr.Alloc(optionLength))
            {
                if (GetOptionInner(option, optionValue.Ptr, ref optionLength, out _))
                {
                    value = Marshal.ReadInt32(optionValue.Ptr);
                    return true;
                }

                return false;
            }
        }

        public int GetOptionInt32(ZSocketOption option)
        {
            return GetOption(option, out int result) ? result : 0;
        }

        public bool GetOption(ZSocketOption option, out uint value)
        {
            var result = GetOption(option, out int resultValue);
            value = (uint)resultValue;
            return result;
        }

        public uint GetOptionUInt32(ZSocketOption option)
        {
            return GetOption(option, out uint result) ? result : default(uint);
        }

        public bool GetOption(ZSocketOption option, out long value)
        {
            value = default(long);

            var optionLength = Marshal.SizeOf(typeof(long));
            using (var optionValue = DispoIntPtr.Alloc(optionLength))
            {
                if (!GetOptionInner(option, optionValue.Ptr, ref optionLength, out _))
                    return false;
                value = Marshal.ReadInt64(optionValue);
                return true;
            }
        }

        public long GetOptionInt64(ZSocketOption option)
        {
            if (GetOption(option, out long result)) return result;
            return default(long);
        }

        public bool GetOption(ZSocketOption option, out ulong value)
        {
            var result = GetOption(option, out long resultValue);
            value = (ulong)resultValue;
            return result;
        }

        public ulong GetOptionUInt64(ZSocketOption option)
        {
            if (GetOption(option, out ulong result)) return result;
            return default(ulong);
        }



        public bool SetOptionNull(ZSocketOption option)
        {
            return SetOptionInner(option, IntPtr.Zero, 0);
        }

        public bool SetOption(ZSocketOption option, byte[] value)
        {
            if (value == null) return SetOptionNull(option);

            var optionLength = /* Marshal.SizeOf(typeof(byte)) * */ value.Length;
            using (var optionValue = DispoIntPtr.Alloc(optionLength))
            {
                Marshal.Copy(value, 0, optionValue.Ptr, optionLength);

                return SetOptionInner(option, optionValue.Ptr, optionLength);
            }
        }

        public bool SetOption(ZSocketOption option, string value)
        {
            if (value == null) return SetOptionNull(option);

            using (var optionValue = DispoIntPtr.AllocString(value, out var optionLength))
            {
                return SetOptionInner(option, optionValue, optionLength);
            }
        }

        public bool SetOption(ZSocketOption option, int value)
        {
            var optionLength = Marshal.SizeOf(typeof(int));
            using (var optionValue = DispoIntPtr.Alloc(optionLength))
            {
                Marshal.WriteInt32(optionValue, value);

                return SetOptionInner(option, optionValue.Ptr, optionLength);
            }
        }

        public bool SetOption(ZSocketOption option, uint value)
        {
            return SetOption(option, (int)value);
        }

        public bool SetOption(ZSocketOption option, long value)
        {
            var optionLength = Marshal.SizeOf(typeof(long));
            using (var optionValue = DispoIntPtr.Alloc(optionLength))
            {
                Marshal.WriteInt64(optionValue, value);

                return SetOptionInner(option, optionValue.Ptr, optionLength);
            }
        }

        public bool SetOption(ZSocketOption option, ulong value)
        {
            return SetOption(option, (long)value);
        }



        #endregion

        #region Identity

        /// <summary>
        /// 格式化身份名称
        /// </summary>
        /// <param name="isService"></param>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public static string CreateRealName(bool isService, params string[] ranges)
        {
            var sb = new StringBuilder();
            sb.Append(isService ? "+<" : "+>");
            sb.Append(GlobalContext.ServiceRealName);
            foreach (var range in ranges)
            {
                if (range == null)
                    continue;
                sb.Append("-");
                sb.Append(range);
            }
            sb.Append("-");
            sb.Append(RandomOperate.Generate(4));
            return sb.ToString();
        }

        /// <summary>
        /// 格式化身份名称
        /// </summary>
        /// <param name="isService"></param>
        /// <returns></returns>
        public static byte[] CreateIdentity(bool isService = false)
        {
            var sb = new StringBuilder();
            sb.Append(isService ? "+<" : "+>");
            sb.Append(GlobalContext.ServiceRealName);
            sb.Append("-");
            sb.Append(RandomOperate.Generate(4));
            return sb.ToString().ToZeroBytes();
        }
        /// <summary>
        /// 格式化身份名称
        /// </summary>
        /// <param name="isService"></param>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public static byte[] CreateIdentity(bool isService, params string[] ranges)
        {
            var sb = new StringBuilder();
            sb.Append(isService ? "+<" : "+>");
            sb.Append(GlobalContext.ServiceRealName);
            foreach (var range in ranges)
            {
                if (range == null)
                    continue;
                sb.Append("-");
                sb.Append(range);
            }
            sb.Append("-");
            sb.Append(RandomOperate.Generate(4));
            return sb.ToString().ToZeroBytes();
        }
        #endregion

        #region state

        /// <summary>
        /// 关联的站点名称（仅用于MicroZero）
        /// </summary>
        public string StationName
        {
            get;
            set;
        }

        /// <summary>
        /// 是否使用中（仅用于MicroZero）
        /// </summary>
        public bool IsUsing
        {
            get;
            set;
        }

        /// <summary>
        ///     是否为空
        /// </summary>
        public bool IsEmpty => SocketPtr == IntPtr.Zero;

        /// <summary>
        /// 状态
        /// </summary>
        public SocketState State { get; private set; }

        /// <summary>
        /// 最后错误
        /// </summary>
        private ZError _error;

        /// <summary>
        /// 最后错误
        /// </summary>
        public ZError LastError => _error;
        /// <summary>
        /// 最后错误
        /// </summary>
        /// <returns></returns>
        public ZError GetLastError()
        {
            return _error = ZError.GetLastErr();
        }

        /// <summary>
        /// 状态
        /// </summary>
        [Flags]
        public enum SocketState
        {
            /// <summary>
            /// 未确定
            /// </summary>
            None = 0x0,
            /// <summary>
            /// 对象已生成
            /// </summary>
            Create = 0x1,
            /// <summary>
            /// 连接
            /// </summary>
            Connection = 0x2,
            /// <summary>
            /// 绑定
            /// </summary>
            Binding = 0x4,
            /// <summary>
            /// 连接方式开启
            /// </summary>
            Open = 0x8,
            /// <summary>
            /// 对象正常但因暂停而不可用
            /// </summary>
            Pause = 0x10,
            /// <summary>
            /// 正常关闭
            /// </summary>
            Close = 0x20,
            /// <summary>
            /// 对象已释放
            /// </summary>
            Free = 0x40,
            /// <summary>
            /// 损坏
            /// </summary>
            Failed = 0x1000
        }

        #endregion

        #region Navite

        private bool SendFrame(ZFrame frame, int flags)
        {
            if (zmq.msg_send(frame.Ptr, SocketPtr, flags) >= 0)
                return true;
            _error = ZError.GetLastErr();
            LogRecorderX.Error($"Send Error: {_error.Text} | {Endpoint} | Socket Ptr:{SocketPtr}");
            return false;
        }

        /// <summary>
        ///     Sends HARD bytes from a byte[n]. Please don't use SendBytes, use instead SendFrame.
        /// </summary>
        private bool SendBytesInner(byte[] buffer, int offset, int count, ZSocketFlags flags, out ZError error)
        {
            //EnsureNotDisposed();

            error = _error = ZError.None;

            // int zmq_send (void *socket, void *buf, size_t len, int flags);

            var pin = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var pinPtr = pin.AddrOfPinnedObject() + offset;

            while (-1 == (zmq.send(SocketPtr, pinPtr, count, (int)flags)))
            {
                error = _error = ZError.GetLastErr();

                if (error.IsError(ZError.Code.EINTR))
                {
                    error = _error = null;
                    continue;
                }
                pin.Free();
                return false;
            }

            pin.Free();
            return true;
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private bool RecvFrame(out ZFrame frame, int flags)
        {
            _error = null;
            frame = ZFrame.CreateEmpty();
            if (zmq.msg_recv(frame.Ptr, SocketPtr, flags) != -1)
                return true;
            _error = ZError.GetLastErr();
            frame.Dispose();
            LogRecorderX.Error($"Recv Error: {_error.Text} | {Endpoint} | {SocketPtr}");
            return false;
        }

        /// <summary>
        ///     Receives HARD bytes into a new byte[n]. Please don't use ReceiveBytes, use instead ReceiveFrame.
        /// </summary>
        private int ReceiveBytes(byte[] buffer, int offset, int count, ZSocketFlags flags, out ZError error)
        {
            //EnsureNotDisposed();

            error = _error = ZError.None;

            // int zmq_recv(void* socket, void* buf, size_t len, int flags);

            var pin = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var pinPtr = pin.AddrOfPinnedObject() + offset;

            int length;
            while (-1 == (length = zmq.recv(SocketPtr, pinPtr, count, (int)flags)))
            {
                error = _error = ZError.GetLastErr();
                if (error.IsError(ZError.Code.EINTR))
                {
                    error = _error = null;
                    continue;
                }

                break;
            }

            pin.Free();
            return length;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        private bool Initialize(out ZError error)
        {
            SocketPtr = zmq.socket(Context.ContextPtr, (int)SocketType);
            if (IntPtr.Zero == SocketPtr)
            {
                State = SocketState.Failed;
                error = _error = ZError.GetLastErr();
                return false;
            }
            State = SocketState.Create;
            error = _error = null;
            ZContext.AddSocket(this);
            return true;
        }

        /// <summary>
        ///     Close the current socket.
        /// </summary>
        private bool CloseInner(out ZError error)
        {
            error = _error = ZError.None;
            if (State.HasFlag(SocketState.Free) || State.HasFlag(SocketState.Failed))
                return false;
            try
            {
                var success = true;
                error = _error = ZError.None;
                if (SocketPtr != IntPtr.Zero)
                {
                    if (State.HasFlag(SocketState.Connection))
                        Disconnect(Endpoint, out _);
                    if (State.HasFlag(SocketState.Binding))
                        Unbind(Endpoint, out _);

                    if (-1 == zmq.close(SocketPtr))
                    {
                        error = _error = ZError.GetLastErr();
                        LogRecorderX.Error($"Socket Close Error:{_error}");
                        State |= SocketState.Failed;
                        success = false;
                    }
                }
                State |= SocketState.Free;
                SocketPtr = IntPtr.Zero;
                ZContext.RemoveSocket(this);
                return success;
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e);
                error = new ZError(ZError.Code.ENETDOWN);
            }
            return false;
        }
        /// <summary>
        ///     Connect the specified endpoint.
        /// </summary>
        /// <param name="endpoint">
        ///     A string consisting of a transport and an address, formatted as
        ///     <c><em>transport</em>://<em>address</em></c>.
        /// </param>
        /// <param name="error"></param>
        [SecurityCritical]//捕获c++异常
        [HandleProcessCorruptedStateExceptions]//捕获c++异常
        private bool ConnectInner(string endpoint, out ZError error)
        {
            error = _error = null;
            if (State.HasFlag(SocketState.Close) || State.HasFlag(SocketState.Connection) || State.HasFlag(SocketState.Failed))
                return false;
            try
            {
                if (string.IsNullOrWhiteSpace(endpoint))
                    throw new ArgumentException("IsNullOrWhiteSpace", nameof(endpoint));

                using (var endpointPtr = DispoIntPtr.AllocString(endpoint))
                {
                    if (-1 == zmq.connect(SocketPtr, endpointPtr))
                    {
                        error = _error = ZError.GetLastErr();
                        State |= SocketState.Failed;
                        return false;
                    }
                }
                State |= SocketState.Connection;
                Endpoint = endpoint;
                return true;
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e);
                error = new ZError(ZError.Code.ENETDOWN);
            }
            return false;
        }
        /// <summary>
        ///     Disconnect the specified endpoint.
        /// </summary>
        /// <param name="endpoint">
        ///     A string consisting of a transport and an address, formatted as
        ///     <c><em>transport</em>://<em>address</em></c>.
        /// </param>
        /// <param name="error"></param>
        [SecurityCritical]//捕获c++异常
        [HandleProcessCorruptedStateExceptions]//捕获c++异常
        private bool DisconnectInner(string endpoint, out ZError error)
        {
            error = _error = null;
            if (State.HasFlag(SocketState.Close) || !State.HasFlag(SocketState.Connection) || State.HasFlag(SocketState.Failed))
                return false;
            try
            {

                if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentException("IsNullOrWhiteSpace", nameof(endpoint));

                using (var endpointPtr = DispoIntPtr.AllocString(endpoint))
                {
                    try
                    {
                        if (-1 == zmq.disconnect(SocketPtr, endpointPtr))
                        {
                            error = _error = ZError.GetLastErr();
                            State |= SocketState.Failed;
                            return false;
                        }
                    }
                    catch
                    {
                        error = _error = ZError.GetLastErr();
                        return false;
                    }
                }
                State |= SocketState.Close;
                return true;
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e);
                error = new ZError(ZError.Code.ENETDOWN);
            }
            return false;
        }


        /// <summary>
        ///     Bind the specified endpoint.
        /// </summary>
        /// <param name="endpoint">
        ///     A string consisting of a transport and an address, formatted as
        ///     <c><em>transport</em>://<em>address</em></c>.
        /// </param>
        /// <param name="error"></param>
        private bool BindInner(string endpoint, out ZError error)
        {
            error = _error = null;
            if (State.HasFlag(SocketState.Close) || State.HasFlag(SocketState.Binding) || State.HasFlag(SocketState.Failed))
                return false;
            if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentException("IsNullOrWhiteSpace", nameof(endpoint));

            using (var endpointPtr = DispoIntPtr.AllocString(endpoint))
            {
                if (-1 == zmq.bind(SocketPtr, endpointPtr))
                {
                    error = _error = ZError.GetLastErr();
                    State |= SocketState.Failed;
                    return false;
                }
            }
            State = SocketState.Binding;
            LogRecorderX.SystemLog($"Bind:{endpoint}");
            Endpoint = endpoint;
            return true;
        }

        /// <summary>
        ///     Unbind the specified endpoint.
        /// </summary>
        /// <param name="endpoint">
        ///     A string consisting of a transport and an address, formatted as
        ///     <c><em>transport</em>://<em>address</em></c>.
        /// </param>
        /// <param name="error"></param>
        private bool UnbindInner(string endpoint, out ZError error)
        {
            error = _error = null;
            if (State.HasFlag(SocketState.Close) || !State.HasFlag(SocketState.Binding) || State.HasFlag(SocketState.Failed))
                return false;
            if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentException("IsNullOrWhiteSpace", nameof(endpoint));

            using (var endpointPtr = DispoIntPtr.AllocString(endpoint))
            {
                if (-1 == zmq.unbind(SocketPtr, endpointPtr))
                {
                    error = _error = ZError.GetLastErr();
                    State |= SocketState.Failed;
                    return false;
                }
            }
            State |= SocketState.Close;
            LogRecorderX.SystemLog($"Unbind:{endpoint}");
            return true;
        }
        private bool SetOptionInner(ZSocketOption option, IntPtr optionValue, int optionLength)
        {
            if (-1 != zmq.setsockopt(SocketPtr, (int)option, optionValue, optionLength))
                return true;
            _error = ZError.GetLastErr();
            if (!_error.IsError(ZError.Code.EINTR))
                return false;
            _error = null;

            return true;
        }
        private bool GetOptionInner(ZSocketOption option, IntPtr optionValue, ref int optionLength, out ZError error)
        {
            using (var optionLengthP = DispoIntPtr.Alloc(IntPtr.Size))
            {
                switch (IntPtr.Size)
                {
                    case 4:
                        Marshal.WriteInt32(optionLengthP.Ptr, optionLength);
                        break;
                    case 8:
                        Marshal.WriteInt64(optionLengthP.Ptr, optionLength);
                        break;
                    default:
                        _error = error = new ZError(ZError.Code.NOSUPPORT);
                        return false;
                }


                if (zmq.getsockopt(SocketPtr, (int)option, optionValue, optionLengthP.Ptr) == -1)
                {
                    _error = error = ZError.GetLastErr();
                    return false;
                }

                switch (IntPtr.Size)
                {
                    case 4:
                        optionLength = Marshal.ReadInt32(optionLengthP.Ptr);
                        break;
                    case 8:
                        optionLength = (int)Marshal.ReadInt64(optionLengthP.Ptr);
                        break;
                    default:
                        _error = error = new ZError(ZError.Code.NOSUPPORT);
                        return false;
                }
            }
            _error = error = ZError.None;
            return true;
        }

        #endregion

#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
    }
}