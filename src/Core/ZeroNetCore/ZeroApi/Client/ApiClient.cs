using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public class ApiClient
    {
        #region Properties

        /// <summary>
        ///     返回的数据
        /// </summary>
        private string _result;

        /// <summary>
        ///     返回值
        /// </summary>
        public string Result => _result;

        /// <summary>
        ///     请求站点
        /// </summary>
        public string Station { get; set; }

        /// <summary>
        ///     上下文内容（透传方式）
        /// </summary>
        public string ContextJson { get; set; }

        /// <summary>
        ///     请求站点
        /// </summary>
        public string RequestId { get; } = RandomOperate.Generate(8);

        /// <summary>
        ///     标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     调用命令
        /// </summary>
        public string Commmand { get; set; }

        /// <summary>
        ///     参数
        /// </summary>
        public string Argument { get; set; }

        /// <summary>
        ///     扩展参数
        /// </summary>
        public string ExtendArgument { get; set; }

        /// <summary>
        ///     请求时申请的全局标识(本地)
        /// </summary>
        public string GlobalId;

        /// <summary>
        ///     结果状态
        /// </summary>
        public ZeroOperatorStateType State { get; set; }
        /// <summary>
        /// 最后一个返回值
        /// </summary>
        public ZeroResultData LastResult { get; set; }
        /// <summary>
        /// 简单调用
        /// </summary>
        /// <remarks>
        /// 1 不获取全局标识(过时）
        /// 2 无远程定向路由,
        /// 3 无上下文信息
        /// </remarks>
        public bool Simple { set; get; }

        #endregion


        #region Command

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <param name="station"></param>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static async Task<string> CallSync(string station, string commmand, string argument)
        {
            return await CallTask(station, commmand, argument);
        }

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <param name="station"></param>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static Task<string> CallTask(string station, string commmand, string argument)
        {
            return Task.Factory.StartNew(() => Call(station, commmand, argument));
        }

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <param name="station"></param>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static string Call(string station, string commmand, string argument)
        {
            var client = new ApiClient
            {
                Station = station,
                Commmand = commmand,
                Argument = argument
            };
            client.CallCommand();
            return client.Result;
        }


        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        public void CallCommand()
        {
            using (MonitorScope.CreateScope("内部Zero调用"))
            {
                Prepare();
                Call();
                End();
            }
        }
        #endregion

        #region Flow

        /// <summary>
        ///     检查在非成功状态下的返回值
        /// </summary>
        public void CheckStateResult()
        {
            if (_result != null)
                return;
            ApiResult apiResult;
            switch (State)
            {
                case ZeroOperatorStateType.Ok:
                    apiResult = ApiResult.Ok;
                    break;
                case ZeroOperatorStateType.LocalNoReady:
                case ZeroOperatorStateType.LocalZmqError:
                    apiResult = ApiResult.NoReady;
                    break;
                case ZeroOperatorStateType.LocalSendError:
                case ZeroOperatorStateType.LocalRecvError:
                    apiResult = ApiResult.NetworkError;
                    break;
                case ZeroOperatorStateType.LocalException:
                    apiResult = ApiResult.LocalException;
                    break;
                case ZeroOperatorStateType.Plan:
                case ZeroOperatorStateType.Runing:
                case ZeroOperatorStateType.VoteBye:
                case ZeroOperatorStateType.Wecome:
                case ZeroOperatorStateType.VoteSend:
                case ZeroOperatorStateType.VoteWaiting:
                case ZeroOperatorStateType.VoteStart:
                case ZeroOperatorStateType.VoteEnd:
                    apiResult = ApiResult.Error(ErrorCode.Success, State.Text());
                    break;
                case ZeroOperatorStateType.Error:
                    apiResult = ApiResult.InnerError;
                    break;
                case ZeroOperatorStateType.Unavailable:
                    apiResult = ApiResult.Unavailable;
                    break;
                case ZeroOperatorStateType.NotSupport:
                case ZeroOperatorStateType.NotFind:
                case ZeroOperatorStateType.NoWorker:
                    apiResult = ApiResult.NoFind;
                    break;
                case ZeroOperatorStateType.ArgumentInvalid:
                    apiResult = ApiResult.ArgumentError;
                    break;
                case ZeroOperatorStateType.TimeOut:
                    apiResult = ApiResult.TimeOut;
                    break;
                case ZeroOperatorStateType.FrameInvalid:

                    apiResult = ApiResult.NetworkError;
                    break;
                case ZeroOperatorStateType.NetError:

                    apiResult = ApiResult.NetworkError;
                    break;
                case ZeroOperatorStateType.Failed:
                case ZeroOperatorStateType.Bug:
                    apiResult = ApiResult.LogicalError;
                    break;
                case ZeroOperatorStateType.Pause:
                    apiResult = ApiResult.Pause;
                    break;
                case ZeroOperatorStateType.DenyAccess:
                    apiResult = ApiResult.DenyAccess;
                    break;
                default:
                    apiResult = ApiResult.RemoteEmptyError;
                    break;
            }
            if (LastResult != null && LastResult.InteractiveSuccess)
            {
                if (!LastResult.TryGetValue(ZeroFrameType.Responser, out var point))
                    point = "zero_center";
                apiResult.Status.Point = point;
            }
            _result = JsonConvert.SerializeObject(apiResult);
        }

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        private void Call()
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                State = ZeroOperatorStateType.LocalNoReady;
                return;
            }

            var socket = ZeroConnectionPool.GetSocket(Station, GlobalContext.RequestInfo.RequestId);
            if (socket?.Socket == null)
            {
                _result = ApiResult.NoReadyJson;
                State = ZeroOperatorStateType.LocalNoReady;
                return;
            }

            using (socket)
            {
                CallApi(socket);
            }
        }

        #endregion


        #region 操作注入

        /// <summary>
        ///     Api处理器
        /// </summary>
        public interface IHandler
        {
            /// <summary>
            ///     准备
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            void Prepare(ApiClient item);

            /// <summary>
            ///     结束处理
            /// </summary>
            /// <param name="item"></param>
            void End(ApiClient item);
        }

        /// <summary>
        ///     处理器
        /// </summary>
        private static readonly List<Func<IHandler>> ApiHandlers = new List<Func<IHandler>>();

        /// <summary>
        ///     注册处理器
        /// </summary>
        public static void RegistHandlers<THandler>() where THandler : class, IHandler, new()
        {
            ApiHandlers.Add(() => new THandler());
        }

        private readonly List<IHandler> _handlers = new List<IHandler>();

        /// <summary>
        ///     注册处理器
        /// </summary>
        public ApiClient()
        {
            foreach (var func in ApiHandlers)
                _handlers.Add(func());
        }


        private void Prepare()
        {
            if (Simple)
                return;
            LogRecorder.MonitorTrace($"Station:{Station},Command:{Commmand}");
            foreach (var handler in _handlers)
                try
                {
                    handler.Prepare(this);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(Station, e, "PreActions", Commmand);
                }
        }

        private void End()
        {
            CheckStateResult();
            if (Simple)
                return;
            LogRecorder.MonitorTrace($"Result:{Result}");
            foreach (var handler in _handlers)
            {
                try
                {
                    handler.End(this);
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(Station, e, "EndActions", Commmand);
                }
            }
        }

        #endregion

        #region Socket

        ///// <summary>
        /////     请求格式说明
        ///// </summary>
        //private static readonly byte[] SimpleCallDescription =
        //{
        //    5,
        //    (byte)ZeroByteCommand.General,
        //    ZeroFrameType.Command,
        //    ZeroFrameType.RequestId,
        //    ZeroFrameType.Argument,
        //    ZeroFrameType.Requester,
        //    ZeroFrameType.SerivceKey,
        //    ZeroFrameType.End
        //};


        /// <summary>
        ///     请求格式说明
        /// </summary>
        private static readonly byte[] CallDescription =
        {
            10,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.PubTitle,
            ZeroFrameType.Command,
            ZeroFrameType.RequestId,
            ZeroFrameType.Context,
            ZeroFrameType.Argument,
            ZeroFrameType.TextContent,
            ZeroFrameType.Requester,
            ZeroFrameType.Responser,
            ZeroFrameType.CallId,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };
        private void CallApi(PoolSocket socket)
        {
            //ZeroResultData result;
            //var old = GlobalContext.RequestInfo.LocalGlobalId;
            /*if (Simple)
            {
                result = Send(socket.Socket, 
                    CallDescription,
                    Title,
                    Commmand,
                    GlobalContext.RequestInfo.RequestId,
                    null,//JsonConvert.SerializeObject(GlobalContext.Current),
                    Argument,
                    null,
                    ZeroApplication.Config.StationName,
                    null,
                    "0",
                    GlobalContext.ServiceKey);
            }
            else
            {
                result = Send(socket.Socket, GetGlobalIdDescription,
                    GlobalContext.RequestInfo.RequestId,
                    GlobalContext.ServiceKey);
                if (!result.InteractiveSuccess)
                {
                    State = result.State;
                    socket.HaseFailed = true;
                    return;
                }

                if (result.State == ZeroOperatorStateType.Pause)
                {
                    socket.HaseFailed = true;
                    State = result.State;
                    return;
                }

                result = Receive(socket.Socket);
                if (!result.InteractiveSuccess)
                {
                    socket.HaseFailed = true;
                    State = result.State;
                    return;
                }

                if (result.TryGetValue(ZeroFrameType.GlobalId, out GlobalId))
                {
                    GlobalContext.RequestInfo.LocalGlobalId = GlobalId;
                    LogRecorder.MonitorTrace($"GlobalId:{GlobalId}");
                }

                result = Send(socket.Socket, CallDescription,
                    Title,
                    Commmand,
                    GlobalContext.RequestInfo.RequestId,
                    ContextJson ?? JsonConvert.SerializeObject(GlobalContext.Current),
                    Argument,
                    ExtendArgument,
                    ZeroApplication.Config.StationName,
                    GlobalContext.Current.Organizational.RouteName,
                    GlobalContext.RequestInfo.LocalGlobalId,
                    GlobalContext.ServiceKey);
                GlobalContext.RequestInfo.LocalGlobalId = old;
            }*/
            LastResult = Send(socket.Socket, CallDescription,
                Title,
                Commmand,
                GlobalContext.RequestInfo.RequestId,
                ContextJson ?? JsonConvert.SerializeObject(GlobalContext.Current),
                Argument,
                ExtendArgument,
                ZeroApplication.Config.StationName,
                GlobalContext.Current.Organizational.RouteName,
                GlobalContext.RequestInfo.LocalGlobalId,
                GlobalContext.ServiceKey);

            if (!LastResult.InteractiveSuccess)
            {
                socket.HaseFailed = true;
                State = LastResult.State;
                return;
            }

            LastResult = Receive(socket.Socket);
            if (!LastResult.InteractiveSuccess)
            {
                socket.HaseFailed = true;
                State = LastResult.State;
                return;
            }
            if (LastResult.State != ZeroOperatorStateType.Runing)
            {
                socket.HaseFailed = true;
                State = LastResult.State;
                return;
            }
            LastResult = Receive(socket.Socket);
            if (!LastResult.InteractiveSuccess)
            {
                socket.HaseFailed = true;
                State = LastResult.State;
                return;
            }

            //if (result.State == ZeroOperatorStateType.NoWorker)
            //{
            //    return;
            //}

            //var lr = socket.Socket.QuietSend(CloseDescription, name, globalId);
            //if (!lr.InteractiveSuccess)
            //{
            //    WriteError(station, "Close Failed", commmand, globalId);
            //}
            //lr = ReceiveString(socket.Socket);
            //if (!lr.InteractiveSuccess)
            //{
            //    socket.HaseFailed = true;
            //    WriteError(station, "Close Failed", commmand, globalId, lr.ZmqErrorMessage);
            //}
            LastResult.TryGetValue(ZeroFrameType.JsonValue, out _result);
            LogRecorder.MonitorTrace($"Remte result:{_result}");
            State = LastResult.State;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="messages"></param>
        public static void WriteError(string title, params object[] messages)
        {
            LogRecorder.MonitorTrace($"{title} : {messages.LinkToString(" > ")}");
        }

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private ZeroResultData Receive(ZSocket socket)
        {
            if (!ZeroApplication.ZerCenterIsRun)
                return new ZeroResultData
                {
                    State = ZeroOperatorStateType.LocalNoReady,
                    InteractiveSuccess = false
                };
            List<Byte[]> messages = new List<byte[]>();
            bool success = false;
            {
                ZMessage frames = null;
                int cnt = 0;
                while (++cnt < 3)
                {
                    success = socket.Recv(out frames);
                    if (success)
                        break;
                    //if (socket.LastError.IsError(ZError.Code.EAGAIN))
                    //    continue;

                    WriteError("Receive", socket.Endpoint, socket.LastError.Text,
                        $"Socket Ptr:{socket.SocketPtr}");
                    return new ZeroResultData
                    {
                        State = ZeroOperatorStateType.LocalRecvError
                    };
                }
                using (frames)
                {
                    frames.Foreach(p => messages.Add(p.Read()));
                }
            }
            if (!success)
            {
                WriteError("Receive", socket.Endpoint, "Time Out",
                    $"Socket Ptr:{socket.SocketPtr}");
                return new ZeroResultData
                {
                    State = ZeroOperatorStateType.TimeOut
                };
            }
            try
            {
                var description = messages[0];
                if (description.Length == 0)
                {
                    WriteError("Receive", "LaoutError", socket.Endpoint,
                        description.LinkToString(p => p.ToString("X2"), ""), $"Socket Ptr:{socket.SocketPtr}.");
                    return new ZeroResultData
                    {
                        State = ZeroOperatorStateType.FrameInvalid,
                        Message = "网络格式错误"
                    };
                }

                var end = description[0] + 1;
                //if (end != messages.Count)
                //{
                //    WriteError("Receive", "LaoutError", socket.Endpoint,
                //        $"FrameSize{messages.Count}", description.LinkToString(p => p.ToString("X2"), ""),
                //        $"Socket Ptr:{socket.SocketPtr}.");
                //    return new ZeroResultData
                //    {
                //        State = ZeroOperatorStateType.FrameInvalid,
                //        Message = "网络格式错误"
                //    };
                //}
                //Console.Write(description[0]);
                //Console.Write((ZeroCenterState)description[0]);
                //for (int i = 2; i < description.Length; i++)
                //{
                //    Console.WriteLine($"{i-1}:{ZeroFrameType.FrameName(description[i])}");
                //}
                //for (var idx = 1; idx < messages.Count; idx++)
                //    Console.WriteLine(Encoding.UTF8.GetString(messages[idx]));
                var result = new ZeroResultData
                {
                    InteractiveSuccess = true,
                    State = (ZeroOperatorStateType)description[1]
                };
                for (var idx = 1; idx < end; idx++)
                    result.Add(description[idx + 1], Encoding.UTF8.GetString(messages[idx]));

                return result;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Endpoint,
                    $"Socket Ptr:{socket.SocketPtr}.");
                return new ZeroResultData
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }
        }

        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="desicription"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        static ZeroResultData Send(ZSocket socket, byte[] desicription, params string[] args)
        {
            var message = new ZMessage();
            var frame = new ZFrame(desicription);
            message.Add(frame);
            if (args != null)
            {
                foreach (var arg in args)
                {
                    message.Add(new ZFrame(arg.ToZeroBytes()));
                }
            }
            using (message)
                if (!socket.SendTo(message))
                {
                    return new ZeroResultData
                    {
                        State = ZeroOperatorStateType.LocalSendError,
                        ZmqError = socket.LastError
                    };
                }
            return new ZeroResultData
            {
                State = ZeroOperatorStateType.Ok,
                InteractiveSuccess = true
            };
        }

        #endregion

        #region 快捷方法
        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static TResult Call<TArgument, TResult>(string station, string api, TArgument arg)
        {
            ApiClient client = new ApiClient
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonConvert.SerializeObject(arg)
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client.CheckStateResult();
            }
            return JsonConvert.DeserializeObject<TResult>(client.Result);
        }
        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static ApiResult<TResult> CallApi<TArgument, TResult>(string station, string api, TArgument arg)
        {
            ApiClient client = new ApiClient
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonConvert.SerializeObject(arg)
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client.CheckStateResult();
            }
            return JsonConvert.DeserializeObject<ApiResult<TResult>>(client.Result);
        }
        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static ApiResult CallApi<TArgument>(string station, string api, TArgument arg)
        {
            ApiClient client = new ApiClient
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonConvert.SerializeObject(arg)
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client.CheckStateResult();
            }
            return JsonConvert.DeserializeObject<ApiResult>(client.Result);
        }
        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static ApiResult CallApi(string station, string api)
        {
            ApiClient client = new ApiClient
            {
                Station = station,
                Commmand = api
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client.CheckStateResult();
            }
            return JsonConvert.DeserializeObject<ApiResult>(client.Result);
        }
        #endregion
    }
}