using System;
using System.Linq;
using System.Text;
using System.Threading;
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
    public class BridgeCaller
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
                Call();
            }
        }

        ///// <summary>
        /////     远程调用
        ///// </summary>
        ///// <returns></returns>
        //public string CheckResult()
        //{
        //    LogRecorder.MonitorTrace($"{State} : {Result}");
        //    return State < ZeroOperatorStateType.Failed ? (Result ?? "error") : "error";
        //}

        ///// <summary>
        /////     远程调用
        ///// </summary>
        ///// <returns></returns>
        //public void Do()
        //{
        //    using (MonitorScope.CreateScope("内部Zero调用"))
        //    {
        //        Call();
        //    }
        //}
        ///// <summary>
        /////     远程调用
        ///// </summary>
        ///// <returns></returns>
        //public void CallSync()
        //{
        //    Task.Factory.StartNew(Do);
        //}
        #endregion

        #region Flow

        /// <summary>
        ///     检查在非成功状态下的返回值
        /// </summary>
        public string GetStateJson()
        {
            switch (State)
            {
                case ZeroOperatorStateType.LocalNoReady:
                case ZeroOperatorStateType.LocalZmqError:
                    return ApiResult.NoReadyJson;

                case ZeroOperatorStateType.LocalSendError:
                case ZeroOperatorStateType.LocalRecvError:
                    return ApiResult.NetworkErrorJson;

                case ZeroOperatorStateType.LocalException:
                    return ApiResult.LocalExceptionJson;

                case ZeroOperatorStateType.Plan:
                case ZeroOperatorStateType.Runing:
                case ZeroOperatorStateType.VoteBye:
                case ZeroOperatorStateType.Wecome:
                case ZeroOperatorStateType.VoteSend:
                case ZeroOperatorStateType.VoteWaiting:
                case ZeroOperatorStateType.VoteStart:
                case ZeroOperatorStateType.VoteEnd:
                    return JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.Success, State.Text()));

                case ZeroOperatorStateType.Error:
                    return ApiResult.InnerErrorJson;

                case ZeroOperatorStateType.Unavailable:
                    return ApiResult.UnavailableJson;

                case ZeroOperatorStateType.NotSupport:
                case ZeroOperatorStateType.NotFind:
                case ZeroOperatorStateType.NoWorker:
                    //ApiContext.Current.LastError = ErrorCode.NoFind;
                    return ApiResult.NoFindJson;

                case ZeroOperatorStateType.ArgumentInvalid:
                    //ApiContext.Current.LastError = ErrorCode.LogicalError;
                    return ApiResult.ArgumentErrorJson;

                case ZeroOperatorStateType.TimeOut:
                    return ApiResult.TimeOutJson;

                case ZeroOperatorStateType.FrameInvalid:
                    //ApiContext.Current.LastError = ErrorCode.NetworkError;
                    return ApiResult.NetworkErrorJson;

                case ZeroOperatorStateType.NetError:
                    //ApiContext.Current.LastError = ErrorCode.NetworkError;
                    return ApiResult.NetworkErrorJson;

                case ZeroOperatorStateType.Failed:
                case ZeroOperatorStateType.Bug:
                    //ApiContext.Current.LastError = ErrorCode.LocalError;
                    return ApiResult.LogicalErrorJson;

                case ZeroOperatorStateType.Pause:
                    //ApiContext.Current.LastError = ErrorCode.LocalError;
                    return ApiResult.LogicalErrorJson;

                default:
                    return ApiResult.PauseJson;
            }
        }
        private readonly string _address = ZeroApplication.Config.BridgeLocalAddress;//$"inproc://{ZeroApplication.Config.StationName}.inp"
        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        private void Call()
        {
            //if (!ZeroApplication.CanDo)
            //{
            //    State = ZeroOperatorStateType.LocalNoReady;
            //    return;
            //}

            var socket = ZSocket.CreateClientSocket(_address, ZSocketType.DEALER, ZeroIdentityHelper.CreateIdentity(false, Station));
            if (socket == null)
            {
                _result = "no ready";
                State = ZeroOperatorStateType.LocalNoReady;
                return;
            }

            using (socket)
            {
                if (!Send(socket))
                {
                    State = ZeroOperatorStateType.LocalSendError;
                    return;
                }
                var result = Receive(socket);
                if (!result.InteractiveSuccess)
                {
                    State = ZeroOperatorStateType.LocalRecvError;
                    return;
                }
                result.TryGetValue(ZeroFrameType.JsonValue, out _result);
                State = result.State;
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
            ZeroFrameType.Station,
            ZeroFrameType.Command,
            ZeroFrameType.RequestId,
            ZeroFrameType.Context,
            ZeroFrameType.Argument,
            ZeroFrameType.TextContent,
            ZeroFrameType.Requester,
            ZeroFrameType.Responser,
            ZeroFrameType.GlobalId,
            ZeroFrameType.End
        };

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private bool Send(ZSocket socket)
        {
            int cnt = 0;
            do
            {
                using (var msg = new ZMessage(CallDescription,
                    Station,
                    Commmand,
                    GlobalContext.RequestInfo.RequestId,
                    JsonConvert.SerializeObject(GlobalContext.Current),
                    Argument,
                    ExtendArgument,
                    ZeroApplication.Config.StationName,
                    GlobalContext.Current.Organizational.RouteName,
                    GlobalContext.RequestInfo.LocalGlobalId))
                {
                    if (socket.SendTo(msg))
                        return true;
                }
                Thread.Sleep(100);
            } while (++cnt <= 3);
            return false;
        }

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private static ZeroResultData Receive(ZSocket socket)
        {
            ZMessage frames;
            if (!socket.Recv(out frames))
            {
                ZeroTrace.WriteError("Receive",
                    socket.LastError.Text,
                    socket.Endpoint,
                    $"Socket Ptr:{socket.SocketPtr}");
                return new ZeroResultData
                {
                    State = ZeroOperatorStateType.LocalRecvError
                };
            }
            try
            {
                return ZeroResultData.Unpack<ZeroResultData>(frames,true);
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
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="station">站点</param>
        /// <param name="api">api名称</param>
        /// <returns></returns>
        public static ApiResult<TResult> CallApi<TResult>(string station, string api)
        {
            var client = new ApiClient
            {
                Station = station,
                Commmand = api,
                Argument = null
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