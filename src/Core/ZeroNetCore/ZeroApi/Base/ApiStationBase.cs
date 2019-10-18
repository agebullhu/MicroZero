using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agebull.MicroZero.ApiDocuments;
using Agebull.Common.Logging;
using Agebull.MicroZero.ZeroManagemant;
using ZeroMQ;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public abstract class ApiStationBase : ZeroStation
    {
        #region 流程

        private readonly string _typeName;
        /// <summary>
        /// 构造
        /// </summary>
        protected ApiStationBase(ZeroStationType type, bool isService) : base(type, isService)
        {
            _typeName = StationConfig.TypeName(type);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected sealed override StationConfig OnNofindConfig()
        {
            if (!SystemManager.Instance.TryInstall(StationName, _typeName))
                return null;
            var config = SystemManager.Instance.LoadConfig(StationName);
            if (config == null)
                return null;
            config.State = ZeroCenterState.Run;
            ZeroTrace.SystemLog(StationName, "successfully");
            return config;
        }

        /// <summary>
        ///     配置校验
        /// </summary>
        protected abstract ZeroStationOption GetApiOption();

        /// <summary>
        /// 构造Pool
        /// </summary>
        /// <returns></returns>
        protected abstract IZmqPool PrepareLoop(byte[] identity, out ZSocket socket);

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal virtual bool OnExecuestEnd(ZSocket socket, ApiCallItem item, ZeroOperatorStateType state)
        {
            int i = 0;
            var des = new byte[10 + item.Originals.Count];
            des[i++] = (byte)(item.Originals.Count + (item.EndTag == ZeroFrameType.ResultFileEnd ? 7 : 6));
            des[i++] = (byte)state;
            des[i++] = ZeroFrameType.Requester;
            des[i++] = ZeroFrameType.RequestId;
            des[i++] = ZeroFrameType.CallId;
            des[i++] = ZeroFrameType.GlobalId;
            des[i++] = ZeroFrameType.ResultText;
            var msg = new List<byte[]>
            {
                item.Caller,
                des,
                item.Requester.ToZeroBytes(),
                item.RequestId.ToZeroBytes(),
                item.CallId.ToZeroBytes(),
                item.GlobalId.ToZeroBytes(),
                item.Result.ToZeroBytes()
            };
            if (item.EndTag == ZeroFrameType.ResultFileEnd)
            {
                des[i++] = ZeroFrameType.BinaryContent;
                msg.Add(item.Binary);
            }
            foreach (var org in item.Originals)
            {
                des[i++] = org.Key;
                msg.Add((org.Value));
            }
            des[i++] = ZeroFrameType.SerivceKey;
            msg.Add(ZeroCommandExtend.ServiceKeyBytes);
            des[i] = item.EndTag > 0 ? item.EndTag : ZeroFrameType.ResultEnd;
            return SendResult(socket, new ZMessage(msg));
        }

        private static readonly byte[] LayoutErrorFrame = new byte[]
        {
            2,
            (byte) ZeroOperatorStateType.FrameInvalid,
            ZeroFrameType.Requester,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.ResultEnd
        };

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        internal virtual void SendLayoutErrorResult(ZSocket socket, ApiCallItem item)
        {
            //if (!CanLoop)
            //{
            //    ZeroTrace.WriteError(StationName, "Can`t send result,station is closed");
            //    return;
            //}
            if (item == null)
                return;
            SendResult(socket, new ZMessage
            {
                new ZFrame(item.Caller),
                new ZFrame(LayoutErrorFrame),
                new ZFrame(item.Requester),
                new ZFrame(ZeroCommandExtend.ServiceKeyBytes)
            });
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected bool SendResult(ZSocket socket, ZMessage message)
        {
            //if (!CanLoop)
            //{
            //    ZeroTrace.WriteError(StationName, "Can`t send result,station is closed");
            //    LogRecorderX.MonitorTrace($"{StationName} : Can`t send result,station is closed");
            //    return false;
            //}

            try
            {
                ZError error;
                lock (socket)
                {
                    using (message)
                    {
                        if (socket.Send(message, out error))
                            return true;
                    }
                }

                ZeroTrace.WriteError(StationName, error.Text, error.Name);
                LogRecorderX.MonitorTrace($"{StationName}({error.Name}) : {error.Text}");
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e, "ApiStation.SendResult");
                LogRecorderX.MonitorTrace(e.Message);
            }
#if UNMANAGE_MONEY_CHECK
            finally
            {
                LogRecorderX.MonitorTrace($"MemoryCheck:{MemoryCheck.AliveCount}");
            }
#endif
            Interlocked.Increment(ref SendError);
            return false;
        }
        #endregion

        #region 主循环

        /// <summary>
        /// 调用计数
        /// </summary>
        public int CallCount, ErrorCount, SuccessCount, RecvCount, SendCount, SendError, WaitCount;


        /// <summary>
        /// 轮询
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected sealed override bool Loop(/*CancellationToken token*/)
        {
            WaitCount = 0;
            var option = GetApiOption();
            int max = 1;
            switch (option.SpeedLimitModel)
            {
                case SpeedLimitType.ThreadCount:
                    max = (int)(Environment.ProcessorCount * option.TaskCpuMultiple);
                    if (max < 1)
                        max = 1;
                    _processSemaphore = new SemaphoreSlim(0, max);
                    ZeroTrace.SystemLog(StationName, "ThreadCount", max);
                    for (int idx = 1; idx <= max; idx++)
                        new Thread(RunThread)
                        {
                            IsBackground = true,
                            Priority = ThreadPriority.Highest
                        }.Start(idx);
                    break;
                case SpeedLimitType.WaitCount:
                    _processSemaphore = new SemaphoreSlim(0, max);
                    ZeroTrace.SystemLog(StationName, "WaitCount", ZeroApplication.Config.MaxWait);
                    new Thread(RunWaitCount)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Highest
                    }.Start();
                    break;
                default:
                    _processSemaphore = new SemaphoreSlim(0, max);
                    ZeroTrace.SystemLog(StationName, "Single");
                    new Thread(RunSingle)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Highest
                    }.Start();
                    break;
            }
            RealState = StationState.BeginRun;
            //第一次全部运行
            for (int idx = 0; idx < max; idx++)
                _processSemaphore.Wait();
            RealState = StationState.Run;
            //第二次全部关闭
            for (int idx = 0; idx < max; idx++)
                _processSemaphore.Wait();
            return true;
        }

        private SemaphoreSlim _processSemaphore;

        /// <summary>
        /// 轮询
        /// </summary>
        private void RunSingle(object arg)
        {
            DoPoll(RealName, Identity, false);
        }
        /// <summary>
        /// 轮询
        /// </summary>
        private void RunWaitCount(object arg)
        {
            DoPoll(RealName, Identity, true);
        }
        /// <summary>
        /// 轮询
        /// </summary>
        private void RunThread(object arg)
        {
            var idx = (int)arg;
            var realName = $"{RealName}-{idx:D2}";
            DoPoll(realName, realName.ToAsciiBytes(), false);
        }

        /// <summary>
        /// 轮询
        /// </summary>
        private void DoPoll(string realName, byte[] identity, bool checkWait)
        {
            //var executer = checkWait
            //    ? null
            //    : new ApiExecuter
            //    {
            //        Station = this
            //    };
            ZeroTrace.SystemLog(StationName, "Task", "start", realName);
            using (var pool = PrepareLoop(identity, out var socket))
            {
                Hearter.HeartReady(StationName, realName);
                _processSemaphore?.Release();
                //int cnt = 0;
                while (CanLoop)
                {
                    DoPollMessage(checkWait, pool, socket);
                }
                pool.Sockets[0].Disconnect(Config.WorkerCallAddress);
                Hearter.HeartLeft(StationName, realName);
                ZeroTrace.SystemLog(StationName, "closing");
                try
                {
                    while (DoPollMessage(checkWait, pool, socket))
                    {
                        Console.WriteLine("处理堆积任务");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"处理堆积任务{e.Message}");
                }
                while(WaitCount > 0)
                    Thread.Sleep(10);
            }
            ZeroTrace.SystemLog(StationName, "Task", "end", realName);
            _processSemaphore?.Release();
        }

        private bool DoPollMessage(bool checkWait, IZmqPool pool, ZSocket socket)
        {
            try
            {
                if (!pool.Poll())
                {
                    //cnt += pool.TimeoutMs;
                    OnLoopIdle();
                    //if (CanLoop && cnt >= 2000)
                    //{
                    //    hearter.Heartbeat(StationName, realName);
                    //    cnt = 0;
                    //}
                    return false;
                }

                if (pool.CheckIn(1, out var message)) //对Result端口的返回的丢弃处理
                {
                    message.Dispose();
                }

                if (!pool.CheckIn(0, out message))
                {
                    message?.Dispose();
                    return false;
                }
                Interlocked.Increment(ref RecvCount);
                if (checkWait)
                {
                    Task.Factory.StartNew(ExecuteAsync, new TaskArg
                    {
                        Socket = socket,
                        Messages = message
                    }, TaskCreationOptions.PreferFairness | TaskCreationOptions.DenyChildAttach);
                }
                else
                {
                    Execute(socket, message);
                }
            }
            catch (Exception e)
            {
                LogRecorderX.Exception(e);
            }

            return true;
        }

        #endregion

        #region 方法

        #region 调用

        class TaskArg
        {
            public ZSocket Socket;
            public ZMessage Messages;
        }

        private void ExecuteAsync(object arg)
        {
            var targ = (TaskArg)arg;
            Interlocked.Increment(ref CallCount);
            if (!ApiCallItem.Unpack(targ.Messages, out var item))
            {
                SendLayoutErrorResult(targ.Socket, item);
                return;
            }
            if (WaitCount > ZeroApplication.Config.MaxWait)
            {
                item.Result = ApiResultIoc.UnavailableJson;
                OnExecuestEnd(targ.Socket, item, ZeroOperatorStateType.Unavailable);
                return;
            }
            ExecuteAsync(targ.Socket, item);
        }

        private void ExecuteAsync(ZSocket socket, ApiCallItem item)
        {
            Interlocked.Increment(ref WaitCount);
            try
            {
                if (!PrepareExecute(item))
                    return;
                var executer = new ApiExecuter
                {
                    Station = this,
                    Item = item,
                    Socket = socket
                };
                executer.ExecuteAsync();
            }
            finally
            {
                Interlocked.Decrement(ref WaitCount);
            }
        }

        private void Execute(ZSocket socket, ZMessage message)
        {
            Interlocked.Increment(ref CallCount);
            if (!ApiCallItem.Unpack(message, out var item))
            {
                SendLayoutErrorResult(socket, item);
                return;
            }
            Interlocked.Increment(ref WaitCount);
            try
            {
                if (!PrepareExecute(item))
                    return;
                var executer = new ApiExecuter
                {
                    Station = this,
                    Item = item,
                    Socket = socket
                };
                executer.Execute();
            }
            finally
            {
                Interlocked.Decrement(ref WaitCount);
            }
        }

        /// <summary>
        /// 准备执行
        /// </summary>
        /// <returns></returns>
        protected virtual bool PrepareExecute(ApiCallItem item)
        {
            return true;
        }

        #endregion

        #region 注册方法

        internal readonly Dictionary<string, ApiAction> ApiActions = new Dictionary<string, ApiAction>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="info">反射信息</param>
        public void RegistAction(string name, ApiAction action, ApiActionInfo info = null)
        {
            if (!ApiActions.ContainsKey(name))
            {
                action.Name = name;
                ApiActions.Add(name, action);
            }
            else
            {
                ApiActions[name] = action;
            }

            ZeroTrace.SystemLog(StationName,
                info == null
                    ? name
                    : info.Controller == null
                        ? $"{name}({info.Name})"
                        : $"{name}({info.Controller}.{info.Name})");
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="info">反射信息</param>
        /// <param name="access">访问设置</param>
        public ApiAction RegistAction(string name, Func<IApiResult> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiAction<IApiResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);
            return a;
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        /// <param name="info">反射信息</param>
        public ApiAction RegistAction(string name, Func<object, object> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiActionObj
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);
            return a;
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        /// <param name="info">反射信息</param>
        public ApiAction RegistAction(string name, Func<object> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiActionObj2
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);
            return a;
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        /// <param name="info">反射信息</param>
        public ApiAction RegistAction<TResult>(string name, Func<TResult> action, ApiAccessOption access, ApiActionInfo info = null)
            where TResult : IApiResult
        {
            var a = new ApiAction<TResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);
            return a;
        }
        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        /// <param name="info">反射信息</param>
        public ApiAction RegistAction2<TResult>(string name, Func<TResult> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiAction2<TResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);
            return a;
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        /// <param name="info">反射信息</param>
        public ApiAction RegistAction2<TArg, TResult>(string name, Func<TArg, TResult> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiAction2<TArg, TResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);
            return a;
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        /// <param name="info">反射信息</param>
        public ApiAction RegistAction(string name, Func<IApiArgument, IApiResult> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiAction<IApiArgument, IApiResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);

            return a;
        }
        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        /// <param name="info">反射信息</param>
        public ApiAction RegistAction<TArgument, TResult>(string name, Func<TArgument, TResult> action, ApiAccessOption access, ApiActionInfo info = null)
            where TArgument : class, IApiArgument
            where TResult : IApiResult
        {
            var a = new ApiAction<TArgument, TResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a, info);

            return a;
        }

        #endregion

        #region 扩展

        /// <summary>
        /// 处理器
        /// </summary>
        private static readonly List<Func<IApiHandler>> ApiHandlers = new List<Func<IApiHandler>>();

        /// <summary>
        ///     注册处理器
        /// </summary>
        public static void RegistHandlers<TApiHandler>() where TApiHandler : class, IApiHandler, new()
        {
            ApiHandlers.Add(() => new TApiHandler());
        }

        private List<IApiHandler> _handlers;

        /// <summary>
        ///     注册处理器
        /// </summary>
        internal List<IApiHandler> CreateHandlers()
        {
            if (ApiHandlers.Count == 0)
                return null;
            if (_handlers != null)
                return _handlers;
            _handlers = new List<IApiHandler>();
            foreach (var func in ApiHandlers)
                _handlers.Add(func());
            return _handlers;
        }

        #endregion


        #endregion
    }
}


/*/readonly TaskQueue<ApiCallItem> Quote = new TaskQueue<ApiCallItem>();

#region 限定Task数量模式


/// <inheritdoc />
protected override void OnRunStop()
{
    //if (ZeroApplication.Config.SpeedLimitModel == SpeedLimitType.ThreadCount)
    //    _processSemaphore.Wait();
    //if (ZContext.IsAlive)
    //{
    //    while (!Quote.IsEmpty)
    //    {
    //        var t = Quote.Queue.Dequeue();
    //        t.Result = ZeroStatuValue.UnavailableJson;
    //        Interlocked.Increment(ref CallCount);
    //        Interlocked.Increment(ref ErrorCount);
    //        SendResult(ref _resultSocket, t, false);
    //    }
    //}

    base.OnRunStop();
}

//void ProcessTask(object obj)
//{
//    Interlocked.Increment(ref ptocessTaskCount);
//    var socket = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER);

//    var pool = ZmqPool.CreateZmqPool();
//    pool.Prepare(new[] { ZSocket.CreateClientSocket($"inproc://{StationName}_api.route", ZSocketType.PAIR) }, ZPollEvent.In);
//    var token = (CancellationToken)obj;
//    while (!token.IsCancellationRequested && CanRun)
//    {
//        //if (!Quote.StartProcess(out var item))
//        //{
//        //    continue;
//        //}
//        //ApiCall(ref socket, item);
//        //Quote.EndProcess();

//        if (!pool.Poll() || !pool.CheckIn(0, out var message))
//        {
//            continue;
//        }

//        using (message)
//        {
//            if (!Unpack(message, out var item))
//            {
//                SendLayoutErrorResult(ref socket, item.Caller, item.Requester);
//                continue;
//            }
//            ApiCall(ref socket, item);
//        }
//    }
//    socket.TryClose();
//    if (Interlocked.Decrement(ref ptocessTaskCount) == 0)
//        _processSemaphore.Release();
//}
#endregion*/
