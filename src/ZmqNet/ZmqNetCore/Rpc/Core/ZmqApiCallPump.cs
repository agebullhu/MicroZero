using System;
using System.Diagnostics;
using NetMQ;
using NetMQ.Sockets;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    ///     问答式命令泵
    /// </summary>
    public class ZmqApiCallPump : ZmqCommandPump<RequestSocket>
    {
        #region 消息泵

        /// <summary>
        /// 生成SOCKET对象
        /// </summary>
        /// <remarks>版本不兼容导致改变</remarks>
        protected override RequestSocket CreateSocket()
        {
            return new RequestSocket();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected sealed override void DoInitialize()
        {

        }

        /// <summary>
        /// 配置Socket对象
        /// </summary>
        /// <param name="socket"></param>
        protected sealed override void OptionSocktet(RequestSocket socket)
        {
            //用户识别
            socket.Options.Identity = RouteRpc.Singleton.Config.Token.ToAsciiBytes();
            //列消息只作用于已完成的链接
            //关闭设置停留时间,毫秒
            socket.Options.Linger = new TimeSpan(0, 0, 0, 0, 50);
            socket.Options.ReceiveLowWatermark = 500;
            socket.Options.SendLowWatermark = 500;
        }

        /// <summary>
        /// 执行工作
        /// </summary>
        /// <param name="socket"></param>
        /// <returns>返回状态,其中-1会导致重连</returns>
        protected sealed override int DoWork(RequestSocket socket)
        {
            CommandArgument callArg = Pop();
            if (callArg == null)
                return 0;
            RpcEnvironment.CacheCommand(callArg);
            try
            {
                bool state;
                //发送命令请求
                state = socket.TrySendFrame(timeOut, callArg.commandName);
                if (!state)
                    return -1;
                //接收处理反馈
                bool more;
                do
                {
                    string result;
                    state = socket.TryReceiveFrameString(timeOut, out result, out more);
                    if (!state)
                        return -1;
                    if (!string.IsNullOrWhiteSpace(result))
                        Console.WriteLine(result);
                } while (more);
            }
            catch (Exception ex)
            {
                OnException(socket, ex);
                callArg.cmdState = RpcEnvironment.NET_COMMAND_STATE_CLIENT_UNKNOW;
                return -1;
            }
            return 1;
        }

        #endregion

        #region 命令请求


        /// <summary>
        ///     命令调用
        /// </summary>
        /// <param name="command">命令参数</param>
        /// <param name="cacheCommand">是否缓存命令(可在回发时找到原始命令)</param>
        public void request_net_cmmmand(CommandArgument command, bool cacheCommand = true)
        {
            if (command.cmdId == 0)
                command.cmdId = RpcEnvironment.NET_COMMAND_DATA_PUSH;
            Push(command);
        }

        /// <summary>
        ///     发送出错的重试处理
        /// </summary>
        /// <param name="callArg"></param>
        /// <param name="state"></param>
        private void TryRequest(CommandArgument callArg, int state)
        {
            callArg.cmdState = state;
            callArg.OnRequestStateChanged(callArg);
            if (callArg.tryNum >= RpcEnvironment.NET_COMMAND_RETRY_MAX)
            {
                callArg.cmdState = RpcEnvironment.NET_COMMAND_STATE_RETRY_MAX;
                callArg.OnRequestStateChanged(callArg);
            }
            else if (callArg.tryNum >= 0)
            {
                callArg.tryNum++;
                Push(callArg);
            }
        }
        #endregion
    }
}