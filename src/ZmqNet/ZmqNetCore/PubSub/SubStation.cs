using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common;
using Agebull.Common.Logging;
using Agebull.ZeroNet.Core;
using ZeroMQ;

namespace Agebull.ZeroNet.PubSub
{
    /// <summary>
    /// 消息订阅站点
    /// </summary>
    public abstract class SubStation : ZeroStation
    {
        /// <summary>
        /// 构造
        /// </summary>
        protected SubStation() : base(StationTypePublish)
        {

        }
        /// <summary>
        /// 订阅主题
        /// </summary>
        public string Subscribe { get; set; } = "";

        /// <summary>
        /// 是否实时数据(如为真,则不保存未处理数据)
        /// </summary>
        public bool IsRealModel { get; set; }


        private ZSocket _socket;

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
            StationConsole.WriteInfo("Pub",$"{ StationName}:{Name}", Config.WorkerAddress);
            StationConsole.WriteInfo("Pub",$"{ StationName}:{Name}", RealName);
            StationConsole.WriteInfo("Pub",$"{ StationName}:{Name}", "start...");
            State = StationState.Start;
            InPoll = false;

            State = StationState.Run;
            if (!IsRealModel)
                Items = SyncQueue<PublishItem>.Load(CacheFileName);

            Task.Factory.StartNew(HandleTask);
            Task.Factory.StartNew(PollTask).ContinueWith(task => OnTaskStop());
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
                State = StationState.Failed;
                return;
            }
            StationConsole.WriteInfo(StationName, "run...");
            InPoll = true;
            while (State == StationState.Run)
            {
                if (_socket.Subscribe(out var item))
                    Items.Push(item);
            }
            InPoll = false;
            StationConsole.WriteInfo("Pub",$"{ StationName}:{Name}", "poll stop");
        }

        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        protected override void OnTaskStop()
        {
            CloseSocket();
            if (!ZeroApplication.IsAlive)
            {
                if (!IsRealModel)
                    Items.Save(CacheFileName);
            }
            base.OnTaskStop();
        }

        private bool CreateSocket()
        {
            try
            {
                _socket = ZeroHelper.CreateSubscriberSocket(Config.WorkerAddress, Identity, Subscribe);
                return true;
            }
            catch (Exception e)
            {
                LogRecorder.Exception(e);
                State = StationState.Failed;
                CloseSocket();
                StationConsole.WriteException("Pub",$"{ StationName}:{Name}", e);
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
        protected virtual void HandleTask()
        {
            while (State == StationState.Run)
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
                    StationConsole.WriteException("Pub",$"{ StationName}:{Name}", e);
                    Thread.Sleep(5);
                }
                finally
                {
                    if (IsRealModel)
                        Items.EndProcess();
                }
            }
        }
    }
}