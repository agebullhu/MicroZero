using System;

namespace Agebull.ZeroNet.Core.ZeroServices.StateMachine
{
    /// <summary>
    /// 监控状态机
    /// </summary>
    public class StationStateMachineBase : IDisposable
    {
        /// <summary>
        /// 站点
        /// </summary>
        public IZeroObject Station { get; set; }


        /// <summary>
        /// 是否已析构
        /// </summary>
        public bool IsDisposed { get; protected set; }

        void IDisposable.Dispose()
        {
            IsDisposed = true;
        }
    }
}