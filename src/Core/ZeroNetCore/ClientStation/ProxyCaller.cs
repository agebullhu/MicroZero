using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        internal StationConfig Config;
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
            if (!CreateSocket(out var socket))
                return;
            using (socket)
            {
                var task = CallApi(socket);
                task.Wait();
            }
        }


        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        internal async Task CallAsync()
        {
            if (!CreateSocket(out var socket))
                return;
            using (socket)
            {
                await CallApi(socket);
            }
        }


        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        internal void Plan(ZeroPlanInfo plan)
        {
            if (!CreateSocket(out var socket))
                return;
            using (socket)
            {
                var task = CallPlan(socket, plan);
                task.Wait();
            }
        }

        /// <summary>
        ///     远程调用
        /// </summary>
        /// <returns></returns>
        internal async Task PlanAsync(ZeroPlanInfo plan)
        {
            if (!CreateSocket(out var socket))
                return;
            using (socket)
            {
                await CallPlan(socket, plan);
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
                case ZeroOperatorStateType.NetTimeOut:
                    apiResult = ApiResultIoc.Ioc.NetTimeOut;
                    break;
                case ZeroOperatorStateType.ExecTimeOut:
                    apiResult = ApiResultIoc.Ioc.ExecTimeOut;
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

        private bool CreateSocket(out ZSocket socket)
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                Result = ApiResultIoc.NoReadyJson;
                State = ZeroOperatorStateType.LocalNoReady;
                socket = null;
                return false;
            }

            if (!ZeroApplication.Config.TryGetConfig(Station, out Config))
            {
                Result = ApiResultIoc.NoFindJson;
                State = ZeroOperatorStateType.NotFind;
                socket = null;
                return false;
            }

            if (Config.State == ZeroCenterState.None || Config.State == ZeroCenterState.Pause)
            {
                Result = ApiResultIoc.PauseJson;
                State = ZeroOperatorStateType.Pause;
                socket = null;
                return false;
            }

            if (Config.State != ZeroCenterState.Run)
            {
                Result = ApiResultIoc.NotSupportJson;
                State = ZeroOperatorStateType.Pause;
                socket = null;
                return false;
            }

            State = ZeroOperatorStateType.None;
            socket = ApiProxy.GetProxySocket(Name);
            socket.ServiceKey = Config.ServiceKey;
            return true;
        }

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


        private async Task CallApi(ZSocket socket)
        {
            if (Files == null || Files.Count <= 0)
                LastResult = await CallNoFileApi(socket);
            else
                LastResult = await CallByFileApi(socket);

            if (!LastResult.InteractiveSuccess)
            {

                State = LastResult.State;
                return;
            }

            await CallEnd(socket);
        }
        private async Task<ZeroResult> CallNoFileApi(ZSocket socket)
        {
            return await Send(socket,
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

        private async Task<ZeroResult> CallByFileApi(ZSocket socket)
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

            return await SendToZero(socket, description, frames);
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

        private async Task CallPlan(ZSocket socket, ZeroPlanInfo plan)
        {
            LastResult = await Send(socket,
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
            await CallEnd(socket);

        }
        async Task<bool> CallEnd(ZSocket socket)
        {
            int cnt = 0;
            while (cnt < 5)
            {
                LastResult = await Receive(socket);
                State = LastResult.State;
                if (!LastResult.InteractiveSuccess)
                {
                    LogRecorderX.MonitorTrace($"failed {State}");
                    return false;
                }
                if (LastResult.State == ZeroOperatorStateType.Runing)
                {
                    LogRecorderX.MonitorTrace($"delay {cnt}");
                    await Task.Delay(cnt * 100);
                    cnt++;
                    continue;
                }

                Result = LastResult.Result;
                Binary = LastResult.Binary;
                ResultType = LastResult.ResultType;
                LogRecorderX.MonitorTrace($"result:{Result}");
                return true;
            }

            LogRecorderX.MonitorTrace("time out");
            State = ZeroOperatorStateType.NetTimeOut;
            return false;
        }

        /// <summary>
        ///     接收
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        internal async Task<ZeroResult> Receive(ZSocket socket)
        {
            using (MonitorScope.CreateScope("Recv"))
            {
                var frames = await socket.RecvAsync();
                if (frames == null)
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
        internal async Task<ZeroResult> Send(ZSocket socket, byte[] description, params byte[][] args)
        {
            return await SendToZero(socket, description, args);
        }


        /// <summary>
        ///     一次请求
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="description"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task<ZeroResult> SendToZero(ZSocket socket, byte[] description, IEnumerable<byte[]> args)
        {
            using (MonitorScope.CreateScope("SendToZero"))
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
                    message.Add(new ZFrame(Config.ServiceKey));

                }

                var res = await socket.SendToAsync(message);
                if (!res)
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