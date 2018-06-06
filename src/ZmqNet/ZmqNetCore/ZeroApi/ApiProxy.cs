using System;
using System.Globalization;
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
    public class ApiProxy : ZeroStation
    {
        public ApiProxy() : base(StationTypeApi, false)
        {
        }
        private int waitCount;

        private ZSocket _inprocPollSocket;

        private ZSocket _callPollSocket;
        protected override void OnStart()
        {
            //Identity = RealName.ToAsciiBytes();
            _inprocPollSocket = ZeroHelper.CreateServiceSocket($"inproc://{StationName}_Proxy", ZSocketType.ROUTER, Identity);
            _inprocPollSocket.Backlog = 4096;
            _callPollSocket = ZeroHelper.CreateClientSocket(Config.RequestAddress, ZSocketType.DEALER, Identity);
            //_callPollSocket.SetOption(ZSocketOption.CONNECT_RID, Identity);
            base.OnStart();
        }

        /// <summary>
        /// 请求格式说明
        /// </summary>
        private static readonly byte[] NetErrorDescription = {
            0,
            (byte)ZeroOperatorStateType.NetError,
            ZeroFrameType.End
        };

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        protected override bool RunInner(CancellationToken token)
        {
            waitCount = 0;
            using (var pool = ZmqPool.CreateZmqPool())
            {
                pool.Prepare(new[] { _inprocPollSocket, _callPollSocket }, ZPollEvent.In);
                //SystemManager.HeartReady(StationName, RealName);
                while (!token.IsCancellationRequested && CanRun)
                {
                    if (!pool.Poll())
                        continue;
                    if (pool.CheckIn(0, out var message))
                    {
                        try
                        {
                            using (message)
                            {
                                bool success;
                                using (ZMessage clone = new ZMessage())
                                {
                                    for (var index = 1; index < message.Count; index++)
                                    {
                                        clone.Add(message[index].Duplicate());
                                    }

                                    success = _callPollSocket.SendMessage(clone, out var error2);
                                }
                                if (!success)
                                {
                                    using (var res = new ZMessage
                                    {
                                        message[1].Duplicate(),
                                        new ZFrame(NetErrorDescription)
                                    })
                                    {
                                        _inprocPollSocket.SendMessage(res, out var error);
                                    }
                                }
                                else
                                {
                                    ++waitCount;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }

                    if (!pool.CheckIn(1, out message))
                        continue;
                    try
                    {
                        using (message)
                        {
                            using (ZMessage clone = new ZMessage())
                            {
                                byte[] des = message[0].Read();
                                int size = des[0] + 2;
                                for (var index = 2; index < size && index < des.Length; index++)
                                {
                                    if (des[index] != ZeroFrameType.Requester)
                                        continue;
                                    clone.Add(message[index - 1].Duplicate());
                                    break;
                                }
                                //clone.Add(new ZFrame("".ToAsciiBytes()));
                                clone.Add(new ZFrame(des));
                                for (var index = 1; index < message.Count; index++)
                                {
                                    clone.Add(message[index].Duplicate());
                                }
                                _inprocPollSocket.SendMessage(clone, out var error);
                                --waitCount;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            //SystemManager.HeartLeft(StationName, RealName);
            return true;
        }

        //protected override void OnRunStop()
        //{
        //    _inprocPollSocket.CloseSocket();
        //    _callPollSocket.CloseSocket();
        //    base.OnRunStop();
        //}

    }
}