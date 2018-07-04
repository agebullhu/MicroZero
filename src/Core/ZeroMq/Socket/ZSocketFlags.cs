using System;

namespace ZeroMQ
{
#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
    [Flags]
	public enum ZSocketFlags : int
	{
		/// <summary>
		/// No socket flags are specified.
		/// </summary>
		None = 0,

		/// <summary>
		/// The operation should be performed in non-blocking mode.
		/// </summary>
		DontWait = 1,

		/// <summary>
		/// The message being sent is a multi-part message, and that further message parts are to follow.
		/// </summary>
		More = 2
	}
}
