using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     站点应用
    /// </summary>
    public static class StationProgram
    {
        #region Station

        /// <summary>
        ///     站点集合
        /// </summary>
        internal static readonly Dictionary<string, ZeroStation> Stations = new Dictionary<string, ZeroStation>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     监测中心广播地址
        /// </summary>
        public static string ZeroMonitorAddress => $"tcp://{Config.ZeroAddress}:{Config.ZeroMonitorPort}";

        /// <summary>
        ///     监测中心管理地址
        /// </summary>
        public static string ZeroManageAddress => $"tcp://{Config.ZeroAddress}:{Config.ZeroManagePort}";

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
        private static LocalStationConfig _config;

        /// <summary>
        ///     站点配置
        /// </summary>
        public static LocalStationConfig Config
        {
            get
            {
                if (_config != null)
                    return _config;
                if (!File.Exists("host.json"))
                    return _config = new LocalStationConfig();
                var json = File.ReadAllText("host.json");
                return _config = JsonConvert.DeserializeObject<LocalStationConfig>(json);
            }
            set => _config = value;
        }

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
                    StationConsole.WriteError($"【{stationName}】无法获取配置");
                    status = ZeroCommandStatus.Error;
                    return null;
                case ZeroNetStatus.ZeroCommandNoFind:
                    //WriteError($"【{stationName}】未安装");
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
        #endregion

        #region Program

        /// <summary>
        ///     状态
        /// </summary>
        public static StationState State { get; internal set; }


        /// <summary>
        ///     执行
        /// </summary>
        public static void Launch()
        {
            if (State >= StationState.Run)
                return;
            Initialize();
            Start();
        }

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize()
        {
            ZeroPublisher.Initialize();
            Discove();
        }
        /// <summary>
        /// 发现
        /// </summary>
        static void Discove()
        {
            var discover = new ZeroApiDiscover();
            discover.FindApies(Assembly.GetCallingAssembly());
            if (discover.ApiItems.Count == 0)
                return;
            var station = new ApiStation
            {
                StationName = Config.StationName
            };
            foreach (var action in discover.ApiItems)
            {
                var a = action.Value.HaseArgument
                    ? station.RegistAction(action.Key, action.Value.ArgumentAction, action.Value.AccessOption)
                    : station.RegistAction(action.Key, action.Value.Action, action.Value.AccessOption);
                a.ArgumenType = action.Value.ArgumenType;
            }
            RegisteStation(station);
        }
        /// <summary>
        ///     启动
        /// </summary>
        internal static void Start()
        {
            switch (State)
            {
                case StationState.Run:
                    StationConsole.WriteInfo("*run...");
                    return;
                case StationState.Closing:
                    StationConsole.WriteInfo("*closing...");
                    return;
                case StationState.Destroy:
                    StationConsole.WriteInfo("*destroy...");
                    return;
            }
            StationConsole.WriteLine(ZeroManageAddress);
            State = StationState.Start;
            ZeroPublisher.Start();
            Task.Factory.StartNew(SystemMonitor.Run);
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
        public static void Exit()
        {
            Close();
            State = StationState.Destroy;
            StationConsole.WriteInfo("Program Destroy");
            Process.GetCurrentProcess().Close();
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
            Exit();
        }

        private static void ConsoleInput()
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            while (true)
            {
                var cmd = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(cmd))
                    continue;
                switch (cmd.Trim().ToLower())
                {
                    case "quit":
                    case "exit":
                        Exit();
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

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Exit();
        }

        #endregion
    }
}