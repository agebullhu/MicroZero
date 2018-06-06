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
        protected ZeroStation(int type, bool isService)
        {
            StationType = type;
            IsService = isService;
        }

        /// <summary>
        /// 是否服务
        /// </summary>
        public bool IsService { get; }
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
        /// 取消标记
        /// </summary>
        private CancellationTokenSource RunTaskCancel;
        /// <summary>
        /// 能不能运行
        /// </summary>
        public bool CanRun => ZeroApplication.CanDo && State == StationState.Run;

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual bool OnNofindConfig()
        {
            string type;
            switch (this.StationType)
            {
                case StationTypePublish:
                    type = "pub";
                    break;
                case StationTypeApi:
                    type = "api";
                    break;
                default:
                    type = null;
                    break;
            }
            ZeroTrace.WriteError(StationName, "No find,try install ...");
            var result = SystemManager.CallCommand("install", type, StationName, StationName);
            if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
                return false;
            ZeroTrace.WriteError(StationName, "Is install ,try start it ...");
            result = SystemManager.CallCommand("start", StationName);
            if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
                return false;
            Config = SystemManager.LoadConfig(StationName, out _);
            if (Config == null)
                return false;
            Config.State = ZeroCenterState.Run;
            ZeroTrace.WriteError(StationName, "successfully");
            return true;
        }

        /// <summary>
        /// 开始
        /// </summary>
        /// <returns></returns>
        bool Start()
        {
            using (OnceScope.CreateScope(this))
            {
                State = StationState.Start;
                //取配置
                Config = ZeroApplication.Config[StationName];
                if (Config == null && !OnNofindConfig())
                {
                    ZeroApplication.OnObjectFailed();
                    ZeroTrace.WriteError(StationName, "No config");
                    State = StationState.ConfigError;
                    return false;
                }
                if (Config.State == ZeroCenterState.Uninstall)
                {
                    ZeroApplication.OnObjectFailed();
                    ZeroTrace.WriteError(StationName, "Uninstall");
                    State = StationState.ConfigError;
                    return false;
                }
                ZeroTrace.WriteInfo(StationName, Config.WorkerCallAddress, Name, RealName);
                //名称初始化
                RealName = ZeroIdentityHelper.CreateRealName(IsService, Config?.ShortName ?? StationName, Name);
                Identity = RealName.ToAsciiBytes();
                SystemManager.HeartJoin(Config.StationName, RealName);
                //扩展动作
                OnStart();
                //执行主任务
                RunTaskCancel = new CancellationTokenSource();
                Task.Factory.StartNew(Run, RunTaskCancel.Token);
            }
            return true;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void OnStart()
        {
        }
        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        void Run(object objToken)
        {
            CancellationToken token = (CancellationToken)objToken;
            using (OnceScope.CreateScope(this, OnRun, OnStop))
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (RunInner(token))
                    State = StationState.Closed;
                else
                    State = StationState.Failed;
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
        void OnRun()
        {
            State = StationState.Run;
            ZeroTrace.WriteInfo(StationName, "Run");
            ZeroApplication.OnObjectActive();
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        void OnStop()
        {
            OnRunStop();
            ZeroTrace.WriteInfo(StationName, "Closed");
            ZeroApplication.OnObjectClose();
        }

        /// <summary>
        /// 关闭时的处理
        /// </summary>
        /// <returns></returns>
        protected virtual void OnRunStop()
        {
        }
        bool Close()
        {
            SystemManager.HeartLeft(StationName, RealName);
            if (RunTaskCancel != null)
            {
                ZeroTrace.WriteInfo(StationName, "closing....");
                State = StationState.Closing;
                RunTaskCancel.Cancel();
                using (OnceScope.CreateScope(this))
                {
                    RunTaskCancel.Dispose();
                    RunTaskCancel = null;
                }
            }
            State = StationState.Closed;
            return true;
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
            if (config.State == ZeroCenterState.Run)
                Start();
        }

        bool IZeroObject.OnZeroEnd()
        {
            return Close();
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