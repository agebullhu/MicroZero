using System.Collections.Generic;
using System.IO;
using System.Threading;
using Agebull.Common;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using ZeroMQ;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Log
{
    /// <summary>
    ///     批量消息发布器
    /// </summary>
    public abstract class BatchPublisher<TData> : ZeroStation
        where TData : class, IPublishData
    {
        #region 广播
        /// <summary>
        /// 构造
        /// </summary>
        protected BatchPublisher() : base(StationTypePublish,false)
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
        private static readonly SyncBatchQueue<TData> Items = new SyncBatchQueue<TData>();

        #endregion

        #region Task

        /// <summary>
        /// 缓存文件名称
        /// </summary>
        private string CacheFileName => Path.Combine(ZeroApplication.Config.DataFolder, $"{StationName}.{Name}.sub.json");

        /// <summary>
        /// 初始化
        /// </summary>
        protected override void Initialize()
        {
            Items.Load(CacheFileName);
        }

        /// <summary>
        /// 数据进入的处理
        /// </summary>
        protected virtual void OnSend(List<TData> data)
        {
        }
        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected sealed override bool RunInner(CancellationToken token)
        {
            _socket = ZSocket.CreateRequestSocket(Config.RequestAddress, Identity);
            SystemManager.HeartReady(StationName, RealName);
            State = StationState.Run;
            while (CanRun)
            {
                if (!Items.StartProcess(out List<TData> data, 100))
                {
                    continue;
                }
                if (token.IsCancellationRequested)
                    break;
                OnSend(data);
                while (data.Count > 0)
                {
                    var batch= data.GetRange(0, data.Count > 36 ? 36 :data.Count);
                    if (!_socket.Publish(new PublishItem
                    {
                        Title = Name,
                        Content = JsonConvert.SerializeObject(batch)
                    }))
                    {
                        _socket.TryClose();
                        Thread.Sleep(100);
                        _socket = ZSocket.CreateRequestSocket(Config.RequestAddress, Identity);
                        continue;
                    }
                    if (data.Count > 36)
                        data.RemoveRange(0, 36);
                    else
                        break;
                }
                Items.EndProcess();
            }
            SystemManager.HeartLeft(StationName, RealName);
            _socket.TryClose();
            return true;
        }

        #endregion
    }
}
