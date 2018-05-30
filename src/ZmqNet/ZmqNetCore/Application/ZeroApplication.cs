using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common;
using Agebull.Common.Logging;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     站点应用
    /// </summary>
    public class ZeroApplication
    {
        #region Config

        static string _zeroMonitorAddress;

        /// <summary>
        ///     监测中心广播地址
        /// </summary>
        public static string ZeroMonitorAddress => _zeroMonitorAddress;

        static string _zeroManageAddress;

        /// <summary>
        ///     监测中心管理地址
        /// </summary>
        public static string ZeroManageAddress => _zeroManageAddress ;

        /// <summary>
        /// 本机IP地址
        /// </summary>
        public static string LocalIpAddress { get; private set; }

        /// <summary>
        ///     站点集合
        /// </summary>
        public static readonly Dictionary<string, StationConfig> Configs = new Dictionary<string, StationConfig>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     站点配置
        /// </summary>
        public static LocalStationConfig Config { get; set; }

        /// <summary>
        ///     读取配置(不从服务器拉取)
        /// </summary>
        /// <returns></returns>
        public static StationConfig GetLocalConfig(string stationName)
        {
            if (ApplicationState != StationState.Run)
            {
                return null;
            }

            lock (Configs)
            {
                if (Configs.TryGetValue(stationName, out var config))
                {
                    return config;
                }
            }
            return null;
        }
        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public static StationConfig GetConfig(string stationName, out ZeroCommandStatus status)
        {
            if (ApplicationState != StationState.Run)
            {
                status = ZeroCommandStatus.NoRun;
                return null;
            }
            StationConfig config;
            lock (Configs)
            {
                if (Configs.TryGetValue(stationName, out config))
                {
                    status = ZeroCommandStatus.Success;
                    return config;
                }
            }
            config = SystemManager.GetConfig(stationName, out status);
            if (config == null)
                return null;
            lock (Configs)
            {
                Configs.Add(stationName, config);
            }
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
        private static int  _appState;

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
        public static void RegistZeroObject<TZeroObject>() where TZeroObject : class, IZeroObject, new()
        {
            if (ZeroObjects.Any(p => p is TZeroObject))
                return;
            var zero = new TZeroObject();
            ZeroObjects.Add(zero);
            if (ApplicationState >= StationState.Initialized)
                zero.OnZeroInitialize();
            if (CanDo)
                zero.OnZeroStart();
        }

        /// <summary>
        /// 系统启动时调用
        /// </summary>
        public static bool RegistZeroObject<TZeroObject>(TZeroObject zero) where TZeroObject : class, IZeroObject
        {
            if (ZeroObjects.Contains(zero))
                return false;
            ZeroObjects.Add(zero);
            if (ApplicationState >= StationState.Initialized)
                zero.OnZeroInitialize();
            if (CanDo)
                zero.OnZeroStart();
            return true;
        }

        /// <summary>
        /// 系统启动时调用
        /// </summary>
        internal static void OnZeroInitialize()
        {
            RegistZeroObject(ZeroPublisher.Instance);
            RegistZeroObject(ZeroConnectionPool.Instance);
            foreach (var obj in ZeroObjects)
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
        }

        /// <summary>
        /// 系统启动时调用
        /// </summary>
        internal static void OnZeroStart()
        {
            SystemManager.HeartJoin();
            foreach (var obj in ZeroObjects)
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
            SystemMonitor.RaiseEvent(Config, "program_run");
        }

        /// <summary>
        /// 系统关闭时调用
        /// </summary>
        internal static void OnZeroEnd()
        {
            foreach (var obj in ZeroObjects)
            {
                try
                {
                    ZeroTrace.WriteInfo(obj.Name, "Close");
                    obj.OnZeroEnd();
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    ZeroTrace.WriteError(obj.Name, "Close", e);
                }
            }
        }

        /// <summary>
        /// 注销时调用
        /// </summary>
        internal static void OnZeroDistory()
        {
            foreach (var obj in ZeroObjects)
            {
                try
                {
                    ZeroTrace.WriteInfo(obj.Name, "Distory");
                    obj.OnZeroDistory();
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    ZeroTrace.WriteError(obj.Name, "OnZeroDistory", e);
                }
            }

            foreach (var obj in ZeroObjects)
            {
                try
                {
                    ZeroTrace.WriteInfo(obj.Name, "Dispose");
                    obj.Dispose();
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    ZeroTrace.WriteError(obj.Name, "Dispose", e);
                }
            }
            ZeroObjects.Clear();
        }

        /// <summary>
        /// 系统启动时调用
        /// </summary>
        internal static void OnStationStateChanged(StationConfig config)
        {
            foreach (var obj in ZeroObjects)
            {
                try
                {
                    obj.OnZeroDistory();
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    ZeroTrace.WriteError(obj.Name, "OnStationStateChanged", e);
                }
            }
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
            ZeroTrace.Initialize();
            ZeroTrace.WriteInfo("ZeroApplication", $"weconme {AppName ?? "zero net"}");
            Console.CursorVisible = false;
            //引发静态构造
            ZeroTrace.WriteInfo("ZeroApplication", "Initialize...");
            ZContext.Initialize();
            LogRecorder.Initialize();
            ApiContext.SetLogRecorderDependency();
            InitializeConfig();
            OnZeroInitialize();
            ApplicationState = StationState.Initialized;
        }


        private static void InitializeConfig()
        {

            if (ApiContext.Configuration == null)
            {
                ApiContext.SetConfiguration(new ConfigurationBuilder().AddJsonFile("host.json").Build());
                ApiContext.Configuration["contentRoot"] = Environment.CurrentDirectory;

            }

            bool isnew = false;
            var file = Path.Combine(ApiContext.Configuration["contentRoot"], "host.json");
            if (Config == null)
            {
                isnew = true;
                if (File.Exists(file))
                {
                    Config = JsonConvert.DeserializeObject<LocalStationConfig>(File.ReadAllText(file)) ?? new LocalStationConfig();
                }
                else
                {
                    Config = new LocalStationConfig();
                }
            }
            Config.DataFolder = String.IsNullOrWhiteSpace(Config.DataFolder)
                ? IOHelper.CheckPath(ApiContext.Configuration["contentRoot"], "datas")
                : IOHelper.CheckPath(Config.DataFolder);

            Config.ServiceName = Dns.GetHostName();

            ApiContext.MyRealName = Config.RealName;
            ApiContext.MyServiceKey = Config.ServiceKey;
            ApiContext.MyServiceName = Config.ServiceName;

            StringBuilder ips = new StringBuilder();
            bool first = true;
            foreach (var address in Dns.GetHostAddresses(Config.ServiceName))
            {
                if (address.IsIPv4MappedToIPv6 || address.IsIPv6LinkLocal || address.IsIPv6Multicast ||
                    address.IsIPv6SiteLocal || address.IsIPv6Teredo)
                    continue;
                string ip = address.ToString();
                if (ip == "127.0.0.1" || ip == "::1" || ip == "-1")
                    continue;
                if (first)
                    first = false;
                else
                    ips.Append(" , ");
                ips.Append(ip);
            }
            LocalIpAddress = ips.ToString();

            if (isnew)
                File.WriteAllText(file, JsonConvert.SerializeObject(Config));
            if (String.IsNullOrWhiteSpace(Config.ZeroAddress))
                Config.ZeroAddress = "127.0.0.1";

            _zeroManageAddress = ZeroIdentityHelper.GetRequestAddress("SystemManage", Config.ZeroManagePort);
            _zeroMonitorAddress = ZeroIdentityHelper.GetWorkerAddress("SystemMonitor", Config.ZeroMonitorPort);
        }
        /// <summary>
        /// 发现
        /// </summary>
        public static void Discove(Assembly assembly = null)
        {
            var discover = new ZeroApiDiscover { Assembly = assembly ?? Assembly.GetCallingAssembly() };
            discover.FindApies();
            discover.FindSubs();
        }
        #endregion

        #region Run

        /// <summary>
        ///     启动
        /// </summary>
        private static Task Start()
        {
            ZeroTrace.WriteInfo("ZeroApplication", ZeroManageAddress);
            ApplicationState = StationState.Start;
            Task task = Task.Factory.StartNew(SystemMonitor.Monitor);
            JoinCenter();
            return task;
        }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static bool JoinCenter()
        {
            ZeroTrace.WriteInfo("ZeroApplication", $"try connect zero center ({ZeroManageAddress})...");
            if (!SystemManager.PingCenter() || !SystemManager.HeartJoin())
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
            ApplicationState = StationState.Run;
            ZerCenterStatus = ZeroCenterState.Run;
            OnZeroStart();
            return true;
        }

        /// <summary>
        ///     执行(无挂起操作)
        /// </summary>
        public static void Launch()
        {
            Initialize();
            Start();
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
            ZeroTrace.WriteInfo("ZeroApplication", "Closing...");
            ApplicationState = StationState.Closing;
            OnZeroEnd();
            ApplicationState = StationState.Closed;
            ZeroTrace.WriteInfo("ZeroApplication", "Closed");

            ApplicationState = StationState.Destroy;
            OnZeroDistory();
            LogRecorder.Shutdown();
            ZContext.Destroy();
            ZeroTrace.WriteInfo("ZeroApplication", "Destroy");
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Shutdown();
        }
        #endregion

        #endregion
    }
}