using System;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.PubSub;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    public class SystemMonitor
    {
        /// <summary>
        ///     进入系统侦听
        /// </summary>
        public static void RunMonitor()
        {
            var timeout = new TimeSpan(0, 0, 1);
            try
            {
                var subscriber = new SubscriberSocket();
                subscriber.Options.Identity = StationProgram.Config.StationName.ToAsciiBytes();
                subscriber.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                subscriber.Connect(StationProgram.ZeroMonitorAddress);
                subscriber.Subscribe("");

                StationProgram.WriteInfo("System Monitor Runing...");
                while (StationProgram.State == StationState.Start || StationProgram.State == StationState.Run)
                {
                    if (!subscriber.TryReceiveFrameString(timeout, out var title, out var more) || !more)
                    {
                        continue;
                    }
                    if (!subscriber.TryReceiveFrameBytes(out var description, out more) || !more)
                    {
                        continue;
                    }
                    PublishItem item = new PublishItem
                    {
                        Title = title
                    };
                    int idx = 1;
                    while (more)
                    {
                        if (!subscriber.TryReceiveFrameString(out var val, out more))
                        {
                            continue;
                        }
                        switch (description[idx++])
                        {
                            case ZeroFrameType.SubTitle:
                                item.SubTitle = val;
                                break;
                            case ZeroFrameType.Publisher:
                                item.Station = val;
                                break;
                            case ZeroFrameType.Argument:
                                item.Content = val;
                                break;
                        }
                    }

                    if (string.IsNullOrEmpty(title))
                        continue;
                    OnMessagePush(item.Title, item.Station, item.Content);
                }
            }
            catch (Exception e)
            {
                StationProgram.WriteError(e.Message);
                LogRecorder.Exception(e);
            }
            if (StationProgram.State == StationState.Run)
                Task.Factory.StartNew(RunMonitor);
        }

        /// <summary>
        ///     收到信息的处理
        /// </summary>
        public static void OnMessagePush(string cmd, string station, string content)
        {
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

        /// <summary>
        /// 站点心跳
        /// </summary>
        /// <param name="station"></param>
        /// <param name="content"></param>
        private static void station_heat(string station, string content)
        {
            if (String.IsNullOrEmpty(content))
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

            if (StationProgram.Configs.ContainsKey(station))
                StationProgram.Configs[station].Copy(cfg);
            else
                lock (StationProgram.Configs)
                    StationProgram.Configs.Add(station, cfg);

            StationEvent?.Invoke(cfg, new StationEventArgument("station_heat", cfg));
        }

        private static void station_install(string name, string content)
        {
            Console.WriteLine($"【station_install】{name}\r\n{content}");
            if (String.IsNullOrEmpty(content))
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
            if (StationProgram.Configs.ContainsKey(name))
                StationProgram.Configs[name].Copy(cfg);
            else lock (StationProgram.Configs)
                    StationProgram.Configs.Add(name, cfg);

            StationEvent?.Invoke(cfg, new StationEventArgument("station_install", cfg));
        }

        private static void station_closing(string name, string content)
        {
            if (!StationProgram.Configs.TryGetValue(name, out var cfg))
                return;
            Console.WriteLine($"【station_closing】{name}\r\n{content}");
            if (StationProgram.State == StationState.Run && StationProgram.Stations.TryGetValue(name, out var station))
            {
                station.Close();
            }
            StationEvent?.Invoke(cfg, new StationEventArgument("station_closing", cfg));
        }

        private static void station_resume(string name, string content)
        {
            if (!StationProgram.Configs.TryGetValue(name, out var cfg))
                return;
            Console.WriteLine($"【station_resume】{name}\r\n{content}");
            if (StationProgram.State == StationState.Run && StationProgram.Stations.TryGetValue(name, out var station) && station.RunState == StationState.Pause)
            {
                station.RunState = StationState.Run;
                StationProgram.WriteLine($"{name} is resume");
            }
            StationEvent?.Invoke(cfg, new StationEventArgument("station_resume", cfg));
        }

        private static void station_pause(string name, string content)
        {
            if (!StationProgram.Configs.TryGetValue(name, out var cfg))
                return;
            Console.WriteLine($"【station_pause】{name}\r\n{content}");
            if (StationProgram.State == StationState.Run && StationProgram.Stations.TryGetValue(name, out var station) && station.RunState == StationState.Run)
            {
                station.RunState = StationState.Pause;
                StationProgram.WriteLine($"{name} is pause");
            }
            StationEvent?.Invoke(cfg, new StationEventArgument("station_pause", cfg));
        }

        private static void station_left(string name)
        {
            Console.WriteLine($"【station_left】{name}");
            if (!StationProgram.Configs.TryGetValue(name, out var cfg))
                return;
            if (StationProgram.Stations.ContainsKey(name))
            {
                StationProgram.WriteLine($"{name} is left");
                StationProgram.Stations[name].Close();
            }

            StationEvent?.Invoke(cfg, new StationEventArgument("station_left", cfg));
        }

        private static void station_join(string name, string content)
        {
            Console.WriteLine($"【station_join】{name}\r\n{content}");
            if (String.IsNullOrEmpty(content))
                return;
            StationConfig cfg;
            try
            {
                cfg = JsonConvert.DeserializeObject<StationConfig>(content);
            }
            catch (Exception e)
            {
                StationProgram.WriteLine($"{name} error : {e.Message}");
                LogRecorder.Exception(e);
                return;
            }
            if (StationProgram.Configs.ContainsKey(name))
                StationProgram.Configs[name].Copy(cfg);
            else lock (StationProgram.Configs)
                    StationProgram.Configs.Add(name, cfg);
            if (!StationProgram.Stations.ContainsKey(name))
                return;
            StationProgram.Stations[name].Config= cfg;
            StationProgram.WriteLine($"{name} is join");
            StationEvent?.Invoke(cfg, new StationEventArgument("station_join", cfg));
            if (StationProgram.State != StationState.Run)
                return;
            var s = StationProgram.Stations[name];
            if (s.RunState != StationState.Run)
                ZeroStation.Run(s);
        }

        private static void system_stop(string content)
        {
            lock (StationProgram.Config)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"【system_stop】\r\n{content}");
                Console.ForegroundColor = ConsoleColor.Black;
            }
            StationEvent?.Invoke(null, new StationEventArgument("system_stop", null));
            foreach (var sta in StationProgram.Stations.Values)
            {
                StationProgram.WriteLine($"Close {sta.StationName}");
                sta.Close();
                StationEvent?.Invoke(sta, new StationEventArgument("system_stop", sta.Config));
            }
            StationProgram.ConfigsDispose();
        }

        private static void system_start(string content)
        {
            lock (StationProgram.Config)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"【system_start】\r\n{content}");
                Console.ForegroundColor = ConsoleColor.Black;
            }

            StationProgram.ConfigsDispose();
            StationEvent?.Invoke(null, new StationEventArgument("system_start", null));
            //foreach (var sta in StationProgram.Stations.Values)
            //{
            //    StationProgram.WriteLine($"Restart {sta.StationName}");
            //    ZeroStation.Run(sta);
            //    StationEvent?.Invoke(sta, new StationEventArgument("station_join", sta.Config));
            //}
        }
    }
}