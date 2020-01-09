using System;
using ZeroMQ;

namespace Agebull.MicroZero
{
    /// <summary>
    ///     Zmq帮助类
    /// </summary>
    public static class ZeroCommandExtend
    {
        #region 接收支持

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="showError">是否显示错误</param>
        /// <returns></returns>
        [Obsolete]
        public static ZeroResult ReceiveString(this ZSocketEx socket, bool showError = false)
        {
            return Receive<ZeroResult>(socket);
        }

        /// <summary>
        ///     接收字节
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static TZeroResultData Receive<TZeroResultData>(this ZSocketEx socket)
            where TZeroResultData : ZeroResultData, new()
        {
            ZMessage messages;
            try
            {
                if (!socket.Recv(out messages))
                {
                    return new TZeroResultData
                    {
                        State = ZeroOperatorStateType.LocalRecvError,
                        ZmqError = socket.LastError
                    };
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Endpoint, "Exception");
                return new TZeroResultData
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }
            try
            {
                return ZeroResultData.Unpack<TZeroResultData>(messages, true);
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Endpoint);
                return new TZeroResultData
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }
        }

        #endregion

        /// <summary>
        /// 应用名称字节内容
        /// </summary>
        public static byte[] AppNameBytes { get; internal set; }

        /*// <summary>
        /// 服务令牌字节内容
        /// </summary>
        public static byte[] ServiceKeyBytes { get; internal set; }*/


    }
}