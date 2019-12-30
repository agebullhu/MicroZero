using System.Threading.Tasks;
using ZeroMQ.lib;

namespace ZeroMQ
{
    /// <summary>
    /// Linux平台使用
    /// </summary>
    public unsafe class LinuxZPoll : ZPollBase, IZmqPool
    {
        int IZmqPool.TimeoutMs => TimeoutMs;

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
            Ptr = MarshalPtr.Alloc(sizeof(zmq_pollitem_posix_t) * sockets.Length);
            var natives = (zmq_pollitem_posix_t*)Ptr.Ptr;
            for (var i = 0; i < Size; ++i)
            {
                var native = natives + i;
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
            Sockets = sockets;
            error = null;
            Size = sockets.Length;
            Ptr = MarshalPtr.Alloc(sizeof(zmq_pollitem_posix_t) * sockets.Length);
            var natives = (zmq_pollitem_posix_t*)Ptr.Ptr;
            for (var i = 0; i < Size; ++i)
            {
                var native = natives + i;
                native->SocketPtr = sockets[i].SocketPtr;
                native->Events = (short)(events);
                native->ReadyEvents = (short)ZPollEvent.None;
            }
        }
        
        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="events"></param>
        public void RePrepare(ZPollEvent events)
        {
            Ptr?.Dispose();
            Size = Sockets.Length;
            Ptr = MarshalPtr.Alloc(sizeof(zmq_pollitem_posix_t) * Sockets.Length);
            var natives = (zmq_pollitem_posix_t*)Ptr.Ptr;
            for (var i = 0; i < Size; ++i)
            {
                var native = natives + i;
                native->SocketPtr = Sockets[i].SocketPtr;
                native->Events = (short)(events);
                native->ReadyEvents = (short)ZPollEvent.None;
            }
        }

        /// <summary>
        /// 一次Pool
        /// </summary>
        /// <returns></returns>
        public bool Poll()
        {
            error = null;

            var natives = (zmq_pollitem_posix_t*)Ptr.Ptr;
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
            if (index > Sockets.Length)
            {
                message = null;
                return false;
            }
            var native = ((zmq_pollitem_posix_t*)Ptr.Ptr) + index;
            if (native->ReadyEvents == 0)
            {
                message = null;
                return false;
            }
            if (!((ZPollEvent)native->ReadyEvents).HasFlag(ZPollEvent.In))
            {
                message = null;
                return false;
            }
            return Sockets[index].Recv(out message, ZSocket.FlagsDontwait);

        }

        /// <summary>
        /// 检查下标是否有数据
        /// </summary>
        /// <param name="index"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool CheckOut(int index, out ZMessage message)
        {
            if (index > Sockets.Length)
            {
                message = null;
                return false;
            }
            var native = ((zmq_pollitem_posix_t*)Ptr.Ptr) + index;
            if (native->ReadyEvents == 0)
            {
                message = null;
                return false;
            }
            if (!((ZPollEvent)native->ReadyEvents).HasFlag(ZPollEvent.Out))
            {
                message = null;
                return false;
            }
            return Sockets[index].Recv(out message, ZSocket.FlagsDontwait);
        }


        /// <summary>
        /// 一次Pool
        /// </summary>
        /// <returns></returns>
        public Task<bool> PollAsync()
        {
            return Task.FromResult(Poll());
        }


        /// <summary>
        /// 一次Pool
        /// </summary>
        /// <returns></returns>
        public Task<ZMessage> CheckInAsync(int index)
        {
            var res = CheckIn(index, out var message);
            return Task.FromResult(res ? message : null);
        }
    }
}