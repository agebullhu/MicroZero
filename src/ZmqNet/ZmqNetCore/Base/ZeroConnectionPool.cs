using System;
using System.Collections.Generic;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 站点连接池
    /// </summary>
    public class ZeroConnectionPool : IZeroObject
    {
        #region 单例
        /// <summary>
        /// 阻止构造
        /// </summary>
        ZeroConnectionPool()
        {

        }
        /// <summary>
        /// 单例
        /// </summary>
        public static readonly ZeroConnectionPool Instance = new ZeroConnectionPool();

        /// <summary>
        /// 取得一个连接对象
        /// </summary>
        /// <returns></returns>
        public static ZSocket GetSocket(string station)
        {
            return Instance.GetPoolSocket(station);
        }

        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        public static void Free(ZSocket socket)
        {
            Instance.FreePoolSocket(socket);
        }
        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        public static void Close(ref ZSocket socket)
        {
            Instance.ClosePoolSocket(ref socket);
        }
        #endregion

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
        public void OnZeroStart()
        {
            foreach (var pool in Pools.Values)
            {
                pool.Resume();
            }

            ZeroApplication.Config.Foreach(config =>
            {
                if (!Pools.ContainsKey(config.StationName))
                {
                    Pools.Add(config.StationName, new StationConnectionPool
                    {
                        Config = config
                    });
                }
            });
            CanDo = true;
        }

        /// <summary>
        /// 系统关闭时调用
        /// </summary>
        public void OnZeroEnd()
        {
            Dispose();
        }
        /// <summary>
        /// 系统启动时调用
        /// </summary>
        void IZeroObject.OnStationStateChanged(StationConfig config)
        {
            StationConnectionPool pool;
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
                            Pools.Add(config.StationName, new StationConnectionPool
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
        void IZeroObject.OnZeroDistory() => Dispose();

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

        #region 实现

        private readonly Dictionary<string, StationConnectionPool> Pools = new Dictionary<string, StationConnectionPool>();

        /// <summary>
        /// 能否工作
        /// </summary>
        public bool CanDo { get; private set; }

        /// <summary>
        /// 取得一个连接对象
        /// </summary>
        /// <returns></returns>
        private ZSocket GetPoolSocket(string station)
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
                if (!ZeroApplication.Config.TryGetConfig(station,out var config))
                    return null;
                Pools.Add(station, pool = new StationConnectionPool
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
        private void FreePoolSocket(ZSocket socket)
        {
            if (socket == null)
                return;
            if (socket.StationName != null && Pools.TryGetValue(socket.StationName, out var pool))
            {
                pool.Free(socket);
            }
            else
            {
                socket.CloseSocket();
            }
        }
        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        private void ClosePoolSocket(ref ZSocket socket)
        {
            if (socket == null)
                return;
            if (socket.StationName != null && Pools.TryGetValue(socket.StationName, out var pool))
            {
                pool.Close(ref socket);
            }
            else
            {
                socket.CloseSocket();
                socket = null;
            }
        }

        #endregion

    }
}
