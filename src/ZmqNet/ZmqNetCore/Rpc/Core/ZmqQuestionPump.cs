using System;
using System.Diagnostics;
using NetMQ;
using NetMQ.Sockets;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    ///     问答式命令泵
    /// </summary>
    public class ZmqQuestionPump : ZmqCommandPump<RequestSocket>
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
            socket.Options.Identity = RpcEnvironment.Token;
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
                callArg.cmdState = RpcEnvironment.NET_COMMAND_STATE_SENDED;
                //序列化
                var writer = new CommandWriter(callArg);
                writer.WriteCommandToBuffer();
                //发送命令请求
                bool state = socket.TrySendFrame(timeOut, writer.Buffer, writer.DataLen);
                if (!state)
                {
                    TryRequest(callArg, RpcEnvironment.NET_COMMAND_STATE_NETERROR);
                    return -1; //出错了
                }
                //log_debug4(DEBUG_CALL, 3, "%s:命令%d(%s)发送成功(%d)", address, call_arg.cmd_id, call_arg.cmd_identity, zmq_result);
                //接收处理反馈
                byte[] result;
                state = socket.TryReceiveFrameBytes(timeOut, out result);
                if (!state)
                {
                    TryRequest(callArg, RpcEnvironment.NET_COMMAND_STATE_UNKNOW);
                    return -1; //出错了
                }
                if (result[0] == '0')
                {
                    TryRequest(callArg, RpcEnvironment.NET_COMMAND_STATE_UNKNOW);
                }
                else
                {
                    callArg.cmdState = RpcEnvironment.NET_COMMAND_STATE_WAITING;
                    callArg.OnRequestStateChanged(callArg);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex, GetType().Name + "DoWork");
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