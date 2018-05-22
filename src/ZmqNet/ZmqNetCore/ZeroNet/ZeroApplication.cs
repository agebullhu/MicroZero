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
        public static string ZeroMonitorAddress => $"tcp://{Config.ZeroAddress}:{Config.ZeroMonitorPort}";//Config.GetRemoteAddress("SystemMonitor", Config.ZeroMonitorPort);

        /// <summary>
        ///     监测中心管理地址
        /// </summary>
        public static string ZeroManageAddress => Config.GetRemoteAddress("SystemManage", Config.ZeroManagePort);

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

            if (State == StationState.Run)
                ZeroStation.Run(station);
        }
        /// <summary>
        /// 注册站点
        /// </summary>
        public static void RegisteStation<TStation>() where TStation : ZeroStation, new()
        {
            RegisteStation(new TStation());
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
        public static StationConfig GetConfig(string stationName, out ZeroCommandStatus status)
        {
            if (State != StationState.Run)
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
            string result;
            try
            {
                result = ZeroManageAddress.RequestNet("host", stationName);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                status = ZeroCommandStatus.Exception;
                return null;
            }
            switch (result)
            {
                case null:
                    StationConsole.WriteError($"[{stationName}]无法获取配置");
                    status = ZeroCommandStatus.Error;
                    return null;
                case ZeroNetStatus.ZeroCommandNoFind:
                    //WriteError($"[{stationName}]未安装");
                    status = ZeroCommandStatus.NoFind;
                    return null;
            }
            try
            {
                config = JsonConvert.DeserializeObject<StationConfig>(result);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationConsole.WriteError($"{e.Message}\r\n{result}");
                status = ZeroCommandStatus.Error;
                return null;
            }

            lock (Configs)
            {
                Configs.Add(stationName, config);
            }
            status = ZeroCommandStatus.Success;
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
        ///     执行(无挂起操作)
        /// </summary>
        public static void Launch()
        {
            Initialize();
            Start();
        }

        /// <summary>
        ///     执行并等待
        /// </summary>
        public static void RunAwaite()
        {
            StationConsole.WriteLine(ZeroManageAddress);
            Console.CancelKeyPress += OnCancelKeyPress;
            State = StationState.Start;
            ZeroPublisher.Start();
            SystemManager.Run();
            StationConsole.WriteLine("Application started. Press Ctrl+C to shut down.");
            Task.Factory.StartNew(SystemMonitor.Run).GetAwaiter().GetResult();
            StationConsole.WriteLine("Application shut down.");
            while (State < StationState.Destroy)
            {
                Thread.Sleep(1000);
            }
        }
        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine();
            Destroy();
        }
        #endregion

        #region Program

        /// <summary>
        ///     状态
        /// </summary>
        public static StationState State { get; internal set; }


        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize()
        {
            LogRecorder.Initialize();
            ApiContext.SetLogRecorderDependency();
            InitializeConfig();
            ZeroPublisher.Initialize();
        }

        static void InitializeConfig()
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
            Config.DataFolder = string.IsNullOrWhiteSpace(Config.DataFolder)
                ? IOHelper.CheckPath(ApiContext.Configuration["contentRoot"], "datas")
                : IOHelper.CheckPath(Config.DataFolder);

            ApiContext.MyRealName = Config.RealName;
            ApiContext.MyServiceKey = Config.ServiceKey;
            ApiContext.MyServiceName = Config.ServiceName;

            Config.ServiceName = Dns.GetHostName();
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
            if (string.IsNullOrWhiteSpace(Config.ZeroAddress))
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

        /// <summary>
        ///     启动
        /// </summary>
        public static void Start()
        {
            StationConsole.WriteLine(ZeroManageAddress);
            State = StationState.Start;
            ZeroPublisher.Start();
            Task.Factory.StartNew(SystemMonitor.Run2);
            SystemManager.Run();
        }

        /// <summary>
        ///     关闭
        /// </summary>
        internal static void Close()
        {
            if (State >= StationState.Closing)
                return;
            StationConsole.WriteInfo("Program Closing...");
            State = StationState.Closing;
            ConfigsDispose();
            ZeroPublisher.Stop();
            foreach (var stat in Stations)
                stat.Value.Close();
            //while (Stations.Values.Any(p => p.RunState == StationState.Run))
            //{
            //    Console.Write(".");
            //    Thread.Sleep(100);
            //}
            State = StationState.Closed;
            StationConsole.WriteInfo("Program Closed");
        }
        /// <summary>
        ///     关闭
        /// </summary>
        public static void Destroy()
        {
            Close();
            State = StationState.Destroy;
            StationConsole.WriteInfo("Program Destroy");
            LogRecorder.Shutdown();
        }

        #endregion

        #region Console


        /// <summary>
        ///     运行
        /// </summary>
        public static void RunConsole()
        {
            Launch();
            ConsoleInput();
            Destroy();
        }

        private static void ConsoleInput()
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

                SystemManager.Request(words[0], words.Length == 1 ? null : words[1]);
            }
        }


        #endregion
    }
}