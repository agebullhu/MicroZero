using System;
using System.Diagnostics;
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
        protected ZeroStation(ZeroStationType type, bool isService)
        {
            StationType = type;
            IsService = isService;
            Name = GetType().Name ;
        }

        /// <summary>
        /// 是否服务
        /// </summary>
        public bool IsService { get; }

        /// <summary>
        /// 类型
        /// </summary>
        public ZeroStationType StationType { get; }
        /// <summary>
        /// 站点名称
        /// </summary>
        public string StationName { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; protected internal set; }

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
        /// 能不能循环处理
        /// </summary>
        protected bool CanLoop => RunTaskCancel != null && !RunTaskCancel.Token.IsCancellationRequested && ZeroApplication.CanDo && State == StationState.Run;

        /// <summary>
        /// 是否为运行状态
        /// </summary>
        public bool IsRun => State > StationState.Start && State < StationState.Closing;
        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void Initialize()
        {
            Debug.Assert(Name != null);
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
                    ZeroApplication.OnObjectFailed(this);
                    ZeroTrace.WriteError(StationName, "No config");
                    State = StationState.ConfigError;
                    return false;
                }

                if (Config.State == ZeroCenterState.Stop)
                {
                    ZeroApplication.OnObjectFailed(this);
                    ZeroTrace.WriteError(StationName, "Uninstall");
                    State = StationState.ConfigError;
                    return false;
                }

                //名称初始化
                RealName = ZeroIdentityHelper.CreateRealName(IsService, Name == Config.StationName ? null : Name);
                Identity = RealName.ToAsciiBytes();
                ZeroTrace.SystemLog(StationName, Config.WorkerCallAddress, Name, RealName);
                //扩展动作
                if (!OnStart())
                {
                    ZeroApplication.OnObjectFailed(this);
                    State = StationState.Failed;
                    return false;
                }
                //可执行
                SystemManager.Instance.HeartJoin(Config.StationName, RealName);
                //执行主任务
                RunTaskCancel = new CancellationTokenSource();
                Task.Factory.StartNew(Run);
            }
            _waitToken.Wait();
            return true;
        }

        /// <summary>
        /// 将要开始
        /// </summary>
        protected virtual bool OnStart()
        {
            return true;
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
                try
                {
                    success = RunInner(RunTaskCancel.Token);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(StationName, e, "Run");
                    success = false;
                }
            }
            if (ZeroApplication.CanDo && !success)
            {
                //自动重启
                ZeroTrace.SystemLog(StationName, "ReStart");
                Task.Factory.StartNew(Start);
            }
            else
            {
                _waitToken.Release();
            }

            GC.Collect();
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
            ZeroApplication.OnObjectActive(this);
            _waitToken.Release();
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        private void OnStop()
        {
            State = StationState.Closing;
            OnRunStop();
            RunTaskCancel.Dispose();
            RunTaskCancel = null;
            State = StationState.Closed;
            ZeroApplication.OnObjectClose(this);
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
            if (Interlocked.CompareExchange(ref _state, StationState.Closing, StationState.Run) == StationState.Run)
            {
                SystemManager.Instance.HeartLeft(StationName, RealName);
                ZeroTrace.SystemLog(StationName, "Closing....");
                RunTaskCancel?.Cancel();
                _waitToken.Wait();
            }
            else
            {
                State = StationState.Closed;
            }
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
            if (CanLoop)
                SystemManager.Instance.Heartbeat(Config.StationName, RealName);
        }

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
            if (IsRun && (config.State == ZeroCenterState.Run || config.State == ZeroCenterState.Pause))
                return;
            if (config.State == ZeroCenterState.Run && ZeroApplication.CanDo)
            {
                ZeroTrace.SystemLog(Name, "Start by config state changed");
                Start();
            }
            else
            {
                ZeroTrace.SystemLog(Name, "Close by config state changed");
                Close();
            }
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