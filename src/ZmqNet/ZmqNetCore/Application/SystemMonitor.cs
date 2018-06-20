using System;
using System.Threading;
using Agebull.Common.Logging;
using Agebull.ZeroNet.PubSub;
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

        private static readonly SemaphoreSlim TaskEndSem = new SemaphoreSlim(0);

        /// <summary>
        /// 等待正常结束
        /// </summary>
        internal static void WaitMe()
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
            }
            using (var poll = ZmqPool.CreateZmqPool())
            {
                poll.Prepare(new[] { ZSocket.CreateSubscriberSocket(ZeroApplication.Config.ZeroMonitorAddress) }, ZPollEvent.In);
                ZeroTrace.WriteLine("Zero center in monitor...");
                TaskEndSem.Release();
                while (ZeroApplication.IsAlive)
                {
                    if (!poll.Poll() || !poll.CheckIn(0, out var message))
                    {
                        continue;
                    }
                    if (!message.Unpack(out var item))
                        continue;
                    OnMessagePush(item.ZeroEvent, item.Station, item.Content);
                }
            }
            TaskEndSem.Release();
        }

        #endregion

        #region 事件处理

        /// <summary>
        ///     收到信息的处理
        /// </summary>
        private static void OnMessagePush(ZeroNetEventType zeroNetEvent, string station, string content)
        {
            switch (zeroNetEvent)
            {
                case ZeroNetEventType.CenterSystemStart:
                    center_start(content);
                    return;
                case ZeroNetEventType.CenterSystemClosing:
                    center_closing(content);
                    return;
                case ZeroNetEventType.CenterSystemStop:
                    center_stop(content);
                    return;
                case ZeroNetEventType.CenterWorkerSoundOff:
                    worker_sound_off();
                    return;
            }
            if (!ZeroApplication.InRun)
                return;
            switch (zeroNetEvent)
            {
                case ZeroNetEventType.CenterStationJoin:
                    station_join(station, content);
                    return;
                case ZeroNetEventType.CenterStationLeft:
                    station_left(station);
                    return;
                case ZeroNetEventType.CenterStationPause:
                    station_pause(station);
                    return;
                case ZeroNetEventType.CenterStationResume:
                    station_resume(station);
                    return;
                case ZeroNetEventType.CenterStationClosing:
                    station_closing(station);
                    return;
                case ZeroNetEventType.CenterStationInstall:
                    station_install(station, content);
                    return;
                case ZeroNetEventType.CenterStationUninstall:
                    station_uninstall(station);
                    return;
                case ZeroNetEventType.CenterStationState:
                    station_state(station, content);
                    return;
            }
        }

        private static void station_uninstall(string name)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroTrace.WriteInfo("station_uninstall", name);
            ZeroApplication.Config[name] = null;
            config.State = ZeroCenterState.Uninstall;
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            InvokeEvent(ZeroNetEventType.CenterStationUninstall, null, config);
        }

        private static void station_install(string name, string content)
        {
            if (!ZeroApplication.Config.UpdateConfig(name, content, out var config))
                return;
            ZeroTrace.WriteInfo("station_install", name, content);
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            InvokeEvent(ZeroNetEventType.CenterStationInstall, content, config);
        }

        private static void station_closing(string name)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroTrace.WriteInfo("station_closing", name);
            config.State = ZeroCenterState.Closing;
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            InvokeEvent(ZeroNetEventType.CenterStationClosing, null, config);
        }

        private static void station_resume(string name)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroTrace.WriteInfo("station_resume", name);
            config.State = ZeroCenterState.Run;
            if (ZeroApplication.InRun)
                ZeroApplication.OnStationStateChanged(config);
            InvokeEvent(ZeroNetEventType.CenterStationResume, null, config);
        }

        private static void station_pause(string name)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroTrace.WriteInfo("station_pause", name);
            config.State = ZeroCenterState.Pause;
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            InvokeEvent(ZeroNetEventType.CenterStationPause, null, config);
        }

        private static void station_left(string name)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroTrace.WriteInfo("station_left", name);
            config.State = ZeroCenterState.Closed;
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            InvokeEvent(ZeroNetEventType.CenterStationLeft, null, config);
        }

        private static void station_join(string name, string content)
        {
            if (!ZeroApplication.Config.UpdateConfig(name, content, out var config))
                return;
            ZeroTrace.WriteInfo("station_join", content);
            config.State = ZeroCenterState.Run;
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            InvokeEvent(ZeroNetEventType.CenterStationJoin, content, config);
        }

        /// <summary>
        /// 站点心跳
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        private static void station_state(string name, string content)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var old))
                return;
            try
            {
                var config = JsonConvert.DeserializeObject<StationConfig>(content);
                InvokeEvent(ZeroNetEventType.CenterStationState, content, old, config);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                ZeroTrace.WriteException("station_state", e, name, content);
            }
        }


        private static void center_start(string content)
        {
            if (Interlocked.CompareExchange(ref ZeroApplication._appState, StationState.Initialized, StationState.Failed) == StationState.Failed)
            {
                ZeroTrace.WriteInfo("center_start", content);
                ZeroApplication.JoinCenter();
            }
        }

        private static void center_closing(string content)
        {
            if (Interlocked.CompareExchange(ref ZeroApplication._appState, StationState.Closing, StationState.Run) == StationState.Run)
            {
                ZeroTrace.WriteError("center_close", content);
                RaiseEvent(ZeroNetEventType.CenterSystemClosing);
                ZeroApplication.ZerCenterStatus = ZeroCenterState.Closed;
                ZeroApplication.OnZeroEnd();
                ZeroApplication.ApplicationState = StationState.Failed;
            }
        }

        private static void center_stop(string content)
        {
            if (ZeroApplication.ZerCenterStatus == ZeroCenterState.Destroy)
                return;
            ZeroApplication.ZerCenterStatus = ZeroCenterState.Destroy;
            ZeroTrace.WriteInfo("center_stop ", content);
        }

        /// <summary>
        /// 站点心跳
        /// </summary>
        private static void worker_sound_off()
        {
            //SystemManage对象重启时机
            if (Interlocked.CompareExchange(ref ZeroApplication._appState, StationState.Start, StationState.Failed) == StationState.Failed)
            {
                ZeroApplication.JoinCenter();
                return;
            }

            return;
            if (ZeroApplication.InRun)
                ZeroApplication.OnHeartbeat();
        }

        #endregion

        #region 对外事件

        /// <summary>
        /// 站点事件参数
        /// </summary>
        public class ZeroNetEventArgument : EventArgs
        {
            /// <summary>
            /// 构造
            /// </summary>
            /// <param name="centerEvent"></param>
            /// <param name="context"></param>
            /// <param name="config"></param>
            /// <param name="nc"></param>
            public ZeroNetEventArgument(ZeroNetEventType centerEvent, string context, StationConfig config, StationConfig nc)
            {
                EventConfig = config;
                Event = centerEvent;
                Context = context;
                NewConfig = nc;
            }

            /// <summary>
            /// 站点名称
            /// </summary>
            public readonly ZeroNetEventType Event;

            /// <summary>
            /// 内容
            /// </summary>
            public readonly string Context;

            /// <summary>
            /// 配置
            /// </summary>
            public readonly StationConfig EventConfig;

            /// <summary>
            /// 配置
            /// </summary>
            public readonly StationConfig NewConfig;
        }

        /// <summary>
        /// 站点事件发生
        /// </summary>
        public static event EventHandler<ZeroNetEventArgument> ZeroNetEvent;

        /// <summary>
        /// 发出事件
        /// </summary>
        static void InvokeEvent(ZeroNetEventType centerEvent, string context, StationConfig config, StationConfig newcfg = null)
        {
            try
            {
                ZeroNetEvent?.Invoke(ZeroApplication.Config, new ZeroNetEventArgument(centerEvent, context, config, newcfg));
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("InvokeEvent", e, centerEvent);
            }
        }
        /// <summary>
        /// 发出事件
        /// </summary>
        /// <param name="centerEvent"></param>
        internal static void RaiseEvent(ZeroNetEventType centerEvent)
        {
            ZeroNetEvent?.Invoke(ZeroApplication.Config, new ZeroNetEventArgument(centerEvent, null, null, null));
        }

        #endregion
    }

}