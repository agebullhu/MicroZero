namespace ZeroMQ
{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using lib;


    public unsafe class WinZPoll : MemoryCheck, IZmqPool
    {
        public ZSocket[] Sockets { get; set; }

        public int Size { get; set; }
        public int TimeoutMs { get; set; }
        private ZError error;
        public ZError ZError => error ?? (error = ZError.GetLastErr());
        public DispoIntPtr Ptr { get; set; }
        public void Prepare(ZSocket[] sockets, ZPollEvent events)
        {
            TimeoutMs = 1000;
            Sockets = sockets;
            error = null;
            Size = sockets.Length;
            Ptr = DispoIntPtr.Alloc(sizeof(zmq_pollitem_windows_t) * sockets.Length);
            zmq_pollitem_windows_t* natives = (zmq_pollitem_windows_t*)Ptr.Ptr;
            for (int i = 0; i < Size; ++i)
            {
                zmq_pollitem_windows_t* native = natives + i;
                native->SocketPtr = sockets[i].SocketPtr;
                native->Events = (short)(events);
                native->ReadyEvents = (short)ZPollEvent.None;
            }
        }
        public bool Poll()
        {
            error = null;

            zmq_pollitem_windows_t* natives = (zmq_pollitem_windows_t*)Ptr.Ptr;
            var state = zmq.poll(natives, Size, TimeoutMs);
            return state > 0;
        }

        public bool CheckIn(int index, out ZMessage message)
        {
            zmq_pollitem_windows_t* native = ((zmq_pollitem_windows_t*)Ptr.Ptr) + index;
            if (native->ReadyEvents == 0)
            {
                message = null;
                return false;
            }
            if (!((ZPollEvent)native->ReadyEvents).HasFlag(ZPollEvent.In))
            {
                message = null;
                return false;
            }
            return Sockets[index].Recv(out message,1);
        }

        public bool CheckOut(int index, out ZMessage message)
        {
            zmq_pollitem_windows_t* native = ((zmq_pollitem_windows_t*)Ptr.Ptr) + index;
            if (native->ReadyEvents == 0)
            {
                message = null;
                return false;
            }
            if (!((ZPollEvent)native->ReadyEvents).HasFlag(ZPollEvent.Out))
            {
                message = null;
                return false;
            }
            return Sockets[index].Recv(out message, 1);
        }

        protected override void DoDispose()
        {
            Ptr.Dispose();
            foreach (var socket in Sockets)
            {
                socket.Close();
                socket.Dispose();
            }
        }
    }
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
				bool result = false;
				int count = items.Count();
				int timeoutMs = !timeout.HasValue ? -1 : (int)timeout.Value.TotalMilliseconds;

				zmq_pollitem_windows_t* natives = stackalloc zmq_pollitem_windows_t[count];
				// fixed (zmq_pollitem_windows_t* natives = managedArray) {

				for (int i = 0; i < count; ++i)
				{
					ZSocket socket = sockets.ElementAt(i);
					ZPollItem item = items.ElementAt(i);
					zmq_pollitem_windows_t* native = natives + i;

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

				for (int i = 0; i < count; ++i)
				{
					ZPollItem item = items.ElementAt(i);
					zmq_pollitem_windows_t* native = natives + i;

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
				bool result = false;
				int timeoutMs = !timeout.HasValue ? -1 : (int)timeout.Value.TotalMilliseconds;

				zmq_pollitem_windows_t* native = stackalloc zmq_pollitem_windows_t[1];
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