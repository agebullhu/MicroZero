using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using ZeroMQ;
using Newtonsoft.Json;
namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public class ApiStation : ZeroStation
    {
        #region 注册方法

        private readonly Dictionary<string, ApiAction> _apiActions =
            new Dictionary<string, ApiAction>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void RegistAction(string name, ApiAction action)
        {
            if (!_apiActions.ContainsKey(name))
            {
                action.Name = name;
                _apiActions.Add(name, action);
                ZeroTrace.WriteInfo(StationName, $"{name} is registed");
            }
            else
            {
                _apiActions[name] = action;
            }
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        public ApiAction RegistAction(string name, Func<IApiResult> action, ApiAccessOption access)
        {
            var a = new ApiAction<IApiResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a);
            return a;
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        public ApiAction RegistAction(string name, Func<IApiArgument, IApiResult> action, ApiAccessOption access)
        {
            var a = new ApiAction<IApiArgument, IApiResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a);
            return a;
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        public ApiAction RegistAction<TResult>(string name, Func<TResult> action, ApiAccessOption access)
            where TResult : ApiResult
        {
            var a = new ApiAction<TResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a);
            return a;
        }

        /// <summary>
        ///     注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="access">访问设置</param>
        public ApiAction RegistAction<TArgument, TResult>(string name, Func<TArgument, TResult> action,
            ApiAccessOption access)
            where TArgument : class, IApiArgument
            where TResult : ApiResult
        {
            var a = new ApiAction<TArgument, TResult>
            {
                Name = name,
                Action = action,
                Access = access
            };
            RegistAction(name, a);
            return a;
        }


        /// <summary>
        /// 执行前的处理
        /// </summary>
        public static List<Action<ApiStation, ApiCallItem>> PreActions { get; } =
            new List<Action<ApiStation, ApiCallItem>>();

        /// <summary>
        /// 执行后的处理
        /// </summary>
        public static List<Action<ApiStation, ApiCallItem>> EndActions { get; } =
            new List<Action<ApiStation, ApiCallItem>>();
        #endregion

        #region Api调用

        /// <summary>
        /// 调用计数
        /// </summary>
        public int CallCount, ErrorCount, SuccessCount, RecvCount, SendCount, SendError;

        /// <summary>
        /// 构造
        /// </summary>
        public ApiStation() : base(StationTypeApi, true)
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
            foreach (var p in PreActions)
            {
                try
                {
                    p(this, item);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(StationName, e, "PreActions", item.ApiName);
                }
            }
        }

        void End(ApiCallItem item)
        {
            foreach (var p in EndActions)
            {
                try
                {
                    p(this, item);
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
                Prepare(item);
                bool success;
                using (MonitorStepScope.CreateScope("Do"))
                {
                    success = ExecCommand(item);
                }
                if (!success)
                    Interlocked.Increment(ref ErrorCount);
                else
                    Interlocked.Increment(ref SuccessCount);
                LogRecorder.MonitorTrace(item.Result);
                if (!SendResult(ref socket, item, success))
                {
                    ZeroTrace.WriteError(item.ApiName, "SendResult");
                }
                End(item);
            }
        }
        private void ApiCallNoMonitor(ref ZSocket socket, ApiCallItem item)
        {
            Prepare(item);

            bool success = ExecCommand(item);

            if (!success)
                Interlocked.Increment(ref ErrorCount);
            else
                Interlocked.Increment(ref SuccessCount);
            LogRecorder.MonitorTrace(item.Result);
            if (!SendResult(ref socket, item, success))
            {
                Interlocked.Increment(ref SendError);
                ZeroTrace.WriteError(item.ApiName, "SendResult");
            }
            End(item);
        }

        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool ExecCommand(ApiCallItem item)
        {
            if (!_apiActions.TryGetValue(item.ApiName, out var action))
            {
                item.Result = ZeroStatuValue.NoFindJson;
                item.Status = OperatorStatus.FormalError;
                return false;
            }

            //1 还原调用上下文
            try
            {
                if (!string.IsNullOrWhiteSpace(item.ContextJson))
                {
                    ApiContext.SetContext(JsonConvert.DeserializeObject<ApiContext>(item.ContextJson));
                }

                ApiContext.RequestContext.SetValue(item.GlobalId, item.Requester, item.RequestId);
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(StationName, e, item.ApiName, "restory context", item.ContextJson);
                item.Result = ZeroStatuValue.ArgumentErrorJson;
                item.Status = OperatorStatus.FormalError;
                return false;
            }

            //2 确定调用方法及对应权限
            if (action.NeedLogin && (ApiContext.Customer == null || ApiContext.Customer.UserId <= 0))
            {
                item.Result = ZeroStatuValue.DenyAccessJson;
                item.Status = OperatorStatus.DenyAccess;
                return false;
            }

            //3 参数校验
            try
            {
                if (!action.RestoreArgument(item.Argument))
                {
                    item.Result = ZeroStatuValue.ArgumentErrorJson;
                    item.Status = OperatorStatus.FormalError;
                    return false;
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(StationName, e, item.ApiName, "restory argument", item.Argument);
                item.Result = ZeroStatuValue.ArgumentErrorJson;
                item.Status = OperatorStatus.FormalError;
                return false;
            }

            try
            {

                if (!action.Validate(out var message))
                {
                    item.Result = JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.ArgumentError, message));
                    item.Status = OperatorStatus.FormalError;
                    return false;
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(StationName, e, item.ApiName, "invalidate argument", item.Argument);
                item.Result = ZeroStatuValue.ArgumentErrorJson;
                item.Status = OperatorStatus.FormalError;
                return false;
            }

            //4 方法执行
            try
            {
                var result = action.Execute();
                if (result != null)
                {
                    if (result.Status == null)
                        result.Status = new ApiStatsResult { InnerMessage = item.GlobalId };
                    else
                        result.Status.InnerMessage = item.GlobalId;
                }

                item.Result = result == null ? ZeroStatuValue.SucceesJson : JsonConvert.SerializeObject(result);
                item.Status = OperatorStatus.Success;
                return true;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(StationName, e, item.ApiName, "execute", JsonConvert.SerializeObject(item));
                item.Result = ZeroStatuValue.InnerErrorJson;
                item.Status = OperatorStatus.LocalError;
                return false;
            }
        }

        #endregion

        #region 网络与执行

        /// <summary>
        /// 初始化
        /// </summary>
        protected sealed override bool OnNofindConfig()
        {
            string type;
            switch (this.StationType)
            {
                case StationTypePublish:
                    type = "pub";
                    break;
                case StationTypeApi:
                    type = "api";
                    break;
                default:
                    type = null;
                    break;
            }
            ZeroTrace.WriteError(StationName, "No find,try install ...");
            var result = SystemManager.CallCommand("install", type, StationName, StationName);
            if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
                return false;
            ZeroTrace.WriteError(StationName, "Is install ,try start it ...");
            result = SystemManager.CallCommand("start", StationName);
            if (!result.InteractiveSuccess || result.State != ZeroOperatorStateType.Ok)
            {
                ZeroTrace.WriteError(StationName, $"Can't start {StationName}");
                return false;
            }
            Config = SystemManager.LoadConfig(StationName);
            if (Config == null)
            {
                ZeroTrace.WriteError(StationName, $"Can't load config  {StationName}");
                return false;
            }
            Config.State = ZeroCenterState.Run;
            ZeroTrace.WriteError(StationName, "successfully");
            return true;
        }

        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected sealed override bool RunInner(CancellationToken token)
        {
            waitCount = 0;
            SystemManager.HeartReady(StationName, RealName);
            switch (ZeroApplication.Config.SpeedLimitModel)
            {
                case SpeedLimitType.ThreadCount:
                    int max = (int)(Environment.ProcessorCount * ZeroApplication.Config.TaskCpuMultiple);
                    if (max < 1)
                        max = 1;
                    _processSemaphore = new SemaphoreSlim(0, max);
                    for (int idx = 0; idx < max; idx++)
                        Task.Factory.StartNew(RunSignle, token);
                    for (int idx = 0; idx < max; idx++)
                        _processSemaphore.Wait(token);
                    break;
                case SpeedLimitType.WaitCount:
                    RunWaite();
                    break;
                default:
                    RunSignle();
                    break;
            }
            SystemManager.HeartLeft(StationName, RealName);
            return true;
        }

        private int waitCount;

        private void ApiCallTask(object item)
        {
            var socket = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER);
            ApiCall(ref socket, (ApiCallItem)item);
        }

        /// <summary>
        /// 具体执行
        /// </summary>
        private void RunWaite()
        {
            var socket = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER);
            using (var pool = ZmqPool.CreateZmqPool())
            {
                pool.Prepare(new[] { ZSocket.CreateClientSocket(Config.WorkerCallAddress, ZSocketType.PULL, Identity) }, ZPollEvent.In);
                while (CanRun)
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
                            SendLayoutErrorResult(ref socket, item.Caller, item.Requester);
                            continue;
                        }

                        Interlocked.Increment(ref waitCount);
                        if (waitCount > ZeroApplication.Config.MaxWait)
                        {
                            item.Result = ZeroStatuValue.UnavailableJson;
                            SendResult(ref socket, item, false);
                        }
                        else
                        {
                            Task.Factory.StartNew(ApiCallTask, item);
                        }
                    }
                }
            }

            socket.TryClose();
        }

        private SemaphoreSlim _processSemaphore;
        /// <summary>
        /// 具体执行
        /// </summary>
        private void RunSignle()
        {
            var socket = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER);
            using (var pool = ZmqPool.CreateZmqPool())
            {
                pool.Prepare(new[] { ZSocket.CreateClientSocket(Config.WorkerCallAddress, ZSocketType.PULL, Identity) }, ZPollEvent.In);
                while (CanRun)
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
                            SendLayoutErrorResult(ref socket, item.Caller, item.Requester);
                            continue;
                        }
                        ApiCall(ref socket, item);
                    }
                }
            }

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
                Caller = Encoding.ASCII.GetString(messages[0].Read())
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
                    var val = Encoding.UTF8.GetString(bytes);
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
                return item.ApiName != null && item.RequestId != null && item.GlobalId != null && item.ContextJson != null;
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
        /// <param name="success"></param>
        /// <returns></returns>
        private bool SendResult(ref ZSocket socket, ApiCallItem item, bool success)
        {
            if (!CanRun)
            {
                ZeroTrace.WriteError("SendResult", "is closed", StationName);
                return false;
            }
            var description = new byte[]
            {
                3, (byte) (success ? ZeroOperatorStateType.Ok : ZeroOperatorStateType.Error), ZeroFrameType.JsonValue,ZeroFrameType.Requester,ZeroFrameType.GlobalId
            };
            return SendResult(ref socket, new ZMessage
            {
                new ZFrame(item.Caller.ToAsciiBytes()),
                //new ZFrame("".ToAsciiBytes()),
                new ZFrame(description),
                new ZFrame((item.Result ?? "").ToUtf8Bytes()),
                new ZFrame(item.Requester.ToAsciiBytes()),
                new ZFrame((item.GlobalId ?? "").ToUtf8Bytes())
            });
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="caller"></param>
        /// <param name="requester"></param>
        /// <returns></returns>
        private bool SendLayoutErrorResult(ref ZSocket socket, string caller, string requester)
        {
            return SendResult(ref socket, new ZMessage
            {
                new ZFrame(caller.ToAsciiBytes()),
                //new ZFrame("".ToAsciiBytes()),
                new ZFrame(new byte[]{1, (byte)ZeroOperatorStateType.Failed, ZeroFrameType.Requester, ZeroFrameType.End}),
                new ZFrame(caller.ToAsciiBytes())
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