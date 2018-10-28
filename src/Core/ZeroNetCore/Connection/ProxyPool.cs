using System;
using System.Threading;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 站点连接池
    /// </summary>
    public class ProxyPool : IZeroConnectionPool
    {
        #region IZeroObject

        /// <summary>
        /// 名称
        /// </summary>
        string IZeroObject.StationName => "___";

        //readonly Dictionary<string, ConnectionProxy> proxys = new Dictionary<string, ConnectionProxy>();

        readonly PollProxy proxy = new PollProxy();
        /// <summary>
        /// 名称
        /// </summary>
        public string Name => nameof(ZeroConnectionPool);

        /// <summary>
        ///     运行状态
        /// </summary>
        private int _state;

        /// <summary>
        ///     运行状态
        /// </summary>
        public int State
        {
            get => _state;
            set => Interlocked.Exchange(ref _state, value);
        }

        /// <summary>
        /// 系统初始化时调用
        /// </summary>
        void IZeroObject.OnZeroInitialize()
        {
            State = StationState.Initialized;
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
            if (State != StationState.Run)
            {
                State = StationState.Run;
                proxy.Start();
                //ZeroApplication.Config.Foreach(config =>
                //{
                //    if (config.StationType == ZeroStation.StationTypeDispatcher)
                //        return;
                //    proxys.Add(config.StationName, new ConnectionProxy()
                //    {
                //        Config = config
                //    });
                //});
                //Parallel.ForEach(proxys.Values, proxy => proxy.Start());
            }
            ZeroApplication.OnObjectActive(this);
            return true;
        }

        /// <summary>
        /// 系统关闭时调用
        /// </summary>
        public bool OnZeroEnd()
        {
            ZeroApplication.OnObjectClose(this);
            return true;
        }
        /// <summary>
        /// 系统启动时调用
        /// </summary>
        void IZeroObject.OnStationStateChanged(StationConfig config)
        {
            //if (config.StationType == ZeroStation.StationTypeDispatcher || proxys.ContainsKey(config.StationName))
            //    return;
            //var proxy = new ConnectionProxy()
            //{
            //    Config = config
            //};
            //proxys.Add(config.StationName, proxy);
            //proxy.Start();
        }

        /// <summary>
        /// 注销时调用
        /// </summary>
        void IZeroObject.OnZeroDestory()
        {
            proxy.End();
            //Parallel.ForEach(proxys.Values, proxy=> proxy.End());
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            State = StationState.Destroy;
        }

        #endregion

        #region IZeroConnectionPool

        /// <summary>
        /// 能否工作
        /// </summary>
        bool IZeroConnectionPool.CanDo => true;

        /// <summary>
        /// 取得一个连接对象
        /// </summary>
        /// <returns></returns>
        ZSocket IZeroConnectionPool.GetSocket(string station, string name)
        {
            return ZSocket.CreateClientSocket($"inproc://{station}_Proxy", ZSocketType.PAIR, name.ToAsciiBytes());
        }

        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        void IZeroConnectionPool.FreeSocket(ZSocket socket)
        {
            socket?.TryClose();
        }
        /// <summary>
        /// 释放一个连接对象
        /// </summary>
        /// <returns></returns>
        void IZeroConnectionPool.CloseSocket(ref ZSocket socket)
        {
            if (socket == null)
                return;
            socket.TryClose();
            socket = null;
        }

        #endregion
    }
}