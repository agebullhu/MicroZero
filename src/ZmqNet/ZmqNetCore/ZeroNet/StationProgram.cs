using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace ZmqNet.Rpc.Core.ZeroNet
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
        public static readonly Dictionary<string, StationConfig> configs =
            new Dictionary<string, StationConfig>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     站点集合
        /// </summary>
        public static readonly Dictionary<string, ApiStation> stations =
            new Dictionary<string, ApiStation>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     站点配置
        /// </summary>
        public static readonly LocalStationConfig Config;

        /// <summary>
        ///     监测中心广播地址
        /// </summary>
        public static string ZeroMonitorAddress => $"tcp://{Config.ZeroAddress}:{Config.ZeroMonitorPort}";

        /// <summary>
        ///     监测中心管理地址
        /// </summary>
        public static string ZeroManageAddress => $"tcp://{Config.ZeroAddress}:{Config.ZeroManagePort}";

        /// <summary>
        ///     静态构造
        /// </summary>
        static StationProgram()
        {
            var path = Path.GetDirectoryName(typeof(StationProgram).Assembly.Location);
            var file = Path.Combine(path, "host.json");
            var json = File.ReadAllText(file);
            Config = JsonConvert.DeserializeObject<LocalStationConfig>(json);
        }

        /// <summary>
        /// </summary>
        /// <param name="station"></param>
        public static void RegisteApiStation(ApiStation station)
        {
            if (stations.ContainsKey(station.StationName))
            {
                stations[station.StationName].Close();
                stations[station.StationName] = station;
            }
            else
            {
                stations.Add(station.StationName, station);
            }
            if (State == StationState.Run)
                ApiStation.Run(station);
        }

        #endregion

        #region System Command

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="host"></param>
        /// <param name="requestId"></param>
        /// <param name="context"></param>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static string Call(string host, string requestId, string context, string commmand, string argument)
        {
            var config = GetConfig(host);
            if (config == null)
            {
                return "{\"Result\":false,\"Message\":\"UnknowHost\",\"ErrorCode\":404}";
            }
            string result = config.OutAddress.RequestNet(commmand, requestId, context, argument);
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
                    return "{\"Result\":false,\"Message\":\"参数错误\",\"ErrorCode\":-1}";
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
            if (configs.ContainsKey(stationName))
                return configs[stationName];
            lock (configs)
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
                    configs.Add(stationName, config);
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
            if (configs.ContainsKey(stationName))
                return configs[stationName];
            lock (configs)
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
                    configs.Add(stationName, config);
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
            foreach (var stat in stations)
                stat.Value.Close();
            while (stations.Values.Any(p => p.RunState == StationState.Run))
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
            foreach (var station in stations.Values)
                ApiStation.Run(station);
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
                    string result;
                    if (!subscriber.TryReceiveFrameString(timeout, out result))
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
            }
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
            if (configs.ContainsKey(station))
                configs[station] = cfg;
            else
                configs.Add(station, cfg);
        }

        private static void station_closing(string station, string content)
        {
            StationConfig cfg;
            if (configs.TryGetValue(station, out cfg))
                cfg.State = StationState.Closing;
            if (stations.ContainsKey(station))
            {
                WriteLine($"{station} is close");
                stations[station].Close();
            }
        }

        private static void station_resume(string station, string content)
        {
            StationConfig cfg;
            if (configs.TryGetValue(station, out cfg))
                cfg.State = StationState.Run;
            if (stations.ContainsKey(station))
            {
                WriteLine($"{station} is resume");
                ApiStation.Run(stations[station]);
            }
        }

        private static void station_pause(string station, string content)
        {
            StationConfig cfg;
            if (configs.TryGetValue(station, out cfg))
                cfg.State = StationState.Pause;
            if (stations.ContainsKey(station))
            {
                WriteLine($"{station} is pause");
                stations[station].Close();
            }
        }

        private static void station_left(string station)
        {
            StationConfig cfg;
            if (configs.TryGetValue(station, out cfg))
                cfg.State = StationState.Closed;
            if (stations.ContainsKey(station))
            {
                WriteLine($"{station} is left");
                stations[station].Close();
            }
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
            if (configs.ContainsKey(station))
                configs[station] = cfg;
            else
                configs.Add(station, cfg);
            if (stations.ContainsKey(station))
            {
                stations[station].Config = cfg;
                WriteLine($"{station} is join");
                ApiStation.Run(stations[station]);
            }
        }

        private static void system_stop(string content)
        {
            WriteLine(content);
            foreach (var sta in stations)
            {
                WriteLine($"Close {sta.Value.StationName}");
                sta.Value.Close();
            }
            configs.Clear();
        }

        private static void system_start(string content)
        {
            WriteLine(content);
            configs.Clear();

            foreach (var station in stations.Values)
            {
                WriteLine($"Restart {station.StationName}");
                ApiStation.Run(station);
            }
        }

        #endregion
    }
}