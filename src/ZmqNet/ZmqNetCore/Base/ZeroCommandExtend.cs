using System;
using System.Linq;
using System.Text;
using Agebull.ZeroNet.Log;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     Zmq帮助类
    /// </summary>
    public static class ZeroCommandExtend
    {

        #region 调用支持

        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="desicription"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ZeroResultData QuietSend(this ZSocket socket, byte[] desicription, params string[] args)
        {
            var message = new ZMessage();
            var frame = new ZFrame(desicription);
            message.Add(frame);
            if (args != null)
            {
                foreach (var arg in args)
                {
                    message.Add(new ZFrame(arg.ToZeroBytes()));
                }
            }
            if (!socket.SendTo(message))
            {
                return new ZeroResultData
                {
                    State = ZeroOperatorStateType.LocalRecvError,
                    ZmqError = socket.LastError
                };
            }
            return new ZeroResultData
            {
                State = ZeroOperatorStateType.Ok,
                InteractiveSuccess = true
            };
        }
        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="desicription"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ZeroResultData SendTo(this ZSocket socket, byte[] desicription, params string[] args)
        {
            var message = new ZMessage
            {
                new ZFrame(desicription)
            };
            if (args != null)
            {
                foreach (var arg in args)
                {
                    message.Add(new ZFrame((arg).ToZeroBytes()));
                }
            }
            if (!socket.SendTo(message))
            {
#if DEBUG
                        ZeroTrace.WriteError("SendTo", /*error.Text,*/ socket.Connects.LinkToString(','),
                            $"Socket Ptr:{socket.SocketPtr}");
#endif
                return new ZeroResultData
                {
                    State = ZeroOperatorStateType.LocalRecvError,
                    ZmqError = socket.LastError
                };
            }
            return new ZeroResultData
            {
                State = ZeroOperatorStateType.Ok,
                InteractiveSuccess = true
            };
        }

        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="desicription"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ZeroResultData Call(this ZSocket socket, byte[] desicription, params string[] args)
        {
            var result = SendTo(socket, desicription, args);
            return !result.InteractiveSuccess ? result : socket.ReceiveString();
        }

        #endregion

        #region 接收支持

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="showError">是否显示错误</param>
        /// <returns></returns>
        public static ZeroResultData ReceiveString(this ZSocket socket, bool showError = false)
        {

            ZMessage messages;
            try
            {
                if (!socket.Recv(out messages))
                {
                    if (showError)
                        ZeroTrace.WriteError("Receive", socket.Connects.LinkToString(','), socket.LastError.Text);
                    return new ZeroResultData
                    {
                        State = ZeroOperatorStateType.LocalRecvError,
                        ZmqError = socket.LastError
                    };
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Connects.LinkToString(','));
                return new ZeroResultData
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }
            return Unpack(messages, showError);
        }

        /// <summary>
        ///     接收字节
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static ZeroResultData<byte[]> Receive(this ZSocket socket)
        {
            ZMessage messages;
            try
            {
                if (!socket.Recv(out messages))
                {
                    return new ZeroResultData<byte[]>
                    {
                        State = ZeroOperatorStateType.LocalRecvError,
                        ZmqError = socket.LastError
                    };
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Connects.LinkToString(','), "Exception");
                return new ZeroResultData<byte[]>
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }

            try
            {
                var description = messages[0].Read();
                if (description.Length < 2)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", socket.Connects.LinkToString(','), description.LinkToString(p => p.ToString("X2"), ""));
                    return new ZeroResultData<byte[]>
                    {
                        State = ZeroOperatorStateType.FrameInvalid,
                        Message = "网络格式错误"
                    };
                }

                int end = description[0] + 1;
                if (end != messages.Count)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", socket.Connects.LinkToString(','), $"FrameSize{messages.Count}", description.LinkToString(p => p.ToString("X2"), ""));
                    return new ZeroResultData<byte[]>
                    {
                        State = ZeroOperatorStateType.FrameInvalid,
                        Message = "网络格式错误"
                    };
                }

                var result = new ZeroResultData<byte[]>
                {
                    InteractiveSuccess = true,
                    State = (ZeroOperatorStateType)description[1]
                };
                for (int idx = 1; idx < end; idx++)
                {
                    result.Add(description[idx + 1], messages[idx].Read());
                }

                return result;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Connects.LinkToString(','), $"FrameSize{messages.Count},Socket Ptr:{ socket.SocketPtr}.");
                return new ZeroResultData<byte[]>
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }
            finally
            {
                messages.Dispose();
            }
        }

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static ZeroResultData<byte[]> ReceiveUnknow(this ZSocket socket)
        {
            //tryCnt = 0;
            ZMessage messages;
            try
            {
                if (!socket.Recv(out messages))
                {
                    ZeroTrace.WriteError("Receive", socket.Connects.LinkToString(','), socket.LastError.Text);
                    return new ZeroResultData<byte[]>
                    {
                        State = ZeroOperatorStateType.LocalRecvError,
                        ZmqError = socket.LastError
                    };
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Connects.LinkToString(','));
                return new ZeroResultData<byte[]>
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }

            try
            {
                var result = new ZeroResultData<byte[]>
                {
                    InteractiveSuccess = true,
                    State = ZeroOperatorStateType.Ok
                };
                foreach (var frame in messages)
                {
                    result.Add(ZeroFrameType.BinaryValue, frame.Read());
                }

                return result;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Connects.LinkToString(','), $"FrameSize{messages.Count}, Socket Ptr:{ socket.SocketPtr}.");
                return new ZeroResultData<byte[]>
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }
            finally
            {
                messages.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// 命令解包
        /// </summary>
        /// <param name="showError"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static ZeroResultData Unpack(this ZMessage messages, bool showError = false)
        {
            try
            {
                var description = messages[0].Read();
                if (description.Length == 0)
                {
                    if (showError)
                        ZeroTrace.WriteError("Unpack", "LaoutError",
                            description.LinkToString(p => p.ToString("X2"), ""));
                    return new ZeroResultData
                    {
                        State = ZeroOperatorStateType.FrameInvalid,
                        Message = "网络格式错误"
                    };
                }

                int end = description[0] + 1;
                if (end != messages.Count)
                {
                    if (showError)
                        ZeroTrace.WriteError("Unpack", "LaoutError",
                            $"FrameSize{messages.Count}", description.LinkToString(p => p.ToString("X2"), ""));
                    return new ZeroResultData
                    {
                        State = ZeroOperatorStateType.FrameInvalid,
                        Message = "网络格式错误"
                    };
                }

                var result = new ZeroResultData
                {
                    InteractiveSuccess = true,
                    State = (ZeroOperatorStateType)description[1]
                };
                for (int idx = 1; idx < end; idx++)
                {
                    result.Add(description[idx + 1], Encoding.UTF8.GetString(messages[idx].Read()));
                }

                return result;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Unpack", e);
                return new ZeroResultData
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }
            finally
            {
                messages.Dispose();
            }
        }

    }
}