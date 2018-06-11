using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZeroMQ;
using ZeroMQ.lib;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     站点应用
    /// </summary>
    public class ZeroApplication
    {
        #region Console

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
                    ZeroTrace.WriteLine("请输入正确命令");
                    continue;
                }

                var result = SystemManager.CallCommand(words);
                if (result.InteractiveSuccess)
                    ZeroTrace.WriteInfo("Console", result.TryGetValue(ZeroFrameType.TextValue, out var value)
                        ? value
                        : result.State.Text());
                else
                    ZeroTrace.WriteError("Console", result.TryGetValue(ZeroFrameType.TextValue, out var value)
                        ? value
                        : result.State.Text());
            }
        }

        #endregion

        #region Config

        /// <summary>
        ///     站点配置
        /// </summary>
        public static ZeroAppConfig Config { get; set; }

        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public static StationConfig GetConfig(string stationName)
        {
            if (Config.TryGetConfig(stationName, out var config)) return config;
            config = SystemManager.LoadConfig(stationName);
            if (config == null)
                return null;
            Config[stationName] = config;
            return config;
        }

        #endregion

        #region State

        /// <summary>
        ///     ZeroCenter是否正在运行
        /// </summary>
        public static bool ZerCenterIsRun => ZerCenterStatus == ZeroCenterState.Run;

        /// <summary>
        ///     服务器状态
        /// </summary>
        public static ZeroCenterState ZerCenterStatus { get; internal set; }

        /// <summary>
        ///     运行状态（本地与服务器均正常）
        /// </summary>
        public static bool CanDo =>
            (ApplicationState == StationState.BeginRun || ApplicationState == StationState.Run) &&
            ZerCenterStatus == ZeroCenterState.Run;

        /// <summary>
        ///     运行状态（本地未关闭）
        /// </summary>
        public static bool IsAlive => ApplicationState < StationState.Destroy;

        /// <summary>
        ///     已关闭
        /// </summary>
        public static bool IsDestroy => ApplicationState == StationState.Destroy;

        /// <summary>
        ///     已关闭
        /// </summary>
        public static bool IsClosed => ApplicationState >= StationState.Closed;

        /// <summary>
        ///     是否正在运行
        /// </summary>
        public static bool InRun => ApplicationState == StationState.Run;

        /// <summary>
        ///     运行状态
        /// </summary>
        internal static int _appState;

        /// <summary>
        ///     状态
        /// </summary>
        public static int ApplicationState
        {
            get => _appState;
            internal set => Interlocked.Exchange(ref _appState, value);
        }

        #endregion

        #region IZeroObject

        private static readonly List<IZeroObject> ZeroObjects = new List<IZeroObject>();


        /// <summary>
        ///     系统启动时调用
        /// </summary>
        public static bool RegistZeroObject<TZeroObject>() where TZeroObject : class, IZeroObject, new()
        {
            using (OnceScope.CreateScope(ZeroObjects))
            {
                return !ZeroObjects.Any(p => p is TZeroObject) && RegistZeroObject(new TZeroObject());
            }
        }

        /// <summary>
        ///     对象活动状态记录器锁定
        /// </summary>
        private static readonly SemaphoreSlim ObjectsSemaphore = new SemaphoreSlim(0, short.MaxValue);

        /// <summary>
        ///     当前活动对象数量
        /// </summary>
        private static int ActiveCount;

        /// <summary>
        ///     当前活动对象数量
        /// </summary>
        private static int FailedCount;

        /// <summary>
        ///     重置当前活动数量
        /// </summary>
        public static void ResetObjectActive()
        {
            Interlocked.Exchange(ref ActiveCount, 0);
        }

        /// <summary>
        ///     对象活动时登记
        /// </summary>
        public static void OnObjectActive()
        {
            if (Interlocked.Increment(ref ActiveCount) + FailedCount == ZeroObjects.Count)
                ObjectsSemaphore.Release(); //发出完成信号
        }

        /// <summary>
        ///     对象关闭时登记
        /// </summary>
        public static void OnObjectClose()
        {
            if (Interlocked.Decrement(ref ActiveCount) == 0)
                ObjectsSemaphore.Release(); //所有对象均关闭时发出信号
        }

        /// <summary>
        ///     对象关闭时登记
        /// </summary>
        public static void OnObjectFailed()
        {
            if (ActiveCount + Interlocked.Increment(ref FailedCount) == ZeroObjects.Count)
                ObjectsSemaphore.Release(); //发出完成信号
        }

        /// <summary>
        ///     等待所有对象关闭
        /// </summary>
        public static bool HaseActiveObject => ActiveCount > 0;

        /// <summary>
        ///     等待所有对象信号(全开或全关)
        /// </summary>
        public static void WaitAllObjectSemaphore()
        {
            ObjectsSemaphore.Wait();
        }

        /// <summary>
        ///     系统启动时调用
        /// </summary>
        public static bool RegistZeroObject(IZeroObject obj)
        {
            using (OnceScope.CreateScope(ZeroObjects))
            {
                if (ZeroObjects.Contains(obj))
                    return false;
                ZeroTrace.WriteInfo(AppName, $"RegistZeroObject:{obj.Name}");
                ZeroObjects.Add(obj);
                if (ApplicationState >= StationState.Initialized)
                    try
                    {
                        ZeroTrace.WriteInfo(obj.Name, "Initialize");
                        obj.OnZeroInitialize();
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e);
                        ZeroTrace.WriteError(obj.Name, "Initialize", e);
                    }

                if (!CanDo)
                    return true;
                try
                {
                    ZeroTrace.WriteInfo(obj.Name, "Start");
                    obj.OnZeroStart();
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    ZeroTrace.WriteError(obj.Name, "Start", e);
                }
            }

            return true;
        }

        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static void OnZeroInitialize()
        {
            using (OnceScope.CreateScope(ZeroObjects))
            {
                ZeroTrace.WriteInfo(AppName, "OnZeroInitialize....");
                RegistZeroObject(ZeroConnectionPool.CreatePool());
                Parallel.ForEach(ZeroObjects, obj =>
                {
                    try
                    {
                        obj.OnZeroInitialize();
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e);
                        ZeroTrace.WriteError(obj.Name, "*Initialize", e);
                    }
                });
                ZeroTrace.WriteInfo(AppName, "OnZeroInitialize");
            }
        }

        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static void OnZeroStart()
        {
            using (OnceScope.CreateScope(ZeroObjects, ResetObjectActive))
            {
                ZeroTrace.WriteInfo(AppName, "OnZeroStart....");
                Parallel.ForEach(ZeroObjects, obj =>
                {
                    try
                    {
                        ZeroTrace.WriteInfo(obj.Name, "*Start");
                        obj.OnZeroStart();
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e);
                        ZeroTrace.WriteError(obj.Name, "*Start", e);
                    }
                });
                WaitAllObjectSemaphore();
                SystemManager.HeartReady();
                ApplicationState = StationState.Run;
                SystemMonitor.RaiseEvent("program_run");
                ZeroTrace.WriteInfo(AppName, "OnZeroStart");
                ZeroTrace.WriteInfo(AppName, "All ZeroObject actived");
            }
        }


        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static void OnStationStateChanged(StationConfig config)
        {
            using (OnceScope.CreateScope(ZeroObjects))
            {
                if (!HaseActiveObject)
                    return;
                ZeroTrace.WriteInfo(AppName, "OnStationStateChanged...");
                Parallel.ForEach(ZeroObjects, obj =>
                {
                    try
                    {
                        obj.OnStationStateChanged(config);
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e);
                        ZeroTrace.WriteError(obj.Name, "OnStationStateChanged", e);
                    }
                });
                ZeroTrace.WriteInfo(AppName, "OnStationStateChanged");
            }
        }

        /// <summary>
        ///     心跳
        /// </summary>
        internal static void OnHeartbeat()
        {
            if (!InRun)
                return;
            using (OnceScope.CreateScope(ZeroObjects))
            {
                if (!InRun)
                    return;
                SystemManager.Heartbeat();
                if (!HaseActiveObject)
                    return;
                ZeroObjects.ForEach(obj =>
                {
                    if (!InRun)
                        return;
                    try
                    {
                        obj.OnHeartbeat();
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e);
                        ZeroTrace.WriteError(obj.Name, "OnHeartbeat", e);
                    }
                });
            }
        }

        /// <summary>
        ///     系统关闭时调用
        /// </summary>
        internal static void OnZeroEnd()
        {
            using (OnceScope.CreateScope(ZeroObjects))
            {
                ZeroTrace.WriteInfo(AppName, "OnZeroEnd....");
                SystemManager.HeartLeft();
                ApplicationState = StationState.Closing;
                if (HaseActiveObject)
                {
                    Parallel.ForEach(ZeroObjects, obj =>
                    {
                        try
                        {
                            ZeroTrace.WriteInfo(obj.Name, "*Close");
                            obj.OnZeroEnd();
                        }
                        catch (Exception e)
                        {
                            LogRecorder.Exception(e);
                            ZeroTrace.WriteError(obj.Name, "*Close", e);
                        }
                    });
                    WaitAllObjectSemaphore();
                }

                ApplicationState = StationState.Closed;
                ZeroTrace.WriteInfo(AppName, "OnZeroEnd");
            }
        }

        /// <summary>
        ///     注销时调用
        /// </summary>
        internal static void OnZeroDestory()
        {
            ZeroTrace.WriteInfo(AppName, "OnZeroDestory....");
            if (!Monitor.TryEnter(ZeroObjects))
                return;
            using (OnceScope.CreateScope(ZeroObjects))
            {
                var array = ZeroObjects.ToArray();
                ZeroObjects.Clear();
                Parallel.ForEach(array, obj =>
                {
                    try
                    {
                        ZeroTrace.WriteInfo(obj.Name, "*Destory");
                        obj.OnZeroDestory();
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e);
                        ZeroTrace.WriteError(obj.Name, "*Destory", e);
                    }
                });


                Parallel.ForEach(array, obj =>
                {
                    try
                    {
                        ZeroTrace.WriteInfo(obj.Name, "*Dispose");
                        obj.Dispose();
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e);
                        ZeroTrace.WriteError(obj.Name, "*Dispose", e);
                    }
                });
                ZeroTrace.WriteInfo(AppName, "OnZeroDestory");
            }
        }

        #endregion

        #region Flow

        #region Option

        /// <summary>
        ///     配置校验,作为第一步
        /// </summary>
        public static void CheckOption()
        {
            ZeroTrace.WriteInfo(AppName, zmq.LibraryVersion);

            CheckConfig();

            AppName = Config.StationName;
            ZeroTrace.WriteInfo(AppName, $"Weconme {AppName}");
            InitializeDependency();
        }


        /// <summary>
        ///     配置校验
        /// </summary>
        private static void CheckConfig()
        {
            var root = ConfigurationManager.Root.GetValue("contentRoot", Environment.CurrentDirectory);
            if (ConfigurationManager.Root["ASPNETCORE_ENVIRONMENT_"] == "Development")
            {
                ZeroTrace.WriteInfo(AppName, "Development");
                AppName = ConfigurationManager.Root["AppName"];
            }
            else
            {
                ZeroTrace.WriteInfo(AppName, RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" : "Windows");
                AppName = Path.GetFileName(root);
                root = Path.GetDirectoryName(root);
                IOHelper.CheckPath(root, "config");
            }
            var sec = ConfigurationManager.Get("Zero");
            if (sec == null || sec.IsEmpty)//说明未配置
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var file = Path.Combine(root, "zero.json");
                if (File.Exists(file))
                    ConfigurationManager.Load(file);
                ConfigurationManager.Load("host.json");
                sec = ConfigurationManager.Get("Zero");
            }

            Config = sec.Child<ZeroAppConfig>(AppName ?? "Station");
            Config.IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var global = sec.Child<ZeroAppConfig>("Global");

            TxtRecorder.LogPath = IOHelper.CheckPath(root, "logs");
            if (global.DataFolder == null)
                global.DataFolder = IOHelper.CheckPath(root, "datas");
            if (global.ServiceName == null)
                global.ServiceName = Dns.GetHostName();
            if (global.ZeroManagePort == 0)
                global.ZeroManagePort = 8000;
            if (global.ZeroMonitorPort == 0)
                global.ZeroMonitorPort = 8001;

            if (global.SpeedLimitModel < SpeedLimitType.Single)
                global.SpeedLimitModel = SpeedLimitType.Single;
            else if (global.SpeedLimitModel > SpeedLimitType.WaitCount)
                global.SpeedLimitModel = SpeedLimitType.ThreadCount;

            if (global.TaskCpuMultiple < 0)
                global.TaskCpuMultiple = 0.5M;
            else if (global.TaskCpuMultiple > 64)
                global.TaskCpuMultiple = 64;
            switch (global.SpeedLimitModel)
            {
                default:
                    //case SpeedLimitType.Single:
                    global.MaxWait = 0;
                    break;
                case SpeedLimitType.ThreadCount:
                    if (global.MaxWait < 0xFF)
                        global.MaxWait = 0xFF;
                    else if (global.MaxWait > 0xFFFFF)
                        global.MaxWait = 0xFFFFF;
                    break;
                case SpeedLimitType.WaitCount:
                    if (global.MaxWait < 0xFF)
                        global.MaxWait = 0xFF;
                    else if (global.MaxWait > 0xFFFFF)
                        global.MaxWait = 0xFFFFF;
                    break;
            }

            Config.ServiceName = global.ServiceName?.Trim();
            Config.ZeroAddress = global.ZeroAddress?.Trim();
            Config.ZeroManagePort = global.ZeroManagePort;
            Config.ZeroMonitorPort = global.ZeroMonitorPort;

            Config.LocalIpAddress = GetHostIps();
            Config.DataFolder = IOHelper.CheckPath(global.DataFolder, Config.StationName);
            if (Config.ShortName == null)
                Config.ShortName = Config.StationName;
            if (Config.ServiceKey == null)
                Config.ServiceKey = RandomOperate.Generate(4);
            Config.RealName = ZeroIdentityHelper.CreateRealName(false);
            Config.Identity = Config.RealName.ToAsciiBytes();

            Config.SpeedLimitModel = global.SpeedLimitModel;
            Config.MaxWait = global.MaxWait;
            Config.TaskCpuMultiple = global.TaskCpuMultiple;


            Config.ZeroManageAddress = ZeroIdentityHelper.GetRequestAddress("SystemManage", Config.ZeroManagePort);
            Config.ZeroMonitorAddress = ZeroIdentityHelper.GetWorkerAddress("SystemMonitor", Config.ZeroMonitorPort);

            string model;
            switch (global.SpeedLimitModel)
            {
                default:
                    model = "单线程:线程(1) 等待(0)";
                    break;
                case SpeedLimitType.ThreadCount:
                    var max = (int)(Environment.ProcessorCount * global.TaskCpuMultiple);
                    model =
                        $"按线程数限制:线程({Environment.ProcessorCount}×{global.TaskCpuMultiple}={max}) 等待({global.MaxWait})";
                    break;
                case SpeedLimitType.WaitCount:
                    model = $"按等待数限制:线程(1) 等待({global.MaxWait})";
                    break;
            }

            ZeroTrace.WriteInfo("限速", model);
            ZeroTrace.WriteInfo("ZeroCenter", Config.ZeroManageAddress, Config.ZeroMonitorAddress);
        }

        /// <summary>
        ///     设置LogRecorder的依赖属性(内部使用)
        /// </summary>
        private static void InitializeDependency()
        {
            ApiContext.MyRealName = Config.RealName;
            ApiContext.MyServiceKey = Config.ServiceKey;
            ApiContext.MyServiceName = Config.ServiceName;
            LogRecorder.GetMachineNameFunc = () => Config.ServiceName;
            LogRecorder.GetUserNameFunc = () => ApiContext.Customer?.Account ?? "Unknow";
            LogRecorder.GetRequestIdFunc = () => ApiContext.RequestContext?.RequestId ?? Guid.NewGuid().ToString();

            AddInImporter.Importe();


            AddInImporter.Instance.Initialize();

            IocHelper.SetServiceProvider(IocHelper.ServiceCollection.BuildServiceProvider());

            LogRecorder.Initialize();
        }

        private static string GetHostIps()
        {
            var ips = new StringBuilder();
            var first = true;
            foreach (var address in Dns.GetHostAddresses(Config.ServiceName))
            {
                if (address.IsIPv4MappedToIPv6 || address.IsIPv6LinkLocal || address.IsIPv6Multicast ||
                    address.IsIPv6SiteLocal || address.IsIPv6Teredo)
                    continue;
                var ip = address.ToString();
                if (ip == "127.0.0.1" || ip == "127.0.1.1" || ip == "::1" || ip == "-1")
                    continue;
                if (first)
                    first = false;
                else
                    ips.Append(" , ");
                ips.Append(ip);
            }

            return ips.ToString();
        }

        /// <summary>
        ///     发现
        /// </summary>
        public static void Discove(Assembly assembly = null)
        {
            var discover = new ZeroDiscover { Assembly = assembly ?? Assembly.GetCallingAssembly() };
            discover.FindApies();
            discover.FindZeroObjects();
        }

        #endregion

        #region Initialize

        /// <summary>
        ///     当前应用名称
        /// </summary>
        public static string AppName { get; set; }

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize()
        {
            ApplicationState = StationState.Initialized;

            //引发静态构造
            ZeroTrace.WriteInfo(AppName, "Initialize...");
            ZeroTrace.Initialize();
            ZContext.Initialize();

            AddInImporter.Instance.AutoRegist();

            OnZeroInitialize();
        }

        #endregion

        #region Run

        /// <summary>
        ///     启动
        /// </summary>
        private static void Start()
        {
            using (OnceScope.CreateScope(Config))
            {
                ZeroTrace.WriteInfo(AppName, Config.ZeroManageAddress);
                ApplicationState = StationState.Start;
                Task.Factory.StartNew(SystemMonitor.Monitor);
                JoinCenter();
            }

            SystemMonitor.WaitMe();
        }

        /// <summary>
        ///     运行
        /// </summary>
        public static void Run()
        {
            Start();
        }

        /// <summary>
        ///     执行并等待
        /// </summary>
        public static void RunAwaite()
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            Console.WriteLine("Application start...");
            Start();
            Console.WriteLine("Application started. Press Ctrl+C to shutdown.");
            Task.Factory.StartNew(WaitTask).Wait();
        }

        /// <summary>
        ///     应用程序等待结果的信号量对象
        /// </summary>
        private static readonly SemaphoreSlim WaitToken = new SemaphoreSlim(0, 1);

        /// <summary>
        ///     执行直到连接成功
        /// </summary>
        private static void WaitTask()
        {
            WaitToken.Wait();
            Console.WriteLine("Application shutdown ,see you late.");
        }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static bool JoinCenter()
        {
            ZeroTrace.WriteInfo(AppName, $"try connect zero center ({Config.ZeroManageAddress})...");
            if (!SystemManager.PingCenter())
            {
                ApplicationState = StationState.Failed;
                ZerCenterStatus = ZeroCenterState.Failed;
                ZeroTrace.WriteError(AppName, "zero center can`t connection.");
                return false;
            }

            ZerCenterStatus = ZeroCenterState.Run;
            ApplicationState = StationState.BeginRun;
            if (!SystemManager.HeartJoin())
            {
                ApplicationState = StationState.Failed;
                ZerCenterStatus = ZeroCenterState.Failed;
                ZeroTrace.WriteError(AppName, "zero center can`t connection.");
                return false;
            }

            if (!SystemManager.LoadAllConfig())
            {
                ApplicationState = StationState.Failed;
                ZerCenterStatus = ZeroCenterState.Failed;
                return false;
            }

            ZeroTrace.WriteInfo(AppName, "be connected successfully,start local stations.");
            OnZeroStart();
            return true;
        }

        #endregion

        #region Destroy

        /// <summary>
        ///     关闭
        /// </summary>
        public static void Shutdown()
        {
            switch (ApplicationState)
            {
                case StationState.Destroy:
                    return;
                case StationState.BeginRun:
                case StationState.Run:
                    OnZeroEnd();
                    break;
                case StationState.Failed:
                    SystemManager.HeartLeft();
                    break;
            }

            ZeroTrace.WriteInfo(AppName, "Destroy");
            ApplicationState = StationState.Destroy;
            OnZeroDestory();
            SystemManager.Destroy();
            LogRecorder.Shutdown();
            SystemMonitor.WaitMe();
            ZContext.Destroy();
            ApplicationState = StationState.Disposed;
            WaitToken.Release();
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            ZeroTrace.WriteLine("Begin shutdown...");
            Shutdown();
        }

        #endregion

        #endregion
    }
}