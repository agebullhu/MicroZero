using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 站点连接池
    /// </summary>
    class StationConnectionPool
    {
        /// <summary>
        /// 对应的配置
        /// </summary>
        public StationConfig Config { get; set; }
        /// <summary>
        /// Socket名称标识
        /// </summary>
        private ulong _socketId;

        /// <summary>
        /// 所有连接
        /// </summary>
        private readonly List<ZSocket> _sockets = new List<ZSocket>();

        /// <summary>
        /// 连接池
        /// </summary>
        [IgnoreDataMember, JsonIgnore] private readonly Queue<ZSocket> _pools = new Queue<ZSocket>();

        private byte[] CreateIdentity()
        {
            return ZeroIdentityHelper.ToZeroIdentity(Config.ShortName ?? Config.StationName, (++_socketId).ToString());
        }
        /// <summary>
        /// 取得一个连接对象
        /// </summary>
        /// <returns></returns>
        internal ZSocket GetSocket()
        {
            if (_isDisposed)
                return null;
            lock (_pools)
            {
                if (_pools.Count != 0)
                {
                    var socket = _pools.Dequeue();
                    socket.IsUsing = true;
                    return socket;
                }
            }
            {
                var socket = ZeroHelper.CreateRequestSocket(Config.RequestAddress, CreateIdentity());
                if (socket == null)
                    return null;
                socket.IsUsing = true;
                socket.StationName = Config.StationName;
                lock (_sockets)
                    _sockets.Add(socket);
                return socket;
            }
        }
        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        internal void Free(ZSocket socket)
        {
            if (socket == null)
                return;
            socket.IsUsing = false;
            if (_isDisposed)
            {
                Close(ref socket);
            }
            //Close(ref socket);
            lock (_pools)
            {
                if (_isDisposed || socket.LastError != null || _pools.Count > 99)
                {
                    Close(ref socket);
                }
                else
                {
                    _pools.Enqueue(socket);
                }
            }
        }
        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        internal void Close(ref ZSocket socket)
        {
            if (socket == null)
                return;
            if (!_isDisposed)
                lock (_sockets)
                    _sockets.Remove(socket);
            socket.CloseSocket();
            socket = null;
        }

        /// <summary>
        /// 是否已析构
        /// </summary>
        [IgnoreDataMember, JsonIgnore]
        private bool _isDisposed;

        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        public void Resume()
        {
            if (!_isDisposed)
                return;
            DoDispose();
            _isDisposed = false;
        }

        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        public void Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            DoDispose();
        }

        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        private void DoDispose()
        {
            lock (_pools)
            {
                while (_pools.Count > 0)
                    _pools.Dequeue().CloseSocket();
            }
            lock (_sockets)
            {
                foreach (var socket in _sockets.ToArray())
                {
                    if (!socket.IsUsing)
                        socket.CloseSocket();
                }
                _sockets.Clear();
            }
        }
    }
}