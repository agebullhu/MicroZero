using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 站点连接池
    /// </summary>
    public interface IZeroConnectionPool : IZeroObject
    {
        /// <summary>
        /// 能否工作
        /// </summary>
        bool CanDo { get;}

        /// <summary>
        /// 取得一个连接对象
        /// </summary>
        /// <returns></returns>
        ZSocket GetSocket(string station, string name);

        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        void FreeSocket(ZSocket socket);

        /// <summary>
        /// 关闭一个连接对象
        /// </summary>
        /// <returns></returns>
        void CloseSocket(ref ZSocket socket);
    }
}