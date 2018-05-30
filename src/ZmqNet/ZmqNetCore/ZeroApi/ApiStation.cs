using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public ApiAction RegistAction<TResult>(string name, Func<TResult> action, ApiAccessOption access) where TResult : ApiResult
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
        public ApiAction RegistAction<TArgument, TResult>(string name, Func<TArgument, TResult> action, ApiAccessOption access)
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

        #endregion

        #region Api调用
        /// <summary>
        /// 调用计数
        /// </summary>
        public int CallCount, ErrorCount, SuccessCount, RecvCount, SendCount, SendError;

        /// <summary>
        /// 构造
        /// </summary>
        public ApiStation() : base(StationTypeApi)
        {

        }
        /// <summary>
        /// 执行前的处理
        /// </summary>
        public static List<Action<ApiStation, ApiCallItem>> PreActions { get; } = new List<Action<ApiStation, ApiCallItem>>();
        /// <summary>
        /// 执行后的处理
        /// </summary>
        public static List<Action<ApiStation, ApiCallItem>> EndActions { get; } = new List<Action<ApiStation, ApiCallItem>>();


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

        private ZSocket resultCallSocket, resultPoolSocket, outSocket;


        /// <summary>
        /// 初始化
        /// </summary>
        protected sealed override void OnStart()
        {
            base.OnStart();
            string name = $"inproc://{StationName}";
            resultCallSocket = ZeroHelper.CreateClientSocket(name, ZSocketType.PAIR, name.ToAsciiBytes());
            resultPoolSocket = ZeroHelper.CreateServiceSocket(name, ZSocketType.PAIR, name.ToAsciiBytes());
            outSocket = ZeroHelper.CreateClientSocket(Config.WorkerAddress, ZSocketType.DEALER, Identity);
        }
        #endregion

        #region 命令

        /// <inheritdoc />
        /// <summary>
        ///     命令轮询
        /// </summary>
        /// <returns></returns>
        protected sealed override bool Run()
        {
            ZeroTrace.WriteInfo(StationName, "run...");

            var sockets = new[] { outSocket, resultPoolSocket };
            var pollItems = new[] { ZPollItem.CreateReceiver(), ZPollItem.CreateReceiver() };
            InPoll = true;
            State = StationState.Run;
            while (CanRun)
            {
                ZMessage[] messages = null;
                if (!sockets.PollIn(pollItems, out messages, out var error, new TimeSpan(0, 0, 0,0,500)))
                {
                    if (error != null && !Equals(error, ZError.EAGAIN))
                    {
                        ZeroTrace.WriteError(StationName, error.Text, error.Name);
                    }
                }
                else
                {
                    if (messages[0] != null)
                    {
                        //Interlocked.Increment(ref RecvCount);
                        Task.Factory.StartNew(ApiCall, messages[0].Clone());
                    }
                    if (messages[1] != null)
                    {
                        if (!outSocket.Send(messages[1], out error) || error != null)
                        {
                            ZeroTrace.WriteError(StationName, error.Text, error.Name);
                            //Interlocked.Increment(ref SendError);
                        }
                        //Interlocked.Increment(ref SendCount);
                    }
                }
                //if (CallCount % 100 == 0)
                //    ZeroTrace.WriteLoop("Run", $"count:{CallCount} success:{SuccessCount} error:{ErrorCount} send:{SendCount}|{SendError} recv:{RecvCount} MemAlive:{MemoryCheck.AliveCount}");

                if (messages == null) continue;
                foreach (var messsage in messages)
                {
                    messsage?.Dispose();
                }
            }
            InPoll = false;
            ZeroTrace.WriteInfo(StationName, "poll stop");
            return true;
        }

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        protected override void OnTaskStop()
        {
            outSocket.CloseSocket();
            resultCallSocket.CloseSocket();
            resultPoolSocket.CloseSocket();
            base.OnTaskStop();
        }

        #endregion

        #region Socket

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool ReceiveApiCall(ZSocket socket, out ApiCallItem item)
        {
            ZMessage messages;
            try
            {
                messages = socket.ReceiveMessage(out var error);
                if (error != null || messages.Count < 3)
                {
                    if (error != null && error.Number != 11)
                        ZeroTrace.WriteError("Receive", error.Text, socket.Connects.LinkToString(','), $"Socket Ptr:{ socket.SocketPtr}.");
                    item = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteError("Receive", "Exception", socket.Connects.LinkToString(','), $"Socket Ptr:{ socket.SocketPtr}.", e);
                LogRecorder.Exception(e);
                item = null;
                return false;
            }

            return ReceiveApiCall(messages, out item);
        }

        private void ApiCall(object msg)
        {
            if (!ReceiveApiCall((ZMessage)msg, out var item))
                return;
            //using (MonitorScope.CreateScope(item.ApiName))
            {
                Interlocked.Increment(ref CallCount);
                //LogRecorder.MonitorTrace($"Caller:{item.Caller}");
                //LogRecorder.MonitorTrace($"GlobalId:{item.GlobalId}");
                //LogRecorder.MonitorTrace(JsonConvert.SerializeObject(item));
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
                    //using (MonitorStepScope.CreateScope("Do"))
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
                if (!SendResult(item, success))
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
        }

        private bool ReceiveApiCall(ZMessage messages, out ApiCallItem item)
        {
            try
            {
                var description = messages[2].Read();
                if (description.Length < 2)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", outSocket.Connects.LinkToString(','),
                        description.LinkToString(p => p.ToString("X2"), ""), $"Socket Ptr:{outSocket.SocketPtr}.");
                    item = null;
                    return false;
                }

                int end = description[0] + 3;
                if (end != messages.Count)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", outSocket.Connects.LinkToString(','),
                        $"FrameSize{messages.Count}", description.LinkToString(p => p.ToString("X2"), ""),
                        $"Socket Ptr:{outSocket.SocketPtr}.");
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
                ZeroTrace.WriteError("Receive", "Exception", outSocket.Connects.LinkToString(','),
                    $"FrameSize{messages.Count}.Socket Ptr:{outSocket.SocketPtr}.");
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
        /// <param name="item"></param>
        /// <param name="success"></param>
        /// <returns></returns>
        private bool SendResult(ApiCallItem item, bool success)
        {
            if (!InPoll)
            {
                ZeroTrace.WriteError("SendResult", "is closed", StationName);
                return false;
            }
            var description = new byte[]
            {
                2, (byte) (success ? ZeroOperatorStateType.Ok : ZeroOperatorStateType.Error), ZeroFrameType.JsonValue,ZeroFrameType.GlobalId
            };
            lock (resultCallSocket)
            {
                using (var frames = new ZMessage
                {
                    new ZFrame(item.Caller.ToAsciiBytes()),
                    new ZFrame("".ToAsciiBytes()),
                    new ZFrame(description),
                    new ZFrame((item.Result ?? "").ToAsciiBytes()),
                    new ZFrame((item.GlobalId ?? "").ToAsciiBytes())
                })
                {
                    var ok = resultCallSocket.Send(frames, out var error);
                    if (error != null)
                        ZeroTrace.WriteError("SendResult", error.Text);
                    return ok;
                }
            }
        }

        #endregion
    }
}