namespace ZeroMQ.lib
{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct zmq_pollitem_posix_t // : zmq_pollitem_i
	{
	    public zmq_pollitem_posix_t(IntPtr socket, ZPollEvent pollEvents)
		{
			if (socket == IntPtr.Zero)
			{
				throw new ArgumentException("Expected a valid socket handle.", nameof(socket));
			}

			SocketPtr = socket;
			FileDescriptor = 0;
			Events = (short)pollEvents;
			ReadyEvents = (short)ZPollEvent.None;
		}

		public IntPtr SocketPtr { get; set; }

	    public int FileDescriptor { get; set; }

	    public short Events { get; set; }

	    public short ReadyEvents { get; set; }
	}
}