using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    ///  事件返回命令泵
    /// </summary>
    public class ZmqAnswerPump : ZmqSubscribePumpBase
    {
        #region 消息泵
        
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
        protected sealed override void OptionSocktet(SubscriberSocket socket)
        {
            //用户识别
            socket.Options.Identity = RpcEnvironment.Token;
            //关闭设置停留时间,毫秒
            socket.Options.Linger = new TimeSpan(0, 0, 0, 0, 50);
        }
        /// <summary>
        /// 连接处理
        /// </summary>
        /// <param name="socket"></param>
        protected sealed override void OnConnected(SubscriberSocket socket)
        {
            //socket.Subscribe(RpcCore.Singleton.Config.Token);
        }

        /// <summary>
        /// 执行工作
        /// </summary>
        /// <param name="socket"></param>
        /// <returns>返回状态,其中-1会导致重连</returns>
        protected sealed override int DoWork(SubscriberSocket socket)
        {
            byte[] buffer;
            Debug.WriteLine(this.ZmqAddress);
            if (!socket.TryReceiveFrameBytes(timeOut, out buffer))
            {
                //Thread.SpinWait(1);
                return 0;
            }
            try
            {
                var reader = new CommandReader(buffer);
                reader.ReadCommandFromBuffer();
                var cmdMsg= reader.Command;
                try
                {
                    var old = RpcEnvironment.SyncCacheCommand(cmdMsg);
                    old?.OnRequestStateChanged(cmdMsg);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex, GetType().Name + "MessageNotifyTask");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex, GetType().Name + "DoWork");
                return -1;
            }
            return 0;
        }
        #endregion
        
    }

    
}