using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.PubSub;
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
        private static ZeroMQ.ZSocket _subscriber;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        private static bool InitializeSocket()
        {
            try
            {
                _subscriber = ZeroHelper.CreateSubscriberSocket(ZeroApplication.ZeroMonitorAddress, ZeroApplication.Config.Identity,"");
                return true;
            }
            catch (Exception e)
            {
                StationConsole.WriteException("SystemMonitor", e);
                _subscriber = null;
                return false;
            }
        }
        /// <summary>
        ///     运行状态
        /// </summary>
        private static int _state;

        /// <summary>
        ///     运行状态
        /// </summary>
        public static int State
        {
            get => _state;
            set => Interlocked.Exchange(ref _state, value);
        }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static void Monitor()
        {
            State = StationState.Run;
            StationConsole.WriteInfo("ZeroApplication", "Monitor...");
            while (ZeroApplication.ApplicationState < StationState.Destroy)
            {
                if (_subscriber == null &&!InitializeSocket())
                {
                    Thread.Sleep(200);
                    continue;
                }
                if (!_subscriber.Subscribe(out var item,false) || string.IsNullOrEmpty(item.Title))
                    continue;
                OnMessagePush(item.Title, item.Station, item.Content);
            }
            Heartbeat(true);
            _subscriber.CloseSocket();
            _subscriber = null;
            State = StationState.Closed;
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
            if (station == null)
                return;
            //SystemManage对象重启时机
            switch (cmd)
            {
                case "system_start":
                case "station_join":
                case "station_state":
                    if (ZeroApplication.ApplicationState == StationState.Failed)
                    {
                        ZeroApplication.ApplicationState = StationState.Start;
                        ZeroApplication.JoinCenter();
                    }
                    break;
            }
            switch (cmd)
            {
                case "system_start":
                    system_start(content);
                    break;//SystemManage对象重启时机
                case "system_stop":
                    system_stop(content);
                    return;
                case "system_distory":
                    system_distory(content);
                    return;
                case "station_join":
                    station_join(station, content);
                    break;//SystemManage对象重启时机
                case "station_left":
                    station_left(station);
                    break;//SystemManage对象重启时机
                case "station_pause":
                    station_pause(station);
                    return;
                case "station_resume":
                    station_resume(station);
                    return;
                case "station_closing":
                    station_closing(station);
                    return;
                case "station_install":
                    station_install(station, content);
                    return;
                case "station_uninstall":
                    station_uninstall(station);
                    return;
                case "station_state":
                    station_state(station, content);
                    return;
                case "worker_sound_off":
                    if (ZeroApplication.ZerCenterStatus >= StationState.Closing)
                        return;
                    Heartbeat(false);
                    StationEvent?.Invoke(ZeroApplication.Config, new StationEventArgument("worker_sound_off", null));
                    return;
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

        static void InvokeEvent(object sender, string name, StationConfig config)
        {
            try
            {
                StationEvent?.Invoke(sender, new StationEventArgument(name, config));
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationConsole.WriteException(name, e);
            }
        }
        /// <summary>
        /// 站点心跳
        /// </summary>
        /// <param name="station"></param>
        /// <param name="content"></param>
        private static void station_state(string station, string content)
        {
            if (String.IsNullOrEmpty(station))
                return;
            StationConfig config;
            try
            {
                config = JsonConvert.DeserializeObject<StationConfig>(content);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationConsole.WriteError("station_state", content);
                return;
            }
            InvokeEvent(ZeroApplication.Config, "station_state", config);
        }

        private static void station_uninstall(string name)
        {
            StationConsole.WriteInfo("station_uninstall", name);
            if (String.IsNullOrEmpty(name))
                return;
            if (!ZeroApplication.Configs.TryGetValue(name, out var config))
                return;
            InvokeEvent(ZeroApplication.Config, "station_uninstall", config);
            config.Dispose();
            ZeroApplication.Configs.Remove(name);
        }

        private static void station_install(string name, string content)
        {
            StationConsole.WriteInfo("station_install", content);
            if (String.IsNullOrEmpty(content))
                return;
            StationConfig config;
            try
            {
                config = JsonConvert.DeserializeObject<StationConfig>(content);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationConsole.WriteError("station_install", content);
                return;
            }
            InvokeEvent(ZeroApplication.Config, "station_install", config);
            config.State = StationState.None;
            if (ZeroApplication.Configs.ContainsKey(name))
                ZeroApplication.Configs[name].Copy(config);
            else lock (ZeroApplication.Configs)
                    ZeroApplication.Configs.Add(name, config);

            InvokeEvent(config, "station_install", config);
        }

        private static void station_closing(string name)
        {
            StationConsole.WriteInfo("station_closing", name);
            if (!ZeroApplication.Configs.TryGetValue(name, out var config))
                return;
            InvokeEvent(config, "station_closing", config);
            if (ZeroApplication.ApplicationState == StationState.Run && ZeroApplication.Stations.TryGetValue(name, out var station))
            {
                station.Close();
            }
        }

        private static void station_resume(string name)
        {
            StationConsole.WriteInfo("station_resume", name);
            if (!ZeroApplication.Configs.TryGetValue(name, out var config))
                return;
            InvokeEvent(config, "station_resume", config);
            if (ZeroApplication.ApplicationState == StationState.Run &&
                ZeroApplication.Stations.TryGetValue(name, out var station) && station.State == StationState.Pause)
            {
                station.State = StationState.Run;
                StationConsole.WriteLine($"{name} is resume");
            }
        }

        private static void station_pause(string name)
        {
            StationConsole.WriteInfo("station_pause", name);
            if (!ZeroApplication.Configs.TryGetValue(name, out var config))
                return;
            InvokeEvent(config, "station_pause", config);
            if (ZeroApplication.ApplicationState == StationState.Run && ZeroApplication.Stations.TryGetValue(name, out var station) && station.State == StationState.Run)
            {
                station.State = StationState.Pause;
                StationConsole.WriteLine($"{name} is pause");
            }
        }

        private static void station_left(string name)
        {
            StationConsole.WriteInfo("station_left", name);
            if (!ZeroApplication.Configs.TryGetValue(name, out var config))
                return;
            InvokeEvent(config, "station_left", config);
            if (!ZeroApplication.Stations.ContainsKey(name))
                return;
            ZeroApplication.Stations[name].Close();
            StationConsole.WriteLine($"{name} is left");

        }

        private static void station_join(string name, string content)
        {
            StationConsole.WriteInfo("station_join", content);
            if (String.IsNullOrEmpty(content))
                return;
            StationConfig config;
            try
            {
                config = JsonConvert.DeserializeObject<StationConfig>(content);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationConsole.WriteError("station_join", content);
                return;
            }
            InvokeEvent(ZeroApplication.Config, "station_join", config);
            if (ZeroApplication.Configs.ContainsKey(name))
                ZeroApplication.Configs[name].Copy(config);
            else lock (ZeroApplication.Configs)
                    ZeroApplication.Configs.Add(name, config);
            if (!ZeroApplication.Stations.ContainsKey(name))
                return;
            ZeroApplication.Stations[name].Config = config;
            if (ZeroApplication.ApplicationState == StationState.Run)
            {
                var s = ZeroApplication.Stations[name];
                if (s.State != StationState.Run)
                    ZeroStation.Run(s);
            }
            InvokeEvent(config, "station_join", config);
        }
        private static void system_distory(string content)
        {
            StationConsole.WriteInfo("system_distory", content);
            ZeroApplication.ZerCenterStatus = StationState.Closed;
            InvokeEvent(null, "system_distory", null);
        }

        private static void system_stop(string content)
        {
            StationConsole.WriteInfo("system_stop", content);
            ZeroApplication.ZerCenterStatus = StationState.Closed;
            InvokeEvent(null, "system_stop", null);
            foreach (var sta in ZeroApplication.Stations.Values)
            {
                InvokeEvent(sta.Config, "station_left", sta.Config);
                sta.Close();
            }
            ZeroApplication.ConfigsDispose();
        }

        private static void system_start(string content)
        {
            StationConsole.WriteInfo("system_start", content);
            ZeroApplication.ZerCenterStatus = StationState.Run;
            ZeroApplication.ConfigsResume();
            InvokeEvent(null, "system_start", null);
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