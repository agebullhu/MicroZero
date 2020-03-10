using System;
using ZeroMQ;

namespace Agebull.MicroZero
{
    /// <summary>
    /// 返回值
    /// </summary>
    public interface IZeroState
    {
        /// <summary>
        /// 逻辑操作状态
        /// </summary>
        ZeroOperatorStateType State { get; set; }

        /// <summary>
        /// 与服务器网络交互是否正常（特别注意：不是指逻辑操作是否成功,逻辑成功看InteractiveSuccess=true时的State）
        /// </summary>
        bool InteractiveSuccess { get; set; }

        /// <summary>
        /// ZMQ错误码
        /// </summary>
        ZError ZmqError { get; set; }

        /// <summary>
        /// 异常
        /// </summary>
        Exception Exception { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        string ErrorMessage { get; set; }
    }
}