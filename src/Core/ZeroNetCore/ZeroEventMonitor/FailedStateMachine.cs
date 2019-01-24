namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    public class FailedStateMachine : MonitorStateMachineBase, IMonitorStateMachine
    {
        /// <summary>
        /// 构造
        /// </summary>
        public FailedStateMachine()
        {
            ZeroTrace.WriteError(nameof(FailedStateMachine));
        }

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
                    IsDisposed = true;
                    center_start(station, content);
                    return;
                case ZeroNetEventType.CenterWorkerSoundOff:
                    IsDisposed = true;
                    ZeroTrace.SystemLog("Restart");
                    ZeroApplication.JoinCenter();
                    return;
            }
        }
    }
}