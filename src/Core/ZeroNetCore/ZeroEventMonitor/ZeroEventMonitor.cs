using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agebull.MicroZero.PubSub;
using ZeroMQ;

namespace Agebull.MicroZero.ZeroManagemant
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    public class ZeroEventMonitor
    {
        internal readonly SemaphoreSlim TaskEndSem = new SemaphoreSlim(0);

        /// <summary>
        /// 等待正常结束
        /// </summary>
        internal async Task WaitMe()
        {
            if (ZeroApplication.WorkModel == ZeroWorkModel.Service)
                await TaskEndSem.WaitAsync();
        }

        /// <summary>
        ///     进入系统分布式消息侦听处理
        /// </summary>
        internal async Task Monitor()
        {
            ZeroTrace.SystemLog("Zero center in monitor...");
            await Task.Yield();
            List<string> subs = new List<string>();
            if (ZeroApplication.Config.CanRaiseEvent != true)
            {
                subs.Add("system");
                subs.Add("station");
            }
            while (ZeroApplication.IsAlive)
            {
                DateTime failed = DateTime.MinValue;
                using var poll = ZmqPool.CreateZmqPool();
                var socket = ZSocketEx.CreateSubSocket(ZeroApplication.Config.Master.MonitorAddress, ZeroApplication.Config.Master.ServiceKey.ToZeroBytes(), ZSocket.CreateIdentity(false, "Monitor"), subs);

                poll.Prepare(ZPollEvent.In, socket);
                while (ZeroApplication.IsAlive)
                {
                    if (await poll.PollAsync())
                    {
                        var message = await poll.CheckInAsync(0);
                        if (message == null)
                            continue;
                        failed = DateTime.MinValue;
                        if (PublishItem.Unpack(message, out var item) && MonitorStateMachine.StateMachine != null)
                        {
                            await MonitorStateMachine.StateMachine.OnMessagePush(item.ZeroEvent, item.SubTitle, item.Content);
                        }
                    }
                    else if (failed == DateTime.MinValue)
                        failed = DateTime.Now;
                    else if ((DateTime.Now - failed).TotalMinutes > 1)
                    {
                        //超时，连接重置
                        ZeroTrace.WriteError("Zero center event monitor failed,there was no message for a long time");
                        ZeroApplication.ZeroCenterState = ZeroCenterState.Failed;
                        await ZeroApplication.OnZeroEnd();
                        if (ZeroApplication.ApplicationState != StationState.Failed)
                            ZeroApplication.ApplicationState = StationState.Failed;
                        Thread.Sleep(500);
                        break;
                    }
                }
            }
            ZeroTrace.SystemLog("Zero center monitor stoped!");
            TaskEndSem.Release();
        }
    }
}