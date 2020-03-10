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
        ///     返回值
        /// </summary>
        public string Result => _json;

        /// <summary>
        ///     请求站点
        /// </summary>
        public string Station { get; set; }

        /// <summary>
        ///     请求站点
        /// </summary>
        public string RequestId { get; } = RandomOperate.Generate(8);

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
        /// 简单调用
        /// </summary>
        /// <remarks>
        /// 1 不获取全局标识
        /// 2 无远程定向路由,
        /// 3 无上下文信息
        /// </remarks>
        public bool Simple { set; get; }

        /// <summary>
        ///     返回的数据
        /// </summary>
        private string _json;

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
            if (_json != null)
                return;
            switch (State)
            {
                case ZeroOperatorStateType.LocalNoReady:
                case ZeroOperatorStateType.LocalZmqError:
                    _json = ApiResult.NoReadyJson;
                    return;
                case ZeroOperatorStateType.LocalSendError:
                case ZeroOperatorStateType.LocalRecvError:
                    _json = ApiResult.NetworkErrorJson;
                    return;
                case ZeroOperatorStateType.LocalException:
                    _json = ApiResult.LocalExceptionJson;
                    return;
                case ZeroOperatorStateType.Plan:
                case ZeroOperatorStateType.Runing:
                case ZeroOperatorStateType.VoteBye:
                case ZeroOperatorStateType.Wecome:
                case ZeroOperatorStateType.VoteSend:
                case ZeroOperatorStateType.VoteWaiting:
                case ZeroOperatorStateType.VoteStart:
                case ZeroOperatorStateType.VoteEnd:
                    _json = JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.Success, State.Text()));
                    return;
                case ZeroOperatorStateType.Error:
                    _json = ApiResult.InnerErrorJson;
                    return;
                case ZeroOperatorStateType.Unavailable:
                    _json = ApiResult.UnavailableJson;
                    return;
                case ZeroOperatorStateType.NotSupport:
                case ZeroOperatorStateType.NotFind:
                case ZeroOperatorStateType.NoWorker:
                    //ApiContext.Current.LastError = ErrorCode.NoFind;
                    _json = ApiResult.NoFindJson;
                    return;
                case ZeroOperatorStateType.ArgumentInvalid:
                    //ApiContext.Current.LastError = ErrorCode.LogicalError;
                    _json = ApiResult.ArgumentErrorJson;
                    return;
                case ZeroOperatorStateType.TimeOut:
                    _json = ApiResult.TimeOutJson;
                    return;
                case ZeroOperatorStateType.FrameInvalid:
                    //ApiContext.Current.LastError = ErrorCode.NetworkError;
                    _json = ApiResult.NetworkErrorJson;
                    return;
                case ZeroOperatorStateType.NetError:
                    //ApiContext.Current.LastError = ErrorCode.NetworkError;
                    _json = ApiResult.NetworkErrorJson;
                    return;
                case ZeroOperatorStateType.Failed:
                case ZeroOperatorStateType.Bug:
                    //ApiContext.Current.LastError = ErrorCode.LocalError;
                    _json = ApiResult.LogicalErrorJson;
                    return;
                case ZeroOperatorStateType.Pause:
                    //ApiContext.Current.LastError = ErrorCode.LocalError;
                    _json = ApiResult.LogicalErrorJson;
                    return;
                default:
                    _json = ApiResult.PauseJson;
                    return;
                case ZeroOperatorStateType.Ok:
                    //ApiContext.Current.LastError = ErrorCode.Success;
                    return;
            }
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
            if (socket.Socket == null)
            {
                //ApiContext.Current.LastError = ErrorCode.NoReady;
                _json = ApiResult.NoReadyJson;
                State = ZeroOperatorStateType.LocalNoReady;
                return;
            }

            using (socket)
            {
                ReadNetWork(socket);
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

        /// <summary>
        ///     请求格式说明
        /// </summary>
        private static readonly byte[] SimpleCallDescription =
        {
            4,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.Command,
            ZeroFrameType.RequestId,
            ZeroFrameType.Argument,
            ZeroFrameType.Requester,
            ZeroFrameType.End
        };

        /// <summary>
        ///     请求格式说明
        /// </summary>
        private static readonly byte[] CallDescription =
        {
            8,
            (byte)ZeroByteCommand.General,
            ZeroFrameType.Command,
            ZeroFrameType.RequestId,
            ZeroFrameType.Context,
            ZeroFrameType.Argument,
            ZeroFrameType.Content,
            ZeroFrameType.Requester,
            ZeroFrameType.Responser,
            ZeroFrameType.GlobalId
        };

        /// <summary>
        ///     请求格式说明
        /// </summary>
        private static readonly byte[] GetGlobalIdDescription =
        {
            1,
            (byte)ZeroByteCommand.GetGlobalId,
            ZeroFrameType.RequestId,
            ZeroFrameType.End
        };

        private void ReadNetWork(PoolSocket socket)
        {
            ZeroResultData result;
            var old = GlobalContext.RequestInfo.LocalGlobalId;
            if (Simple)
            {
                result = socket.Socket.QuietSend(CallDescription,
                    Commmand,
                    GlobalContext.RequestInfo.RequestId,
                    null,//JsonConvert.SerializeObject(GlobalContext.Current),
                    Argument,
                    null,
                    ZeroApplication.Config.StationName,
                    null,
                    "0");
            }
            else
            {
                result = socket.Socket.QuietSend(GetGlobalIdDescription, GlobalContext.RequestInfo.RequestId);
                if (!result.InteractiveSuccess)
                {
                    socket.HaseFailed = true;
                    //ZeroTrace.WriteError("GetGlobalId", "Send Failed", station, commmand, argument);
                    //ApiContext.Current.LastError = ErrorCode.NetworkError;
                    State = ZeroOperatorStateType.LocalSendError;
                    return;
                }

                if (result.State == ZeroOperatorStateType.Pause)
                {
                    socket.HaseFailed = true;
                    State = ZeroOperatorStateType.Pause;
                    return;
                }

                result = ReceiveString(socket.Socket);
                if (!result.InteractiveSuccess)
                {
                    socket.HaseFailed = true;
                    //ZeroTrace.WriteError("GlobalId", "Recv  Failed", station, commmand, argument);
                    //ApiContext.Current.LastError = ErrorCode.NetworkError;
                    State = ZeroOperatorStateType.LocalRecvError;
                    return;
                }

                if (result.TryGetValue(ZeroFrameType.GlobalId, out GlobalId))
                {
                    GlobalContext.RequestInfo.LocalGlobalId = GlobalId;
                    LogRecorder.MonitorTrace($"GlobalId:{GlobalId}");
                }

                result = socket.Socket.QuietSend(CallDescription,
                    Commmand,
                    GlobalContext.RequestInfo.RequestId,
                    JsonConvert.SerializeObject(GlobalContext.Current),
                    Argument,
                    ExtendArgument,
                    ZeroApplication.Config.StationName,
                    GlobalContext.Current.Organizational.RouteName,
                    GlobalContext.RequestInfo.LocalGlobalId);
                GlobalContext.RequestInfo.LocalGlobalId = old;
            }

            if (!result.InteractiveSuccess)
            {
                socket.HaseFailed = true;
                //ZeroTrace.WriteError(station, "Send Failed", commmand, argument);
                //ApiContext.Current.LastError = ErrorCode.NetworkError;
                State = ZeroOperatorStateType.LocalSendError;
                return;
            }

            result = ReceiveString(socket.Socket);
            if (!result.InteractiveSuccess)
            {
                socket.HaseFailed = true;
                //ZeroTrace.WriteError("API", "incorrigible", commmand, globalId);
                //ApiContext.Current.LastError = ErrorCode.NetworkError;
                State = ZeroOperatorStateType.LocalRecvError;
                return;
            }

            //if (result.State == ZeroOperatorStateType.NoWorker)
            //{
            //    return;
            //}

            //var lr = socket.Socket.QuietSend(CloseDescription, name, globalId);
            //if (!lr.InteractiveSuccess)
            //{
            //    ZeroTrace.WriteError(station, "Close Failed", commmand, globalId);
            //}
            //lr = ReceiveString(socket.Socket);
            //if (!lr.InteractiveSuccess)
            //{
            //    socket.HaseFailed = true;
            //    ZeroTrace.WriteError(station, "Close Failed", commmand, globalId, lr.ZmqErrorMessage);
            //}
            result.TryGetValue(ZeroFrameType.JsonValue, out _json);
            State = result.State;
        }


        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private ZeroResultData ReceiveString(ZSocket socket)
        {
            if (!ZeroApplication.ZerCenterIsRun)
                return new ZeroResultData
                {
                    State = ZeroOperatorStateType.LocalRecvError,
                    InteractiveSuccess = false
                };
            if (!socket.Recv(out var messages))
            {
                if (!Equals(socket.LastError, ZError.EAGAIN))
                    ZeroTrace.WriteError("Receive", socket.Connects.LinkToString(','), socket.LastError.Text,
                        $"Socket Ptr:{socket.SocketPtr}");
                return new ZeroResultData
                {
                    State = ZeroOperatorStateType.LocalRecvError
                    //ZmqErrorMessage = error?.Text,
                    //ZmqErrorCode = error?.Number ?? 0
                };
            }

            try
            {
                var description = messages[0].Read();
                if (description.Length == 0)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", socket.Connects.LinkToString(','),
                        description.LinkToString(p => p.ToString("X2"), ""), $"Socket Ptr:{socket.SocketPtr}.");
                    return new ZeroResultData
                    {
                        State = ZeroOperatorStateType.FrameInvalid,
                        Message = "网络格式错误"
                    };
                }

                var end = description[0] + 1;
                if (end != messages.Count)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", socket.Connects.LinkToString(','),
                        $"FrameSize{messages.Count}", description.LinkToString(p => p.ToString("X2"), ""),
                        $"Socket Ptr:{socket.SocketPtr}.");
                    return new ZeroResultData
                    {
                        State = ZeroOperatorStateType.FrameInvalid,
                        Message = "网络格式错误"
                    };
                }

                var result = new ZeroResultData
                {
                    InteractiveSuccess = true,
                    State = (ZeroOperatorStateType)description[1]
                };
                for (var idx = 1; idx < end; idx++)
                    result.Add(description[idx + 1], Encoding.UTF8.GetString(messages[idx].Read()));

                return result;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Connects.LinkToString(','),
                    $"Socket Ptr:{socket.SocketPtr}.");
                return new ZeroResultData
                {
                    State = ZeroOperatorStateType.LocalException,
                    Exception = e
                };
            }
            finally
            {
                messages.Dispose();
            }
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