using System;
using System.Threading;
using Agebull.Common.Logging;
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
                ZeroTrace.WriteException("SystemMonitor", e);
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
            ZeroTrace.WriteInfo("ZeroApplication", "zero center in monitor...");
            while (ZeroApplication.ApplicationState < StationState.Destroy)//ZeroApplication.IsAlive
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
                    return;
            }
            switch (cmd)
            {
                case "system_start":
                    system_start(content);
                    return;
                case "system_stop":
                    system_stop(content);
                    return;
                case "system_distory":
                    system_distory(content);
                    return;
                case "station_join":
                    station_join(station, content);
                    return;
                case "station_left":
                    station_left(station);
                    return;
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
                    if (!ZeroApplication.IsAlive)
                        return;
                    SystemManager.Heartbeat();
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
                ZeroTrace.WriteException(name, e);
            }
        }
        /// <summary>
        /// 站点心跳
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        private static void station_state(string name, string content)
        {
            if (String.IsNullOrEmpty(name))
                return;
            StationConfig config;
            try
            {
                config = JsonConvert.DeserializeObject<StationConfig>(content);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                ZeroTrace.WriteError("station_state", "Exception", name, content, e);
                return;
            }
            InvokeEvent(ZeroApplication.Config, "station_state", config);
        }

        private static void station_uninstall(string name)
        {
            ZeroTrace.WriteInfo("station_uninstall", name);
            if (String.IsNullOrEmpty(name))
                return;
            if (!ZeroApplication.Configs.TryGetValue(name, out var config))
                return;
            config.State = ZeroCenterState.Uninstall;
            ZeroApplication.OnStationStateChanged(config);
            ZeroApplication.Configs.Remove(name);
            InvokeEvent(ZeroApplication.Config, "station_uninstall", config);
        }

        private static void station_install(string name, string content)
        {
            ZeroTrace.WriteInfo("station_install", name, content);
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
                ZeroTrace.WriteError("station_install", "Exception", name, content, e);
                return;
            }
            InvokeEvent(ZeroApplication.Config, "station_install", config);
            config.State = StationState.None;
            if (ZeroApplication.Configs.ContainsKey(name))
                ZeroApplication.Configs[name].Copy(config);
            else lock (ZeroApplication.Configs)
                    ZeroApplication.Configs.Add(name, config);

            config.State = ZeroCenterState.None;
            ZeroApplication.OnStationStateChanged(config);
            InvokeEvent(config, "station_install", config);
        }

        private static void station_closing(string name)
        {
            ZeroTrace.WriteInfo("station_closing", name);
            if (!ZeroApplication.Configs.TryGetValue(name, out var config))
                return;
            config.State = ZeroCenterState.Closing;
            ZeroApplication.OnStationStateChanged(config);
            InvokeEvent(config, "station_closing", config);
        }

        private static void station_resume(string name)
        {
            ZeroTrace.WriteInfo("station_resume", name);
            if (!ZeroApplication.Configs.TryGetValue(name, out var config) || config.State != ZeroCenterState.Pause)
                return;
            config.State = ZeroCenterState.Run;
            ZeroApplication.OnStationStateChanged(config);
            InvokeEvent(config, "station_resume", config);
        }

        private static void station_pause(string name)
        {
            ZeroTrace.WriteInfo("station_pause", name);
            if (!ZeroApplication.Configs.TryGetValue(name, out var config))
                return;
            config.State = ZeroCenterState.Pause;
            ZeroApplication.OnStationStateChanged(config);
            InvokeEvent(config, "station_pause", config);
        }

        private static void station_left(string name)
        {
            ZeroTrace.WriteInfo("station_left", name);
            if (!ZeroApplication.Configs.TryGetValue(name, out var config))
                return;
            config.State = ZeroCenterState.Closed;
            ZeroApplication.OnStationStateChanged(config);
            InvokeEvent(config, "station_left", config);
        }

        private static void station_join(string name, string content)
        {
            ZeroTrace.WriteInfo("station_join", content);
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
                ZeroTrace.WriteError("station_join", "Exception", name, content, e);
                return;
            }
            InvokeEvent(ZeroApplication.Config, "station_join", config);
            if (ZeroApplication.Configs.ContainsKey(name))
                ZeroApplication.Configs[name].Copy(config);
            else lock (ZeroApplication.Configs)
                    ZeroApplication.Configs.Add(name, config);

            config.State = ZeroCenterState.Run;
            ZeroApplication.OnStationStateChanged(config);
            InvokeEvent(config, "station_join", config);
        }
        private static void system_distory(string content)
        {
            ZeroTrace.WriteInfo("system_distory", content);
            ZeroApplication.ZerCenterStatus = ZeroCenterState.Destroy;
            InvokeEvent(null, "system_distory", null);
        }

        private static void system_stop(string content)
        {
            ZeroTrace.WriteInfo("system_stop", content);
            ZeroApplication.ZerCenterStatus = ZeroCenterState.Closed;
            ZeroApplication.OnZeroEnd();
            InvokeEvent(null, "system_stop", null);
        }

        private static void system_start(string content)
        {
            ZeroTrace.WriteInfo("system_start", content);
            ZeroApplication.ZerCenterStatus = ZeroCenterState.Run;
            ZeroApplication.OnZeroStart();
            InvokeEvent(null, "system_start", null);
        }

        #endregion
    }
}