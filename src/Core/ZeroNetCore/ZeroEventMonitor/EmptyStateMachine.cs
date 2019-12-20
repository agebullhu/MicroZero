using System.Threading.Tasks;

namespace Agebull.MicroZero.ZeroManagemant
{
    internal partial class MonitorStateMachine
    {
        /// <summary>
        /// 监控状态机
        /// </summary>
        class EmptyStateMachine : IMonitorStateMachine
        {
            /// <summary>
            ///     收到信息的处理
            /// </summary>
            Task IMonitorStateMachine.OnMessagePush(ZeroNetEventType zeroNetEvent, string station, string content)
            {
                //启动中，什么也不做
                return Task.CompletedTask;
            }
        }
    }
}