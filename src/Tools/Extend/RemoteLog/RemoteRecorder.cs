using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.Common.Tson;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
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
            ZeroTrace.WriteInfo("RemoteLogRecorder", "ILogRecorder.Initialize",LogRecorder.Level);
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
        /// TSON标识
        /// </summary>
        private readonly byte[] _logsByte = "Logs".ToUtf8Bytes();

        /// <summary>
        ///     订阅时的标准网络数据说明
        /// </summary>
        static readonly byte[] LogDescription =
        {
            2,
            ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.TsonValue,
            ZeroFrameType.End
        };

        /// <summary>
        ///   记录日志
        /// </summary>
        /// <param name="infos"> 日志消息 </param>
        void ILogRecorder.RecordLog(List<RecordInfo> infos)
        {
            if (_socket != null)
            {
                int idx = 0;
                while (idx <= infos.Count)
                {
                    byte[] buf;
                    using (TsonSerializer serializer = new TsonSerializer(TsonDataType.Array))
                    {
                        serializer.WriteType(TsonDataType.Object);
                        int size = infos.Count - idx;
                        if (size > 255)
                            size = 255;
                        serializer.WriteLen(size);
                        for (; size > 0 && idx < infos.Count; idx++, --size)
                        {
                            serializer.Begin();
                            RecordInfoTson.ToTson(serializer, infos[idx]);
                            serializer.End();
                        }
                        buf = serializer.Close();
                    }
                    if (_socket.SendTo(LogDescription, _logsByte, buf))
                        return;
                }
            }
            LogRecorder.BaseRecorder.RecordLog(infos);
        }


        /// <summary>
        ///   记录日志
        /// </summary>
        /// <param name="info"> 日志消息 </param>
        void ILogRecorder.RecordLog(RecordInfo info)
        {
            if (_socket != null)
            {
                byte[] buf;
                using (TsonSerializer serializer = new TsonSerializer())
                {
                    RecordInfoTson.ToTson(serializer, info);
                    buf = serializer.Close();
                }
                if (_socket.SendTo(LogDescription, _logsByte, buf))
                    return;
            }
            LogRecorder.BaseRecorder.RecordLog(info);
        }

        #endregion

        #region Field

        /// <summary>
        /// 配置
        /// </summary>
        private StationConfig Config;

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
                    ZeroApplication.OnObjectFailed(this);
                    return false;
                }
                RealName = ZeroIdentityHelper.CreateRealName(false, Config.ShortName ?? Config.StationName);
                Identity = RealName.ToAsciiBytes();
                RunTaskCancel = new CancellationTokenSource();
                //Task.Factory.StartNew(SendTask, RunTaskCancel.Token);
                Task.Factory.StartNew(RunWaite);
            }
            return true;
        }
        /// <summary>
        /// 应用程序等待结果的信号量对象
        /// </summary>
        private readonly SemaphoreSlim _waitToken = new SemaphoreSlim(0, 1);

        private bool Close()
        {
            if (Interlocked.CompareExchange(ref _state, StationState.Closing, StationState.Run) != StationState.Run)
                return true;
            RunTaskCancel.Dispose();
            RunTaskCancel = null;
            _waitToken.Wait();
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
            ZeroApplication.OnObjectActive(this);
        }

        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void OnStop()
        {
            State = StationState.Closed;
            ZeroApplication.OnObjectClose(this);
            _waitToken.Release();
        }
        private bool CanRun => RunTaskCancel != null && !RunTaskCancel.Token.IsCancellationRequested &&
                               ZeroApplication.CanDo && State == StationState.Run;

        private ZSocket _socket;
        /// <summary>
        /// 具体执行
        /// </summary>
        private void RunWaite()
        {
            using (OnceScope.CreateScope(this, OnRun, OnStop))
            {
                var pool = ZmqPool.CreateZmqPool();
                pool.Prepare(ZPollEvent.In, ZSocket.CreateServiceSocket("inproc://RemoteLog.req", ZSocketType.PULL));
                using (pool)
                {
                    _socket = ZSocket.CreateClientSocket("inproc://RemoteLog.req", ZSocketType.PUSH);
                    var send = ZSocket.CreateClientSocket(Config.RequestAddress, ZSocketType.DEALER);
                    while (CanRun)
                    {
                        if (!pool.Poll() || !pool.CheckIn(0, out var message))
                        {
                            continue;
                        }

                        using (message)
                        {
                            using (var copy = message.Duplicate())
                            {
                                send.Send(copy);
                            }
                        }
                    }

                    send.Dispose();
                    _socket.Dispose();
                    _socket = null;
                }
            }
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
                SystemManager.Instance.Heartbeat("RemoteLogRecorder", RealName);
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
            if (State == StationState.Run && (config.State == ZeroCenterState.Run || config.State == ZeroCenterState.Pause))
                return;
            if (config.State == ZeroCenterState.Run && ZeroApplication.CanDo)
            {
                ZeroTrace.WriteInfo("RemoteLog", "Start by config state changed");
                Start();
            }
            else
            {
                ZeroTrace.WriteInfo("RemoteLog", "Close by config state changed");
                Close();
            }
        }

        bool IZeroObject.OnZeroEnd()
        {
            return Close();
        }

        void IZeroObject.OnZeroDestory()
        {
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
        }
        #endregion

    }
}
