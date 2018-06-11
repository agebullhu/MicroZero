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
        public bool CanRun => RunTaskCancel != null && !RunTaskCancel.Token.IsCancellationRequested &&
                              ZeroApplication.CanDo && State == StationState.Run;
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
            return false;
        }

        /// <summary>
        /// 开始
        /// </summary>
        /// <returns></returns>
        private bool Start()
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
            _waitToken.Wait();
            return true;
        }

        /// <summary>
        /// 将要开始
        /// </summary>
        protected virtual void OnStart()
        {
        }

        /// <summary>
        /// 应用程序等待结果的信号量对象
        /// </summary>
        private readonly SemaphoreSlim _waitToken = new SemaphoreSlim(0, 1);

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        private void Run()
        {
            bool success;
            using (OnceScope.CreateScope(this, OnRun, OnStop))
            {
                success = RunInner(RunTaskCancel.Token);
            }
            if (ZeroApplication.CanDo && !success)
            {
                //自动重启
                ZeroTrace.WriteInfo(StationName, "ReStart");
                Task.Factory.StartNew(Start);
            }
            else
            {
                ZeroTrace.WriteInfo(StationName, "Closed");
                _waitToken.Release();
            }
        }

        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected abstract bool RunInner(CancellationToken token);

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        private void OnRun()
        {
            State = StationState.Run;
            ZeroTrace.WriteInfo(StationName, "Run");
            ZeroApplication.OnObjectActive();
            _waitToken.Release();
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        private void OnStop()
        {
            OnRunStop();
            RunTaskCancel.Dispose();
            RunTaskCancel = null;
            ZeroApplication.OnObjectClose();
        }

        /// <summary>
        /// 关闭时的处理
        /// </summary>
        /// <returns></returns>
        protected virtual void OnRunStop()
        {
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        private bool Close()
        {
            if (RunTaskCancel != null)
            {
                SystemManager.HeartLeft(StationName, RealName);
                ZeroTrace.WriteInfo(StationName, "closing....");
                RunTaskCancel.Cancel();
                _waitToken.Wait();
            }
            State = StationState.Closed;
            return true;
        }

        /// <summary>
        /// 析构
        /// </summary>
        protected virtual void DoDispose()
        {

        }
        /// <summary>
        /// 析构
        /// </summary>
        protected virtual void DoDestory()
        {

        }
        #region IZeroObject
        /// <summary>
        ///     要求心跳
        /// </summary>
        void IZeroObject.OnHeartbeat()
        {
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
            if (config.State == ZeroCenterState.Run && CanRun)
                Start();
        }

        bool IZeroObject.OnZeroEnd()
        {
            return Close();
        }

        void IZeroObject.OnZeroDestory()
        {
            DoDestory();
        }

        private bool _isDisposed;
        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;
            _isDisposed = true;
            DoDispose();
        }
        #endregion
    }
}