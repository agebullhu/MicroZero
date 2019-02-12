using System;
using System.Collections.Generic;
using System.Threading;
using Agebull.ZeroNet.Core.ZeroManagemant.StateMachine;
using Agebull.ZeroNet.PubSub;
using ZeroMQ;

namespace Agebull.ZeroNet.Core.ZeroManagemant
{
    /// <summary>
    /// 系统侦听器
    /// </summary>
    public class ZeroEventMonitor
    {
        #region 网络处理

        private readonly SemaphoreSlim TaskEndSem = new SemaphoreSlim(0);

        /// <summary>
        /// 等待正常结束
        /// </summary>
        internal void WaitMe()
        {
            if (ZeroApplication.WorkModel == ZeroWorkModel.Service)
                TaskEndSem.Wait();
        }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        internal void Monitor()
        {
            if (ZeroApplication.WorkModel != ZeroWorkModel.Service)
                return;
            using (OnceScope.CreateScope(ZeroApplication.Config))
            {
            }
            TaskEndSem.Release();
            ZeroTrace.SystemLog("Zero center in monitor...");
            while (ZeroApplication.IsAlive)
            {
                if (MonitorInner())
                    continue;
                ZeroTrace.WriteError("Zero center monitor failed", "There was no message for a long time");
                ZeroApplication.ZeroCenterState = ZeroCenterState.Failed;
                ZeroApplication.OnZeroEnd();
                ZeroApplication.ApplicationState = StationState.Failed;
                Thread.Sleep(1000);
            }
            ZeroTrace.SystemLog("Zero center monitor stoped!");
            TaskEndSem.Release();
        }

        #endregion

        private IMonitorStateMachine _stateMachine = new EmptyStateMachine();

        /// <summary>
        /// 状态机
        /// </summary>
        public IMonitorStateMachine StateMachine
        {
            get => _stateMachine;
            private set
            {
                _stateMachine?.Dispose();
                _stateMachine = value;
            }
        }
        /// <summary>
        /// 状态变更同步状态机
        /// </summary>
        public void OnApplicationStateChanged()
        {
            lock (this)
            {
                switch (ZeroApplication.ApplicationState)
                {
                    case StationState.None: // 刚构造
                    case StationState.ConfigError: // 配置错误
                    case StationState.Initialized: // 已初始化
                    case StationState.Start: // 正在启动
                    case StationState.BeginRun: // 开始运行
                    case StationState.Closing: // 将要关闭
                    case StationState.Closed: // 已关闭
                    case StationState.Destroy: // 已销毁，析构已调用
                    case StationState.Disposed: // 已销毁，析构已调用
                        if (/*_stateMachine != null && !_stateMachine.IsDisposed && */_stateMachine is EmptyStateMachine)
                            return;
                        StateMachine = new EmptyStateMachine();
                        return;
                    case StationState.Failed: // 错误状态
                        StateMachine = new FailedStateMachine();
                        return;
                    case StationState.Run: // 正在运行
                        StateMachine = new RuningStateMachine();
                        return;
                }
            }
        }

        /// <summary>
        ///     进入系统侦听
        /// </summary>
        private bool MonitorInner()
        {
            List<string> subs = new List<string>();
            long cnt = 0;
            DateTime failed = DateTime.MinValue;
            using (var poll = ZmqPool.CreateZmqPool())
            {
                if (ZeroApplication.Config.CanRaiseEvent != true)
                {
                    subs.Add("system");
                    subs.Add("station");
                }
                var socket = ZSocket.CreateSubSocket(ZeroApplication.Config.ZeroMonitorAddress, ZSocket.CreateIdentity(false, "Monitor"), subs);
                
                poll.Prepare(ZPollEvent.In, socket);
                while (ZeroApplication.IsAlive)
                {
                    if (!poll.Poll())
                    {
                        if (failed == DateTime.MinValue)
                            failed = DateTime.Now;
                        else if ((DateTime.Now - failed).TotalMinutes > 1)
                            return false;
                        continue;
                    }
                    if (!poll.CheckIn(0, out var message))
                    {
                        continue;
                    }
                    failed = DateTime.MinValue;
                    if (PublishItem.Unpack(message, out var item))
                    {
                        StateMachine?.OnMessagePush(item.ZeroEvent, item.SubTitle, item.Content);
                    }
                    cnt++;
                }
            }
            return true;
        }
    }
}