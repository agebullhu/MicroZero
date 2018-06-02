using System;
using System.Threading;
using System.Threading.Tasks;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 站点
    /// </summary>
    public abstract class ZeroStation : IZeroObject
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="type">站点类型</param>
        /// <param name="isService">是否服务</param>
        protected ZeroStation(int type,bool isService)
        {
            StationType = type;
            IsService = isService;
        }

        /// <summary>
        /// 是否服务
        /// </summary>
        public bool IsService {get;}
        /// <summary>
        /// 调度器
        /// </summary>
        public const int StationTypeDispatcher = 1;
        /// <summary>
        /// 广播
        /// </summary>
        public const int StationTypePublish = 2;
        /// <summary>
        /// API
        /// </summary>
        public const int StationTypeApi = 3;
        /// <summary>
        /// 投票器
        /// </summary>
        public const int StationTypeVote = 4;

        /// <summary>
        /// 类型
        /// </summary>
        public int StationType { get; }
        /// <summary>
        /// 站点名称
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// 实例名称
        /// </summary>
        public string RealName { get; protected set; }
        /// <summary>
        /// 实例名称
        /// </summary>
        public byte[] Identity { get; protected set; }

        /// <summary>
        /// 站点配置
        /// </summary>
        public StationConfig Config { get; set; }

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
        /// <summary>
        /// 能不能运行
        /// </summary>
        public bool CanRun => ZeroApplication.CanDo && State == StationState.Run;

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        void Run(object objToken)
        {
            Monitor.Enter(this);
            try
            {
                CancellationToken token = (CancellationToken)objToken;
                ZeroTrace.WriteInfo(StationName, "Runing");
                State = StationState.Run;
                if (RunInner(token))
                    State = StationState.Closed;
                else
                    State = StationState.Failed;
                SystemManager.HeartLeft(Config.StationName, RealName);
                OnRunStop();
                if (ZeroApplication.CanDo && State == StationState.Failed)
                {
                    //自动重启
                    ZeroTrace.WriteInfo(StationName, "ReStart");
                    Start();
                }
                else
                {
                    ZeroTrace.WriteInfo(StationName, "Stop");
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        protected abstract bool RunInner(CancellationToken token);

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        protected virtual void OnRunStop()
        {
        }
        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void OnStart()
        {
        }
        private CancellationTokenSource RunTaskCancel;
        void Start()
        {
            if (State >= StationState.Start)
                return;
            State = StationState.Start;
            Config = ZeroApplication.Config[StationName];
            if (Config == null || Config.State == ZeroCenterState.Uninstall)
            {
                ZeroTrace.WriteError(StationName, "No config");
                State = StationState.ConfigError;
                return;
            }
            RealName = ZeroIdentityHelper.CreateRealName(IsService, Config?.ShortName ?? StationName, Name);
            Identity = RealName.ToAsciiBytes();
            ZeroTrace.WriteInfo(StationName, Config.WorkerCallAddress, Name, RealName);
            OnStart();
            RunTaskCancel = new CancellationTokenSource();
            //取消时执行回调
            RunTaskCancel.Token.Register(RunTaskCancel.Dispose);
            Task.Factory.StartNew(Run, RunTaskCancel.Token).ContinueWith(task => OnRunStop());
            SystemManager.HeartJoin(Config.StationName, RealName);
        }

        void Close()
        {
            if (RunTaskCancel == null)
                return;
            ZeroTrace.WriteInfo(StationName, "closing....");
            State = StationState.Closing;
            RunTaskCancel.Cancel();
            Monitor.Enter(this);
            RunTaskCancel = null;
            Monitor.Exit(this);
        }

        #region IZeroObject
        /// <summary>
        ///     要求心跳
        /// </summary>
        void IZeroObject.OnHeartbeat()
        {
            if (State == StationState.Run)
                SystemManager.Heartbeat(Config.StationName, RealName);
        }

        string IZeroObject.Name => StationName;

        void IZeroObject.OnZeroInitialize()
        {
            State = StationState.Initialized;
            Initialize();
        }

        void IZeroObject.OnZeroStart()
        {
            Start();
        }

        void IZeroObject.OnStationStateChanged(StationConfig config)
        {
            if (config != Config)
                return;
            if (State == StationState.Run)
                Close();
            if (config.State == ZeroCenterState.Run)
                Start();
        }

        void IZeroObject.OnZeroEnd()
        {
            Close();
        }

        void IZeroObject.OnZeroDistory()
        {
        }

        private bool isDisposed;
        void IDisposable.Dispose()
        {
            if (isDisposed)
                return;
            isDisposed = true;
            DoDispose();
        }
        /// <summary>
        /// 析构
        /// </summary>
        protected virtual void DoDispose()
        {

        }
        #endregion
    }
}