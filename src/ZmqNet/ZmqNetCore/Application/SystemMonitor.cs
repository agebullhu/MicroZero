using System;
using System.Threading;
using Agebull.Common.Logging;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    public static class SystemMonitor
    {
        #region 网络处理

        /// <summary>
        ///     运行状态
        /// </summary>
        private static int State
        {
            get; set;
        }
        static readonly SemaphoreSlim TaskEndSem = new SemaphoreSlim(0);

        /// <summary>
        /// 等待正常结束
        /// </summary>
        internal static void WaitSafeClose()
        {
            TaskEndSem.Wait();
        }
        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static void Monitor()
        {
            using (OnceScope.CreateScope(ZeroApplication.Config))
            {
                State = StationState.Failed;
                ZeroTrace.WriteInfo(ZeroApplication.AppName, "zero center in monitor...");
            }
            var sockets = new[] { ZeroHelper.CreateSubscriberSocket(ZeroApplication.Config.ZeroMonitorAddress) };
            var pollItems = new[] { ZPollItem.CreateReceiver() };
            int errorCount = 0;
            while (!ZeroApplication.IsClosed)
            {
                if (!sockets.PollIn(pollItems, out var messages, out var error, new TimeSpan(0, 0, 0, 0, 500)))
                {
                    errorCount++;
                    if (error != null && !Equals(error, ZError.EAGAIN))
                    {
                        if (Equals(error, ZError.ETERM))
                            break;
                        ZeroTrace.WriteError("SystemMonitor", error.Text, error.Name);
                    }
                    State = StationState.Failed;
                    if (errorCount > 10 && State == StationState.Run) //最少5秒与服务器失联
                    {
                        ReBoot(sockets);
                    }
                    continue;
                }

                if (messages == null || messages.Length == 0 || !messages[0].Unpack(out var item))
                    continue;
                State = StationState.Run;
                errorCount = 0;
                OnMessagePush(item.Title, item.Station, item.Content);
                if (item.Title == "system_stop")
                    ReBoot(sockets);
            }
            sockets[0].CloseSocket();
            State = StationState.Closed;
            TaskEndSem.Release();
        }

        private static void ReBoot(ZSocket[] sockets)
        {
            ZeroTrace.WriteInfo(ZeroApplication.AppName, "Reboot...");
            sockets[0].CloseSocket();
            ZeroApplication.OnZeroEnd();
            ZContext.Destroy();
            ZContext.Initialize();
            //重构路由
            sockets[0] = ZeroHelper.CreateSubscriberSocket(ZeroApplication.Config.ZeroMonitorAddress);
            ZeroApplication.JoinCenter();
            ZeroTrace.WriteInfo(ZeroApplication.AppName, "Reboot");
        }

        #endregion

        #region 事件处理

        /// <summary>
        ///     收到信息的处理
        /// </summary>
        private static void OnMessagePush(string cmd, string station, string content)
        {
            switch (cmd)
            {
                case "system_start":
                    system_start(content);
                    return;
                case "system_stop":
                    system_stop(content);
                    return;
                case "station_state":
                    station_state(station, content);
                    return;
            }
            if (ZeroApplication.ZerCenterStatus != ZeroCenterState.Run)
                return;
            switch (cmd)
            {
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
                case "worker_sound_off":
                    if (ZeroApplication.IsRun)
                        ZeroApplication.OnHeartbeat();
                    return;
            }
        }

        private static void station_uninstall(string name)
        {
            ZeroTrace.WriteInfo("station_uninstall", name);
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            config.State = ZeroCenterState.Uninstall;
            if (ZeroApplication.IsRun)
                ZeroApplication.OnStationStateChanged(config);
            ZeroApplication.Config[name] = null;
            InvokeEvent("station_uninstall", null, config);
        }

        private static void station_install(string name, string content)
        {
            ZeroTrace.WriteInfo("station_install", name, content);
            if (!ZeroApplication.Config.UpdateConfig(name, content, out var config))
                return;
            if (ZeroApplication.IsRun)
                ZeroApplication.OnStationStateChanged(config);
            InvokeEvent("station_install", content, config);
        }

        private static void station_closing(string name)
        {
            ZeroTrace.WriteInfo("station_closing", name);
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            config.State = ZeroCenterState.Closing;
            if (ZeroApplication.IsRun)
                ZeroApplication.OnStationStateChanged(config);
            InvokeEvent("station_closing", null, config);
        }

        private static void station_resume(string name)
        {
            ZeroTrace.WriteInfo("station_resume", name);
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            config.State = ZeroCenterState.Run;
            if (ZeroApplication.IsRun)
                ZeroApplication.OnStationStateChanged(config);
            InvokeEvent("station_resume", null, config);
        }

        private static void station_pause(string name)
        {
            ZeroTrace.WriteInfo("station_pause", name);
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            config.State = ZeroCenterState.Pause;
            if (ZeroApplication.IsRun)
                ZeroApplication.OnStationStateChanged(config);
            InvokeEvent("station_pause", null, config);
        }

        private static void station_left(string name)
        {
            ZeroTrace.WriteInfo("station_left", name);
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            config.State = ZeroCenterState.Closed;
            if (ZeroApplication.IsRun)
                ZeroApplication.OnStationStateChanged(config);
            InvokeEvent("station_left", null, config);
        }

        private static void station_join(string name, string content)
        {
            ZeroTrace.WriteInfo("station_join", content);
            if (!ZeroApplication.Config.UpdateConfig(name, content, out var config))
                return;
            config.State = ZeroCenterState.Run;
            if (ZeroApplication.IsRun)
                ZeroApplication.OnStationStateChanged(config);
            InvokeEvent("station_join", content, config);
        }


        private static void system_start(string content)
        {
            ZeroApplication.ZerCenterStatus = ZeroCenterState.Run;
            if (Interlocked.CompareExchange(ref ZeroApplication._appState, StationState.Initialized, StationState.Failed) == StationState.Failed)
            {
                ZeroTrace.WriteInfo("system_start", content);
                ZeroApplication.JoinCenter();
            }
        }

        /// <summary>
        /// 站点心跳
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        private static void station_state(string name, string content)
        {
            //SystemManage对象重启时机
            if (Interlocked.CompareExchange(ref ZeroApplication._appState, StationState.Start, StationState.Failed) == StationState.Failed)
            {
                ZeroApplication.ZerCenterStatus = ZeroCenterState.Run;
                ZeroApplication.JoinCenter();
                return;
            }

            if (!ZeroApplication.IsRun || !ZeroApplication.Config.TryGetConfig(name, out var old))
                return;
            try
            {
                var config = JsonConvert.DeserializeObject<StationConfig>(content);
                old.CheckValue(config);
                InvokeEvent("station_state", content, old);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                ZeroTrace.WriteError("station_state", "Exception", name, content, e);
            }
        }

        private static void system_stop(string content)
        {
            ZeroApplication.ZerCenterStatus = ZeroCenterState.Closed;
            if (Interlocked.CompareExchange(ref ZeroApplication._appState, StationState.Closing, StationState.Run) == StationState.Run)
            {
                ZeroTrace.WriteError("system_stop", content);
                ZeroApplication.OnZeroEnd();
                ZeroApplication.ApplicationState = StationState.Failed;
                RaiseEvent("system_stop");
            }
        }
        #endregion

        #region 对外事件


        /// <summary>
        /// 站点事件参数
        /// </summary>
        public class StationEventArgument : EventArgs
        {
            /// <summary>
            /// 构造
            /// </summary>
            /// <param name="eventName"></param>
            /// <param name="context"></param>
            /// <param name="config"></param>
            public StationEventArgument(string eventName, string context, StationConfig config)
            {
                EventConfig = config;
                EventName = eventName;
                Context = context;
            }
            /// <summary>
            /// 站点名称
            /// </summary>
            public string EventName { get; }
            /// <summary>
            /// 内容
            /// </summary>
            public string Context { get; }
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
        /// 发出事件
        /// </summary>
        static void InvokeEvent(string name, string context, StationConfig config)
        {
            try
            {
                StationEvent?.Invoke(ZeroApplication.Config, new StationEventArgument(name, context, config));
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(name, e);
            }
        }
        /// <summary>
        /// 发出事件
        /// </summary>
        /// <param name="name"></param>
        internal static void RaiseEvent(string name)
        {
            StationEvent?.Invoke(ZeroApplication.Config, new StationEventArgument(name, null, null));
        }

        #endregion
    }
}