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
        #region 网络处理

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
        ///     进入系统侦听
        /// </summary>
        internal async Task Monitor()
        {
            if (ZeroApplication.WorkModel != ZeroWorkModel.Service)
                return;
            //TaskEndSem.Release();
            await Task.Yield();
            ZeroTrace.SystemLog("Zero center in monitor...");
            while (ZeroApplication.IsAlive)
            {
                if (await MonitorInner())
                    continue;
                ZeroTrace.WriteError("Zero center monitor failed", "There was no message for a long time");
                ZeroApplication.ZeroCenterState = ZeroCenterState.Failed;
                await ZeroApplication.OnZeroEnd();
                Thread.Sleep(500);
                if (ZeroApplication.ApplicationState != StationState.Failed)
                    ZeroApplication.ApplicationState = StationState.Failed;
            }
            ZeroTrace.SystemLog("Zero center monitor stoped!");
            TaskEndSem.Release();
        }

        #endregion


        /// <summary>
        ///     进入系统侦听
        /// </summary>
        private async Task<bool> MonitorInner()
        {
           await Task.Yield();
            List<string> subs = new List<string>();
            DateTime failed = DateTime.MinValue;
            using (var poll = ZmqPool.CreateZmqPool())
            {
                if (ZeroApplication.Config.CanRaiseEvent != true)
                {
                    subs.Add("system");
                    subs.Add("station");
                }
                var socket = ZSocket.CreateSubSocket(ZeroApplication.Config.Master.MonitorAddress, ZSocket.CreateIdentity(false, "Monitor"), subs);

                poll.Prepare(ZPollEvent.In, socket);
                while (ZeroApplication.IsAlive)
                {
                    if (!await poll.PollAsync())
                    {
                        if (failed == DateTime.MinValue)
                            failed = DateTime.Now;
                        else if ((DateTime.Now - failed).TotalMinutes > 1)
                            return false;
                        continue;
                    }

                    var message = await poll.CheckInAsync(0);
                    if (message == null)
                    {
                        continue;
                    }
                    failed = DateTime.MinValue;
                    if (PublishItem.Unpack(message, out var item) && MonitorStateMachine.StateMachine != null)
                    {
                        await MonitorStateMachine.StateMachine.OnMessagePush(item.ZeroEvent, item.SubTitle, item.Content);
                    }
                }
            }
            return true;
        }
    }
}