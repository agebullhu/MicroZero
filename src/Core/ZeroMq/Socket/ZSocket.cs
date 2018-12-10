using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Agebull.Common.Logging;
using Gboxt.Common.DataModel;
using ZeroMQ.lib;

namespace ZeroMQ
{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    /// <summary>
    ///     Sends and receives messages, single frames and byte frames across ZeroMQ.
    /// </summary>
    public class ZSocket : MemoryCheck
    {
        #region Option
        /// <summary>
        /// 配置
        /// </summary>
        public static SocketOption Option = new SocketOption();
        
#if UNMANAGE_MONEY_CHECK
        protected override string TypeName => nameof(ZMessage);
#endif
        /// <summary>
        /// 关联的站点名称（仅用于ZeroNet）
        /// </summary>
        public string StationName
        {
            get;
            set;
        }
        /// <summary>
        /// 是否使用中（仅用于ZeroNet）
        /// </summary>
        public bool IsUsing
        {
            get;
            set;
        }

        public static List<ZSocket> AliveSockets = new List<ZSocket>();


        /// <summary>
        ///     已绑定地址
        /// </summary>
        public List<string> Binds = new List<string>();

        /// <summary>
        ///     已连接对象
        /// </summary>
        public List<string> Connects = new List<string>();
        #endregion

        #region Create

        /// <summary>
        /// 构建套接字
        /// </summary>
        /// <param name="address"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ZSocket CreateServiceSocket(string address, ZSocketType type)
        {
            if (!ZContext.IsAlive)
                return null;
            var socket = Create(type, out var error);
            if (error != null)
            {
                LogRecorder.Error($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
                return null;
            }
            //socket.SetOption(ZSocketOption.RECONNECT_IVL, Option.ReconnectIvl);
            //socket.SetOption(ZSocketOption.RECONNECT_IVL_MAX, Option.ReconnectIvlMax);

            socket.SetOption(ZSocketOption.LINGER, Option.Linger);
            socket.SetOption(ZSocketOption.RCVTIMEO, Option.RecvTimeout);
            socket.SetOption(ZSocketOption.SNDTIMEO, Option.SendTimeout);

            socket.SetOption(ZSocketOption.BACKLOG, Option.Backlog);
            //socket.SetOption(ZSocketOption.HEARTBEAT_IVL, Option.HeartbeatIvl);
            //socket.SetOption(ZSocketOption.HEARTBEAT_TIMEOUT, Option.HeartbeatTimeout);
            //socket.SetOption(ZSocketOption.HEARTBEAT_TTL, Option.HeartbeatTtl);

            //socket.SetOption(ZSocketOption.TCP_KEEPALIVE, Option.TcpKeepalive);
            //socket.SetOption(ZSocketOption.TCP_KEEPALIVE_IDLE, Option.TcpKeepaliveIdle);
            //socket.SetOption(ZSocketOption.TCP_KEEPALIVE_INTVL, Option.TcpKeepaliveIntvl);

            if (socket.Bind(address, out error))
                return socket;
            LogRecorder.SystemLog($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
            socket.Close();
            socket.Dispose();
            return null;
        }

        /// <summary>
        /// 构建套接字
        /// </summary>
        /// <param name="address"></param>
        /// <param name="type"></param>
        /// <param name="identity"></param>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        public static ZSocket CreateClientSocket(string address, ZSocketType type, byte[] identity, byte[] subscribe)
        {
            if (!ZContext.IsAlive)
                return null;
            var socket = Create(type, out var error);
            if (error != null)
            {
                LogRecorder.Error($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
                return null;
            }
            socket.SetOption(ZSocketOption.IDENTITY, identity ?? Encoding.ASCII.GetBytes("+>" + RandomOperate.Generate(8)));
            socket.SetOption(ZSocketOption.CONNECT_TIMEOUT, Option.ConnectTimeout);
            socket.SetOption(ZSocketOption.RECONNECT_IVL, Option.ReconnectIvl);
            socket.SetOption(ZSocketOption.RECONNECT_IVL_MAX, Option.ReconnectIvlMax);
            socket.SetOption(ZSocketOption.LINGER, Option.Linger);
            socket.SetOption(ZSocketOption.RCVTIMEO, Option.RecvTimeout);
            if (type != ZSocketType.SUB)
            {
                socket.SetOption(ZSocketOption.SNDTIMEO, Option.SendTimeout);
            }
            else
            {
                socket.SetOption(ZSocketOption.SUBSCRIBE, subscribe);
            }
            if (socket.Connect(address, out error))
                return socket;
            LogRecorder.Error($"CreateSocket: {error.Text} > Address:{address} > type:{type}.");
            socket.Close();
            socket.Dispose();
            return null;
        }

        /// <summary>
        /// 构建套接字
        /// </summary>
        /// <param name="address"></param>
        /// <param name="type"></param>
        /// <param name="identity"></param>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        public static ZSocket CreateClientSocket(string address, ZSocketType type, byte[] identity = null, string subscribe = null)
        {
            if (!ZContext.IsAlive)
                return null;
            return CreateClientSocket(address, type, identity, Encoding.ASCII.GetBytes(subscribe ?? ""));
        }
        /// <summary>
        /// 构建套接字
        /// </summary>
        /// <param name="address"></param>
        /// <param name="identity"></param>
        /// <returns></returns>
        public static ZSocket CreateDealerSocket(string address, byte[] identity = null)
        {
            return CreateClientSocket(address, ZSocketType.DEALER, identity, new byte[0]);
        }


        /// <summary>
        /// 构建套接字
        /// </summary>
        /// <param name="address"></param>
        /// <param name="identity"></param>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        public static ZSocket CreateSubscriberSocket(string address, byte[] identity = null, string subscribe = "")
        {
            return CreateClientSocket(address, ZSocketType.SUB, identity, Encoding.ASCII.GetBytes(subscribe ?? ""));
        }

        #endregion

        #region MySend

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns>1 发送成功 0 发送失败 -1部分发送</returns>
        public bool SendTo(ZMessage message)
        {
            using (message)
            {
                _error = null;
                var array = message.ToArray();
                int i = 0;
                bool first = true;
                int retry = 5;
                for (; i < array.Length - 1; ++i)
                {
                    if (!Send(array[i], first, 2, ref retry))
                    {
                        return false;
                    }
                    first = false;
                }
                return Send(array[i], first, 1, ref retry);
            }
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="array">消息</param>
        /// <returns>是否发送成功</returns>
        public bool SendTo(params byte[][] array)
        {
            if (array.Length == 0)
                return false;
            _error = null;
            int i = 0;
            bool first = true;
            int retry = 5;
            for (; i < array.Length - 1; ++i)
            {
                using (var f = new ZFrame(array[i]))
                    if (!Send(f, first, 2, ref retry))
                        return false;
                first = false;
            }
            using (var f = new ZFrame(array[i]))
                return Send(f, first, 1, ref retry);
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="array">消息</param>
        /// <returns>是否发送成功</returns>
        public bool SendTo(params ZFrame[] array)
        {
            _error = null;
            int i = 0;
            bool first = true;
            int retry = 5;
            bool success = true;
            for (; i < array.Length - 1; ++i)
            {
                if (success)
                {
                    success = Send(array[i], first, 2, ref retry);
                    if (success)
                        first = false;
                }
                array[i].Dispose();
            }
            if (success)
                success = Send(array[i], first, 1, ref retry);
            array[i].Dispose();
            return success;
        }

        private bool Send(ZFrame frame, bool first, int flags, ref int retry)
        {
            while (zmq.msg_send(frame.Ptr, SocketPtr, flags) == -1)
            {
                _error = ZError.GetLastErr();
                if (first || !Equals(_error, ZError.EAGAIN) || retry <= 0)
                    return false;
                --retry;
            }
            return true;
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="message"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public bool Recv(out ZMessage message, int flags = 0)
        {
            message = new ZMessage();
            bool more;
            bool first = true;
            int retry = 5;
            do
            {
                _error = null;
                var frame = ZFrame.CreateEmpty();
                while (zmq.msg_recv(frame.Ptr, SocketPtr, flags) == -1)
                {
                    _error = ZError.GetLastErr();
                    if (!first && Equals(_error, ZError.EAGAIN) && retry > 0)
                    {
                        --retry;
                        more = true;
                        continue;
                    }
                    frame.Close();
                    return false;
                }
                message.Add(frame);
                first = false;
                more = ReceiveMore;
                //retry = 0;
            } while (more);

            return true;
        }
        #endregion

        #region Option


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

        public byte[] ConnectRID
        {
            get => GetOptionBytes(ZSocketOption.CONNECT_RID);
            set => SetOption(ZSocketOption.CONNECT_RID, value);
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
        ///     which are NOT printed (in Console.WriteLine)!
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
            get => GetOptionInt64(ZSocketOption.MAX_MSG_SIZE);
            set => SetOption(ZSocketOption.MAX_MSG_SIZE, value);
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

        private bool GetOption(ZSocketOption option, IntPtr optionValue, ref int optionLength)
        {
            //EnsureNotDisposed();

            using (var optionLengthP = DispoIntPtr.Alloc(IntPtr.Size))
            {
                if (IntPtr.Size == 4)
                    Marshal.WriteInt32(optionLengthP.Ptr, optionLength);
                else if (IntPtr.Size == 8)
                    Marshal.WriteInt64(optionLengthP.Ptr, optionLength);
                else
                    throw new PlatformNotSupportedException();


                while (zmq.getsockopt(SocketPtr, (int)option, optionValue, optionLengthP.Ptr) == -1)
                {
                    _error = ZError.GetLastErr();

                    if (Equals(_error, ZError.EINTR))
                    {
                        _error = default(ZError);
                        continue;
                    }

                    throw new ZException(_error);
                }

                if (IntPtr.Size == 4)
                    optionLength = Marshal.ReadInt32(optionLengthP.Ptr);
                else if (IntPtr.Size == 8)
                    optionLength = (int)Marshal.ReadInt64(optionLengthP.Ptr);
                else
                    throw new PlatformNotSupportedException();
            }

            return true;
        }

        public bool GetOption(ZSocketOption option, out byte[] value, int size)
        {
            value = null;

            var optionLength = size;
            using (var optionValue = DispoIntPtr.Alloc(optionLength))
            {
                if (GetOption(option, optionValue, ref optionLength))
                {
                    value = new byte[optionLength];
                    Marshal.Copy(optionValue, value, 0, optionLength);
                    return true;
                }

                return false;
            }
        }

        public byte[] GetOptionBytes(ZSocketOption option, int size = MaxBinaryOptionSize)
        {
            if (GetOption(option, out byte[] result, size)) return result;
            return null;
        }

        public bool GetOption(ZSocketOption option, out string value)
        {
            value = null;

            var optionLength = MaxBinaryOptionSize;
            using (var optionValue = DispoIntPtr.Alloc(optionLength))
            {
                if (GetOption(option, optionValue, ref optionLength))
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
            value = default(int);

            var optionLength = Marshal.SizeOf(typeof(int));
            using (var optionValue = DispoIntPtr.Alloc(optionLength))
            {
                if (GetOption(option, optionValue.Ptr, ref optionLength))
                {
                    value = Marshal.ReadInt32(optionValue.Ptr);
                    return true;
                }

                return false;
            }
        }

        public int GetOptionInt32(ZSocketOption option)
        {
            if (GetOption(option, out int result)) return result;
            return default(int);
        }

        public bool GetOption(ZSocketOption option, out uint value)
        {
            var result = GetOption(option, out int resultValue);
            value = (uint)resultValue;
            return result;
        }

        public uint GetOptionUInt32(ZSocketOption option)
        {
            if (GetOption(option, out uint result)) return result;
            return default(uint);
        }

        public bool GetOption(ZSocketOption option, out long value)
        {
            value = default(long);

            var optionLength = Marshal.SizeOf(typeof(long));
            using (var optionValue = DispoIntPtr.Alloc(optionLength))
            {
                if (GetOption(option, optionValue.Ptr, ref optionLength))
                {
                    value = Marshal.ReadInt64(optionValue);
                    return true;
                }

                return false;
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


        private bool SetOption(ZSocketOption option, IntPtr optionValue, int optionLength)
        {
            //EnsureNotDisposed();


            while (-1 == zmq.setsockopt(SocketPtr, (int)option, optionValue, optionLength))
            {
                _error = ZError.GetLastErr();

                if (!Equals(_error, ZError.EINTR)) return false;
                _error = default(ZError);
            }

            return true;
        }

        public bool SetOptionNull(ZSocketOption option)
        {
            return SetOption(option, IntPtr.Zero, 0);
        }

        public bool SetOption(ZSocketOption option, byte[] value)
        {
            if (value == null) return SetOptionNull(option);

            var optionLength = /* Marshal.SizeOf(typeof(byte)) * */ value.Length;
            using (var optionValue = DispoIntPtr.Alloc(optionLength))
            {
                Marshal.Copy(value, 0, optionValue.Ptr, optionLength);

                return SetOption(option, optionValue.Ptr, optionLength);
            }
        }

        public bool SetOption(ZSocketOption option, string value)
        {
            if (value == null) return SetOptionNull(option);

            using (var optionValue = DispoIntPtr.AllocString(value, out int optionLength))
            {
                return SetOption(option, optionValue, optionLength);
            }
        }

        public bool SetOption(ZSocketOption option, int value)
        {
            var optionLength = Marshal.SizeOf(typeof(int));
            using (var optionValue = DispoIntPtr.Alloc(optionLength))
            {
                Marshal.WriteInt32(optionValue, value);

                return SetOption(option, optionValue.Ptr, optionLength);
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

                return SetOption(option, optionValue.Ptr, optionLength);
            }
        }

        public bool SetOption(ZSocketOption option, ulong value)
        {
            return SetOption(option, (long)value);
        }
        #endregion

        #region 状态

        /// <summary>
        ///     Create a <see cref="ZSocket" /> instance.
        ///     You are using ZContext.Current!
        /// </summary>
        /// <returns>
        ///     <see cref="ZSocket" />
        /// </returns>
        public ZSocket(ZSocketType socketType) : this(ZContext.Current, socketType)
        {
        }

        /// <summary>
        ///     Create a <see cref="ZSocket" /> instance.
        /// </summary>
        /// <returns>
        ///     <see cref="ZSocket" />
        /// </returns>
        public ZSocket(ZContext context, ZSocketType socketType)
        {
            Context = context;
            SocketType = socketType;

            if (!Initialize(out _error)) throw new ZException(_error);
        }

        /// <summary>
        ///     Create a <see cref="ZSocket" /> instance.
        /// </summary>
        /// <returns>
        ///     <see cref="ZSocket" />
        /// </returns>
        public ZSocket(ZContext context, ZSocketType socketType, out ZError error)
        {
            Context = context;
            SocketType = socketType;

            Initialize(out error);
        }


        protected ZSocket()
        {
        }



        private ZError _error;

        public ZError LastError => _error;

        public ZError GetLastError()
        {
            return _error = ZError.GetLastErr();
        }
        #endregion
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
        // From options.hpp: unsigned char identity [256];
        private const int MaxBinaryOptionSize = 256;

        public const int BinaryKeySize = 32;



        /// <summary>
        ///     是否为空
        /// </summary>
        public bool IsEmpty => SocketPtr == IntPtr.Zero;

        public ZContext Context { get; private set; }

        public IntPtr SocketPtr { get; private set; }

        /// <summary>
        ///     Gets the <see cref="ZeroMQ.ZSocketType" /> value for the current socket.
        /// </summary>
        public ZSocketType SocketType { get; private set; }


        /// <summary>
        ///     Gets a value indicating whether the multi-part message currently being read has more message parts to follow.
        /// </summary>
        public bool ReceiveMore => GetOptionInt32(ZSocketOption.RCVMORE) == 1;

        public string LastEndpoint => GetOptionString(ZSocketOption.LAST_ENDPOINT);



        /// <summary>
        ///     Create a <see cref="ZSocket" /> instance.
        /// </summary>
        /// <returns>
        ///     <see cref="ZSocket" />
        /// </returns>
        public static ZSocket Create(ZContext context, ZSocketType socketType)
        {
            return new ZSocket(context, socketType);
        }

        /// <summary>
        ///     Create a <see cref="ZSocket" /> instance.
        /// </summary>
        /// <returns>
        ///     <see cref="ZSocket" />
        /// </returns>
        public static ZSocket Create(ZContext context, ZSocketType socketType, out ZError error)
        {
            var socket = new ZSocket
            {
                Context = context,
                SocketType = socketType
            };

            return !socket.Initialize(out error) ? default(ZSocket) : socket;
        }

        /// <summary>
        ///     Create a <see cref="ZSocket" /> instance.
        /// </summary>
        /// <returns>
        ///     <see cref="ZSocket" />
        /// </returns>
        public static ZSocket Create(ZSocketType socketType)
        {
            return new ZSocket(socketType);
        }

        /// <summary>
        ///     Create a <see cref="ZSocket" /> instance.
        /// </summary>
        /// <returns>
        ///     <see cref="ZSocket" />
        /// </returns>
        public static ZSocket Create(ZSocketType socketType, out ZError error)
        {
            return new ZSocket(ZContext.Current, socketType, out error);
        }

        protected bool Initialize(out ZError error)
        {
            if (IntPtr.Zero == (SocketPtr = zmq.socket(Context.ContextPtr, (int)SocketType)))
            {
                error = _error = ZError.GetLastErr();
                return false;
            }
            error = _error = default(ZError);
            try
            {
                AliveSockets.Add(this);

            }
            catch { }
            return true;
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="ZSocket" /> class.
        /// </summary>
        ~ZSocket()
        {
            Dispose();
        }

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
            error = _error = ZError.None;
            if (SocketPtr != IntPtr.Zero)
            {
                if (ZContext.IsAlive)
                {
                    foreach (var con in Connects.ToArray())
                    {
                        Disconnect(con, out _);
                    }

                    foreach (var bin in Binds.ToArray())
                    {
                        Unbind(bin, out _);
                    }

                    if (-1 == zmq.close(SocketPtr))
                    {
                        error = _error = ZError.GetLastErr();
                        return false;
                    }
                }

            }

            try
            {
                AliveSockets.Remove(this);
            }
            catch
            {
            }
            finally
            {
                SocketPtr = IntPtr.Zero;
                GC.SuppressFinalize(this);
                GC.Collect();
            }
            return true;
        }

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
            //EnsureNotDisposed();

            error = _error = default(ZError);

            if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentException("IsNullOrWhiteSpace", nameof(endpoint));

            using (var endpointPtr = DispoIntPtr.AllocString(endpoint))
            {
                if (-1 == zmq.bind(SocketPtr, endpointPtr))
                {
                    error = _error = ZError.GetLastErr();
                    return false;
                }
            }
            LogRecorder.SystemLog($"Bind:{endpoint}");
            Binds.Add(endpoint);

            return true;
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
            //EnsureNotDisposed();

            error = _error = default(ZError);

            if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentException("IsNullOrWhiteSpace", nameof(endpoint));

            using (var endpointPtr = DispoIntPtr.AllocString(endpoint))
            {
                if (-1 == zmq.unbind(SocketPtr, endpointPtr))
                {
                    error = _error = ZError.GetLastErr();
                    return false;
                }
            }

            LogRecorder.SystemLog($"Unbind:{endpoint}");
            Binds.Remove(endpoint);
            LogRecorder.RecordStackTrace(endpoint);
            return true;
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
        public bool Connect(string endpoint, out ZError error)
        {
            //EnsureNotDisposed();

            error = _error = default(ZError);

            if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentException("IsNullOrWhiteSpace", nameof(endpoint));

            using (var endpointPtr = DispoIntPtr.AllocString(endpoint))
            {
                if (-1 == zmq.connect(SocketPtr, endpointPtr))
                {
                    error = _error = ZError.GetLastErr();
                    return false;
                }
            }

            Connects.Add(endpoint);

            return true;
        }

        /// <summary>
        ///     Disconnect the specified endpoint.
        /// </summary>
        public void Disconnect(string endpoint)
        {
            if (!Disconnect(endpoint, out _error)) throw new ZException(_error);
        }

        /// <summary>
        ///     Disconnect the specified endpoint.
        /// </summary>
        /// <param name="endpoint">
        ///     A string consisting of a transport and an address, formatted as
        ///     <c><em>transport</em>://<em>address</em></c>.
        /// </param>
        /// <param name="error"></param>
        public bool Disconnect(string endpoint, out ZError error)
        {
            //EnsureNotDisposed();

            error = _error = default(ZError);

            if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentException("IsNullOrWhiteSpace", nameof(endpoint));

            using (var endpointPtr = DispoIntPtr.AllocString(endpoint))
            {
                try
                {
                    if (-1 == zmq.disconnect(SocketPtr, endpointPtr))
                    {
                        error = _error = ZError.GetLastErr();
                        return false;
                    }
                }
                catch
                {
                    error = _error = ZError.GetLastErr();
                    return false;
                }
            }

            Connects.Remove(endpoint);
            return true;
        }

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

        /// <summary>
        ///     Receives HARD bytes into a new byte[n]. Please don't use ReceiveBytes, use instead ReceiveFrame.
        /// </summary>
        public int ReceiveBytes(byte[] buffer, int offset, int count, ZSocketFlags flags, out ZError error)
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
                if (Equals(error, ZError.EINTR))
                {
                    error = _error = default(ZError);
                    continue;
                }

                break;
            }

            pin.Free();
            return length;
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
            //EnsureNotDisposed();

            error = _error = ZError.None;

            // int zmq_send (void *socket, void *buf, size_t len, int flags);

            var pin = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var pinPtr = pin.AddrOfPinnedObject() + offset;

            while (-1 == (zmq.send(SocketPtr, pinPtr, count, (int)flags)))
            {
                error = _error = ZError.GetLastErr();

                if (Equals(error, ZError.EINTR))
                {
                    error = _error = default(ZError);
                    continue;
                }

                pin.Free();
                return false;
            }

            pin.Free();
            return true;
        }

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
        } // just Send*

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
                if (Equals(error, ZError.EAGAIN) && (flags & ZSocketFlags.DontWait) == ZSocketFlags.DontWait) break;
                return null;
            }

            return frames;
        }

        public bool ReceiveFrames<ListT>(ref int framesToReceive, ref ListT frames, ZSocketFlags flags,
            out ZError error)
            where ListT : IList<ZFrame>, new()
        {
            //EnsureNotDisposed();

            error = _error = default(ZError);
            flags = flags | ZSocketFlags.More;

            do
            {
                var frame = ZFrame.CreateEmpty();

                if (framesToReceive == 1) flags = flags & ~ZSocketFlags.More;

                while (-1 == zmq.msg_recv(frame.Ptr, SocketPtr, (int)flags))
                {
                    error = _error = ZError.GetLastErr();

                    if (Equals(error, ZError.EINTR))
                    {
                        error = _error = default(ZError);
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

            error = _error = default(ZError);

            while (-1 == zmq.msg_send(frame.Ptr, SocketPtr, (int)flags))
            {
                error = _error = ZError.GetLastErr();

                if (Equals(error, ZError.EINTR))
                {
                    error = _error = default(ZError);
                    continue;
                }

                return false;
            }

            // Tell IDisposable to not unallocate zmq_msg
            frame.Close();
            return true;
        }

        public bool Forward(ZSocket destination, out ZMessage message, out ZError error)
        {
            //EnsureNotDisposed();

            error = _error = default(ZError);
            message = null; // message is always null

            using (var msg = ZFrame.CreateEmpty())
            {
                bool more;
                do
                {
                    while (-1 == zmq.msg_recv(msg.Ptr, SocketPtr, (int)ZSocketFlags.None))
                    {
                        error = _error = ZError.GetLastErr();

                        if (!Equals(error, ZError.EINTR)) return false;
                        error = _error = default(ZError);
                    }

                    // will have to receive more?
                    more = ReceiveMore;

                    // sending scope
                    while (-1 != zmq.msg_send(msg.Ptr, destination.SocketPtr,
                               more ? (int)ZSocketFlags.More : (int)ZSocketFlags.None))
                    {
                        error = _error = ZError.GetLastErr();

                        if (!Equals(error, ZError.EINTR))
                            return false;
                        error = _error = default(ZError);
                    }

                    // msg.Dismiss
                } while (more);
            } // using (msg) -> Dispose

            return true;
        }


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
            SetOption(ZSocketOption.SUBSCRIBE, ZContext.Encoding.GetBytes(prefix));
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

        private void EnsureNotDisposed()
        {
            if (!ZContext.IsAlive || SocketPtr == IntPtr.Zero)
                throw new ObjectDisposedException(GetType().FullName);
        }
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
    }
}