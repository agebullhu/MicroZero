using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;

using Agebull.Common.Tson;
using Agebull.MicroZero.ZeroManagemant;
using Agebull.MicroZero.ZeroApis;
using ZeroMQ;

namespace Agebull.MicroZero.Log
{
    /// <summary>
    ///   远程记录器
    /// </summary>
    internal sealed class RemoteLogRecorder : ILogRecorder, IZeroObject
    {
        #region Override

        /// <summary>
        /// 是否初始化
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     启动
        /// </summary>
        void ILogRecorder.Initialize()
        {
            //LogRecorderX.TraceToConsole = false;
            IsInitialized = true;
            _state = StationState.Initialized;
            ZeroTrace.SystemLog("RemoteLogRecorder", "ILogRecorder.Initialize", LogRecorderX.Level);
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
        private static readonly byte[] LogDescription =
        {
            3,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.TextContent,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        /// <summary>
        ///   记录日志
        /// </summary>
        /// <param name="infos"> 日志消息 </param>
        void ILogRecorder.RecordLog(List<RecordInfo> infos)
        {
            if (infos.Count == 0)
                return;
            if (_socket == null)
            {
                LogRecorderX.BaseRecorder.RecordLog(infos);
                return;
            }
            var array = infos.ToArray();
            infos.Clear();

            _socket.SendTo(
                LogDescription, 
                _logsByte,
                JsonHelper.SerializeObject(array).ToZeroBytes(),
                ZeroCommandExtend.ServiceKeyBytes);
            //int idx = 0;
            //while (idx <= array.Length)
            //{
            //    byte[] buf;
            //    using (TsonSerializer serializer = new TsonSerializer(TsonDataType.Array))
            //    {
            //        serializer.WriteType(TsonDataType.Object);
            //        int size = array.Length - idx;
            //        if (size > 255)
            //            size = 255;
            //        serializer.WriteLen(size);
            //        for (; size > 0 && idx < array.Length; idx++, --size)
            //        {
            //            serializer.Begin();
            //            RecordInfoTson.ToTson(serializer, array[idx]);
            //            serializer.End();
            //        }

            //        buf = serializer.Close();
            //    }
            //    _socket.SendTo(LogDescription, _logsByte, buf, ZeroCommandExtend.ServiceKeyBytes);
            //}

        }


        /// <summary>
        ///   记录日志
        /// </summary>
        /// <param name="info"> 日志消息 </param>
        void ILogRecorder.RecordLog(RecordInfo info)
        {
            if (info == null)
                return;
            if (_socket != null)
            {
                byte[] buf;
                using (TsonSerializer serializer = new TsonSerializer())
                {
                    RecordInfoTson.ToTson(serializer, info);
                    buf = serializer.Close();
                }
                if (_socket.SendTo(LogDescription, _logsByte, buf, ZeroCommandExtend.ServiceKeyBytes))
                    return;
            }
            LogRecorderX.BaseRecorder.RecordLog(info);
        }

        #endregion

        #region Field

        /// <summary>
        ///     配置状态
        /// </summary>
        public StationStateType ConfigState => StationStateType.Run;

        /// <summary>
        /// 配置
        /// </summary>
        private StationConfig Config;
        public string StationName => "RemoteLog";


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
        public int RealState
        {
            get => _state;
            set => Interlocked.Exchange(ref _state, value);
        }
        private CancellationTokenSource RunTaskCancel;

        public bool Start()
        {
            using (OnceScope.CreateScope(this))
            {
                if (!ZeroApplication.Config.TryGetConfig("RemoteLog", out Config))
                {
                    ZeroTrace.WriteError("RemoteLogRecorder", "No config");
                    RealState = StationState.ConfigError;
                    ZeroApplication.OnObjectFailed(this);
                    return false;
                }
                RealName = ZSocket.CreateRealName(false, Config.StationName);
                Identity = RealName.ToAsciiBytes();
                RunTaskCancel = new CancellationTokenSource();
                //Task.Factory.StartNew(SendTask, RunTaskCancel.Token);
                Task.Factory.StartNew(RunWaite);
                return true;
            }
        }
        /// <summary>
        /// 应用程序等待结果的信号量对象
        /// </summary>
        private readonly SemaphoreSlim _waitToken = new SemaphoreSlim(0, 1);

        public bool Close()
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
            ZeroTrace.SystemLog("RemoteLogRecorder", "Run", $"{RealName} : {Config.RequestAddress}");
            RealState = StationState.Run;
            ZeroApplication.OnObjectActive(this);
        }

        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void OnStop()
        {
            RealState = StationState.Closed;
            ZeroApplication.OnObjectClose(this);
            _waitToken.Release();
        }
        private bool CanRun => RunTaskCancel != null && !RunTaskCancel.Token.IsCancellationRequested &&
                               ZeroApplication.CanDo && RealState == StationState.Run;

        private ZSocket _socket;
        /// <summary>
        /// 轮询
        /// </summary>
        private void RunWaite()
        {
            using (OnceScope.CreateScope(this, OnRun, OnStop))
            {
                var pool = ZmqPool.CreateZmqPool();
                pool.Prepare(ZPollEvent.In, ZSocket.CreateServiceSocket("inproc://RemoteLog.req", ZSocketType.PAIR));
                using (pool)
                {
                    _socket = ZSocket.CreateClientSocketByInproc("inproc://RemoteLog.req", ZSocketType.PAIR);
                    var send = ZSocket.CreateClientSocket(Config.RequestAddress, ZSocketType.DEALER, ZSocket.CreateIdentity(false, StationName));
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
            if (RealState == StationState.Run)
                SystemManager.Instance.Heartbeat("RemoteLogRecorder", RealName);
        }

        void IZeroObject.OnZeroInitialize()
        {
            RealState = StationState.Initialized;
        }

        bool IZeroObject.OnZeroStart()
        {
            return Start();
        }

        /// <summary>
        /// 是否为运行状态
        /// </summary>
        public bool IsRun => RealState > StationState.Start && RealState < StationState.Closing;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name = "RemoteLog";
        /// <summary>
        /// 站点状态变更时调用
        /// </summary>
        void IZeroObject.OnStationStateChanged(StationConfig config)
        {
            if (!IsRun)
            {
                if (config.State < ZeroCenterState.Run && ZeroApplication.CanDo)
                {
                    ZeroTrace.SystemLog(Name, "Start by config state changed");
                    Start();
                }
            }
            else
            {
                if (config.State >= ZeroCenterState.Failed || !ZeroApplication.CanDo)
                {
                    ZeroTrace.SystemLog(Name, "Close by config state changed");
                    Close();
                }
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
