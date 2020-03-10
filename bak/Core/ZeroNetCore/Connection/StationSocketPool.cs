using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Agebull.Common.Configuration;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.MicroZero
{
    /// <summary>
    /// 站点连接池
    /// </summary>
    class StationSocketPool
    {
        /// <summary>
        /// 对应的配置
        /// </summary>
        public StationConfig Config { get; set; }

        /// <summary>
        /// 所有连接
        /// </summary>
        private readonly List<ZSocket> _sockets = new List<ZSocket>();

        /// <summary>
        /// 连接池
        /// </summary>
        [IgnoreDataMember, JsonIgnore] private readonly ConcurrentQueue<ZSocket> _pools = new ConcurrentQueue<ZSocket>();

        /// <summary>
        /// 取得一个连接对象
        /// </summary>
        /// <returns></returns>
        internal ZSocket GetSocket()
        {
            if (_isDisposed)
                return null;
            ZSocket socket;

            while (true)
            {
                lock (_pools)
                {
                    if (!_pools.TryDequeue(out socket))
                        break;
                }

                if (socket == null || socket.IsDisposed)
                    continue;
                socket.IsUsing = true;
                return socket;
            }

            socket = ZSocket.CreateDealerSocket(Config.RequestAddress, ZSocket.CreateIdentity(false,Config.StationName));
            if (socket == null)
                return null;
            socket.IsUsing = true;
            socket.StationName = Config.StationName;
            lock (_sockets)
            {
                _sockets.Add(socket);
            }
            return socket;
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
           //Close(ref socket);
            if (ZeroApplication.WorkModel == ZeroWorkModel.Client ||
                _isDisposed || socket.IsDisposed || socket.LastError != null)
            {
                Close(ref socket);
            }
            //Close(ref socket);
            lock (_pools)
            {
                if (_pools.Count > ZeroApplication.Config.PoolSize)
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
            {
                lock (_sockets)
                    _sockets.Remove(socket);
            }
            socket.TryClose();
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
            while (true)
            {
                ZSocket socket;
                lock (_pools)
                {
                    if (!_pools.TryDequeue(out socket))
                        break;
                }
                socket?.TryClose();
            }
            ZSocket[] array;
            lock (_sockets)
            {
                array = _sockets.ToArray();
                _sockets.Clear();
            }
            foreach (var socket in array)
            {
                if (socket == null)
                    continue;
                if (!socket.IsUsing)
                    socket.TryClose();
            }
        }
    }
}