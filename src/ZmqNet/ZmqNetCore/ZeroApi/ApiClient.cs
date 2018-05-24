using System;
using System.Collections.Generic;
using System.Globalization;
using Agebull.Common.Logging;
using Agebull.ZeroNet.ZeroApi;
using NetMQ;
using Newtonsoft.Json;
using ErrorCode = NetMQ.ErrorCode;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public static class ApiClient
    {
        /// <summary>
        /// 请求格式说明
        /// </summary>
        private static readonly byte[] CallDescription = new byte[]
        {
            5,
            ZeroByteCommand.General,
            ZeroFrameType.Command,
            ZeroFrameType.Requester,
            ZeroFrameType.RequestId,
            ZeroFrameType.Context,
            ZeroFrameType.Argument,
            ZeroFrameType.End
        };
        /// <summary>
        /// 请求格式说明
        /// </summary>
        private static readonly byte[] WaitingDescription = new byte[]
        {
            0,
            ZeroByteCommand.Waiting,
            ZeroFrameType.End
        };
        /// <summary>
        /// 远程调用
        /// </summary>
        /// <param name="station"></param>
        /// <param name="commmand"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static string Call(string station, string commmand, string argument)
        {
            string result = null;
            using (MonitorStepScope.CreateScope("内部Zero调用"))
            {
                try
                {
                    LogRecorder.MonitorTrace($"Station:{station},Command:{commmand}");
                    result = CallInner(station, commmand, argument);
                }
                catch (Exception ex)
                {
                    LogRecorder.Exception(ex);
                    LogRecorder.MonitorTrace($"发生异常：{ex.Message}");
                    result = ZeroNetStatus.NetworkErrorJson;
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
        public static string CallInner(string station, string commmand, string argument)
        {
            if (ZeroApplication.State != StationState.Run)
                return ZeroNetStatus.NoReadyJson;
            if (!ZeroApplication.Configs.TryGetValue(station, out var config))
            {
                return ZeroNetStatus.NoFindJson;
            }
            var socket = config.GetSocket();
            if (socket == null)
            {
                return ZeroNetStatus.NoReadyJson;
            }

            try
            {
                var result = socket.Send(CallDescription,
                    commmand,
                    ZeroApplication.Config.StationName,
                    ApiContext.RequestContext.RequestId,
                    JsonConvert.SerializeObject(ApiContext.Current),
                    argument);
                if (!result.InteractiveSuccess)
                {
                    config.Close(socket);
                    socket = null;
                    return JsonConvert.SerializeObject(ApiResult.Error(Agebull.ZeroNet.ZeroApi.ErrorCode.NetworkError, result.State.Text()));
                }

                result = socket.ReceiveString();
                if (!result.InteractiveSuccess)
                {
                    config.Close(socket);
                    socket = null;
                    return JsonConvert.SerializeObject(ApiResult.Error(Agebull.ZeroNet.ZeroApi.ErrorCode.NetworkError, result.State.Text()));
                }

                if(result.TryGetValue(ZeroFrameType.GlobalId, out var gid))
                    LogRecorder.MonitorTrace($"GlobalId:{long.Parse(gid,NumberStyles.HexNumber)}");
                if (result.State == ZeroStateType.VoteWaiting)
                {
                    result = socket.Send(WaitingDescription);
                    if (!result.InteractiveSuccess)
                    {
                        config.Close(socket);
                        socket = null;
                        return JsonConvert.SerializeObject(ApiResult.Error(Agebull.ZeroNet.ZeroApi.ErrorCode.NetworkError, result.State.Text()));
                    }
                    result = socket.ReceiveString();
                    if (!result.InteractiveSuccess)
                    {
                        config.Close(socket);
                        socket = null;
                        var apiResult = ApiResult.Error(Agebull.ZeroNet.ZeroApi.ErrorCode.NetworkError, result.State.Text());
                        apiResult.Status.InnerMessage = gid;
                        return JsonConvert.SerializeObject(apiResult);
                    }
                }
                switch (result.State)
                {
                    case ZeroStateType.Plan:
                    case ZeroStateType.VoteRuning:
                    case ZeroStateType.VoteBye:
                    case ZeroStateType.VoteWecome:
                    case ZeroStateType.VoteSend:
                    case ZeroStateType.VoteWaiting:
                    case ZeroStateType.VoteStart:
                    case ZeroStateType.VoteEnd:
                        return ZeroNetStatus.SucceesJson;
                    case ZeroStateType.Error:
                        return ZeroNetStatus.InnerErrorJson;
                    case ZeroStateType.NoSupport:
                    case ZeroStateType.NoFind:
                    case ZeroStateType.NoWorker:
                        return ZeroNetStatus.NoFindJson;
                    case ZeroStateType.Invalid:
                        return ZeroNetStatus.ArgumentErrorJson;
                    case ZeroStateType.TimeOut:
                        return ZeroNetStatus.TimeOutJson;
                    case ZeroStateType.NetError:
                        return ZeroNetStatus.NetworkErrorJson;
                    case ZeroStateType.Failed:
                        return ZeroNetStatus.UnknowErrorJson;
                    default:
                        return ZeroNetStatus.UnknowErrorJson;
                    case ZeroStateType.Ok:
                        return result.TryGetValue(ZeroFrameType.JsonValue, out var json) ? json : ZeroNetStatus.SucceesJson;
                }
            }
            finally
            {
                config.Free(socket);
            }
            
        }
    }
}