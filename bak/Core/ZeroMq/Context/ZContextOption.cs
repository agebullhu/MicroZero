namespace ZeroMQ
{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    public enum ZContextOption : int
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
	{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		IO_THREADS = 1,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		MAX_SOCKETS = 2,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		SOCKET_LIMIT = 3,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		THREAD_PRIORITY = 3,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		THREAD_SCHED_POLICY = 4,
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
		IPV6 = 42	// in zmq.h ZMQ_IPV6 is in the socket options section
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
	}
}