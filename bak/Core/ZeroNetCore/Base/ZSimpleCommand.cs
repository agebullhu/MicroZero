using System;
using ZeroMQ;

namespace Agebull.MicroZero
{
    /// <summary>
    /// 管理命令
    /// </summary>
    public abstract class ZSimpleCommand
    {
        /// <summary>
        /// 管理站点地址
        /// </summary>
        public string ManageAddress { get; set; }

        /// <summary>
        /// 管理站点地址
        /// </summary>
        public byte[] ServiceKey { get; set; }

        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <param name="args">请求参数,第一个必须为命令名称</param>
        /// <returns></returns>
        public ZeroResult CallCommand(params string[] args)
        {
            byte[] description = new byte[5 + args.Length];
            description[0] = (byte)(args.Length + 1);
            description[1] = (byte)ZeroByteCommand.General;
            description[2] = ZeroFrameType.Command;
            int idx = 3;
            for (var index = 1; index < args.Length; index++)
            {
                description[idx++] = ZeroFrameType.Argument;
            }
            description[idx++] = ZeroFrameType.SerivceKey;
            description[idx] = ZeroFrameType.ExtendEnd;
            return CallCommandInner(description, args);
        }

        /// <summary>
        ///     执行管理命令(快捷命令，命令在说明符号的第二个字节中)
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool ByteCommand(ZeroByteCommand cmd, params string[] args)
        {
            byte[] description = new byte[4 + args.Length];
            description[0] = (byte)(args.Length + 1);
            description[1] = (byte)cmd;
            int idx = 2;
            for (var index = 0; index < args.Length; index++)
            {
                description[idx++] = ZeroFrameType.Argument;
            }
            description[idx++] = ZeroFrameType.SerivceKey;
            description[idx] = ZeroFrameType.ExtendEnd;
            return CallCommandInner(description, args).InteractiveSuccess;
        }

        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <param name="des">数据格式声明</param>
        /// <param name="args">请求参数</param>
        /// <returns></returns>
        protected ZeroResult CallCommandInner(byte[] des, params string[] args)
        {
            if (ManageAddress == null || ServiceKey == null)
                return new ZeroResult
                {
                    InteractiveSuccess = false,
                    ErrorMessage = "地址无效"
                };

            var socket = ZSocketEx.CreateOnceSocket(ManageAddress, ServiceKey, ZSocket.CreateIdentity(false, "Dispatcher"));
            if (socket == null)
                return new ZeroResult
                {
                    InteractiveSuccess = false,
                    State = ZeroOperatorStateType.NetError
                };
            try
            {
                using (socket)
                {
                    using (var message = new ZMessage(des, args))
                    {
                        if (!socket.SendByServiceKey(message))
                            return new ZeroResult
                            {
                                State = ZeroOperatorStateType.LocalRecvError,
                                ZmqError = socket.LastError
                            };
                    }
                    return socket.ReceiveString();
                }
            }
            catch (Exception e)
            {
                return new ZeroResult
                {
                    InteractiveSuccess = false,
                    Exception = e
                };
            }
        }

    }
}