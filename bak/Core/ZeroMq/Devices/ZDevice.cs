#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
namespace ZeroMQ
{
    using System;
    using System.Threading;

    /// <summary>
    /// Forwards messages received by a front-end socket to a back-end socket, from which
    /// they are then sent.
    /// </summary>
    /// <remarks>
    /// The base implementation of <see cref="ZDevice"/> is <b>not</b> threadsafe. Do not construct
    /// a device with sockets that were created in separate threads or separate contexts.
    /// </remarks>
    public abstract class ZDevice : ZThread
	{
		/// <summary>
		/// The polling interval in milliseconds.
		/// </summary>
		protected readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(500);

		/// <summary>
		/// The ZContext reference, to not become finalized
		/// </summary>
		protected readonly ZContext Context;

		/// <summary>
		/// The frontend socket that will normally pass messages to <see cref="BackendSocket"/>.
		/// </summary>
		public ZSocket FrontendSocket;

		/// <summary>
		/// The backend socket that will normally receive messages from (and possibly send replies to) <see cref="FrontendSocket"/>.
		/// </summary>
		public ZSocket BackendSocket;

		/// <summary>
		/// You are using ZContext.Current!
		/// </summary>
		protected ZDevice()
			: this (ZContext.Current)
		{ }

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		protected ZDevice(ZContext context)
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
		{
			Context = context;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="frontendType"></param>
        /// <param name="backendType"></param>
        protected ZDevice(ZSocketType frontendType, ZSocketType backendType)
			: this (ZContext.Current, frontendType, backendType)
		{ }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="frontendType"></param>
        /// <param name="backendType"></param>
		protected ZDevice(ZContext context, ZSocketType frontendType, ZSocketType backendType)
		{
			Context = context;

            if (!Initialize(frontendType, backendType, out var error))
            {
                throw new ZException(error);
            }
        }

		protected bool Initialize(ZSocketType frontendType, ZSocketType backendType, out ZError error)
		{
			error = null;

			/* if (frontendType == ZSocketType.None && backendType == ZSocketType.None)
			{
				throw new InvalidOperationException();
			} /**/

			if (frontendType != ZSocketType.None)
			{
				if (null == (FrontendSocket = ZSocket.Create(Context, frontendType, out error)))
				{
					return false;
				}
				FrontendSetup = new ZSocketSetup(FrontendSocket);
			}

			if (backendType != ZSocketType.None)
			{
				if (null == (BackendSocket = ZSocket.Create(Context, backendType, out error)))
				{
					return false;
				}
				BackendSetup = new ZSocketSetup(BackendSocket);
			}

			return true;
		}

		/// <summary>
		/// Gets a <see cref="ZSocketSetup"/> for configuring the frontend socket.
		/// </summary>
		public ZSocketSetup BackendSetup { get; protected set; }

		/// <summary>
		/// Gets a <see cref="ZSocketSetup"/> for configuring the backend socket.
		/// </summary>
		public ZSocketSetup FrontendSetup { get; protected set; }

		/*/ <summary>
		/// Gets a <see cref="ManualResetEvent"/> that can be used to block while the device is running.
		/// </summary>
		public ManualResetEvent DoneEvent { get; private set; } /**/

		/*/ <summary>
		/// Gets an <see cref="AutoResetEvent"/> that is pulsed after every Poll call.
		/// </summary>
		public AutoResetEvent PollerPulse
		{
				get { return _poller.Pulse; }
		}*/

		/// <summary>
		/// Initializes the frontend and backend sockets. Called automatically when starting the device.
		/// If called multiple times, will only execute once.
		/// </summary>
		public virtual void Initialize()
		{
			EnsureNotDisposed();

		    FrontendSetup?.Configure();
		    BackendSetup?.Configure();
		}

		/// <summary>
		/// Start the device in the current thread. Should be used by implementations of the method.
		/// </summary>
		protected override void Run()
		{
			EnsureNotDisposed();

			Initialize();

			ZSocket[] sockets;
			ZPollItem[] polls;
			if (FrontendSocket != null && BackendSocket != null)
			{
				sockets = new[] {
					FrontendSocket,
					BackendSocket
				};
				polls = new[] {
					ZPollItem.Create(FrontendHandler),
					ZPollItem.Create(BackendHandler)
				};
			}
			else if (FrontendSocket != null)
			{
				sockets = new[] {
					FrontendSocket
				}; 
				polls = new[] {
					ZPollItem.Create(FrontendHandler)
				};
			}
			else
			{
				sockets = new[] {
					BackendSocket
				};
				polls = new[] {
					ZPollItem.Create(BackendHandler)
				};
			}

			/* ZPollItem[] polls;
			{
				var pollItems = new List<ZPollItem>();
				switch (FrontendSocket.SocketType)
				{
					case ZSocketType.Code.ROUTER:
					case ZSocketType.Code.XSUB:
					case ZSocketType.Code.PUSH:
						// case ZSocketType.Code.STREAM:
						pollItems.Add(new ZPollItem(FrontendSocket, ZPoll.In)
						{
								ReceiveMessage = FrontendHandler
						});

						break;
				}
				switch (BackendSocket.SocketType)
				{
					case ZSocketType.Code.DEALER:
						// case ZSocketType.Code.STREAM:
						pollItems.Add(new ZPollItem(BackendSocket, ZPoll.In)
						{
								ReceiveMessage = BackendHandler
						});

						break;
				}
				polls = pollItems.ToArray();
			} */

			// Because of using ZmqSocket.Forward, this field will always be null
			ZMessage[] lastMessageFrames = null;

            FrontendSetup?.BindConnect();
            BackendSetup?.BindConnect();

            ZError error = null;
			try
			{
				while (!Cancellor.IsCancellationRequested)
				{
                    if (sockets.Poll(polls, ZPollEvent.In, ref lastMessageFrames, out error, PollingInterval)) 
                        continue;
                    if (error.IsError(ZError.Code.EAGAIN))
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    if (error.IsError(ZError.Code.ETERM))
                    {
                        break;
                    }

                    // EFAULT
                    throw new ZException(error);
                }
			}
			catch (ZException)
			{
				// Swallow any exceptions thrown while stopping
				if (!Cancellor.IsCancellationRequested)
				{
					throw;
				}
			}

		    FrontendSetup?.UnbindDisconnect();
		    BackendSetup?.UnbindDisconnect();

		    if (error.IsError(ZError.Code.ETERM))
            {
				Close();
			}
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        /// <param name="error"></param>
        /// <returns></returns>
		protected abstract bool FrontendHandler(ZSocket socket, out ZMessage message, out ZError error);

		/// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="message"></param>
        /// <param name="error"></param>
        /// <returns></returns>
		protected abstract bool BackendHandler(ZSocket args, out ZMessage message, out ZError error);

		/// <summary>
		/// Stops the device and releases the underlying sockets. Optionally disposes of managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
                FrontendSocket?.Dispose();
                BackendSocket?.Dispose();
            }

			base.Dispose(disposing);
		}

	}
}

#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释