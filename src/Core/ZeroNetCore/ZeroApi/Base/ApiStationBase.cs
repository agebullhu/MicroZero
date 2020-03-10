using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.MicroZero.ApiDocuments;
using Agebull.Common.Logging;
using Agebull.MicroZero.ZeroManagemant;
using ZeroMQ;
using Agebull.EntityModel.Common;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public abstract class ApiStationBase : ZeroStation
    {

        #region 主循环

        /// <summary>
        /// 调用计数
        /// </summary>
        public int CallCount, ErrorCount, SuccessCount, RecvCount, SendCount, SendError;



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
            var mg = new ConfigManager(ZeroApplication.Config.Master);
            if (!mg.TryInstall(StationName, _typeName))
                return null;
            var config = mg.LoadConfig(StationName);
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
        protected abstract IZmqPool PrepareLoop();

        ZeroStationOption _option;

        /// <summary>
        /// 代理地址
        /// </summary>
        protected string InprocAddress = "inproc://ApiProxy.req";

        /// <summary>
        /// 本地代理
        /// </summary>
        protected ZSocket _proxyServiceSocket;
        /// <summary>
        /// 轮询
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected sealed override bool Loop(/*CancellationToken token*/)
        {
            InprocAddress = $"inproc://{StationName}_{RandomOperate.Generate(4)}.req";
            //await Task.Yield();
            Tasks.Clear();
            timeout = ZeroApplication.Config.ApiTimeout * 1000;
            _option = GetApiOption();
            bool checkWait;
            switch (_option.SpeedLimitModel)
            {
                default:
                    ZeroTrace.SystemLog(StationName, "WaitCount", ZeroApplication.Config.MaxWait);
                    checkWait = true;
                    break;
                case SpeedLimitType.Single:
                    ZeroTrace.SystemLog(StationName, "Single");
                    checkWait = false;
                    break;
            }
            ZeroTrace.SystemLog(StationName, "Task", "start", RealName);

            using (var pool = PrepareLoop())
            {
                Hearter.HeartReady(StationName, RealName);
                RealState = StationState.Run;
                while (CanLoop)
                {
                    Listen(checkWait, pool);
                }
                pool.Sockets[0].Disconnect(Config.WorkerCallAddress);
                Hearter.HeartLeft(StationName, RealName);
                ZeroTrace.SystemLog(StationName, "closing");
                try
                {
                    int num = 0;
                    while (Listen(checkWait, pool))
                    {
                        LogRecorder.Trace("处理堆积任务{0}", ++num);
                    }
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e, "处理堆积任务出错{0}", StationName);
                }

                int cnt = 0;
                while (WaitCount > 0 && ++cnt < 100)
                    Thread.Sleep(10);
                CloseTask();
            }

            ZeroTrace.SystemLog(StationName, "Task", "end", RealName);
            return true;
        }

        private bool Listen(bool checkWait, IZmqPool pool)
        {
            try
            {
                if (!pool.Poll())
                {
                    OnLoopIdle();
                    return false;
                }
                //对Result端口的返回的丢弃处理
                if (pool.CheckIn(0, out var message))
                    message?.Dispose();

                if (pool.CheckIn(1, out message))
                {
                    Interlocked.Increment(ref RecvCount);
                    OnCall(checkWait, message);
                }
                if (pool.CheckIn(2, out message))
                {
                    OnResult(pool.Sockets[0], message);
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }

            return true;
        }

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        private void OnResult(ZSocket socket, ZMessage message)
        {
            ZMessage message2;
            using (message)
            {
                message2 = message.Duplicate(1);
            }

            bool success;
            ZError error;
            using (message2)
            {
                success = socket.SendMessage(message2, ZSocketFlags.DontWait, out error);
            }
            if (!success)
                ZeroTrace.WriteError(StationName, error.Text, error.Name);
        }

        /// <summary>
        /// Api调用时的Task节点信息
        /// </summary>
        public class ApiTaskItem
        {
            /// <summary>
            /// TaskId
            /// </summary>
            public long TaskId { get; set; }
            /// <summary>
            /// 执行器
            /// </summary>
            public ApiExecuter Executer { get; set; }
            /// <summary>
            /// 所在线程（注意可能失效）
            /// </summary>
            public Thread Thread { get; set; }

            /// <summary>
            /// API调用的信息
            /// </summary>
            public ApiCallItem Api { get; set; }
            /// <summary>
            /// 任务
            /// </summary>
            public Task Task { get; set; }
            /// <summary>
            /// 开始时间（用于超时检查）
            /// </summary>
            public DateTime Start { get; set; }
        }

        /// <summary>
        /// 当前活跃任务
        /// </summary>
        internal readonly ConcurrentDictionary<long, ApiTaskItem> Tasks = new ConcurrentDictionary<long, ApiTaskItem>();

        /// <summary>
        /// 总等待数
        /// </summary>
        public int WaitCount => Tasks.Count;

        /// <summary>
        /// 检查超时任务
        /// </summary>
        /// <returns></returns>
        internal void CheckTask()
        {
            try
            {
                if (Tasks.Count == 0 || !CanLoop)
                    return;
                var array = Tasks.Values.ToArray();
                LogRecorder.SystemLog($"【CheckTask|{StationName}({array.Length})】CallCount:{CallCount},ErrorCount:{ErrorCount},SuccessCount:{SuccessCount},RecvCount:{RecvCount},SendCount:{SendCount},SendError:{SendError},WaitCount:{WaitCount}");
                foreach (var item in array)
                {
                    try
                    {
                        if (item.Task.IsCanceled || item.Task.IsCompleted || item.Task.IsFaulted)
                        {
                            Tasks.TryRemove(item.TaskId, out _);
                        }
                        else if ((DateTime.Now - item.Start).TotalSeconds >= _option.ApiTimeout)
                        {
                            KillTask(item);
                        }
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e, "CheckTask|{0}", item.TaskId);
                        Tasks.TryRemove(item.TaskId, out _);
                    }
                }
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex, "CheckTask");
            }
        }

        /// <summary>
        /// 空转
        /// </summary>
        /// <returns></returns>
        private void CloseTask()
        {
            if (Tasks.Count == 0)
                return;
            try
            {
                LogRecorder.SystemLog($"【CloseTask|{StationName}】CallCount:{CallCount},ErrorCount:{ErrorCount},SuccessCount:{ SuccessCount},RecvCount:{ RecvCount},SendCount:{ SendCount},SendError:{ SendError},WaitCount:{ WaitCount}");
                foreach (var item in Tasks.Values.ToArray())
                {
                    try
                    {
                        //强行中断线程
                        if (!item.Task.IsCanceled && !item.Task.IsCompleted && !item.Task.IsFaulted)
                            KillTask(item);
                    }
                    catch (Exception e)
                    {
                        LogRecorder.Exception(e, "CloseTask|{0}", item.TaskId);
                    }
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "CloseTask");
            }
            finally
            {
                Tasks.Clear();
            }
        }

        /// <summary>
        /// 杀死任务
        /// </summary>
        /// <param name="item"></param>
        private void KillTask(ApiTaskItem item)
        {
            if (item.Task.IsCanceled || item.Task.IsCompleted || item.Task.IsFaulted)
                return;
            //if (item.Executer?.CancellationToken == null)
            //    return;
            //item.Executer.CancellationToken.Cancel(false);
            var info = new StringBuilder();
            info.Append($@"【KillTask|{StationName}】 ({item.Start}|{DateTime.Now})
{item.Api.ApiName}
{item.Api.Argument}
{item.Api.Content}
{item.Api.Context}");
            try
            {
                Tasks.TryRemove(item.TaskId, out _);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "KillTask Remove");
                info.Append($"Remove : {e.Message}");
            }
            if (ZeroApplication.Config.ApiTimeoutKill)
            {
                try
                {
                    //强行中断线程
                    item.Thread?.Interrupt();
                    //item.Task?.Dispose();
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e, "KillTask Interrupt");
                    info.Append($"Interrupt : {e.Message}");
                }

                try
                {
                    item.Executer.ScopeResource?.Dispose();
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e, "KillTask Dispose");
                    info.Append($"Dispose : {e.Message}");
                }
            }
            try
            {
                SendResult(new ZMessage
                    {
                        new ZFrame(item.Api.Caller),
                        new ZFrame(TimeOutKillFrame),
                        new ZFrame(item.Api.Requester),
                        new ZFrame(Config.ServiceKey)
                    });
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "KillTask SendResult");
                info.Append($"SendResult : {e.Message}");
            }
            LogRecorder.Error(info.ToString());
        }

        private static readonly byte[] TimeOutKillFrame = new byte[]
        {
            2,
            (byte) ZeroOperatorStateType.ExecTimeOut,
            ZeroFrameType.Requester,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.ResultEnd
        };

        #endregion

        #region 方法

        #region 方法调用

        private int timeout;
        private void OnCall(bool checkWait, ZMessage message)
        {
            Interlocked.Increment(ref CallCount);
            if (!ApiCallItem.Unpack(message, out var item) || string.IsNullOrWhiteSpace(item.ApiName))
            {
                SendLayoutErrorResult(item);
                return;
            }
            switch (item.ApiName[0])
            {
                case '$':
                    item.Result = ApiResult.SucceesJson;
                    OnExecuestEnd(item, ZeroOperatorStateType.Ok);
                    return;
                case '*':
                    item.Result = ZeroApplication.TestFunc();
                    OnExecuestEnd(item, ZeroOperatorStateType.Ok);
                    return;
            }

            var arg = new ApiTaskItem
            {
                Api = item,
                Start = DateTime.Now
            };
            if (!checkWait)
            {
                Execute(arg, true);
                return;
            }

            if (WaitCount > ZeroApplication.Config.MaxWait)
            {
                LogRecorder.SystemLog("Unavailable");
                item.Result = ApiResultIoc.UnavailableJson;
                OnExecuestEnd(item, ZeroOperatorStateType.Unavailable);
            }
            else
            {
                Execute(arg, false);
            }
        }

        private void Execute(ApiTaskItem arg, bool wait)
        {
            try
            {
                if (!PrepareExecute(arg.Api))
                    return;
                arg.Executer = new ApiExecuter
                {
                    TaskItem = arg,
                    Station = this,
                    Item = arg.Api
                };
                arg.Task = arg.Executer.Execute();
                arg.TaskId = arg.Task.Id;
                Tasks.TryAdd(arg.TaskId, arg);
                if (wait && !arg.Task.Wait(timeout))
                    KillTask(arg);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
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

        #region 方法注册

        internal readonly Dictionary<string, ApiAction> ApiActions = new Dictionary<string, ApiAction>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
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
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="action">动作</param>
        /// <param name="info">反射信息</param>
        /// <param name="access">访问设置</param>
        public ApiAction RegistAction(string name, Func<Task<IApiResult>> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiTaskAction<IApiResult>
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
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
        /// <param name="action">动作</param>
        /// <param name="info">反射信息</param>
        /// <param name="access">访问设置</param>
        public ApiAction RegistAction(string name, Func<object, Task<IApiResult>> action, ApiAccessOption access, ApiActionInfo info = null)
        {
            var a = new ApiTaskAction2<IApiResult>
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
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
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
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
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
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
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
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
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
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
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
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
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
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
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
        /// <param name="name">方法外部方法名称，如 v1/auto/getdid </param>
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

        #region 方法扩展

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

        #region 返回

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal virtual bool OnExecuestEnd(ApiCallItem item, ZeroOperatorStateType state)
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
            msg.Add(Config.ServiceKey);
            des[i] = item.EndTag > 0 ? item.EndTag : ZeroFrameType.ResultEnd;
            return SendResult(new ZMessage(msg));
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
        /// <param name="item"></param>
        /// <returns></returns>
        internal virtual void SendLayoutErrorResult(ApiCallItem item)
        {
            if (item == null)
            {
                Interlocked.Increment(ref SendError);
                return;
            }
            SendResult(new ZMessage
            {
                new ZFrame(item.Caller),
                new ZFrame(LayoutErrorFrame),
                new ZFrame(item.Requester),
                new ZFrame(Config.ServiceKey)
            });
        }


        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected bool SendResult(ZMessage message)
        {
            using (message)
            {
                if (!CanLoop)
                {
                    ZeroTrace.WriteError(StationName, "Can`t send result,station is closed");
                    return false;
                }
                try
                {
                    var socket = ZSocketEx.CreateOnceSocket(InprocAddress, null, null, ZSocketType.PAIR);
                    if (socket == null)
                    {
                        Interlocked.Increment(ref SendError);
                        return false;
                    }
                    ZError error;
                    bool success;
                    using (socket)
                    {
                        success = socket.Send(message, out error);
                    }

                    if (success)
                    {
                        Interlocked.Increment(ref SendCount);
                        return true;
                    }

                    ZeroTrace.WriteError(StationName, error.Text, error.Name);
                    LogRecorder.MonitorTrace(() => $"{StationName}({socket.Endpoint}) : {error.Text}");
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e, "ApiStation.SendResult");
                    LogRecorder.MonitorTrace(() => $"Exception : {StationName} : {e.Message}");
                }
            }
            Interlocked.Increment(ref SendError);
            return false;
        }
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
