using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common;
using Agebull.Common.Logging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Agebull.ZeroNet.ZeroApi;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// Api站点
    /// </summary>
    public class ApiStation : ZeroStation
    {
        #region Api调用

        /// <summary>
        /// 类型
        /// </summary>
        public override int StationType => StationTypeApi;


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

        /// <summary>
        /// 参数错误的Json文本
        /// </summary>
        /// <remarks>参数校验不通过</remarks>
        public static readonly string ArgumentError = JsonConvert.SerializeObject(ApiResult.Error(ZeroApi.ErrorCode.ArgumentError));

        /// <summary>
        /// 拒绝访问的Json文本
        /// </summary>
        /// <remarks>权限校验不通过</remarks>
        public static readonly string DenyAccess = JsonConvert.SerializeObject(ApiResult.Error(ZeroApi.ErrorCode.DenyAccess));

        /// <summary>
        /// 未知错误的Json文本
        /// </summary>
        public static readonly string UnknowError = JsonConvert.SerializeObject(ApiResult.Error(ZeroApi.ErrorCode.UnknowError));

        /// <summary>
        /// 网络错误的Json文本
        /// </summary>
        /// <remarks>调用其它Api时时抛出未处理异常</remarks>
        public static readonly string NetworkError = JsonConvert.SerializeObject(ApiResult.Error(ZeroApi.ErrorCode.NetworkError));

        /// <summary>
        /// 内部错误的Json文本
        /// </summary>
        /// <remarks>执行方法时抛出未处理异常</remarks>
        public static readonly string InnerError = JsonConvert.SerializeObject(ApiResult.Error(ZeroApi.ErrorCode.InnerError));

        /// <summary>
        /// 找不到方法的Json文本
        /// </summary>
        /// <remarks>方法未注册</remarks>
        public static readonly string NoFind = JsonConvert.SerializeObject(ApiResult.Error(ZeroApi.ErrorCode.NoFind));

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public string ExecCommand(List<string> args)
        {
            //1 还原调用上下文
            try
            {
                var context = JsonConvert.DeserializeObject<ApiContext>(args[4]);
                ApiContext.SetContext(context);
                ApiContext.TryCheckByAnymouse();
                ApiContext.RequestContext.ServiceKey = args[0];
                ApiContext.RequestContext.RequestId = args[3];
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return DenyAccess;
            }
            // 确定调用方法及对应权限
            ApiAction action;
            try
            {
                if (!_apiActions.TryGetValue(args[2], out action))
                {
                    Console.WriteLine(args.LinkToString(" , "));
                    return NoFind;
                }
                if (action.NeedLogin && ApiContext.Customer.UserId <= 0)
                {
                    return DenyAccess;
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return DenyAccess;
            }
            //3 参数校验
            try
            {
                if (!action.RestoreArgument(args[5]))
                {
                    return ArgumentError;
                }
                if (!action.Validate(out var message))
                {
                    return JsonConvert.SerializeObject(ApiResult.Error(ZeroApi.ErrorCode.ArgumentError, message));
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return ArgumentError;
            }
            //4 方法执行
            try
            {
                var result = action.Execute();
                return JsonConvert.SerializeObject(result ?? ApiResult.Error(-3));
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                return InnerError;
            }
        }

        #endregion

        #region 网络与执行

        /// <summary>
        /// 使用的套接字
        /// </summary>
        private RequestSocket _socket;
        
        /// <inheritdoc />
        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        public sealed override bool Run()
        {
            StationProgram.WriteLine($"【{StationName}:{RealName}】start...");
            lock (this)
            {
                if (RunState != StationState.Run)
                    RunState = StationState.Start;
                if (_socket != null)
                    return false;
            }
            InPoll = false;
            InHeart = false;

            _socket = new RequestSocket();
            try
            {
                _socket.Options.Identity = RealName.ToAsciiBytes();
                _socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 200);
                _socket.Connect(Config.InnerAddress);
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                RunState = StationState.Failed;
                CloseSocket();
                StationProgram.WriteError($"【{StationName}:{RealName}】connect error =>{e.Message}");
                return false;
            }
            try
            {
                var word = _socket.Request("@", JsonConvert.SerializeObject(Config));
                if (word != "wecome")
                {
                    CloseSocket();
                    RunState = StationState.Failed;
                    StationProgram.WriteError($"【{StationName}:{RealName}】proto error");
                    return false;
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                RunState = StationState.Failed;
                CloseSocket();
                StationProgram.WriteError($"【{StationName}:{RealName}】request error =>{e.Message}");
                return false;
            }

            RunState = StationState.Run;
            _items = MulitToOneQueue<List<string>>.Load(CacheFileName);
            Task.Factory.StartNew(CommandTask);

            var task1 = Task.Factory.StartNew(PollTask);
            task1.ContinueWith(OnTaskStop);

            var task2 = Task.Factory.StartNew(HeartbeatTask);
            task2.ContinueWith(OnTaskStop);

            while (!InHeart || !InPoll)
                Thread.Sleep(50);
            StationProgram.WriteLine($"【{StationName}:{RealName}】runing...");
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
            StationProgram.WriteLine($"【{StationName}:{RealName}】heartbeat start");
            try
            {
                while (RunState == StationState.Run)
                {
                    Thread.Sleep(5000);
                    var result = Config.HeartAddress.RequestNet("@", RealName);
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        if (++errorCount > 3)
                        {
                            RunState = StationState.Failed;
                        }
                        StationProgram.WriteError($"【{StationName}:{RealName}】heartbeat error{errorCount}...");
                    }
                    else if (errorCount > 0)
                    {
                        errorCount = 0;
                        StationProgram.WriteInfo($"【{StationName}:{RealName}】heartbeat resume...");
                    }
                }
                //退出
                Config.HeartAddress.RequestNet("-", RealName);
            }
            catch (Exception e)
            {
                StationProgram.WriteError($"【{StationName}:{RealName}】heartbeat error{e.Message}...");
                RunState = StationState.Failed;
                LogRecorder.Exception(e);
            }
            StationProgram.WriteLine($"【{StationName}:{RealName}】heartbeat stop");
            InHeart = false;
        }
        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        private void PollTask()
        {
            _socket.SendString("*", "*");
            InPoll = true;
            StationProgram.WriteLine($"【{StationName}:{RealName}】poll start");
            //var timeout = new TimeSpan(0, 0, 5);
            try
            {
                while (RunState == StationState.Run)
                {
                    //if (!socket.Poll(timeout))
                    //    continue;
                    //if (!socket.HasIn)
                    //    continue;
                    //string caller = socket.ReceiveFrameString();
                    //string command = socket.ReceiveFrameString();
                    //string argument = socket.ReceiveFrameString();
                    //string response = ExecCommand(command, argument);
                    //socket.SendMoreFrame(caller);
                    if (!_socket.ReceiveString(out var arg, 0))
                    {
                        continue;
                    }
                    _items.Push(arg);
                }
            }
            catch (Exception e)
            {
                StationProgram.WriteError($"【{StationName}:{RealName}】poll error{e.Message}...");
                LogRecorder.Exception(e);
                RunState = StationState.Failed;
            }
            InPoll = false;
            StationProgram.WriteInfo($"【{StationName}:{RealName}】poll stop");

            _items.Save(CacheFileName);
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
                    _socket.Disconnect(Config.InnerAddress);
                }
                catch (Exception)
                {
                    //LogRecorder.Exception(e);//一般是无法连接服务器而出错
                }
                _socket.Close();
                _socket = null;
            }
        }

        #endregion

        #region 请求队列

        /// <summary>
        /// 缓存文件名称
        /// </summary>
        private string CacheFileName => Path.Combine(StationProgram.Config.DataFolder,
            $"zero_sub_queue_{StationName}.json");

        /// <summary>
        /// 请求队列
        /// </summary>
        private static MulitToOneQueue<List<string>> _items = new MulitToOneQueue<List<string>>();


        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        private void CommandTask()
        {
            while (RunState == StationState.Run)
            {
                if (!_items.StartProcess(out var item, 1000))
                    continue;
                var response = ExecCommand(item);
                _socket.SendMoreFrame(item[0]);
                _socket.SendFrame(response);
                //StationProgram.WriteLine($"【{StationName}:{RealName}】call {arg.LinkToString(",")}=>{response}");
                _items.EndProcess();
            }
        }

        #endregion
    }
}