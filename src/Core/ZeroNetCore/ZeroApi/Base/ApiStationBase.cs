using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.ApiDocuments;
using Agebull.Common.DataModel;
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
        #region 流程

        private readonly string _typeName;
        /// <summary>
        /// 构造
        /// </summary>
        protected ApiStationBase(ZeroStationType type, bool isService) : base(type, isService)
        {
            switch (type)
            {
                case ZeroStationType.Api:
                    _typeName = "api";
                    break;
                case ZeroStationType.Vote:
                    _typeName = "vote";
                    break;
                case ZeroStationType.RouteApi:
                    _typeName = "rapi";
                    break;
                case ZeroStationType.Queue:
                    _typeName = "queue";
                    break;
                default:
                    _typeName = "err";
                    break;
            }
        }

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
            if (!SystemManager.Instance.TryInstall(StationName, _typeName))
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
        ///     配置校验
        /// </summary>
        protected abstract ZeroStationOption GetApiOption();

        /// <summary>
        /// 构造Pool
        /// </summary>
        /// <returns></returns>
        protected abstract IZmqPool Prepare(byte[] identity, out ZSocket socket);

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
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected sealed override bool RunInner(CancellationToken token)
        {
            WaitCount = 0;
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
            return true;
        }

        /// <summary>
        /// 具体执行
        /// </summary>
        private void RunWait()
        {
            RunSignle(RealName, Identity, true);
        }
        private SemaphoreSlim _processSemaphore;

        /// <summary>
        /// 具体执行
        /// </summary>
        private void RunThread()
        {
            var realName = ZeroIdentityHelper.CreateRealName(IsService, Config.StationName);
            RunSignle(realName, realName.ToAsciiBytes(), false);
        }

        /// <summary>
        /// 具体执行
        /// </summary>
        private void RunSignle()
        {
            RunSignle(RealName, Identity, false);
        }

        /// <summary>
        /// 具体执行
        /// </summary>
        private void RunSignle(string realName, byte[] identity, bool checkWait)
        {
            ApiExecuter executer = checkWait
                ? null
                : new ApiExecuter
                {
                    Station = this
                };
            using (var pool = Prepare(identity, out var socket))
            {
                HeartManager hearter;
                if (pool.Sockets[0].SocketType == ZSocketType.DEALER)
                {
                    hearter = new HeartManager
                    {
                        Socket = pool.Sockets[0],
                        IsInner = true
                    };
                }
                else
                {
                    hearter = new HeartManager
                    {
                        Socket = socket,
                        IsInner = true
                    };
                }
                hearter.HeartJoin(StationName, realName);
                hearter.HeartReady(StationName, realName);
                ZeroTrace.SystemLog(StationName, "run", Config.WorkerCallAddress, Name, realName);
                State = StationState.Run;
                while (CanLoop)
                {
                    try
                    {
                        if (!pool.Poll())
                        {
                            Idle();
                            hearter.Heartbeat(StationName, realName);
                            continue;
                        }
                        if (pool.CheckIn(1, out var message))
                        {
                            message.Dispose();
                        }
                        if (!pool.CheckIn(0, out message))
                        {
                            continue;
                        }
                        Interlocked.Increment(ref RecvCount);
                        if (!Unpack(message, out var item))
                        {
                            SendLayoutErrorResult(socket, item);
                            continue;
                        }
                        Interlocked.Increment(ref WaitCount);
                        //hearter.Heartbeat(StationName, realName);
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
                hearter.HeartLeft(StationName, realName);
            }
            ZeroTrace.SystemLog(StationName, "end", Config.WorkerCallAddress, Name, realName);
            _processSemaphore?.Release();
        }
        #endregion

        #region Unpack

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool Unpack(ZMessage messages, out ApiCallItem item)
        {
            if (messages.Count == 0)
            {
                item = null;
                return false;
            }
            byte[][] buffers;
            using (messages)
                buffers = messages.Select(p => p.Read()).ToArray();
            item = new ApiCallItem
            {
                First = buffers[0]
            };
            try
            {
                var description = buffers[1];
                if (description.Length < 2)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", Config.WorkerResultAddress,
                        description.LinkToString(p => p.ToString("X2"), ""));
                    item = null;
                    return false;
                }

                int end = description[0] + 2;
                if (end != buffers.Length)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", Config.WorkerResultAddress,
                        $"FrameSize{buffers.Length}", description.LinkToString(p => p.ToString("X2"), ""));
                    item = null;
                    return false;
                }

                for (int idx = 2; idx < end; idx++)
                {
                    var bytes = buffers[idx];
                    item.Frames.Add(new NameValue<byte, byte[]>
                    {
                        name = description[idx],
                        value = bytes
                    });
                    if (bytes.Length == 0)
                        continue;
                    switch (description[idx])
                    {
                        case ZeroFrameType.GlobalId:
                            item.GlobalId = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                            break;
                        case ZeroFrameType.CallId:
                            item.CallId = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                            break;
                        case ZeroFrameType.LocalId:
                            item.LocalId = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                            break;
                        case ZeroFrameType.RequestId:
                            item.RequestId = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                            break;
                        case ZeroFrameType.Requester:
                            item.Requester = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                            break;
                        case ZeroFrameType.Context:
                            item.ContextJson = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                            break;
                        case ZeroFrameType.Command:
                            item.Command = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                            break;
                        case ZeroFrameType.Argument:
                            item.Argument = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                            break;
                        case ZeroFrameType.TextContent:
                            item.Content = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                            break;
                        case ZeroFrameType.Station:
                            item.StationName = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                            break;
                        case ZeroFrameType.StationType:
                            item.StationType = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                            break;
                        case ZeroFrameType.Status:
                            item.StatusMessage = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
                            break;
                        case ZeroFrameType.Original1:
                        case ZeroFrameType.Original2:
                        case ZeroFrameType.Original3:
                        case ZeroFrameType.Original4:
                        case ZeroFrameType.Original5:
                        case ZeroFrameType.Original6:
                        case ZeroFrameType.Original7:
                        case ZeroFrameType.Original8:
                            if (!item.Originals.ContainsKey(description[idx]))
                                item.Originals.Add(description[idx], bytes);
                            break;
                    }
                }
                return item.ApiName != null;// && item.GlobalId != null;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e,
                    Config.WorkerResultAddress, $"FrameSize{buffers.Length}");
                return false;
            }
        }

        #endregion

        #region 方法

        private void Execute(ZSocket socket, ApiCallItem item)
        {
            if (WaitCount > ZeroApplication.Config.MaxWait)
            {
                item.Result = ApiResult.UnavailableJson;
                OnExecuestEnd(socket, item, ZeroOperatorStateType.Unavailable);
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    var socketT = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER, ZeroIdentityHelper.CreateIdentity(false, StationName));
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
