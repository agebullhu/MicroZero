using System.Collections.Generic;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 站点连接池
    /// </summary>
    public class SocketPool : IZeroConnectionPool
    {
        #region IZeroObject

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => nameof(ZeroConnectionPool);

        /// <summary>
        ///     运行状态
        /// </summary>
        int IZeroObject.State => CanDo ? StationState.Run : StationState.None;


        /// <summary>
        /// 系统初始化时调用
        /// </summary>
        void IZeroObject.OnZeroInitialize()
        {
        }

        /// <summary>
        ///     要求心跳
        /// </summary>
        void IZeroObject.OnHeartbeat()
        {
        }


        /// <summary>
        /// 系统启动时调用
        /// </summary>
        public bool OnZeroStart()
        {
            foreach (var pool in Pools.Values)
            {
                pool.Resume();
            }

            lock (Pools)
            {
                ZeroApplication.Config.Foreach(config =>
                {
                    if (!Pools.ContainsKey(config.StationName))
                    {
                        Pools.Add(config.StationName, new StationSocketPool
                        {
                            Config = config
                        });
                    }
                });
            }
            CanDo = true;
            ZeroApplication.OnObjectActive(this);
            return true;
        }

        /// <summary>
        /// 系统关闭时调用
        /// </summary>
        public bool OnZeroEnd()
        {
            Dispose();
            ZeroApplication.OnObjectClose(this);
            return true;
        }
        /// <summary>
        /// 系统启动时调用
        /// </summary>
        void IZeroObject.OnStationStateChanged(StationConfig config)
        {
            StationSocketPool pool;
            lock (Pools)
            {
                if (!Pools.TryGetValue(config.StationName, out pool))
                {
                    return;
                }
            }
            switch (config.State)
            {
                case ZeroCenterState.Run when pool != null:
                    pool.Resume();
                    break;
                case ZeroCenterState.Run:
                    lock (Pools)
                    {
                        if (!Pools.ContainsKey(config.StationName))
                        {
                            Pools.Add(config.StationName, new StationSocketPool
                            {
                                Config = config
                            });
                        }
                    }

                    break;
                default:
                    pool?.Dispose();
                    break;
            }
        }

        /// <summary>
        /// 注销时调用
        /// </summary>
        void IZeroObject.OnZeroDestory() => Dispose();

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if (!CanDo)
                return;
            CanDo = false;
            foreach (var pool in Pools.Values)
            {
                pool.Dispose();
            }
        }

        #endregion

        #region IZeroConnectionPool

        private readonly Dictionary<string, StationSocketPool> Pools = new Dictionary<string, StationSocketPool>();

        /// <summary>
        /// 能否工作
        /// </summary>
        public bool CanDo { get; private set; }

        /// <summary>
        /// 取得一个连接对象
        /// </summary>
        /// <returns></returns>
        ZSocket IZeroConnectionPool.GetSocket(string station, string name)
        {
            if (!CanDo)
            {
                return null;
            }
            if (Pools.TryGetValue(station, out var pool))
            {
                return pool.GetSocket();
            }
            lock (Pools)
            {
                if (!ZeroApplication.Config.TryGetConfig(station, out var config))
                    return null;
                Pools.Add(station, pool = new StationSocketPool
                {
                    Config = config
                });
                return pool.GetSocket();
            }
        }

        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        void IZeroConnectionPool.FreeSocket(ZSocket socket)
        {
            if (socket == null)
                return;
            if (socket.StationName != null && Pools.TryGetValue(socket.StationName, out var pool))
            {
                pool.Free(socket);
            }
            else
            {
                socket.TryClose();
            }
        }
        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        void IZeroConnectionPool.CloseSocket(ref ZSocket socket)
        {
            if (socket == null)
                return;
            if (socket.StationName != null && Pools.TryGetValue(socket.StationName, out var pool))
            {
                pool.Close(ref socket);
            }
            else
            {
                socket.TryClose();
                socket = null;
            }
        }

        #endregion

    }
}
