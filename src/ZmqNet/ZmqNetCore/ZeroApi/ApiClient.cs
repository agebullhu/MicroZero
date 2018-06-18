using System;
using System.Globalization;
using System.Linq;
using System.Text;
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
    public class ApiClient
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
            return Task.Factory.StartNew(() => Call(station, commmand, argument));
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
        /// 返回值
        /// </summary>
        public string Result => _json;
        /// <summary>
        /// 请求站点
        /// </summary>
        public string Station { get; set; }
        /// <summary>
        /// 调用命令
        /// </summary>
        public string Commmand { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        public string Argument { get; set; }
        /// <summary>
        /// 结果状态
        /// </summary>
        public ZeroOperatorStateType State { get; set; }

        /// <summary>
        /// 返回的数据
        /// </summary>
        private string _json;

        /// <summary>
        /// 远程调用
        /// </summary>
        /// <returns></returns>
        public void CallCommand()
        {
            using (MonitorScope.CreateScope("内部Zero调用"))
            {
                //ApiContext.Current.LastError = 0;
                LogRecorder.MonitorTrace($"Station:{Station},Command:{Commmand}");
                Call();
                //LogRecorder.MonitorTrace($"Result:{Result}");
            }
        }
        /// <summary>
        /// 检查在非成功状态下的返回值
        /// </summary>
        public void CheckStateResult()
        {
            switch (State)
            {
                case ZeroOperatorStateType.LocalNoReady:
                case ZeroOperatorStateType.LocalZmqError:
                    _json = ZeroStatuValue.NoReadyJson;
                    return;
                case ZeroOperatorStateType.LocalSendError:
                case ZeroOperatorStateType.LocalRecvError:
                    _json = ZeroStatuValue.NetworkErrorJson;
                    return;
                case ZeroOperatorStateType.LocalException:
                    _json = ZeroStatuValue.UnknowErrorJson;
                    return;
                case ZeroOperatorStateType.Plan:
                case ZeroOperatorStateType.Runing:
                case ZeroOperatorStateType.VoteBye:
                case ZeroOperatorStateType.Wecome:
                case ZeroOperatorStateType.VoteSend:
                case ZeroOperatorStateType.VoteWaiting:
                case ZeroOperatorStateType.VoteStart:
                case ZeroOperatorStateType.VoteEnd:
                    _json = ZeroStatuValue.SucceesJson;
                    return;
                case ZeroOperatorStateType.Error:
                    _json = ZeroStatuValue.InnerErrorJson;
                    return;
                case ZeroOperatorStateType.NotSupport:
                case ZeroOperatorStateType.NotFind:
                case ZeroOperatorStateType.NoWorker:
                    ApiContext.Current.LastError = ErrorCode.NoFind;
                    _json = ZeroStatuValue.NoFindJson;
                    return;
                case ZeroOperatorStateType.Invalid:
                    ApiContext.Current.LastError = ErrorCode.ArgumentError;
                    _json = ZeroStatuValue.ArgumentErrorJson;
                    return;
                case ZeroOperatorStateType.TimeOut:
                    _json = ZeroStatuValue.TimeOutJson;
                    return;
                case ZeroOperatorStateType.NetError:
                    ApiContext.Current.LastError = ErrorCode.NetworkError;
                    _json = ZeroStatuValue.NetworkErrorJson;
                    return;
                case ZeroOperatorStateType.Failed:
                    ApiContext.Current.LastError = ErrorCode.BusinessError;
                    _json = ZeroStatuValue.UnknowErrorJson;
                    return;
                default:
                    _json = ZeroStatuValue.UnknowErrorJson;
                    return;
                case ZeroOperatorStateType.Ok:
                    ApiContext.Current.LastError = ErrorCode.Success;
                    return;
            }
        }
        /// <summary>
        /// 远程调用
        /// </summary>
        /// <returns></returns>
        private void Call()
        {
            if (!ZeroApplication.ZerCenterIsRun)
            {
                State = ZeroOperatorStateType.LocalNoReady;
                return;
            }
            var socket = ZeroConnectionPool.GetSocket(Station, ApiContext.RequestContext.RequestId);
            if (socket.Socket == null)
            {
                ApiContext.Current.LastError = ErrorCode.NoReady;
                _json = ZeroStatuValue.NoReadyJson;
                State = ZeroOperatorStateType.LocalNoReady;
                return;
            }
            using (socket)
            {
                ReadNetWork(socket);
            }
        }

        private void ReadNetWork(PoolSocket socket)
        {
            var result = socket.Socket.QuietSend(GetGlobalIdDescription, ApiContext.RequestContext.RequestId);
            if (!result.InteractiveSuccess)
            {
                socket.HaseFailed = true;
                //ZeroTrace.WriteError("GetGlobalId", "Send Failed", station, commmand, argument);
                ApiContext.Current.LastError = ErrorCode.NetworkError;
                State = ZeroOperatorStateType.LocalSendError;
                return;
            }

            result = ReceiveString(socket.Socket);
            if (!result.InteractiveSuccess)
            {
                socket.HaseFailed = true;
                //ZeroTrace.WriteError("GlobalId", "Recv  Failed", station, commmand, argument);
                ApiContext.Current.LastError = ErrorCode.NetworkError;
                State = ZeroOperatorStateType.LocalRecvError;
                return;
            }

            if (result.TryGetValue(ZeroFrameType.GlobalId, out var globalId))
                LogRecorder.MonitorTrace($"GlobalId:{long.Parse(globalId, NumberStyles.HexNumber)}");


            result = socket.Socket.QuietSend(CallDescription,
                Commmand,
                ApiContext.RequestContext.RequestId,
                JsonConvert.SerializeObject(ApiContext.Current),
                Argument, 
                ApiContext.RequestContext.RequestId,//作名称
                globalId);
            if (!result.InteractiveSuccess)
            {
                socket.HaseFailed = true;
                //ZeroTrace.WriteError(station, "Send Failed", commmand, argument);
                ApiContext.Current.LastError = ErrorCode.NetworkError;
                State = ZeroOperatorStateType.LocalSendError;
                return;
            }

            result = ReceiveString(socket.Socket);
            if (!result.InteractiveSuccess)
            {
                //bool finded = false;
                //int cnt = 0;
                //while (!finded && ++cnt < 5)
                //{
                //    Thread.Sleep(1000);
                //    socket.ReBuild();
                //    result = socket.Socket.QuietSend(FindDescription, globalId, name);
                //    if (!result.InteractiveSuccess)
                //    {
                //        continue;
                //    }
                //    if (result.State == ZeroOperatorStateType.NoWorker)
                //    {
                //        serializeObject = null;
                //        return ZeroOperatorStateType.NetError;
                //    }
                //    result = ReceiveString(socket.Socket);
                //    if (!result.InteractiveSuccess || result.State == ZeroOperatorStateType.NoWorker)
                //        continue;
                //    ZeroTrace.WriteInfo("API", "deliverer", commmand, globalId, cnt.ToString());
                //    finded = true;
                //}

                //if (!finded)
                {
                    socket.HaseFailed = true;
                    //ZeroTrace.WriteError("API", "incorrigible", commmand, globalId);
                    ApiContext.Current.LastError = ErrorCode.NetworkError;
                    State = ZeroOperatorStateType.LocalRecvError;
                    return;
                }
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
                {
                    ZeroTrace.WriteError("Receive", socket.Connects.LinkToString(','), socket.LastError.Text, $"Socket Ptr:{ socket.SocketPtr}");
                }
                return new ZeroResultData
                {
                    State = ZeroOperatorStateType.LocalRecvError,
                    //ZmqErrorMessage = error?.Text,
                    //ZmqErrorCode = error?.Number ?? 0
                };
            }

            try
            {
                var description = messages[0].Read();
                if (description.Length == 0)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", socket.Connects.LinkToString(','), description.LinkToString(p => p.ToString("X2"), ""), $"Socket Ptr:{ socket.SocketPtr}.");
                    return new ZeroResultData
                    {
                        State = ZeroOperatorStateType.Invalid,
                        Message = "网络格式错误"
                    };
                }

                int end = description[0] + 1;
                if (end != messages.Count)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", socket.Connects.LinkToString(','), $"FrameSize{messages.Count}", description.LinkToString(p => p.ToString("X2"), ""), $"Socket Ptr:{ socket.SocketPtr}.");
                    return new ZeroResultData
                    {
                        State = ZeroOperatorStateType.Invalid,
                        Message = "网络格式错误"
                    };
                }

                var result = new ZeroResultData
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

    }
}