using System;
using System.Runtime.InteropServices;
using ZeroMQ.lib;

namespace ZeroMQ
{/// <summary>
    /// 更原始封装的Pool对象
    /// </summary>
    public interface IZmqPool : IDisposable
    {
        /// <summary>
        /// Sockt数量
        /// </summary>
        int Size { get; set; }
        /// <summary>
        /// Sockt数量
        /// </summary>
        ZSocket[] Sockets { get; }
        /// <summary>
        /// 超时
        /// </summary>
        int TimeoutMs { get; set; }
        /// <summary>
        /// 错误对象
        /// </summary>
        ZError ZError { get; }
        /// <summary>
        /// 非托管句柄
        /// </summary>
        DispoIntPtr Ptr { get; set; }
        /// <summary>
        /// 准备
        /// </summary>
        /// <param name="sockets"></param>
        /// <param name="events"></param>
        void Prepare(ZSocket[] sockets, ZPollEvent events);
        /// <summary>
        /// 一次Pool
        /// </summary>
        /// <returns></returns>
        bool Poll();

        /// <summary>
        /// 检查下标是否有数据
        /// </summary>
        /// <param name="index"></param>
        /// <param name="message"></param>
        bool CheckIn(int index, out ZMessage message);
        /// <summary>
        /// 检查下标是否有数据
        /// </summary>
        /// <param name="index"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool CheckOut(int index, out ZMessage message);
    }
    /// <summary>
    /// pool对象
    /// </summary>
    public static class ZmqPool
    {
        /// <summary>
        /// 生成一个Pool对象
        /// </summary>
        /// <returns></returns>
        public static IZmqPool CreateZmqPool()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WinZPoll();
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LinuxZPoll();
            }
            throw new NotSupportedException("only support Windows or Linux");
        }
    }
}