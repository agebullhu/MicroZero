namespace Agebull.ZeroNet.Core.ZeroManagemant.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    public class RuningStateMachine : MonitorStateMachineBase, IMonitorStateMachine
    {
        /// <summary>
        ///     收到信息的处理
        /// </summary>
        void IMonitorStateMachine.OnMessagePush(ZeroNetEventType zeroNetEvent, string station, string content)
        {
            if (IsDisposed)
                return;
            switch (zeroNetEvent)
            {
                case ZeroNetEventType.CenterSystemStart:
                    return;
                case ZeroNetEventType.CenterSystemClosing:
                    IsDisposed = true;
                    center_closing(station, content);
                    return;
                case ZeroNetEventType.CenterSystemStop:
                    IsDisposed = true;
                    center_stop(station, content);
                    return;
                case ZeroNetEventType.CenterWorkerSoundOff:
                    worker_sound_off();
                    return;
                case ZeroNetEventType.CenterStationTrends:
                    station_trends(station, content);
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
    }
}