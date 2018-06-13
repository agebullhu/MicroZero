using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;
using Newtonsoft.Json;
using ZeroMQ;

namespace Agebull.ZeroNet.Log
{
    /// <summary>
    ///   远程记录器
    /// </summary>
    internal sealed class RemoteLogRecorder : ILogRecorder, IZeroObject
    {
        #region Override

        /// <inheritdoc />
        /// <summary>
        ///     启动
        /// </summary>
        void ILogRecorder.Initialize()
        {
            LogRecorder.TraceToConsole = false;
            _state = StationState.Initialized;
        }
        /// <inheritdoc />
        /// <summary>
        ///   停止
        /// </summary>
        void ILogRecorder.Shutdown()
        {
            Close();
            _state = StationState.Destroy;
        }


        /// <summary>
        ///   记录日志
        /// </summary>
        /// <param name="infos"> 日志消息 </param>
        void ILogRecorder.RecordLog(List<RecordInfo> infos)
        {
            if (socket != null)
            {
                try
                {
                    using (var frames = new ZMessage
                    {
                        new ZFrame(ZeroHelper.PubDescription),
                        new ZFrame(("Logs").ToUtf8Bytes()),
                        new ZFrame(ApiContext.RequestContext.RequestId.ToUtf8Bytes()),
                        new ZFrame(ZeroApplication.Config.RealName.ToUtf8Bytes()),
                        new ZFrame((ZeroApplication.Config.StationName).ToUtf8Bytes()),
                        new ZFrame(JsonConvert.SerializeObject(infos).ToUtf8Bytes())
                    })
                    {
                        if (socket.SendTo(frames))
                        {
                            return;
                        }

                        //send.HaseFailed = true;
                    }
                }
                catch (Exception e)
                {
                    LogRecorder.BaseRecorder.RecordLog(new RecordInfo
                    {
                        Type = LogType.Error,
                        Name = "RemoteLogRecorder",
                        Message = $"日志发送失败，异常为：\r\n{e}"
                    });
                }
                //finally
                //{
                //    ZeroConnectionPool.Free(send);
                //}
            }
            LogRecorder.BaseRecorder.RecordLog(infos);
        }


        /// <summary>
        ///   记录日志
        /// </summary>
        /// <param name="info"> 日志消息 </param>
        void ILogRecorder.RecordLog(RecordInfo info)
        {
            info.User = $"{ApiContext.Customer.Account}({ApiContext.RequestContext.Ip}:{ApiContext.RequestContext.Port})";
            info.Machine = ZeroApplication.Config.RealName;
            using (LogRecordingScope.CreateScope())
            {
                //if (ZeroApplication.IsAlive)
                //    Items.Push(info);
                //else
                //    LogRecorder.BaseRecorder.RecordLog(info);
                //var send = ZeroConnectionPool.GetSocket("RemoteLog", RealName);
                //if (send != null && send.Socket != null)
                if (socket != null)
                {
                    try
                    {
                        using (var frames = new ZMessage
                        {
                            new ZFrame(ZeroHelper.PubDescription),
                            new ZFrame((info.TypeName ?? "").ToUtf8Bytes()),
                            new ZFrame(ApiContext.RequestContext.RequestId.ToUtf8Bytes()),
                            new ZFrame(ZeroApplication.Config.RealName.ToUtf8Bytes()),
                            new ZFrame((ZeroApplication.Config.StationName ?? "").ToUtf8Bytes()),
                            new ZFrame(JsonConvert.SerializeObject(info).ToUtf8Bytes())
                        })
                        {
                            if (socket.SendTo(frames))
                            {
                                return;
                            }

                            //send.HaseFailed = true;
                        }
                    }
                    catch (Exception e)
                    {
                        LogRecorder.BaseRecorder.RecordLog(new RecordInfo
                        {
                            Type = LogType.Error,
                            Name = "RemoteLogRecorder",
                            Message = $"日志发送失败，异常为：\r\n{e}"
                        });
                    }
                    //finally
                    //{
                    //    ZeroConnectionPool.Free(send);
                    //}
                }
                LogRecorder.BaseRecorder.RecordLog(info);
            }
        }

        #endregion

        #region Field

        /// <summary>
        /// 配置
        /// </summary>
        private StationConfig Config;

        /// <summary>
        /// 请求队列
        /// </summary>
        public static readonly LogQueue Items = new LogQueue();

        /// <summary>
        /// 节点名称
        /// </summary>
        string IZeroObject.Name => "RemoteLogRecorder";
        /// <summary>
        /// 实例名称
        /// </summary>
        internal string RealName { get; private set; }
        /// <summary>
        /// 实例名称
        /// </summary>
        internal byte[] Identity { get; private set; }

        /// <summary>
        ///     运行状态
        /// </summary>
        private int _state;

        /// <summary>
        ///     运行状态
        /// </summary>
        public int State
        {
            get => _state;
            set => Interlocked.Exchange(ref _state, value);
        }
        private CancellationTokenSource RunTaskCancel;

        bool Start()
        {
            using (OnceScope.CreateScope(this))
            {
                if (!ZeroApplication.Config.TryGetConfig("RemoteLog", out Config))
                {
                    ZeroTrace.WriteError("RemoteLogRecorder", "No config");
                    State = StationState.ConfigError;
                    return false;
                }
                RealName = ZeroIdentityHelper.CreateRealName(false, Config.ShortName ?? Config.StationName);
                Identity = RealName.ToAsciiBytes();
                RunTaskCancel = new CancellationTokenSource();
                //Task.Factory.StartNew(SendTask, RunTaskCancel.Token);
                Task.Factory.StartNew(RunWaite, RunTaskCancel.Token);
            }
            return true;
        }
        /// <summary>
        /// 应用程序等待结果的信号量对象
        /// </summary>
        private readonly SemaphoreSlim _waitToken = new SemaphoreSlim(0, 1);
        bool Close()
        {
            if (RunTaskCancel == null)
                return true;
            RunTaskCancel.Dispose();
            RunTaskCancel = null;
            _waitToken.Wait();
            return true;
        }
        #endregion

        #region Task

        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void OnRun()
        {
            if (!ZeroApplication.Config.TryGetConfig("RemoteLog", out Config))
            {
                ZeroTrace.WriteError("RemoteLogRecorder", "No config");
                State = StationState.ConfigError;
                ZeroApplication.OnObjectFailed(this);
                return ;
            }
            ZeroTrace.WriteInfo("RemoteLogRecorder", "Run", $"{RealName} : {Config.RequestAddress}");
            State = StationState.Run;
            ZeroApplication.OnObjectActive(this);
        }

        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void OnStop()
        {
            State = StationState.Closed;
            ZeroApplication.OnObjectClose(this);
            _waitToken.Release();
        }
        private bool CanRun => RunTaskCancel != null && !RunTaskCancel.Token.IsCancellationRequested &&
                               ZeroApplication.CanDo && State == StationState.Run;

        /// <summary>
        ///     发送广播的后台任务
        /// </summary>
        private void SendTask(object objToken)
        {
            using (OnceScope.CreateScope(this, OnRun, OnStop))
            {
                socket = ZSocket.CreateRequestSocket(Config.RequestAddress, Identity);
                while (CanRun)
                {
                    if (!Items.Wait(out var title, out var items, 100))
                    {
                        continue;
                    }
                    try
                    {
                        while (!socket.Publish(title.ToString(), ZeroApplication.Config.StationName, JsonConvert.SerializeObject(items)))
                        {
                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception e)
                    {
                        LogRecorder.BaseRecorder.RecordLog(new RecordInfo
                        {
                            Type = LogType.Error,
                            Name = "RemoteLogRecorder",
                            Message = $"日志发送失败，异常为：\r\n{e}"
                        });
                        socket.TryClose();
                        socket = ZSocket.CreateRequestSocket(Config.RequestAddress, Identity);
                        foreach (var info in items)
                        {
                            TxtRecorder.Recorder.RecordLog(info);
                        }
                    }
                }
                socket.TryClose();
            }
        }

        private ZSocket socket;
        /// <summary>
        /// 具体执行
        /// </summary>
        private void RunWaite()
        {
            using (OnceScope.CreateScope(this, OnRun, OnStop))
            {
                var pool = ZmqPool.CreateZmqPool();
                pool.Prepare(new[] {ZSocket.CreateServiceSocket("inproc://RemoteLog.req", ZSocketType.PAIR)},
                    ZPollEvent.In);
                using (pool)
                {
                    socket  = ZSocket.CreateClientSocket("inproc://RemoteLog.req", ZSocketType.PAIR);
                    var send = ZSocket.CreateClientSocket(Config.RequestAddress, ZSocketType.DEALER);
                    while (CanRun)
                    {
                        if (!pool.Poll() || !pool.CheckIn(0, out var message))
                        {
                            continue;
                        }

                        using (message)
                        {
                            using (var copy = message.Duplicate())
                            {
                                send.Send(copy);
                            }
                        }
                    }

                    send.TryClose();
                    socket.TryClose();
                    socket = null;
                }
            }
        }
        #endregion

        #region IZeroObject
        /// <summary>
        /// 实例
        /// </summary>
        public static readonly RemoteLogRecorder Instance = new RemoteLogRecorder();


        /// <summary>
        ///     要求心跳
        /// </summary>
        void IZeroObject.OnHeartbeat()
        {
            if (State == StationState.Run)
                SystemManager.Heartbeat("RemoteLogRecorder", RealName);
        }

        void IZeroObject.OnZeroInitialize()
        {
            State = StationState.Initialized;
        }

        bool IZeroObject.OnZeroStart()
        {
            return  Start();
        }

        void IZeroObject.OnStationStateChanged(StationConfig config)
        {
            //if (config != Config)
            //    return;
            //if (State == StationState.Run)
            //    Close();
            //if (config.State == ZeroCenterState.Run && CanRun)
            //    Start();
        }

        bool IZeroObject.OnZeroEnd()
        {
            return Close();
        }

        void IZeroObject.OnZeroDestory()
        {
            foreach (var items in Items.Items.Values)
            {
                foreach (var item in items)
                    LogRecorder.BaseRecorder.RecordLog(item);
            }
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
        }
        #endregion

    }
}
