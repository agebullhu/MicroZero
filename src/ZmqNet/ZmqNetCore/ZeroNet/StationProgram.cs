using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.ZeroApi;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     站点应用
    /// </summary>
    public static class StationProgram
    {

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
                if(!File.Exists("host.json"))
                    return _config =new LocalStationConfig();
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
        /// </summary>
        /// <param name="station"></param>
        public static void RegisteApiStation(ZeroStation station)
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

            station.Config = GetConfig(station.StationName);
            if (State == StationState.Run)
                ZeroStation.Run(station);
        }

        #endregion

        #region System Command

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="station"></param>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static string Call(string station, string commmand, string argument)
        {
            var config = GetConfig(station);
            if (config == null)
            {
                return "{\"Result\":false,\"Message\":\"UnknowHost\",\"ErrorCode\":404}";
            }
            var result = config.OutAddress.RequestNet(commmand, ApiContext.RequestContext.RequestId, JsonConvert.SerializeObject(ApiContext.Current), argument);
            if (string.IsNullOrEmpty(result))
                return "{\"Result\":false,\"Message\":\"UnknowHost\",\"ErrorCode\":500}";
            if (result[0] == '{')
                return result;
            switch (result)
            {
                case "Invalid":
                    return "{\"Result\":false,\"Message\":\"参数错误\",\"ErrorCode\":-2}";
                case "NoWork":
                    return "{\"Result\":false,\"Message\":\"服务器正忙\",\"ErrorCode\":503}";
                default:
                    return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
            Console.Write("$ > ");
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
                WriteLine("*处理超时");
                return false;
            }
            WriteLine(result);
            return true;
        }

        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public static StationConfig GetConfig(string stationName)
        {
            if (Configs.ContainsKey(stationName))
                return Configs[stationName];
            lock (Configs)
            {
                try
                {
                    var result = ZeroManageAddress.RequestNet("host", stationName);
                    if (result == null)
                    {
                        WriteLine("无法获取消息中心的配置");
                        return null;
                    }
                    var config = JsonConvert.DeserializeObject<StationConfig>(result);
                    Configs.Add(stationName, config);
                    return config;
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    return null;
                }
            }
        }

        /// <summary>
        ///     读取配置
        /// </summary>
        /// <returns></returns>
        public static StationConfig InstallApiStation(string stationName)
        {
            if (Configs.ContainsKey(stationName))
                return Configs[stationName];
            lock (Configs)
            {
                try
                {
                    var result = ZeroManageAddress.RequestNet("install_api", stationName);
                    if (result == null)
                    {
                        WriteLine("无法获取消息中心的配置");
                        return null;
                    }
                    var config = JsonConvert.DeserializeObject<StationConfig>(result);
                    Configs.Add(stationName, config);
                    return config;
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    return null;
                }
            }
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

        /// <summary>
        ///     停止
        /// </summary>
        public static void Start()
        {
            switch (State)
            {
                case StationState.Run:
                    WriteLine("*run...");
                    return;
                case StationState.Closing:
                    WriteLine("*closing...");
                    return;
                case StationState.Destroy:
                    WriteLine("*destroy...");
                    return;
            }
            Run();
        }

        /// <summary>
        ///     停止
        /// </summary>
        public static void Stop()
        {
            Console.Write("Program Stop.");
            State = StationState.Closing;
            foreach (var stat in Stations)
                stat.Value.Close();
            while (Stations.Values.Any(p => p.RunState == StationState.Run))
            {
                Console.Write(".");
                Thread.Sleep(100);
            }
            State = StationState.Closed;
            WriteLine("@");
        }

        /// <summary>
        ///     关闭
        /// </summary>
        public static void Exit()
        {
            if (State == StationState.Run)
                Stop();
            State = StationState.Destroy;
            WriteLine("Program Exit");
        }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        public static void Run()
        {
            State = StationState.Run;
            foreach (var station in Stations.Values)
                ZeroStation.Run(station);
            Task.Factory.StartNew(RunMonitor);
        }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        private static void RunMonitor()
        {
            var timeout = new TimeSpan(0, 0, 1);
            try
            {
                WriteLine("StationCache Runing...");
                var subscriber = new SubscriberSocket();
                subscriber.Options.Identity = Config.StationName.ToAsciiBytes();
                subscriber.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                subscriber.Connect(ZeroMonitorAddress);
                subscriber.Subscribe("");

                while (State == StationState.Run)
                {
                    if (!subscriber.TryReceiveFrameString(timeout, out var result))
                        continue;
                    OnMessagePush(result);
                }
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                LogRecorder.Exception(e);
            }
            if (State == StationState.Run)
                Task.Factory.StartNew(RunMonitor);
        }

        #endregion

        #region System Monitor

        /// <summary>
        ///     收到信息的处理
        /// </summary>
        /// <param name="msg"></param>
        public static void OnMessagePush(string msg)
        {
            if (string.IsNullOrEmpty(msg))
                return;
            var array = msg.Split(new[] { ' ' }, 3);
            var cmd = array[0];
            var station = array.Length > 1 ? array[1] : "*";
            var content = array.Length > 2 ? array[2] : "{}";
            switch (cmd)
            {
                case "system_start":
                    system_start(content);
                    break;
                case "system_stop":
                    system_stop(content);
                    break;
                case "station_join":
                    station_join(station, content);
                    break;
                case "station_left":
                    station_left(station);
                    break;
                case "station_pause":
                    station_pause(station, content);
                    break;
                case "station_resume":
                    station_resume(station, content);
                    break;
                case "station_closing":
                    station_closing(station, content);
                    break;
                case "station_install":
                    station_install(station, content);
                    break;
                case "worker_heat":
                    station_heat(station, content);
                    break;
            }
        }
        /// <summary>
        /// 站点事件参数
        /// </summary>
        public class StationEventArgument : EventArgs
        {
            /// <summary>
            /// 构造
            /// </summary>
            /// <param name="eventName"></param>
            /// <param name="config"></param>
            public StationEventArgument(string eventName, StationConfig config)
            {
                EventConfig = config;
                EventName = eventName;
            }
            /// <summary>
            /// 站点名称
            /// </summary>
            public string EventName { get; }
            /// <summary>
            /// 配置
            /// </summary>
            public StationConfig EventConfig { get; }
        }
        /// <summary>
        /// 站点事件发生
        /// </summary>
        public static event EventHandler<StationEventArgument> StationEvent;
        private static void station_heat(string station, string content)
        {
            if (string.IsNullOrEmpty(content))
                return;
            StationConfig cfg;
            try
            {
                cfg = JsonConvert.DeserializeObject<StationConfig>(content);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return;
            }
            cfg.State = StationState.None;
            if (Configs.ContainsKey(station))
                Configs[station] = cfg;
            else
                Configs.Add(station, cfg);

            StationEvent?.Invoke(cfg, new StationEventArgument("station_heat", cfg));
        }

        private static void station_install(string station, string content)
        {
            if (string.IsNullOrEmpty(content))
                return;
            StationConfig cfg;
            try
            {
                cfg = JsonConvert.DeserializeObject<StationConfig>(content);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return;
            }
            cfg.State = StationState.None;
            if (Configs.ContainsKey(station))
                Configs[station] = cfg;
            else
                Configs.Add(station, cfg);
            StationEvent?.Invoke(cfg, new StationEventArgument("station_install", cfg));
        }

        private static void station_closing(string station, string content)
        {
            if (Configs.TryGetValue(station, out var cfg))
                cfg.State = StationState.Closing;
            if (Stations.ContainsKey(station))
            {
                WriteLine($"{station} is close");
                Stations[station].Close();
            }
            StationEvent?.Invoke(cfg, new StationEventArgument("station_closing", cfg));
        }

        private static void station_resume(string station, string content)
        {
            if (Configs.TryGetValue(station, out var cfg))
                cfg.State = StationState.Run;
            if (Stations.ContainsKey(station))
            {
                WriteLine($"{station} is resume");
                ZeroStation.Run(Stations[station]);
            }
            StationEvent?.Invoke(cfg, new StationEventArgument("station_resume", cfg));
        }

        private static void station_pause(string station, string content)
        {
            if (Configs.TryGetValue(station, out var cfg))
                cfg.State = StationState.Pause;
            if (Stations.ContainsKey(station))
            {
                WriteLine($"{station} is pause");
                Stations[station].Close();
            }
            StationEvent?.Invoke(cfg, new StationEventArgument("station_pause", cfg));
        }

        private static void station_left(string station)
        {
            if (Configs.TryGetValue(station, out var cfg))
                cfg.State = StationState.Closed;
            if (Stations.ContainsKey(station))
            {
                WriteLine($"{station} is left");
                Stations[station].Close();
            }
            StationEvent?.Invoke(cfg, new StationEventArgument("station_left", cfg));
        }

        private static void station_join(string station, string content)
        {
            if (string.IsNullOrEmpty(content))
                return;
            StationConfig cfg;
            try
            {
                cfg = JsonConvert.DeserializeObject<StationConfig>(content);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return;
            }
            cfg.State = StationState.Run;
            if (Configs.ContainsKey(station))
                Configs[station] = cfg;
            else
                Configs.Add(station, cfg);
            if (Stations.ContainsKey(station))
            {
                Stations[station].Config = cfg;
                WriteLine($"{station} is join");
                StationEvent?.Invoke(cfg, new StationEventArgument("station_join", cfg));
                ZeroStation.Run(Stations[station]);
            }
        }

        private static void system_stop(string content)
        {
            StationEvent?.Invoke(null, new StationEventArgument("system_stop", null));
            WriteLine(content);
            foreach (var sta in Stations.Values)
            {
                WriteLine($"Close {sta.StationName}");
                sta.Close();
                StationEvent?.Invoke(sta, new StationEventArgument("station_closing", sta.Config));
            }
            Configs.Clear();
        }

        private static void system_start(string content)
        {
            WriteLine(content);
            Configs.Clear();

            StationEvent?.Invoke(null, new StationEventArgument("system_start", null));
            foreach (var sta in Stations.Values)
            {
                WriteLine($"Restart {sta.StationName}");
                ZeroStation.Run(sta);
                StationEvent?.Invoke(sta, new StationEventArgument("station_join", sta.Config));
            }
        }

        #endregion
    }
}