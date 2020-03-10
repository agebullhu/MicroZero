using System;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using ZeroMQ;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public class BridgeService
    {
        /// <summary>
        /// 实例
        /// </summary>
        public static BridgeService Instance = new BridgeService();

        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        public void Check()
        {
            try
            {
                if (_task != null)
                {
                    if (_task.Status != TaskStatus.Faulted)
                        return;
                    ZeroTrace.SystemLog("BridgeService", "restart");
                    _task.Dispose();
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
            Run();
        }

        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        public void Run()
        {
            ZeroApplication.Run();
            _task = Task.Factory.StartNew(RunInner, null, TaskCreationOptions.LongRunning);
            semaphore.Wait();
        }

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);
        private Task _task;
        /// <summary>
        ///     运行状态
        /// </summary>
        private int _state;

        /// <summary>
        ///     状态
        /// </summary>
        public int State
        {
            get => _state;
            set => Interlocked.Exchange(ref _state, value);
        }

        /// <summary>
        ///     执行直到连接成功
        /// </summary>
        public void Close()
        {
            switch (State)
            {
                case StationState.BeginRun:
                    State = StationState.Closing;
                    break;
                case StationState.Run:
                    State = StationState.Closing;
                    semaphore.Wait();
                    break;
            }
        }

        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        private void RunInner(object arg)
        {
            State = StationState.BeginRun;
            ZeroTrace.SystemLog("BridgeService", "run");
            var socketInner = ZSocket.CreateServiceSocket(ZeroApplication.Config.BridgeLocalAddress, ZSocketType.ROUTER);
            var socketCall = ZSocket.CreateServiceSocket($"tcp://*:{ZeroApplication.Config.BridgeCallAddress}", ZSocketType.PUSH);
            var socketResult = ZSocket.CreateServiceSocket($"tcp://*:{ZeroApplication.Config.BridgeResultAddress}", ZSocketType.PULL);

            var pool = ZmqPool.CreateZmqPool();
            pool.Prepare(ZPollEvent.In, socketInner, socketResult);
            State = StationState.Run;
            semaphore.Release();
            using (pool)
            {
                while (State == StationState.Run)
                {
                    try
                    {
                        if (!pool.Poll())
                        {
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e, "Poll");
                        continue;
                    }
                    try
                    {
                        if (pool.CheckIn(0, out var message))
                        {
                            using (message)
                            {
                                using (var nm = message.Duplicate())
                                {
                                    if (!socketCall.SendMessage(nm, out var err))
                                    {
                                        Thread.Sleep(10);
                                        using (var nm2 = message.Duplicate())
                                        {
                                            if (!socketInner.SendMessage(nm2, out err))
                                                LogRecorder.Error($"ReQueue(Call):{err.Text}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e, "CheckIn(Call)");
                    }
                    try
                    {
                        if (!pool.CheckIn(1, out var message))
                        {
                            continue;
                        }

                        using (message)
                        {
                            bool success = false;
                            for (int i = 0; i < 3; i++)
                            {
                                using (var nm = message.Duplicate())
                                {
                                    if (socketInner.SendMessage(nm, out _))
                                    {
                                        success = true;
                                        break;
                                    }
                                    Thread.Sleep(10);
                                }
                            }

                            if (success)
                                continue;
                            LogRecorder.Error("CheckIn(Result):can`t send result");
                        }
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e, "CheckIn(Result)");
                    }
                }
            }
            State = StationState.Closing;
            socketCall.Dispose();
            socketResult.Dispose();
            socketInner.Dispose();
            _task = null;
            State = StationState.Closed;
            ZeroTrace.SystemLog("BridgeService", "end");
            semaphore.Release();
        }
    }
}