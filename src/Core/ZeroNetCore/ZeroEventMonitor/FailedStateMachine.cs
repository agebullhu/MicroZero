using System.Threading.Tasks;

namespace Agebull.MicroZero.ZeroManagemant
{
    internal partial class MonitorStateMachine
    {
        /// <summary>
        /// 监控状态机
        /// </summary>
        class FailedStateMachine : IMonitorStateMachine
        {
            /// <summary>
            ///     收到信息的处理
            /// </summary>
            async Task IMonitorStateMachine.OnMessagePush(ZeroNetEventType zeroNetEvent, string station, string content)
            {
                switch (zeroNetEvent)
                {
                    case ZeroNetEventType.CenterSystemStart:
                        //StateMachine = new RuningStateMachine();
                        //await center_start(station, content);
                        //return;
                    case ZeroNetEventType.CenterWorkerSoundOff:
                        //StateMachine = new RuningStateMachine();
                        //ZeroTrace.SystemLog("Restart");
                        //await ZeroApplication.JoinCenter();
                        StateMachine = new EmptyStateMachine();
                        await center_start(station, content);
                        return;
                }
            }
        }
    }
}