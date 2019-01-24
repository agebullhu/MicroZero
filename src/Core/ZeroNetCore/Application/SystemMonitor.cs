#if !UseStateMachine
using System;
using System.Threading;
using Agebull.Common.ApiDocuments;
using Agebull.Common.Logging;
using Agebull.ZeroNet.PubSub;
using Newtonsoft.Json;
using ZeroMQ;
using Timer = System.Timers.Timer;
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
            if (ZeroApplication.WorkModel == ZeroWorkModel.Service)
                TaskEndSem.Wait();
        }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal static void Monitor()
        {
            if (ZeroApplication.WorkModel != ZeroWorkModel.Service)
                return;
            using (OnceScope.CreateScope(ZeroApplication.Config))
            {
            }
            TaskEndSem.Release();
            ZeroTrace.SystemLog("Zero center in monitor...");
            while (ZeroApplication.IsAlive)
            {
                if (MonitorInner())
                    continue;
                ZeroTrace.WriteError("Zero center monitor failed", "There was no message for a long time");
                ZeroApplication.ZerCenterStatus = ZeroCenterState.Failed;
                ZeroApplication.OnZeroEnd();
                ZeroApplication.ApplicationState = StationState.Failed;
                Thread.Sleep(1000);
            }
            ZeroTrace.SystemLog("Zero center monitor stoped!");
            TaskEndSem.Release();
        }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        static bool MonitorInner()
        {
            DateTime failed = DateTime.MinValue;
            using (var poll = ZmqPool.CreateZmqPool())
            {
                poll.Prepare(ZPollEvent.In, ZSocket.CreateSubSocket(ZeroApplication.Config.ZeroMonitorAddress, ZeroIdentityHelper.CreateIdentity()));
                while (ZeroApplication.IsAlive)
                {
                    if (!poll.Poll())
                    {
                        if (failed == DateTime.MinValue)
                            failed = DateTime.Now;
                        else if ((DateTime.Now - failed).TotalMinutes > 1)
                            return false;
                        continue;
                    }
                    if (!poll.CheckIn(0, out var message))
                    {
                        continue;
                    }
                    failed = DateTime.MinValue;
                    if (!PublishItem.Unpack(message, out var item))
                        continue;
                    OnMessagePush(item.ZeroEvent, item.SubTitle, item.Content);
                }
            }
            return true;
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
                    center_start(station, content);
                    return;
                case ZeroNetEventType.CenterSystemClosing:
                    center_closing(station, content);
                    return;
                case ZeroNetEventType.CenterSystemStop:
                    center_stop(station, content);
                    return;
            }

            if (ZeroApplication.ZerCenterStatus >= ZeroCenterState.Failed)
            {
                switch (zeroNetEvent)
                {
                    case ZeroNetEventType.CenterStationClosing:
                    case ZeroNetEventType.CenterStationLeft:
                    case ZeroNetEventType.CenterStationStop:
                    case ZeroNetEventType.CenterClientLeft:
                        return;
                    case ZeroNetEventType.CenterWorkerSoundOff:
                        if (_time != null)
                            return;
                        _time = new Timer(1000)
                        {
                            AutoReset = false
                        };
                        _time.Elapsed += Time_Elapsed;
                        _time.Start();
                        return;
                    default:
                        return;
                }
            }
            if (ZeroApplication.ApplicationState != StationState.Run)
                return;

            switch (zeroNetEvent)
            {
                case ZeroNetEventType.CenterWorkerSoundOff:
                    worker_sound_off();
                    return;
                case ZeroNetEventType.CenterStationState:
                    station_state(station, content);
                    return;
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
                case ZeroNetEventType.CenterStationDocument:
                    station_document(station, content);
                    return;
                case ZeroNetEventType.CenterClientJoin:
                    client_join(station, content);
                    return;
                case ZeroNetEventType.CenterClientLeft:
                    client_left(station, content);
                    return;
            }
        }

        private static void Time_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _time.Dispose();
            _time = null;
            if (ZeroApplication.ApplicationIsRun)
                return;
            ZeroTrace.SystemLog("Restart");
            ZeroApplication.JoinCenter();
        }

        private static Timer _time;
        /// <summary>
        /// 站点心跳
        /// </summary>
        private static void worker_sound_off()
        {
            if (ZeroApplication.ApplicationState != StationState.Run || ZeroApplication.ZerCenterStatus != ZeroCenterState.Run)
                return;
            SystemManager.Instance.Heartbeat();
        }

        #region CenterEvent

        private static string ZeroCenterIdentity;

        private static void center_start(string identity, string content)
        {
            if (identity == ZeroCenterIdentity)
                return;
            ZeroCenterIdentity = identity;
            ZeroTrace.SystemLog("center_start", $"{identity}:{ZeroApplication.ZerCenterStatus}:{ZeroCenterIdentity}");
            if (ZeroApplication.ZerCenterStatus >= ZeroCenterState.Failed)
            {
                ZeroApplication.JoinCenter();
            }
            else
            {
                SystemManager.Instance.LoadAllConfig();
            }
        }

        private static void center_closing(string identity, string content)
        {
            var id = $"*{identity}";
            if (id == ZeroCenterIdentity)
                return;
            ZeroCenterIdentity = id;
            ZeroTrace.SystemLog("center_closing", $"{identity}:{ZeroApplication.ZerCenterStatus}:{ZeroCenterIdentity}");
            if (ZeroApplication.ZerCenterStatus < ZeroCenterState.Closing)
            {
                ZeroApplication.ZerCenterStatus = ZeroCenterState.Closing;
                ZeroApplication.RaiseEvent(ZeroNetEventType.CenterSystemClosing);
                ZeroApplication.OnZeroEnd();
                ZeroApplication.ApplicationState = StationState.Failed;
            }
        }

        private static void center_stop(string identity, string content)
        {
            var id = $"#{identity}";
            if (id == ZeroCenterIdentity)
                return;
            ZeroCenterIdentity = id;
            ZeroTrace.SystemLog("center_stop", $"{identity}:{ZeroApplication.ZerCenterStatus}:{ZeroCenterIdentity}");
            ZeroApplication.ZerCenterStatus = ZeroCenterState.Closed;
        }

        #endregion

        #region StationEvent

        private static void ChangeStationState(string name, ZeroCenterState state, ZeroNetEventType eventType)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            config.State = state;
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            ZeroApplication.InvokeEvent(eventType, null, config);
        }

        private static void station_update(string name, string content)
        {
            ZeroTrace.SystemLog("station_update", name, content);
            if (!ZeroApplication.Config.UpdateConfig(name, content, out var config))
                return;
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationUpdate, content, config);
        }
        private static void station_install(string name, string content)
        {
            if (!ZeroApplication.Config.UpdateConfig(name, content, out var config))
                return;
            ZeroTrace.SystemLog("station_install", name, content);
            config.State = ZeroCenterState.None;
            if (!ZeroApplication.InRun)
                return;
            ZeroApplication.OnStationStateChanged(config);
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationInstall, content, config);
        }

        private static void station_uninstall(string name)
        {
            ZeroTrace.SystemLog("station_uninstall", name);
            ChangeStationState(name, ZeroCenterState.Remove, ZeroNetEventType.CenterStationRemove);
        }


        private static void station_closing(string name)
        {
            ZeroTrace.SystemLog("station_closing", name);
            ChangeStationState(name, ZeroCenterState.Closing, ZeroNetEventType.CenterStationClosing);
        }


        private static void station_resume(string name)
        {
            ZeroTrace.SystemLog("station_resume", name);
            ChangeStationState(name, ZeroCenterState.Run, ZeroNetEventType.CenterStationResume);
        }

        private static void station_pause(string name)
        {
            ZeroTrace.SystemLog("station_pause", name);
            ChangeStationState(name, ZeroCenterState.Pause, ZeroNetEventType.CenterStationPause);
        }

        private static void station_left(string name)
        {
            ZeroTrace.SystemLog("station_left", name);
            ChangeStationState(name, ZeroCenterState.Closed, ZeroNetEventType.CenterStationLeft);
        }

        private static void station_stop(string name)
        {
            ZeroTrace.SystemLog("station_stop", name);
            ChangeStationState(name, ZeroCenterState.Stop, ZeroNetEventType.CenterStationLeft);
        }

        private static void station_join(string name, string content)
        {
            ZeroTrace.SystemLog("station_join", content);
            ChangeStationState(name, ZeroCenterState.Run, ZeroNetEventType.CenterStationJoin);
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


        private static void station_document(string station, string content)
        {
            if (!ZeroApplication.Config.TryGetConfig(station, out var config))
                return;
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
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterStationDocument, station, config);
        }



        #endregion

        #region ClientEvent



        private static void client_join(string name, string content)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterClientJoin, name, config);
        }

        private static void client_left(string name, string content)
        {
            if (!ZeroApplication.Config.TryGetConfig(name, out var config))
                return;
            ZeroApplication.InvokeEvent(ZeroNetEventType.CenterClientLeft, name, config);
        }

        #endregion
        #endregion
    }

}
#endif