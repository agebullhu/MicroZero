using System.Threading.Tasks;

namespace Agebull.MicroZero.ZeroServices.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    public class StartStateMachine : StationStateMachineBase, IStationStateMachine
    {
        /// <summary>
        ///     开始的处理
        /// </summary>
        async Task<bool> IStationStateMachine.Start()
        {
            if (IsDisposed)
            {
                ZeroApplication.OnObjectFailed(Station);
                return false;
            }
            IsDisposed = true;
            return await Station.Start();
        }

        /// <summary>
        ///     关闭的处理
        /// </summary>
        bool IStationStateMachine.Close()
        {
            ZeroApplication.OnObjectClose(Station);
            return false;
        }

        /// <summary>
        ///     结束的处理
        /// </summary>
        Task<bool> IStationStateMachine.End()
        {
            return Task.FromResult(false);
        }
    }
}