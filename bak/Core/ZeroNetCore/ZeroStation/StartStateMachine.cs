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
        bool IStationStateMachine.Start()
        {
            if (IsDisposed)
            {
                ZeroApplication.OnObjectFailed(Station);
                return false;
            }
            IsDisposed = true;
            return Station.Start();
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
        bool IStationStateMachine.End()
        {
            return false;
        }
    }
}