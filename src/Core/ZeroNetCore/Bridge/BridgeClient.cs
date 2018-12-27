using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Gboxt.Common.DataModel;
using ZeroMQ;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public class BridgeClient : ZeroStation
    {
        /// <inheritdoc />
        public BridgeClient() : base(ZeroStationType.Dispatcher, true)
        {
            Name = "Bridge";
            StationName = "Bridge";
        }

        /// <summary>
        /// 配置检查
        /// </summary>
        /// <returns></returns>
        protected override bool CheckConfig()
        {
            Config = new StationConfig
            {
                Name = "Bridge",
                StationName = "Bridge"
            };
            return base.CheckConfig();
        }

        private ZSocket _pushSocket;

        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected override bool RunInner(CancellationToken token)
        {
            ZeroTrace.SystemLog(StationName, "run", "Bridge");
            var socket = ZSocket.CreateClientSocket(ZeroApplication.Config.BridgeCallAddress, ZSocketType.PULL);
            _pushSocket = ZSocket.CreateClientSocket(ZeroApplication.Config.BridgeResultAddress, ZSocketType.PUSH);
            using (var pool = ZmqPool.CreateZmqPool())
            {
                pool.Prepare(ZPollEvent.In, socket);
                State = StationState.Run;
                while (CanLoop)
                {
                    if (!pool.Poll() || !pool.CheckIn(0, out var message))
                    {
                        continue;
                    }
                    Task.Factory.StartNew(() => Call(message), token);
                }
            }

            ZeroTrace.SystemLog(StationName, "end", Config.WorkerCallAddress, Name, "Bridge");
            socket.Dispose();
            _pushSocket.Dispose();
            return true;
        }

        void Call(ZMessage message)
        {
            var frames = new List<byte[]>();
            if (!Unpack(message, out var station, frames, out var item))
            {
                return;
            }
            var socket = ZeroConnectionPool.GetSocket(station, RandomOperate.Generate(8));
            if (socket?.Socket == null)
            {
                SendResult(item, ZeroOperatorStateType.LocalNoReady);
                return;
            }

            using (socket)
            {
                using (ZMessage message2 = new ZMessage())
                {
                    foreach (var line in frames)
                        message2.Append(new ZFrame(line));
                    if (!socket.Socket.SendMessage(message2, out var error))
                    {
                        ZeroTrace.WriteError(StationName, error.Text, error.Name);
                        SendResult(item, ZeroOperatorStateType.RemoteSendError);
                        return;
                    }
                }
                if (!socket.Socket.Recv(out var message3))
                {
                    SendResult(item, ZeroOperatorStateType.RemoteRecvError);
                    return;
                }
                SendResult(item.Caller, message3);
            }
        }

        #region IO

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="station"></param>
        /// <param name="frames"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool Unpack(ZMessage messages, out string station, List<byte[]> frames, out ApiCallItem item)
        {
            station = null;

            item = new ApiCallItem
            {
                First = messages[0].Read(),
            };
            try
            {
                station = messages[1].ReadString(Encoding.ASCII);
                var description = messages[2].Read();
                if (description.Length < 2)
                {
                    ZeroTrace.WriteError("Receive", "LaoutError", Config.WorkerResultAddress, description.LinkToString(p => p.ToString("X2"), ""));
                    item = null;
                    return false;
                }
                for (int idx = 3; idx < messages.Count; idx++)
                {
                    var bytes = messages[idx].Read();
                    frames.Add(bytes);
                    if (bytes.Length == 0)
                        continue;
                    switch (description[idx])
                    {
                        case ZeroFrameType.GlobalId:
                            item.CallerGlobalId = Encoding.ASCII.GetString(bytes).TrimEnd('\0');
                            break;
                        case ZeroFrameType.RequestId:
                            item.RequestId = Encoding.ASCII.GetString(bytes).TrimEnd('\0');
                            break;
                        case ZeroFrameType.Requester:
                            item.Requester = Encoding.ASCII.GetString(bytes).TrimEnd('\0');
                            break;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("Receive", e,
                    Config.WorkerResultAddress, $"FrameSize{messages.Count}");
                return false;
            }
            finally
            {
                messages.Dispose();
            }
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private bool SendResult(ApiCallItem item, ZeroOperatorStateType state)
        {
            if (_pushSocket == null)
            {
                ZeroTrace.WriteError("SendResult", "is closed", StationName);
                return false;
            }
            try
            {
                var message4 = new ZMessage
                {
                    new ZFrame(item.Caller),
                    new ZFrame(new byte[]
                    {
                        2, (byte) state, ZeroFrameType.RequestId, ZeroFrameType.GlobalId
                    }),
                    new ZFrame(item.RequestId.ToZeroBytes()),
                    new ZFrame((item.CallerGlobalId).ToZeroBytes())
                };
                using (message4)
                {
                    if (_pushSocket.Send(message4, out var error))
                        return true;
                    ZeroTrace.WriteError(StationName, error.Text, error.Name);
                    return false;
                }
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "ApiStation.SendResult");
                return false;
            }
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        private bool SendResult(byte[] caller, ZMessage messages)
        {
            if (_pushSocket == null)
            {
                ZeroTrace.WriteError("SendResult", "is closed", StationName);
                return false;
            }
            try
            {
                ZError error;
                using (messages)
                {
                    using (ZMessage message4 = new ZMessage())
                    {
                        message4.Append(new ZFrame(caller));
                        foreach (var line in messages)
                            message4.Append(new ZFrame(line.Read()));
                        if (_pushSocket.Send(message4, out error))
                            return true;
                    }
                }
                ZeroTrace.WriteError(StationName, error.Text, error.Name);
                return false;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "ApiStation.SendResult");
                return false;
            }
        }

        #endregion
    }
}