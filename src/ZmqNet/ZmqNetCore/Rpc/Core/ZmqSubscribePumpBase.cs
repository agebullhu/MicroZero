using System;
using System.Diagnostics;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    /// 订阅命令泵
    /// </summary>
    public abstract class ZmqSubscribePumpBase : ZmqCommandPump<SubscriberSocket>
    {
        #region 消息泵
        /// <summary>
        /// 生成SOCKET对象
        /// </summary>
        /// <remarks>版本不兼容导致改变</remarks>
        protected override SubscriberSocket CreateSocket()
        {
            return new SubscriberSocket();
        }

        /// <summary>
        /// 配置Socket对象
        /// </summary>
        /// <param name="socket"></param>
        protected override void OptionSocktet(SubscriberSocket socket)
        {
            //用户识别
            socket.Options.Identity = RpcEnvironment.Token;
            //关闭设置停留时间,毫秒
            socket.Options.Linger = new TimeSpan(0, 0, 0, 0, 50);
        }
        
        #endregion

    }
}