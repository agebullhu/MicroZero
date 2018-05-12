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
        #region IOC

        //private static IServiceProvider _serviceProvider;

        //static void InitIoc()
        //{
        //    _serviceProvider = new ServiceCollection().BuildServiceProvider();
        //}

        #endregion

        #region Station & Configs

        /// <summary>
        ///     站点集合
        /// </summary>
        public static readonly Dictionary<string, StationConfig> Configs = new Dictionary<string, StationConfig>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     站点集合
        /// </summary>
        internal static readonly Dictionary<string, ZeroStation> Stations = new Dictionary<string, ZeroStation>(StringComparer.OrdinalIgnoreCase);

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

        #region System Command
        /// <summary>
        /// 锁对象
        /// </summary>
        public static object lock_obj = new object();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLine(string message)
        {
            lock (lock_obj)
            {
                //Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteError(string message)
        {
            lock (lock_obj)
            {
                //Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteInfo(string message)
        {
            lock (lock_obj)
            {
                //Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        /// <summary>
        ///     执行管理命令
        /// </summary>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static bool Request(string commmand, string argument)
        {
            var result = ZeroManageAddress.RequestNet(commmand, argument);
            if (string.IsNullOrWhiteSpace(result))
            {
                WriteError($"【{commmand}】{argument}\r\n处理超时");
                return false;
            }
            WriteLine(result);
            return true;
        }
        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public static StationConfig GetConfig(string stationName, out ZeroCommandStatus status)
        {
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
                    WriteError($"【{stationName}】无法获取配置");
                    status = ZeroCommandStatus.Error;
                    return null;
                case ZeroNetStatus.ZeroCommandNoFind:
                    WriteError($"【{stationName}】未安装");
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
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public static StationConfig InstallStation(string stationName, string type)
        {
            StationConfig config;
            lock (Configs)
            {
                if (Configs.TryGetValue(stationName, out config))
                    return config;
            }

            WriteInfo($"【{stationName}】auto regist...");
            try
            {
                var result = ZeroManageAddress.RequestNet("install", type, stationName);

                switch (result)
                {
                    case null:
                        WriteError($"【{stationName}】auto regist failed");
                        return null;
                    case ZeroNetStatus.ZeroCommandNoSupport:
                        WriteError($"【{stationName}】auto regist failed:type no supper");
                        return null;
                    case ZeroNetStatus.ZeroCommandFailed:
                        WriteError($"【{stationName}】auto regist failed:config error");
                        return null;
                }
                config = JsonConvert.DeserializeObject<StationConfig>(result);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                WriteError($"【{stationName}】auto regist failed:{e.Message}");
                return null;
            }
            lock (Configs)
            {
                Configs.Add(stationName, config);
            }
            WriteError($"【{stationName}】auto regist succeed");
            return config;
        }

        #endregion

        #region Program Flow

        /// <summary>
        ///     状态
        /// </summary>
        public static StationState State { get; private set; }

        /// <summary>
        ///     运行
        /// </summary>
        public static void RunConsole()
        {
            Run();
            ConsoleInput();
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
                    case "stop":
                        Stop();
                        continue;
                    case "start":
                        Start();
                        continue;
                }
                var words = cmd.Split(' ', '\t', '\r', '\n');
                if (words.Length == 0)
                {
                    WriteLine("请输入正确命令");
                    continue;
                }
                Request(words[0], words.Length == 1 ? null : words[1]);
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Exit();
        }

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Initialize()
        {
            ZeroPublisher.Start();

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
        ///     停止
        /// </summary>
        public static void Start()
        {
            switch (State)
            {
                case StationState.Run:
                    WriteInfo("*run...");
                    return;
                case StationState.Closing:
                    WriteInfo("*closing...");
                    return;
                case StationState.Destroy:
                    WriteInfo("*destroy...");
                    return;
            }
            Run();
        }

        /// <summary>
        ///     停止
        /// </summary>
        public static void Stop()
        {
            WriteInfo("Program Stop.");
            State = StationState.Closing;
            foreach (var stat in Stations)
                stat.Value.Close();
            while (Stations.Values.Any(p => p.RunState == StationState.Run))
            {
                Console.Write(".");
                Thread.Sleep(100);
            }
            ZeroPublisher.Stop();
            State = StationState.Closed;
            WriteInfo("@");
        }

        /// <summary>
        ///     关闭
        /// </summary>
        public static void Close()
        {
            ConfigsDispose();
            WriteInfo("Program Stop...");
            if (State == StationState.Run)
                Stop();
            State = StationState.Destroy;
            WriteInfo("Program Exit");
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
        ///     关闭
        /// </summary>
        public static void Exit()
        {
            Close();
            Process.GetCurrentProcess().Close();
        }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        public static void Run()
        {
            WriteInfo("Program Start...");
            WriteLine(ZeroManageAddress);
            int tryCnt = 0;
            State = StationState.Start;
            try
            {
                while (true)
                {
                    var res = ZeroManageAddress.RequestNet("ping");
                    if (res != null)
                        break;
                    if (tryCnt > 12)
                    {
                        WriteError("ZeroCenter can`t connection,waiting for monitor message..");
                        return;
                    }
                    tryCnt++;
                    WriteError($"ZeroCenter can`t connection,try again in a second({tryCnt}).");
                    Thread.Sleep(1000);
                }
                Task.Factory.StartNew(SystemMonitor.RunMonitor);

                LoadAllConfig();

                Thread.Sleep(50);
                State = StationState.Run;
                if (Stations.Count > 0)
                {
                    foreach (var station in Stations.Values)
                        ZeroStation.Run(station);
                }
                WriteInfo("Program Run...");
            }
            catch (Exception e)
            {
                WriteError(e.Message);
                State = StationState.Failed;
            }
        }

        static void LoadAllConfig()
        {
            string result;
            int trycnt = 0;
            while (true)
            {
                result = ZeroManageAddress.RequestNet("Host", "*");
                if (result != null)
                {
                    break;
                }
                if (++trycnt > 5)
                    return;
                Thread.Sleep(10);
            }
            try
            {
                var configs = JsonConvert.DeserializeObject<List<StationConfig>>(result);
                foreach (var config in configs)
                {
                    lock (Configs)
                    {
                        if (Configs.ContainsKey(config.StationName))
                            Configs[config.StationName].Copy(config);
                        else
                            Configs.Add(config.StationName, config);
                    }
                }
            }
            catch (Exception e)
            {
                WriteError(e.Message);
            }

        }
        #endregion
    }
}