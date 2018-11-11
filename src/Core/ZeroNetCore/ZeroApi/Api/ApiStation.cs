using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using ZeroMQ;
using Gboxt.Common.DataModel;
using Agebull.Common.Rpc;
using Agebull.Common.ApiDocuments;

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
            socket = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER);
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
            if (item.Result == null)
                return SendResult(ref socket, new ZMessage
                {
                    new ZFrame(item.Caller),
                    new ZFrame(new byte[]
                    {
                        4,
                        (byte) state,
                        ZeroFrameType.Requester,
                        ZeroFrameType.RequestId,
                        ZeroFrameType.GlobalId,
                        ZeroFrameType.SerivceKey,
                        ZeroFrameType.End
                    }),
                    new ZFrame(item.Requester.ToZeroBytes()),
                    new ZFrame(item.RequestId.ToZeroBytes()),
                    new ZFrame(item.GlobalId.ToZeroBytes()),
                    new ZFrame(GlobalContext.ServiceKey.ToZeroBytes())
                });

            return SendResult(ref socket, new ZMessage
            {
                new ZFrame(item.Caller),
                new ZFrame(new byte[]
                {
                    5,
                    (byte) state,
                    ZeroFrameType.JsonValue,
                    ZeroFrameType.Requester,
                    ZeroFrameType.RequestId,
                    ZeroFrameType.GlobalId,
                    ZeroFrameType.SerivceKey,
                    ZeroFrameType.End
                }),
                new ZFrame(item.Result.ToZeroBytes()),
                new ZFrame(item.Requester.ToZeroBytes()),
                new ZFrame(item.RequestId.ToZeroBytes()),
                new ZFrame(item.GlobalId.ToZeroBytes()),
                new ZFrame(GlobalContext.ServiceKey.ToZeroBytes())
            });
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
            SendResult(ref socket, new ZMessage
            {
                new ZFrame(item.Caller),
                new ZFrame(LayoutErrorFrame),
                new ZFrame(GlobalContext.ServiceKey.ToZeroBytes())
            });
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
                    socket.TryClose();
                    socket = ZSocket.CreateClientSocket(Config.WorkerResultAddress, ZSocketType.DEALER, Identity);
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
        }

    }
}