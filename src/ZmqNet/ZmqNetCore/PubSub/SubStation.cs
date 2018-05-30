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

        /// <summary>
        /// 初始化
        /// </summary>
        protected sealed override void Initialize()
        {
            try
            {
                if (!IsRealModel)
                    Items = SyncQueue<PublishItem>.Load(CacheFileName);
            }
            catch
            {
            }
            Task.Factory.StartNew(HandleTask);
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

        /// <inheritdoc />
        /// <summary>
        /// 命令轮询
        /// </summary>
        /// <returns></returns>
        protected sealed override bool Run()
        {
            Heartbeat(false);
            _socket = ZeroHelper.CreateSubscriberSocket(Config.WorkerAddress, Identity, Subscribe);
            ZeroTrace.WriteInfo(StationName, "run...");
            InPoll = true;
            State = StationState.Run;
            while (CanRun)
            {
                if (_socket.Subscribe(out var item))
                {
                    Items.Push(item);
                }
            }
            _socket.CloseSocket();
            InPoll = false;
            ZeroTrace.WriteInfo("Pub",$"{ StationName}:{Name}", "poll stop");
            return true;
        }

        /// <summary>
        /// 析构
        /// </summary>
        protected override void DoDispose()
        {
            if (!IsRealModel)
                Items.Save(CacheFileName);
        }
        /// <summary>
        /// 命令处理任务
        /// </summary>
        protected virtual void HandleTask()
        {
            while (ZeroApplication.IsAlive)
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
                    ZeroTrace.WriteException("Pub",$"{ StationName}:{Name}", e);
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