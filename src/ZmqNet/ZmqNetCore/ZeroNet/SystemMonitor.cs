using System;
using System.Threading;
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
    public static class SystemMonitor
    {
        #region 网络处理

        /// <summary>
        /// 连接对象
        /// </summary>
        private static SubscriberSocket _subscriber;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public static bool Initialize()
        {
            try
            {
                _subscriber = new SubscriberSocket();
                _subscriber.Options.Identity = ZeroApplication.Config.Identity;
                _subscriber.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 10);
                _subscriber.Options.ReconnectIntervalMax = new TimeSpan(0, 0, 0, 0, 500);
                _subscriber.Options.TcpKeepalive = true;
                _subscriber.Options.TcpKeepaliveIdle = new TimeSpan(0, 1, 0);
                _subscriber.Connect(ZeroApplication.ZeroMonitorAddress);
                _subscriber.Subscribe("");
                return true;
            }
            catch (Exception e)
            {
                StationConsole.WriteError($"构造系统侦听订阅时出错：\r\n{e}");
                return false;
            }
        }
        /// <summary>
        ///     状态
        /// </summary>
        public static StationState State { get; internal set; }

        /// <summary>
        ///     重启
        /// </summary>
        public static void ReStart(bool async)
        {
            if (ZeroApplication.State >= StationState.Closing)
                return;
            Thread.Sleep(1000);
            if (State == StationState.Failed)
            {
                Task.Factory.StartNew(SystemManager.Run);
            }
            if (async)
                Task.Factory.StartNew(Run2);
            else
                Run();
        }
        /// <summary>
        ///     进入系统侦听
        /// </summary>
        public static void Run2()
        {
            if (!Initialize())
            {
                ReStart(true);
                return;
            }
            State = StationState.Start;
            Monitor();
            if (ZeroApplication.State < StationState.Closing)
                ReStart(true);
        }
        /// <summary>
        ///     进入系统侦听
        /// </summary>
        public static void Run()
        {
            if (!Initialize())
            {
                ReStart(false);
                return;
            }
            State = StationState.Start;
            Monitor();
            if (ZeroApplication.State < StationState.Closing)
                ReStart(false);
        }
        /// <summary>
        ///     进入系统侦听
        /// </summary>
        private static void Monitor()
        {
            var timeout = new TimeSpan(0, 0, 1);

            StationConsole.WriteInfo($"System Monitor({ZeroApplication.ZeroMonitorAddress}) Runing...");

            Heartbeat(false);
            State = StationState.Run;
            while (ZeroApplication.State < StationState.Destroy)
            {
                PublishItem item;
                try
                {
                    if (!_subscriber.TryReceiveFrameString(timeout, out var title, out var more) || !more)
                    {
                        continue;
                    }
                    if (!_subscriber.TryReceiveFrameBytes(out var description, out more) || !more)
                    {
                        continue;
                    }
                    item = new PublishItem
                    {
                        Title = title
                    };
                    int idx = 2;
                    while (more)
                    {
                        if (!_subscriber.TryReceiveFrameString(out var val, out more))
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
                }
                catch (Exception e)
                {
                    StationConsole.WriteError(e.Message);
                    LogRecorder.Exception(e);
                    //退出,自动重启
                    State = StationState.Failed;
                    break;
                }

                if (string.IsNullOrEmpty(item.Title))
                    continue;
                OnMessagePush(item.Title, item.Station, item.Content);
            }
            if (State != StationState.Failed)
                State = StationState.Closed;
            Heartbeat(true);
            if (_subscriber == null)
                return;
            _subscriber.CloseSocket(ZeroApplication.ZeroMonitorAddress);
            _subscriber = null;
        }

        #endregion

        #region 事件处理
        /// <summary>
        /// 发出事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="name"></param>
        internal static void RaiseEvent(object sender, string name)
        {
            StationEvent?.Invoke(sender, new StationEventArgument(name, null));
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
                    return;
                case "system_stop":
                    system_stop();
                    return;
                case "system_distory":
                    system_distory(content);
                    return;
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
                case "station_uninstall":
                    station_uninstall(station);
                    break;
                case "station_state":
                    station_state(station, content);
                    break;
                case "worker_sound_off":
                    Heartbeat(false);
                    StationEvent?.Invoke(ZeroApplication.Config, new StationEventArgument("worker_sound_off", null));
                    break;
            }
            if (SystemManager.State != StationState.Run)
            {
                SystemManager.Run();
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
        private static void station_state(string station, string content)
        {
            if (String.IsNullOrEmpty(station))
                return;
            try
            {
                var cfg = JsonConvert.DeserializeObject<StationConfig>(content);
                StationEvent?.Invoke(ZeroApplication.Config, new StationEventArgument("station_state", cfg));
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationConsole.WriteError($"{e.Message}\r\n{content}");
            }
        }

        private static void station_uninstall(string name)
        {
            StationConsole.WriteInfo($"[station_uninstall]{name}");
            if (String.IsNullOrEmpty(name))
                return;
            if (!ZeroApplication.Configs.TryGetValue(name, out var cfg))
                return;
            StationEvent?.Invoke(cfg, new StationEventArgument("station_uninstall", cfg));
            cfg.Dispose();
            ZeroApplication.Configs.Remove(name);
        }

        private static void station_install(string name, string content)
        {
            StationConsole.WriteInfo($"[station_install]{name}\r\n{content}");
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
                StationConsole.WriteError($"{e.Message}\r\n{content}");
                return;
            }
            cfg.State = StationState.None;
            if (ZeroApplication.Configs.ContainsKey(name))
                ZeroApplication.Configs[name].Copy(cfg);
            else lock (ZeroApplication.Configs)
                    ZeroApplication.Configs.Add(name, cfg);

            StationEvent?.Invoke(cfg, new StationEventArgument("station_install", cfg));
        }

        private static void station_closing(string name, string content)
        {
            if (!ZeroApplication.Configs.TryGetValue(name, out var cfg))
                return;
            StationConsole.WriteInfo($"[station_closing]{name}\r\n{content}");
            StationEvent?.Invoke(cfg, new StationEventArgument("station_closing", cfg));
            if (ZeroApplication.State == StationState.Run && ZeroApplication.Stations.TryGetValue(name, out var station))
            {
                station.Close();
            }
        }

        private static void station_resume(string name, string content)
        {
            if (!ZeroApplication.Configs.TryGetValue(name, out var cfg))
                return;
            StationConsole.WriteInfo($"[station_resume]{name}\r\n{content}");
            StationEvent?.Invoke(cfg, new StationEventArgument("station_resume", cfg));
            if (ZeroApplication.State != StationState.Run ||
                !ZeroApplication.Stations.TryGetValue(name, out var station) ||
                station.RunState != StationState.Pause) return;
            station.RunState = StationState.Run;
            StationConsole.WriteLine($"{name} is resume");
        }

        private static void station_pause(string name, string content)
        {
            if (!ZeroApplication.Configs.TryGetValue(name, out var cfg))
                return;
            StationConsole.WriteInfo($"[station_pause]{name}\r\n{content}");
            StationEvent?.Invoke(cfg, new StationEventArgument("station_pause", cfg));
            if (ZeroApplication.State == StationState.Run && ZeroApplication.Stations.TryGetValue(name, out var station) && station.RunState == StationState.Run)
            {
                station.RunState = StationState.Pause;
                StationConsole.WriteLine($"{name} is pause");
            }
        }

        private static void station_left(string name)
        {
            StationConsole.WriteInfo($"[station_left]{name}");
            if (!ZeroApplication.Configs.TryGetValue(name, out var cfg))
                return;
            StationEvent?.Invoke(cfg, new StationEventArgument("station_left", cfg));
            if (!ZeroApplication.Stations.ContainsKey(name))
                return;
            ZeroApplication.Stations[name].Close();
            StationConsole.WriteLine($"{name} is left");

        }

        private static void station_join(string name, string content)
        {
            StationConsole.WriteLine($"[station_join]{name}\r\n{content}");
            if (String.IsNullOrEmpty(content))
                return;
            StationConfig cfg;
            try
            {
                cfg = JsonConvert.DeserializeObject<StationConfig>(content);
            }
            catch (Exception e)
            {
                StationConsole.WriteLine($"{name} error : {e.Message}\r\n{content}");
                LogRecorder.Exception(e);
                return;
            }
            if (ZeroApplication.Configs.ContainsKey(name))
                ZeroApplication.Configs[name].Copy(cfg);
            else lock (ZeroApplication.Configs)
                    ZeroApplication.Configs.Add(name, cfg);
            if (!ZeroApplication.Stations.ContainsKey(name))
                return;
            ZeroApplication.Stations[name].Config = cfg;
            if (ZeroApplication.State == StationState.Run)
            {
                var s = ZeroApplication.Stations[name];
                if (s.RunState != StationState.Run)
                    ZeroStation.Run(s);
            }
            StationEvent?.Invoke(cfg, new StationEventArgument("station_join", cfg));
        }
        private static void system_distory(string content)
        {
            SystemManager.State = StationState.Destroy;
            StationEvent?.Invoke(null, new StationEventArgument("system_distory", null));
            StationConsole.WriteInfo($"[system_distory]\r\n{content}");
        }

        private static void system_stop()
        {
            SystemManager.State = StationState.Closed;
            StationConsole.WriteInfo("[system_stop]");
            StationEvent?.Invoke(null, new StationEventArgument("system_stop", null));
            foreach (var sta in ZeroApplication.Stations.Values)
            {
                StationConsole.WriteLine($"Close {sta.StationName}");
                StationEvent?.Invoke(sta, new StationEventArgument("system_stop", sta.Config));
                sta.Close();
            }
            ZeroApplication.ConfigsDispose();
        }

        private static void system_start(string content)
        {
            StationConsole.WriteInfo($"[system_start]\r\n{content}");
            ZeroApplication.ConfigsResume();
            SystemManager.Run();
            StationEvent?.Invoke(null, new StationEventArgument("system_start", null));
        }

        /// <summary>
        /// 心跳
        /// </summary>
        private static void Heartbeat(bool left)
        {
            if (left)
                SystemManager.HeartLeft();
            else
                SystemManager.Heartbeat();
        }
        #endregion
    }
}