using System;
using System.IO;
using System.Linq;
using System.Threading;
using Agebull.Common;
using Agebull.Common.Tson;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using ZeroMQ;

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
        protected BatchPublisher() : base(ZeroStationType.Publish, false)
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
        /// <summary>
        /// TSON序列化操作器
        /// </summary>
        protected ITsonOperator<TData> TsonOperator { get; set; }

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
        /// 析构
        /// </summary>
        protected override void DoDispose()
        {
            Items.Save(CacheFileName);
        }

        /// <summary>
        /// 数据进入的处理
        /// </summary>
        protected virtual void OnSend(TData[] data)
        {
        }
        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected sealed override bool RunInner(CancellationToken token)
        {
            _socket = ZSocket.CreateRequestSocket(Config.RequestAddress, Identity);
            SystemManager.Instance.HeartReady(StationName, RealName);
            State = StationState.Run;
            int cnt = 0;

            while (CanLoop)
            {
                Thread.Sleep(10);
                if (token.IsCancellationRequested)
                    break;
                if (++cnt == 36)
                {
                    GC.Collect();
                    cnt = 0;
                }
                var array = Items.Switch();
                if (array == null)
                {
                    Thread.Sleep(10);
                    continue;
                }
                if (TsonOperator == null)
                {
                    do
                    {
                        var datas = array.Count > 300 ? array.Take(255).ToArray() : array.ToArray();
                        while (!_socket.Publish(Name, datas) && CanLoop)
                            Thread.Sleep(10);
                        if (array.Count > 300)
                            array.RemoveRange(0, 255);
                    } while (array.Count > 300);
                    continue;
                }

                int idx = 0;
                while (idx < array.Count)
                {
                    byte[] buf;
                    using (TsonSerializer serializer = new TsonSerializer(TsonDataType.Array))
                    {
                        serializer.WriteType(TsonDataType.Object);
                        int size = array.Count - idx;
                        if (size > 255)
                            size = 255;
                        serializer.WriteLen(size);
                        for (; size > 0 && idx < array.Count; idx++, --size)
                        {
                            if (array[idx] == null)
                            {
                                serializer.WriteType(TsonDataType.Empty);
                                continue;
                            }
                            using (TsonObjectSerializeScope.CreateScope(serializer))
                            {
                                if (array[idx] != null)
                                    TsonOperator.ToTson(serializer, array[idx]);
                            }
                        }
                        buf = serializer.Close();
                    }
                    while (!_socket.Publish(ZeroPublishExtend.PubDescriptionTson2, Name, array.Count.ToString(), buf) && CanLoop)
                        Thread.Sleep(10);
                }
            }
            SystemManager.Instance.HeartLeft(StationName, RealName);
            _socket.TryClose();
            return true;
        }

        #endregion
    }
}
