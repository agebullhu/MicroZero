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
            var pSocket = ZeroApplication.Config.ApiRouterModel
                    ? ZSocket.CreateClientSocket(Config.WorkerCallAddress, ZSocketType.DEALER, identity)
                    : ZSocket.CreateClientSocket(Config.WorkerCallAddress, ZSocketType.PULL, identity);

            var pool = ZmqPool.CreateZmqPool();
            pool.Prepare(ZPollEvent.In, pSocket, socket);
            return pool;
        }


        /// <summary>
        ///     配置校验
        /// </summary>
        protected override ZeroStationOption GetApiOption()
        {
            var option = ZeroApplication.GetApiOption(StationName);
            if (option.SpeedLimitModel == SpeedLimitType.None)
                option.SpeedLimitModel = SpeedLimitType.ThreadCount;
            return option;
        }

        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal override bool OnExecuestEnd(ZSocket socket, ApiCallItem item, ZeroOperatorStateType state)
        {
            int i = 0;
            var des = new byte[10 + item.Originals.Count];
            des[i++] = (byte)(6 + item.Originals.Count);
            des[i++] = (byte)state;
            des[i++] = ZeroFrameType.Requester;
            des[i++] = ZeroFrameType.RequestId;
            des[i++] = ZeroFrameType.CallId;
            des[i++] = ZeroFrameType.GlobalId;
            des[i++] = ZeroFrameType.ResultText;
            var msg = new List<byte[]>
            {
                item.Caller,
                des,
                item.Requester.ToZeroBytes(),
                item.RequestId.ToZeroBytes(),
                item.CallId.ToZeroBytes(),
                item.GlobalId.ToZeroBytes(),
                item.Result.ToZeroBytes()
            };
            foreach (var org in item.Originals)
            {
                des[i++] = org.Key;
                msg.Add((org.Value));
            }
            des[i++] = ZeroFrameType.SerivceKey;
            msg.Add((GlobalContext.ServiceKey.ToZeroBytes()));
            des[i] = ZeroFrameType.ResultEnd;
            return SendResult(socket, new ZMessage(msg));
        }

        private static readonly byte[] LayoutErrorFrame = new byte[]
        {
            1,
            (byte) ZeroOperatorStateType.FrameInvalid,
            ZeroFrameType.SerivceKey,
            ZeroFrameType.ResultEnd
        };
        /// <summary>
        /// 发送返回值 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        internal override void SendLayoutErrorResult(ZSocket socket, ApiCallItem item)
        {
            if (!CanLoop)
            {
                ZeroTrace.WriteError(StationName, "Can`t send result,station is closed");
                return;
            }
            if (item == null)
                return;
            try
            {
                SendResult(socket, new ZMessage
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
        bool SendResult(ZSocket socket, ZMessage message)
        {
            if (!CanLoop)
            {
                ZeroTrace.WriteError(StationName, "Can`t send result,station is closed");
                return false;
            }

            try
            {
                ZError error;
                using (message)
                {
                    if (socket.Send(message, out error))
                        return true;
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