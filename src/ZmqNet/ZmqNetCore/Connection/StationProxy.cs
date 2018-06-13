using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     Api调用代理
    /// </summary>
    public class StationProxy
    {
        /// <summary>
        /// 站点配置
        /// </summary>
        public StationConfig Config { get; set; }

        /// <summary>
        /// 取消标记
        /// </summary>
        public CancellationTokenSource RunTaskCancel { get; set; }

        /// <summary>
        /// 开始
        /// </summary>
        public void Start()
        {
            ZeroTrace.WriteInfo($"{Config.StationName}(proxy)", "Start");
            RunTaskCancel = new CancellationTokenSource();
            Task.Factory.StartNew(Run, RunTaskCancel.Token);
            _waitToken.Wait();
        }
        /// <summary>
        /// 结束
        /// </summary>
        /// <returns></returns>
        public bool End()
        {
            if (RunTaskCancel == null)
                return false;
            RunTaskCancel.Cancel();
            _waitToken.Wait();
            RunTaskCancel.Dispose();
            RunTaskCancel = null;
            ZeroTrace.WriteInfo($"{Config.StationName}(proxy)", "End");
            return true;
        }
        /// <summary>
        /// 应用程序等待结果的信号量对象
        /// </summary>
        private readonly SemaphoreSlim _waitToken = new SemaphoreSlim(0, 1);

        /// <summary>
        /// 未完成的请求数量
        /// </summary>
        public int WaitCount { get; set; }

        private ZSocket _inprocPollSocket;

        private ZSocket _callPollSocket;

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
        private void Run()
        {
            _inprocPollSocket =
                ZSocket.CreateServiceSocket($"inproc://{Config.StationName}_Proxy", ZSocketType.ROUTER);
            _inprocPollSocket.Backlog = 4096;
            _callPollSocket = ZSocket.CreateClientSocket(Config.RequestAddress, ZSocketType.DEALER);

            WaitCount = 0;
            ZeroTrace.WriteInfo($"{Config.StationName}(proxy)", "Listen");
            _waitToken.Release();
            using (var pool = ZmqPool.CreateZmqPool())
            {
                pool.Prepare(new[] {_inprocPollSocket, _callPollSocket}, ZPollEvent.In);
                //SystemManager.HeartReady(StationName, RealName);
                while (!RunTaskCancel.Token.IsCancellationRequested)
                {
                    if (!pool.Poll())
                        continue;
                    CheckCall(pool);

                    CheckResult(pool);
                }

            }
            _waitToken.Release();
        }

        private void CheckResult(IZmqPool pool)
        {
            if (!pool.CheckIn(1, out var message))
                return;
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

                        _inprocPollSocket.SendMessage(clone, out _);
                        --WaitCount;
                    }
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(Config.Name, e, "CheckResult");
            }
        }

        private void CheckCall(IZmqPool pool)
        {
            if (!pool.CheckIn(0, out var message)) return;
            try
            {
                using (message)
                {
                    if (!ZeroApplication.ZerCenterIsRun)
                    {
                        using (var res = new ZMessage
                        {
                            message[1].Duplicate(),
                            new ZFrame(NetErrorDescription)
                        })
                        {
                            _inprocPollSocket.SendMessage(res, out _);
                        }
                    }
                    bool success;
                    using (ZMessage clone = new ZMessage())
                    {
                        for (var index = 1; index < message.Count; index++)
                        {
                            clone.Add(message[index].Duplicate());
                        }

                        success = _callPollSocket.SendMessage(clone, out _);
                    }

                    if (!success)
                    {
                        using (var res = new ZMessage
                        {
                            message[1].Duplicate(),
                            new ZFrame(NetErrorDescription)
                        })
                        {
                            _inprocPollSocket.SendMessage(res, out _);
                        }
                    }
                    else
                    {
                        ++WaitCount;
                    }
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(Config.Name, e, "CheckCall");
            }
        }
    }
}