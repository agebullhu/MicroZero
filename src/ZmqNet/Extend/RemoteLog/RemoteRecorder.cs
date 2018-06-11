using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.ZeroNet.Log
{
    /// <summary>
    ///   远程记录器
    /// </summary>
    internal sealed class RemoteLogRecorder : ILogRecorder, IZeroObject
    {
        #region Override

        /// <inheritdoc />
        /// <summary>
        ///     启动
        /// </summary>
        void ILogRecorder.Initialize()
        {
            LogRecorder.TraceToConsole = false;
            _state = StationState.Initialized;
        }
        /// <inheritdoc />
        /// <summary>
        ///   停止
        /// </summary>
        void ILogRecorder.Shutdown()
        {
            Close();
            _state = StationState.Destroy;
        }


        /// <summary>
        ///   记录日志
        /// </summary>
        /// <param name="info"> 日志消息 </param>
        void ILogRecorder.RecordLog(RecordInfo info)
        {
            info.User = $"{ApiContext.Customer.Account}({ApiContext.RequestContext.Ip}:{ApiContext.RequestContext.Port})";
            info.Machine = ZeroApplication.Config.RealName;
            using (LogRecordingScope.CreateScope())
            {
                if (ZeroApplication.IsAlive)
                    Items.Push(info);
                else
                    LogRecorder.BaseRecorder.RecordLog(info);
            }
        }

        #endregion

        #region Field

        /// <summary>
        /// 配置
        /// </summary>
        private StationConfig Config;

        /// <summary>
        /// 请求队列
        /// </summary>
        public static readonly LogQueue Items = new LogQueue();

        /// <summary>
        /// 节点名称
        /// </summary>
        string IZeroObject.Name => "RemoteLogRecorder";
        /// <summary>
        /// 实例名称
        /// </summary>
        internal string RealName { get; private set; }
        /// <summary>
        /// 实例名称
        /// </summary>
        internal byte[] Identity { get; private set; }

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
        private CancellationTokenSource RunTaskCancel;

        bool Start()
        {
            using (OnceScope.CreateScope(this))
            {
                if (!ZeroApplication.Config.TryGetConfig("RemoteLog", out Config))
                {
                    ZeroTrace.WriteError("RemoteLogRecorder", "No config");
                    State = StationState.ConfigError;
                    return false;
                }
                RealName = ZeroIdentityHelper.CreateRealName(false, Config.ShortName ?? Config.StationName);
                Identity = RealName.ToAsciiBytes();
                RunTaskCancel = new CancellationTokenSource();
                Task.Factory.StartNew(SendTask, RunTaskCancel.Token);
            }
            return true;
        }
        /// <summary>
        /// 应用程序等待结果的信号量对象
        /// </summary>
        private readonly SemaphoreSlim _waitToken = new SemaphoreSlim(0, 1);
        bool Close()
        {
            if (RunTaskCancel != null)
            {
                _waitToken.Release();
                RunTaskCancel.Dispose();
                RunTaskCancel = null;
            }
            State = StationState.Closed;
            return true;
        }
        #endregion

        #region Task

        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void OnRun()
        {
            ZeroTrace.WriteInfo("RemoteLogRecorder", "Run", $"{RealName} : {Config.RequestAddress}");
            State = StationState.Run;
            ZeroApplication.OnObjectActive();
        }

        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void OnStop()
        {
            ZeroTrace.WriteInfo("RemoteLogRecorder", "Closed");
            State = StationState.Closed;
            ZeroApplication.OnObjectClose();
        }
        private bool CanRun => RunTaskCancel != null && !RunTaskCancel.Token.IsCancellationRequested &&
                               ZeroApplication.CanDo && State == StationState.Run;

        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void SendTask(object objToken)
        {
            using (OnceScope.CreateScope(this, OnRun, OnStop))
            {
                var socket = ZSocket.CreateRequestSocket(Config.RequestAddress, Identity);
                while (CanRun)
                {
                    if (!Items.StartProcess(out var title, out var items, 100))
                    {
                        continue;
                    }
                    try
                    {
                        if (!socket.Publish(title.ToString(), ZeroApplication.Config.StationName, JsonConvert.SerializeObject(items)))
                        {
                            LogRecorder.BaseRecorder.RecordLog(new RecordInfo
                            {
                                Type = LogType.Error,
                                Name = "RemoteLogRecorder",
                                Message = "日志发送失败"
                            });
                            socket.TryClose();
                            socket = ZSocket.CreateRequestSocket(Config.RequestAddress, Identity);
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        LogRecorder.BaseRecorder.RecordLog(new RecordInfo
                        {
                            Type = LogType.Error,
                            Name = "RemoteLogRecorder",
                            Message = $"日志发送失败，异常为：\r\n{e}"
                        });
                        socket.TryClose();
                        socket = ZSocket.CreateRequestSocket(Config.RequestAddress, Identity);
                        continue;
                    }
                    Items.EndProcess();
                }
                socket.TryClose();
            }

            _waitToken.Release();
        }


        #endregion

        #region IZeroObject
        /// <summary>
        /// 实例
        /// </summary>
        public static readonly RemoteLogRecorder Instance = new RemoteLogRecorder();


        /// <summary>
        ///     要求心跳
        /// </summary>
        void IZeroObject.OnHeartbeat()
        {
            if (State == StationState.Run)
                SystemManager.Heartbeat("RemoteLogRecorder", RealName);
        }

        void IZeroObject.OnZeroInitialize()
        {
            State = StationState.Initialized;
        }

        bool IZeroObject.OnZeroStart()
        {
            return Start();
        }

        void IZeroObject.OnStationStateChanged(StationConfig config)
        {
            if (config != Config)
                return;
            if (State == StationState.Run)
                Close();
            if (config.State == ZeroCenterState.Run && CanRun)
                Start();
        }

        bool IZeroObject.OnZeroEnd()
        {
            return Close();
        }

        void IZeroObject.OnZeroDestory()
        {
            foreach (var items in Items.DoItems.Values)
            {
                foreach (var item in items)
                    LogRecorder.BaseRecorder.RecordLog(item);
            }
            foreach (var items in Items.Items.Values)
            {
                foreach (var item in items)
                    LogRecorder.BaseRecorder.RecordLog(item);
            }
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
        }
        #endregion

    }
}
