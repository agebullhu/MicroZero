using System;
using System.Threading.Tasks;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 管理命令
    /// </summary>
    public class ZeroManageCommand
    {
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
            byte[] description = new byte[3 + args.Length];
            description[0] = (byte)(args.Length);
            description[1] = ZeroByteCommand.General;
            description[2] = ZeroFrameType.Command;
            int idx = 3;
            for (var index = 1; index < args.Length; index++)
            {
                description[idx++] = ZeroFrameType.Argument;
            }
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
                    _socket = ZSocket.CreateRequestSocket(ManageAddress);
                try
                {
                    var result = _socket.SendTo(description, args);
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
        ///     执行管理命令(快捷命令，命令在说明符号的第二个字节中)
        /// </summary>
        /// <param name="commmand"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected bool ByteCommand(byte commmand, params string[] args)
        {
            byte[] description = new byte[4 + args.Length];
            description[0] = (byte)(args.Length);
            description[1] = commmand;
            int idx = 2;
            for (var index = 0; index < args.Length; index++)
            {
                description[idx++] = ZeroFrameType.Argument;
            }
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