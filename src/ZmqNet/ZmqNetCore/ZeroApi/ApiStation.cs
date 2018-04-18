using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// Api站点
    /// </summary>
    public class ApiStation : ZeroStation
    {
        #region 注册方法


        private readonly Dictionary<string, ApiAction> _apiActions =
            new Dictionary<string, ApiAction>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 注册方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void RegistAction(string name, ApiAction action)
        {
            if (!_apiActions.ContainsKey(name))
            {
                action.Name = name;
                _apiActions.Add(name, action);
                StationProgram.WriteLine($"{StationName}:{name} is registed");
            }
        }
        /// <summary>
        /// 注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="needLogin">是否需要登录</param>
        public void RegistAction(string name, Func<IApiResult> action, bool needLogin)
        {
            RegistAction(name, new ApiAction<IApiResult>
            {
                Name = name,
                Action = action
            });
        }
        /// <summary>
        /// 注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="needLogin">是否需要登录</param>
        public void RegistAction(string name, Func<IApiArgument, IApiResult> action, bool needLogin)
        {
            RegistAction(name, new ApiAction<IApiArgument, IApiResult>
            {
                Name = name,
                Action = action
            });
        }

        /// <summary>
        /// 注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="needLogin">是否需要登录</param>
        public void RegistAction<TResult>(string name, Func<TResult> action, bool needLogin) where TResult : ApiResult
        {
            RegistAction(name, new ApiAction<TResult>
            {
                Name = name,
                Action = action
            });
        }

        /// <summary>
        /// 注册方法
        /// </summary>
        /// <param name="name">方法外部方法名称，如 v1\auto\getdid </param>
        /// <param name="action">动作</param>
        /// <param name="needLogin">是否需要登录</param>
        public void RegistAction<TArgument, TResult>(string name, Func<TArgument, TResult> action, bool needLogin)
            where TArgument : class, IApiArgument
            where TResult : ApiResult
        {
            RegistAction(name, new ApiAction<TArgument, TResult>
            {
                Name = name,
                Action = action
            });
        }

        /// <summary>
        /// 注册方法
        /// </summary>
        public void RegistAction<TControler>()
            where TControler : class, new()
        {
            Debug.WriteLine($"//{typeof(TControler).FullName}");
            foreach (var action in typeof(TControler).GetMethods())
            {
                if (action == null) continue;
                var attributes = action.GetCustomAttributesData();
                var route = attributes?.FirstOrDefault(p => p.AttributeType == typeof(RouteAttribute));
                if (route == null)
                    continue;
                var name = route.ConstructorArguments.FirstOrDefault().Value?.ToString();
                var optionAtt = attributes.FirstOrDefault(p => p.AttributeType == typeof(ApiAccessOptionFilterAttribute));
                var option = ApiAccessOption.None;
                if (optionAtt != null && optionAtt.ConstructorArguments.Count > 0)
                    option = (ApiAccessOption)optionAtt.ConstructorArguments[0].Value;

                Debug.WriteLine($@"
                oAuthStation.RegistAction<{action.GetParameters().FirstOrDefault()?.ParameterType.Name},{action.ReturnType.Name}>(""{name}"",arg =>
                {{
                    var ctl = new {typeof(TControler).Name}();
                    return ctl.{action.Name}(arg);
                }},{((option & ApiAccessOption.Customer) == ApiAccessOption.Customer).ToString().ToLower()});");
            }
        }

        #endregion

        #region Api调用
        /// <summary>
        /// 类型
        /// </summary>
        public override int StationType => StationTypeApi;

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool ExecCommand(ApiCallItem item)
        {
            if (!_apiActions.TryGetValue(item.ApiName, out var action))
            {
                item.Result = ZeroNetStatus.NoFindJson;
                item.Status = Agebull.ZeroNet.ZeroApi.ErrorCode.NoFind;
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
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                item.Result = ZeroNetStatus.ArgumentErrorJson;
                item.Status = Agebull.ZeroNet.ZeroApi.ErrorCode.ArgumentError;
                return false;
            }
            ApiContext.RequestContext.ServiceKey = item.Requester;
            ApiContext.RequestContext.RequestId = item.RequestId;
            ApiContext.TryCheckByAnymouse();
            //2 确定调用方法及对应权限
            try
            {
                if (action.NeedLogin && ApiContext.Customer.UserId <= 0)
                {
                    item.Result = ZeroNetStatus.DenyAccessJson;
                    item.Status = Agebull.ZeroNet.ZeroApi.ErrorCode.DenyAccess;
                    return false;
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                item.Result = ZeroNetStatus.ArgumentErrorJson;
                item.Status = Agebull.ZeroNet.ZeroApi.ErrorCode.InnerError;
                return false;
            }
            //3 参数校验
            try
            {
                if (!action.RestoreArgument(item.Argument))
                {
                    item.Result = ZeroNetStatus.ArgumentErrorJson;
                    item.Status = Agebull.ZeroNet.ZeroApi.ErrorCode.ArgumentError;
                    return false;
                }
                if (!action.Validate(out var message))
                {
                    item.Result = JsonConvert.SerializeObject(ApiResult.Error(ZeroApi.ErrorCode.ArgumentError, message));
                    item.Status = Agebull.ZeroNet.ZeroApi.ErrorCode.ArgumentError;
                    return false;
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                item.Result = ZeroNetStatus.ArgumentErrorJson;
                item.Status = Agebull.ZeroNet.ZeroApi.ErrorCode.ArgumentError;
                return false;
            }
            //4 方法执行
            try
            {
                var result = action.Execute();
                item.Result = result == null ? ZeroNetStatus.SucceesJson : JsonConvert.SerializeObject(result);
                item.Status = Agebull.ZeroNet.ZeroApi.ErrorCode.Success;
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                item.Result = ZeroNetStatus.InnerErrorJson;
                item.Status = Agebull.ZeroNet.ZeroApi.ErrorCode.InnerError;
                return false;
            }
        }

        #endregion

        #region 网络与执行

        readonly Dictionary<string, ApiStationItem> items = new Dictionary<string, ApiStationItem>();

        /// <inheritdoc />
        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        public sealed override bool Run()
        {
            StationProgram.WriteLine($"【{StationName}:{RealName}】start...");

            if (RunState == StationState.Run)
                return false;

            InPoll = false;
            InHeart = false;
            if (items.Count == 0)
            {
                for (int idx = 0; idx < 32; idx++)
                {
                    var item = new ApiStationItem
                    {
                        Station = this,
                        RealName = RandomOperate.Generate(8)
                    };
                    items.Add(item.RealName, item);
                }
            }
            RunState = StationState.Run;
            foreach (var item in items.Values)
            {
                item.Run();
            }

            while (items.Values.Any(item => !item.InHeart || !item.InPoll))
                Thread.Sleep(50);

            StationProgram.WriteLine($"【{StationName}:{RealName}】runing...");
            return true;
        }

        #endregion

        /// <summary>
        /// Api站点
        /// </summary>
        public class ApiStationItem
        {
            /// <summary>
            /// 站点
            /// </summary>
            internal ApiStation Station
            {
                private get;
                set;
            }
            /// <summary>
            /// 在心跳
            /// </summary>
            internal bool InHeart;
            /// <summary>
            /// 在侦听
            /// </summary>
            internal bool InPoll;
            /// <summary>
            /// 实名
            /// </summary>
            public string RealName { get; set; }
            #region 网络与执行

            /// <summary>
            /// 使用的套接字
            /// </summary>
            private RequestSocket _socket;

            /// <summary>
            /// 运行状态
            /// </summary>
            public StationState RunState { get; set; }

            /// <summary>
            /// 尝试重启次数
            /// </summary>
            private int _tryCount;
            /// <summary>
            /// 尝试重启
            /// </summary>
            /// <returns></returns>
            private bool TryRun()
            {
                if (++_tryCount > 9)
                    return false;
                CloseSocket();
                Thread.Sleep(1000);
                Task.Factory.StartNew(Run);
                return true;
            }
            /// <summary>
            /// 命令轮询
            /// </summary>
            /// <returns></returns>
            public bool Run()
            {
                StationProgram.WriteLine($"【{Station.StationName}:{RealName}】start...");
                InPoll = false;
                InHeart = false;

                _socket = new RequestSocket();
                try
                {
                    _socket.Options.Identity = RealName.ToAsciiBytes();
                    _socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                    _socket.Connect(Station.Config.InnerAddress);
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    StationProgram.WriteError($"【{Station.StationName}:{RealName}】connect error =>{e.Message}");
                    return TryRun();
                }
                try
                {
                    var word = _socket.Request(ZeroByteCommand.WorkerJoin);
                    if (word == null || word[0] != ZeroNetStatus.ZeroStatusSuccess)
                    {
                        StationProgram.WriteError($"【{Station.StationName}:{RealName}】 proto error");
                        return TryRun();
                    }
                    word = Station.Config.HeartAddress.RequestNet(ZeroByteCommand.HeartJoin, RealName);
                    if (word == null || word[0] != ZeroNetStatus.ZeroStatusSuccess)
                    {
                        return TryRun();
                    }
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    StationProgram.WriteError($"【{Station.StationName}:{RealName}】request error =>{e.Message}");
                    return TryRun();
                }

                RunState = StationState.Run;

                var task1 = Task.Factory.StartNew(PollTask);
                task1.ContinueWith(Station.OnTaskStop);

                var task2 = Task.Factory.StartNew(HeartbeatTask);
                task2.ContinueWith(Station.OnTaskStop);

                while (!InHeart || !InPoll)
                    Thread.Sleep(50);
                StationProgram.WriteLine($"【{Station.StationName}:{RealName}】runing...");
                return true;
            }

            /// <summary>
            /// 心跳
            /// </summary>
            /// <returns></returns>
            private void HeartbeatTask()
            {
                InHeart = true;
                var errorCount = 0;
                StationProgram.WriteLine($"【{Station.StationName}:{RealName}】heartbeat start");
                try
                {
                    while (Station.RunState == StationState.Run && RunState == StationState.Run)
                    {
                        Thread.Sleep(5000);
                        var result = Station.Config.HeartAddress.RequestNet(ZeroByteCommand.HeartPitpat, RealName);
                        if (string.IsNullOrWhiteSpace(result))
                        {
                            if (++errorCount > 3)
                            {
                                RunState = StationState.Failed;
                            }
                            StationProgram.WriteError($"【{Station.StationName}:{RealName}】heartbeat error {errorCount}...");
                        }
                        else if (result[0] == ZeroNetStatus.ZeroStatusBad)
                        {
                            RunState = StationState.Failed;
                        }
                        else if (errorCount > 0)
                        {
                            errorCount = 0;
                            StationProgram.WriteInfo($"【{Station.StationName}:{RealName}】heartbeat resume...");
                        }
                    }
                    //退出
                    Station.Config.HeartAddress.RequestNet(ZeroByteCommand.HeartLeft, RealName);
                }
                catch (Exception e)
                {
                    StationProgram.WriteError($"【{Station.StationName}:{RealName}】heartbeat error{e.Message}...");
                    RunState = StationState.Failed;
                    LogRecorder.Exception(e);
                }
                StationProgram.WriteLine($"【{Station.StationName}:{RealName}】heartbeat stop");
                InHeart = false;
            }

            /// <summary>
            /// 命令轮询
            /// </summary>
            /// <returns></returns>
            private void PollTask()
            {
                StationProgram.WriteLine($"【{Station.StationName}:{RealName}】poll start");
                var timeout = new TimeSpan(0, 0, 5);
                try
                {
                    if (!_socket.TrySendFrame(timeout, ZeroByteCommand.WorkerListen))
                    {
                        RunState = StationState.Failed;
                        return;
                    }
                    InPoll = true;
                    while (RunState == StationState.Run)
                    {

                        if (!_socket.TryReceiveFrameBytes(timeout, out var description, out var more))
                        {
                            continue;
                        }

                        if (!more)
                        {
                            if (!_socket.TrySendFrame(timeout, ZeroByteCommand.WorkerListen))
                            {
                                RunState = StationState.Failed;
                                break;
                            }

                            continue;
                        }

                        ApiCallItem item = new ApiCallItem();

                        int idx = 1;
                        while (more)
                        {
                            if (!_socket.TryReceiveFrameString(out var val, out more))
                            {
                                continue;
                            }
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
                        var response = Station.ExecCommand(item);
                        _socket.SendMoreFrame(item.Requester);
                        _socket.SendMoreFrameEmpty();
                        _socket.SendMoreFrame(response ? ZeroNetStatus.ZeroCommandOk : ZeroNetStatus.ZeroCommandError);
                        description[0] = 1;
                        description[1] = ZeroFrameType.JsonValue;
                        _socket.SendMoreFrame(description);
                        _socket.SendFrame(item.Result);
                    }
                }
                catch (Exception e)
                {
                    StationProgram.WriteError($"【{Station.StationName}:{RealName}】poll error{e.Message}...");
                    LogRecorder.Exception(e);
                    RunState = StationState.Failed;
                }
                InPoll = false;
                StationProgram.WriteInfo($"【{Station.StationName}:{RealName}】poll stop");
                CloseSocket();
            }
            /// <summary>
            /// 关闭套接字
            /// </summary>
            private void CloseSocket()
            {
                lock (this)
                {
                    if (_socket == null)
                        return;
                    try
                    {
                        _socket.Disconnect(Station.Config.InnerAddress);
                    }
                    catch (Exception)
                    {
                        //LogRecorder.Exception(e);//一般是无法连接服务器而出错
                    }
                    _socket.Close();
                    _socket = null;
                }
            }

            /// <summary>
            /// 命令轮询
            /// </summary>
            /// <returns></returns>
            protected virtual void OnTaskStop(Task task)
            {
                while (InPoll || InHeart)
                    Thread.Sleep(50);
                if (StationProgram.State == StationState.Run && (RunState == StationState.Failed || RunState == StationState.Start))
                {
                    StationProgram.WriteInfo($"【{Station.StationName}:{RealName}】restart...");
                    _tryCount = 0;
                    TryRun();
                    return;
                }
                if (RunState == StationState.Closing)
                    RunState = StationState.Closed;
            }
            #endregion

        }
    }
}