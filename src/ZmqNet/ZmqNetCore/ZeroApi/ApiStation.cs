using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common;
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
                ApiCallByMonitor(ref socket,item);
            else
                ApiCallNoMonitor(ref socket, item);
            Interlocked.Decrement(ref waitCount);
        }


        private void ApiCallByMonitor(ref ZSocket socket, ApiCallItem item)
        {
            using (MonitorScope.CreateScope(item.ApiName))
            {
                LogRecorder.MonitorTrace($"Caller:{item.Caller}");
                LogRecorder.MonitorTrace($"GlobalId:{item.GlobalId}");
                LogRecorder.MonitorTrace(JsonConvert.SerializeObject(item));
                try
                {
                    PreActions.ForEach(p => p(this, item));
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteError(item.ApiName, "PreActions");
                    ZeroTrace.WriteException(item.ApiName, e);
                }
                bool success;
                try
                {
                    using (MonitorStepScope.CreateScope("Do"))
                    {
                        success = ExecCommand(item);
                    }
                }
                catch (Exception ex)
                {
                    LogRecorder.Exception(ex);
                    LogRecorder.MonitorTrace(ex.Message);
                    ZeroTrace.WriteException(StationName, ex);
                    success = false;
                    item.Result = ZeroStatuValue.InnerErrorJson;
                }
                if (!success)
                    Interlocked.Increment(ref ErrorCount);
                else
                    Interlocked.Increment(ref SuccessCount);
                LogRecorder.MonitorTrace(item.Result);
                if (!SendResult(ref socket, item, success))
                {
                    ZeroTrace.WriteError(item.ApiName, "PreActions");
                }

                try
                {
                    EndActions.ForEach(p => p(this, item));
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteError(item.ApiName, "EndActions");
                    ZeroTrace.WriteException(item.ApiName, e);
                }
            }
        }
        private void ApiCallNoMonitor(ref ZSocket socket,ApiCallItem item)
        {
            try
            {
                PreActions.ForEach(p => p(this, item));
            }
            catch (Exception e)
            {
                ZeroTrace.WriteError(item.ApiName, "PreActions");
                ZeroTrace.WriteException(item.ApiName, e);
            }

            bool success;
            try
            {
                success = ExecCommand(item);
            }
            catch (Exception ex)
            {
                LogRecorder.Exception(ex);
                LogRecorder.MonitorTrace(ex.Message);
                ZeroTrace.WriteException(StationName, ex);
                success = false;
                item.Result = ZeroStatuValue.InnerErrorJson;
            }

            if (!success)
                Interlocked.Increment(ref ErrorCount);
            else
                Interlocked.Increment(ref SuccessCount);
            LogRecorder.MonitorTrace(item.Result);
            if (!SendResult(ref socket, item, success))
            {
                Interlocked.Increment(ref SendError);
                ZeroTrace.WriteError(item.ApiName, "PreActions");
            }


            try
            {
                EndActions.ForEach(p => p(this, item));
            }
            catch (Exception e)
            {
                ZeroTrace.WriteError(item.ApiName, "EndActions");
                ZeroTrace.WriteException(item.ApiName, e);
            }
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
                LogRecorder.Exception(e);
                ZeroTrace.WriteException($"{StationName}:{item.ApiName}:restory context", e);
                ZeroTrace.WriteError($"{StationName}:{item.ApiName}:restory context", item.ContextJson);
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
                LogRecorder.Exception(e);
                item.Result = ZeroStatuValue.ArgumentErrorJson;
                item.Status = OperatorStatus.FormalError;
                ZeroTrace.WriteException($"{StationName}:{item.ApiName}:restory argument", e);
                ZeroTrace.WriteError($"{StationName}:{item.ApiName}:restory argument", item.Argument);
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
                LogRecorder.Exception(e);
                item.Result = ZeroStatuValue.ArgumentErrorJson;
                item.Status = OperatorStatus.FormalError;
                ZeroTrace.WriteException($"{StationName}:{item.ApiName}:invalidate argument", e);
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
                LogRecorder.Exception(e);
                item.Result = ZeroStatuValue.InnerErrorJson;
                item.Status = OperatorStatus.LocalError;
                ZeroTrace.WriteException($"{StationName}:{item.ApiName}:execuest", e);
                return false;
            }
        }

        #endregion

        #region 网络与执行

        private ZSocket _callSocket, _resultSocket /*, _inprocCallSocket, _inprocPollSocket*/;


        /// <summary>
        /// 初始化
        /// </summary>
        protected sealed override void OnStart()
        {
            base.OnStart();
            //Identity = RealName.ToAsciiBytes();
            //string name = $"inproc://{StationName}";
            //_inprocCallSocket = ZeroHelper.CreateClientSocket(name, ZSocketType.PAIR, name.ToAsciiBytes());
            //_inprocPollSocket = ZeroHelper.CreateServiceSocket(name, ZSocketType.PAIR, name.ToAsciiBytes());
            _callSocket = ZeroHelper.CreateClientSocket(Config.WorkerCallAddress, ZSocketType.PULL, Identity);
            _resultSocket = ZeroHelper.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER, Identity);
        }

        /// <inheritdoc />
        /// <summary>
        ///     命令轮询
        /// </summary>
        /// <returns></returns>
        protected sealed override bool RunInner(CancellationToken token)
        {
            waitCount = 0;
            var sockets = new[] { _callSocket };
            var pollItems = new[] { ZPollItem.CreateReceiver() };
            if (ZeroApplication.Config.SpeedLimitModel == SpeedLimitType.ThreadCount)
            {
                for (int i = 0; i < Environment.ProcessorCount * ZeroApplication.Config.TaskCpuMultiple; i++)
                    new Thread(ProcessTask)
                    {
                        IsBackground = true
                    }.Start(token);
            }
            while (!token.IsCancellationRequested && CanRun)
            {
                if (!sockets.PollIn(pollItems, out var messages, out var error, new TimeSpan(0, 0, 0, 0, 500)))
                {
                    if (error == null)
                        continue;
                    if (Equals(error, ZError.ETERM))
                        break;
                    if (!Equals(error, ZError.EAGAIN))
                    {
                        ZeroTrace.WriteError(StationName, error.Text, error.Name);
                    }
                    continue;
                }

                if (messages[0] == null)
                    continue;
                Interlocked.Increment(ref RecvCount);
                ReceiveApiCall(messages[0], out var item);
                messages[0].Dispose();
                if (item == null)
                    continue;
                switch (ZeroApplication.Config.SpeedLimitModel)
                {
                    default:
                    case SpeedLimitType.Single:
                        ApiCall(ref _resultSocket, item);
                        break;
                    case SpeedLimitType.ThreadCount:
                        Interlocked.Increment(ref waitCount);
                        if (waitCount > ZeroApplication.Config.MaxWait)
                        {
                            item.Result = ZeroStatuValue.UnavailableJson;
                            SendResult(ref _resultSocket, item, false);
                        }
                        else
                        {
                            Quote.Push(item);
                        }
                        break;
                    case SpeedLimitType.WaitCount:
                        Interlocked.Increment(ref waitCount);
                        if (waitCount > ZeroApplication.Config.MaxWait)
                        {
                            item.Result = ZeroStatuValue.UnavailableJson;
                            SendResult(ref _resultSocket, item, false);
                        }
                        else
                        {
                            Task.Factory.StartNew(ApiCallTask, item, token);
                        }
                        break;
                }

            }

            return true;
        }

        private int waitCount;

        private void ApiCallTask(object item)
        {
            ApiCall(ref _resultSocket,(ApiCallItem)item);
        }

        #endregion

        #region 限定Task数量模式
        private int ptocessTaskCount;


        readonly TaskQueue<ApiCallItem> Quote = new TaskQueue<ApiCallItem>();

        /// <inheritdoc />
        protected sealed override void OnRunStop()
        {
            while (ptocessTaskCount > 0)
                Thread.Sleep(10);
            if (ZContext.IsAlive)
            {
                while (!Quote.IsEmpty)
                {
                    var t = Quote.Queue.Dequeue();
                    t.Result = ZeroStatuValue.UnavailableJson;
                    Interlocked.Increment(ref CallCount);
                    Interlocked.Increment(ref ErrorCount);
                    SendResult(ref _resultSocket, t, false);
                }
            }
            _callSocket.CloseSocket();
            _resultSocket.CloseSocket();
            base.OnRunStop();
        }

        void ProcessTask(object obj)
        {
            Interlocked.Increment(ref ptocessTaskCount);
            var socket = ZeroHelper.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER);
            var token = (CancellationToken)obj;
            while (!token.IsCancellationRequested && CanRun)
            {
                if (!Quote.StartProcess(out var item))
                {
                    continue;
                }
                if (token.IsCancellationRequested)
                    continue;
                ApiCall(ref socket, item);
                Quote.EndProcess();
            }
            socket.CloseSocket();
            Interlocked.Decrement(ref ptocessTaskCount);
        }
        #endregion

        #region IO


        private bool ReceiveApiCall(ZMessage messages, out ApiCallItem item)
        {
            try
            {
                var description = messages[2].Read();
                if (description.Length < 2)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", Config.WorkerResultAddress,
                        description.LinkToString(p => p.ToString("X2"), ""), $"Socket Ptr:{_callSocket.SocketPtr}.");
                    item = null;
                    return false;
                }

                int end = description[0] + 3;
                if (end != messages.Count)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", Config.WorkerResultAddress,
                        $"FrameSize{messages.Count}", description.LinkToString(p => p.ToString("X2"), ""),
                        $"Socket Ptr:{_callSocket.SocketPtr}.");
                    item = null;
                    return false;
                }

                item = new ApiCallItem
                {
                    Caller = Encoding.ASCII.GetString(messages[0].Read())
                };
                for (int idx = 3; idx < end; idx++)
                {
                    var bytes = messages[idx].Read();
                    if (bytes.Length == 0)
                        continue;
                    var val = Encoding.UTF8.GetString(bytes);
                    switch (description[idx - 1])
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

                return true;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteError("Receive", "Exception", Config.WorkerResultAddress,
                    $"FrameSize{messages.Count}.Socket Ptr:{_callSocket.SocketPtr}.");
                ZeroTrace.WriteException("ReceiveApiCall", e);
                LogRecorder.Exception(e);
                item = null;
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
        private bool SendResult(ref ZSocket socket,ApiCallItem item, bool success)
        {
            if (!CanRun)
            {
                ZeroTrace.WriteError("SendResult", "is closed", StationName);
                return false;
            }
            var description = new byte[]
            {
                2, (byte) (success ? ZeroOperatorStateType.Ok : ZeroOperatorStateType.Error), ZeroFrameType.JsonValue,ZeroFrameType.GlobalId
            };
            var message = new ZMessage
            {
                new ZFrame(item.Caller.ToAsciiBytes()),
                new ZFrame("".ToAsciiBytes()),
                new ZFrame(description),
                new ZFrame((item.Result ?? "").ToAsciiBytes()),
                new ZFrame((item.GlobalId ?? "").ToAsciiBytes())
            };
            //lock (_inprocCallSocket)
            //{
            //    using (var frames = new ZMessage
            //    {
            //        new ZFrame(item.Caller.ToAsciiBytes()),
            //        new ZFrame("".ToAsciiBytes()),
            //        new ZFrame(description),
            //        new ZFrame((item.Result ?? "").ToAsciiBytes()),
            //        new ZFrame((item.GlobalId ?? "").ToAsciiBytes())
            //    })
            //    {
            //        var ok = _inprocCallSocket.Send(frames, out var error);
            //        if (error != null)
            //            ZeroTrace.WriteError("SendResult", error.Text);
            //        return ok;
            //    }
            //}

            ZError error;
            using (message)
            {
                if (socket.Send(message, out error))
                    return true;
                socket.CloseSocket();
                socket = ZeroHelper.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER, Identity);
            }
            ZeroTrace.WriteError(StationName, error.Text, error.Name);
            Interlocked.Increment(ref SendError);
            return false;
        }

        #endregion
    }
}