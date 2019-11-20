using System;

using ZeroMQ.lib;

namespace ZeroMQ.Monitoring
{

    /// <summary>
    /// Defines extension methods related to monitoring for <see cref="ZSocket"/> instances.
    /// </summary>
    public static class ZMonitors
	{
		/// <summary>
		/// Spawns a <see cref="ZSocketType.PAIR"/> socket that publishes all events for
		/// the specified socket over the inproc transport at the given endpoint.
		/// </summary>
		public static bool Monitor(this ZSocket socket, string endpoint)
		{
            if (!Monitor(socket, endpoint, ZMonitorEvents.AllEvents, out var error))
            {
                throw new ZException(error);
            }
            return true;
		}

		/// <summary>
		/// Spawns a <see cref="ZSocketType.PAIR"/> socket that publishes all events for
		/// the specified socket over the inproc transport at the given endpoint.
		/// </summary>
		public static bool Monitor(this ZSocket socket, string endpoint, out ZError error)
		{
			return Monitor(socket, endpoint, ZMonitorEvents.AllEvents, out error);
		}

		/// <summary>
		/// Spawns a <see cref="ZSocketType.PAIR"/> socket that publishes all events for
		/// the specified socket over the inproc transport at the given endpoint.
		/// </summary>
		public static bool Monitor(this ZSocket socket, string endpoint, ZMonitorEvents eventsToMonitor)
		{
            if (!Monitor(socket, endpoint, eventsToMonitor, out var error))
            {
                throw new ZException(error);
            }
            return true;
		}

		/// <summary>
		/// Spawns a <see cref="ZSocketType.PAIR"/> socket that publishes all events for
		/// the specified socket over the inproc transport at the given endpoint.
		/// </summary>
		public static bool Monitor(this ZSocket socket, string endpoint, ZMonitorEvents eventsToMonitor, out ZError error)
		{
			if (socket == null)
			{
				throw new ArgumentNullException(nameof(socket));
			}

			if (endpoint == null)
			{
				throw new ArgumentNullException(nameof(endpoint));
			}

			if (endpoint == string.Empty)
			{
				throw new ArgumentException("Unable to publish socket events to an empty endpoint.", nameof(endpoint));
			}

			error = ZError.None;

			using (var endpointPtr = DispoIntPtr.AllocString(endpoint))
			{
				while (-1 == zmq.socket_monitor(socket.SocketPtr, endpointPtr, (Int32)eventsToMonitor))
				{
					error = ZError.GetLastErr();

					if (error.IsError(ZError.Code.EINTR))
                    {
						error = null;
						continue;
					}

					return false;
				}
			}
			return true;
		}
	}
}