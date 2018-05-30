using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Newtonsoft.Json;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public static class ApiClient
    {
        /// <summary>
        /// 请求格式说明
        /// </summary>
        private static readonly byte[] CallDescription = {
            6,
            ZeroByteCommand.General,
            ZeroFrameType.Command,
            ZeroFrameType.Requester,
            ZeroFrameType.RequestId,
            ZeroFrameType.Context,
            ZeroFrameType.Argument,
            ZeroFrameType.GlobalId
        };
        /// <summary>
        /// 请求格式说明
        /// </summary>
        private static readonly byte[] WaitingDescription = {
            0,
            ZeroByteCommand.Waiting,
            ZeroFrameType.End
        };
        /// <summary>
        /// 请求格式说明
        /// </summary>
        private static readonly byte[] GetGlobalIdDescription = {
            0,
            ZeroByteCommand.GetGlobalId,
            ZeroFrameType.End
        };
        /// <summary>
        /// 请求格式说明
        /// </summary>
        private static readonly byte[] CloseDescription = {
            1,
            ZeroByteCommand.Close,
            ZeroFrameType.GlobalId
        };
        /// <summary>
        /// 请求格式说明
        /// </summary>
        private static readonly byte[] FindDescription = {
            1,
            ZeroByteCommand.Find,
            ZeroFrameType.GlobalId
        };
        /// <summary>
        /// 远程调用
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
        /// 远程调用
        /// </summary>
        /// <param name="station"></param>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        private static Task<string> CallTask(string station, string commmand, string argument)
        {
            var task = Task.Factory.StartNew(() => CallCommand(station, commmand, argument));
            return task;
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="station"></param>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static string CallCommand(string station, string commmand, string argument)
        {
            string result = null;
            using (MonitorStepScope.CreateScope("内部Zero调用"))
            {
                ApiContext.Current.LastError = 0;
                try
                {
                    LogRecorder.MonitorTrace($"Station:{station},Command:{commmand}");
                    result = Call(station, commmand, argument);
                }
                catch (Exception ex)
                {
                    ApiContext.Current.LastError = ErrorCode.Exception;
                    LogRecorder.Exception(ex);
                    LogRecorder.MonitorTrace($"发生异常：{ex.Message}");
                    result = ZeroStatuValue.InnerErrorJson;
                }
                finally
                {
                    LogRecorder.MonitorTrace($"Result:{result}");
                }
            }

            return result;
        }

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="station"></param>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static string Call(string station, string commmand, string argument)
        {
            if (!ZeroApplication.CanDo)
                return ZeroStatuValue.NoReadyJson;
            if (!ZeroApplication.Configs.TryGetValue(station, out var config))
            {
                ApiContext.Current.LastError = ErrorCode.NoFind;
                return ZeroStatuValue.NoFindJson;
            }
            var socket = ZeroConnectionPool.GetSocket(station);
            if (socket == null)
            {
                ApiContext.Current.LastError = ErrorCode.NoReady;
                return ZeroStatuValue.NoReadyJson;
            }

            try
            {
                var result = socket.Send(GetGlobalIdDescription);
                if (!result.InteractiveSuccess)
                {
                    ZeroTrace.WriteError("GetGlobalId", "Send Failed", station, commmand, argument);
                    ApiContext.Current.LastError = ErrorCode.NetworkError;
                    ZeroConnectionPool.Close(ref socket);
                    return JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.NetworkError, result.State.Text()));
                }
                result = socket.ReceiveString();
                if (!result.InteractiveSuccess)
                {
                    ZeroTrace.WriteError("GlobalId", config.RequestAddress, "Recv  Failed", station, commmand, argument);
                    ApiContext.Current.LastError = ErrorCode.NetworkError;
                    ZeroConnectionPool.Close(ref socket);
                    return JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.NetworkError, result.State.Text()));
                }
                if (result.TryGetValue(ZeroFrameType.GlobalId, out var globalId))
                    LogRecorder.MonitorTrace($"GlobalId:{long.Parse(globalId, NumberStyles.HexNumber)}");
                

                result = socket.QuietSend(CallDescription,
                    commmand,
                    ZeroApplication.Config.StationName,
                    ApiContext.RequestContext.RequestId,
                    JsonConvert.SerializeObject(ApiContext.Current),
                    argument,
                    globalId);
                if (!result.InteractiveSuccess)
                {
                    ZeroTrace.WriteError(station, "Send Failed", commmand, argument);
                    ApiContext.Current.LastError = ErrorCode.NetworkError;
                    ZeroConnectionPool.Close(ref socket);
                    return JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.NetworkError, result.State.Text()));
                }

                result = socket.ReceiveString(5,false);
                if (!result.InteractiveSuccess)
                {
                    bool finded = false;
                    int cnt = 0;
                    while (!finded && ++cnt < 5)
                    {
                        ZeroConnectionPool.Close(ref socket);

                        Thread.Sleep(1000);
                        socket = ZeroConnectionPool.GetSocket(station);
                        result = socket.QuietSend(FindDescription, globalId);
                        if (!result.InteractiveSuccess)
                            continue;

                        result = socket.ReceiveString(1, false);
                        if (!result.InteractiveSuccess || result.State == ZeroOperatorStateType.NoWorker)
                            continue;
                        ZeroTrace.WriteInfo("API", config.RequestAddress, "deliverer", commmand, globalId, cnt.ToString());
                        finded = true;
                    }

                    if (!finded)
                    {
                        ZeroTrace.WriteError("API", config.RequestAddress, "incorrigible", commmand, globalId);
                        ApiContext.Current.LastError = ErrorCode.NetworkError;
                        ZeroConnectionPool.Close(ref socket);
                        return JsonConvert.SerializeObject(ApiResult.Error(ErrorCode.TimeOut, result.State.Text(), globalId));
                    }
                }

                var lr = socket.QuietSend(CloseDescription, globalId);
                if (!lr.InteractiveSuccess)
                {
                    ZeroTrace.WriteError(station, config.RequestAddress, "Close Failed", commmand, globalId);
                }
                lr = socket.ReceiveString(1, false);
                if (!lr.InteractiveSuccess)
                {
                    ZeroTrace.WriteError(station, config.RequestAddress, "Close Failed", commmand, globalId);
                }
                //else
                //{
                //    ApiContext.Current.LastError = ErrorCode.CallApiError;
                //    StationConsole.WriteError("API", config.RequestAddress, "No Waiting", commmand, argument);
                //}
                switch (result.State)
                {
                    case ZeroOperatorStateType.Plan:
                    case ZeroOperatorStateType.VoteRuning:
                    case ZeroOperatorStateType.VoteBye:
                    case ZeroOperatorStateType.VoteWecome:
                    case ZeroOperatorStateType.VoteSend:
                    case ZeroOperatorStateType.VoteWaiting:
                    case ZeroOperatorStateType.VoteStart:
                    case ZeroOperatorStateType.VoteEnd:
                        return ZeroStatuValue.SucceesJson;
                    case ZeroOperatorStateType.Error:
                        return ZeroStatuValue.InnerErrorJson;
                    case ZeroOperatorStateType.NoSupport:
                    case ZeroOperatorStateType.NoFind:
                    case ZeroOperatorStateType.NoWorker:
                        ApiContext.Current.LastError = ErrorCode.NoFind;
                        return ZeroStatuValue.NoFindJson;
                    case ZeroOperatorStateType.Invalid:
                        ApiContext.Current.LastError = ErrorCode.ArgumentError;
                        return ZeroStatuValue.ArgumentErrorJson;
                    case ZeroOperatorStateType.TimeOut:
                        return ZeroStatuValue.TimeOutJson;
                    case ZeroOperatorStateType.NetError:
                        ApiContext.Current.LastError = ErrorCode.NetworkError;
                        return ZeroStatuValue.NetworkErrorJson;
                    case ZeroOperatorStateType.Failed:
                        ApiContext.Current.LastError = ErrorCode.BusinessError;
                        return ZeroStatuValue.UnknowErrorJson;
                    default:
                        return ZeroStatuValue.UnknowErrorJson;
                    case ZeroOperatorStateType.Ok:
                        ApiContext.Current.LastError = ErrorCode.Success;
                        return result.TryGetValue(ZeroFrameType.JsonValue, out var json) ? json : ZeroStatuValue.SucceesJson;
                }
            }
            finally
            {
                ZeroConnectionPool.Free(socket);
            }

        }
    }
}