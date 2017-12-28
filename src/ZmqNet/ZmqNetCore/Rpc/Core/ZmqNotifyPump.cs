using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace Agebull.Zmq.Rpc
{
    /// <summary>
    ///  消息订阅命令泵
    /// </summary>
    public class ZmqNotifyPump : ZmqSubscribePumpBase
    {
        #region 消息泵

        /// <summary>
        /// 启动泵任务
        /// </summary>
        protected override void DoRun()
        {
            Task.Factory.StartNew(MessageNotifyTask);
            base.DoRun();
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
            socket.Subscribe(EventWorkerContainer.SubName ?? "");
        }

        /// <summary>
        /// 执行工作
        /// </summary>
        /// <param name="socket"></param>
        /// <returns>返回状态,其中-1会导致重连</returns>
        protected sealed override int DoWork(SubscriberSocket socket)
        {
            byte[] buffer;
            if (!socket.TryReceiveFrameBytes(timeOut, out buffer))
            {
                //Thread.SpinWait(1);
                return 0;
            }
            try
            {
                var reader = new CommandReader(buffer);
                reader.ReadCommandFromBuffer();
                Console.WriteLine("收到消息" + reader.Command.Data.GetType());
                Push(reader.Command);

                //Task.Factory.StartNew(ProcessMessage, reader.Command);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex, GetType().Name + "DoWork");
                return -1;
            }
            return 0;
        }
        #endregion

        #region 事件反馈

        /// <summary>
        ///     消息通知发送Task
        /// </summary>
        private void MessageNotifyTask()
        {
            if (EventWorkerContainer.CreateWorker == null)
                return;
            //log_msg("通知消息泵已启动");
            while (RpcEnvironment.NetState != ZmqNetStatus.Destroy)
            {
                CommandArgument cmdMsg = Pop();
                if (cmdMsg == null)
                    continue;
                Task.Factory.StartNew(ProcessMessage, cmdMsg);
            }
            //log_msg("通知消息泵已关闭");
        }
        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessage(object state)
        {
            CommandArgument cmd = (CommandArgument)state;
            var data = (StringArgumentData)cmd.Data;
            IEventWorker worker = EventWorkerContainer.CreateWorker();
            var arg = worker.ToArgument(cmd, data.Argument);
            using (RpcContextScope.CreateScope(arg))
            {
                try
                {
                    worker.Process(cmd.commandName, arg);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    if (cmd.cmdId == RpcEnvironment.NET_COMMAND_CALL)
                        RpcProxy.Result(new CommandResult
                        {
                            Status = RpcEnvironment.NET_COMMAND_STATE_SERVER_UNKNOW,
                            Message = e.Message
                        });
                }
            }
        }

        #endregion
    }

}