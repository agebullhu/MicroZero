using System;
using System.Collections.Generic;
using System.Threading;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using ZeroMQ;
using Agebull.Common.Rpc;

namespace Agebull.ZeroNet.ZeroApi
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public class ApiStation : ApiStationBase
    {
        /// <summary>
        /// 构造
        /// </summary>
        public ApiStation() : base(ZeroStationType.Api, true)
        {

        }

        /// <summary>
        /// 构造Pool
        /// </summary>
        /// <returns></returns>
        protected override IZmqPool Prepare(byte[] identity, out ZSocket socket)
        {
            socket = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER, identity);
            var pool = ZmqPool.CreateZmqPool();
            pool.Prepare(new[] { ZSocket.CreateClientSocket(Config.WorkerCallAddress, ZSocketType.PULL, identity) }, ZPollEvent.In);
            return pool;
        }
        

        /// <summary>
        ///     配置校验
        /// </summary>
        protected override ZeroStationOption GetApiOption()
        {
            return ZeroApplication.GetApiOption(StationName);
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal override bool OnExecuestEnd(ref ZSocket socket, ApiCallItem item, ZeroOperatorStateType state)
        {
            if (!CanLoop)
            {
                ZeroTrace.WriteError("SendResult", "is closed", StationName);
                return false;
            }

            int i = 0;
            var des = new byte[(item.Result == null ? 7 : 8) + item.Originals.Count];
            des[i++] = (byte)(5 + item.Originals.Count);
            des[i++] = (byte)state;
            des[i++] = ZeroFrameType.Requester;
            des[i++] = ZeroFrameType.RequestId;
            des[i++] = ZeroFrameType.GlobalId;
            var msg = new List<byte[]>
            {
                item.Caller,
                des,
                item.Requester.ToZeroBytes(),
                item.RequestId.ToZeroBytes(),
                item.CallerGlobalId.ToZeroBytes()
            };
            if (item.Result != null)
            {
                des[i++] = ZeroFrameType.JsonValue;
                msg.Add(item.Result.ToZeroBytes());
            }

            foreach (var org in item.Originals)
            {
                des[i++] = org.Key;
                msg.Add((org.Value));
            }
            des[i++] = ZeroFrameType.SerivceKey;
            msg.Add((GlobalContext.ServiceKey.ToZeroBytes()));
            des[i] = ZeroFrameType.End;
            return SendResult(ref socket, new ZMessage(msg));
        }

        private static readonly byte[] LayoutErrorFrame = new byte[]
        {
            1,
            (byte) ZeroOperatorStateType.FrameInvalid,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.End
        };
        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        internal override void SendLayoutErrorResult(ref ZSocket socket, ApiCallItem item)
        {
            if (item == null)
                return;
            try
            {
                SendResult(ref socket, new ZMessage
                {
                    new ZFrame(item.Caller),
                    new ZFrame(LayoutErrorFrame),
                    new ZFrame(GlobalContext.ServiceKey.ToZeroBytes())
                });
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
            }
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool SendResult(ref ZSocket socket, ZMessage message)
        {
            try
            {
                ZError error;
                using (message)
                {
                    if (socket.Send(message, out error))
                        return true;
                    byte[] id = socket.Identity;
                    socket.TryClose();
                    socket = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER, id);
                }
                ZeroTrace.WriteError(StationName, error.Text, error.Name);
                Interlocked.Increment(ref SendError);
                return false;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e, "ApiStation.SendResult");
                return false;
            }
            finally
            {
                Interlocked.Decrement(ref WaitCount);
            }
        }

    }
}