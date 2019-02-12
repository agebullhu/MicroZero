#if !UseStateMachine
using System;
using System.Threading;
using Agebull.ZeroNet.Core.ZeroManagemant.StateMachine;
using Agebull.ZeroNet.PubSub;
using ZeroMQ;
using Timer = System.Timers.Timer;
namespace Agebull.ZeroNet.Core.ZeroManagemant
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
                ZeroApplication.ZeroCenterState = ZeroCenterState.Failed;
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
                    MonitorStateMachineBase.center_start(station, content);
                    return;
                case ZeroNetEventType.CenterSystemClosing:
                    MonitorStateMachineBase.center_closing(station, content);
                    return;
                case ZeroNetEventType.CenterSystemStop:
                    MonitorStateMachineBase.center_stop(station, content);
                    return;
            }

            if (ZeroApplication.ZeroCenterState >= ZeroCenterState.Failed)
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
                    MonitorStateMachineBase.worker_sound_off();
                    return;
                case ZeroNetEventType.CenterStationTrends:
                    MonitorStateMachineBase.station_trends(station, content);
                    return;
                case ZeroNetEventType.CenterStationInstall:
                    MonitorStateMachineBase.station_install(station, content);
                    return;
                case ZeroNetEventType.CenterStationUpdate:
                    MonitorStateMachineBase.station_update(station, content);
                    return;
                case ZeroNetEventType.CenterStationJoin:
                    MonitorStateMachineBase.station_join(station, content);
                    return;
                case ZeroNetEventType.CenterStationPause:
                    MonitorStateMachineBase.station_pause(station);
                    return;
                case ZeroNetEventType.CenterStationResume:
                    MonitorStateMachineBase.station_resume(station);
                    return;
                case ZeroNetEventType.CenterStationClosing:
                    MonitorStateMachineBase.station_closing(station);
                    return;
                case ZeroNetEventType.CenterStationLeft:
                    MonitorStateMachineBase.station_left(station);
                    return;
                case ZeroNetEventType.CenterStationStop:
                    MonitorStateMachineBase.station_stop(station);
                    return;
                case ZeroNetEventType.CenterStationRemove:
                    MonitorStateMachineBase.station_uninstall(station);
                    return;
                case ZeroNetEventType.CenterStationDocument:
                    MonitorStateMachineBase.station_document(station, content);
                    return;
                case ZeroNetEventType.CenterClientJoin:
                    MonitorStateMachineBase.client_join(station, content);
                    return;
                case ZeroNetEventType.CenterClientLeft:
                    MonitorStateMachineBase.client_left(station, content);
                    return;
            }
        }
        private static Timer _time;

        private static void Time_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _time.Dispose();
            _time = null;
            if (ZeroApplication.ApplicationIsRun)
                return;
            ZeroTrace.SystemLog("Restart");
            ZeroApplication.JoinCenter();
        }

        #endregion
    }
}
#endif