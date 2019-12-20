using System;
using System.Threading.Tasks;

namespace Agebull.MicroZero.ZeroManagemant
{
    internal partial class MonitorStateMachine
    {
        /// <summary>
        /// 监控状态机
        /// </summary>
        public interface IMonitorStateMachine //: IDisposable
        {
            /*// <summary>
            /// 是否已析构
            /// </summary>
            bool IsDisposed { get; }*/

            /// <summary>
            ///     收到信息的处理
            /// </summary>
            Task OnMessagePush(ZeroNetEventType zeroNetEvent, string station, string content);
        }
    }
}