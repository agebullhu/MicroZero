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
        /// <param name="type"></param>
        protected ZeroStation(int type)
        {
            StationType = type;
        }
        /// <summary>
        /// 调度器
        /// </summary>
        public const int StationTypeDispatcher = 1;
        /// <summary>
        /// 监视器
        /// </summary>
        public const int StationTypeMonitor = 2;
        /// <summary>
        /// API
        /// </summary>
        public const int StationTypeApi = 3;
        /// <summary>
        /// 投票器
        /// </summary>
        public const int StationTypeVote = 4;
        /// <summary>
        /// 广播
        /// </summary>
        public const int StationTypePublish = 5;

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
        public string RealName{ get; protected set; }
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
        /// 正在侦听的状态开关
        /// </summary>
        protected bool InPoll { get; set; }

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        protected abstract bool Run();

        private void SystemMonitor_StationEvent(object sender, SystemMonitor.StationEventArgument e)
        {
            Heartbeat(false);
        }

        /// <summary>
        /// 心跳
        /// </summary>
        protected void Heartbeat(bool left)
        {
            if (left)
                SystemManager.HeartLeft(Config.StationName, RealName);
            else
                SystemManager.Heartbeat(Config.StationName, RealName);
        }

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        protected virtual void OnTaskStop()
        {
            if (ZeroApplication.CanDo && State == StationState.Failed)
            {
                ZeroTrace.WriteInfo(RealName, "ReStart");
                Start();//自动重启
                return;
            }
            State = StationState.Closed;
            ZeroTrace.WriteInfo(RealName, "Closed");
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
        void Start()
        {
            if (State >= StationState.Start)
                return;
            State = StationState.Start;
            Config = ZeroApplication.GetLocalConfig(StationName);
            if (Config == null || Config.State == ZeroCenterState.Uninstall)
            {
                ZeroTrace.WriteError(StationName, "No config");
                State = StationState.ConfigError;
                return;
            }
            RealName = ZeroIdentityHelper.CreateRealName(Config?.ShortName ?? StationName, Name);
            Identity = ZeroIdentityHelper.ToZeroIdentity(Config?.ShortName ?? StationName, Name);
            ZeroTrace.WriteInfo(StationName, Config.WorkerAddress, Name, RealName);
            OnStart();
            Task.Factory.StartNew(Run).ContinueWith(task => OnTaskStop());
            Heartbeat(false);
            SystemMonitor.StationEvent += SystemMonitor_StationEvent;
        }

        void Close()
        {
            SystemMonitor.StationEvent -= SystemMonitor_StationEvent;
            Heartbeat(true);
            if (State < StationState.Start || State > StationState.Pause)
                return;
            ZeroTrace.WriteInfo(StationName, "closing....");
            State = StationState.Closing;
            do
            {
                Thread.Sleep(20);
            } while (State != StationState.Closed);
        }
        #region IZeroObject

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
            if (config.State == ZeroCenterState.Run)
            {
                Start();
            }
            else
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