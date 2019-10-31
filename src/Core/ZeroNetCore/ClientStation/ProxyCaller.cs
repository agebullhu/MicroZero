using System;
using System.Collections.Generic;
using System.Threading;
using Agebull.Common.Context;
using Agebull.Common.Logging;
using Agebull.EntityModel.Common;
using ZeroMQ;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    ///     Api站点
    /// </summary>
    internal class ProxyCaller
    {
        #region Properties

        /// <summary>
        ///     文件
        /// </summary>
        internal Dictionary<string, byte[]> Files;

        /// <summary>
        ///     调用时使用的名称
        /// </summary>
        internal string Name = RandomOperate.Generate(8);

        /// <summary>
        ///     返回值
        /// </summary>
        internal byte ResultType;

        /// <summary>
        ///     返回值
        /// </summary>
        internal string Result;

        /// <summary>
        ///     返回值
        /// </summary>
        internal byte[] Binary;

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
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        internal void Call()
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                Result = ApiResultIoc.NoReadyJson;
                State = ZeroOperatorStateType.LocalNoReady;
                return;
            }

            var socket = ApiProxy.GetSocket(Station, Name);
            if (socket == null)
            {
                Result = ApiResultIoc.NetworkErrorJson;
                State = ZeroOperatorStateType.LocalZmqError;
                return;
            }

            //socket.SetOption(ZSocketOption.RCVTIMEO, 30000);
            //socket.SetOption(ZSocketOption.SNDTIMEO, 30000);
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
                Result = ApiResultIoc.NoReadyJson;
                State = ZeroOperatorStateType.LocalNoReady;
                return;
            }

            var socket = ApiProxy.GetSocket(Station, Name);
            if (socket == null)
            {
                Result = ApiResultIoc.NetworkErrorJson;
                State = ZeroOperatorStateType.LocalZmqError;
                return;
            }
            //socket.SetOption(ZSocketOption.RCVTIMEO, 30000);
            //socket.SetOption(ZSocketOption.SNDTIMEO, 30000);
            using (socket)
            {
                CallPlan(socket, plan);
            }
        }

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
                if (!LastResult.TryGetString(ZeroFrameType.Responser, out var point))
                    point = "zero_center";
                apiResult.Status.Point = point;
            }
            Result = JsonHelper.SerializeObject(apiResult);
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

        private void CallApi(ZSocket socket)
        {
            if (Files == null || Files.Count <= 0)
                LastResult = CallNoFileApi(socket);
            else
                LastResult = CallByFileApi(socket);

            if (!LastResult.InteractiveSuccess)
            {

                State = LastResult.State;
                return;
            }
            CallEnd(socket);
        }

        private ZeroResult CallNoFileApi(ZSocket socket)
        {
            return Send(socket,
                 CallDescription,
                 Commmand.ToZeroBytes(),
                 Argument.ToZeroBytes(),
                 ExtendArgument.ToZeroBytes(),
                 GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                 Name.ToZeroBytes(),
                 GlobalContext.Current.Organizational.RouteName.ToZeroBytes(),
                 GlobalContext.RequestInfo.LocalGlobalId.ToZeroBytes(),
                 (ContextJson ?? (GlobalContext.CurrentNoLazy == null ? null : JsonHelper.SerializeObject(GlobalContext.CurrentNoLazy))).ToZeroBytes());
        }

        private ZeroResult CallByFileApi(ZSocket socket)
        {
            var frames = new List<byte[]>
            {
                Commmand.ToZeroBytes(),
                Argument.ToZeroBytes(),
                ExtendArgument.ToZeroBytes(),
                GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                Name.ToZeroBytes(),
                GlobalContext.Current.Organizational.RouteName.ToZeroBytes(),
                GlobalContext.RequestInfo.LocalGlobalId.ToZeroBytes(),
                (ContextJson ?? (GlobalContext.CurrentNoLazy == null ? null : JsonHelper.SerializeObject(GlobalContext.CurrentNoLazy))).ToZeroBytes()
            };

            var len = 16 + Files.Count * 2;
            var description = new byte[len];
            var i = 0;
            description[i++] = (byte)(9 + Files.Count * 2);
            description[i++] = (byte)ZeroByteCommand.General;
            description[i++] = ZeroFrameType.Command;
            description[i++] = ZeroFrameType.Argument;
            description[i++] = ZeroFrameType.TextContent;
            description[i++] = ZeroFrameType.RequestId;
            description[i++] = ZeroFrameType.Requester;
            description[i++] = ZeroFrameType.Responser;
            description[i++] = ZeroFrameType.CallId;
            description[i++] = ZeroFrameType.Context;
            foreach (var file in Files)
            {
                description[i++] = ZeroFrameType.ExtendText;
                description[i++] = ZeroFrameType.BinaryContent;
                frames.Add(file.Key.ToZeroBytes());
                frames.Add(file.Value);
            }
            description[i++] = ZeroFrameType.SerivceKey;
            description[i] = ZeroFrameType.End;

            return SendToZero(socket, description, frames);
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

        private void CallPlan(ZSocket socket, ZeroPlanInfo plan)
        {
            LastResult = Send(socket,
                PlanDescription,
                Commmand.ToZeroBytes(),
                Argument.ToZeroBytes(),
                ExtendArgument.ToZeroBytes(),
                GlobalContext.RequestInfo.RequestId.ToZeroBytes(),
                Name.ToZeroBytes(),
                GlobalContext.Current.Organizational.RouteName.ToZeroBytes(),
                GlobalContext.RequestInfo.LocalGlobalId.ToZeroBytes(),
                (ContextJson ?? (GlobalContext.CurrentNoLazy == null ? null : JsonHelper.SerializeObject(GlobalContext.CurrentNoLazy))).ToZeroBytes(),
                plan.ToZeroBytes());

            if (!LastResult.InteractiveSuccess)
            {

                State = LastResult.State;
                return;
            }

            using (MonitorScope.CreateScope("CallEnd"))
                CallEnd(socket);
        }

        void CallEnd(ZSocket socket)
        {

            LastResult = Receive(socket);
            if (!LastResult.InteractiveSuccess)
            {
                State = LastResult.State;
                return;
            }

            if (LastResult.State == ZeroOperatorStateType.Runing)
            {
                CallEnd(socket);
                return;
            }

            Result = LastResult.Result;
            Binary = LastResult.Binary;
            ResultType = LastResult.ResultType;
            LogRecorderX.MonitorTrace($"Remote result:{Result}");
            State = LastResult.State;
        }

        /// <summary>
        ///     接收
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        internal ZeroResult Receive(ZSocket socket)
        {
            using (MonitorScope.CreateScope("Recv"))
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
                    return ZeroResultData.Unpack<ZeroResult>(frames, true, (res, type, bytes) =>
                    {
                        switch (type)
                        {
                            case ZeroFrameType.ResultText:
                                res.Result = ZeroNetMessage.GetString(bytes);
                                return true;
                            case ZeroFrameType.BinaryContent:
                                res.Binary = bytes;
                                return true;
                        }

                        return false;
                    });
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
        }

        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="description"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal ZeroResult Send(ZSocket socket, byte[] description, params byte[][] args)
        {
            return SendToZero(socket, description, args);
        }


        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="description"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private ZeroResult SendToZero(ZSocket socket, byte[] description, IEnumerable<byte[]> args)
        {
            using(MonitorScope.CreateScope("SendToZero"))
            using (var message = new ZMessage())
            {
                message.Add(new ZFrame(Station));
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