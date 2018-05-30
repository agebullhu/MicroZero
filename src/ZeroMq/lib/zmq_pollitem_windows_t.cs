namespace ZeroMQ.lib
{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct zmq_pollitem_windows_t // : zmq_pollitem_i
	{
        public zmq_pollitem_windows_t(IntPtr socket, ZPoll pollEvents)
		{
			if (socket == IntPtr.Zero)
			{
				throw new ArgumentException("Expected a valid socket handle.", nameof(socket));
			}

			SocketPtr = socket;
			FileDescriptor = IntPtr.Zero;
			Events = (short)pollEvents;
			ReadyEvents = 0;
		}

		public IntPtr SocketPtr { get; set; }

        public IntPtr FileDescriptor { get; set; }

        public short Events { get; set; }

        public short ReadyEvents { get; set; }
    }
}