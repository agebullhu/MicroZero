using ZeroMQ.lib;

namespace ZeroMQ
{
    /// <summary>
    /// Linux平台使用
    /// </summary>
    public class ZPollBase : MemoryCheck
    {

        /// <summary>
        /// 对应的Socket集合
        /// </summary>
        public ZSocket[] Sockets { get; set; }

        /// <summary>
        /// Sockt数量
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// 超时
        /// </summary>
        public int TimeoutMs => ZSocket.Option.PoolTimeOut;

        /// <summary>
        /// 错误对象
        /// </summary>
        protected ZError error;

        /// <summary>
        /// 错误对象
        /// </summary>
        public ZError ZError => error ?? (error = ZError.GetLastErr());

        /// <summary>
        /// 非托管句柄
        /// </summary>
        public DispoIntPtr Ptr { get; set; }

        /// <summary>
        /// 析构
        /// </summary>
        protected override void DoDispose()
        {
            Ptr?.Dispose();
            if (Sockets == null) return;
            foreach (var socket in Sockets)
            {
                socket.Dispose();
            }
        }
    }
}