namespace ZeroMQ
{
    using lib;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static partial class ZPollItems
	{

		// unsafe native delegate
	    public delegate bool PollManyDelegate(IEnumerable<ZSocket> sockets, IEnumerable<ZPollItem> items, ZPollEvent pollFirst, out ZError error, TimeSpan? timeoutMs);

	    public static readonly PollManyDelegate PollMany;

		// unsafe native delegate
		internal delegate bool PollSingleDelegate(ZSocket socket, ZPollItem item, ZPollEvent pollFirst, out ZError error, TimeSpan? timeout);
		internal static readonly PollSingleDelegate PollSingle;

		static ZPollItems()
		{
			// Initialize static Fields
			Platform.SetupImplementation(typeof(ZPollItems));
		}
        public static bool PollIn(this ZSocket socket, ZPollItem item, out ZMessage incoming, out ZError error, TimeSpan? timeout = null)
		{
			incoming = null;
			return Poll(socket, item, ZPollEvent.In, ref incoming, out error, timeout);
		}

		public static bool PollOut(this ZSocket socket, ZPollItem item, ZMessage outgoing, out ZError error, TimeSpan? timeout = null)
		{
			if (outgoing == null)
			{
				throw new ArgumentNullException(nameof(outgoing));
			}
			return Poll(socket, item, ZPollEvent.Out, ref outgoing, out error, timeout);
		}

		public static bool Poll(this ZSocket socket, ZPollItem item, ZPollEvent pollEvents, ref ZMessage message, out ZError error, TimeSpan? timeout = null)
		{
			if (PollSingle(socket, item, pollEvents, out error, timeout))
			{

				if (PollSingleResult(socket, item, pollEvents, ref message))
				{

					return true;
				}
				error = new ZError(ZError.Code.EAGAIN);
			}
			return false;
		}

		internal static bool PollSingleResult(ZSocket socket, ZPollItem item, ZPollEvent pollEvents, ref ZMessage message)
		{
			bool shouldReceive = item.ReceiveMessage != null && ((pollEvents & ZPollEvent.In) == ZPollEvent.In);
			bool shouldSend = item.SendMessage != null && ((pollEvents & ZPollEvent.Out) == ZPollEvent.Out);

			if (pollEvents == ZPollEvent.In)
			{

				if (!shouldReceive)
				{
					throw new InvalidOperationException("No ReceiveMessage delegate set for Poll.In");
				}

				if (OnReceiveMessage(socket, item, out message))
				{

					if (!shouldSend)
					{
						return true;
					}

					if (OnSendMessage(socket, item, message))
					{
						return true;
					}
				}
			}
			else if (pollEvents == ZPollEvent.Out)
			{

				if (!shouldSend)
				{
					throw new InvalidOperationException("No SendMessage delegate set for Poll.Out");
				}

				if (OnSendMessage(socket, item, message))
				{

					if (!shouldReceive)
					{
						return true;
					}

					if (OnReceiveMessage(socket, item, out message))
					{
						return true;
					}
				}
			}
			return false;
		}

		internal static bool OnReceiveMessage(ZSocket socket, ZPollItem item, out ZMessage message)
		{
			message = null;

			if (((ZPollEvent)item.ReadyEvents & ZPollEvent.In) == ZPollEvent.In)
			{

                if (item.ReceiveMessage == null)
                {
                    // throw?
                }
                else if (item.ReceiveMessage(socket, out message, out ZError recvWorkerE))
                {
                    // what to do?

                    return true;
                }
            }
			return false;
		}

		internal static bool OnSendMessage(ZSocket socket, ZPollItem item, ZMessage message)
		{
			if (((ZPollEvent)item.ReadyEvents & ZPollEvent.Out) == ZPollEvent.Out)
			{

                if (item.SendMessage == null)
                {
                    // throw?
                }
                else if (item.SendMessage(socket, message, out ZError sendWorkerE))
                {
                    // what to do?

                    return true;
                }
            }
			return false;
		}

		public static bool PollIn(this IEnumerable<ZSocket> sockets, IEnumerable<ZPollItem> items, out ZMessage[] incoming, out ZError error, TimeSpan? timeout = null)
		{
			incoming = null;
			return Poll(sockets, items, ZPollEvent.In, ref incoming, out error, timeout);
		}

		public static bool PollOut(this IEnumerable<ZSocket> sockets, IEnumerable<ZPollItem> items, ZMessage[] outgoing, out ZError error, TimeSpan? timeout = null)
		{
			if (outgoing == null)
			{
				throw new ArgumentNullException(nameof(outgoing));
			}
			return Poll(sockets, items, ZPollEvent.Out, ref outgoing, out error, timeout);
		}

		public static bool Poll(this IEnumerable<ZSocket> sockets, IEnumerable<ZPollItem> items, ZPollEvent pollEvents, ref ZMessage[] messages, out ZError error, TimeSpan? timeout = null)
		{
			if (PollMany(sockets, items, pollEvents, out error, timeout))
			{

				if (PollManyResult(sockets, items, pollEvents, ref messages))
				{

					return true;
			    }
			    error = new ZError(ZError.Code.EAGAIN);
            }
			return false;
		}

		internal static bool PollManyResult(IEnumerable<ZSocket> sockets, IEnumerable<ZPollItem> items, ZPollEvent pollEvents, ref ZMessage[] messages)
		{
			int count = items.Count();
			int readyCount = 0;

			bool send = messages != null && ((pollEvents & ZPollEvent.Out) == ZPollEvent.Out);
			bool receive = ((pollEvents & ZPollEvent.In) == ZPollEvent.In);

			ZMessage[] incoming = null;
			if (receive)
			{
				incoming = new ZMessage[count];
			}

			for (int i = 0; i < count; ++i)
			{
				ZSocket socket = sockets.ElementAt(i);
				ZPollItem item = items.ElementAt(i);
				ZMessage message = send ? messages[i] : null;

				if (PollSingleResult(socket, item, pollEvents, ref message))
				{
					++readyCount;
				}
				if (receive)
				{
					incoming[i] = message;
				}
			}

			if (receive)
			{
				messages = incoming;
			}
			return readyCount > 0;
		}

	}
}