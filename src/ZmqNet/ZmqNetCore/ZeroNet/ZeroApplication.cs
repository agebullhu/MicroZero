using System;
using System.Collections.Generic;
using System.IO;
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
using ZeroMQ.lib;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     站点应用
    /// </summary>
    public static class ZeroApplication
    {
        #region Station

        /// <summary>
        ///     站点集合
        /// </summary>
        internal static readonly Dictionary<string, ZeroStation> Stations = new Dictionary<string, ZeroStation>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     监测中心广播地址
        /// </summary>
        public static string ZeroMonitorAddress => $"tcp://{Config.ZeroAddress}:{Config.ZeroMonitorPort}";//ZeroIdentityHelper.GetRemoteAddress("SystemMonitor", Config.ZeroMonitorPort);

        /// <summary>
        ///     监测中心管理地址
        /// </summary>
        public static string ZeroManageAddress => ZeroIdentityHelper.GetRemoteAddress("SystemManage", Config.ZeroManagePort);

        /// <summary>
        /// 注册站点
        /// </summary>
        /// <param name="station"></param>
        public static void RegisteStation(ZeroStation station)
        {
            if (Stations.ContainsKey(station.StationName))
            {
                Stations[station.StationName].Close();
                Stations[station.StationName] = station;
            }
            else
            {
                Stations.Add(station.StationName, station);
            }

            if (ApplicationState == StationState.Run)
                ZeroStation.Run(station);
        }


        #endregion

        #region Config

        /// <summary>
        ///     站点集合
        /// </summary>
        public static readonly Dictionary<string, StationConfig> Configs = new Dictionary<string, StationConfig>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     站点配置
        /// </summary>
        public static LocalStationConfig Config { get; set; }

        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public static StationConfig GetConfig(string stationName)
        {
            if (ApplicationState != StationState.Run)
            {
                return null;
            }
            StationConfig config;
            lock (Configs)
            {
                if (Configs.TryGetValue(stationName, out config))
                {
                    return config;
                }
            }
            config = SystemManager.GetConfig(stationName, out _);
            if (config == null)
                return null;
            lock (Configs)
            {
                Configs.Add(stationName, config);
            }
            return config;
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

        /// <summary>
        /// 析构配置
        /// </summary>
        internal static void ConfigsResume()
        {
            lock (Configs)
            {
                foreach (var config in Configs.Values)
                {
                    config.Resume();
                }
            }
        }

        /// <summary>
        /// 析构配置
        /// </summary>
        internal static void ConfigsDispose()
        {
            lock (Configs)
            {
                foreach (var config in Configs.Values)
                {
                    config.Dispose();
                }
            }
        }
        /// <summary>
        /// 本机IP地址
        /// </summary>
        public static string Address { get; private set; }
        #endregion

        #region Run

        /// <summary>
        ///     启动
        /// </summary>
        private static Task Start()
        {
            StationConsole.WriteInfo("ZeroApplication", ZeroManageAddress);
            ApplicationState = StationState.Start;
            ZeroPublisher.Start();
            Task task = Task.Factory.StartNew(SystemMonitor.Monitor);
            if (JoinCenter())
            {
                ZerCenterStatus = StationState.Run;
                RunStations();
                StationConsole.WriteInfo("ZeroApplication", "run...");
            }
            return task;
        }

        private static readonly object LockObj = new object();
        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static bool JoinCenter()
        {
            if (ApplicationState == StationState.Run)
                return true;
            ApplicationState = StationState.Start;
            StationConsole.WriteInfo("ZeroApplication", $"try connect({ZeroManageAddress})...");
            if (!SystemManager.PingCenter() || !SystemManager.HeartJoin())
            {
                ApplicationState = StationState.Failed;
                StationConsole.WriteError("ZeroApplication", "can`t connection.");
                return false;
            }

            if (!SystemManager.LoadAllConfig())
            {
                ApplicationState = StationState.Failed;
                StationConsole.WriteError("ZeroApplication", "configs can`t load.");
                return false;
            }
            SystemMonitor.RaiseEvent(Config, "program_run");
            StationConsole.WriteInfo("ZeroApplication", "be connected successfully");
            ApplicationState = StationState.Run;
            return true;
        }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static void RunStations()
        {
            ApplicationState = StationState.Run;
            foreach (var station in Stations.Values)
                ZeroStation.Run(station);
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
            Console.CancelKeyPress += OnCancelKeyPress;
            StationConsole.WriteInfo("ZeroApplication", ZeroManageAddress);
            ApplicationState = StationState.Start;
            ZeroPublisher.Start();
            while (!JoinCenter())
                Thread.Sleep(1000);

            Task task = Task.Factory.StartNew(SystemMonitor.Monitor);
            ZerCenterStatus = StationState.Run;
            RunStations();
            StationConsole.WriteInfo("ZeroApplication", "run...");
            return task;
        }

        /// <summary>
        ///     执行并等待
        /// </summary>
        public static void RunAwaite()
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            var task = Start();
            StationConsole.WriteLine("Application started. Press Ctrl+C to shut down.");
            task.GetAwaiter().GetResult();
            while (ApplicationState < StationState.Closed)
            {
                Thread.Sleep(1000);
            }
            StationConsole.WriteLine("Application shut down.");
        }
        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine();
            Destroy();
        }
        #endregion

        #region Initialize

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize()
        {
            StationConsole.WriteInfo("ZeroNet", "*************Hello World*************");
            Console.CursorVisible = false;
            //引发静态构造
            StationConsole.WriteInfo("ZeroApplication", "Initialize...");
            var context = ZContext.Current;
            LogRecorder.Initialize();
            ApiContext.SetLogRecorderDependency();
            InitializeConfig();
            ZeroPublisher.Initialize();
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
            Address = ips.ToString();

            if (isnew)
                File.WriteAllText(file, JsonConvert.SerializeObject(Config));
            if (String.IsNullOrWhiteSpace(Config.ZeroAddress))
                Config.ZeroAddress = "127.0.0.1";
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

        #region Program

        /// <summary>
        /// 运行状态（本地与服务器均正常）
        /// </summary>
        public static bool CanDo => ApplicationState == StationState.Run && ZerCenterStatus == StationState.Run;

        /// <summary>
        /// 运行状态（本地未关闭）
        /// </summary>
        public static bool IsAlive => ApplicationState < StationState.Closing;

        /// <summary>
        ///     运行状态
        /// </summary>
        private static int _zeroState,_appState;

        /// <summary>
        /// 服务器状态
        /// </summary>
        public static int ZerCenterStatus
        {
            get => _zeroState;
            internal set => Interlocked.Exchange(ref _zeroState, value);
        }

        /// <summary>
        ///     状态
        /// </summary>
        public static int ApplicationState
        {
            get => _appState;
            internal set => Interlocked.Exchange(ref _appState, value);
        }

        /// <summary>
        ///     关闭
        /// </summary>
        internal static void Close()
        {
            if (ApplicationState >= StationState.Closing)
                return;
            StationConsole.WriteInfo("ZeroApplication", "Closing...");
            ApplicationState = StationState.Closing;
            ZeroPublisher.Stop();
            foreach (var stat in Stations)
                stat.Value.Close();
            ConfigsDispose();
            ApplicationState = StationState.Closed;
            StationConsole.WriteInfo("ZeroApplication", "Closed");
        }
        /// <summary>
        ///     关闭
        /// </summary>
        public static void Destroy()
        {
            Close();
            ApplicationState = StationState.Destroy;
            ZContext.Current.Dispose();
            StationConsole.WriteInfo("ZeroApplication", "Destroy");
            LogRecorder.Shutdown();
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
                        Destroy();
                        return;
                    case "start":
                        Start();
                        continue;
                }
                var words = cmd.Split(' ', '\t', '\r', '\n');
                if (words.Length == 0)
                {
                    StationConsole.WriteLine("请输入正确命令");
                    continue;
                }

                var result = SystemManager.CallCommand(words);
                if (result.InteractiveSuccess)
                {
                    StationConsole.WriteInfo("Console",result.TryGetValue(ZeroFrameType.TextValue, out var value)
                        ? value
                        : result.State.Text());
                }
                else
                {
                    StationConsole.WriteError("Console", result.TryGetValue(ZeroFrameType.TextValue, out var value)
                        ? value
                        : result.State.Text());
                }
            }
        }


        #endregion
    }
}