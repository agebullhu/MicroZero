using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.Logging;

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
        public static async Task CommandConsole()
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
                        await Shutdown();
                        return;
                    case "start":
                        await Start();
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
                if (_appState != StationState.Disposed)
                {
                    MonitorStateMachine.SyncAppState();
                    ZeroTrace.SystemLog("ZeroApplication", StationState.Text(value), MonitorStateMachine.StateMachine.GetTypeName());
                }
            }
        }

        /// <summary>
        /// 设置ZeroCenter与Application状态都为Failed
        /// </summary>
        public static void SetFailed()
        {
            ZeroCenterState = ZeroCenterState.Failed;
            ApplicationState = StationState.Failed;
        }

        /// <summary>
        ///     应用中心是否正在运行
        /// </summary>
        public static bool ZerCenterIsRun => ZeroCenterState == ZeroCenterState.Run;

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
            
            Console.WriteLine("Weconme MicroZero");
            ZContext.Initialize();
            //ZeroTrace.Initialize();
            ThreadPool.GetMaxThreads(out var worker, out _);
            ThreadPool.SetMaxThreads(worker, 4096);
            //ThreadPool.GetAvailableThreads(out worker, out io);

            CheckConfig();
            InitializeDependency();
            ShowOptionInfo();
        }


        /// <summary>
        ///     设置LogRecorder的依赖属性(内部使用)
        /// </summary>
        private static void InitializeDependency()
        {
            GlobalContext.ServiceKey = Config.Master.ServiceKey;
            GlobalContext.ServiceName = Config.ServiceName;
            GlobalContext.ServiceRealName = $"{Config.ServiceName}:{Config.StationName}:{RandomOperate.Generate(4)}";

            //日志
            LogRecorder.LogPath = Config.LogFolder;
            LogRecorder.GetMachineNameFunc = () => GlobalContext.ServiceRealName;
            LogRecorder.GetUserNameFunc = () => GlobalContext.CurrentNoLazy?.User?.UserId.ToString() ?? "*";
            LogRecorder.GetRequestIdFunc = () => GlobalContext.CurrentNoLazy?.Request?.RequestId ?? RandomOperate.Generate(10);
            LogRecorder.Initialize();
            IocScope.Logger = IocHelper.Create<ILoggerFactory>().CreateLogger("MicroZero");

            //插件
            AddInImporter.Import();
            AddInImporter.Instance.Initialize();

            //注册默认广播
            IocHelper.AddSingleton<IZeroPublisher, ZPublisher>();

            var testContext = IocHelper.Create<GlobalContext>();
            if (testContext == null)
                IocHelper.AddScoped<GlobalContext, GlobalContext>();

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
            RegistZeroObject<ApiProxy>();
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
            var task = Start();
            task.Wait();
            return task.Result;
        }

        /// <summary>
        ///     运行
        /// </summary>
        public static async Task<bool> RunAsync()
        {
            return await Start();
        }

        /// <summary>
        ///     执行并等待
        /// </summary>
        public static void RunAwaite()
        {
            Start().Wait();
            Console.CancelKeyPress += OnConsoleOnCancelKeyPress;
            ZeroTrace.SystemLog("MicroZero services is runing. Press Ctrl+C to shutdown.");
            WaitToken.Wait(CancelToken.Token);
        }

        private static readonly CancellationTokenSource CancelToken = new CancellationTokenSource();
        /// <summary>
        ///     执行并等待
        /// </summary>
        public static async Task RunAwaiteAsync()
        {
            await Start();
            Console.CancelKeyPress += OnConsoleOnCancelKeyPress;
            ZeroTrace.SystemLog("MicroZero services is runing. Press Ctrl+C to shutdown.");
            await WaitToken.WaitAsync(CancelToken.Token);
        }

        private static void OnConsoleOnCancelKeyPress(object s, ConsoleCancelEventArgs e)
        {
            Shutdown().Wait();
        }

        /// <summary>
        ///     应用程序等待结果的信号量对象
        /// </summary>
        private static readonly SemaphoreSlim WaitToken = new SemaphoreSlim(0);

        /// <summary>
        ///     启动
        /// </summary>
        private static async Task<bool> Start()
        {
            ZeroCenterState = ZeroCenterState.None;
            ApplicationState = StationState.Start;
            var success = await JoinCenter();
#pragma warning disable 4014
            SystemMonitor.Monitor();
            if (WorkModel == ZeroWorkModel.Service)
                new Thread(ApiChecker.RunCheck).Start();
#pragma warning restore 4014
            return success;
        }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static async Task<bool> JoinCenter()
        {
            await Task.Yield();
            if (ApplicationIsRun)
                return false;
            ZeroCenterState = ZeroCenterState.Start;
            ApplicationState = StationState.BeginRun;
            ZeroTrace.SystemLog("ZeroCenter", "JoinCenter", $"try connect zero center ({Config.Master.ManageAddress})...");
            if (!ZeroCenterProxy.Master.PingCenter())
            {
                SetFailed();
                ZeroTrace.WriteError("ZeroCenter", "JoinCenter", "zero center can`t connection.");
                return false;
            }
            ZeroCenterState = ZeroCenterState.Run;
            if (!ZeroCenterProxy.Master.HeartJoin())
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
                var m = new ConfigManager(Config.Master);
                m.UploadDocument();
            }
            await OnZeroStart();
            return true;
        }

        #endregion

        #region Destroy

        /// <summary>
        ///     关闭
        /// </summary>
        public static async Task Shutdown()
        {
            ZeroTrace.SystemLog("Begin shutdown...");
            switch (ApplicationState)
            {
                case StationState.Destroy:
                    return;
                case StationState.BeginRun:
                case StationState.Run:
                    if (WorkModel == ZeroWorkModel.Service)
                        ZeroCenterProxy.Master.HeartLeft();
                    await OnZeroEnd(false);
                    break;
                case StationState.Failed:
                    if (WorkModel == ZeroWorkModel.Service)
                        ZeroCenterProxy.Master.HeartLeft();
                    break;
            }
            ApplicationState = StationState.Destroy;
            if (GlobalObjects.Count > 0)
                GlobalSemaphore.Wait();
            OnZeroDestory();
            await SystemMonitor.WaitMe();
            ApplicationState = StationState.Disposed;
            //LogRecorder.Shutdown(); 
            ZContext.Destroy();
            ZeroTrace.SystemLog("Application shutdown ,see you late.");

            WaitToken.Release();
            CancelToken.Cancel();
        }

        #endregion

        #endregion

        #region Event

        /// <summary>
        /// 站点事件发生
        /// </summary>
        public static List<Func<ZeroAppConfigRuntime, ZeroNetEventArgument, Task>> ZeroNetEvents = new List<Func<ZeroAppConfigRuntime, ZeroNetEventArgument, Task>>();

        /// <summary>
        /// 发出事件
        /// </summary>
        public static void InvokeEvent(ZeroNetEventType centerEvent, string name, string context, StationConfig config, bool sync = false)
        {
            if (Config.CanRaiseEvent != true)
                return;
            var args = new ZeroNetEventArgument(centerEvent, name, context, config);
            InvokeEvent(args, !sync);
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
            InvokeEvent(args, !sync);
        }

        /// <summary>
        /// 发出事件
        /// </summary>
        /// <param name="args"></param>
        /// <param name="waitEnd"></param>
        private static void InvokeEvent(object args, bool waitEnd)
        {
            var tasks = new List<Task>();
            var arg = (ZeroNetEventArgument)args;
            foreach (var action in ZeroNetEvents.ToArray())
            {
                try
                {
                    tasks.Add(action?.Invoke(Config, arg));
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException("ZeroNetEvent", e, arg.Event);
                }
            }

            if (!waitEnd || tasks.Count == 0)
                return;
            Task.WaitAll(tasks.ToArray());
        }
        #endregion
    }
}