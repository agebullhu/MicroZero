using System;
using System.Threading.Tasks;
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
        public byte[] ServiceKey { get; set; }

        /// <summary>
        /// 管理站点地址
        /// </summary>
        public string ManageAddress { get; set; }

        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <param name="args">请求参数,第一个必须为命令名称</param>
        /// <returns></returns>
        public Task<ZeroResult> CallCommand(params string[] args)
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
            return CallCommand(description, args);
        }

        /// <summary>
        ///     执行管理命令(快捷命令，命令在说明符号的第二个字节中)
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected async Task<bool> ByteCommand(ZeroByteCommand cmd, params string[] args)
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
            var res = await CallCommand(description, args);
            return res.InteractiveSuccess;
        }

        /// <summary>
        ///     发起一次请求
        /// </summary>
        /// <param name="description"></param>
        /// <param name="args">请求参数</param>
        /// <returns></returns>
        protected async Task<ZeroResult> CallCommand(byte[] description, params string[] args)
        {
            if (ManageAddress == null)
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
                    if (!await socket.SendByServiceKey(description, args))
                        return new ZeroResult
                        {
                            State = ZeroOperatorStateType.LocalRecvError,
                            ZmqError = socket.LastError
                        };
                    return await socket.Receive<ZeroResult>();
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