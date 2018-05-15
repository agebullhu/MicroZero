using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.ZeroApi;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using ErrorCode = Agebull.ZeroNet.ZeroApi.ErrorCode;

namespace Agebull.ZeroNet.Core
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
                StationConsole.WriteLine($"{StationName}:{name} is registed");
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
        ///     类型
        /// </summary>
        public override int StationType => StationTypeApi;

        private readonly byte[] _responseDescription = new byte[] { 2, ZeroFrameType.Status, ZeroFrameType.JsonValue };
        /// <summary>
        ///     执行命令
        /// </summary>
        private void ExecCommand(object arg)
        {
            var item = (ApiCallItem)arg;
            //StationConsole.WriteLine($"{item.Caller}=>{RealName}");
            var response = ExecCommand(item);
            _socket.SendMoreFrame(item.Caller);
            _socket.SendMoreFrameEmpty();


            _socket.SendMoreFrame(_responseDescription);
            _socket.SendMoreFrame(response ? ZeroNetStatus.ZeroCommandOk : ZeroNetStatus.ZeroCommandError);
            _socket.SendFrame(item.Result);
        }
        /// <summary>
        ///     执行命令
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool ExecCommand(ApiCallItem item)
        {
            if (!_apiActions.TryGetValue(item.ApiName, out var action))
            {
                item.Result = ZeroNetStatus.NoFindJson;
                item.Status = ErrorCode.NoFind;
                return false;
            }

            //1 还原调用上下文
            try
            {
                if (!string.IsNullOrWhiteSpace(item.ContextJson))
                {
                    var context = JsonConvert.DeserializeObject<ApiContext>(item.ContextJson);
                    ApiContext.SetContext(context);
                }
                else
                {
                    ApiContext.TryCheckByAnymouse();
                    ApiContext.RequestContext.SetValue(item.Requester, item.RequestId);
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                item.Result = ZeroNetStatus.ArgumentErrorJson;
                item.Status = ErrorCode.ArgumentError;
                return false;
            }

            //2 确定调用方法及对应权限
            try
            {
                if (action.NeedLogin && ApiContext.Customer.UserId <= 0)
                {
                    item.Result = ZeroNetStatus.DenyAccessJson;
                    item.Status = ErrorCode.DenyAccess;
                    return false;
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                item.Result = ZeroNetStatus.ArgumentErrorJson;
                item.Status = ErrorCode.InnerError;
                return false;
            }

            //3 参数校验
            try
            {
                if (!action.RestoreArgument(item.Argument))
                {
                    item.Result = ZeroNetStatus.ArgumentErrorJson;
                    item.Status = ErrorCode.ArgumentError;
                    return false;
                }

                if (!action.Validate(out var message))
                {
                    item.Result = JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.ArgumentError, message));
                    item.Status = ErrorCode.ArgumentError;
                    return false;
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                item.Result = ZeroNetStatus.ArgumentErrorJson;
                item.Status = ErrorCode.ArgumentError;
                return false;
            }

            //4 方法执行
            try
            {
                var result = action.Execute();
                item.Result = result == null ? ZeroNetStatus.SucceesJson : JsonConvert.SerializeObject(result);
                item.Status = ErrorCode.Success;
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                item.Result = ZeroNetStatus.InnerErrorJson;
                item.Status = ErrorCode.InnerError;
                return false;
            }
        }

        #endregion

        #region 网络与执行

        /// <summary>
        ///     尝试重启次数
        /// </summary>
        private int _tryCount;
        
        /// <summary>
        ///     尝试重启
        /// </summary>
        /// <returns></returns>
        private bool TryRun()
        {
            if (++_tryCount > 9)
                return false;
            Thread.Sleep(1000);
            Task.Factory.StartNew(Run);
            return true;
        }
        /// <inheritdoc />
        /// <summary>
        ///     命令轮询
        /// </summary>
        /// <returns></returns>
        protected sealed override bool Run()
        {
            StationConsole.WriteLine($"【{StationName}】start...");

            InPoll = false;
            InHeart = false;
            RunState = StationState.Start;

            if (!CreateSocket())
            {
                Task.Factory.StartNew(TryRun);
                return false;
            }
            RunState = StationState.Run;
            _tryCount = 0;

            Task.Factory.StartNew(PollTask).ContinueWith(task => OnTaskStop());
            //Task.Factory.StartNew(HeartbeatTask).ContinueWith(task => OnTaskStop());

            while (!InHeart || !InPoll)
                Thread.Sleep(50);

            StationConsole.WriteLine($"【{StationName}】runing...");
            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        protected sealed override void OnTaskStop()
        {
            if (InPoll || InHeart)
                return;
            if (ZeroApplication.State == StationState.Run && RunState == StationState.Failed)
            {
                StationConsole.WriteInfo($"【{RealName}】restart...");
                TryRun();
                return;
            }
            if (RunState == StationState.Closing)
                RunState = StationState.Closed;
        }
        #endregion

        #region 命令

        /// <summary>
        ///     命令轮询
        /// </summary>
        /// <returns></returns>
        private void PollTask()
        {
            StationConsole.WriteLine($"【{RealName}】poll start");
            var timeout = new TimeSpan(0, 0, 5);
            InPoll = true;
            while (RunState == StationState.Run && RunState == StationState.Run)
            {
                try
                {
                    if (!_socket.TryReceiveFrameString(timeout, out var caller, out var more) || !more)
                        continue;
                    var item = new ApiCallItem
                    {
                        Caller = caller
                    };

                    _socket.TryReceiveFrameBytes(out var description, out more);

                    var idx = 1;
                    while (more)
                    {
                        if (!_socket.TryReceiveFrameString(out var val, out more))
                            continue;
                        switch (description[idx++])
                        {
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
                    Task.Factory.StartNew(ExecCommand, item);
                }
                catch (Exception e)
                {
                    StationConsole.WriteError($"【{RealName}】poll error{e.Message}...");
                    LogRecorder.Exception(e);
                    RunState = StationState.Failed;
                }
            }

            InPoll = false;
            StationConsole.WriteInfo($"【{RealName}】poll stop");
            //_socket.TrySendFrame(timeout, ZeroByteCommand.WorkerLeft);

            _socket.CloseSocket(Config.WorkerAddress);
        }
       

        /// <summary>
        ///     使用的套接字
        /// </summary>
        private DealerSocket _socket;

        private bool CreateSocket()
        {
            _socket.CloseSocket(Config.WorkerAddress);
            _socket = new DealerSocket();
            try
            {
                _socket.Options.Identity = Identity;
                _socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                _socket.Options.DisableTimeWait = true;
                _socket.Connect(Config.WorkerAddress);
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                StationConsole.WriteError($"【{RealName}】connect error =>{e.Message}");
                return false;
            }
        }


        #endregion

        #region 心跳


        /// <summary>
        ///     心跳
        /// </summary>
        /// <returns></returns>
        private void HeartbeatTask()
        {
            InHeart = true;
            var errorCount = 0;
            StationConsole.WriteLine($"【{RealName}】heartbeat start");
            //连接
            var socket = new DealerSocket();
            socket.Options.Identity = Identity;
            socket.Options.ReconnectInterval = new TimeSpan(0, 0, 1);
            socket.Options.DisableTimeWait = true;
            socket.Connect(Config.HeartAddress);

            var ts = new TimeSpan(0, 0, 3);
            //收完消息
            Msg msg = new Msg();
            msg.InitEmpty();
            while (socket.TryReceive(ref msg, ts))
            {
                msg.InitEmpty();
            }
            //第一次登记
            List<string> result;
            while (RunState == StationState.Run && RunState == StationState.Run)
            {
                socket.SendMoreFrame(ZeroByteCommand.HeartJoin);
                socket.SendFrame(RealName);
                if (!socket.ReceiveString(out result))
                {
                    ++errorCount;
                    StationConsole.WriteError($"【{RealName}】heartbeat timeout({errorCount})...");
                    Thread.Sleep(100);
                }
                break;
            } 
            //正常跳
            while (RunState == StationState.Run && RunState == StationState.Run)
            {
                try
                {
                    socket.SendFrame(ZeroByteCommand.HeartPitpat,true);
                    socket.SendFrame(RealName);
                    if(!socket.ReceiveString(out result))
                    {
                        ++errorCount;
                        StationConsole.WriteError($"【{RealName}】heartbeat timeout({errorCount})...");
                        Thread.Sleep(100);
                        continue;
                    }
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    ++errorCount;
                    StationConsole.WriteError($"【{RealName}】heartbeat exception({errorCount}):{e.Message}...");
                    Thread.Sleep(100);
                    continue;
                }
                var str = result.FirstOrDefault(p=>!string.IsNullOrEmpty(p));
                if (str==null || str[0] == ZeroNetStatus.ZeroStatusBad)
                {
                    ++errorCount;
                    StationConsole.WriteError($"【{RealName}】heartbeat request failed({errorCount}):{result[0]}...");
                    Thread.Sleep(100);
                    continue;
                }

                if (errorCount > 0)
                {
                    errorCount = 0;
                    StationConsole.WriteInfo($"【{RealName}】heartbeat resume...");
                }
                for (int i = 0; i < 20 && RunState == StationState.Run && RunState == StationState.Run; i++)
                {
                    Thread.Sleep(250);
                }
            }
            //退出
            try
            {
                socket.SendMoreFrame(ZeroByteCommand.HeartLeft);
                socket.SendFrame(RealName);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
            //关闭
            try
            {
                socket.Disconnect(Config.HeartAddress);
                socket.Close();
                socket.Dispose();
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
            StationConsole.WriteLine($"【{RealName}】heartbeat stop");
            InHeart = false;
        }


        #endregion
    }
}