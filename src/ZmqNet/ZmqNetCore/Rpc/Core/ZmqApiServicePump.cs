using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using NetMQ;
using NetMQ.Sockets;
using Yizuan.Service.Api;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    ///     问答式命令泵
    /// </summary>
    public class ZmqApiServicePump : ZmqCommandPump<RequestSocket>
    {
        #region 消息泵

        public ZmqHeartbeatPump Heartbeat { get; private set; }
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName => $"{ApiContext.MyServiceName}:{Thread.CurrentThread.ManagedThreadId}";

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
        /// 清理
        /// </summary>
        protected sealed override void DoDispose()
        {
            Heartbeat.Dispose();
        }

        /// <summary>
        /// 连接处理
        /// </summary>
        /// <param name="socket"></param>
        protected override void OnConnected(RequestSocket socket)
        {
            var result = SendCommand(socket, "READY", ApiContext.MyAddress, false);
            Console.WriteLine($"OnConnected => {ServiceName}:{result}");
            Heartbeat = new ZmqHeartbeatPump
            {
                ServiceName = ServiceName,
                ZmqAddress = RouteRpc.Singleton.Config.HeartbeatUrl
            };
            Heartbeat.Initialize();
            Heartbeat.Run();
        }

        /// <summary>
        /// 配置Socket对象
        /// </summary>
        /// <param name="socket"></param>
        protected sealed override void OptionSocktet(RequestSocket socket)
        {
            //用户识别
            socket.Options.Identity = ServiceName.ToAsciiBytes();
            //列消息只作用于已完成的链接
            //关闭设置停留时间,毫秒
            socket.Options.Linger = new TimeSpan(0, 0, 0, 0, 50);
            socket.Options.ReceiveLowWatermark = 500;
            socket.Options.SendLowWatermark = 500;
        }

        public class CallArgument
        {
            public RequestSocket socket;
            public string client;
            public string callArg;
        }
        /// <summary>
        /// 执行工作
        /// </summary>
        /// <param name="socket"></param>
        /// <returns>返回状态,其中-1会导致重连</returns>
        protected sealed override int DoWork(RequestSocket socket)
        {
            try
            {
                CallArgument argument = new CallArgument
                {
                    socket = socket
                };
                bool more;
                bool state = socket.TryReceiveFrameString(timeOut, out argument.client, out more);
                if (!state)
                {
                    return 0; //超时
                }
                if (!more)
                {
                    return -1; //出错了
                }
                string empty;
                state = socket.TryReceiveFrameString(timeOut, out empty, out more);

                if (!state)
                {
                    return 0; //超时
                }
                if (!more)
                {
                    return -1; //出错了
                }
                state = socket.TryReceiveFrameString(timeOut, out argument.callArg, out more);
                while (more)
                {
                    if (!state)
                    {
                        return 0; //超时
                    }
                    string str;
                    state = socket.TryReceiveFrameString(timeOut, out str, out more);
                }
                OnClientCall(argument);
            }
            catch (Exception ex)
            {
                OnException(socket, ex);
                return -1;
            }
            return 1;
        }

        #endregion

        #region 命令请求
        /// <summary>
        /// 有命令请求时调用
        /// </summary>
        /// <param name="arg"></param>
        protected void OnClientCall(object arg)
        {
            CallArgument argument = (CallArgument)arg;
            string result = OnCall(argument);
            if (RpcEnvironment.NetState != ZmqNetStatus.Runing)
            {
                LogRecorder.Warning($"来自{argument.client}的请求操作已完成，但网络连接已关闭，无法返回。参数为{argument.callArg}");
                return;
            }
            argument.socket.TrySendFrame(timeOut, argument.client, true);
            argument.socket.TrySendFrameEmpty(timeOut, true);
            argument.socket.TrySendFrame(timeOut, result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        protected virtual string OnCall(CallArgument argument)
        {
            return $"{{\"Result\":false,\"Id\":{Thread.CurrentThread.ManagedThreadId}}}";
        }

        #endregion
    }
}