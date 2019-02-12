namespace Agebull.ZeroNet.Core.ZeroManagemant.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    public class EmptyStateMachine : MonitorStateMachineBase, IMonitorStateMachine
    {
        /// <summary>
        ///     收到信息的处理
        /// </summary>
        void IMonitorStateMachine.OnMessagePush(ZeroNetEventType zeroNetEvent, string station, string content)
        {
            //启动中，什么也不做
        }
    }
}