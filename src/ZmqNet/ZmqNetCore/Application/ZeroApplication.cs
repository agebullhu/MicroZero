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
using Agebull.Common.Logging;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ZeroMQ;
using ZeroMQ.lib;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     站点应用
    /// </summary>
    public class ZeroApplication
    {
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
            if (Config.TryGetConfig(stationName, out var config))
            {
                return config;
            }
            if (ApplicationState != StationState.Run)
            {
                return null;
            }
            config = SystemManager.LoadConfig(stationName, out _);
            if (config == null)
                return null;
            Config[stationName] = config;
            return config;
        }
        #endregion

        #region State

        /// <summary>
        /// 运行状态（本地与服务器均正常）
        /// </summary>
        public static bool CanDo => ApplicationState == StationState.Run && ZerCenterStatus == ZeroCenterState.Run;

        /// <summary>
        /// 运行状态（本地未关闭）
        /// </summary>
        public static bool IsAlive => ApplicationState < StationState.Closing;

        /// <summary>
        ///     运行状态
        /// </summary>
        internal static int _appState;

        /// <summary>
        /// 服务器状态
        /// </summary>
        public static ZeroCenterState ZerCenterStatus
        {
            get;
            internal set;
        }

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
        /// 系统启动时调用
        /// </summary>
        public static bool RegistZeroObject<TZeroObject>() where TZeroObject : class, IZeroObject, new()
        {
            if (ZeroObjects.Any(p => p is TZeroObject))
                return false;
            return RegistZeroObject(new TZeroObject());
        }
        /// <summary>
        /// 系统启动时调用
        /// </summary>
        public static bool RegistZeroObject(IZeroObject obj)
        {
            if (ZeroObjects.Contains(obj))
                return false;
            ZeroTrace.WriteInfo("ZeroApplication", $"RegistZeroObject:{obj.Name}");
            Monitor.Enter(ZeroObjects);
            ZeroObjects.Add(obj);
            Monitor.Exit(ZeroObjects);
            if (ApplicationState >= StationState.Initialized)
            {

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
            }

            if (CanDo)
            {
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
        /// 系统启动时调用
        /// </summary>
        internal static void OnZeroInitialize()
        {
            ZeroTrace.WriteInfo("ZeroApplication", "OnZeroInitialize....");
            RegistZeroObject(ZeroPublisher.Instance);
            RegistZeroObject(ZeroConnectionPool.Instance);
            Monitor.Enter(ZeroObjects);
            foreach (var obj in ZeroObjects)
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
            }
            Monitor.Exit(ZeroObjects);
            ZeroTrace.WriteInfo("ZeroApplication", "OnZeroInitialize");
        }

        /// <summary>
        /// 系统启动时调用
        /// </summary>
        internal static void OnZeroStart()
        {
            ZeroTrace.WriteInfo("ZeroApplication", "OnZeroStart....");
            Monitor.Enter(ZeroObjects);
            foreach (var obj in ZeroObjects)
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
            Monitor.Exit(ZeroObjects);
            SystemMonitor.RaiseEvent("program_run");
            ZeroTrace.WriteInfo("ZeroApplication", "OnZeroStart");
        }


        /// <summary>
        /// 系统启动时调用
        /// </summary>
        internal static void OnStationStateChanged(StationConfig config)
        {
            ZeroTrace.WriteInfo("ZeroApplication", "OnStationStateChanged...");
            Monitor.Enter(ZeroObjects);
            foreach (var obj in ZeroObjects)
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
            }
            Monitor.Exit(ZeroObjects);
            ZeroTrace.WriteInfo("ZeroApplication", "OnStationStateChanged");
        }

        /// <summary>
        ///     要求心跳
        /// </summary>
        internal static void OnHeartbeat()
        {
            Monitor.Enter(ZeroObjects);
            SystemManager.Heartbeat();
            foreach (var obj in ZeroObjects)
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
            }
            Monitor.Exit(ZeroObjects);
        }
        /// <summary>
        /// 系统关闭时调用
        /// </summary>
        internal static void OnZeroEnd()
        {
            SystemManager.HeartLeft();
            ApplicationState = StationState.Closing;
            ZeroTrace.WriteInfo("ZeroApplication", "OnZeroEnd....");
            Monitor.Enter(ZeroObjects);
            foreach (var obj in ZeroObjects)
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
            }
            Monitor.Exit(ZeroObjects);
            ApplicationState = StationState.Closed;
            ZeroTrace.WriteInfo("ZeroApplication", "OnZeroEnd");
        }

        /// <summary>
        /// 注销时调用
        /// </summary>
        internal static void OnZeroDistory()
        {
            ZeroTrace.WriteInfo("ZeroApplication", "OnZeroDistory....");
            if (!Monitor.TryEnter(ZeroObjects))
                return;
            IZeroObject[] array;
            Monitor.Enter(ZeroObjects);
            try
            {
                array = ZeroObjects.ToArray();
                ZeroObjects.Clear();
            }
            finally
            {
                Monitor.Exit(ZeroObjects);
            }
            foreach (var obj in array)
            {
                try
                {
                    ZeroTrace.WriteInfo(obj.Name, "*Distory");
                    obj.OnZeroDistory();
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    ZeroTrace.WriteError(obj.Name, "*Distory", e);
                }
            }

            foreach (var obj in array)
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
            }

            ZeroTrace.WriteInfo("ZeroApplication", "OnZeroDistory");

        }
        #endregion

        #region Console

        /// <summary>
        /// 命令行方式管理
        /// </summary>
        public static void CommandConsole()
        {
            while (true)
            {
                var cmd = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(cmd))
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
                {
                    ZeroTrace.WriteInfo("Console", result.TryGetValue(ZeroFrameType.TextValue, out var value)
                        ? value
                        : result.State.Text());
                }
                else
                {
                    ZeroTrace.WriteError("Console", result.TryGetValue(ZeroFrameType.TextValue, out var value)
                        ? value
                        : result.State.Text());
                }
            }
        }

        #endregion


        #region Flow


        #region Initialize
        /// <summary>
        /// 当前应用名称
        /// </summary>
        public static string AppName { get; set; }

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize()
        {
            if (ApplicationState != StationState.None)
                return;
            ApplicationState = StationState.Initialized;
            ZeroTrace.Initialize();
            ZeroTrace.WriteInfo("ZeroApplication", $"weconme {AppName ?? "zero net"}");
            ZeroTrace.WriteInfo("ZeroApplication", zmq.LibraryVersion);
            Console.CursorVisible = false;
            //引发静态构造
            ZeroTrace.WriteInfo("ZeroApplication", "Initialize...");
            InitializeConfig();
            ApiContext.SetLogRecorderDependency();
            LogRecorder.Initialize();
            ZContext.Initialize();
            OnZeroInitialize();
        }


        private static void InitializeConfig()
        {
            var root = ApiContext.Configuration == null
                ? Environment.CurrentDirectory
                : ApiContext.Configuration["contentRoot"];
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ZeroTrace.WriteInfo("ZeroApplication", "Linux");
                root = Path.GetDirectoryName(root);
                IOHelper.CheckPath(root, "config");
                IOHelper.CheckPath(root, "ipc");
            }
            else
            {
                ZeroTrace.WriteInfo("ZeroApplication", "Windows");
            }
            if (ApiContext.Configuration == null)
            {
                ApiContext.SetConfiguration(new ConfigurationBuilder().AddJsonFile("host.json").Build());
                // ReSharper disable once PossibleNullReferenceException
                ApiContext.Configuration["contentRoot"] = Environment.CurrentDirectory;
            }

            bool isnew = false;
            var file = Path.Combine(ApiContext.Configuration["contentRoot"], "host.json");
            if (Config == null)
            {
                if (File.Exists(file))
                {
                    Config = JsonConvert.DeserializeObject<ZeroAppConfig>(File.ReadAllText(file)) ?? new ZeroAppConfig();
                }
                else
                {
                    Config = new ZeroAppConfig();
                    isnew = true;
                }
            }

            ZeroAppConfig globalConfig;
            // ReSharper disable AssignNullToNotNullAttribute
            var global = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? Path.Combine(root, "config", "zero.json")
                : Path.Combine(root, "zero.json");
            // ReSharper enalbe AssignNullToNotNullAttribute
            if (File.Exists(global))
            {
                globalConfig = JsonConvert.DeserializeObject<ZeroAppConfig>(File.ReadAllText(global)) ?? new ZeroAppConfig();
            }
            else
            {
                globalConfig = new ZeroAppConfig();
            }
            if (globalConfig.LogFolder == null)
                globalConfig.LogFolder = IOHelper.CheckPath(root, "logs");
            TxtRecorder.LogPath = globalConfig.LogFolder;
            if (globalConfig.DataFolder == null)
                globalConfig.DataFolder = IOHelper.CheckPath(root, "datas");
            if (globalConfig.ServiceName == null)
                globalConfig.ServiceName = Dns.GetHostName();
            if (globalConfig.ZeroManagePort == 0)
                globalConfig.ZeroManagePort = 8000;
            if (globalConfig.ZeroMonitorPort == 0)
                globalConfig.ZeroMonitorPort = 8001;

            if (globalConfig.SpeedLimitModel < SpeedLimitType.Single)
                globalConfig.SpeedLimitModel = SpeedLimitType.Single;
            else if (globalConfig.SpeedLimitModel > SpeedLimitType.WaitCount)
                globalConfig.SpeedLimitModel = SpeedLimitType.ThreadCount;

            if (globalConfig.TaskCpuMultiple < 1)
                globalConfig.TaskCpuMultiple = 1;
            else if (globalConfig.TaskCpuMultiple > 64)
                globalConfig.TaskCpuMultiple = 64;
            string model;
            switch (globalConfig.SpeedLimitModel)
            {
                default:
                case SpeedLimitType.Single:
                    globalConfig.MaxWait = 0;
                    model = $"单线程:线程(1) 等待(0)";
                    break;
                case SpeedLimitType.ThreadCount:
                    if (globalConfig.MaxWait < 0xFF)
                        globalConfig.MaxWait = 0xFF;
                    else if (globalConfig.MaxWait > 0xFFFFF)
                        globalConfig.MaxWait = 0xFFFFF;
                    int max = Environment.ProcessorCount * globalConfig.TaskCpuMultiple;
                    model = $"按线程数限制:线程({Environment.ProcessorCount}×{globalConfig.TaskCpuMultiple}={max}) 等待({globalConfig.MaxWait})";
                    break;
                case SpeedLimitType.WaitCount:
                    if (globalConfig.MaxWait < 0xFF)
                        globalConfig.MaxWait = 0xFF;
                    else if (globalConfig.MaxWait > 0xFFFFF)
                        globalConfig.MaxWait = 0xFFFFF;
                    model = $"按等待数限制:线程(1) 等待({globalConfig.MaxWait})";
                    break;
            }

            ZeroTrace.WriteInfo("SpeedLimitModel", model);
            
            Config.ServiceName = globalConfig.ServiceName;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || String.IsNullOrWhiteSpace(Config.ZeroAddress))
            {
                Config.ZeroAddress = globalConfig.ZeroAddress;
                Config.ZeroManagePort = globalConfig.ZeroManagePort;
                Config.ZeroMonitorPort = globalConfig.ZeroMonitorPort;
            }

            Config.LocalIpAddress = GetHostIps();
            Config.DataFolder = IOHelper.CheckPath(globalConfig.DataFolder, Config.StationName);
            if (Config.ShortName == null)
                Config.ShortName = Config.StationName;
            if (Config.ServiceKey == null)
                Config.ServiceKey = RandomOperate.Generate(4);
            Config.RealName = ZeroIdentityHelper.CreateRealName(false);
            Config.Identity = Config.RealName.ToAsciiBytes();

            Config.SpeedLimitModel = globalConfig.SpeedLimitModel;
            Config.MaxWait = globalConfig.MaxWait;
            Config.TaskCpuMultiple = globalConfig.TaskCpuMultiple;

            ApiContext.MyRealName = Config.RealName;
            ApiContext.MyServiceKey = Config.ServiceKey;
            ApiContext.MyServiceName = Config.ServiceName;


            if (isnew)
                File.WriteAllText(file, JsonConvert.SerializeObject(Config, Formatting.Indented));
            Config.ZeroManageAddress = ZeroIdentityHelper.GetRequestAddress("SystemManage", Config.ZeroManagePort);
            Config.ZeroMonitorAddress = ZeroIdentityHelper.GetWorkerAddress("SystemMonitor", Config.ZeroMonitorPort);
        }

        static string GetHostIps()
        {
            StringBuilder ips = new StringBuilder();
            bool first = true;
            foreach (var address in Dns.GetHostAddresses(Config.ServiceName))
            {
                if (address.IsIPv4MappedToIPv6 || address.IsIPv6LinkLocal || address.IsIPv6Multicast ||
                    address.IsIPv6SiteLocal || address.IsIPv6Teredo)
                    continue;
                string ip = address.ToString();
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
        /// 发现
        /// </summary>
        public static void Discove(Assembly assembly = null)
        {
            var discover = new ZeroDiscover { Assembly = assembly ?? Assembly.GetCallingAssembly() };
            discover.FindApies();
            discover.FindZeroObjects();
        }
        #endregion

        #region Run

        /// <summary>
        ///     启动
        /// </summary>
        private static Task Start()
        {
            ZeroTrace.WriteInfo("ZeroApplication", Config.ZeroManageAddress);
            ApplicationState = StationState.Start;
            var task = Task.Factory.StartNew(SystemMonitor.Monitor);
            JoinCenter();
            return task;
        }

        /// <summary>
        ///     运行
        /// </summary>
        public static void Run()
        {
            Start();
        }
        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static bool JoinCenter()
        {
            ZeroTrace.WriteInfo("ZeroApplication", $"try connect zero center ({Config.ZeroManageAddress})...");
            if (!SystemManager.PingCenter())
            {
                ApplicationState = StationState.Failed;
                ZerCenterStatus = ZeroCenterState.Failed;
                ZeroTrace.WriteError("ZeroApplication", "zero center can`t connection.");
                return false;
            }
            ApplicationState = StationState.Run;
            ZerCenterStatus = ZeroCenterState.Run;
            if (!SystemManager.HeartJoin())
            {
                ApplicationState = StationState.Failed;
                ZerCenterStatus = ZeroCenterState.Failed;
                ZeroTrace.WriteError("ZeroApplication", "zero center can`t connection.");
                return false;
            }
            if (!SystemManager.LoadAllConfig())
            {
                ApplicationState = StationState.Failed;
                ZerCenterStatus = ZeroCenterState.Failed;
                ZeroTrace.WriteError("ZeroApplication", "configs can`t load.");
                return false;
            }
            ZeroTrace.WriteInfo("ZeroApplication", "be connected successfully");
            OnZeroStart();
            return true;
        }

        /// <summary>
        ///     执行直到连接成功
        /// </summary>
        public static Task RunBySuccess()
        {
            ZeroTrace.WriteLine("Application started. Press Ctrl+C to shut down.");
            Console.CancelKeyPress += OnCancelKeyPress;
            ApplicationState = StationState.Start;
            while (!JoinCenter())
                Thread.Sleep(1000);
            return Task.Factory.StartNew(SystemMonitor.Monitor);
        }

        /// <summary>
        ///     执行并等待
        /// </summary>
        public static void RunAwaite()
        {
            ZeroTrace.WriteLine("Application started. Press Ctrl+C to shut down.");
            Console.CancelKeyPress += OnCancelKeyPress;
            var task = Start();
            task.GetAwaiter().GetResult();
        }
        #endregion

        #region Destroy

        /// <summary>
        ///     关闭
        /// </summary>
        public static void Shutdown()
        {
            SystemManager.HeartLeft();
            switch (ApplicationState)
            {
                case StationState.Destroy:
                    return;
                case StationState.Run:
                    OnZeroEnd();
                    break;
            }

            ApplicationState = StationState.Closed;
            OnZeroDistory();
            ZeroTrace.WriteInfo("ZeroApplication", "Destroy");
            LogRecorder.Shutdown();
            ZContext.Destroy();
            ApplicationState = StationState.Destroy;
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Shutdown();
        }
        #endregion

        #endregion
    }
}