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
            if (_task.Status != TaskStatus.Faulted)
                return;
            _task.Dispose();
            Run();
        }

        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        public void Run()
        {
            ZeroApplication.Run();
            _task = Task.Factory.StartNew(RunInner);
        }

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
        ///     运行状态（本地与服务器均正常）
        /// </summary>
        public bool CanLoop => ZeroApplication.CanDo && (State == StationState.BeginRun || State == StationState.Run);

        /// <summary>
        ///     执行直到连接成功
        /// </summary>
        public void Close()
        {
            if (State == StationState.BeginRun || State == StationState.Run)
                State = StationState.Closing;

        }

        private ZSocket socketInner, socketCall, socketResult;
        private IZmqPool pool;
        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        bool RunInner()
        {
            State = StationState.BeginRun;
            ZeroTrace.SystemLog("BridgeService", "run");
            socketCall?.Dispose();
            socketCall?.Dispose();
            socketResult?.Dispose();
            socketInner = ZSocket.CreateServiceSocket(ZeroApplication.Config.BridgeLocalAddress, ZSocketType.ROUTER);//收
            socketCall = ZSocket.CreateServiceSocket($"tcp://*:{ZeroApplication.Config.BridgeCallAddress}", ZSocketType.PUSH);//推
            socketResult = ZSocket.CreateServiceSocket($"tcp://*:{ZeroApplication.Config.BridgeResultAddress}", ZSocketType.PULL);//拉

            pool?.Dispose();
            pool = ZmqPool.CreateZmqPool();
            pool.Prepare(ZPollEvent.In, socketInner, socketResult);
            State = StationState.Run;

            using (pool)
            {
                while (CanLoop)
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
                        LogRecorder.Exception(e, "pool");
                        Console.WriteLine(e.Source);
                        Console.WriteLine(e.StackTrace);
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
                                        Console.WriteLine(err.Text);
                                }
                            }
                        }
                        if (pool.CheckIn(1, out message))
                        {
                            using (message)
                            {
                                Console.WriteLine(message);
                                using (var nm = message.Duplicate())
                                {
                                    if (!socketInner.SendMessage(nm, out var err))
                                        Console.WriteLine(err.Text);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e, "CheckIn");
                        Console.WriteLine(e.Source);
                        Console.WriteLine(e.StackTrace);
                    }
                }
            }
            State = StationState.Closed;

            socketCall.Dispose();
            socketResult.Dispose();
            socketInner.Dispose();
            socketInner = socketCall = socketResult = null;
            pool = null;
            ZeroTrace.SystemLog("BridgeService", "end");
            _task = null;
            return true;
        }
    }
}