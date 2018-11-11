using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.ApiDocuments;
using Agebull.Common.Logging;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Gboxt.Common.DataModel;
using ZeroMQ;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public abstract class ApiStationBase : ZeroStation
    {
        #region 注册方法

        internal readonly Dictionary<string, ApiAction> ApiActions =
            new Dictionary<string, ApiAction>(StringComparer.OrdinalIgnoreCase);


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
                info != null
                    ? $"{name}({info.Controller}.{info.Name}) is registed."
                    : $"{name} is registed");
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
        public ApiAction RegistAction<TResult>(string name, Func<TResult> action, ApiAccessOption access, ApiActionInfo info = null)
            where TResult : ApiResult
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
        public ApiAction RegistAction<TArgument, TResult>(string name, Func<TArgument, TResult> action, ApiAccessOption access, ApiActionInfo info = null)
            where TArgument : class, IApiArgument
            where TResult : ApiResult
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

        #region Api调用

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
        /// <summary>
        /// 构造
        /// </summary>
        protected ApiStationBase(ZeroStationType type, bool isService) : base(type, isService)
        {

        }

        #endregion

        #region 网络与执行

        /// <summary>
        /// 调用计数
        /// </summary>
        public int CallCount, ErrorCount, SuccessCount, RecvCount, SendCount, SendError, WaitCount;


        /// <summary>
        /// 初始化
        /// </summary>
        protected override void Initialize()
        {
            if (Name == null)
                Name = StationName;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected sealed override bool OnNofindConfig()
        {
            if (!SystemManager.Instance.TryInstall(StationName, "api"))
                return false;
            Config = SystemManager.Instance.LoadConfig(StationName);
            if (Config == null)
            {
                return false;
            }
            Config.State = ZeroCenterState.Run;
            ZeroTrace.SystemLog(StationName, "successfully");
            return true;
        }

        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected sealed override bool RunInner(CancellationToken token)
        {
            WaitCount = 0;
            SystemManager.Instance.HeartReady(StationName, RealName);
            var option = GetApiOption();
            switch (option.SpeedLimitModel)
            {
                case SpeedLimitType.ThreadCount:
                    int max = (int)(Environment.ProcessorCount * option.TaskCpuMultiple);
                    if (max < 1)
                        max = 1;
                    _processSemaphore = new SemaphoreSlim(0, max);
                    for (int idx = 0; idx < max; idx++)
                        Task.Factory.StartNew(RunThread);

                    for (int idx = 0; idx < max; idx++)
                        _processSemaphore.Wait();
                    break;
                case SpeedLimitType.WaitCount:
                    RunWait();
                    break;
                default:
                    RunSignle();
                    break;
            }
            SystemManager.Instance.HeartLeft(StationName, RealName);
            return true;
        }

        /// <summary>
        /// 具体执行
        /// </summary>
        private void RunWait()
        {
            using (var pool = Prepare(Identity, out var socket))
            {
                ZeroTrace.SystemLog(StationName, "run", Config.WorkerCallAddress, Name, RealName);
                State = StationState.Run;
                while (CanLoop)
                {
                    if (!pool.Poll() || !pool.CheckIn(0, out var message))
                    {
                        Idle();
                        continue;
                    }
                    Interlocked.Increment(ref RecvCount);
                    using (message)
                    {
                        if (!Unpack(message, out var item))
                        {
                            SendLayoutErrorResult(ref socket, item);
                            continue;
                        }

                        Interlocked.Increment(ref WaitCount);
                        if (WaitCount > ZeroApplication.Config.MaxWait)
                        {
                            item.Result = ApiResult.UnavailableJson;
                            OnExecuestEnd(ref socket, item, ZeroOperatorStateType.Unavailable);
                        }
                        else
                        {
                            Task.Factory.StartNew(() =>
                            {
                                var socket_t = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER);
                                Execute(new ApiExecuter
                                {
                                    Station = this
                                }, ref socket_t, item);
                            });
                        }
                    }
                }
                socket?.Dispose();
            }
            ZeroTrace.SystemLog(StationName, "end", Config.WorkerCallAddress, Name, RealName);
        }
        /// <summary>
        /// 构造Pool
        /// </summary>
        /// <returns></returns>
        protected abstract IZmqPool Prepare(byte[] identity, out ZSocket socket);

        private SemaphoreSlim _processSemaphore;
        /// <summary>
        /// 具体执行
        /// </summary>
        private void RunThread()
        {
            ApiExecuter executer = new ApiExecuter
            {
                Station = this
            };
            var realName = ZeroIdentityHelper.CreateRealName(IsService, Config.StationName);
            var identity = realName.ToAsciiBytes();
            using (var pool = Prepare(identity, out var socket))
            {
                ZeroTrace.SystemLog(StationName, "run", Config.WorkerCallAddress, Name, realName);
                State = StationState.Run;
                while (CanLoop)
                {
                    if (!pool.Poll() || !pool.CheckIn(0, out var message))
                    {
                        Idle();
                        continue;
                    }

                    Interlocked.Increment(ref RecvCount);
                    using (message)
                    {
                        if (!Unpack(message, out var item))
                        {
                            SendLayoutErrorResult(ref socket, item);
                            continue;
                        }
                        Execute(executer, ref socket, item);
                    }
                }
                socket?.Dispose();
            }
            _processSemaphore?.Release();
            ZeroTrace.SystemLog(StationName, "end", Config.WorkerCallAddress, Name, realName);
        }

        /// <summary>
        /// 具体执行
        /// </summary>
        private void RunSignle()
        {
            ApiExecuter executer = new ApiExecuter
            {
                Station = this
            };
            using (var pool = Prepare(Identity, out var socket))
            {
                ZeroTrace.SystemLog(StationName, "run", Config.WorkerCallAddress, Name, RealName);
                State = StationState.Run;
                while (CanLoop)
                {
                    if (!pool.Poll() || !pool.CheckIn(0, out var message))
                    {
                        Idle();
                        continue;
                    }
                    Interlocked.Increment(ref RecvCount);
                    using (message)
                    {
                        if (!Unpack(message, out var item))
                        {
                            SendLayoutErrorResult(ref socket, item);
                            continue;
                        }

                        Execute(executer, ref socket, item);
                    }
                }
                socket?.Dispose();
            }
            _processSemaphore?.Release();
        }
        #endregion

        #region 执行


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

        #endregion
        #region 限定Task数量模式


        //readonly TaskQueue<ApiCallItem> Quote = new TaskQueue<ApiCallItem>();

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
        #endregion

        #region IO

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool Unpack(ZMessage messages, out ApiCallItem item)
        {
            item = new ApiCallItem
            {
                Caller = messages[0].Read()
            };
            try
            {
                var description = messages[1].Read();
                if (description.Length < 2)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", Config.WorkerResultAddress,
                        description.LinkToString(p => p.ToString("X2"), ""));
                    item = null;
                    return false;
                }

                int end = description[0] + 2;
                if (end != messages.Count)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", Config.WorkerResultAddress,
                        $"FrameSize{messages.Count}", description.LinkToString(p => p.ToString("X2"), ""));
                    item = null;
                    return false;
                }

                for (int idx = 2; idx < end; idx++)
                {
                    var bytes = messages[idx].Read();
                    if (bytes.Length == 0)
                        continue;
                    var val = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                    switch (description[idx])
                    {
                        case ZeroFrameType.GlobalId:
                            item.GlobalId = val;
                            break;
                        case ZeroFrameType.StationId:
                            item.StationCallId = val;
                            break;
                        case ZeroFrameType.RequestId:
                            item.RequestId = val;
                            break;
                        case ZeroFrameType.Requester:
                            item.Requester = val;
                            break;
                        case ZeroFrameType.Context:
                            item.ContextJson = val;
                            break;
                        case ZeroFrameType.Command:
                            item.ApiName = val;
                            break;
                        case ZeroFrameType.Argument:
                            item.Argument = val;
                            break;
                        case ZeroFrameType.Content:
                            item.Content = val;
                            break;
                    }
                }
                return item.ApiName != null && item.GlobalId != null;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e,
                    Config.WorkerResultAddress, $"FrameSize{messages.Count}");
                return false;
            }
            finally
            {
                messages.Dispose();
            }
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal abstract bool OnExecuestEnd(ref ZSocket socket, ApiCallItem item, ZeroOperatorStateType state);

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        internal abstract void SendLayoutErrorResult(ref ZSocket socket, ApiCallItem item);

        /// <summary>
        ///     配置校验
        /// </summary>
        protected abstract ZeroStationOption GetApiOption();

        #endregion
    }
}