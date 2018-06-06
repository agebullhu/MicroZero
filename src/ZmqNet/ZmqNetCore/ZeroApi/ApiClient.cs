using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Newtonsoft.Json;
using ZeroMQ;

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
            ZeroFrameType.RequestId,
            ZeroFrameType.Context,
            ZeroFrameType.Argument,
            ZeroFrameType.Requester,
            ZeroFrameType.GlobalId
        };
        /// <summary>
        /// 请求格式说明
        /// </summary>
        private static readonly byte[] GetGlobalIdDescription = {
            1,
            ZeroByteCommand.GetGlobalId,
            ZeroFrameType.Requester,
            ZeroFrameType.End
        };
        /// <summary>
        /// 请求格式说明
        /// </summary>
        private static readonly byte[] CloseDescription = {
            2,
            ZeroByteCommand.Close,
            ZeroFrameType.Requester,
            ZeroFrameType.GlobalId
        };
        /// <summary>
        /// 请求格式说明
        /// </summary>
        private static readonly byte[] FindDescription = {
            2,
            ZeroByteCommand.Find,
            ZeroFrameType.Requester,
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
        public static Task<string> CallTask(string station, string commmand, string argument)
        {
            return Task.Factory.StartNew(() => CallCommand(station, commmand, argument));
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
            if (ZeroApplication.ApplicationState != StationState.Run || ZeroApplication.ZerCenterStatus != ZeroCenterState.Run)
                return ZeroStatuValue.NoReadyJson;
            var config = ZeroApplication.Config[station];
            if (config == null)
            {
                ApiContext.Current.LastError = ErrorCode.NoFind;
                return ZeroStatuValue.NoFindJson;
            }
            if (config.State != ZeroCenterState.Run)
            {
                ApiContext.Current.LastError = ErrorCode.Unavailable;
                return ZeroStatuValue.UnavailableJson;
            }

            string name = ZeroIdentityHelper.CreateRealName(false, station);
            var socket = ZeroHelper.CreateClientSocket($"inproc://{station}_Proxy", ZSocketType.PAIR, name.ToAsciiBytes());
            if (socket == null)
            {
                ApiContext.Current.LastError = ErrorCode.NoReady;
                return ZeroStatuValue.NoReadyJson;
            }

            string serializeObject;
            ZeroOperatorStateType state = ReadNetWork(station, commmand, argument, socket, name, config, out serializeObject);
            socket.CloseSocket();
            switch (state)
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
                    return serializeObject ?? ZeroStatuValue.SucceesJson;
            }
        }

        private static ZeroOperatorStateType ReadNetWork(string station, string commmand, string argument, ZSocket socket, string name,
            StationConfig config, out string serializeObject)
        {
            serializeObject = null;
            var result = socket.Send(GetGlobalIdDescription, name);
            if (!result.InteractiveSuccess)
            {
                ZeroTrace.WriteError("GetGlobalId", "Send Failed", station, commmand, argument);
                ApiContext.Current.LastError = ErrorCode.NetworkError;
                return ZeroOperatorStateType.NetError;
            }

            result = ReceiveString(socket);
            if (!result.InteractiveSuccess)
            {
                ZeroTrace.WriteError("GlobalId", config.RequestAddress, "Recv  Failed", station, commmand, argument);
                ApiContext.Current.LastError = ErrorCode.NetworkError;
                return ZeroOperatorStateType.NetError;
            }

            if (result.TryGetValue(ZeroFrameType.GlobalId, out var globalId))
                LogRecorder.MonitorTrace($"GlobalId:{long.Parse(globalId, NumberStyles.HexNumber)}");


            result = socket.QuietSend(CallDescription,
                commmand,
                ApiContext.RequestContext.RequestId,
                JsonConvert.SerializeObject(ApiContext.Current),
                argument, name,
                globalId);
            if (!result.InteractiveSuccess)
            {
                ZeroTrace.WriteError(station, "Send Failed", commmand, argument);
                ApiContext.Current.LastError = ErrorCode.NetworkError;
                return ZeroOperatorStateType.NetError;
            }
            result = ReceiveString(socket);
            if (!result.InteractiveSuccess)
            {
                bool finded = false;
                int cnt = 0;
                while (!finded && ++cnt < 5)
                {
                    Thread.Sleep(1000);
                    socket = ZeroConnectionPool.GetSocket(station);
                    result = socket.QuietSend(FindDescription, globalId, name);
                    if (!result.InteractiveSuccess)
                    {
                        continue;
                    }
                    if (result.State == ZeroOperatorStateType.NoWorker)
                    {
                        serializeObject = null;
                        return ZeroOperatorStateType.NetError;
                    }
                    result = ReceiveString(socket);
                    if (!result.InteractiveSuccess || result.State == ZeroOperatorStateType.NoWorker)
                        continue;
                    ZeroTrace.WriteInfo("API", config.RequestAddress, "deliverer", commmand, globalId, cnt.ToString());
                    finded = true;
                }

                if (!finded)
                {
                    ZeroTrace.WriteError("API", config.RequestAddress, "incorrigible", commmand, globalId);
                    ApiContext.Current.LastError = ErrorCode.NetworkError;
                    return ZeroOperatorStateType.TimeOut;
                }
            }

            if (result.State == ZeroOperatorStateType.NoWorker)
            {
                return ZeroOperatorStateType.NoWorker;
            }

            var lr = socket.QuietSend(CloseDescription, name, globalId);
            if (!lr.InteractiveSuccess)
            {
                ZeroTrace.WriteError(station, config.RequestAddress, "Close Failed", commmand, globalId);
            }
            lr = ReceiveString(socket);
            if (!lr.InteractiveSuccess)
            {
                ZeroTrace.WriteError(station, config.RequestAddress, "Close Failed", commmand, globalId, lr.ZmqErrorMessage);
            }

            result.TryGetValue(ZeroFrameType.JsonValue, out serializeObject);
            return result.State;
        }

        /// <summary>
        ///     接收文本
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static ZeroResultData<string> ReceiveString(ZSocket socket)
        {
            var sockets = new[] { socket };
            var pollItems = new[] { ZPollItem.CreateReceiver() };
            if (!sockets.PollIn(pollItems, out var mes, out var error, TimeSpan.FromSeconds(30)))
            {
                if (error != null && !Equals(error, ZError.EAGAIN))
                {
                    ZeroTrace.WriteError("Receive", socket.Connects.LinkToString(','), error.Text, $"Socket Ptr:{ socket.SocketPtr}");
                }
                return new ZeroResultData<string>
                {
                    State = ZeroOperatorStateType.LocalRecvError,
                    ZmqErrorMessage = error?.Text,
                    ZmqErrorCode = error?.Number ?? 0
                };
            }

            var messages = mes[0];

            try
            {
                var description = messages[0].Read();
                if (description.Length == 0)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", socket.Connects.LinkToString(','), description.LinkToString(p => p.ToString("X2"), ""), $"Socket Ptr:{ socket.SocketPtr}.");
                    return new ZeroResultData<string>
                    {
                        State = ZeroOperatorStateType.Invalid,
                        ZmqErrorMessage = "网络格式错误",
                        ZmqErrorCode = -1
                    };
                }

                int end = description[0] + 1;
                if (end != messages.Count)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", socket.Connects.LinkToString(','), $"FrameSize{messages.Count}", description.LinkToString(p => p.ToString("X2"), ""), $"Socket Ptr:{ socket.SocketPtr}.");
                    return new ZeroResultData<string>
                    {
                        State = ZeroOperatorStateType.Invalid,
                        ZmqErrorMessage = "网络格式错误",
                        ZmqErrorCode = -2
                    };
                }

                var result = new ZeroResultData<string>
                {
                    InteractiveSuccess = true,
                    State = (ZeroOperatorStateType)description[1]
                };
                for (int idx = 1; idx < end; idx++)
                {
                    result.Add(description[idx + 1], Encoding.UTF8.GetString(messages[idx].Read()));
                }

                return result;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e, socket.Connects.LinkToString(','), $"Socket Ptr:{ socket.SocketPtr}.");
                return new ZeroResultData<string>
                {
                    State = ZeroOperatorStateType.Exception,
                    Exception = e
                };
            }
            finally
            {
                messages.Dispose();
            }
        }

    }
}