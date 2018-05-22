using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using NetMQ;
using NetMQ.Sockets;

namespace Agebull.ZeroNet.PubSub
{
    /// <summary>
    /// 消息订阅站点
    /// </summary>
    public abstract class SubStation : ZeroStation
    {
        /// <summary>
        /// 类型
        /// </summary>
        public override int StationType => StationTypePublish;
        /// <summary>
        /// 订阅主题
        /// </summary>
        public string Subscribe { get; set; } = "";

        /// <summary>
        /// 是否实时数据(如为真,则不保存未处理数据)
        /// </summary>
        public bool IsRealModel { get; set; }


        private SubscriberSocket _socket;

        /*// <summary>
        /// 命令处理方法 
        /// </summary>
        public Action<string> ExecFunc { get; set; }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual void ExecCommand(string args)
        {
            ExecFunc?.Invoke(args);
        }*/

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public abstract void Handle(PublishItem args);

        /// <inheritdoc />
        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        protected sealed override bool Run()
        {
            StationConsole.WriteInfo($"[{StationName}:{RealName}]{Config.WorkerAddress}start...");
            RunState = StationState.Start;
            InPoll = false;

            RunState = StationState.Run;
            if (!IsRealModel)
                Items = SyncQueue<PublishItem>.Load(CacheFileName);

            Task.Factory.StartNew(HandleTask);
            Task.Factory.StartNew(PollTask).ContinueWith(task => OnTaskStop());
            while (!InPoll)
                Thread.Sleep(50);
            StationConsole.WriteInfo($"[{StationName}:{RealName}]runing...");
            return true;
        }

        /// <summary>
        /// 缓存文件名称
        /// </summary>
        private string CacheFileName => Path.Combine(ZeroApplication.Config.DataFolder,
            $"zero_sub_queue_{StationName}.json");

        /// <summary>
        /// 请求队列
        /// </summary>
        public SyncQueue<PublishItem> Items = new SyncQueue<PublishItem>();

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        private void PollTask()
        {
            Heartbeat(false);
            if (!CreateSocket())
            {
                RunState = StationState.Failed;
                return;
            }
            StationConsole.WriteLine($"[{StationName}:{RealName}]poll start...");
            var timeout = new TimeSpan(0, 0, 5);
            InPoll = true;
            while (RunState == StationState.Run)
            {
                try
                {
                    if (!_socket.TryReceiveFrameString(timeout, out var title, out var more) || !more)
                    {
                        continue;
                    }
                    if (!_socket.TryReceiveFrameBytes(out var description, out more) || !more)
                    {
                        continue;
                    }
                    PublishItem item = new PublishItem
                    {
                        Title = title
                    };
                    int idx = 1;
                    while (more)
                    {
                        if (!_socket.TryReceiveFrameString(out var val, out more))
                        {
                            continue;
                        }
                        switch (description[idx++])
                        {
                            case ZeroFrameType.SubTitle:
                                item.SubTitle = val;
                                break;
                            case ZeroFrameType.Publisher:
                                item.Station = val;
                                break;
                            case ZeroFrameType.Argument:
                                item.Content = val;
                                break;
                            case ZeroFrameType.RequestId:
                                item.RequestId = val;
                                break;
                        }
                    }
                    Items.Push(item);
                }
                catch (Exception e)
                {
                    StationConsole.WriteError($"[{StationName}:{RealName}]poll error.{e.Message}...");
                    LogRecorder.Exception(e);
                    RunState = StationState.Failed;
                }
            }
            InPoll = false;
            StationConsole.WriteLine($"[{StationName}:{RealName}]poll stop");
            if (!IsRealModel)
                Items.Save(CacheFileName);
            CloseSocket();

        }

        private bool CreateSocket()
        {
            _socket = new SubscriberSocket();
            _socket.Options.Identity = Identity;
            _socket.Options.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 10);
            _socket.Options.ReconnectIntervalMax = new TimeSpan(0, 0, 0, 0, 500);
            _socket.Options.TcpKeepalive = true;
            _socket.Options.TcpKeepaliveIdle = new TimeSpan(0, 1, 0);
            try
            {
                _socket.Subscribe(Subscribe);
                _socket.Connect(Config.WorkerAddress);
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                RunState = StationState.Failed;
                CloseSocket();
                StationConsole.WriteError($"[{StationName}:{RealName}]connect error =>{e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 关闭套接字
        /// </summary>
        private void CloseSocket()
        {
            lock (this)
            {
                if (_socket == null)
                    return;
                try
                {
                    _socket.Disconnect(Config.WorkerAddress);
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);//一般是无法连接服务器而出错
                }
                _socket.Close();
                _socket = null;
            }
        }

        /// <summary>
        /// 命令处理任务
        /// </summary>
        private void HandleTask()
        {
            while (RunState == StationState.Run)
            {
                if (!Items.StartProcess(out var item, 1000))
                    continue;
                try
                {
                    Handle(item);
                    if (!IsRealModel)
                        Items.EndProcess();
                }
                catch (Exception e)
                {
                    LogRecorder.Exception(e);
                    Thread.Sleep(5);
                }
                finally
                {
                    if(IsRealModel )
                        Items.EndProcess();
                }
            }
        }
    }
}