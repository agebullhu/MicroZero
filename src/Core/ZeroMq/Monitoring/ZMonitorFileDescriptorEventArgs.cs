using System.Runtime.InteropServices;

namespace ZeroMQ.Monitoring
{
    using System;

    /// <summary>
    /// Provides data for <see cref="ZMonitor.Connected"/>, <see cref="ZMonitor.Listening"/>, <see cref="ZMonitor.Accepted"/>, <see cref="ZMonitor.Closed"/> and <see cref="ZMonitor.Disconnected"/> events.
    /// </summary>
    public class ZMonitorFileDescriptorEventArgs : ZMonitorEventArgs
	{
		internal ZMonitorFileDescriptorEventArgs(ZMonitor monitor, ZMonitorEventData data)
			: base(monitor, data)
		{
#if NETSTANDARD2_0
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				FileDescriptor_Posix = data.EventValue;
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				FileDescriptor_Windows = new IntPtr(data.EventValue);
			}
			else
			{
				throw new PlatformNotSupportedException();
			}
#else
		    FileDescriptor_Windows = new IntPtr(data.EventValue);
#endif
        }

		/// <summary>
		/// Gets the monitor descriptor (Posix)
		/// </summary>
		public int FileDescriptor_Posix { get; private set; }

		/// <summary>
		/// Gets the monitor descriptor (Windows)
		/// </summary>
		public IntPtr FileDescriptor_Windows { get; private set; }
	}
}