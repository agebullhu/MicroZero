using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agebull.Common.Logging;
using ZeroMQ.lib;

namespace ZeroMQ
{
    /// <summary>
    /// Creates <see cref="ZSocket"/> instances within a process boundary.
    /// </summary>
    public sealed class ZContext : MemoryCheck
    {
#if UNMANAGE_MONEY_CHECK
        protected override string TypeName => nameof(ZContext);
#endif
        /// <summary>
		/// Gets and protected sets the default Encoding.
		/// Note: Do not set the Encoding after ZContext.Create.
		/// </summary>
		public static Encoding Encoding => Encoding.UTF8;

        /// <summary>
        /// 当前活动的连接
        /// </summary>
        public static List<ZSocket> AliveSockets = new List<ZSocket>();


        /// <summary>
        /// 初始化
        /// </summary>
	    public static void Initialize()
        {
            if (!IsAlive)
            {
                _current = new ZContext
                {
                    ThreadPoolSize = 4,
                    MaxSockets = 4096
                };
            }
        }

        /// <summary>
        ///     关闭
        /// </summary>
        public static void Destroy()
        {
            if (_current == null)
                return;
#if UNMANAGE_MONEY_CHECK
            Trace();
#endif
            var tmp = _current;
            _current = null;
            tmp.Dispose();
        }

        /// <summary>
        /// 是否存活
        /// </summary>
        public static bool IsAlive => _current != null && _current._contextPtr != IntPtr.Zero;

        /// <summary>
        /// 当前默认实例
        /// </summary>
        private static ZContext _current;
        /// <summary>
        /// 当前默认实例
        /// </summary>
		public static ZContext Current
        {
            get
            {
                if (!IsAlive)
                {
                    throw new Exception("do not run Initialize?");
                }
                return _current;
            }
        }

        /// <summary>
        /// Create a <see cref="ZContext"/> instance.
        /// </summary>
        /// <returns><see cref="ZContext"/></returns>
        private ZContext()
        {
            _contextPtr = zmq.ctx_new();

            if (_contextPtr == IntPtr.Zero)
            {
                throw new InvalidProgramException("zmq_ctx_new");
            }
        }

        private IntPtr _contextPtr;

        /// <summary>
        /// Gets a handle to the native ZeroMQ context.
        /// </summary>
        public IntPtr ContextPtr => _contextPtr;
        ///
        public static bool Has(string capability)
        {
            using (var capabilityPtr = MarshalPtr.AllocString(capability))
            {
                if (0 < zmq.has(capabilityPtr))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 代理
        /// </summary>
        /// <param name="frontend"></param>
        /// <param name="backend"></param>
        public static void Proxy(ZSocket frontend, ZSocket backend)
        {
            Proxy(frontend, backend, null);
        }
        /// <summary>
        /// 代理
        /// </summary>
        /// <param name="frontend"></param>
        /// <param name="backend"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool Proxy(ZSocket frontend, ZSocket backend, out ZError error)
        {
            return Proxy(frontend, backend, null, out error);
        }
        /// <summary>
        /// 代理
        /// </summary>
        /// <param name="frontend"></param>
        /// <param name="backend"></param>
        /// <param name="capture"></param>
        public static void Proxy(ZSocket frontend, ZSocket backend, ZSocket capture)
        {
            if (!Proxy(frontend, backend, capture, out var error))
            {
                if (error.IsError(ZError.Code.ETERM))
                {
                    return; // Interrupted
                }
                throw new ZException(error);
            }
        }
        /// <summary>
        /// 代理
        /// </summary>
        /// <param name="frontend"></param>
        /// <param name="backend"></param>
        /// <param name="capture"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool Proxy(ZSocket frontend, ZSocket backend, ZSocket capture, out ZError error)
        {
            error = ZError.None;

            while (-1 == zmq.proxy(frontend.SocketPtr, backend.SocketPtr, capture?.SocketPtr ?? IntPtr.Zero))
            {
                error = ZError.GetLastErr();

                if (error.Number != ZError.Code.EINTR)
                    return false;
                error = null;
            }
            return true;
        }
        /// <summary>
        /// 代理
        /// </summary>
        /// <param name="frontend"></param>
        /// <param name="backend"></param>
        /// <param name="control"></param>
        public static void ProxySteerable(ZSocket frontend, ZSocket backend, ZSocket control)
        {
            ProxySteerable(frontend, backend, null, control);
        }
        /// <summary>
        /// 代理
        /// </summary>
        /// <param name="frontend"></param>
        /// <param name="backend"></param>
        /// <param name="control"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool ProxySteerable(ZSocket frontend, ZSocket backend, ZSocket control, out ZError error)
        {
            return ProxySteerable(frontend, backend, null, control, out error);
        }
        /// <summary>
        /// 代理
        /// </summary>
        /// <param name="frontend"></param>
        /// <param name="backend"></param>
        /// <param name="capture"></param>
        /// <param name="control"></param>
        public static void ProxySteerable(ZSocket frontend, ZSocket backend, ZSocket capture, ZSocket control)
        {
            if (!ProxySteerable(frontend, backend, capture, control, out var error))
            {
                if (error.IsError(ZError.Code.ETERM))
                {
                    return; // Interrupted
                }
                throw new ZException(error);
            }
        }
        /// <summary>
        /// 代理
        /// </summary>
        /// <param name="frontend"></param>
        /// <param name="backend"></param>
        /// <param name="capture"></param>
        /// <param name="control"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool ProxySteerable(ZSocket frontend, ZSocket backend, ZSocket capture, ZSocket control, out ZError error)
        {
            error = ZError.None;

            while (-1 == zmq.proxy_steerable(frontend.SocketPtr, backend.SocketPtr, capture?.SocketPtr ?? IntPtr.Zero, control?.SocketPtr ?? IntPtr.Zero))
            {
                error = ZError.GetLastErr();

                if (error.IsError(ZError.Code.EINTR))
                {
                    error = null;
                    continue;
                }
                return false;
            }
            return true;
        }
        /// <summary>
        /// 设置Zmq上下文配置
        /// </summary>
        /// <param name="option"></param>
        /// <param name="optionValue"></param>
        public void SetOption(ZContextOption option, int optionValue)
        {
            EnsureNotDisposed();

            var rc = zmq.ctx_set(_contextPtr, (Int32)option, optionValue);
            if (rc != -1) return;
            var error = ZError.GetLastErr();

            if (error.IsError(ZError.Code.EINVAL))
            {
                throw new ArgumentOutOfRangeException(
                    $"The requested option optionName \"{option}\" is invalid.");
            }
            throw new ZException(error);
        }
        /// <summary>
        /// 取Zmq上下文配置
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public int GetOption(ZContextOption option)
        {
            EnsureNotDisposed();

            var rc = zmq.ctx_get(_contextPtr, (Int32)option);
            if (rc != -1) return rc;
            var error = ZError.GetLastErr();

            if (error.IsError(ZError.Code.EINVAL))
            {
                throw new ArgumentOutOfRangeException(
                    $"The requested option optionName \"{option}\" is invalid.");
            }
            throw new ZException(error);
        }

        /// <summary>
        /// Gets or sets the size of the thread pool for the current context (default = 1).
        /// </summary>
        public int ThreadPoolSize
        {
            get => GetOption(ZContextOption.IO_THREADS);
            set => SetOption(ZContextOption.IO_THREADS, value);
        }

        /// <summary>
        /// Gets or sets the maximum number of sockets for the current context (default = 1024).
        /// </summary>
        public int MaxSockets
        {
            get => GetOption(ZContextOption.MAX_SOCKETS);
            set => SetOption(ZContextOption.MAX_SOCKETS, value);
        }

        /// <summary>
        /// Gets or sets the supported socket protocol(s) when using TCP transports. (Default = ProtocolType.Ipv4).
        /// </summary>
        public bool IPv6Enabled
        {
            get => GetOption(ZContextOption.IPV6) == 1;
            set => SetOption(ZContextOption.IPV6, value ? 1 : 0);
        }

        /// <summary>
        /// Shutdown the ZeroMQ context.
        /// </summary>
        public static bool Shutdown()
        {
            return IsAlive && _current.Shutdown(out _);
        }

        /// <summary>
        /// Shutdown the ZeroMQ context.
        /// </summary>
        public bool Shutdown(out ZError error)
        {
            error = null;

            if (_contextPtr == IntPtr.Zero)
                return true;
            if (-1 != zmq.ctx_shutdown(_contextPtr))
                return true;
            error = ZError.GetLastErr();
            return false;
        }

        /// <inheritdoc />
        protected override void DoDispose()
        {
            Terminate(out _);
        }

        /// <summary>
        /// Terminate the ZeroMQ context.
        /// </summary>
        public static void AddSocket(ZSocket socket)
        {
            lock (AliveSockets)
                AliveSockets.Add(socket);
        }

        /// <summary>
        /// 从资源池中删除
        /// </summary>
        internal static void RemoveSocket(ZSocket socket)
        {
            lock (AliveSockets)
                AliveSockets.Remove(socket);
        }

        /// <summary>
        /// 终止所有网络资源
        /// </summary>
        public bool Terminate(out ZError error)
        {
            error = ZError.None;
            if (_contextPtr == IntPtr.Zero)
            {
                return true;
            }
            LogRecorder.SystemLog("Terminate the ZeroMQ context.");
            
            var ptr = _contextPtr;
            _contextPtr = IntPtr.Zero;
            ZSocket[] array;

            lock (AliveSockets)
                array = AliveSockets.Where(p => p != null && !p.IsDisposed).ToArray();

            foreach (var alive in array)
            {
                LogRecorder.SystemLog($"Endpoint : {alive.Endpoint}");
                alive.Dispose();
            }
            if (zmq.ctx_shutdown(ptr) != -1)
            {
                return zmq.ctx_term(ptr) != -1;
            }
            return true;
        }

        /// <summary>
        /// Terminate the ZeroMQ context.
        /// </summary>
        public void Terminate()
        {
            if (!Terminate(out var error))
            {
                throw new ZException(error);
            }
        }

        private void EnsureNotDisposed()
        {
            if (_contextPtr == IntPtr.Zero)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}