using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agebull.Common.Context;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using Agebull.MicroZero.ZeroApis;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.MicroZero.ZeroApis
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
        public ZeroResult LastResult { get; set; }
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
            IApiResult apiResult;
            switch (State)
            {
                case ZeroOperatorStateType.Ok:
                    apiResult = ApiResultIoc.Ioc.Ok;
                    break;
                case ZeroOperatorStateType.LocalNoReady:
                case ZeroOperatorStateType.LocalZmqError:
                    apiResult = ApiResultIoc.Ioc.NoReady;
                    break;
                case ZeroOperatorStateType.LocalSendError:
                case ZeroOperatorStateType.LocalRecvError:
                    apiResult = ApiResultIoc.Ioc.NetworkError;
                    break;
                case ZeroOperatorStateType.LocalException:
                    apiResult = ApiResultIoc.Ioc.LocalException;
                    break;
                case ZeroOperatorStateType.Plan:
                case ZeroOperatorStateType.Runing:
                case ZeroOperatorStateType.VoteBye:
                case ZeroOperatorStateType.Wecome:
                case ZeroOperatorStateType.VoteSend:
                case ZeroOperatorStateType.VoteWaiting:
                case ZeroOperatorStateType.VoteStart:
                case ZeroOperatorStateType.VoteEnd:
                    apiResult = ApiResultIoc.Ioc.Error(ErrorCode.Success, State.Text());
                    break;
                case ZeroOperatorStateType.Error:
                    apiResult = ApiResultIoc.Ioc.InnerError;
                    break;
                case ZeroOperatorStateType.Unavailable:
                    apiResult = ApiResultIoc.Ioc.Unavailable;
                    break;
                case ZeroOperatorStateType.NotSupport:
                case ZeroOperatorStateType.NotFind:
                case ZeroOperatorStateType.NoWorker:
                    apiResult = ApiResultIoc.Ioc.NoFind;
                    break;
                case ZeroOperatorStateType.ArgumentInvalid:
                    apiResult = ApiResultIoc.Ioc.ArgumentError;
                    break;
                case ZeroOperatorStateType.TimeOut:
                    apiResult = ApiResultIoc.Ioc.TimeOut;
                    break;
                case ZeroOperatorStateType.FrameInvalid:

                    apiResult = ApiResultIoc.Ioc.NetworkError;
                    break;
                case ZeroOperatorStateType.NetError:

                    apiResult = ApiResultIoc.Ioc.NetworkError;
                    break;
                case ZeroOperatorStateType.Failed:
                case ZeroOperatorStateType.Bug:
                    apiResult = ApiResultIoc.Ioc.LogicalError;
                    break;
                case ZeroOperatorStateType.Pause:
                    apiResult = ApiResultIoc.Ioc.Pause;
                    break;
                case ZeroOperatorStateType.DenyAccess:
                    apiResult = ApiResultIoc.Ioc.DenyAccess;
                    break;
                default:
                    apiResult = ApiResultIoc.Ioc.RemoteEmptyError;
                    break;
            }
            if (LastResult != null && LastResult.InteractiveSuccess)
            {
                if (!LastResult.TryGetValue(ZeroFrameType.Responser, out var point))
                    point = "zero_center";
                apiResult.Status.Point = point;
            }
            _result = JsonHelper.SerializeObject(apiResult);
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
                _result = ApiResultIoc.NoReadyJson;
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
            LogRecorderX.MonitorTrace($"Station:{Station},Command:{Commmand}");
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
            LogRecorderX.MonitorTrace($"Result:{Result}");
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

        /// <summary>
        ///     请求格式说明
        /// </summary>
        private static readonly byte[] CallDescription =
        {
            9,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.Command,
            ZeroFrameType.Argument,
            ZeroFrameType.TextContent,
            ZeroFrameType.RequestId,
            ZeroFrameType.Requester,
            ZeroFrameType.Responser,
            ZeroFrameType.CallId,
            ZeroFrameType.Context,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };
        private void CallApi(PoolSocket socket)
        {
            LastResult = Send(socket.Socket, CallDescription,
                Commmand,
                Argument,
                ExtendArgument,
                GlobalContext.RequestInfo.RequestId,
                ZeroApplication.Config.StationName,
                GlobalContext.Current.Organizational.RouteName,
                GlobalContext.RequestInfo.LocalGlobalId,
                ContextJson ?? 
                    (GlobalContext.CurrentNoLazy == null 
                        ? null 
                        : JsonHelper.SerializeObject(GlobalContext.CurrentNoLazy)));

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
            LastResult.TryGetValue(ZeroFrameType.ResultText, out _result);
            LogRecorderX.MonitorTrace($"Remte result:{_result}");
            State = LastResult.State;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="messages"></param>
        public static void WriteError(string title, params object[] messages)
        {
            LogRecorderX.MonitorTrace($"{title} : {messages.LinkToString(" > ")}");
        }

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private ZeroResult Receive(ZSocket socket)
        {
            if (!socket.Recv(out var frames))
            {
                return new ZeroResult
                {
                    State = ZeroOperatorStateType.LocalRecvError
                };
            }
            try
            {
                return ZeroResultData.Unpack<ZeroResult>(frames, true);
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Endpoint, $"Socket Ptr:{socket.SocketPtr}.");
                return new ZeroResult
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
        static ZeroResult Send(ZSocket socket, byte[] desicription, params string[] args)
        {
            using (var message = new ZMessage())
            {
                var frame = new ZFrame(desicription);
                message.Add(frame);
                if (args != null)
                {
                    foreach (var arg in args)
                    {
                        message.Add(new ZFrame(arg.ToZeroBytes()));
                    }
                    message.Add(new ZFrame(ZeroCommandExtend.ServiceKeyBytes));
                    
                }
                if (!socket.SendTo(message))
                {
                    return new ZeroResult
                    {
                        State = ZeroOperatorStateType.LocalSendError,
                        ZmqError = socket.LastError
                    };
                }
            }
            return new ZeroResult
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
        public static IApiResult<TResult> CallApi<TArgument, TResult>(string station, string api, TArgument arg)
        {
            ApiClient client = new ApiClient
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject<TResult>(client.Result);
        }
        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TArgument">参数类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <param name="arg">参数</param>
        /// <returns></returns>
        public static IApiResult CallApi<TArgument>(string station, string api, TArgument arg)
        {
            ApiClient client = new ApiClient
            {
                Station = station,
                Commmand = api,
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client.CheckStateResult();
            }
            return ApiResultIoc.Ioc.DeserializeObject(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TResult">参数类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static IApiResult<TResult> CallApi<TResult>(string station, string api)
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
            return ApiResultIoc.Ioc.DeserializeObject<TResult>(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static IApiResult CallApi(string station, string api)
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
            return ApiResultIoc.Ioc.DeserializeObject(client.Result);
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
                Argument = arg == null ? null : JsonHelper.SerializeObject(arg)
            };
            client.CallCommand();
            if (client.State != ZeroOperatorStateType.Ok)
            {
                client.CheckStateResult();
            }
            return JsonHelper.DeserializeObject<TResult>(client.Result);
        }

        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <typeparam name="TResult">参数类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static TResult Call<TResult>(string station, string api)
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
            return JsonHelper.DeserializeObject<TResult>(client.Result);
        }

        #endregion
    }
}