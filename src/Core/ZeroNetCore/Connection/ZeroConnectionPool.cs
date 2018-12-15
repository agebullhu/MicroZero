using Agebull.Common.Base;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <inheritdoc />
    /// <summary>
    /// 站点连接池
    /// </summary>
    public class PoolSocket : ScopeBase
    {
        internal ZSocket _socket;
        internal string Station, Name;
        /// <summary>
        /// 连接对象
        /// </summary>
        public ZSocket Socket => _socket;
        /// <summary>
        /// 是否存在失败
        /// </summary>
        public bool HaseFailed { get; set; }
        /// <summary>
        /// 重新构造
        /// </summary>
        public void ReBuild()
        {
            ZeroConnectionPool.Close(ref _socket);
            _socket = ZeroConnectionPool.GetSocket2(Station, Name);
        }

        internal PoolSocket(ZSocket socket)
        {
            _socket = socket;
        }
        /// <summary>清理资源</summary>
        protected override void OnDispose()
        {
            if (_socket == null)
                return;
            if (HaseFailed)
                ZeroConnectionPool.Close(ref _socket);
            else
                ZeroConnectionPool.Free(_socket);
            _socket = null;
        }
        /// <summary>
        /// 析构
        /// </summary>
        ~PoolSocket()
        {
            _socket?.TryClose();
        }
    }

    /// <summary>
    /// 站点连接池
    /// </summary>
    public static class ZeroConnectionPool
    {
        /// <summary>
        /// 生成实例对象
        /// </summary>
        internal static IZeroConnectionPool CreatePool()
        {
            //if (ZeroApplication.Config.SpeedLimitModel == SpeedLimitType.ThreadCount)
            //    return Pool = new ProxyPool();
            return Pool = new SocketPool();
        }

        /// <summary>
        /// 单例
        /// </summary>
        internal static IZeroConnectionPool Pool;

        /// <summary>
        /// 取得一个连接对象
        /// </summary>
        /// <returns>系统正常时返回一个连接对象</returns>
        public static PoolSocket GetSocket(string station, string name)
        {
            if (Pool == null && ZeroApplication.WorkModel == ZeroWorkModel.Client && !ZeroApplication.Run())
            {
                return null;
            }
            // ReSharper disable once PossibleNullReferenceException
            return new PoolSocket(Pool.GetSocket(station, name))
            {
                Station = station,
                Name = name
            };
        }

        /// <summary>
        /// 取得一个连接对象
        /// </summary>
        /// <returns></returns>
        internal static ZSocket GetSocket2(string station, string name)
        {
            return Pool.GetSocket(station, name);
        }

        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        public static void Free(ZSocket socket)
        {
            Pool.FreeSocket(socket);
        }

        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        public static void Free(PoolSocket socket)
        {
            if (socket.HaseFailed)
                Pool.CloseSocket(ref socket._socket);
            else
                Pool.FreeSocket(socket.Socket);
        }

        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        public static void Close(ref ZSocket socket)
        {
            Pool.CloseSocket(ref socket);
        }
    }
}

