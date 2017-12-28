using System;
using System.Diagnostics;
using System.Threading;
using Agebull.Common.Logging;
using NetMQ;
using NetMQ.Sockets;
using Yizuan.Service.Api;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    ///     问答式命令泵
    /// </summary>
    public class ZmqHeartbeatPump : ZmqCommandPump<RequestSocket>
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }
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
            socket.Options.Identity = ServiceName.ToAsciiBytes();
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
            Thread.Sleep(5000);
            var result = SendCommand(socket,"MAMA", ApiContext.MyAddress,true);
            return result == "Failed" || "Exception" == result ? -1 : 0;
        }

        /// <summary>
        /// 连接处理
        /// </summary>
        /// <param name="socket"></param>
        protected sealed override void OnDeConnected(RequestSocket socket)
        {
            var result =  SendCommand(socket, "LAOWANG", ApiContext.MyAddress, true);
            Console.WriteLine(result);
        }
    }
}