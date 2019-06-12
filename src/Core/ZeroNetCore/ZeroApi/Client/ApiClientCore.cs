using System;
using Agebull.Common.Context;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using ZeroMQ;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    ///     Api站点
    /// </summary>
    internal class ApiClientCore
    {
        #region Properties

        /// <summary>
        ///     返回值
        /// </summary>
        internal string Result;

        /// <summary>
        ///     请求站点
        /// </summary>
        internal string Station;

        /// <summary>
        ///     上下文内容（透传方式）
        /// </summary>
        internal string ContextJson;

        /// <summary>
        ///     请求站点
        /// </summary>
        internal string RequestId;

        /// <summary>
        ///     标题
        /// </summary>
        internal string Title;

        /// <summary>
        ///     调用命令
        /// </summary>
        internal string Commmand;

        /// <summary>
        ///     参数
        /// </summary>
        internal string Argument;

        /// <summary>
        ///     扩展参数
        /// </summary>
        internal string ExtendArgument;

        /// <summary>
        ///     结果状态
        /// </summary>
        internal ZeroOperatorStateType State;

        /// <summary>
        /// 最后一个返回值
        /// </summary>
        internal ZeroResult LastResult;

        #endregion
        
        #region Flow

        /// <summary>
        ///     检查在非成功状态下的返回值
        /// </summary>
        internal void CheckStateResult()
        {
            if (Result != null)
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
            Result = JsonHelper.SerializeObject(apiResult);
        }

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        internal void Call()
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                State = ZeroOperatorStateType.LocalNoReady;
                return;
            }

            var socket = ZeroConnectionPool.GetSocket(Station, GlobalContext.RequestInfo.RequestId);
            if (socket?.Socket == null)
            {
                Result = ApiResultIoc.NoReadyJson;
                State = ZeroOperatorStateType.LocalNoReady;
                return;
            }

            using (socket)
            {
                CallApi(socket);
            }
        }


        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        internal void Plan(ZeroPlanInfo plan)
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                State = ZeroOperatorStateType.LocalNoReady;
                return;
            }

            var socket = ZeroConnectionPool.GetSocket(Station, GlobalContext.RequestInfo.RequestId);
            if (socket?.Socket == null)
            {
                Result = ApiResultIoc.NoReadyJson;
                State = ZeroOperatorStateType.LocalNoReady;
                return;
            }
            

            using (socket)
            {
                CallPlan(socket, plan);
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
                Commmand.ToZeroBytes(),
                Argument.ToZeroBytes(),
                ExtendArgument.ToZeroBytes(),
                GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                ZeroApplication.Config.StationName.ToZeroBytes(),
                GlobalContext.Current.Organizational.RouteName.ToZeroBytes(),
                GlobalContext.RequestInfo.LocalGlobalId.ToZeroBytes(),
                (ContextJson ?? (GlobalContext.CurrentNoLazy == null ? null : JsonHelper.SerializeObject(GlobalContext.CurrentNoLazy))).ToZeroBytes());

            CallEnd(socket);
        }

        /// <summary>
        ///     请求格式说明
        /// </summary>
        private static readonly byte[] PlanDescription =
        {
            10,
            (byte)ZeroByteCommand.Plan,
            ZeroFrameType.Command,
            ZeroFrameType.Argument,
            ZeroFrameType.TextContent,
            ZeroFrameType.RequestId,
            ZeroFrameType.Requester,
            ZeroFrameType.Responser,
            ZeroFrameType.CallId,
            ZeroFrameType.Context,
            ZeroFrameType.Plan,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };

        private void CallPlan(PoolSocket socket, ZeroPlanInfo plan)
        {
            LastResult = Send(socket.Socket, PlanDescription,
                Commmand.ToZeroBytes(),
                Argument.ToZeroBytes(),
                ExtendArgument.ToZeroBytes(),
                GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                ZeroApplication.Config.StationName.ToZeroBytes(),
                GlobalContext.Current.Organizational.RouteName.ToZeroBytes(),
                GlobalContext.RequestInfo.LocalGlobalId.ToZeroBytes(),
                (ContextJson ?? (GlobalContext.CurrentNoLazy == null ? null : JsonHelper.SerializeObject(GlobalContext.CurrentNoLazy))).ToZeroBytes(),
                plan.ToZeroBytes());

            CallEnd(socket);
        }

        void CallEnd(PoolSocket socket)
        {

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
            LastResult.TryGetValue(ZeroFrameType.ResultText, out Result);
            LogRecorderX.MonitorTrace($"Remte result:{Result}");
            State = LastResult.State;
        }

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        internal ZeroResult Receive(ZSocket socket)
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
        /// <param name="description"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static ZeroResult Send(ZSocket socket, byte[] description, params byte[][] args)
        {
            using (var message = new ZMessage())
            {
                message.Add(new ZFrame(description));
                if (args != null)
                {
                    foreach (var arg in args)
                    {
                        message.Add(new ZFrame(arg));
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

    }
}