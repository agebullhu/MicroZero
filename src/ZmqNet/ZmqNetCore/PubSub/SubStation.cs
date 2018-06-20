using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Agebull.Common;
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
        protected SubStation() : base(StationTypePublish, true)
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
        /// 缓存文件名称
        /// </summary>
        private string CacheFileName => Path.Combine(ZeroApplication.Config.DataFolder,
            $"zero_sub_queue_{Name}.json");

        /// <summary>
        /// 请求队列
        /// </summary>
        public SyncQueue<PublishItem> Items = new SyncQueue<PublishItem>();



        /// <summary>
        /// 具体执行
        /// </summary>
        /// <returns>返回False表明需要重启</returns>
        protected  override bool RunInner(CancellationToken token)
        {
            SystemManager.HeartReady(StationName, RealName);
            using (var pool = ZmqPool.CreateZmqPool())
            {
                pool.Prepare(new[] { ZSocket.CreateClientSocket(Config.WorkerCallAddress, ZSocketType.SUB, Identity, Subscribe) }, ZPollEvent.In);
                while (CanRun)
                {
                    if (!pool.Poll() )
                    {
                        continue;
                    }
                    if (!pool.CheckIn(0, out var message))
                    {
                        continue;
                    }
                    if (message.Unpack(out var item))
                    {
                        Items.Push(item);
                    }
                }
            }
            SystemManager.HeartLeft(StationName, RealName);
            return true;
        }

        /// <summary>
        /// 命令处理任务
        /// </summary>
        protected virtual void HandleTask()
        {
            ZeroApplication.OnGlobalStart(this);
            while (ZeroApplication.IsAlive)
            {
                if (!Items.StartProcess(out var item))
                    continue;
                try
                {
                    Handle(item);
                    if (!IsRealModel)
                        Items.EndProcess();
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(StationName, e, Name);
                    Thread.Sleep(5);
                }
                finally
                {
                    if (IsRealModel)
                        Items.EndProcess();
                }
            }
            ZeroApplication.OnGlobalEnd(this);
        }


        /// <summary>
        /// 初始化
        /// </summary>
        protected sealed override void Initialize()
        {
            if (!IsRealModel)
                Items.Load(CacheFileName);
            Task.Factory.StartNew(HandleTask);
        }

        /// <summary>
        /// 析构
        /// </summary>
        protected override void DoDispose()
        {
            if (!IsRealModel)
                Items.Save(CacheFileName);
        }
    }
}