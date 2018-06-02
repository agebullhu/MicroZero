using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Log
{
    /// <summary>
    ///   远程记录器
    /// </summary>
    public sealed class RemoteLogRecorder : ILogRecorder, IZeroObject
    {
        #region Override
        /// <summary>
        /// 注册
        /// </summary>
        public static void Regist()
        {
            LogRecorder.Regist<RemoteLogRecorder>();
            ZeroApplication.RegistZeroObject((RemoteLogRecorder)LogRecorder.Recorder);
        }

        /// <inheritdoc />
        /// <summary>
        ///     启动
        /// </summary>
        void ILogRecorder.Initialize()
        {
            LogRecorder.LogByTask = false;
            LogRecorder.TraceToConsole = false;
            _state = StationState.Initialized;
        }
        /// <inheritdoc />
        /// <summary>
        ///   停止
        /// </summary>
        void ILogRecorder.Shutdown()
        {
            _state = StationState.Closing;
            Monitor.Enter(this);
            Monitor.Exit(this);
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
                if (ZeroApplication.ApplicationState < StationState.Destroy)
                {
                    Items.Push(info);
                }
                else
                {
                    LogRecorder.BaseRecorder.RecordLog(info);
                }
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

        void Start()
        {
            if (!ZeroApplication.Config.TryGetConfig("RemoteLog", out Config))
            {
                ZeroTrace.WriteError("RemoteLogRecorder", "No config");
                State = StationState.ConfigError;
                return;
            }
            RunTaskCancel = new CancellationTokenSource();
            //取消时执行回调
            RunTaskCancel.Token.Register(RunTaskCancel.Dispose);
            RealName = ZeroIdentityHelper.CreateRealName(false,Config.ShortName ?? Config.StationName);
            Identity = RealName.ToAsciiBytes();
            Task.Factory.StartNew(SendTask, RunTaskCancel.Token);
        }
        void Close()
        {
            if (State != StationState.Run)
            {
                State = StationState.Closed;
            }
            else
            {
                State = StationState.Closing;
                Monitor.Enter(this);
                Monitor.Exit(this);
            }
        }
        #endregion

        #region Task

        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void SendTask(object objToken)
        {
            CancellationToken token = (CancellationToken)objToken;
            ZeroTrace.WriteInfo("RemoteLogRecorder", Config.RequestAddress, RealName);
            ZeroTrace.WriteInfo("RemoteLogRecorder", "Runing");
            Monitor.Enter(this);
            try
            {
                State = StationState.Run;
                var socket = ZeroHelper.CreateRequestSocket(Config.RequestAddress, Identity);
                while (!token.IsCancellationRequested && ZeroApplication.CanDo && State == StationState.Run)
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
                            socket.CloseSocket();
                            socket = ZeroHelper.CreateRequestSocket(Config.RequestAddress, Identity);
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
                        socket.CloseSocket();
                        socket = ZeroHelper.CreateRequestSocket(Config.RequestAddress, Identity);
                        continue;
                    }
                    Items.EndProcess();
                }
                socket.CloseSocket();
                State = StationState.Closed;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }


        #endregion

        #region IZeroObject

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

        void IZeroObject.OnZeroStart()
        {
            Start();
        }

        void IZeroObject.OnStationStateChanged(StationConfig config)
        {
            if (config != Config)
                return;
            if (config.State == ZeroCenterState.Run && State != StationState.Run)
            {
                Start();
            }
            else if (State == StationState.Run)
            {
                Close();
            }
        }

        void IZeroObject.OnZeroEnd()
        {
            Close();
        }

        void IZeroObject.OnZeroDistory()
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
