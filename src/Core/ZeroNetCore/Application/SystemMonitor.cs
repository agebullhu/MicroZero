using System;
using System.Threading;
using Agebull.Common.ApiDocuments;
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
                    OnMessagePush(item.ZeroEvent, item.SubTitle, item.Content);
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
                case ZeroNetEventType.CenterStationInstall:
                    station_install(station, content);
                    return;
                case ZeroNetEventType.CenterStationUpdate:
                    station_update(station, content);
                    return;
                case ZeroNetEventType.CenterStationJoin:
                    station_join(station, content);
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
                case ZeroNetEventType.CenterStationLeft:
                    station_left(station);
                    return;
                case ZeroNetEventType.CenterStationStop:
                    station_stop(station);
                    return;
                case ZeroNetEventType.CenterStationRemove:
                    station_uninstall(station);
                    return;
                case ZeroNetEventType.CenterStationState:
                    station_state(station, content);
                    return;
                case ZeroNetEventType.CenterStationDocument:
                    station_document(station, content);
                    return;
            }
        }

        private static void station_uninstall(string name)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroTrace.WriteInfo("station_uninstall", name);
            config.State = ZeroCenterState.Remove;
            ZeroApplication.Config.Remove(config);
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationRemove, null, config);
        }

        private static void station_install(string name, string content)
        {
            if (!ZeroApplication.Config.UpdateConfig(name, content, out var config))
                return;
            ZeroTrace.WriteInfo("station_install", name, content);
            config.State = ZeroCenterState.None;
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationInstall, content, config);
        }

        private static void station_update(string name, string content)
        {
            if (!ZeroApplication.Config.UpdateConfig(name, content, out var config))
                return;
            ZeroTrace.WriteInfo("station_update", name, content);
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationUpdate, content, config);
        }

        private static void station_closing(string name)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroTrace.WriteInfo("station_closing", name);
            if (config.State < ZeroCenterState.Closing)
                config.State = ZeroCenterState.Closing;
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationClosing, null, config);
        }

        private static void station_resume(string name)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroTrace.WriteInfo("station_resume", name);
            config.State = ZeroCenterState.Run;
            if (ZeroApplication.InRun)
                ZeroApplication.OnStationStateChanged(config);
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationResume, null, config);
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
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationPause, null, config);
        }

        private static void station_left(string name)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroTrace.WriteInfo("station_left", name);
            if (config.State < ZeroCenterState.Closed)
                config.State = ZeroCenterState.Closed;
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationLeft, null, config);
        }

        private static void station_stop(string name)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroTrace.WriteInfo("station_stop", name);
            if (config.State < ZeroCenterState.Stop)
                config.State = ZeroCenterState.Stop;
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationLeft, null, config);
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
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationJoin, content, config);
        }

        /// <summary>
        /// 站点心跳
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        private static void station_state(string name, string content)
        {
            //ZeroTrace.WriteInfo("station_state",name);
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            try
            {
                ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationState, content, config);
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
                ZeroApplication.RaiseEvent(ZeroNetEventType.CenterSystemClosing);
                ZeroApplication.ZerCenterStatus = ZeroCenterState.Closed;
                ZeroApplication.OnZeroEnd();
                ZeroApplication.ApplicationState = StationState.Failed;
            }
        }
        private static void station_document(string station, string content)
        {
            var doc = JsonConvert.DeserializeObject<StationDocument>(content);
            if (ZeroApplication.Config.Documents.ContainsKey(station))
            {
                if (!ZeroApplication.Config.Documents[station].IsLocal)
                    ZeroApplication.Config.Documents[station] = doc;
            }
            else
            {
                ZeroApplication.Config.Documents.Add(station, doc);
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

            if (ZeroApplication.InRun)
                ZeroApplication.OnHeartbeat();
        }

        #endregion
    }

}