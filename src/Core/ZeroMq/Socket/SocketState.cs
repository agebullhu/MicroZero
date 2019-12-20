using System;

namespace ZeroMQ
{
    /// <summary>
    /// 状态
    /// </summary>
    [Flags]
    public enum SocketState
    {
        /// <summary>
        /// 未确定
        /// </summary>
        None = 0x0,
        /// <summary>
        /// 对象已生成
        /// </summary>
        Create = 0x1,
        /// <summary>
        /// 连接
        /// </summary>
        Connection = 0x2,
        /// <summary>
        /// 绑定
        /// </summary>
        Binding = 0x4,
        /// <summary>
        /// 连接方式开启
        /// </summary>
        Open = 0x8,
        /// <summary>
        /// 对象正常但因暂停而不可用
        /// </summary>
        Pause = 0x10,
        /// <summary>
        /// 正常关闭
        /// </summary>
        Close = 0x20,
        /// <summary>
        /// 对象已释放
        /// </summary>
        Free = 0x40,
        /// <summary>
        /// 损坏
        /// </summary>
        Failed = 0x1000
    }
}