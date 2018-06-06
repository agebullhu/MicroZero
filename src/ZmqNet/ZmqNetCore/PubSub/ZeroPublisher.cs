using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.PubSub
{
    /// <summary>
    ///     消息发布
    /// </summary>
    public class ZeroPublisher : IZeroObject
    {
        #region 单例
        /// <summary>
        /// 防止构造
        /// </summary>
        private ZeroPublisher()
        {

        }

        /// <summary>
        /// 单例
        /// </summary>
        public static readonly ZeroPublisher Instance = new ZeroPublisher();

        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="sub"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void Publish(string station, string title, string sub, string value)
        {
            Instance.DoPublish(station, title, sub, value);
        }
        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="sub"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void Publish<T>(string station, string title, string sub, T value)
        where T : class
        {
            Instance.DoPublish(station, title, sub, value == default(T) ? "{}" : JsonConvert.SerializeObject(value));
        }
        #endregion

        #region IZeroObject

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => "ZeroPublisher";

        /// <summary>
        /// 系统初始化时调用
        /// </summary>
        void IZeroObject.OnZeroInitialize()
        {
            Items.Load(CacheFileName);
        }

        private CancellationTokenSource SendTaskCancel;
        /// <summary>
        /// 系统启动时调用
        /// </summary>
        bool IZeroObject.OnZeroStart()
        {
            using (OnceScope.CreateScope(this))
            {
                State = StationState.Start;
                SendTaskCancel = new CancellationTokenSource();
                Task.Factory.StartNew(SendTask, SendTaskCancel.Token);
            }
            return true;
        }

        /// <summary>
        /// 系统关闭时调用
        /// </summary>
        bool IZeroObject.OnZeroEnd()
        {
            if (SendTaskCancel != null)
            {
                ZeroTrace.WriteInfo(Name, "closing....");
                State = StationState.Closing;
                SendTaskCancel.Cancel();
                using (OnceScope.CreateScope(this))
                {
                    SendTaskCancel.Dispose();
                    SendTaskCancel = null;
                }
            }
            State = StationState.Closed;
            return true;
        }

        void IZeroObject.OnZeroDistory()
        {
            State = StationState.Destroy;
            Items.Save(CacheFileName);
        }

        void IZeroObject.OnStationStateChanged(StationConfig config)
        {
        }
        /// <summary>
        ///     要求心跳
        /// </summary>
        void IZeroObject.OnHeartbeat()
        {
        }

        #endregion

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

        /// <summary>
        /// 缓存文件名称
        /// </summary>
        private string CacheFileName => Path.Combine(ZeroApplication.Config.DataFolder, "zero_publish_queue.json");

        /// <summary>
        /// 请求队列
        /// </summary>
        private readonly SyncQueue<PublishItem> Items = new SyncQueue<PublishItem>();

        /// <summary>
        /// 发送广播
        /// </summary>
        /// <param name="station"></param>
        /// <param name="title"></param>
        /// <param name="sub"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private void DoPublish(string station, string title, string sub, string value)
        {
            Items.Push(new PublishItem
            {
                Station = station,
                Title = title,
                SubTitle = sub,
                RequestId = ApiContext.RequestContext.RequestId,
                Content = value ?? "{}"
            });
            if (State == StationState.Destroy)
                Items.Save(CacheFileName);
        }

        /// <summary>
        /// 广播总数
        /// </summary>
        public ulong PubCount { get; private set; }


        private void OnTaskStart()
        {
            ZeroTrace.WriteInfo(Name, "Run");
            State = StationState.Run;
            ZeroApplication.OnObjectActive();
        }
        private void OnTaskEnd()
        {
            State = StationState.Closed;
            ZeroTrace.WriteInfo(Name, "Closed");
            ZeroApplication.OnObjectClose();
        }

        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void SendTask(object objToken)
        {
            CancellationToken token = (CancellationToken)objToken;

            using (OnceScope.CreateScope(this, OnTaskStart, OnTaskEnd))
            {
                while (!token.IsCancellationRequested && ZeroApplication.CanDo && State == StationState.Run)
                {
                    if (!Items.StartProcess(out var item))
                        continue;
                    if (token.IsCancellationRequested)
                        break;
                    var socket = ZeroConnectionPool.GetSocket(item.Station);
                    if (socket == null)
                    {
                        continue;
                    }
                    try
                    {
                        if (!socket.Publish(item))
                        {
                            ZeroTrace.WriteError(Name, "消息发送失败", JsonConvert.SerializeObject(item));
                            ZeroConnectionPool.Close(ref socket);
                        }
                        else
                        {
                            PubCount++;
                            Items.EndProcess();
                        }
                    }
                    catch (Exception e)
                    {
                        ZeroTrace.WriteError(Name,e, $"{item.Station}-{item.Title}");
                        ZeroConnectionPool.Close(ref socket);
                    }
                    finally
                    {
                        ZeroConnectionPool.Free(socket);
                    }
                }
            }
        }

        #region IDisposable
        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        void IDisposable.Dispose()
        {
            State = StationState.Destroy;
        }
        #endregion

    }
}