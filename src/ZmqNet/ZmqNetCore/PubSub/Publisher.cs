using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using ZeroMQ;

namespace Agebull.ZeroNet.PubSub
{
    /// <summary>
    ///     消息发布器
    /// </summary>
    public abstract class Publisher<TData> where TData : class, IPublishData
    {
        #region 广播
        /// <summary>
        /// 构造
        /// </summary>
        protected Publisher()
        {
            Task.Factory.StartNew(SendTask);
        }
        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="data"></param>
        public void Publish(TData data)
        {
            Items.Push(data);
        }

        #endregion

        #region Field
        /// <summary>
        /// 节点名称
        /// </summary>
        public string StationName { get; set; }
        /// <summary>
        /// 节点名称
        /// </summary>
        protected string Name { get; set; }
        /// <summary>
        /// 配置
        /// </summary>
        private StationConfig _config;
        /// <summary>
        /// 连接对象
        /// </summary>
        private ZSocket _socket;

        /// <summary>
        /// 请求队列
        /// </summary>
        private static readonly SyncQueue<TData> Items = new SyncQueue<TData>();

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
            protected set => Interlocked.Exchange(ref _state, value);
        }

        #endregion

        #region Task

        /// <summary>
        /// 数据进入的处理
        /// </summary>
        protected virtual void OnSend(TData data)
        {

        }
        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void SendTask()
        {
            if (State != StationState.None)
                return;
            State = StationState.Start;
            while (ZeroApplication.ApplicationState < StationState.Closing && ZeroApplication.ApplicationState != StationState.Run)
            {
                Thread.Sleep(200);
            }

            if (ZeroApplication.ApplicationState >= StationState.Closing)
            {
                State = StationState.Closed;
                return;
            }

            StationConsole.WriteInfo("Pub", $"{ StationName}:{Name}", "Start");
            while (!InitSocket())
            {
                Thread.Sleep(500);
            }
            StationConsole.WriteInfo("Pub", $"{ StationName}:{Name}", "run...");
            State = StationState.Run;
            while (ZeroApplication.IsAlive)
            {
                if (!ZeroApplication.CanDo)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (State == StationState.Failed)
                {
                    Thread.Sleep(500);
                    if (!CreateSocket())
                    {
                        continue;
                    }
                    State = StationState.Run;
                }
                if (!Items.StartProcess(out TData data, 100))
                {
                    Thread.Sleep(10);
                    continue;
                }

                OnSend(data);
                if (!_socket.Publish(data))
                {
                    State = StationState.Failed;
                    continue;
                }
                Items.EndProcess();
            }
            _socket.CloseSocket();
            State = StationState.Closed;
            StationConsole.WriteInfo("Pub", $"{ StationName}:{Name}", "closed");
        }

        #endregion

        #region Socket

        /// <summary>
        /// 标识
        /// </summary>
        protected byte[] Identity { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        protected bool InitSocket()
        {
            _config = ZeroApplication.GetConfig(StationName);
            if (_config == null)
            {
                StationConsole.WriteError("Pub", $"{ StationName}:{Name}", "can't find config");
                return false;
            }
            StationConsole.WriteInfo("Pub", $"{ StationName}:{Name}", _config.WorkerAddress);
            StationConsole.WriteInfo("Pub", $"{ StationName}:{Name}", ZeroIdentityHelper.CreateRealName(_config.ShortName ?? StationName, Name));
            Identity = ZeroIdentityHelper.ToZeroIdentity(_config.ShortName ?? StationName, Name);
            return CreateSocket();
        }

        /// <summary>
        ///     取得Socket对象
        /// </summary>
        /// <returns></returns>
        protected bool CreateSocket()
        {
            _socket.CloseSocket();
            _socket = ZeroHelper.CreateRequestSocket(_config.RequestAddress, Identity);
            return _socket != null;
        }

        #endregion
    }
}