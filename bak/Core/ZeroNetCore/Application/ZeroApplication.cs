using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Configuration;
using Agebull.Common.Context;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;

using Agebull.MicroZero.PubSub;
using Agebull.MicroZero.ZeroApis;
using Agebull.EntityModel.Common;
using Agebull.MicroZero.ZeroManagemant;
using ZeroMQ;
using ZeroMQ.lib;

namespace Agebull.MicroZero
{
    /// <summary>
    ///     站点应用
    /// </summary>
    public partial class ZeroApplication
    {
        #region Console

        /// <summary>
        /// 测试程序，用于系统检查注入
        /// </summary>
        public static Func<string> TestFunc { get; set; } = () => ApiResult.SucceesJson;

        /// <summary>
        ///     命令行方式管理
        /// </summary>
        public static void CommandConsole()
        {
            while (true)
            {
                var cmd = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(cmd))
                    continue;
                switch (cmd.Trim().ToLower())
                {
                    case "quit":
                    case "exit":
                        Shutdown();
                        return;
                    case "start":
                        Start();
                        continue;
                }

                var words = cmd.Split(' ', '\t', '\r', '\n');
                if (words.Length == 0)
                {
                    Console.WriteLine("请输入正确命令");
                    continue;
                }

                var result = ZeroCenterProxy.Master.CallCommand(words);
                if (result.InteractiveSuccess)
                    ZeroTrace.SystemLog("Console", result.TryGetString(ZeroFrameType.Status, out var value)
                        ? value
                        : result.State.Text());
                else
                    ZeroTrace.WriteError("Console", result.TryGetString(ZeroFrameType.Status, out var value)
                        ? value
                        : result.State.Text());
            }
        }

        #endregion

        #region State
        //#if UseStateMachine
        /// <summary>
        /// 中心事件监控对象
        /// </summary>
        static readonly ZeroEventMonitor SystemMonitor = new ZeroEventMonitor();
        //#endif 
        /// <summary>
        ///     应用中心状态
        /// </summary>
        public static ZeroCenterState ZeroCenterState { get; internal set; }

        /// <summary>
        ///     运行状态
        /// </summary>
        private static int _appState;

        /// <summary>
        ///     状态
        /// </summary>
        public static int ApplicationState
        {
            get => _appState;
            internal set
            {
                Interlocked.Exchange(ref _appState, value);
                SystemMonitor.OnApplicationStateChanged();
                ZeroTrace.SystemLog("ZeroApplication", StationState.Text(value), SystemMonitor.StateMachine.GetTypeName());
            }
        }

        /// <summary>
        /// 设置ZeroCenter与Application状态都为Failed
        /// </summary>
        public static void SetFailed()
        {
            ApplicationState = StationState.Failed;
            ZeroCenterState = ZeroCenterState.Failed;
        }

        /// <summary>
        ///     应用中心是否正在运行
        /// </summary>
        public static bool ZerCenterIsRun => WorkModel != ZeroWorkModel.Service || ZeroCenterState == ZeroCenterState.Run;

        /// <summary>
        ///     本地应用是否正在运行
        /// </summary>
        public static bool ApplicationIsRun => ApplicationState == StationState.BeginRun || ApplicationState == StationState.Run;

        /// <summary>
        ///     运行状态（本地与服务器均正常）
        /// </summary>
        public static bool CanDo => ApplicationIsRun && ZerCenterIsRun;

        /// <summary>
        ///     运行状态（本地未关闭）
        /// </summary>
        public static bool IsAlive => ApplicationState < StationState.Destroy;

        /// <summary>
        ///     已关闭
        /// </summary>
        public static bool IsDisposed => ApplicationState == StationState.Disposed;

        /// <summary>
        ///     已关闭
        /// </summary>
        public static bool IsClosed => ApplicationState >= StationState.Closed;

        #endregion

        #region Flow


        #region Option

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        public static void CheckOption()
        {
            ZeroTrace.SystemLog("Weconme MicroZero");
            ZContext.Initialize();
            ZeroTrace.SystemLog("ZMQ", zmq.LibraryVersion);
            //ZeroTrace.Initialize();
            var testContext = IocHelper.Create<GlobalContext>();
            if (testContext == null)
                IocHelper.AddScoped<GlobalContext, GlobalContext>();

            CheckConfig();
            
            if (Config.MaxIOThreads <= 0)
                Config.MaxIOThreads = 4096;
            if (Config.MaxWorkThreads <= 0)
                Config.MaxWorkThreads = 32767;
            ThreadPool.GetMaxThreads(out var worker, out var io);
            ZeroTrace.SystemLog($"   Worker threads: {worker:N0}|{Config.MaxWorkThreads:N0} .Asynchronous I/O threads: { io:N0}|{Config.MaxIOThreads:N0}");
            ThreadPool.GetAvailableThreads(out Config.MaxWorkThreads, out Config.MaxIOThreads);

            InitializeDependency();
            ShowOptionInfo();
        }


        /// <summary>
        ///     设置LogRecorderX的依赖属性(内部使用)
        /// </summary>
        private static void InitializeDependency()
        {
            GlobalContext.ServiceName = Config.ServiceName;
            GlobalContext.ServiceRealName = $"{Config.ServiceName}:{Config.StationName}:{RandomOperate.Generate(4)}";

            //日志
            ConfigurationManager.Get("LogRecorder")["txtPath"] = Config.LogFolder;
            LogRecorderX.LogPath = Config.LogFolder;
            LogRecorderX.GetMachineNameFunc = () => GlobalContext.ServiceRealName;
            LogRecorderX.GetUserNameFunc = () => GlobalContext.CurrentNoLazy?.User?.Account ?? "*";
            LogRecorderX.GetRequestIdFunc = () => GlobalContext.CurrentNoLazy?.Request?.RequestId ?? RandomOperate.Generate(10);
            LogRecorderX.Initialize();

            //插件
            AddInImporter.Importe();
            AddInImporter.Instance.Initialize();

            //注册默认广播
            IocHelper.AddSingleton<IZeroPublisher, ZPublisher>();
        }


        /// <summary>
        ///     发现
        /// </summary>
        public static void Discove(Assembly assembly, string stationName = null)
        {
            var discover = new ZeroDiscover
            {
                StationName = stationName ?? Config.StationName,
                Assembly = assembly
            };
            ZeroTrace.SystemLog("Discove", discover.Assembly.FullName);
            discover.FindApies();
            discover.FindZeroObjects();
        }

        #endregion

        #region Initialize

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize()
        {
            if (WorkModel == ZeroWorkModel.Service)
            {
                RegistZeroObject<ApiProxy>();
                //RegistZeroObject(ZeroConnectionPool.CreatePool());
            }
            AddInImporter.Instance.AutoRegist();
            ApplicationState = StationState.Initialized;
            OnZeroInitialize();
            ZeroCenterProxy.Master = new ZeroCenterProxy(Config.Master);
            IocHelper.Update();
        }

        #endregion

        #region Run

        /// <summary>
        ///     运行
        /// </summary>
        public static bool Run()
        {
            if (WorkModel != ZeroWorkModel.Bridge)
                return Start();
            ApplicationState = StationState.Run;
            return true;
        }

        /// <summary>
        ///     执行并等待
        /// </summary>
        public static void RunAwaite()
        {
            Start();
            Console.CancelKeyPress += (s, e) =>
            {
                Shutdown();
            };
            _waitToken = new SemaphoreSlim(0, 1);
            ZeroTrace.SystemLog("MicroZero services is runing. Press Ctrl+C to shutdown.");
            _waitToken.Wait();

        }

        /// <summary>
        ///     应用程序等待结果的信号量对象
        /// </summary>
        private static SemaphoreSlim _waitToken;

        /// <summary>
        ///     启动
        /// </summary>
        private static bool Start()
        {
            ZeroCenterState = ZeroCenterState.None;
            ApplicationState = StationState.Start;
            var success = JoinCenter();
            if (WorkModel == ZeroWorkModel.Service)
            {
                new Thread(SystemMonitor.Monitor)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest
                }.Start();

                new Thread(ApiChecker.RunCheck)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest
                }.Start();
            }
            SystemMonitor.WaitMe();
            return success;
        }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static bool JoinCenter()
        {
            if (ApplicationIsRun)
                return false;
            ApplicationState = StationState.BeginRun;
            ZeroCenterState = ZeroCenterState.Start;
            ZeroTrace.SystemLog("ZeroCenter", "JoinCenter", $"try connect zero center ({Config.Master.ManageAddress})...");
            if (!ZeroCenterProxy.Master.PingCenter())
            {
                SetFailed();
                ZeroTrace.WriteError("ZeroCenter", "JoinCenter", "zero center can`t connection.");
                return false;
            }
            ZeroCenterState = ZeroCenterState.Run;
            if (WorkModel == ZeroWorkModel.Service && !ZeroCenterProxy.Master.HeartJoin())
            {
                SetFailed();
                ZeroTrace.WriteError("ZeroCenter", "JoinCenter", "zero center can`t join.");
                return false;
            }

            Config.ClearConfig();
            if (!ConfigManager.LoadAllConfig())
            {
                SetFailed();
                ZeroTrace.WriteError("ZeroCenter", "JoinCenter", "station configs can`t loaded.");
                return false;
            }
            ZeroTrace.SystemLog("ZeroCenter", "JoinCenter", "be connected successfully,start local stations.");
            if (WorkModel == ZeroWorkModel.Service)
            {
                var cm = new ConfigManager(ZeroApplication.Config.Master);
                cm.UploadDocuments();
                OnZeroStart();
                //Task.Factory.StartNew(OnZeroStart, TaskCreationOptions.LongRunning);
            }
            //else if (WorkModel == ZeroWorkModel.Client)
            //{
            //    ZeroConnectionPool.Pool = new SocketPool();
            //    ZeroConnectionPool.Pool.OnZeroStart();
            //}
            return true;
        }

        #endregion

        #region Destroy

        /// <summary>
        ///     关闭
        /// </summary>
        public static void Shutdown()
        {
            ZeroTrace.SystemLog("Begin shutdown...");
            switch (ApplicationState)
            {
                case StationState.Destroy:
                    return;
                case StationState.BeginRun:
                case StationState.Run:
                    OnZeroEnd();
                    break;
                case StationState.Failed:
                    if (WorkModel == ZeroWorkModel.Service)
                        ZeroCenterProxy.Master.HeartLeft();
                    break;
            }
            ApplicationState = StationState.Destroy;
            if (WorkModel != ZeroWorkModel.Bridge)
            {
                if (GlobalObjects.Count > 0)
                    GlobalSemaphore.Wait();
                OnZeroDestory();
                SystemMonitor.WaitMe();
            }
            else
            {
                Thread.Sleep(1000);
            }
            ZeroTrace.SystemLog("Application shutdown ,see you late.");
            GC.Collect();
            ZContext.Destroy();
            ApplicationState = StationState.Disposed;
            _waitToken?.Release();
            LogRecorderX.Shutdown();
        }

        #endregion

        #endregion

        #region Event

        /// <summary>
        /// 站点事件发生
        /// </summary>
        public static event EventHandler<ZeroNetEventArgument> ZeroNetEvent;

        /// <summary>
        /// 发出事件
        /// </summary>
        public static void InvokeEvent(ZeroNetEventType centerEvent, string name, string context, StationConfig config, bool sync = false)
        {
            if (Config.CanRaiseEvent != true)
                return;
            var args = new ZeroNetEventArgument(centerEvent, name, context, config);
            if (sync)
                InvokeEvent(args);
            else
                Task.Factory.StartNew(InvokeEvent, args);
        }

        /// <summary>
        /// 发出事件
        /// </summary>
        /// <param name="event"></param>
        /// <param name="sync">是否为同步操作</param>
        internal static void RaiseEvent(ZeroNetEventType @event, bool sync = false)
        {
            if (Config.CanRaiseEvent != true)
                return;
            var args = new ZeroNetEventArgument(@event, null, null, null);
            if (sync)
                InvokeEvent(args);
            else
                Task.Factory.StartNew(InvokeEvent, args);
        }

        /// <summary>
        /// 发出事件
        /// </summary>
        /// <param name="args"></param>
        private static void InvokeEvent(object args)
        {
            var arg = (ZeroNetEventArgument)args;
            try
            {
                ZeroNetEvent?.Invoke(Config, arg);
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("ZeroNetEvent", e, arg.Event);
            }
        }
        #endregion
    }
}