using System;
using System.Threading;
using System.Threading.Tasks;
using ZeroMQ;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    ///     Api站点
    /// </summary>
    public class PollProxy
    {
        /// <summary>
        /// 取消标记
        /// </summary>
        public CancellationTokenSource RunTaskCancel { get; set; }

        /// <summary>
        /// 开始
        /// </summary>
        public void Start()
        {
            ZeroTrace.WriteInfo("ConnectionProxy", "Start");
            RunTaskCancel = new CancellationTokenSource();
            Task.Factory.StartNew(Run);
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
            ZeroTrace.WriteInfo("ConnectionProxy", "End");
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
            var configs = ZeroApplication.Config.GetConfigs(p => p.StationType != ZeroStation.StationTypeDispatcher);
            ZSocket[] sockets = new ZSocket[configs.Length * 2];
            int idx = 0;

            foreach (var config in configs)
            {
                sockets[idx++] = ZSocket.CreateServiceSocket($"inproc://{config.StationName}_Proxy", ZSocketType.ROUTER);
                sockets[idx++] = ZSocket.CreateClientSocket(config.RequestAddress, ZSocketType.DEALER);
            }

            WaitCount = 0;
            ZeroTrace.WriteInfo("ConnectionProxy", "Listen");
            
            using (var pool = ZmqPool.CreateZmqPool())
            {
                pool.Prepare(sockets, ZPollEvent.In);
                _waitToken.Release();
                //SystemManager.HeartReady(StationName, RealName);
                while (!RunTaskCancel.Token.IsCancellationRequested)
                {
                    if (!pool.Poll())
                        continue;
                    Parallel.For(0, configs.Length, index =>
                    {
                        CheckCall(pool, index * 2);
                        CheckResult(pool, index * 2 + 1);
                    });

                }

            }
            _waitToken.Release();
        }

        private void CheckResult(IZmqPool pool, int idx)
        {
            if (!pool.CheckIn(idx, out var message))
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

                        pool.Sockets[idx - 1].SendMessage(clone, out _);
                        --WaitCount;
                    }
                }
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException("ConnectionProxy", e, "CheckResult");
            }
        }

        private void CheckCall(IZmqPool pool, int idx)
        {
            if (!pool.CheckIn(idx, out var message)) return;
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
                            pool.Sockets[idx].SendMessage(res, out var error);
                        }
                    }
                    bool success;
                    using (ZMessage clone = new ZMessage())
                    {
                        for (var index = 1; index < message.Count; index++)
                        {
                            clone.Add(message[index].Duplicate());
                        }

                        success = pool.Sockets[idx + 1].SendMessage(clone, out var error2);
                    }

                    if (!success)
                    {
                        using (var res = new ZMessage
                        {
                            message[1].Duplicate(),
                            new ZFrame(NetErrorDescription)
                        })
                        {
                            pool.Sockets[idx].SendMessage(res, out var error);
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
                ZeroTrace.WriteException("ConnectionProxy", e, "CheckCall");
            }
        }
    }
}