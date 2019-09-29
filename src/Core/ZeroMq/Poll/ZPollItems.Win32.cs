namespace ZeroMQ
{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using lib;


    public static partial class ZPollItems // : IDisposable, IList<ZmqPollItem>
	{
        public static class Win32
		{
			internal static unsafe bool PollMany(
				IEnumerable<ZSocket> sockets, 
				IEnumerable<ZPollItem> items, ZPollEvent pollEvents, 
				out ZError error, TimeSpan? timeout = null)
			{
				error = default(ZError);
				var result = false;
				var count = items.Count();
				var timeoutMs = !timeout.HasValue ? -1 : (int)timeout.Value.TotalMilliseconds;

				var natives = stackalloc zmq_pollitem_windows_t[count];
				// fixed (zmq_pollitem_windows_t* natives = managedArray) {

				for (var i = 0; i < count; ++i)
				{
					var socket = sockets.ElementAt(i);
					var item = items.ElementAt(i);
					var native = natives + i;

					native->SocketPtr = socket.SocketPtr;
					native->Events = (short)(item.Events & pollEvents);
					native->ReadyEvents = (short)ZPollEvent.None;
				}

				while (!(result = (-1 != zmq.poll(natives, count, timeoutMs))))
				{
					error = ZError.GetLastErr();

					// No Signalling on Windows
					/* if (error == ZmqError.EINTR) {
						error = ZmqError.DEFAULT;
						continue;
					} */
					break;
				}

				for (var i = 0; i < count; ++i)
				{
					var item = items.ElementAt(i);
					var native = natives + i;

					item.ReadyEvents = (ZPollEvent)native->ReadyEvents;
				}
				// }

				return result;
			}

			internal static unsafe bool PollSingle(
				ZSocket socket, 
				ZPollItem item, ZPollEvent pollEvents,
				out ZError error, TimeSpan? timeout = null)
			{
				error = default(ZError);
				var result = false;
				var timeoutMs = !timeout.HasValue ? -1 : (int)timeout.Value.TotalMilliseconds;

				var native = stackalloc zmq_pollitem_windows_t[1];
				// fixed (zmq_pollitem_windows_t* native = managedArray) {

				native->SocketPtr = socket.SocketPtr;
				native->Events = (short)(item.Events & pollEvents);
				native->ReadyEvents = (short)ZPollEvent.None;

				while (!(result = (-1 != zmq.poll(native, 1, timeoutMs))))
				{
					error = ZError.GetLastErr();

					/* if (error == ZmqError.EINTR) 
					{
						error = default(ZmqError);
						continue;
					} */
					break;
				}

				item.ReadyEvents = (ZPollEvent)native->ReadyEvents;
				// }

				return result;
			}
		}
	}
}