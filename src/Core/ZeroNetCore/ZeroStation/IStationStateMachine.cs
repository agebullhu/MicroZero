using System;
namespace Agebull.ZeroNet.Core.ZeroServices.StateMachine
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
        ///     开始的处理
        /// </summary>
        bool Start();

        /// <summary>
        ///     关闭的处理
        /// </summary>
        bool Close();

        /// <summary>
        ///     结束的处理
        /// </summary>
        bool End();

    }
}