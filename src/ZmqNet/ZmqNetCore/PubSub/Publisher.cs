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
    public abstract class Publisher<TData> : ZeroStation
        where TData : class, IPublishData
    {
        #region 广播
        /// <summary>
        /// 构造
        /// </summary>
        protected Publisher() : base (StationTypePublish)
        {
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
        /// 连接对象
        /// </summary>
        private ZSocket _socket;

        /// <summary>
        /// 请求队列
        /// </summary>
        private static readonly SyncQueue<TData> Items = new SyncQueue<TData>();

        #endregion

        #region Task

        /// <summary>
        /// 数据进入的处理
        /// </summary>
        protected virtual void OnSend(TData data)
        {

        }
        /// <inheritdoc />
        /// <summary>
        ///     命令轮询
        /// </summary>
        /// <returns></returns>
        protected sealed override bool Run()
        {
            ZeroTrace.WriteInfo("Pub", $"{ StationName}:{Name}", "run...");
            _socket = ZeroHelper.CreateRequestSocket(Config.RequestAddress, Identity);
            State = StationState.Run;
            while (CanRun)
            {
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
            
            ZeroTrace.WriteInfo("Pub", $"{ StationName}:{Name}", "closed");
            return State != StationState.Failed;
        }

        #endregion
    }
}