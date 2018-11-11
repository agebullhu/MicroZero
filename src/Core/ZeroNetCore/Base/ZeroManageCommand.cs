using System;
using System.Linq;
using Agebull.Common.Rpc;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 管理命令
    /// </summary>
    public abstract class ZeroManageCommand
    {
        /// <summary>
        /// 地址错误的情况
        /// </summary>
        /// <returns></returns>
        protected abstract string GetAddress();
        /// <summary>
        /// 管理站点地址
        /// </summary>
        public string ManageAddress { get; set; }

        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <param name="args">请求参数,第一个必须为命令名称</param>
        /// <returns></returns>
        public ZeroResultData CallCommand(params string[] args)
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
            description[idx] = ZeroFrameType.End;
            return CallCommand(description, args);
        }

        private ZSocket _socket;

        private readonly object LockObj = new object();
        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <param name="description"></param>
        /// <param name="args">请求参数</param>
        /// <returns></returns>
        protected ZeroResultData CallCommand(byte[] description, params string[] args)
        {
            lock (LockObj)
            {
                if (_socket == null)
                {
                    if (ManageAddress == null)
                        ManageAddress = GetAddress();
                    if (ManageAddress == null)
                        return new ZeroResultData
                        {
                            InteractiveSuccess = false,
                            Message = "地址无效"
                        };
                    _socket = ZSocket.CreateDealerSocket(ManageAddress);
                }
                try
                {
                    var result = SendTo(_socket, description, args);
                    if (!result.InteractiveSuccess)
                    {
                        _socket.TryClose();
                        _socket = null;
                        return result;
                    }

                    result = _socket.ReceiveString();
                    if (result.InteractiveSuccess)
                        return result;
                    _socket.TryClose();
                    _socket = null;
                    return result;
                }
                catch (Exception e)
                {
                    _socket?.TryClose();
                    _socket = null;
                    return new ZeroResultData
                    {
                        InteractiveSuccess = false,
                        Exception = e
                    };
                }
            }
        }

        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="desicription"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ZeroResultData SendTo(ZSocket socket, byte[] desicription, params string[] args)
        {
            var message = new ZMessage
            {
                new ZFrame(desicription)
            };
            if (args != null)
            {
                foreach (var arg in args)
                {
                    message.Add(new ZFrame(arg.ToZeroBytes()));
                }
                message.Add(new ZFrame(GlobalContext.ServiceKey.ToZeroBytes()));
            }

            if (socket.SendTo(message))
                return new ZeroResultData
                {
                    State = ZeroOperatorStateType.Ok,
                    InteractiveSuccess = true
                };
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


        /// <summary>
        ///     执行管理命令(快捷命令，命令在说明符号的第二个字节中)
        /// </summary>
        /// <param name="commmand"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected bool ByteCommand(ZeroByteCommand commmand, params string[] args)
        {
            byte[] description = new byte[4 + args.Length];
            description[0] = (byte)(args.Length + 1);
            description[1] = (byte)commmand;
            int idx = 2;
            for (var index = 0; index < args.Length; index++)
            {
                description[idx++] = ZeroFrameType.Argument;
            }
            description[idx++] = ZeroFrameType.SerivceKey;
            description[idx] = ZeroFrameType.End;
            return CallCommand(description, args).InteractiveSuccess;
        }

        /// <summary>
        /// 关闭仅有的一个连接
        /// </summary>
        public void Destroy()
        {
            _socket?.Close();
            _socket = null;
        }
    }
}