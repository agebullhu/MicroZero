using System.Threading.Tasks;

namespace Agebull.MicroZero.ZeroServices.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    public class EmptyStateMachine : StationStateMachineBase, IStationStateMachine
    {
        /// <summary>
        ///     开始的处理
        /// </summary>
        Task<bool> IStationStateMachine.Start()
        {
            ZeroApplication.OnObjectFailed(Station);
            return Task.FromResult(false);
        }
        /// <summary>
        ///     结束的处理
        /// </summary>
        Task<bool> IStationStateMachine.End()
        {
            ZeroApplication.OnObjectClose(Station);
            return Task.FromResult(false);
        }

        /// <summary>
        ///     关闭的处理
        /// </summary>
        bool IStationStateMachine.Close()
        {
            return false;
        }
    }
}