namespace ZeroMQ
{
	using System;
    /// <summary>
    /// pool事件
    /// </summary>
	[Flags]
	public enum ZPollEvent : short
	{
        /// <summary>
        /// 无
        /// </summary>
		None = 0x0,
        /// <summary>
        /// 数据入
        /// </summary>
	    In = 0x1,
        /// <summary>
        /// 数据出
        /// </summary>
	    Out = 0x2,
        /// <summary>
        /// 两者
        /// </summary>
        InOut = 0x3,
	    /// <summary>
	    /// 数据错误
	    /// </summary>
	    Err = 0x4,
	    /// <summary>
	    /// 全部
	    /// </summary>
	    All = 0x7,
    }
}