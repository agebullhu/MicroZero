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
        internal abstract bool OnExecuestEnd(ZSocket socket, ApiCallItem item, ZeroOperatorStateType state);

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        internal abstract void SendLayoutErrorResult(ZSocket socket, ApiCallItem item);

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
                            Priority=ThreadPriority.Highest
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
            var idx = (int) arg;
            var realName = $"{RealName}-{idx:D2}";
            DoPoll(realName, realName.ToAsciiBytes(), false);
        }

        /// <summary>
        /// 轮询
        /// </summary>
        private void DoPoll(string realName, byte[] identity, bool checkWait)
        {
            var executer = checkWait
                ? null
                : new ApiExecuter
                {
                    Station = this
                };
            ZeroTrace.SystemLog(StationName, "Task", "start", realName);
            using (var pool = PrepareLoop(identity, out var socket))
            {
                Hearter.HeartReady(StationName, realName);
                _processSemaphore?.Release();
                //int cnt = 0;
                while (CanLoop)
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
                            continue;
                        }

                        if (!CanLoop)
                            break;
                        if (pool.CheckIn(1, out var message))
                        {
                            message.Dispose();
                        }
                        if (!pool.CheckIn(0, out message))
                        {
                            continue;
                        }
                        Interlocked.Increment(ref RecvCount);
                        if (!ApiCallItem.Unpack(message, out var item))
                        {
                            SendLayoutErrorResult(socket, item);
                            continue;
                        }
                        Interlocked.Increment(ref WaitCount);
                        if (checkWait)
                            Execute(socket, item);
                        else
                            Execute(executer, ref socket, item);
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e);
                    }
                }
                Hearter.HeartLeft(StationName, realName);
            }
            ZeroTrace.SystemLog(StationName, "Task", "end", realName);
            _processSemaphore?.Release();
        }
        #endregion

        #region 方法

        private void Execute(ZSocket socket, ApiCallItem item)
        {
            if (WaitCount > ZeroApplication.Config.MaxWait)
            {
                item.Result = ApiResultIoc.UnavailableJson;
                OnExecuestEnd(socket, item, ZeroOperatorStateType.Unavailable);
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    var socketT = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER, ZSocket.CreateIdentity(false, StationName));
                    Execute(new ApiExecuter
                    {
                        Station = this
                    }, ref socketT, item);
                });
            }
        }

        private void Execute(ApiExecuter executer, ref ZSocket socket, ApiCallItem item)
        {
            if (PrepareExecute(item))
                executer.Execute(ref socket, item);
        }
        /// <summary>
        /// 准备执行
        /// </summary>
        /// <returns></returns>
        protected virtual bool PrepareExecute(ApiCallItem item)
        {
            return true;
        }

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
        public ApiAction RegistAction2<TArg, TResult>(string name, Func<TArg,TResult> action, ApiAccessOption access, ApiActionInfo info = null)
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
        /// <summary>
        ///     注册处理器
        /// </summary>
        internal List<IApiHandler> CreateHandlers()
        {
            List<IApiHandler> handlers = new List<IApiHandler>();
            foreach (var func in ApiHandlers)
                handlers.Add(func());
            return handlers;
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
