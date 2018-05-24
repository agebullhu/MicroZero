using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using NetMQ.Sockets;

namespace Agebull.ZeroNet.PubSub
{
    /// <summary>
    ///     消息发布器
    /// </summary>
    public abstract class Publisher<TData> where TData : class, IPublishData
    {
        #region 广播

        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="data"></param>
        public void Publish(TData data)
        {
            if (State == StationState.None)
            {
                Task.Factory.StartNew(SendTask);
            }
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
        private RequestSocket _socket;

        /// <summary>
        /// 请求队列
        /// </summary>
        public static readonly SyncQueue<TData> Items = new SyncQueue<TData>();

        /// <summary>
        ///     运行状态
        /// </summary>
        public StationState State { get; protected set; }

        /// <summary>
        /// 广播总数
        /// </summary>
        public long PubCount { get; private set; }
        /// <summary>
        /// 广播总数
        /// </summary>
        public long DataCount { get; private set; }

        #endregion

        #region Task

        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void SendTask()
        {
            Identity = ZeroIdentityHelper.ToZeroIdentity(_config?.ShortName ?? StationName, Name);
            State = StationState.Run;
            while (ZeroApplication.State < StationState.Closing && State == StationState.Run)
            {
                if (ZeroApplication.State != StationState.Run)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (!Items.StartProcess(out TData data, 100))
                {
                    Thread.Sleep(10);
                    continue;
                }
                if (_socket == null && InitSocket())
                {
                    Thread.Sleep(10);
                    continue;
                }

                try
                {
                    if (!_socket.Publish(data))
                    {
                        LogRecorder.Error($"{StationName}广播失败:发送超时");
                        CreateSocket();
                        continue;
                    }
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e, $"{StationName}广播失败");
                    CreateSocket();
                    continue;
                }
                Items.EndProcess();
                PubCount += 1;
                DataCount += 1;
                if (DataCount == long.MaxValue)
                    DataCount = 0;
                if (PubCount == long.MaxValue)
                    PubCount = 0;
            }
            State = StationState.Closed;
            _socket.CloseSocket(_config?.RequestAddress);
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
                return false;
            }
            return CreateSocket() == ZeroCommandStatus.Success;
        }
        /// <summary>
        ///     取得Socket对象
        /// </summary>
        /// <returns></returns>
        protected ZeroCommandStatus CreateSocket()
        {
            try
            {
                _socket.CloseSocket(_config.RequestAddress);

                _socket = new RequestSocket();
                _socket.Options.Identity = Identity;
                _socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 10);
                _socket.Options.ReconnectIntervalMax = new TimeSpan(0, 0, 0, 0, 500);
                _socket.Options.TcpKeepalive = true;
                _socket.Options.TcpKeepaliveIdle = new TimeSpan(0, 1, 0);
                _socket.Connect(_config.RequestAddress);
                return ZeroCommandStatus.Success;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, $"Socket构造失败\r\n异常为：\r\n{e}");
                return ZeroCommandStatus.Exception;
            }
        }

        #endregion
    }
}