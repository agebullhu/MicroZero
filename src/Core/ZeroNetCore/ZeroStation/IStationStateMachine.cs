using System;
namespace Agebull.ZeroNet.Core.StationStateMachine
{
    /// <summary>
    /// 站点状态机
    /// </summary>
    public interface IStationStateMachine : IDisposable
    {
        /// <summary>
        /// 站点
        /// </summary>
        IZeroObject Station { get; set; }

        /// <summary>
        /// 是否已析构
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// 能否运行
        /// </summary>
        bool CanRun { get; }

        /// <summary>
        ///     开始的处理
        /// </summary>
        void Start();

        /// <summary>
        ///     结束的处理
        /// </summary>
        void Close();

        /// <summary>
        /// 站点状态变更时调用
        /// </summary>
        void OnStationStateChanged(StationConfig config);


        /// <summary>
        ///     状态机变为可用时的处理
        /// </summary>
        void OnLive();

    }

    /// <summary>
    /// 监控状态机
    /// </summary>
    public class StartStateMachine : IStationStateMachine
    {
        /// <summary>
        /// 站点
        /// </summary>
        public IZeroObject Station { get; set; }


        #region IDisposable

        /// <summary>
        /// 是否已析构
        /// </summary>
        public bool IsDisposed { get; protected set; }

        void IDisposable.Dispose()
        {
            IsDisposed = true;
        }

        #endregion

        /// <summary>
        /// 能否运行
        /// </summary>
        bool IStationStateMachine.CanRun => ZeroApplication.CanDo;

        /// <summary>
        ///     状态机变为可用时的处理
        /// </summary>
        void IStationStateMachine.OnLive()
        {
            if(ZeroApplication.CanDo && Station.ConfigState <= ZeroCenterState.Pause)
                Station.OnZeroStart();
        }

        /// <summary>
        ///     开始的处理
        /// </summary>
        void IStationStateMachine.Start()
        {
            Station.OnZeroStart();
        }

        /// <summary>
        ///     结束的处理
        /// </summary>
        void IStationStateMachine.Close()
        {

        }
        /// <summary>
        /// 站点状态变更时调用
        /// </summary>
        void IStationStateMachine.OnStationStateChanged(StationConfig config)
        {

        }
    }



    /// <summary>
    /// 监控状态机
    /// </summary>
    public class RunStateMachine : IStationStateMachine
    {
        /// <summary>
        /// 站点
        /// </summary>
        public IZeroObject Station { get; set; }


        #region IDisposable

        /// <summary>
        /// 是否已析构
        /// </summary>
        public bool IsDisposed { get; protected set; }

        void IDisposable.Dispose()
        {
            IsDisposed = true;
        }

        #endregion

        /// <summary>
        ///     状态机变为可用时的处理
        /// </summary>
        void IStationStateMachine.OnLive()
        {

        }

        /// <summary>
        /// 能否运行
        /// </summary>
        bool IStationStateMachine.CanRun => ZeroApplication.CanDo;

        /// <summary>
        ///     开始的处理
        /// </summary>
        void IStationStateMachine.Start()
        {

        }

        /// <summary>
        ///     结束的处理
        /// </summary>
        void IStationStateMachine.Close()
        {
            Station.OnZeroEnd();
        }
        /// <summary>
        /// 站点状态变更时调用
        /// </summary>
        void IStationStateMachine.OnStationStateChanged(StationConfig config)
        {

        }
    }


    /// <summary>
    /// 监控状态机
    /// </summary>
    public class FailedStateMachine : IStationStateMachine
    {
        /// <summary>
        /// 站点
        /// </summary>
        public IZeroObject Station { get; set; }


        #region IDisposable

        /// <summary>
        /// 是否已析构
        /// </summary>
        public bool IsDisposed { get; protected set; }

        void IDisposable.Dispose()
        {
            IsDisposed = true;
        }

        #endregion

        /// <summary>
        ///     状态机变为可用时的处理
        /// </summary>
        void IStationStateMachine.OnLive()
        {
            if (Station.RealState == StationState.Run)
                Station.OnZeroEnd();
        }

        /// <summary>
        /// 能否运行
        /// </summary>
        bool IStationStateMachine.CanRun => ZeroApplication.CanDo;

        /// <summary>
        ///     开始的处理
        /// </summary>
        void IStationStateMachine.Start()
        {
            if (Station.RealState == StationState.Run)
                Station.OnZeroStart();
        }

        /// <summary>
        ///     结束的处理
        /// </summary>
        void IStationStateMachine.Close()
        {
        }

        /// <summary>
        /// 站点状态变更时调用
        /// </summary>
        void IStationStateMachine.OnStationStateChanged(StationConfig config)
        {

        }
    }

}