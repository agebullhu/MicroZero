using ZeroMQ.lib;

namespace ZeroMQ
{
    public unsafe class WinZPoll : MemoryCheck, IZmqPool
    {
        public ZSocket[] Sockets { get; set; }

        public int Size { get; set; }
        public int TimeoutMs { get; set; } = 1000;
        private ZError error;
        public ZError ZError => error ?? (error = ZError.GetLastErr());
        public DispoIntPtr Ptr { get; set; }
        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="sockets"></param>
        /// <param name="events"></param>
        public void Prepare(ZSocket[] sockets, ZPollEvent events)
        {
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
        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="sockets"></param>
        /// <param name="events"></param>
        public void Prepare(ZPollEvent events, params ZSocket[] sockets)
        {
            Prepare(sockets, events);
        }

        /// <summary>
        /// 一次Pool
        /// </summary>
        /// <returns></returns>
        public bool Poll()
        {
            error = null;
            zmq_pollitem_windows_t* natives = (zmq_pollitem_windows_t*)Ptr.Ptr;
            var state = zmq.poll(natives, Size, TimeoutMs);
            return state > 0;
        }

        /// <summary>
        /// 检查下标是否有数据
        /// </summary>
        /// <param name="index"></param>
        /// <param name="message"></param>
        public bool CheckIn(int index, out ZMessage message)
        {
            zmq_pollitem_windows_t* native = ((zmq_pollitem_windows_t*)Ptr.Ptr) + index;
            if (native->ReadyEvents == 0)
            {
                message = null;
                return false;
            }

            if (((ZPollEvent) native->ReadyEvents).HasFlag(ZPollEvent.In))
                return Sockets[index].Recv(out message, 1);
            message = null;
            return false;
        }

        /// <summary>
        /// 检查下标是否有数据
        /// </summary>
        /// <param name="index"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool CheckOut(int index, out ZMessage message)
        {
            zmq_pollitem_windows_t* native = ((zmq_pollitem_windows_t*)Ptr.Ptr) + index;
            if (native->ReadyEvents == 0)
            {
                message = null;
                return false;
            }

            if (((ZPollEvent) native->ReadyEvents).HasFlag(ZPollEvent.Out))
                return Sockets[index].Recv(out message, 1);
            message = null;
            return false;
        }

        /// <summary>
        /// 析构
        /// </summary>
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
}