using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using ZeroMQ;
using Newtonsoft.Json;
using Gboxt.Common.DataModel;
using Agebull.Common.Rpc;
using Agebull.Common.ApiDocuments;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public class ApiStation : ZeroStation
    {
        #region 注册方法
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
        static List<IApiHandler> CreateHandlers()
        {
            List<IApiHandler> handlers = new List<IApiHandler>();
            foreach (var func in ApiHandlers)
                handlers.Add(func());
            return handlers;
        }

        private readonly Dictionary<string, ApiAction> _apiActions =
            new Dictionary<string, ApiAction>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="info">反射信息</param>
        public void RegistAction(string name, ApiAction action, ApiActionInfo info = null)
        {
            if (!_apiActions.ContainsKey(name))
            {
                action.Name = name;
                _apiActions.Add(name, action);
            }
            else
            {
                _apiActions[name] = action;
            }

            ZeroTrace.WriteInfo(StationName,
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
        /// 调用计数
        /// </summary>
        public int CallCount, ErrorCount, SuccessCount, RecvCount, SendCount, SendError;

        /// <summary>
        /// 构造
        /// </summary>
        public ApiStation() : base(ZeroStationType.Api, true)
        {

        }


        private void ApiCall(ref ZSocket socket, ApiCallItem item)
        {
            Interlocked.Increment(ref CallCount);
            if (LogRecorder.LogMonitor)
                ApiCallByMonitor(ref socket, item);
            else
                ApiCallNoMonitor(ref socket, item);
            Interlocked.Decrement(ref waitCount);
        }

        void Prepare(ApiCallItem item)
        {
            item.Handlers = CreateHandlers();
            if (item.RequestId == null)
                item.RequestId = item.GlobalId;

            if (item.Handlers == null)
                return;
            foreach (var p in item.Handlers)
            {
                try
                {
                    p.Prepare(this, item);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(StationName, e, "PreActions", item.ApiName);
                }
            }
        }

        void End(ApiCallItem item)
        {
            if (item.Handlers == null)
                return;
            foreach (var p in item.Handlers)
            {
                try
                {
                    p.End(this, item);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(StationName, e, "EndActions", item.ApiName);
                }
            }
        }
        private void ApiCallByMonitor(ref ZSocket socket, ApiCallItem item)
        {
            using (MonitorScope.CreateScope(item.ApiName))
            {
                LogRecorder.MonitorTrace($"Caller:{item.Caller}");
                LogRecorder.MonitorTrace($"GlobalId:{item.GlobalId}");
                LogRecorder.MonitorTrace(JsonConvert.SerializeObject(item));
                ZeroOperatorStateType state = RestoryContext(item);
                if (state == ZeroOperatorStateType.Ok)
                {
                    Prepare(item);
                    using (MonitorScope.CreateScope("Do"))
                    {
                        state = ExecCommand(item);
                    }

                    if (state != ZeroOperatorStateType.Ok)
                        Interlocked.Increment(ref ErrorCount);
                    else
                        Interlocked.Increment(ref SuccessCount);
                }
                else
                    Interlocked.Increment(ref ErrorCount);

                LogRecorder.MonitorTrace(item.Result);
                if (!SendResult(ref socket, item, state))
                {
                    ZeroTrace.WriteError(item.ApiName, "SendResult");
                    Interlocked.Increment(ref SendError);
                }
                End(item);
            }
        }
        private void ApiCallNoMonitor(ref ZSocket socket, ApiCallItem item)
        {
            ZeroOperatorStateType state = RestoryContext(item);
            if (state == ZeroOperatorStateType.Ok)
            {
                Prepare(item);
                state = ExecCommand(item);

                if (state != ZeroOperatorStateType.Ok)
                    Interlocked.Increment(ref ErrorCount);
                else
                    Interlocked.Increment(ref SuccessCount);
            }
            else
            {
                Interlocked.Increment(ref ErrorCount);
            }
            if (!SendResult(ref socket, item, state))
            {
                Interlocked.Increment(ref SendError);
            }
            End(item);
        }
        /// <summary>
        /// 还原调用上下文
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private ZeroOperatorStateType RestoryContext(ApiCallItem item)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(item.ContextJson))
                {
                    GlobalContext.SetContext(JsonConvert.DeserializeObject<ApiContext>(item.ContextJson));
                }
                GlobalContext.Current.Request.SetValue(item.GlobalId, item.Requester, item.RequestId);
                return ZeroOperatorStateType.Ok;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(StationName, e, item.ApiName, "restory context", item.ContextJson);
                item.Result = ApiResult.ArgumentErrorJson;
                item.Status = ZeroOperatorStatus.FormalError;
                return ZeroOperatorStateType.LocalException;
            }
        }
        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private ZeroOperatorStateType ExecCommand(ApiCallItem item)
        {
            //1 查找调用方法
            if (!_apiActions.TryGetValue(item.ApiName.Trim(), out var action))
            {
                item.Result = ApiResult.NoFindJson;
                item.Status = ZeroOperatorStatus.NotFind;
                return ZeroOperatorStateType.NotFind;
            }

            //2 确定调用方法及对应权限
            if (action.NeedLogin && (GlobalContext.Customer == null || GlobalContext.Customer.UserId <= 0))
            {
                item.Result = ApiResult.DenyAccessJson;
                item.Status = ZeroOperatorStatus.DenyAccess;
                return ZeroOperatorStateType.DenyAccess;
            }

            //3 参数校验
            try
            {
                if (!action.RestoreArgument(item.Argument ?? "{}"))
                {
                    item.Result = ApiResult.ArgumentErrorJson;
                    item.Status = ZeroOperatorStatus.FormalError;
                    return ZeroOperatorStateType.ArgumentInvalid;
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(StationName, e, item.ApiName, "restory argument", item.Argument);
                item.Result = ApiResult.LocalExceptionJson;
                item.Status = ZeroOperatorStatus.FormalError;
                return ZeroOperatorStateType.LocalException;
            }

            try
            {

                if (!action.Validate(out var message))
                {
                    item.Result = JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.LogicalError, message));
                    item.Status = ZeroOperatorStatus.LogicalError;
                    return ZeroOperatorStateType.ArgumentInvalid;
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(StationName, e, item.ApiName, "invalidate argument", item.Argument);
                item.Result = ApiResult.LocalExceptionJson;
                item.Status = ZeroOperatorStatus.LocalException;
                return ZeroOperatorStateType.LocalException;
            }

            //4 方法执行
            try
            {
                var result = action.Execute();
                if (result != null)
                {
                    if (result.Status == null)
                        result.Status = new ApiStatusResult { InnerMessage = item.GlobalId };
                    else
                        result.Status.InnerMessage = item.GlobalId;
                }

                item.Result = result == null ? ApiResult.SucceesJson : JsonConvert.SerializeObject(result);
                item.Status = result == null || result.Success ? ZeroOperatorStatus.Success : ZeroOperatorStatus.LogicalError;
                return result == null || result.Success ? ZeroOperatorStateType.Ok : ZeroOperatorStateType.Failed;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(StationName, e, item.ApiName, "execute", JsonConvert.SerializeObject(item));
                item.Result = ApiResult.LocalExceptionJson;
                item.Status = ZeroOperatorStatus.LocalException;
                return ZeroOperatorStateType.LocalException;
            }
        }

        #endregion

        #region 网络与执行

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
            ZeroTrace.WriteInfo(StationName, "successfully");
            return true;
        }

        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected sealed override bool RunInner(CancellationToken token)
        {
            waitCount = 0;
            SystemManager.Instance.HeartReady(StationName, RealName);
            var option = ZeroApplication.GetApiOption(StationName);
            switch (option.SpeedLimitModel)
            {
                case SpeedLimitType.ThreadCount:
                    int max = (int)(Environment.ProcessorCount * option.TaskCpuMultiple);
                    if (max < 1)
                        max = 1;
                    _processSemaphore = new SemaphoreSlim(0, max);
                    for (int idx = 0; idx < max; idx++)
                        Task.Factory.StartNew(RunSignle);

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

        private int waitCount;

        private void ApiCallTask(object item)
        {
            var socket = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER);
            ApiCall(ref socket, (ApiCallItem)item);
            socket.Dispose();
        }

        /// <summary>
        /// 具体执行
        /// </summary>
        private void RunWait()
        {
            var socket = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER);
            {
                using (var pool = ZmqPool.CreateZmqPool())
                {
                    pool.Prepare(ZPollEvent.In, ZSocket.CreateClientSocket(Config.WorkerCallAddress, ZSocketType.PULL, Identity));
                    State = StationState.Run;
                    while (CanLoop)
                    {
                        if (!pool.Poll() || !pool.CheckIn(0, out var message))
                        {
                            continue;
                        }
                        Interlocked.Increment(ref RecvCount);
                        using (message)
                        {
                            if (!Unpack(message, out var item))
                            {
                                SendLayoutErrorResult(ref socket, item.Caller);
                                continue;
                            }

                            Interlocked.Increment(ref waitCount);
                            if (waitCount > ZeroApplication.Config.MaxWait)
                            {
                                item.Result = ApiResult.UnavailableJson;
                                SendResult(ref socket, item, ZeroOperatorStateType.Unavailable);
                            }
                            else
                            {
                                Task.Factory.StartNew(ApiCallTask, item);
                            }
                        }
                    }
                }
            }
            socket.Dispose();
        }

        private SemaphoreSlim _processSemaphore;
        /// <summary>
        /// 具体执行
        /// </summary>
        private void RunSignle()
        {
            var socket = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER);
            {
                using (var pool = ZmqPool.CreateZmqPool())
                {
                    pool.Prepare(new[] { ZSocket.CreateClientSocket(Config.WorkerCallAddress, ZSocketType.PULL, Identity) }, ZPollEvent.In);
                    State = StationState.Run;
                    while (CanLoop)
                    {
                        if (!pool.Poll() || !pool.CheckIn(0, out var message))
                        {
                            continue;
                        }
                        Interlocked.Increment(ref RecvCount);
                        using (message)
                        {
                            if (!Unpack(message, out var item))
                            {
                                SendLayoutErrorResult(ref socket, item.Caller);
                                continue;
                            }
                            ApiCall(ref socket, item);
                        }
                    }
                }
            }
            socket.Dispose();
            IocHelper.DisposeScope();
            _processSemaphore?.Release();
        }
        #endregion

        #region 限定Task数量模式


        //readonly TaskQueue<ApiCallItem> Quote = new TaskQueue<ApiCallItem>();

        /// <inheritdoc />
        protected sealed override void OnRunStop()
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
        private bool SendResult(ref ZSocket socket, ApiCallItem item, ZeroOperatorStateType state)
        {
            if (!CanLoop)
            {
                ZeroTrace.WriteError("SendResult", "is closed", StationName);
                return false;
            }
            if (item.Result == null)
                return SendResult(ref socket, new ZMessage
                {
                    new ZFrame(item.Caller),
                    new ZFrame(new byte[]
                    {
                        2, (byte) state, ZeroFrameType.Requester, ZeroFrameType.GlobalId
                    }),
                    new ZFrame(item.Requester.ToZeroBytes()),
                    new ZFrame((item.GlobalId).ToZeroBytes())
                });

            return SendResult(ref socket, new ZMessage
            {
                new ZFrame(item.Caller),
                new ZFrame(new byte[]
                {
                    3, (byte) state, ZeroFrameType.JsonValue, ZeroFrameType.Requester, ZeroFrameType.GlobalId
                }),
                new ZFrame((item.Result).ToZeroBytes()),
                new ZFrame(item.Requester.ToZeroBytes()),
                new ZFrame((item.GlobalId).ToZeroBytes())
            });
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        private void SendLayoutErrorResult(ref ZSocket socket, byte[] caller)
        {
            SendResult(ref socket, new ZMessage
            {
                new ZFrame(caller),
                new ZFrame(new byte[]{0, (byte)ZeroOperatorStateType.FrameInvalid,ZeroFrameType.End})
            });
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool SendResult(ref ZSocket socket, ZMessage message)
        {
            ZError error;
            using (message)
            {
                if (socket.Send(message, out error))
                    return true;
                socket.TryClose();
                socket = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER, Identity);
            }
            ZeroTrace.WriteError(StationName, error.Text, error.Name);
            Interlocked.Increment(ref SendError);
            return false;
        }

        #endregion
    }
}