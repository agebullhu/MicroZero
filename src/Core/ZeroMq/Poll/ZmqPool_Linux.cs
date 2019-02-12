using System;
using ZeroMQ.lib;

namespace ZeroMQ
{
    /// <summary>
    /// Linux平台使用
    /// </summary>
    public unsafe class LinuxZPoll : ZPollBase, IZmqPool
    {
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
            Ptr = DispoIntPtr.Alloc(sizeof(zmq_pollitem_posix_t) * sockets.Length);
            zmq_pollitem_posix_t* natives = (zmq_pollitem_posix_t*)Ptr.Ptr;
            for (int i = 0; i < Size; ++i)
            {
                zmq_pollitem_posix_t* native = natives + i;
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
            Ptr = DispoIntPtr.Alloc(sizeof(zmq_pollitem_posix_t) * sockets.Length);
            zmq_pollitem_posix_t* natives = (zmq_pollitem_posix_t*)Ptr.Ptr;
            for (int i = 0; i < Size; ++i)
            {
                zmq_pollitem_posix_t* native = natives + i;
                native->SocketPtr = sockets[i].SocketPtr;
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

            zmq_pollitem_posix_t* natives = (zmq_pollitem_posix_t*)Ptr.Ptr;
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
                Console.WriteLine("index > Sockets.Length");
                return false;
            }
            var native = ((zmq_pollitem_posix_t*)Ptr.Ptr) + index;
            if (native->ReadyEvents == 0)
            {
                message = null;
                Console.WriteLine("native->ReadyEvents == 0");
                return false;
            }
            if (!((ZPollEvent)native->ReadyEvents).HasFlag(ZPollEvent.In))
            {
                Console.WriteLine("!((ZPollEvent)native->ReadyEvents).HasFlag(ZPollEvent.In)");
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
            zmq_pollitem_posix_t* native = ((zmq_pollitem_posix_t*)Ptr.Ptr) + index;
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

    }
}