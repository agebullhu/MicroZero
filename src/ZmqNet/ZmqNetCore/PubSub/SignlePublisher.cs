using System.IO;
using System.Threading;
using Agebull.Common;
using Agebull.ZeroNet.Core;
using ZeroMQ;

namespace Agebull.ZeroNet.PubSub
{
    /// <summary>
    ///     消息发布器
    /// </summary>
    public abstract class SignlePublisher<TData> : ZeroStation
        where TData : class, IPublishData
    {
        #region 广播
        /// <summary>
        /// 构造
        /// </summary>
        protected SignlePublisher() : base(StationTypePublish, false)
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
        /// 缓存文件名称
        /// </summary>
        private string CacheFileName => Path.Combine(ZeroApplication.Config.DataFolder, $"{StationName}.{Name}.sub.json");

        /// <summary>
        /// 初始化
        /// </summary>
        protected sealed override void Initialize()
        {
            Items.Load(CacheFileName);
        }

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
        protected sealed override bool RunInner(CancellationToken token)
        {
            _socket = ZeroHelper.CreateRequestSocket(Config.RequestAddress, Identity);
            State = StationState.Run;
            while (!token.IsCancellationRequested && CanRun)
            {
                if (!Items.StartProcess(out TData data, 100))
                {
                    continue;
                }
                if (token.IsCancellationRequested)
                    break;
                OnSend(data);
                if (!_socket.Publish(data))
                {
                    _socket.CloseSocket();
                    Thread.Sleep(100);
                    _socket = ZeroHelper.CreateRequestSocket(Config.RequestAddress, Identity);
                    continue;
                }
                Items.EndProcess();
            }
            _socket.CloseSocket();
            return State != StationState.Failed;
        }

        #endregion
    }
}