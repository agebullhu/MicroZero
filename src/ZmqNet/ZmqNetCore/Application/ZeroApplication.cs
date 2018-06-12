using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Agebull.Common.Logging;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Microsoft.Extensions.Configuration;
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
        ///     当前应用名称
        /// </summary>
        public static string AppName { get; set; }

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


        /// <summary>
        ///     配置校验
        /// </summary>
        private static void CheckConfig()
        {
            AppName = ConfigurationManager.Root["AppName"];
            ZeroTrace.WriteLine($"Weconme {AppName}");
            ZeroTrace.WriteInfo("Option", "ZeroMQ", zmq.LibraryVersion);
            string file;
            var root = ConfigurationManager.Root.GetValue("contentRoot", Environment.CurrentDirectory);
            if (ConfigurationManager.Root["ASPNETCORE_ENVIRONMENT_"] == "Development")
            {
                ZeroTrace.WriteInfo("Option", "Development");
                // ReSharper disable once AssignNullToNotNullAttribute
                file = Path.Combine(root, "zero.json");
            }
            else
            {
                ZeroTrace.WriteInfo("Option", RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" : "Windows");
                root = Path.GetDirectoryName(root);
                // ReSharper disable once AssignNullToNotNullAttribute
                file = Path.Combine(root, "config", "zero.json");
            }

            ConfigurationManager.Root["rootPath"] = root;
            ZeroTrace.WriteInfo("Option", "RootPath", root);
            if (File.Exists(file))
                ConfigurationManager.Load(file);

            var sec = ConfigurationManager.Get("Zero");

            Config = sec.Child<ZeroAppConfig>(AppName ?? "Station") ?? new ZeroAppConfig
            {
                StationName = AppName
            };
            //if (Config == null)
            //    throw new Exception($"无法找到主配置节点,路径为Zero.{AppName ?? "Station"},可在appsettings.json或{file}中设置");
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


            Config.ServiceName = global.ServiceName?.Trim();
            Config.ZeroAddress = global.ZeroAddress?.Trim();
            Config.ZeroManagePort = global.ZeroManagePort;
            Config.ZeroMonitorPort = global.ZeroMonitorPort;

            Config.ZeroManageAddress = ZeroIdentityHelper.GetRequestAddress("SystemManage", Config.ZeroManagePort);
            Config.ZeroMonitorAddress = ZeroIdentityHelper.GetWorkerAddress("SystemMonitor", Config.ZeroMonitorPort);

            Config.LocalIpAddress = GetHostIps();
            Config.DataFolder = IOHelper.CheckPath(global.DataFolder, Config.StationName);
            if (Config.ShortName == null)
                Config.ShortName = Config.StationName;
            if (Config.ServiceKey == null)
                Config.ServiceKey = RandomOperate.Generate(4);
            Config.RealName = ZeroIdentityHelper.CreateRealName(false);
            Config.Identity = Config.RealName.ToAsciiBytes();
            //模式选择

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
            Config.SpeedLimitModel = global.SpeedLimitModel;
            Config.MaxWait = global.MaxWait;
            Config.TaskCpuMultiple = global.TaskCpuMultiple;

            ShowOptionInfo();
        }

        static void ShowOptionInfo()
        {
            string model;
            switch (Config.SpeedLimitModel)
            {
                default:
                    model = "单线程:线程(1) 等待(0)";
                    break;
                case SpeedLimitType.ThreadCount:
                    var max = (int)(Environment.ProcessorCount * Config.TaskCpuMultiple);
                    if (max < 1)
                        max = 1;
                    model =
                        $"按线程数限制:线程({Environment.ProcessorCount}×{Config.TaskCpuMultiple}={max}) 等待({Config.MaxWait})";
                    break;
                case SpeedLimitType.WaitCount:
                    model = $"按等待数限制:线程(1) 等待({Config.MaxWait})";
                    break;
            }

            ZeroTrace.WriteInfo("Option", model);
            ZeroTrace.WriteInfo("Option", "ZeroCenter", Config.ZeroManageAddress, Config.ZeroMonitorAddress);
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
        public static bool IsDisposed => ApplicationState == StationState.Disposed;

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

        /// <summary>
        /// 已注册的对象
        /// </summary>
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
        /// 活动对象(执行中)
        /// </summary>
        private static readonly List<IZeroObject> ActiveObjects = new List<IZeroObject>();

        /// <summary>
        /// 活动对象(执行中)
        /// </summary>
        private static readonly List<IZeroObject> FailedObjects = new List<IZeroObject>();

        /// <summary>
        /// 全局执行对象(内部的Task)
        /// </summary>
        private static readonly List<IZeroObject> GlobalObjects = new List<IZeroObject>();

        /// <summary>
        ///     对象活动状态记录器锁定
        /// </summary>
        private static readonly SemaphoreSlim ActiveSemaphore = new SemaphoreSlim(0, short.MaxValue);

        /// <summary>
        ///     对象活动状态记录器锁定
        /// </summary>
        private static readonly SemaphoreSlim GlobalSemaphore = new SemaphoreSlim(0, short.MaxValue);

        /// <summary>
        ///     重置当前活动数量
        /// </summary>
        public static void ResetObjectActive()
        {
            ActiveObjects.Clear();
            FailedObjects.Clear();
        }

        /// <summary>
        ///     对象活动时登记
        /// </summary>
        public static void OnGlobalStart(IZeroObject obj)
        {
            lock (GlobalObjects)
            {
                GlobalObjects.Add(obj);
                ZeroTrace.WriteInfo(obj.Name, "GlobalStart");
            }
        }

        /// <summary>
        ///     对象活动时登记
        /// </summary>
        public static void OnGlobalEnd(IZeroObject obj)
        {
            lock (GlobalObjects)
            {
                GlobalObjects.Remove(obj);
                ZeroTrace.WriteInfo(obj.Name, "GlobalEnd");
                if (GlobalObjects.Count == 0)
                    GlobalSemaphore.Release();
            }
        }

        /// <summary>
        ///     对象活动时登记
        /// </summary>
        public static void OnObjectActive(IZeroObject obj)
        {
            lock (ActiveObjects)
            {
                ActiveObjects.Add(obj);
                ZeroTrace.WriteInfo(obj.Name, "Run");
                if (ActiveObjects.Count + FailedObjects.Count == ZeroObjects.Count)
                    ActiveSemaphore.Release(); //发出完成信号
            }
        }

        /// <summary>
        ///     对象关闭时登记
        /// </summary>
        public static void OnObjectClose(IZeroObject obj)
        {
            lock (ActiveObjects)
            {
                ActiveObjects.Remove(obj);
                ZeroTrace.WriteInfo(obj.Name, "Closed");
                if (ActiveObjects.Count == 0)
                    ActiveSemaphore.Release(); //发出完成信号
            }
        }

        /// <summary>
        ///     对象关闭时登记
        /// </summary>
        public static void OnObjectFailed(IZeroObject obj)
        {
            lock (ActiveObjects)
            {
                FailedObjects.Add(obj);
                ZeroTrace.WriteInfo(obj.Name, "Failed");
                if (ActiveObjects.Count + FailedObjects.Count == ZeroObjects.Count)
                    ActiveSemaphore.Release(); //发出完成信号
            }
        }

        /// <summary>
        ///     是否存在活动对象
        /// </summary>
        public static bool HaseActiveObject => ActiveObjects.Count > 0;

        /// <summary>
        ///     等待所有对象信号(全开或全关)
        /// </summary>
        public static void WaitAllObjectSemaphore()
        {
            ActiveSemaphore.Wait();
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
                ZeroTrace.WriteInfo("RegistZeroObject", obj.Name);
                ZeroObjects.Add(obj);
                if (ApplicationState >= StationState.Initialized)
                {
                    try
                    {
                        obj.OnZeroInitialize();
                        ZeroTrace.WriteInfo(obj.Name, "Initialize");
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e);
                        ZeroTrace.WriteError(obj.Name, "Initialize", e);
                    }
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
                ZeroTrace.WriteLine("[OnZeroInitialize>>");


                Parallel.ForEach(ZeroObjects.ToArray(), obj =>
                {
                    try
                    {
                        obj.OnZeroInitialize();
                        ZeroTrace.WriteInfo(obj.Name, "Initialize");
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e);
                        ZeroTrace.WriteError(obj.Name, "*Initialize", e);
                    }
                });
                ZeroTrace.WriteLine("<<OnZeroInitialize]");
            }
        }

        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static void OnZeroStart()
        {
            Debug.Assert(!HaseActiveObject);
            using (OnceScope.CreateScope(ZeroObjects, ResetObjectActive))
            {
                ZeroTrace.WriteLine("[OnZeroStart>>");
#if DEBUG
                foreach (var obj in ZeroObjects.ToArray())
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
                }
#else
                Parallel.ForEach(ZeroObjects.ToArray(), obj =>
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
#endif
                WaitAllObjectSemaphore();
            }
            SystemManager.HeartReady();
            ApplicationState = StationState.Run;
            SystemMonitor.RaiseEvent("program_run");
            ZeroTrace.WriteLine("<<OnZeroStart]");
        }


        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static void OnStationStateChanged(StationConfig config)
        {
            using (OnceScope.CreateScope(ZeroObjects))
            {
                ZeroTrace.WriteLine("[OnStationStateChanged>>");
                Parallel.ForEach(ActiveObjects.ToArray(), obj =>
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
                ZeroTrace.WriteLine("<<OnStationStateChanged]");
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
                Parallel.ForEach(ActiveObjects.ToArray(), obj =>
                {
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
                ZeroTrace.WriteLine("[OnZeroEnd>>");
                SystemManager.HeartLeft();
                ApplicationState = StationState.Closing;
                if (HaseActiveObject)
                {
                    Parallel.ForEach(ActiveObjects.ToArray(), obj =>
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
                ZeroTrace.WriteLine("<<OnZeroEnd]");
            }
        }

        /// <summary>
        ///     注销时调用
        /// </summary>
        internal static void OnZeroDestory()
        {
            if (!Monitor.TryEnter(ZeroObjects))
                return;
            ZeroTrace.WriteLine("[OnZeroDestory>>");
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

                ZeroTrace.WriteLine("<<OnZeroDestory]");

                ZeroTrace.WriteLine("[OnZeroDispose>>");
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
                ZeroTrace.WriteLine("<<OnZeroDispose]");
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
            ZeroTrace.Initialize();
            CheckConfig();
            InitializeDependency();
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
            ZeroTrace.WriteInfo("Discove", discover.Assembly.FullName);
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
            RegistZeroObject(ZeroConnectionPool.CreatePool());
            AddInImporter.Instance.AutoRegist();

            ApplicationState = StationState.Initialized;
            OnZeroInitialize();
        }

        #endregion

        #region Run

        /// <summary>
        ///     启动
        /// </summary>
        private static void Start()
        {
            ZContext.Initialize();

            using (OnceScope.CreateScope(Config))
            {
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
            Console.WriteLine("Application started. Press Ctrl+C to shutdown.");
            WaitToken.Wait();
        }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static bool JoinCenter()
        {
            ZeroTrace.WriteLine($"try connect zero center ({Config.ZeroManageAddress})...");
            if (!SystemManager.PingCenter())
            {
                ApplicationState = StationState.Failed;
                ZerCenterStatus = ZeroCenterState.Failed;
                ZeroTrace.WriteError("JoinCenter", "zero center can`t connection.");
                return false;
            }

            ZerCenterStatus = ZeroCenterState.Run;
            ApplicationState = StationState.BeginRun;
            if (!SystemManager.HeartJoin())
            {
                ApplicationState = StationState.Failed;
                ZerCenterStatus = ZeroCenterState.Failed;
                ZeroTrace.WriteError("JoinCenter", "zero center can`t connection.");
                return false;
            }

            if (!SystemManager.LoadAllConfig())
            {
                ApplicationState = StationState.Failed;
                ZerCenterStatus = ZeroCenterState.Failed;
                return false;
            }

            ZeroTrace.WriteLine("be connected successfully,start local stations.");
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
            ZeroTrace.WriteLine("Begin shutdown...");
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
            ApplicationState = StationState.Destroy;
            if (GlobalObjects.Count > 0)
                GlobalSemaphore.Wait();
            OnZeroDestory();
            SystemManager.Destroy();
            LogRecorder.Shutdown();
            SystemMonitor.WaitMe();
            ZContext.Destroy();
            ZeroTrace.WriteLine("Application shutdown ,see you late.");
            ApplicationState = StationState.Disposed;
            WaitToken.Release();
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Shutdown();
        }

        #endregion

        #endregion
    }
}